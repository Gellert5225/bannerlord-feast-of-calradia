using System.Text;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace FeastsOfCalradia.Behaviors
{
    public class LordHallBotProbeBehavior : CampaignBehaviorBase
    {
        private bool _enabled = true;
        private bool _verboseDiagnostics = true;
        private string _targetHeroStringId;

        // Tracks the LocationCharacter we last added so we can detect whether the engine cleared it
        // between missions (the suspected cause of the bot not appearing in the rendered scene).
        private LocationCharacter _currentBot;

        // The hero we last used as the bot template AND moved into the settlement. Tracked so we can
        // (a) avoid redundant re-applies within the same visit, (b) cleanly release them on player exit.
        // Not synced — see VERIFY note in OnSettlementLeft for save/load edge case.
        private Hero _currentGuestHero;
        // Set if the guest was brought in via ApplyForParty (party-leading lord). Drives which
        // LeaveSettlementAction overload to use during cleanup.
        private MobileParty _currentGuestParty;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public bool VerboseDiagnostics
        {
            get { return _verboseDiagnostics; }
            set { _verboseDiagnostics = value; }
        }

        public string TargetHeroStringId
        {
            get { return _targetHeroStringId; }
            set { _targetHeroStringId = value; }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            // OnMissionEnded fires when the player leaves a mission scene (e.g., town menu → lord hall).
            // Vanilla HeroAgentSpawnCampaignBehavior uses the same hook to keep location populations fresh
            // between missions — which is when the lord hall's character list gets read for spawning.
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("FeastsOfCalradia_LordHallBotProbe_Enabled", ref _enabled);
            dataStore.SyncData("FeastsOfCalradia_LordHallBotProbe_VerboseDiagnostics", ref _verboseDiagnostics);
            dataStore.SyncData("FeastsOfCalradia_LordHallBotProbe_TargetHero", ref _targetHeroStringId);
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (!_enabled || party != MobileParty.MainParty)
            {
                return;
            }
            TrySpawnInSettlement(settlement, "SettlementEntered");
        }

        private void OnMissionEnded(IMission mission)
        {
            if (!_enabled)
            {
                return;
            }
            TrySpawnInSettlement(Settlement.CurrentSettlement, "OnMissionEnded");
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty)
            {
                return;
            }
            ReleaseGuest(settlement);
            _currentBot = null;
        }

        public string ForceSpawnInCurrentSettlement()
        {
            return TrySpawnInSettlement(Settlement.CurrentSettlement, "force-spawn command");
        }

        private string TrySpawnInSettlement(Settlement settlement, string source)
        {
            if (settlement == null || !settlement.IsFortification)
            {
                return Diagnostic("[" + source + "] skip: settlement null or not a fortification.");
            }
            if (LocationComplex.Current == null || PlayerEncounter.LocationEncounter == null)
            {
                return Diagnostic("[" + source + "] skip: LocationComplex.Current or PlayerEncounter.LocationEncounter is null.");
            }
            if (settlement.LocationComplex != LocationComplex.Current)
            {
                return Diagnostic("[" + source + "] skip: settlement's LocationComplex is not Current.");
            }

            Location lordHall = settlement.LocationComplex.GetLocationWithId("lordshall");
            if (lordHall == null)
            {
                return Diagnostic("[" + source + "] skip: no 'lordshall' location.");
            }

            // Pre-state diagnostic: list current lordshall characters.
            int existingCount = 0;
            var existing = new StringBuilder();
            foreach (LocationCharacter c in lordHall.GetCharacterList())
            {
                existingCount++;
                if (existing.Length > 0)
                {
                    existing.Append(", ");
                }
                existing.Append(c.Character?.Name?.ToString() ?? "?");
            }
            Diagnostic("[" + source + "] lordshall has " + existingCount + " character(s): " + (existingCount == 0 ? "<empty>" : existing.ToString()));

            Hero template = SelectTemplateHero();
            if (template?.CharacterObject == null)
            {
                return Diagnostic("[" + source + "] skip: no usable template hero.");
            }
            Diagnostic("Selected '" + template.Name + "' (" + template.StringId + "), faction=" + (template.MapFaction?.Name?.ToString() ?? "?") + ", clan=" + (template.Clan?.Name?.ToString() ?? "?") + ", isLord=" + template.IsLord + ", hasParty=" + (template.PartyBelongedTo != null));

            // Two paths depending on whether the chosen hero is a noble.
            //  Path A (full integration): lord/lady, no governor role. If partyless, ApplyForCharacterOnly
            //          marks them as staying in settlement. If they have a party, ApplyForParty pulls
            //          their whole party (with troops) into the settlement — same mechanism vanilla uses
            //          when an NPC lord visits another lord's fief. Either way, their party (if any)
            //          parks at your settlement and they appear in keep menu + lord hall scene.
            //  Path B (manual lord-hall placement): non-noble (e.g. fallback notable from tier 4). Skip
            //          ApplyForXXX entirely — vanilla would route a notable to town center, not lord hall.
            //          Just add a LocationCharacter directly to lordshall. Lord-hall scene only.
            bool canIntegrate = template.IsLord && template.GovernorOf == null;

            if (canIntegrate)
            {
                if (_currentGuestHero == template)
                {
                    return Diagnostic("[" + source + "] full-integration guest '" + template.Name + "' already applied this visit.");
                }
                ReleaseGuest(settlement);

                if (template.PartyBelongedTo != null)
                {
                    // Party-leading noble: bring their party into the settlement (vanilla "lord visits" pattern).
                    EnterSettlementAction.ApplyForParty(template.PartyBelongedTo, settlement);
                    _currentGuestParty = template.PartyBelongedTo;
                    _currentGuestHero = template;
                    return Diagnostic("[" + source + "] full integration via ApplyForParty: '" + template.Name + "' arrived with their party. Should appear in keep menu + lord hall.");
                }
                else
                {
                    EnterSettlementAction.ApplyForCharacterOnly(template, settlement);
                    _currentGuestHero = template;
                    return Diagnostic("[" + source + "] full integration via ApplyForCharacterOnly: '" + template.Name + "' is now staying in settlement. Should appear in keep menu + lord hall.");
                }
            }

            // Path B — manual lord-hall placement.
            bool botAlreadyPresent = false;
            foreach (LocationCharacter c in lordHall.GetCharacterList())
            {
                if (c == _currentBot)
                {
                    botAlreadyPresent = true;
                    break;
                }
            }
            if (botAlreadyPresent)
            {
                return Diagnostic("[" + source + "] manual bot for '" + template.Name + "' already in lordshall.");
            }

            LocationCharacter bot = BuildLocationCharacterFor(template);
            if (bot == null)
            {
                return Diagnostic("[" + source + "] skip: BuildLocationCharacterFor returned null.");
            }
            lordHall.AddCharacter(bot);
            _currentBot = bot;
            return Diagnostic("[" + source + "] manual placement of '" + template.Name + "' in lordshall (party-leading or non-noble; lord-hall scene only, not keep menu).");
        }

        private void ReleaseGuest(Settlement here)
        {
            // Party-path release: the guest came in via ApplyForParty (party leader). Send their party
            // back out via ApplyForParty's counterpart.
            if (_currentGuestParty != null)
            {
                if (_currentGuestParty.CurrentSettlement == here)
                {
                    Diagnostic("Releasing guest party '" + (_currentGuestParty.Name?.ToString() ?? "?") + "' from " + here.Name + ".");
                    LeaveSettlementAction.ApplyForParty(_currentGuestParty);
                }
                _currentGuestParty = null;
                _currentGuestHero = null;
                return;
            }
            // Character-path release: the guest came in via ApplyForCharacterOnly.
            if (_currentGuestHero == null)
            {
                return;
            }
            if (_currentGuestHero.StayingInSettlement == here)
            {
                Diagnostic("Releasing guest '" + _currentGuestHero.Name + "' from " + here.Name + ".");
                if (_currentGuestHero.CurrentSettlement != null)
                {
                    LeaveSettlementAction.ApplyForCharacterOnly(_currentGuestHero);
                }
                else
                {
                    _currentGuestHero.StayingInSettlement = null;
                }
            }
            _currentGuestHero = null;
        }

        // Builds a LocationCharacter for a specific hero using the bodyguard pattern (null spawnTag,
        // fixedLocation=false). Used by the manual placement path when the hero can't go through the
        // EnterSettlementAction integration (party leaders, governors, notables in fallback).
        private static LocationCharacter BuildLocationCharacterFor(Hero template)
        {
            AgentData agentData = new AgentData(new SimpleAgentOrigin(template.CharacterObject, -1, null, default))
                .Monster(FaceGen.GetBaseMonsterFromRace(template.CharacterObject.Race))
                .NoHorses(true);

            return new LocationCharacter(
                agentData,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                null,
                fixedLocation: false,
                LocationCharacter.CharacterRelations.Neutral,
                actionSetCode: null,
                useCivilianEquipment: true);
        }

        private Hero SelectTemplateHero()
        {
            Settlement here = Settlement.CurrentSettlement;

            // 1. Explicit target set via console command takes priority. Honored regardless of category
            // (noble vs notable, partyless vs party-leading) — caller decides what to do based on the
            // returned hero's properties.
            if (!string.IsNullOrEmpty(_targetHeroStringId))
            {
                foreach (Hero h in Hero.AllAliveHeroes)
                {
                    if (h.StringId == _targetHeroStringId)
                    {
                        return h;
                    }
                }
                // Target was set but the hero is no longer alive / findable. Fall through to defaults.
            }

            IFaction myFaction = Hero.MainHero?.MapFaction;

            // 2. Best: partyless noble in player's faction. ApplyForCharacterOnly works on them and
            // vanilla's HeroAgentLocationModel will route them to lord hall, giving us both keep menu
            // visibility and in-scene presence.
            foreach (Hero h in Hero.AllAliveHeroes)
            {
                if (!IsValidStranger(h, here))
                {
                    continue;
                }
                if (myFaction != null && h.MapFaction != myFaction)
                {
                    continue;
                }
                if (h.IsLord && h.PartyBelongedTo == null && h.GovernorOf == null)
                {
                    return h;
                }
            }

            // 3. Next: any partyless noble (cross-faction). Same routing benefits as tier 2.
            foreach (Hero h in Hero.AllAliveHeroes)
            {
                if (!IsValidStranger(h, here))
                {
                    continue;
                }
                if (h.IsLord && h.PartyBelongedTo == null && h.GovernorOf == null)
                {
                    return h;
                }
            }

            // 4. Fallback: any non-clan stranger (including notables). Manual lord-hall placement only —
            // they won't appear in the keep menu, and if vanilla decides their canonical location isn't
            // lord hall they may be moved out by RefreshLocationOfHeroForSettlement. Best-effort.
            foreach (Hero h in Hero.AllAliveHeroes)
            {
                if (IsValidStranger(h, here))
                {
                    return h;
                }
            }
            return null;
        }

        private static bool IsValidStranger(Hero h, Settlement here)
        {
            if (h.IsHumanPlayerCharacter)
            {
                return false;
            }
            if (h.Clan == Clan.PlayerClan)
            {
                return false;
            }
            if (here != null && h.CurrentSettlement == here)
            {
                return false;
            }
            return true;
        }

        // Used by the console command to resolve a user-typed name fragment to a specific hero.
        public static Hero FindAliveHeroByNameQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }
            foreach (Hero h in Hero.AllAliveHeroes)
            {
                if (h.IsHumanPlayerCharacter)
                {
                    continue;
                }
                string name = h.Name?.ToString();
                if (!string.IsNullOrEmpty(name) && name.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return h;
                }
            }
            return null;
        }

        private string Diagnostic(string message)
        {
            if (_verboseDiagnostics)
            {
                InformationManager.DisplayMessage(new InformationMessage("[LordHallBotProbe] " + message));
            }
            return message;
        }
    }
}
