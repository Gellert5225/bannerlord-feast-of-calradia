using System.Collections.Generic;
using FeastsOfCalradia.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace FeastsOfCalradia.Util
{
    public static class FeastsOfCalradiaCheats
    {
        // Invoked from the dev console as: campaign.list_clan_heroes
        // Requires cheat mode (set cheat_mode=1 in engine_config.txt) and an active campaign.
        [CommandLineFunctionality.CommandLineArgumentFunction("list_clan_heroes", "campaign")]
        public static string ListClanHeroes(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "No active campaign.";
            }

            ClanHeroesProbeBehavior behavior = Campaign.Current.GetCampaignBehavior<ClanHeroesProbeBehavior>();
            if (behavior == null)
            {
                return "ClanHeroesProbeBehavior not registered. Did the SubModule wire it up in InitializeGameStarter?";
            }

            return behavior.GetClanHeroSummary();
        }

        // Invoked from the dev console as: campaign.toggle_lord_hall_bot
        // Toggles whether a probe bot is spawned in the lord hall on settlement entry. The change
        // takes effect on the next settlement entry — bots already in a hall persist for that visit.
        [CommandLineFunctionality.CommandLineArgumentFunction("toggle_lord_hall_bot", "campaign")]
        public static string ToggleLordHallBot(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "No active campaign.";
            }

            LordHallBotProbeBehavior behavior = Campaign.Current.GetCampaignBehavior<LordHallBotProbeBehavior>();
            if (behavior == null)
            {
                return "LordHallBotProbeBehavior not registered. Did the SubModule wire it up in InitializeGameStarter?";
            }

            behavior.Enabled = !behavior.Enabled;
            return "Lord hall bot probe is now " + (behavior.Enabled ? "ENABLED" : "DISABLED") + ". Re-enter a settlement for the change to take effect.";
        }

        // Force-spawns a probe bot into the current settlement's lord hall right now, regardless of
        // whether the player triggered SettlementEntered this session. Use when already inside a settlement.
        [CommandLineFunctionality.CommandLineArgumentFunction("force_spawn_lord_hall_bot", "campaign")]
        public static string ForceSpawnLordHallBot(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "No active campaign.";
            }

            LordHallBotProbeBehavior behavior = Campaign.Current.GetCampaignBehavior<LordHallBotProbeBehavior>();
            if (behavior == null)
            {
                return "LordHallBotProbeBehavior not registered.";
            }

            return behavior.ForceSpawnInCurrentSettlement();
        }

        // Pin the lord-hall probe bot to a specific hero by name. "clear" resets to the default
        // (any non-player hero in the player's MapFaction). Re-enter the settlement after changing.
        [CommandLineFunctionality.CommandLineArgumentFunction("set_lord_hall_bot", "campaign")]
        public static string SetLordHallBot(List<string> strings)
        {
            if (Campaign.Current == null)
            {
                return "No active campaign.";
            }

            LordHallBotProbeBehavior behavior = Campaign.Current.GetCampaignBehavior<LordHallBotProbeBehavior>();
            if (behavior == null)
            {
                return "LordHallBotProbeBehavior not registered.";
            }

            if (strings == null || strings.Count == 0)
            {
                return "Usage: campaign.set_lord_hall_bot <hero name>  |  campaign.set_lord_hall_bot clear";
            }

            string query = string.Join(" ", strings).Trim();

            if (string.Equals(query, "clear", System.StringComparison.OrdinalIgnoreCase))
            {
                behavior.TargetHeroStringId = null;
                return "Target cleared. Default selection (any kingdom mate) will be used.";
            }

            Hero match = LordHallBotProbeBehavior.FindAliveHeroByNameQuery(query);
            if (match == null)
            {
                return "No alive hero matching '" + query + "' found.";
            }

            behavior.TargetHeroStringId = match.StringId;
            return "Lord hall bot will now spawn as " + match.Name + " (" + match.StringId + "). Re-enter the settlement to take effect.";
        }
    }
}
