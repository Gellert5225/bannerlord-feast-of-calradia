using System.Text;
using SandBox;
using TaleWorlds.CampaignSystem;
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
            if (party == MobileParty.MainParty)
            {
                _currentBot = null;
            }
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

            // Dedup: if our previously-added bot is still in lordshall, don't add another. If it's been
            // cleared by the engine between missions, _currentBot is stale and we re-add.
            bool botAlreadyPresent = false;
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
                if (c == _currentBot)
                {
                    botAlreadyPresent = true;
                }
            }

            string preState = "[" + source + "] lordshall has " + existingCount + " character(s): " + (existingCount == 0 ? "<empty>" : existing.ToString())
                              + " | our bot already present: " + botAlreadyPresent;
            Diagnostic(preState);

            if (botAlreadyPresent)
            {
                return preState;
            }

            LocationCharacter probeBot = CreateProbeBot();
            if (probeBot == null)
            {
                return Diagnostic("[" + source + "] skip: CreateProbeBot returned null.");
            }
            lordHall.AddCharacter(probeBot);
            _currentBot = probeBot;
            return Diagnostic("[" + source + "] added probe bot. lordshall now has " + (existingCount + 1) + " character(s).");
        }

        private LocationCharacter CreateProbeBot()
        {
            Hero template = SelectTemplateHero();
            if (template?.CharacterObject == null)
            {
                Diagnostic("CreateProbeBot: no usable template hero available.");
                return null;
            }
            Diagnostic("CreateProbeBot: selected '" + template.Name + "' (" + template.StringId + "), faction=" + (template.MapFaction?.Name?.ToString() ?? "?") + ", clan=" + (template.Clan?.Name?.ToString() ?? "?"));

            AgentData agentData = new AgentData(new SimpleAgentOrigin(template.CharacterObject, -1, null, default))
                .Monster(FaceGen.GetBaseMonsterFromRace(template.CharacterObject.Race))
                .NoHorses(true);

            // Bodyguard pattern: null spawnTag + fixedLocation=false lets the engine pick any valid spawn
            // point in the scene rather than requiring a specific sp_notable-tagged position.
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

            // 1. Explicit target set via console command takes priority.
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

            // 2. Default: a "stranger" — non-player, not in player's clan (skips spouse/siblings already
            // in the hall), not already at this settlement, ideally same MapFaction.
            IFaction myFaction = Hero.MainHero?.MapFaction;
            if (myFaction != null)
            {
                foreach (Hero h in Hero.AllAliveHeroes)
                {
                    if (!IsValidStranger(h, here))
                    {
                        continue;
                    }
                    if (h.MapFaction == myFaction)
                    {
                        return h;
                    }
                }
            }

            // 3. Last resort: any non-player non-clan stranger not at this settlement.
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
