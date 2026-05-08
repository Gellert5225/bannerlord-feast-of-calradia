# CampaignBehaviorBase

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem`. Game 1.3.15.110062.

```csharp
using System;

namespace TaleWorlds.CampaignSystem
{
    public abstract class CampaignBehaviorBase : ICampaignBehavior
    {
        public CampaignBehaviorBase(string stringId)
        {
            this.StringId = stringId;
        }

        public CampaignBehaviorBase()
        {
            this.StringId = base.GetType().Name;
        }

        public abstract void RegisterEvents();

        public static T GetCampaignBehavior<T>()
        {
            return Campaign.Current.GetCampaignBehavior<T>();
        }

        public abstract void SyncData(IDataStore dataStore);

        public readonly string StringId;
    }
}
```

## Notes

- Two abstract methods to implement: `RegisterEvents()` (event hookup) and `SyncData(IDataStore)` (save/load).
- `StringId` defaults to the class name if you use the parameterless constructor — sufficient for most cases.
- Static `GetCampaignBehavior<T>()` is the cross-behavior lookup utility (used from console commands etc.).
