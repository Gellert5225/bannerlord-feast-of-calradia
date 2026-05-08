using Bannerlord.UIExtenderEx;
using FeastsOfCalradia.Behaviors;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace FeastsOfCalradia
{
    public class FeastsOfCalradiaSubModule : MBSubModuleBase
    {
        // Single Harmony instance for the whole mod. Patches are discovered via [HarmonyPatch] attributes.
        private static readonly Harmony Harmony = new Harmony("com.feastsofcalradia.harmony");

        // UIExtenderEx instance for Gauntlet-screen modifications. Each mod uses one UIExtender keyed by
        // a unique name; ViewModelMixin and PrefabExtension classes auto-register via attributes.
        private static readonly UIExtender UIExtender = UIExtender.Create("FeastsOfCalradia");

        protected override void OnSubModuleLoad()
        {
            // Bootstrap Harmony — scans the executing assembly for [HarmonyPatch]-decorated classes and
            // applies them. Currently a no-op (no patches yet), but proves the wiring works.
            Harmony.PatchAll(typeof(FeastsOfCalradiaSubModule).Assembly);

            // Bootstrap UIExtenderEx — scans the executing assembly for [ViewModelMixin] and
            // [PrefabExtension]-decorated classes and applies them to the matching vanilla screens.
            UIExtender.Register(typeof(FeastsOfCalradiaSubModule).Assembly);
            UIExtender.Enable();

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
                // The central feast-system behavior. Registers its own game-menu options via the
                // OnSessionLaunched event (vanilla menus aren't created yet at InitializeGameStarter time).
                campaignStarter.AddBehavior(new FeastSystemBehavior());

                // Throwaway probes — slated for deletion when Phase 1 architecture lands.
                campaignStarter.AddBehavior(new ClanHeroesProbeBehavior());

                // LordHallBotProbeBehavior disabled — was useful for the NPC-spawning spike but
                // intrusive during normal play. Re-enable for testing if needed.
                // campaignStarter.AddBehavior(new LordHallBotProbeBehavior());
            }
        }
    }
}
