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
                "        SuggestedWidth=\"240\" " +
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
            // No outer Popup.Frame — each stage panel provides its own framing (Frame1Brush) where
            // appropriate, mirroring vanilla ClanFiefs/ClanMembers which don't add an extra outer
            // frame around the active tab's content.
            return
                "<Widget Id=\"FeastCenterColumn\" " +
                "        WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" " +
                "        SuggestedWidth=\"720\" " +
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

        // Phase D content for the Guests stage. Replicates the canonical ClanFiefs.xml left-panel
        // pattern: a Frame1Brush BrushListPanel containing a sort-button header row (with the
        // decorative scroll_header sprite at right) and a horizontal ListPanel of
        // {ScrollablePanel, Standard.VerticalScrollbar}.
        //
        // Column widths sum to 640 (220+160+160+100). The ScrollablePanel uses CoverChildren+MinWidth
        // so the frame self-sizes to content, just like vanilla.
        private static string GuestsStagePanelXml()
        {
            return
                "<Widget Id=\"FeastGuestsStagePanel\" " +
                "        WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "        IsVisible=\"@IsGuestsStageSelected\">" +
                "  <Children>" +
                // Outer vertical stack: body text on top, frame fills remaining height below.
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "               StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "               MarginTop=\"15\" MarginBottom=\"15\" MarginLeft=\"10\" MarginRight=\"10\">" +
                "      <Children>" +
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" " +
                "                    HorizontalAlignment=\"Center\" " +
                "                    MarginBottom=\"10\" " +
                "                    Text=\"@GuestsStageBodyText\" " +
                "                    Brush=\"Popup.Title.Text\" />" +
                // Frame1Brush — vanilla's wood-and-brass frame. CoverChildren width sizes to the
                // header row + scrollbar; StretchToParent height fills below the body text.
                "        <BrushListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                        HorizontalAlignment=\"Center\" VerticalAlignment=\"Bottom\" " +
                "                        Brush=\"Frame1Brush\" " +
                "                        StackLayout.LayoutMethod=\"VerticalBottomToTop\">" +
                "          <Children>" +
                // Header row — RenderLate=true matches vanilla so headers draw on top of the list edge.
                "            <ListPanel Id=\"FeastSortButtons\" " +
                "                       WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                       RenderLate=\"true\" " +
                "                       StackLayout.LayoutMethod=\"HorizontalLeftToRight\">" +
                "              <Children>" +
                HeaderCellXml("Clan.Fiefs.Sort.1", 220, "Name") +
                HeaderCellXml("Clan.Fiefs.Sort.2", 160, "Relation") +
                HeaderCellXml("Clan.Fiefs.Sort.2", 160, "Possibility") +
                HeaderCellXml("Clan.Fiefs.Sort.3", 100, "Invite") +
                // Decorative scroll-header sprite — the brass clip-cap that anchors the top of the
                // vertical scrollbar visually. Copied verbatim from ClanFiefs.xml line 79.
                "                <Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "                        SuggestedWidth=\"20\" SuggestedHeight=\"44\" " +
                "                        Sprite=\"StdAssets\\scroll_header\" " +
                "                        ExtendRight=\"3\" ExtendTop=\"6\" ExtendLeft=\"3\" ExtendBottom=\"4\" " +
                "                        HorizontalAlignment=\"Right\" />" +
                "              </Children>" +
                "            </ListPanel>" +
                // Body: ScrollablePanel + Standard.VerticalScrollbar as siblings in a horizontal
                // ListPanel (default direction is horizontal — matches vanilla ClanFiefs line 84).
                "            <ListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\">" +
                "              <Children>" +
                "                <ScrollablePanel Id=\"InviteesScrollablePanel\" " +
                "                                 WidthSizePolicy=\"CoverChildren\" MinWidth=\"640\" " +
                "                                 HeightSizePolicy=\"StretchToParent\" " +
                "                                 MarginLeft=\"3\" MarginBottom=\"3\" " +
                "                                 AutoHideScrollBars=\"true\" " +
                "                                 ClipRect=\"InviteesRect\" " +
                "                                 InnerPanel=\"InviteesRect\\InviteesListPanel\" " +
                "                                 MouseScrollAxis=\"Vertical\" " +
                "                                 VerticalScrollbar=\"..\\InviteesScrollbar\\Scrollbar\">" +
                "                  <Children>" +
                "                    <Widget Id=\"InviteesRect\" " +
                "                            WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"StretchToParent\" " +
                "                            ClipContents=\"true\">" +
                "                      <Children>" +
                "                        <ListPanel Id=\"InviteesListPanel\" DataSource=\"{Invitees}\" " +
                "                                   WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                                   StackLayout.LayoutMethod=\"VerticalBottomToTop\">" +
                "                          <ItemTemplate>" +
                InviteeRowXml() +
                "                          </ItemTemplate>" +
                "                        </ListPanel>" +
                "                      </Children>" +
                "                    </Widget>" +
                "                  </Children>" +
                "                </ScrollablePanel>" +
                // Vanilla scrollbar prefab — brass channel + handle, properly themed.
                "                <Standard.VerticalScrollbar Id=\"InviteesScrollbar\" " +
                "                                            HeightSizePolicy=\"StretchToParent\" " +
                "                                            HorizontalAlignment=\"Right\" VerticalAlignment=\"Bottom\" " +
                "                                            MarginRight=\"2\" MarginLeft=\"2\" MarginBottom=\"3\" />" +
                "              </Children>" +
                "            </ListPanel>" +
                "          </Children>" +
                "        </BrushListPanel>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</Widget>";
        }

        // Per-row item template for the invitee list. Four fixed-width cells whose widths match the
        // header row (220+160+160+100=640). The outer ButtonWidget is the click target that toggles
        // IsInvited; the checkbox inside cell 4 is purely visual (DoNotAcceptEvents=true).
        private static string InviteeRowXml()
        {
            return
                "<ButtonWidget DoNotPassEventsToChildren=\"true\" " +
                "              WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "              SuggestedWidth=\"640\" SuggestedHeight=\"90\" " +
                "              MarginBottom=\"4\" " +
                "              Brush=\"Clan.Item.Tuple\" " +
                "              Command.Click=\"ExecuteToggle\" " +
                "              IsSelected=\"@IsInvited\" " +
                "              UpdateChildrenStates=\"true\">" +
                "  <Children>" +
                "    <ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "               StackLayout.LayoutMethod=\"HorizontalLeftToRight\">" +
                "      <Children>" +
                // Cell 1: Name (220 wide) — portrait on left, name+house stacked to its right
                "        <Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" SuggestedWidth=\"220\">" +
                "          <Children>" +
                "            <ImageIdentifierWidget DataSource=\"{ImageIdentifier}\" " +
                "                                   WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "                                   SuggestedWidth=\"100\" SuggestedHeight=\"70\" " +
                "                                   HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" " +
                "                                   MarginLeft=\"8\" " +
                "                                   AdditionalArgs=\"@AdditionalArgs\" ImageId=\"@Id\" TextureProviderName=\"@TextureProviderName\" />" +
                "            <ListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                       StackLayout.LayoutMethod=\"VerticalBottomToTop\" " +
                "                       HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" " +
                "                       MarginLeft=\"118\">" +
                "              <Children>" +
                "                <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                            Text=\"@HeroName\" />" +
                "                <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                            MarginTop=\"2\" " +
                "                            Brush=\"Clan.Leader.Text\" " +
                "                            Text=\"@HouseName\" />" +
                "              </Children>" +
                "            </ListPanel>" +
                "          </Children>" +
                "        </Widget>" +
                // Cell 2: Relation (160 wide) — centered text
                "        <Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" SuggestedWidth=\"160\">" +
                "          <Children>" +
                "            <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                        HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                        Brush.TextHorizontalAlignment=\"Center\" " +
                "                        Text=\"@RelationText\" />" +
                "          </Children>" +
                "        </Widget>" +
                // Cell 3: Possibility (160 wide) — placeholder until RSVP probability lands
                "        <Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" SuggestedWidth=\"160\">" +
                "          <Children>" +
                "            <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                        HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                        Brush.TextHorizontalAlignment=\"Center\" " +
                "                        Text=\"—\" />" +
                "          </Children>" +
                "        </Widget>" +
                // Cell 4: Invite (100 wide) — visual-only Toggle checkbox; row absorbs the click
                "        <Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" SuggestedWidth=\"100\">" +
                "          <Children>" +
                "            <ButtonWidget DoNotAcceptEvents=\"true\" " +
                "                          WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "                          SuggestedWidth=\"32\" SuggestedHeight=\"32\" " +
                "                          HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                          Brush=\"SPOptions.Checkbox.Empty.Button\" " +
                "                          ButtonType=\"Toggle\" " +
                "                          IsSelected=\"@IsInvited\" " +
                "                          ToggleIndicator=\"ToggleIndicator\" " +
                "                          UpdateChildrenStates=\"true\">" +
                "              <Children>" +
                "                <ImageWidget Id=\"ToggleIndicator\" " +
                "                             WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" " +
                "                             Brush=\"SPOptions.Checkbox.Full.Button\" />" +
                "              </Children>" +
                "            </ButtonWidget>" +
                "          </Children>" +
                "        </Widget>" +
                "      </Children>" +
                "    </ListPanel>" +
                "  </Children>" +
                "</ButtonWidget>";
        }

        private static string HeaderCellXml(string brush, int width, string label)
        {
            return
                "<ButtonWidget DoNotAcceptEvents=\"true\" " +
                "              WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" " +
                "              SuggestedWidth=\"" + width + "\" SuggestedHeight=\"44\" " +
                "              Brush=\"" + brush + "\" " +
                "              UpdateChildrenStates=\"true\">" +
                "  <Children>" +
                "    <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" " +
                "                HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" " +
                "                Brush=\"Clan.LeftPanel.Header.Text\" " +
                "                Text=\"" + label + "\" />" +
                "  </Children>" +
                "</ButtonWidget>";
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
                "        SuggestedWidth=\"240\" " +
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
