using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace FeastsOfCalradia.UI
{
    // Injects the Feast panel into the Clan screen's "lower half" — the area where the
    // Members/Parties/Fiefs/Income panels live. Anchors on ClanIncome and appends as a sibling.
    //
    // Layout (3 columns):
    //   Left (180px): stage navigation list — three buttons (Venue/Guests/Provisions) with status icon.
    //   Center (~460px): three stacked sub-panels, each with its own IsVisible binding driven by the
    //                    selected stage. Phase C builds out Venue's content; D/E populate the others.
    //   Right (160px): placeholder for finance + action button (Phase F).
    [PrefabExtension(
        "ClanScreen",
        "descendant::ClanIncome")]
    internal sealed class ClanScreenFeastPanelPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;
        public override int Index => 0;

        private readonly XmlDocument _document;

        public ClanScreenFeastPanelPatch()
        {
            _document = new XmlDocument();
            _document.LoadXml(BuildPanelXml());
        }

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension() => _document;

        private static string BuildPanelXml()
        {
            return
                "<Widget Id=\"FeastsOfCalradiaFeastPanel\" " +
                "WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "IsVisible=\"@IsFeastSelected\">" +
                "  <Children>" +
                LeftColumnXml() +
                CenterColumnXml() +
                RightColumnXml() +
                "  </Children>" +
                "</Widget>";
        }

        // --- Left column: stage navigation ---

        private static string LeftColumnXml()
        {
            return
                "<Widget Id=\"FeastLeftColumn\" " +
                "        WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" " +
                "        SuggestedWidth=\"180\" " +
                "        HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" " +
                "        MarginLeft=\"10\" MarginTop=\"10\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\">" +
                "      <Children>" +
                StageButtonXml("FeastStageVenue", "SelectVenueStage", "@IsVenueStageSelected", "@VenueStageLabel", "@VenueStatusIcon") +
                StageButtonXml("FeastStageGuests", "SelectGuestsStage", "@IsGuestsStageSelected", "@GuestsStageLabel", "@GuestsStatusIcon") +
                StageButtonXml("FeastStageProvisions", "SelectProvisionsStage", "@IsProvisionsStageSelected", "@ProvisionsStageLabel", "@ProvisionsStatusIcon") +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        private static string StageButtonXml(string id, string clickCommand, string isSelectedBinding, string labelBinding, string statusBinding)
        {
            return
                "<ButtonWidget Id=\"" + id + "\" " +
                "              DoNotPassEventsToChildren=\"true\" " +
                "              WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" " +
                "              SuggestedHeight=\"50\" MarginBottom=\"6\" " +
                "              Command.Click=\"" + clickCommand + "\" " +
                "              IsSelected=\"" + isSelectedBinding + "\" " +
                "              UpdateChildrenStates=\"true\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" " +
                "                MarginLeft=\"12\" " +
                "                Text=\"" + labelBinding + "\" />" +
                "    <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" " +
                "                MarginRight=\"12\" " +
                "                Text=\"" + statusBinding + "\" />" +
                "  </Children>" +
                "</ButtonWidget>";
        }

        // --- Center column: stage-specific sub-panels ---

        private static string CenterColumnXml()
        {
            return
                "<Widget Id=\"FeastCenterColumn\" " +
                "        WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" " +
                "        SuggestedWidth=\"460\" " +
                "        HorizontalAlignment=\"Center\" VerticalAlignment=\"Top\" " +
                "        MarginTop=\"10\">" +
                "  <Children>" +
                VenueStagePanelXml() +
                GuestsStagePanelXml() +
                ProvisionsStagePanelXml() +
                "  </Children>" +
                "</Widget>";
        }

        // Phase C content for the Venue stage: fief display + 3-button scale picker.
        private static string VenueStagePanelXml()
        {
            return
                "<Widget Id=\"FeastVenueStagePanel\" " +
                "        WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "        IsVisible=\"@IsVenueStageSelected\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "               MarginTop=\"30\">" +
                "      <Children>" +
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                    HorizontalAlignment=\"Center\" " +
                "                    MarginBottom=\"24\" " +
                "                    Text=\"@VenueStageBodyText\" " +
                "                    Brush=\"Popup.Title.Text\" />" +
                "        <ListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                   HorizontalAlignment=\"Center\" " +
                "                   StackLayout.LayoutMethod=\"HorizontalLeftToRight\">" +
                "          <Children>" +
                ScaleButtonXml("FeastScaleModest", "SelectModestScale", "@IsModestScaleSelected", "@ModestScaleLabel") +
                ScaleButtonXml("FeastScaleGrand", "SelectGrandScale", "@IsGrandScaleSelected", "@GrandScaleLabel") +
                ScaleButtonXml("FeastScaleRoyal", "SelectRoyalScale", "@IsRoyalScaleSelected", "@RoyalScaleLabel") +
                "          </Children>" +
                "        </ListPanel>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        private static string ScaleButtonXml(string id, string clickCommand, string isSelectedBinding, string labelBinding)
        {
            return
                "<ButtonWidget Id=\"" + id + "\" " +
                "              DoNotPassEventsToChildren=\"true\" " +
                "              WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "              SuggestedWidth=\"110\" SuggestedHeight=\"40\" " +
                "              MarginLeft=\"8\" MarginRight=\"8\" " +
                "              Command.Click=\"" + clickCommand + "\" " +
                "              IsSelected=\"" + isSelectedBinding + "\" " +
                "              UpdateChildrenStates=\"true\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                Text=\"" + labelBinding + "\" />" +
                "  </Children>" +
                "</ButtonWidget>";
        }

        // Phase D content for the Guests stage: header text + scrollable list of kingdom lords.
        private static string GuestsStagePanelXml()
        {
            return
                "<Widget Id=\"FeastGuestsStagePanel\" " +
                "        WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "        IsVisible=\"@IsGuestsStageSelected\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "               VerticalAlignment=\"Top\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "               MarginTop=\"20\" MarginBottom=\"10\" MarginLeft=\"10\" MarginRight=\"10\">" +
                "      <Children>" +
                // Header
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                    HorizontalAlignment=\"Center\" " +
                "                    MarginBottom=\"16\" " +
                "                    Text=\"@GuestsStageBodyText\" " +
                "                    Brush=\"Popup.Title.Text\" />" +
                // Invitee list (no scroll for now — overflow is acceptable for v0; ScrollablePanel
                // requires a complex ClipRect/InnerPanel/Scrollbar sibling-widget setup we'll add later).
                "        <ListPanel DataSource=\"{Invitees}\" " +
                "                   WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                   StackLayout.LayoutMethod=\"VerticalBottomToTop\">" +
                "          <ItemTemplate>" +
                "            <ButtonWidget DoNotPassEventsToChildren=\"true\" " +
                "                          WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" " +
                "                          SuggestedHeight=\"36\" MarginBottom=\"4\" " +
                "                          Command.Click=\"ExecuteToggle\" " +
                "                          IsSelected=\"@IsInvited\" " +
                "                          UpdateChildrenStates=\"true\">" +
                "              <Children>" +
                "                <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                            HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" " +
                "                            MarginLeft=\"12\" " +
                "                            Text=\"@HeroName\" />" +
                "              </Children>" +
                "            </ButtonWidget>" +
                "          </ItemTemplate>" +
                "        </ListPanel>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        // Phase E content for the Provisions stage: header text + list of required-vs-current per
        // food item. Read-only; players deposit into the fief stash via the keep menu and the panel
        // reflects updated counts on next display.
        private static string ProvisionsStagePanelXml()
        {
            return
                "<Widget Id=\"FeastProvisionsStagePanel\" " +
                "        WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "        IsVisible=\"@IsProvisionsStageSelected\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "               VerticalAlignment=\"Top\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "               MarginTop=\"20\" MarginBottom=\"10\" MarginLeft=\"10\" MarginRight=\"10\">" +
                "      <Children>" +
                // Header
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                    HorizontalAlignment=\"Center\" " +
                "                    MarginBottom=\"16\" " +
                "                    Text=\"@ProvisionsStageBodyText\" " +
                "                    Brush=\"Popup.Title.Text\" />" +
                // Item list
                "        <ListPanel DataSource=\"{Provisions}\" " +
                "                   WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                   StackLayout.LayoutMethod=\"VerticalBottomToTop\">" +
                "          <ItemTemplate>" +
                "            <Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" " +
                "                    SuggestedHeight=\"32\" MarginBottom=\"4\">" +
                "              <Children>" +
                "                <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                            HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" " +
                "                            MarginLeft=\"12\" " +
                "                            Text=\"@DisplayText\" />" +
                "                <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                            HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" " +
                "                            MarginRight=\"12\" " +
                "                            Text=\"@StatusIcon\" />" +
                "              </Children>" +
                "            </Widget>" +
                "          </ItemTemplate>" +
                "        </ListPanel>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        // Generic stub panel used for stages still to be implemented.
        private static string StageStubPanelXml(string id, string isVisibleBinding, string text)
        {
            return
                "<Widget Id=\"" + id + "\" " +
                "        WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "        IsVisible=\"" + isVisibleBinding + "\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                MarginTop=\"60\" " +
                "                Text=\"" + EscapeXml(text) + "\" " +
                "                Brush=\"Popup.Title.Text\" />" +
                "  </Children>" +
                "</Widget>";
        }

        private static string EscapeXml(string s) => s.Replace("\n", "&#10;");

        // --- Right column ---

        // Phase F: right column now has cost / gold / Send Invitations button.
        private static string RightColumnXml()
        {
            return
                "<Widget Id=\"FeastRightColumn\" " +
                "        WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" " +
                "        SuggestedWidth=\"220\" " +
                "        HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" " +
                "        MarginRight=\"10\" MarginTop=\"10\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "               VerticalAlignment=\"Top\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "               MarginTop=\"20\">" +
                "      <Children>" +
                FinanceRowXml("@CostLabel", "@CostValue") +
                FinanceRowXml("@GoldLabel", "@GoldValue") +
                // Send invitations button
                "        <ButtonWidget Id=\"FeastSendInvitations\" " +
                "                      DoNotPassEventsToChildren=\"true\" " +
                "                      WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" " +
                "                      SuggestedHeight=\"50\" MarginTop=\"30\" " +
                "                      Command.Click=\"ExecuteSendInvitations\" " +
                "                      IsEnabled=\"@CanSendInvitations\" " +
                "                      UpdateChildrenStates=\"true\">" +
                "          <Children>" +
                "            <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                        HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                        Text=\"@SendInvitationsButtonText\" />" +
                "          </Children>" +
                "        </ButtonWidget>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        // Vertical stack inside each row: small label above, larger value below. Avoids horizontal
        // overflow when value strings are long (e.g. "4,199,577 d").
        private static string FinanceRowXml(string labelBinding, string valueBinding)
        {
            return
                "<ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "           StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "           MarginBottom=\"12\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                HorizontalAlignment=\"Left\" " +
                "                MarginLeft=\"6\" " +
                "                Text=\"" + labelBinding + "\" />" +
                "    <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                HorizontalAlignment=\"Left\" " +
                "                MarginLeft=\"6\" MarginTop=\"2\" " +
                "                Text=\"" + valueBinding + "\" " +
                "                Brush=\"Popup.Title.Text\" />" +
                "  </Children>" +
                "</ListPanel>";
        }
    }
}
