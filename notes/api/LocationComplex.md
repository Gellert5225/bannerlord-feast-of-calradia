# LocationComplex

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem.Settlements.Locations`. Game 1.3.15.110062.

The settlement-scoped dictionary of `Location`s. Each fortification has a LocationComplex with locations like `lordshall`, `tavern`, `prison`, `town_center`, etc.

```csharp
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TaleWorlds.CampaignSystem.Settlements.Locations
{
    public class LocationComplex
    {
        // Static current — null unless a LocationEncounter is active.
        public static LocationComplex Current
        {
            get
            {
                if (PlayerEncounter.LocationEncounter != null)
                    return PlayerEncounter.LocationEncounter.Settlement.LocationComplex;
                return null;
            }
        }

        // Predicates used by Location.CanCharacterEnter style filters.
        public static bool CanAlways(LocationCharacter c, Location l) => true;
        public static bool CanNever(LocationCharacter c, Location l) => false;
        public static bool CanIfHero(LocationCharacter c, Location l) => c.Character.IsHero;
        public static bool CanIfDay(LocationCharacter c, Location l) => !Campaign.Current.IsNight;
        public static bool CanIfPriceIsPaid(LocationCharacter c, Location l)
        {
            // For "lordshall" requires bribe to enter lord hall == 0; for "prison" same for dungeon.
        }
        // CanIfMaleOrHero, CanIfGrownUpMaleOrHero, CanIfSettlementAccessModelLetsPlayer ...

        public LocationComplex() { _locations = new Dictionary<string, Location>(); }
        public LocationComplex(LocationComplexTemplate template) : this() { /* populate from template */ }

        public void AddPassage(Location a, Location b) { /* makes locations bidirectionally adjacent */ }

        public void ChangeLocation(LocationCharacter character, Location from, Location to)
        {
            // Removes character from `from` (if non-null), adds to `to` (if non-null), notifies AI,
            // and notifies PlayerEncounter.LocationEncounter if a mission is active in the relevant scene.
        }

        public IEnumerable<LocationCharacter> GetListOfCharactersInLocation(string locationName);
        public IList<LocationCharacter> GetListOfCharacters();  // across all locations
        public IEnumerable<Location> GetListOfLocations();

        public Location GetLocationOfCharacter(LocationCharacter character);
        public Location GetLocationOfCharacter(Hero hero);
        public LocationCharacter GetLocationCharacterOfHero(Hero hero);
        public LocationCharacter GetFirstLocationCharacterOfCharacter(CharacterObject character);

        public void RemoveCharacterIfExists(Hero hero);
        public void RemoveCharacterIfExists(LocationCharacter locationCharacter);
        public void ClearTempCharacters();  // wipes all locations' character lists

        public Location GetLocationWithId(string id);  // <-- main lookup. Returns null if not present.
        public string GetScene(string stringId, int upgradeLevel);
        public LocationCharacter FindCharacter(IAgent agent);
        public IEnumerable<Location> FindAll(Func<string, bool> predicate);

        [SaveableField(1)]
        private readonly Dictionary<string, Location> _locations;
    }
}
```

## Notes

- `_locations` is keyed by **string id** (e.g., `"lordshall"`, `"tavern"`, `"prison"`, `"town_center"`).
- `GetLocationWithId("lordshall")` returns the lord hall `Location` for fortifications, null for villages.
- `GetLocationOfCharacter` and `GetLocationCharacterOfHero` are the lookup-by-hero utilities — useful for "is this hero already placed?" checks.
- `ChangeLocation` is the canonical "move character between locations" call; vanilla `HeroAgentSpawnCampaignBehavior.RefreshLocationOfHeroForSettlement` uses it to relocate misplaced characters.
- **`ClearTempCharacters` exists** — wipes all character lists across all locations. Don't know offhand who calls it, but if you find your `LocationCharacter` mysteriously disappearing, search for usages of this method.

## Confirmed location IDs visible from the decompile

- `"lordshall"` (in `CanIfPriceIsPaid` — bribe-to-enter check)
- `"prison"` (same)
