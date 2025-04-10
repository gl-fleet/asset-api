namespace asset_allocation_api.Model.Input_Model
{
    public class AssetAllocationReturnInput
    {
        public int? AssetAllocationId { get; set; }
        public int? AssetId { get; set; }
        public int ReturnedUserId { get; set; }
    }
}