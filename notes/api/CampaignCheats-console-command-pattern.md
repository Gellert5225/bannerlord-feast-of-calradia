# Console command pattern (CampaignCheats excerpt)

Source: `TaleWorlds.CampaignSystem.dll`, namespace `TaleWorlds.CampaignSystem`. Game 1.3.15.110062.

`CampaignCheats` is a `public static class` containing dozens of `[CommandLineFunctionality.CommandLineArgumentFunction]`-decorated methods that register dev console commands. The full class is huge; below is the minimum useful excerpt — helper utilities + one or two representative commands — to show the pattern.

## The attribute

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("verb", "category")]
public static string MethodName(List<string> strings) { ... }
```

- Lives in namespace `TaleWorlds.Library` (the `CommandLineFunctionality` class).
- Method must be `public static`, return `string`, take exactly `List<string> strings`.
- Invoked in dev console as `category.verb`. Vanilla campaign commands all use `"campaign"` as the category, so they're invoked as `campaign.verb`.

## Minimal viable command

```csharp
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

public static class MyCheats
{
    [CommandLineFunctionality.CommandLineArgumentFunction("hello", "myMod")]
    public static string Hello(List<string> strings)
    {
        return "Hello from MyMod!";
    }
}
```

Invoked: `myMod.hello` → prints `Hello from MyMod!` in the console.

## Helpers used by vanilla commands

```csharp
public static class CampaignCheats
{
    public static bool CheckCheatUsage(ref string ErrorType)
    {
        if (Campaign.Current == null) { ErrorType = "Campaign was not started."; return false; }
        if (!Game.Current.CheatMode) { ErrorType = "Cheat mode is disabled!"; return false; }
        ErrorType = ""; return true;
    }

    public static bool CheckParameters(List<string> strings, int ParameterCount)
    {
        if (strings.Count == 0) return ParameterCount == 0;
        return strings.Count == ParameterCount;
    }

    public static bool CheckHelp(List<string> strings)
    {
        return strings.Count != 0 && strings[0].ToLower() == "help";
    }

    // Multi-arg commands separate parameters with the pipe character. GetSeparatedNames splits on "|".
    public static List<string> GetSeparatedNames(List<string> strings, bool removeEmptySpaces = false);
    public static string ConcatenateString(List<string> strings);

    // Generic "find object by name fragment" helper — looks up a Hero, Settlement, etc. by their
    // display name. Variants exist; signatures roughly:
    //   bool TryGetObject<T>(string requestedId, out T obj, out string errorMessage, Func<T, bool> filter)
}
```

## Typical command shape

```csharp
[CommandLineFunctionality.CommandLineArgumentFunction("import_main_hero", "campaign")]
public static string ImportMainHero(List<string> strings)
{
    string error = string.Empty;
    if (!CampaignCheats.CheckCheatUsage(ref error)) return error;

    string usage = "Format is \"campaign.import_main_hero [filenamewithoutextension]\".";
    if (CampaignCheats.CheckParameters(strings, 0)) return usage;

    try
    {
        string text = CampaignCheats.ConcatenateString(strings);
        // ... do the work, return success or error message
        return "Main hero was imported successfully.";
    }
    catch
    {
        return "An error occurred";
    }
}
```

## Notes

- The dev console is reachable when **cheat mode is enabled** in `engine_config.txt` (`cheat_mode = 1`). Without that, `~` does nothing.
- The engine **scans loaded assemblies** for the attribute, so commands defined in mod DLLs are auto-registered without explicit hookup.
- Pipe-separated arguments (`campaign.cmd foo bar | baz | quux`) are common — use `GetSeparatedNames(strings, ...)` to split. Single-string args can use `ConcatenateString` to rejoin.
- Returning the format string when args are missing (and on `help` keyword) is the established UX.
- Commands are static, so they can't directly access `CampaignBehavior` instance state — use `Campaign.Current.GetCampaignBehavior<T>()` to look up your behavior, then call methods on it.
