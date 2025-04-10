using asset_allocation_api.Model;

namespace asset_allocation_api.Models
{
    public class AssetAllocationListResult(int pageCount, IEnumerable<AssetAllocationView> asset)
    {
        public int PageCount { get; set; } = pageCount;
        public IEnumerable<AssetAllocationView> AssetAllocation { get; set; } = asset;
    }
}