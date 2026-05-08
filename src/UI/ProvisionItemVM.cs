using TaleWorlds.Library;

namespace FeastsOfCalradia.UI
{
    // Per-provision-item VM used as the item type in the Provisions-stage list. Read-only display:
    // shows item name, required quantity, current quantity in the host fief's stash, plus a derived
    // "met" flag.
    public sealed class ProvisionItemVM : ViewModel
    {
        public ProvisionItemVM(string itemName, int requiredQuantity, int currentQuantity)
        {
            ItemName = itemName;
            RequiredQuantity = requiredQuantity;
            CurrentQuantity = currentQuantity;
        }

        public string ItemName { get; }
        public int RequiredQuantity { get; }
        public int CurrentQuantity { get; }

        public bool IsMet => CurrentQuantity >= RequiredQuantity;

        [DataSourceProperty]
        public string DisplayText => ItemName + ": " + CurrentQuantity + " / " + RequiredQuantity;

        [DataSourceProperty]
        public string StatusIcon => IsMet ? "OK" : "—";
    }
}
