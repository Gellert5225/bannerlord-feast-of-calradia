# HeroAgentSpawnCampaignBehavior

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem.CampaignBehaviors`. Game 1.3.15.110062.

The vanilla campaign behavior responsible for placing heroes into settlement locations as `LocationCharacter`s. Reference for hooking the right events and constructing `LocationCharacter`s.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors
{
    public class HeroAgentSpawnCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.PrisonersChangeInSettlement.AddNonSerializedListener(this, new Action<Settlement, FlattenedTroopRoster, Hero, bool>(this.OnPrisonersChangeInSettlement));
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, new Action<Town, Hero, Hero>(this.OnGovernorChanged));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(this.OnSettlementLeft));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, new Action<PartyBase, Hero>(this.OnHeroPrisonerTaken));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.OnGameLoadFinished));
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionEnded));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void RefreshLocationOfHeroesForPlayersCurrentSettlement()
        {
            if (LocationComplex.Current != null && Settlement.CurrentSettlement != null
                && (Settlement.CurrentSettlement.IsFortification || Settlement.CurrentSettlement.IsVillage)
                && LocationComplex.Current == Settlement.CurrentSettlement.LocationComplex)
            {
                Settlement currentSettlement = Settlement.CurrentSettlement;
                List<Hero> list = currentSettlement.HeroesWithoutParty.ToList<Hero>();
                Hero hero = currentSettlement.MapFaction.IsKingdomFaction ? ((Kingdom)currentSettlement.MapFaction).Leader : currentSettlement.OwnerClan.Leader;
                Hero hero2 = (hero != null) ? hero.Spouse : null;
                if (hero != null) list.Add(hero);
                if (hero2 != null) list.Add(hero2);
                list.AddRange(Clan.PlayerClan.AliveLords);
                list.AddRange(Hero.MainHero.CompanionsInParty);
                list.AddRange(from x in currentSettlement.SettlementComponent.GetPrisonerHeroes() select x.HeroObject);
                foreach (MobileParty mobileParty in currentSettlement.Parties)
                {
                    if (mobileParty.LeaderHero != null && mobileParty.LeaderHero != Hero.MainHero)
                        list.Add(mobileParty.LeaderHero);
                }
                foreach (Hero hero3 in list)
                    this.RefreshLocationOfHeroForSettlement(hero3, currentSettlement);
            }
        }

        private void RefreshLocationOfHeroForSettlement(Hero hero, Settlement settlement)
        {
            Location locationOfCharacter = settlement.LocationComplex.GetLocationOfCharacter(hero);
            HeroAgentLocationModel.HeroLocationDetail heroLocationDetail;
            Location locationForHero = Campaign.Current.Models.HeroAgentLocationModel.GetLocationForHero(hero, settlement, out heroLocationDetail);
            if (locationOfCharacter == null && locationForHero != null)
            {
                LocationCharacter locationCharacter = this.CreateLocationCharacterForHero(hero, settlement, heroLocationDetail);
                locationForHero.AddCharacter(locationCharacter);
                return;
            }
            if (locationOfCharacter != null && locationOfCharacter != locationForHero)
            {
                LocationCharacter locationCharacterOfHero = settlement.LocationComplex.GetLocationCharacterOfHero(hero);
                settlement.LocationComplex.ChangeLocation(locationCharacterOfHero, locationOfCharacter, locationForHero);
            }
        }

        // CreateLocationCharacterForHero — see LocationCharacter.md for the constructor pattern; this
        // method picks per-role spawn tags ("sp_throne" for kings, "sp_notable_*" for notables, etc.) and
        // action set codes via ActionSetCode.GenerateActionSetNameWithSuffix.

        // Event handlers that all converge on RefreshLocation...:
        public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null
                && settlement.LocationComplex == LocationComplex.Current)
                this.RefreshLocationOfHeroesForPlayersCurrentSettlement();
        }

        private void OnMissionEnded(IMission mission)
        {
            if (LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null
                && Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner
                && !Settlement.CurrentSettlement.IsUnderSiege)
                this.RefreshLocationOfHeroesForPlayersCurrentSettlement();
        }

        // Other handlers (OnSettlementLeft, OnGovernorChanged, OnGameLoadFinished, OnHeroPrisonerTaken,
        // OnPrisonersChangeInSettlement) — same pattern, different trigger conditions. See full decompile
        // if needed.
    }
}
```

## Notes

- The two main hook events are **`SettlementEntered`** (player enters a settlement) and **`OnMissionEndedEvent`** (player leaves a mission scene — fires between town menu and lord hall, etc.).
- Always check `LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null` before acting — firing too early during settlement transitions causes silent no-ops.
- `RefreshLocationOfHeroForSettlement` is idempotent: adds the hero if missing, moves them if in the wrong location, no-op if already in the right one. So re-firing handlers is safe.
- **`HeroAgentLocationModel.GetLocationForHero(hero, settlement, out detail)`** is the routing oracle — given a hero and settlement, it returns which location that hero "belongs" in (lord hall, town center, etc.) plus a `HeroLocationDetail` enum that drives spawn-tag and action-set choices.
