# LeaveSettlementAction

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem.Actions`. Game 1.3.15.110062.

Counterpart to `EnterSettlementAction`. Cleans up settlement state and fires the `OnSettlementLeft` event chain.

```csharp
using System;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace TaleWorlds.CampaignSystem.Actions
{
    public static class LeaveSettlementAction
    {
        public static void ApplyForParty(MobileParty mobileParty)
        {
            Settlement currentSettlement = mobileParty.CurrentSettlement;
            // If army leader, recurse for attached parties (player encounter finishes, others leave).
            if (mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty) { /* ... */ }
            if (mobileParty == MobileParty.MainParty && /* not army-attached */) mobileParty.SetMoveModeHold();
            mobileParty.CurrentSettlement = null;
            if (mobileParty.IsCurrentlyAtSea) mobileParty.Anchor.ResetPosition();
            currentSettlement.SettlementComponent.OnPartyLeft(mobileParty);
            CampaignEventDispatcher.Instance.OnSettlementLeft(mobileParty, currentSettlement);
        }

        public static void ApplyForCharacterOnly(Hero hero)
        {
            Settlement currentSettlement = hero.CurrentSettlement;  // <-- dereferenced, can NRE if null
            hero.StayingInSettlement = null;
            LocationComplex locationComplex = currentSettlement.LocationComplex;
            Location location = locationComplex?.GetLocationOfCharacter(hero);
            if (location != null && location.GetLocationCharacter(hero) != null)
            {
                currentSettlement.LocationComplex.RemoveCharacterIfExists(hero);
                LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
                if (locationEncounter == null) return;
                locationEncounter.RemoveAccompanyingCharacter(hero);
            }
        }
    }
}
```

## Notes

- **`ApplyForCharacterOnly` does more than `hero.StayingInSettlement = null`** — it also removes the hero's `LocationCharacter` from any location they're in, and from the LocationEncounter's accompanying-character list. Use this for cleanup rather than direct property assignment.
- **`hero.CurrentSettlement` is dereferenced** at the start of `ApplyForCharacterOnly`. Guard against null in pathological cases (e.g., hero already left some other way).
- `ApplyForParty` mirrors `EnterSettlementAction.ApplyForParty` — releases a party from `currentSettlement` back to the world map. Call this on the guest's party if you brought them in via `EnterSettlementAction.ApplyForParty`.
- Both fire `OnSettlementLeft` events, so other behaviors (including ours) get notified.
