using FeastsOfCalradia.UI;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

namespace FeastsOfCalradia.Patches
{
    // Harmony patch on ClanManagementVM.SetSelectedCategory that lets us hook the tab-selection logic
    // for our injected fifth Feast tab.
    //
    // Vanilla SetSelectedCategory(int) handles indices 0–3 (Members/Parties/Fiefs/Income); the else
    // branch clamps anything ≥ 3 to Income, so we MUST skip vanilla for our index 4.
    //
    // - For index 4 (Feast): clear all four vanilla Is*Selected flags so the existing panels and
    //   tab-button highlights deselect, then set our IsFeastSelected. Skip vanilla.
    // - For indices 0–3: clear our IsFeastSelected (vanilla just selected a different tab), then run
    //   vanilla normally.
    [HarmonyPatch(typeof(ClanManagementVM), "SetSelectedCategory")]
    internal static class ClanScreenSetSelectedCategoryPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ClanManagementVM __instance, int index)
        {
            if (index == 4)
            {
                // Vanilla panels (ClanMembers, ClanParties, etc.) hide based on their CHILD VM's
                // IsSelected, not the parent VM's Is*Selected (which are derived from the children at
                // line 235 of the ClanManagementVM decompile). Clear both for completeness.
                if (__instance.ClanMembers != null) __instance.ClanMembers.IsSelected = false;
                if (__instance.ClanParties != null) __instance.ClanParties.IsSelected = false;
                if (__instance.ClanFiefs != null) __instance.ClanFiefs.IsSelected = false;
                if (__instance.ClanIncome != null) __instance.ClanIncome.IsSelected = false;
                __instance.IsMembersSelected = false;
                __instance.IsPartiesSelected = false;
                __instance.IsFiefsSelected = false;
                __instance.IsIncomeSelected = false;
                ClanScreenHostFeastMixin.SetSelected(true);
                return false;
            }

            ClanScreenHostFeastMixin.SetSelected(false);
            return true;
        }
    }
}
