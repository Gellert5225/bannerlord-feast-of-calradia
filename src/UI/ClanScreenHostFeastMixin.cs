using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using FeastsOfCalradia.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace FeastsOfCalradia.UI
{
    // ViewModelMixin: extends the vanilla ClanManagementVM with properties our injected Feast tab and
    // panel data-bind to. handleDerived=true is critical — by default ViewModelMixin matches only the
    // exact ClanManagementVM type; the live runtime instance may be a derived/wrapped class.
    [ViewModelMixin(handleDerived: true)]
    public sealed class ClanScreenHostFeastMixin : BaseViewModelMixin<ClanManagementVM>
    {
        public enum FeastScale { None = 0, Modest = 1, Grand = 2, Royal = 3 }

        // The Clan screen is modal — only one instance open at a time — so a single static
        // "current mixin" reference is sufficient for our Harmony patch on SetSelectedCategory to
        // toggle our flag. Cleared in OnFinalize when the screen closes.
        private static ClanScreenHostFeastMixin _current;

        private bool _isFeastSelected;

        // Stage-selection state: which of the three stages — Venue/Guests/Provisions — is currently
        // shown in the center detail panel.
        private bool _isVenueStageSelected = true;
        private bool _isGuestsStageSelected;
        private bool _isProvisionsStageSelected;

        // Phase C — Venue stage selections.
        private FeastScale _selectedScale = FeastScale.None;

        // Phase D — Guests stage invitees.
        private MBBindingList<InviteeItemVM> _invitees;

        // Phase E — Provisions stage items. Rebuilt whenever the scale changes.
        private MBBindingList<ProvisionItemVM> _provisions;

        public ClanScreenHostFeastMixin(ClanManagementVM vm) : base(vm)
        {
            _current = this;
            BuildInviteesList();
            BuildProvisionsList();
        }

        private void BuildInviteesList()
        {
            _invitees = new MBBindingList<InviteeItemVM>();
            Kingdom kingdom = Hero.MainHero?.MapFaction as Kingdom;
            if (kingdom == null)
            {
                return;
            }
            // All non-player-clan kingdom heroes who are alive lords. Player's own clan is implicitly
            // present at the feast and not invited separately.
            foreach (Clan clan in kingdom.Clans)
            {
                if (clan == Clan.PlayerClan)
                {
                    continue;
                }
                foreach (Hero h in clan.AliveLords)
                {
                    if (h == null)
                    {
                        continue;
                    }
                    _invitees.Add(new InviteeItemVM(h, OnInviteToggled));
                }
            }
        }

        private void OnInviteToggled()
        {
            OnPropertyChanged("InvitedCount");
            OnPropertyChanged("GuestsStatusIcon");
            OnPropertyChanged("GuestsStageBodyText");
            OnPropertyChanged("CanSendInvitations");
        }

        // --- Phase E: Provisions ---

        // Hardcoded requirements per scale. VERIFY: item StringIds ("grain", "wine", "meat", "cheese")
        // are conventional Bannerlord item IDs but if the lookup returns null the count will be 0;
        // double-check via Settlement.Stash if grain shows 0/X despite real grain in your stash.
        private static List<(string id, string display, int qty)> RequirementsForScale(FeastScale scale)
        {
            switch (scale)
            {
                case FeastScale.Modest:
                    return new List<(string, string, int)>
                    {
                        ("grain", "Grain", 20),
                        ("wine", "Wine", 10),
                    };
                case FeastScale.Grand:
                    return new List<(string, string, int)>
                    {
                        ("grain", "Grain", 50),
                        ("wine", "Wine", 30),
                        ("meat", "Meat", 15),
                    };
                case FeastScale.Royal:
                    return new List<(string, string, int)>
                    {
                        ("grain", "Grain", 100),
                        ("wine", "Wine", 60),
                        ("meat", "Meat", 30),
                        ("cheese", "Cheese", 20),
                    };
                default:
                    return new List<(string, string, int)>();
            }
        }

        private void BuildProvisionsList()
        {
            if (_provisions == null)
            {
                _provisions = new MBBindingList<ProvisionItemVM>();
            }
            _provisions.Clear();

            Town fief = AutoSelectedFief();
            ItemRoster stash = fief?.Settlement?.Stash;

            foreach (var req in RequirementsForScale(_selectedScale))
            {
                int have = 0;
                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(req.id);
                if (item != null && stash != null)
                {
                    have = stash.GetItemNumber(item);
                }
                _provisions.Add(new ProvisionItemVM(req.display, req.qty, have));
            }
        }

        [DataSourceProperty]
        public MBBindingList<ProvisionItemVM> Provisions => _provisions;

        public override void OnFinalize()
        {
            if (_current == this)
            {
                _current = null;
            }
            base.OnFinalize();
        }

        [DataSourceProperty]
        public string HostFeastButtonText => "Feast";

        // Our pseudo-tab selection state. Drives the Feast panel's IsVisible binding and the Feast tab
        // button's IsSelected binding.
        [DataSourceProperty]
        public bool IsFeastSelected
        {
            get { return _isFeastSelected; }
            set
            {
                if (_isFeastSelected != value)
                {
                    _isFeastSelected = value;
                    OnPropertyChangedWithValue(value, "IsFeastSelected");
                    // Vanilla finance panel binds its IsVisible to this derived property; need to
                    // notify it when our flag flips so it shows on vanilla tabs and hides on Feast.
                    OnPropertyChanged("IsFinancePanelVisible");
                }
            }
        }

        // Derived: vanilla finance panel hides on Feast tab, shows everywhere else.
        [DataSourceProperty]
        public bool IsFinancePanelVisible => !_isFeastSelected;

        // --- Stage selection ---

        [DataSourceProperty]
        public bool IsVenueStageSelected
        {
            get { return _isVenueStageSelected; }
            set
            {
                if (_isVenueStageSelected != value)
                {
                    _isVenueStageSelected = value;
                    OnPropertyChangedWithValue(value, "IsVenueStageSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsGuestsStageSelected
        {
            get { return _isGuestsStageSelected; }
            set
            {
                if (_isGuestsStageSelected != value)
                {
                    _isGuestsStageSelected = value;
                    OnPropertyChangedWithValue(value, "IsGuestsStageSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsProvisionsStageSelected
        {
            get { return _isProvisionsStageSelected; }
            set
            {
                if (_isProvisionsStageSelected != value)
                {
                    _isProvisionsStageSelected = value;
                    OnPropertyChangedWithValue(value, "IsProvisionsStageSelected");
                }
            }
        }

        [DataSourceMethod]
        public void SelectVenueStage()
        {
            IsVenueStageSelected = true;
            IsGuestsStageSelected = false;
            IsProvisionsStageSelected = false;
        }

        [DataSourceMethod]
        public void SelectGuestsStage()
        {
            IsVenueStageSelected = false;
            IsGuestsStageSelected = true;
            IsProvisionsStageSelected = false;
        }

        [DataSourceMethod]
        public void SelectProvisionsStage()
        {
            IsVenueStageSelected = false;
            IsGuestsStageSelected = false;
            IsProvisionsStageSelected = true;
        }

        // --- Stage labels (static) ---

        [DataSourceProperty]
        public string VenueStageLabel => "Venue";

        [DataSourceProperty]
        public string GuestsStageLabel => "Guests";

        [DataSourceProperty]
        public string ProvisionsStageLabel => "Provisions";

        // --- Stage status icons ---
        // Phase C wires Venue's icon to actual configured state. Guests/Provisions remain placeholder
        // until D/E land.

        [DataSourceProperty]
        public string VenueStatusIcon
        {
            get
            {
                bool hasFief = AutoSelectedFief() != null;
                bool hasScale = _selectedScale != FeastScale.None;
                return (hasFief && hasScale) ? "OK" : "—";
            }
        }

        [DataSourceProperty]
        public string GuestsStatusIcon => InvitedCount > 0 ? "OK" : "—";

        [DataSourceProperty]
        public string ProvisionsStatusIcon
        {
            get
            {
                if (_provisions == null || _provisions.Count == 0)
                {
                    return "—";
                }
                foreach (ProvisionItemVM p in _provisions)
                {
                    if (!p.IsMet) return "—";
                }
                return "OK";
            }
        }

        [DataSourceProperty]
        public string ProvisionsStageBodyText
        {
            get
            {
                if (_selectedScale == FeastScale.None)
                {
                    return "Choose a feast scale (in the Venue stage) to see required provisions.";
                }
                Town fief = AutoSelectedFief();
                string fiefName = fief?.Settlement?.Name?.ToString() ?? "?";
                return "Required provisions for a " + _selectedScale + " feast at " + fiefName + ".\nDeposit items into the fief stash via the keep menu.";
            }
        }

        // --- Phase D: Guests stage ---

        [DataSourceProperty]
        public MBBindingList<InviteeItemVM> Invitees => _invitees;

        [DataSourceProperty]
        public int InvitedCount
        {
            get
            {
                if (_invitees == null) return 0;
                int count = 0;
                foreach (var item in _invitees)
                {
                    if (item.IsInvited) count++;
                }
                return count;
            }
        }

        [DataSourceProperty]
        public string GuestsStageBodyText
        {
            get
            {
                if (_invitees == null || _invitees.Count == 0)
                {
                    return "No kingdom-mates available to invite.\n(Are you in a kingdom? Are there other clans in it?)";
                }
                int cap = CalculateGuestCap();
                return "Invited: " + InvitedCount + " / " + cap + " (soft cap)\nClick names below to toggle invitations.";
            }
        }

        // Soft cap based on host fief type. Per design: castle 8, town 20, capital 30+.
        // VERIFY: "capital" detection deferred — town vs castle is sufficient for v0.
        private int CalculateGuestCap()
        {
            Town fief = AutoSelectedFief();
            if (fief == null) return 0;
            if (fief.IsCastle) return 8;
            if (fief.IsTown) return 20;
            return 0;
        }

        // --- Phase C: Venue stage body ---

        // Auto-selects the player clan's first fortification (town or castle) as the venue. Multi-fief
        // picker is a follow-up iteration.
        private static Town AutoSelectedFief()
        {
            Clan clan = Hero.MainHero?.Clan;
            if (clan == null)
            {
                return null;
            }
            var fiefs = clan.Fiefs;
            if (fiefs == null || fiefs.Count == 0)
            {
                return null;
            }
            return fiefs[0];
        }

        [DataSourceProperty]
        public string VenueStageBodyText
        {
            get
            {
                Town fief = AutoSelectedFief();
                if (fief == null)
                {
                    return "Your clan owns no fiefs.\nA feast must be hosted at a fief you own.";
                }
                int totalFiefs = Hero.MainHero.Clan.Fiefs.Count;
                string fiefSuffix = totalFiefs > 1
                    ? " (auto-selected; " + totalFiefs + " fiefs total — picker pending)"
                    : "";
                string scaleLabel = _selectedScale == FeastScale.None
                    ? "(choose a scale below)"
                    : _selectedScale.ToString();
                return "Host fief: " + fief.Settlement.Name + fiefSuffix + "\nScale: " + scaleLabel;
            }
        }

        // --- Phase C: Scale selection ---

        [DataSourceProperty]
        public bool IsModestScaleSelected => _selectedScale == FeastScale.Modest;

        [DataSourceProperty]
        public bool IsGrandScaleSelected => _selectedScale == FeastScale.Grand;

        [DataSourceProperty]
        public bool IsRoyalScaleSelected => _selectedScale == FeastScale.Royal;

        [DataSourceProperty]
        public string ModestScaleLabel => "Modest";

        [DataSourceProperty]
        public string GrandScaleLabel => "Grand";

        [DataSourceProperty]
        public string RoyalScaleLabel => "Royal";

        [DataSourceMethod]
        public void SelectModestScale() => SetScale(FeastScale.Modest);

        [DataSourceMethod]
        public void SelectGrandScale() => SetScale(FeastScale.Grand);

        [DataSourceMethod]
        public void SelectRoyalScale() => SetScale(FeastScale.Royal);

        private void SetScale(FeastScale scale)
        {
            if (_selectedScale == scale)
            {
                return;
            }
            _selectedScale = scale;
            OnPropertyChanged("IsModestScaleSelected");
            OnPropertyChanged("IsGrandScaleSelected");
            OnPropertyChanged("IsRoyalScaleSelected");
            OnPropertyChanged("VenueStatusIcon");
            OnPropertyChanged("VenueStageBodyText");

            // Provisions requirements depend on scale — rebuild the list and refresh the status/body.
            BuildProvisionsList();
            OnPropertyChanged("Provisions");
            OnPropertyChanged("ProvisionsStatusIcon");
            OnPropertyChanged("ProvisionsStageBodyText");

            // Right-column finance + action also depend on scale (cost) and provisions (CanSend).
            OnPropertyChanged("CostValue");
            OnPropertyChanged("CanSendInvitations");
        }

        // --- Phase F: Right column finance + action ---

        // Cost tiers per design (Part II): roughly 5k/20k/50k denars by scale.
        private int CalculateCost()
        {
            switch (_selectedScale)
            {
                case FeastScale.Modest: return 5000;
                case FeastScale.Grand: return 20000;
                case FeastScale.Royal: return 50000;
                default: return 0;
            }
        }

        [DataSourceProperty]
        public string CostLabel => "Cost";

        [DataSourceProperty]
        public string CostValue
        {
            get
            {
                int c = CalculateCost();
                return c > 0 ? c.ToString("N0") + " d" : "—";
            }
        }

        [DataSourceProperty]
        public string GoldLabel => "Your gold";

        [DataSourceProperty]
        public string GoldValue => (Hero.MainHero?.Gold ?? 0).ToString("N0") + " d";

        [DataSourceProperty]
        public string SendInvitationsButtonText => "Send invitations";

        [DataSourceProperty]
        public bool CanSendInvitations
        {
            get
            {
                bool venueOk = AutoSelectedFief() != null && _selectedScale != FeastScale.None;
                bool guestsOk = InvitedCount > 0;
                bool provisionsOk = AreAllProvisionsMet();
                bool canAfford = (Hero.MainHero?.Gold ?? 0) >= CalculateCost();
                return venueOk && guestsOk && provisionsOk && canAfford;
            }
        }

        private bool AreAllProvisionsMet()
        {
            if (_provisions == null || _provisions.Count == 0)
            {
                return false;
            }
            foreach (ProvisionItemVM p in _provisions)
            {
                if (!p.IsMet) return false;
            }
            return true;
        }

        [DataSourceMethod]
        public void ExecuteSendInvitations()
        {
            if (!CanSendInvitations)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[Feast] Cannot send invitations — not all requirements met."));
                return;
            }
            int invitedCount = InvitedCount;
            int cost = CalculateCost();
            string fiefName = AutoSelectedFief()?.Settlement?.Name?.ToString() ?? "?";
            InformationManager.DisplayMessage(new InformationMessage(
                "[Feast] Sent " + invitedCount + " invitation(s) for a " + _selectedScale +
                " feast at " + fiefName + " (cost: " + cost.ToString("N0") +
                " denars). [Phase G — feast progression — coming next.]"));
        }

        // Existing right-column summary kept for the stub during transitions; can remove once Phase G
        // makes the panel switch to the in-progress view.
        [DataSourceProperty]
        public string FeastSummaryText
        {
            get
            {
                FeastSystemBehavior behavior = Campaign.Current?.GetCampaignBehavior<FeastSystemBehavior>();
                return behavior != null
                    ? behavior.GetFeastSummary()
                    : "Feast system not loaded.";
            }
        }

        // Called from the Harmony patch on ClanManagementVM.SetSelectedCategory to toggle our flag
        // when the user clicks the Feast tab (or any other tab, which clears it).
        internal static void SetSelected(bool selected)
        {
            if (_current != null)
            {
                _current.IsFeastSelected = selected;
            }
        }
    }
}
