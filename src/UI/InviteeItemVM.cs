using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace FeastsOfCalradia.UI
{
    // Per-hero VM used as the item type in the Guests-stage list. ItemTemplate bindings on the prefab
    // (@HeroName, @IsInvited, ExecuteToggle) resolve against properties/methods on this class.
    //
    // Notifies the parent mixin via the onToggled callback whenever IsInvited flips, so the mixin can
    // refresh derived properties like InvitedCount / GuestsStatusIcon / GuestsStageBodyText.
    public sealed class InviteeItemVM : ViewModel
    {
        private readonly Action _onToggled;
        private bool _isInvited;

        public InviteeItemVM(Hero hero, Action onToggled)
        {
            Hero = hero;
            _onToggled = onToggled;
        }

        public Hero Hero { get; }

        [DataSourceProperty]
        public string HeroName => Hero.Name?.ToString() ?? "?";

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
                }
            }
        }

        public void ExecuteToggle()
        {
            IsInvited = !IsInvited;
            _onToggled?.Invoke();
        }
    }
}
