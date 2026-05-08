# API Notes

Decompiled C# excerpts of TaleWorlds classes captured during development sessions.
Shipped here so future sessions don't have to redo the dnSpy work to reference these
patterns.

All pastes are from game version **1.3.15.110062**. If the game updates and a paste
becomes outdated, re-decompile the relevant DLL and update the file.

| File | Source DLL | Why it's here |
|---|---|---|
| `CampaignBehaviorBase.md` | `TaleWorlds.CampaignSystem.dll` | Base class for campaign-scoped behaviors. Abstract methods we override. |
| `IDataStore.md` | `TaleWorlds.CampaignSystem.dll` | Save/load persistence interface — the `SyncData<T>` signature |
| `SandBoxSubModule.md` | `Modules/SandBox/.../SandBox.dll` | Vanilla SubModule lifecycle and `InitializeGameStarter` registration pattern |
| `HeroAgentSpawnCampaignBehavior.md` | `TaleWorlds.CampaignSystem.dll` | The vanilla pattern for placing heroes into settlement locations as `LocationCharacter`s |
| `LocationCharacter.md` | `TaleWorlds.CampaignSystem.Settlements.Locations` | The data class that bridges hero → mission spawn. 14-arg constructor |
| `LocationComplex.md` | `TaleWorlds.CampaignSystem.Settlements.Locations` | The settlement's location dictionary. `GetLocationWithId`, `ChangeLocation`, etc. |
| `EnterSettlementAction.md` | `TaleWorlds.CampaignSystem.Actions` | Canonical "make a hero / party arrive at a settlement" action |
| `LeaveSettlementAction.md` | `TaleWorlds.CampaignSystem.Actions` | Counterpart for clean release |
| `CampaignCheats-console-command-pattern.md` | `TaleWorlds.CampaignSystem.dll` | Excerpt showing the `[CommandLineArgumentFunction]` attribute pattern |
