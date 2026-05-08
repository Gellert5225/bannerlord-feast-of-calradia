using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using PrefabAttribute = Bannerlord.UIExtenderEx.Prefabs2.PrefabExtensionSetAttributePatch.Attribute;

namespace FeastsOfCalradia.UI
{
    // Shrinks the Clan-screen tab buttons so all five fit within the 887px tab-strip background
    // without overflowing into the renown UI. Done by changing the MultiplyResult on the three
    // "Header.Tab.*.Width.Scaled" constants at the top of ClanScreen.xml — vanilla uses 0.90; we
    // drop to 0.72 so five tabs fit where four did. Heights left alone.
    //
    // VERIFY: 0.72 is approximate. If overflow remains at low UI scales drop further; if tabs look
    // too cramped at high UI scales raise it.

    [PrefabExtension(
        "ClanScreen",
        "descendant::Constant[@Name='Header.Tab.Left.Width.Scaled']")]
    internal sealed class ClanTabLeftWidthPatch : PrefabExtensionSetAttributePatch
    {
        public override List<PrefabAttribute> Attributes => new List<PrefabAttribute>
        {
            new PrefabAttribute("MultiplyResult", "0.72"),
        };
    }

    [PrefabExtension(
        "ClanScreen",
        "descendant::Constant[@Name='Header.Tab.Center.Width.Scaled']")]
    internal sealed class ClanTabCenterWidthPatch : PrefabExtensionSetAttributePatch
    {
        public override List<PrefabAttribute> Attributes => new List<PrefabAttribute>
        {
            new PrefabAttribute("MultiplyResult", "0.72"),
        };
    }

    [PrefabExtension(
        "ClanScreen",
        "descendant::Constant[@Name='Header.Tab.Right.Width.Scaled']")]
    internal sealed class ClanTabRightWidthPatch : PrefabExtensionSetAttributePatch
    {
        public override List<PrefabAttribute> Attributes => new List<PrefabAttribute>
        {
            new PrefabAttribute("MultiplyResult", "0.72"),
        };
    }
}
