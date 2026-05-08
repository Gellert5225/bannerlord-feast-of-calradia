using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace FeastsOfCalradia.UI
{
    // Inserts a "Feast" tab in the Clan screen's tab strip as the FOURTH tab (between Fiefs and the
    // rightmost tab — Income / "Other"). The XPath anchors on the Income tab button (uniquely
    // identified by its IsSelected binding to @IsIncomeSelected); InsertType.Prepend makes our new
    // tab a sibling immediately BEFORE Income in the same <Children> block.
    //
    // The "Feast" tab is a pseudo-tab: clicking it calls our ExecuteHostFeast command rather than
    // SetSelectedCategory, so the visible panel doesn't switch. This is a v0 simplification — proper
    // tab integration would require patching ClanManagementVM to support a 5th category and adding a
    // matching content panel.
    //
    // VERIFY: brush "Header.Tab.Center" is reused from the Parties/Fiefs tabs. The Income tab uses
    // "Header.Tab.Right" because it's the rightmost tab, so visually our new tab should be the
    // rightmost-with-end-cap and Income should change to Center. Acceptable for v0; cleanup involves
    // a separate Replace patch on the Income tab brush.
    [PrefabExtension(
        "ClanScreen",
        "descendant::ButtonWidget[@IsSelected='@IsIncomeSelected']")]
    internal sealed class ClanScreenHostFeastButtonPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Prepend;
        public override int Index => 0;

        private readonly XmlDocument _document;

        public ClanScreenHostFeastButtonPatch()
        {
            _document = new XmlDocument();
            _document.LoadXml(
                "<ButtonWidget Id=\"FeastsOfCalradiaFeastTab\" " +
                "DoNotPassEventsToChildren=\"true\" " +
                "WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" " +
                "SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" " +
                "PositionYOffset=\"6\" " +
                "Brush=\"Header.Tab.Center\" " +
                "Command.Click=\"SetSelectedCategory\" " +
                "CommandParameter.Click=\"4\" " +
                "IsSelected=\"@IsFeastSelected\" " +
                "UpdateChildrenStates=\"true\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                Brush=\"Clan.TabControl.Text\" " +
                "                Text=\"@HostFeastButtonText\" />" +
                "  </Children>" +
                "</ButtonWidget>");
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;
    }
}
