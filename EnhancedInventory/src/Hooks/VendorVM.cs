using HarmonyLib;
using Kingmaker.UI.MVVM._VM.Vendor;

namespace EnhancedInventory.Hooks
{
    // For some reason the vendor slots group changed notification is not dispatched, it is handled immediately, so we dispatch it here.
    // This is safe to do whether we're enabled or not - if we're disabled, the notification will simply fall upon deaf ears.
    [HarmonyPatch(typeof(VendorVM), nameof(VendorVM.UpdateVendorSide))]
    public static class VendorVM_UpdateVendorSide
    {
        [HarmonyPostfix]
        public static void Postfix(VendorVM __instance)
        {
            __instance.VendorSlotsGroup.CollectionChangedCommand.Execute(false);
        }
    }
}
