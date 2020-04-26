#region

using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Services
{
    /// <summary>
    ///     This policy's job is to determine whether a ProductVariant stock status change should be ignored or not. This is to avoid excessive notifications.
    ///     Only variants that we know just became in stock should be passed to the function. It does not check whether the InStock flag is set to true.
    /// </summary>
    public class BecameInStockTriggerPolicy
    {
        public bool ShouldIgnoreInStockChange(ProductVariant variant)
        {
            return false;
        }
    }
}