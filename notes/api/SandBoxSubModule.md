# SandBoxSubModule

Source: `Modules/SandBox/bin/Win64_Shipping_Client/SandBox.dll`, namespace `SandBox`. Game 1.3.15.110062.

The vanilla SubModule for the SandBox campaign module. Useful as a reference for the full `MBSubModuleBase` lifecycle and the canonical pattern for registering `CampaignBehaviorBase`s and `GameModel`s.

```csharp
using System;
using SandBox.AI;
using SandBox.CampaignBehaviors;
using SandBox.GameComponents;
using SandBox.Issues;
using SandBox.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace SandBox
{
    public class SandBoxSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Module.CurrentModule.SetEditorMissionTester(new SandBoxEditorMissionTester());
            TauntUsageManager.Initialize();
        }

        protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                gameStarterObject.AddModel<AgentStatCalculateModel>(new SandboxAgentStatCalculateModel());
                gameStarterObject.AddModel<StrikeMagnitudeCalculationModel>(new SandboxStrikeMagnitudeModel());
                // ... many more AddModel calls ...

                CampaignGameStarter campaignGameStarter = gameStarterObject as CampaignGameStarter;
                if (campaignGameStarter != null)
                {
                    campaignGameStarter.AddBehavior(new HideoutConversationsCampaignBehavior());
                    campaignGameStarter.AddBehavior(new AlleyCampaignBehavior());
                    campaignGameStarter.AddBehavior(new CommonTownsfolkCampaignBehavior());
                    // ... ~30 more AddBehavior calls — see full decompile if needed ...
                }
            }
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            Campaign campaign = game.GameType as Campaign;
            if (campaign != null)
            {
                SandBoxManager sandBoxManager = campaign.SandBoxManager;
                sandBoxManager.SandBoxMissionManager = new SandBoxMissionManager();
                sandBoxManager.AgentBehaviorManager = new AgentBehaviorManager();
                sandBoxManager.SandBoxSaveManager = new SandBoxSaveManager();
            }
        }

        public override void OnGameInitializationFinished(Game game)
        {
            Campaign campaign = game.GameType as Campaign;
            if (campaign != null)
            {
                campaign.CampaignMissionManager = new CampaignMissionManager();
                campaign.MapSceneCreator = new MapSceneCreator();
                campaign.EncyclopediaManager.CreateEncyclopediaPages();
                this.OnRegisterTypes();
            }
        }

        public override void OnGameLoaded(Game game, object starterObject)
        {
            Campaign campaign = game.GameType as Campaign;
            if (campaign != null)
            {
                SandBoxManager sandBoxManager = campaign.SandBoxManager;
                sandBoxManager.SandBoxMissionManager = new SandBoxMissionManager();
                sandBoxManager.AgentBehaviorManager = new AgentBehaviorManager();
                sandBoxManager.SandBoxSaveManager = new SandBoxSaveManager();
            }
        }

        // ... OnRegisterTypes, RegisterSubModuleObjects, AfterRegisterSubModuleObjects,
        // StartGame, OnBeforeInitialModuleScreenSetAsRoot, OnConfigChanged, OnNewModuleLoad ...

        private bool _initialized;
    }
}
```

## Notes

- The override for registering campaign behaviors is **`InitializeGameStarter(Game, IGameStarter)`** — NOT `OnGameStart` (which is for different lifecycle reasons).
- Pattern: `if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter cgs) cgs.AddBehavior(...)`.
- `OnCampaignStart` and `OnGameLoaded` both initialize `SandBoxManager`'s sub-managers — note both are needed (one for new campaigns, one for loaded).
- `SandBoxManager.Instance.AgentBehaviorManager` exposes pre-built `AddBehaviorsDelegate` factories (`AddCompanionBehaviors`, `AddFixedCharacterBehaviors`) used when building `LocationCharacter`s.
