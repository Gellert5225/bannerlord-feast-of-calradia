using System.Collections.Generic;
using FeastsOfCalradia.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace FeastsOfCalradia.Behaviors
{
    // Step 1 of the implementation roadmap: the trigger system. Hooks the campaign events that may
    // schedule a feast, applies a per-kingdom cooldown, and picks a tentative host. Currently announces
    // the schedule via in-game notification — the actual feast scene/flow lands in later steps.
    public class FeastSystemBehavior : CampaignBehaviorBase
    {
        // Kingdom StringId → time of the most recent feast scheduled for that kingdom. Persisted via
        // SyncData so cooldowns survive save/load. VERIFY: SyncData supports Dictionary<string,
        // CampaignTime>; if the load fails, fall back to Dictionary<string, long> with raw ticks.
        private Dictionary<string, CampaignTime> _lastFeastByKingdom = new Dictionary<string, CampaignTime>();

        // Per-kingdom minimum gap between scheduled feasts. Design doc targets ~6 months between feasts
        // but allows "two within a season" for eventful kingdoms; using 60 days as the floor and trusting
        // event sparsity to keep the actual rate near 1–2 per year.
        private const double CooldownDays = 60.0;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.RulingClanChanged.AddNonSerializedListener(this, OnRulingClanChanged);
            CampaignEvents.BeforeHeroesMarried.AddNonSerializedListener(this, OnHeroesMarried);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, OnMakePeace);
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);

            // Game menus must be registered on OnSessionLaunched, NOT in the SubModule's
            // InitializeGameStarter — vanilla creates the "town"/"castle" menus on session launch, so
            // adding options before that point silently no-ops.
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("FeastsOfCalradia_LastFeastByKingdom", ref _lastFeastByKingdom);
        }

        // --- event handlers ---

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (!IsEnabled())
            {
                return;
            }
            Diagnostic("trigger:settlement_owner_changed — '" + settlement.Name + "' to '" + (newOwner?.Name?.ToString() ?? "?") + "' (detail: " + detail + ")");
            TryScheduleFeast(newOwner?.MapFaction as Kingdom, "victory", capturerHero ?? newOwner);
        }

        private void OnRulingClanChanged(Kingdom kingdom, Clan newRulingClan)
        {
            if (!IsEnabled())
            {
                return;
            }
            Diagnostic("trigger:ruling_clan_changed — kingdom '" + kingdom.Name + "', new ruling clan '" + newRulingClan.Name + "'");
            TryScheduleFeast(kingdom, "coronation", newRulingClan.Leader);
        }

        private void OnHeroesMarried(Hero hero1, Hero hero2, bool showNotification)
        {
            if (!IsEnabled())
            {
                return;
            }
            Diagnostic("trigger:heroes_married — '" + hero1.Name + "' and '" + hero2.Name + "'");
            TryScheduleFeast(hero1.MapFaction as Kingdom, "wedding", hero1);
        }

        private void OnWarDeclared(IFaction side1, IFaction side2, DeclareWarAction.DeclareWarDetail detail)
        {
            if (!IsEnabled())
            {
                return;
            }
            // Wars are not feast triggers (anti-celebration); kept as a listener for future state-tracking.
            Diagnostic("trigger:war_declared — '" + side1.Name + "' and '" + side2.Name + "' (detail: " + detail + ")");
        }

        private void OnMakePeace(IFaction side1, IFaction side2, MakePeaceAction.MakePeaceDetail detail)
        {
            if (!IsEnabled())
            {
                return;
            }
            Diagnostic("trigger:make_peace — '" + side1.Name + "' and '" + side2.Name + "' (detail: " + detail + ")");
            TryScheduleFeast(side1 as Kingdom, "peace", null);
            TryScheduleFeast(side2 as Kingdom, "peace", null);
        }

        private void OnHeroComesOfAge(Hero hero)
        {
            if (!IsEnabled())
            {
                return;
            }
            Diagnostic("trigger:hero_comes_of_age — '" + hero.Name + "' (clan: " + (hero.Clan?.Name?.ToString() ?? "?") + ")");
            TryScheduleFeast(hero.MapFaction as Kingdom, "heir", hero.Clan?.Leader);
        }

        // --- view / status ---

        // Returns a human-readable status for the player's kingdom — used by the Clan-screen Feast tab
        // for "what's going on" display. Doesn't require being at a fief; pure data lookup.
        public string GetFeastSummary()
        {
            Hero player = Hero.MainHero;
            if (player?.Clan == null)
            {
                return "[Feasts] No clan.";
            }
            Kingdom kingdom = player.MapFaction as Kingdom;
            if (kingdom == null)
            {
                return "[Feasts] " + player.Clan.Name + " is not in a kingdom.";
            }

            if (_lastFeastByKingdom.TryGetValue(kingdom.StringId, out CampaignTime last))
            {
                int daysSince = (int)(CampaignTime.Now.ToDays - last.ToDays);
                int daysRemaining = (int)CooldownDays - daysSince;
                if (daysRemaining > 0)
                {
                    return "[Feasts] " + kingdom.Name + ": last feast " + daysSince + " days ago. Next eligible in " + daysRemaining + " days.";
                }
                return "[Feasts] " + kingdom.Name + ": last feast " + daysSince + " days ago. Eligible to host now.";
            }
            return "[Feasts] " + kingdom.Name + ": no feasts scheduled yet. Eligible to host now.";
        }

        // --- player-triggered ---

        // Called when the player chooses to host a feast (via console command or town/castle menu option).
        // Schedules a feast for the player's kingdom with the player as host, gated by the same cooldown
        // that AI-triggered feasts use.
        public string RequestPlayerFeast()
        {
            if (!IsEnabled())
            {
                return "Feast system is disabled in mod settings.";
            }
            Hero player = Hero.MainHero;
            if (player?.Clan == null)
            {
                return "Player has no clan.";
            }
            Kingdom kingdom = player.MapFaction as Kingdom;
            if (kingdom == null)
            {
                return "Player isn't part of a kingdom (mercenaries / minor clans can't host).";
            }
            Settlement here = Settlement.CurrentSettlement;
            if (here == null || here.OwnerClan != player.Clan)
            {
                return "Must be inside a fief you own to host a feast.";
            }
            bool scheduled = TryScheduleFeast(kingdom, "player_hosted", player);
            return scheduled
                ? "Feast scheduled at " + here.Name + "."
                : "Could not schedule a feast right now (cooldown or no eligible host fief).";
        }

        // --- game menu integration ---

        // Adds a "Host a feast" option to the town and castle main menus. Only visible when the player
        // is currently inside a fief their clan owns. Registered on OnSessionLaunched (after vanilla
        // creates the menus), not in InitializeGameStarter.
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption(
                "town",
                "feastsofcalradia_host_feast",
                "{=feastsofcalradia_host}Host a feast here",
                IsHostFeastOptionAvailable,
                OnHostFeastOptionSelected,
                false,
                4,
                false);
            starter.AddGameMenuOption(
                "castle",
                "feastsofcalradia_host_feast_castle",
                "{=feastsofcalradia_host}Host a feast here",
                IsHostFeastOptionAvailable,
                OnHostFeastOptionSelected,
                false,
                4,
                false);
        }

        private static bool IsHostFeastOptionAvailable(MenuCallbackArgs args)
        {
            Settlement here = Settlement.CurrentSettlement;
            if (here?.OwnerClan != Clan.PlayerClan)
            {
                return false;
            }
            // Could also gate on cooldown here, but leaving the option visible-but-fails is more
            // discoverable for the player than hiding it.
            return true;
        }

        private static void OnHostFeastOptionSelected(MenuCallbackArgs args)
        {
            FeastSystemBehavior behavior = Campaign.Current?.GetCampaignBehavior<FeastSystemBehavior>();
            string result = behavior != null
                ? behavior.RequestPlayerFeast()
                : "FeastSystemBehavior not registered.";
            InformationManager.DisplayMessage(new InformationMessage(result));
        }

        // --- scheduler ---

        private bool TryScheduleFeast(Kingdom kingdom, string trigger, Hero suggestedHost)
        {
            if (kingdom == null)
            {
                return false;
            }

            // Cooldown check.
            if (_lastFeastByKingdom.TryGetValue(kingdom.StringId, out CampaignTime last))
            {
                double daysSince = CampaignTime.Now.ToDays - last.ToDays;
                if (daysSince < CooldownDays)
                {
                    Diagnostic("scheduler: " + trigger + " for '" + kingdom.Name + "' — on cooldown (" + (int)daysSince + "/" + (int)CooldownDays + " days)");
                    return false;
                }
            }

            Hero host = SelectHost(kingdom, suggestedHost);
            if (host?.HomeSettlement == null)
            {
                Diagnostic("scheduler: " + trigger + " for '" + kingdom.Name + "' — no eligible host (need a fief-holder)");
                return false;
            }

            _lastFeastByKingdom[kingdom.StringId] = CampaignTime.Now;
            Announce("Lord " + host.Name + " of " + kingdom.Name + " plans a feast at " + host.HomeSettlement.Name + " (" + trigger + ")");
            return true;
        }

        private static Hero SelectHost(Kingdom kingdom, Hero suggested)
        {
            // Honour the suggestion if they're a fief-holder.
            if (suggested?.HomeSettlement != null && suggested.MapFaction == kingdom)
            {
                return suggested;
            }
            // Else fall back to ruling clan leader if they hold a fief.
            Hero ruler = kingdom.RulingClan?.Leader;
            if (ruler?.HomeSettlement != null)
            {
                return ruler;
            }
            // Else any fief-holding clan leader in the kingdom.
            foreach (Clan clan in kingdom.Clans)
            {
                if (clan.Leader?.HomeSettlement != null)
                {
                    return clan.Leader;
                }
            }
            return null;
        }

        // --- output helpers ---

        // Diagnostic logging (gated by MCM Verbose Logging).
        private static void Diagnostic(string message)
        {
            if (FeastSystemSettings.Instance?.VerboseLogging != true)
            {
                return;
            }
            InformationManager.DisplayMessage(new InformationMessage("[FeastSystem] " + message));
        }

        // Announce a scheduled feast (always shown — this is the user-facing output).
        private static void Announce(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage("[Feast] " + message));
        }

        private static bool IsEnabled()
        {
            return FeastSystemSettings.Instance?.Enabled ?? true;
        }
    }
}
