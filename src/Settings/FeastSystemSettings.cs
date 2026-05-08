using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace FeastsOfCalradia.Settings
{
    // MCM global settings — shared across all saves on this installation. AttributeGlobalSettings auto-
    // discovers properties decorated with [SettingProperty*] and renders them in the MCM menu.
    // VERIFY: namespace paths for MCM.Abstractions.Attributes.v2 and Base.Global match MCM 5.11.4. If
    // build fails on missing namespaces, decompile MCMv5.dll to find the correct paths (the API has
    // shifted between minor versions historically).
    public class FeastSystemSettings : AttributeGlobalSettings<FeastSystemSettings>
    {
        public override string Id { get; } = "FeastsOfCalradia_v1";
        public override string FolderName { get; } = "FeastsOfCalradia";
        public override string DisplayName => "Feasts of Calradia";

        // MCM v5's default FormatType is "none" — settings won't persist to disk without this override.
        // "json" and "xml" are the built-in formats; both write to %USERPROFILE%\Documents\Mount and Blade
        // II Bannerlord\Configs\ModSettings\Global\<FolderName>\<Id>.<ext>.
        public override string FormatType => "json";

        [SettingPropertyBool(
            "Enabled",
            RequireRestart = false,
            HintText = "Master toggle for the feast system. When disabled, no feasts will be triggered or hosted.")]
        public bool Enabled { get; set; } = true;

        [SettingPropertyBool(
            "Verbose Logging",
            RequireRestart = false,
            HintText = "Print detailed feast-system events to the in-game info corner. Useful while developing.")]
        public bool VerboseLogging { get; set; } = false;
    }
}
