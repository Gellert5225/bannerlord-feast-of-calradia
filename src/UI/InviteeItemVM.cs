using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace FeastsOfCalradia.UI
{
    // Per-hero VM used as the item type in the Guests-stage list. ItemTemplate bindings on the prefab
    // resolve against properties on this class.
    //
    // Notifies the parent mixin via the onToggled callback whenever IsInvited flips, so the mixin can
    // refresh derived properties like InvitedCount / GuestsStatusIcon / GuestsStageBodyText /
    // CanSendInvitations.
    public sealed class InviteeItemVM : ViewModel
    {
        private readonly Action _onToggled;

        // Wraps the hero in a vanilla HeroVM so we can reuse its ImageIdentifier — ImageIdentifierVM
        // itself is abstract, but HeroVM.ImageIdentifier exposes a concrete subclass internally.
        // VERIFY: HeroVM constructor signature in 1.3.15. If the second-arg overload doesn't exist,
        // try `new HeroVM(hero)` (single arg) — the bool flag varies by version.
        private readonly HeroVM _heroVM;

        private bool _isInvited;

        public InviteeItemVM(Hero hero, Action onToggled)
        {
            Hero = hero;
            _onToggled = onToggled;
            _heroVM = new HeroVM(hero, false);
        }

        public Hero Hero { get; }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier => _heroVM.ImageIdentifier;

        [DataSourceProperty]
        public string HeroName => Hero.Name?.ToString() ?? "?";

        // House / clan name shown as subtitle (matches sketch's "House Tarsus" subtitle).
        [DataSourceProperty]
        public string HouseName => Hero.Clan?.Name?.ToString() ?? "";

        // Relation between this hero and the player. Positive/negative shown via prefix only — color
        // coding deferred (Brush.FontColor binding from a property is uncertain in this Gauntlet ver).
        [DataSourceProperty]
        public string RelationText
        {
            get
            {
                int r = Hero.MainHero != null ? Hero.MainHero.GetRelation(Hero) : 0;
                return r > 0 ? "+" + r : r.ToString();
            }
        }

        [DataSourceProperty]
        public bool IsInvited
        {
            get { return _isInvited; }
            set
            {
                if (_isInvited != value)
                {
                    _isInvited = value;
                    OnPropertyChangedWithValue(value, "IsInvited");
                    OnPropertyChanged("IsNotInvited");
                }
            }
        }

        // Inverse of IsInvited — Gauntlet's IsVisible binding doesn't support negation, so we expose
        // both flags and bind two stacked widgets (one for each state).
        [DataSourceProperty]
        public bool IsNotInvited => !_isInvited;

        public void ExecuteToggle()
        {
            IsInvited = !IsInvited;
            _onToggled?.Invoke();
        }
    }
}
