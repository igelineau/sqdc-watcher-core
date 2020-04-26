namespace Models.EntityFramework
{
    public class ProductVariantMetaData
    {
        public bool WasFetched { get; set; }
        public bool HasBecomeInStock { get; set; }
        public bool HasBecomeOutOfStock { get; set; }
        public bool ShouldSendNotification { get; set; }
    }
}