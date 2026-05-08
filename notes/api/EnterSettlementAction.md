# EnterSettlementAction

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem.Actions`. Game 1.3.15.110062.

The canonical action for moving a hero or party into a settlement. Sets state, fires the `OnSettlementEntered` event chain. Use this rather than direct property assignment.

```csharp
using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace TaleWorlds.CampaignSystem.Actions
{
    public static class EnterSettlementAction
    {
        // Internal — fires OnBefore/On/OnAfter SettlementEntered events plus prisoner-change events.
        // Has special-case handling: army merge, fleeing parties, "becoming emissary" for partyless
        // player-clan heroes that are not governors. Don't call directly.
        private static void ApplyInternal(Hero hero, MobileParty mobileParty, Settlement settlement, EnterSettlementDetail detail, object subject = null, bool isPlayerInvolved = false);

        // For a party (with troops) — sets party.CurrentSettlement = settlement, fires events. Also
        // handles army-leader interactions and at-sea state (for navies). Use this for noble lords with
        // parties; their party is pulled to the settlement (vanilla "lord visits" pattern).
        public static void ApplyForParty(MobileParty mobileParty, Settlement settlement);

        // For a party entering an alley — internal sub-pattern, less commonly needed.
        public static void ApplyForPartyEntersAlley(MobileParty party, Settlement settlement, Alley alley, bool isPlayerInvolved = false);

        // For a hero WITHOUT a party — sets hero.StayingInSettlement = settlement, fires events.
        // Has a side-effect for partyless player-clan members who aren't governors: triggers
        // "BecomeEmissary" via OnHeroGetsBusy. Filter our candidates to avoid that path.
        public static void ApplyForCharacterOnly(Hero hero, Settlement settlement)
        {
            hero.StayingInSettlement = settlement;
            EnterSettlementAction.ApplyInternal(hero, null, settlement, EnterSettlementDetail.Character, null, false);
        }

        // For taking a prisoner. Changes hero state to Prisoner, fires events.
        public static void ApplyForPrisoner(Hero hero, Settlement settlement)
        {
            hero.ChangeState(Hero.CharacterStates.Prisoner);
            EnterSettlementAction.ApplyInternal(hero, null, settlement, EnterSettlementDetail.Prisoner, null, false);
        }

        private enum EnterSettlementDetail { WarParty, PartyEntersAlley, Character, Prisoner }
    }
}
```

## Notes

- **Three public Apply methods**: `ApplyForParty`, `ApplyForCharacterOnly`, `ApplyForPrisoner` (plus the alley special case).
- `ApplyForCharacterOnly` is the partyless-hero version. Sets `StayingInSettlement` (which is what the keep menu queries).
- `ApplyForParty` brings the party WITH its troops into the settlement. The party parks at the settlement and re-emerges only after `LeaveSettlementAction.ApplyForParty`. This is the canonical "an NPC lord visits another lord's fief" mechanism.
- **Both fire `OnSettlementEntered` events**, which causes vanilla's `HeroAgentSpawnCampaignBehavior` to run `RefreshLocationOfHeroForSettlement` for the just-arrived hero — placing them in their canonical location (lord hall for nobles, town center for notables) automatically.
- Do not call `ApplyForCharacterOnly` on a hero in a player's clan unless they're a governor — triggers `OnHeroGetsBusy(hero, BecomeEmissary)` which has campaign-state implications.
