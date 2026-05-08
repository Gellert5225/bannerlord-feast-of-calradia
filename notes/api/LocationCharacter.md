# LocationCharacter

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem.Settlements.Locations`. Game 1.3.15.110062.

The data class that bridges a hero/character to a mission spawn — added to a `Location`'s character list, then spawned as an `Agent` when the mission for that location starts.

```csharp
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Settlements.Locations
{
    public class LocationCharacter
    {
        public CharacterObject Character => (CharacterObject)this.AgentData.AgentCharacter;
        public IAgentOriginBase AgentOrigin => this.AgentData.AgentOrigin;
        public AgentData AgentData { get; }
        public bool UseCivilianEquipment { get; }
        public string ActionSetCode { get; }
        public string AlarmedActionSetCode { get; }
        public string SpecialTargetTag { get; set; }
        public bool ForceSpawnInSpecialTargetTag { get; set; }
        public LocationCharacter.AddBehaviorsDelegate AddBehaviors { get; }
        public LocationCharacter.AfterAgentCreatedDelegate AfterAgentCreated { get; }
        public bool FixedLocation { get; }
        public Alley MemberOfAlley { get; private set; }
        public ItemObject SpecialItem { get; }

        public LocationCharacter(
            AgentData agentData,
            LocationCharacter.AddBehaviorsDelegate addBehaviorsDelegate,
            string spawnTag,
            bool fixedLocation,
            LocationCharacter.CharacterRelations characterRelation,
            string actionSetCode,
            bool useCivilianEquipment,
            bool isFixedCharacter = false,
            ItemObject specialItem = null,
            bool isHidden = false,
            bool isVisualTracked = false,
            bool overrideBodyProperties = true,
            LocationCharacter.AfterAgentCreatedDelegate afterAgentCreated = null,
            bool forceSpawnOnSpecialTargetTag = false)
        {
            // Constructor body sets BodyProperties based on character equipment + a seed, then assigns
            // properties. ActionSetCode defaults to villager action set if null is passed.
        }

        // Static factory used by vanilla for bodyguards in lord halls — useful as a "minimal viable"
        // LocationCharacter pattern.
        public static LocationCharacter CreateBodyguardHero(Hero hero, MobileParty party, AddBehaviorsDelegate addBehaviorsDelegate)
        {
            UniqueTroopDescriptor uniqueNo = new UniqueTroopDescriptor(FlattenedTroopRoster.GenerateUniqueNoFromParty(party, 0));
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(hero.CharacterObject.Race, "_settlement");
            return new LocationCharacter(
                new AgentData(new PartyAgentOrigin(PartyBase.MainParty, hero.CharacterObject, -1, uniqueNo, false, false))
                    .Monster(monsterWithSuffix).NoHorses(true),
                addBehaviorsDelegate,
                null,                    // spawnTag = null
                false,                   // fixedLocation = false
                CharacterRelations.Friendly,
                null,                    // actionSetCode = null (defaults to _villager)
                !PlayerEncounter.LocationEncounter.Settlement.IsVillage,
                false, null, false, false, true, null, false);
        }

        public delegate void AddBehaviorsDelegate(IAgent agent);
        public delegate void AfterAgentCreatedDelegate(IAgent agent);
        public enum CharacterRelations { Neutral, Friendly, Enemy }
    }
}
```

## Notes

- 14-arg constructor; **7 are optional** (defaulted). Minimal viable invocation: `agentData`, `addBehaviorsDelegate`, `spawnTag`, `fixedLocation`, `characterRelation`, `actionSetCode`, `useCivilianEquipment`.
- **`spawnTag = null` + `fixedLocation = false`** is the "bodyguard pattern" — gives the engine flexibility to pick any valid spawn point in the scene rather than requiring a specific `sp_*`-tagged position. Use this for testing.
- **`actionSetCode = null`** → constructor defaults to villager-suffix animations. For lords use `ActionSetCode.GenerateActionSetNameWithSuffix(monster, isFemale, "_lord")`.
- `AgentData` builder pattern: `new AgentData(new SimpleAgentOrigin(characterObject, -1, null, default)).Monster(...).NoHorses(true)`.
- **`AddBehaviorsDelegate`** sources from `SandBoxManager.Instance.AgentBehaviorManager`:
  - `AddCompanionBehaviors` — companions/clan members (active follow behavior)
  - `AddFixedCharacterBehaviors` — static idle (notables, lords standing around)

## Common spawn tags (from HeroAgentSpawnCampaignBehavior.CreateLocationCharacterForHero)

| Tag | Used for |
|---|---|
| `sp_throne` | Settlement king/queen (in lord hall) |
| `sp_governor` | Governor (in lord hall) |
| `sp_notable` | Generic notable in fortification |
| `sp_notable_artisan` / `_merchant` / `_preacher` / `_gangleader` / `_rural_notable` | Specific notable subtypes |
| `sp_prisoner` | Prisoner (with `forceSpawnOnSpecialTargetTag = true`) |
| `npc_common` | Wanderers / common NPCs |
