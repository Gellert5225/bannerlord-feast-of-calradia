# IDataStore

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem` (NOT `TaleWorlds.SaveSystem` despite the SaveSystem.dll name). Game 1.3.15.110062.

```csharp
namespace TaleWorlds.CampaignSystem
{
    public interface IDataStore
    {
        bool SyncData<T>(string key, ref T data);
        bool IsSaving { get; }
        bool IsLoading { get; }
    }
}
```

## Notes

- Single bidirectional method: `SyncData<T>` reads `data` on save, writes `data` on load.
- Returns `bool` (not `void`) — true if the key was present in the data store. On a fresh load with a new key, returns false and leaves `data` untouched.
- `IsSaving` / `IsLoading` for branching when load needs different logic from save (e.g. recompute caches on load).
- Heavily namespace your keys to avoid mod collisions: `<ModName>_<Behavior>_<Field>`.
