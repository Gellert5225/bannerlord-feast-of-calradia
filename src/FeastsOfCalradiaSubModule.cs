using FeastsOfCalradia.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace FeastsOfCalradia
{
    public class FeastsOfCalradiaSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            InformationMessage message = new InformationMessage("Hello from FeastsOfCalradia!");

            // VERIFY: InitialStateOption's 5th argument changed from bool to Func<(bool, TextObject)> in
            // some Bannerlord version. The lambda returns (isDisabled, disabledReasonText). Returning
            // (false, empty) means "always enabled, no tooltip". Confirm tooltip text shows nothing in-game
            // when the option is hovered.
            InitialStateOption initStateOpt = new InitialStateOption(
                "FeastsOfCalradia",
                new TextObject("FeastsOfCalradia", null),
                9990,
                () => InformationManager.DisplayMessage(message),
                () => (false, new TextObject("", null))
            );

            Module.CurrentModule.AddInitialStateOption(initStateOpt);
        }

        protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
        {
            base.InitializeGameStarter(game, gameStarterObject);

            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter campaignStarter)
            {
                campaignStarter.AddBehavior(new ClanHeroesProbeBehavior());
                campaignStarter.AddBehavior(new LordHallBotProbeBehavior());
            }
        }
    }
}
