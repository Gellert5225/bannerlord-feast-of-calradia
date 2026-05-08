using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using PrefabAttribute = Bannerlord.UIExtenderEx.Prefabs2.PrefabExtensionSetAttributePatch.Attribute;

namespace FeastsOfCalradia.UI
{
    // Vanilla Clan screen's finance panel (gold / income / expenses, on the right side) is shown on
    // every tab via no IsVisible binding. Our Feast tab uses the right column for its own content,
    // so we bind the finance widget's IsVisible to a mixin property that's true on vanilla tabs and
    // false on the Feast tab.
    [PrefabExtension(
        "ClanScreen",
        "descendant::Widget[@Id='FinancePanelWidget']")]
    internal sealed class ClanScreenFinancePanelHidePatch : PrefabExtensionSetAttributePatch
    {
        public override List<PrefabAttribute> Attributes => new List<PrefabAttribute>
        {
            new PrefabAttribute("IsVisible", "@IsFinancePanelVisible"),
        };
    }
}
