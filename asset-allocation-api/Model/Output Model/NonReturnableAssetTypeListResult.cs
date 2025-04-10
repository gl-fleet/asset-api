using asset_allocation_api.Context;
using asset_allocation_api.Model;

namespace asset_allocation_api.Models
{
    public class NonReturnableAssetTypeListResult(int pageCount, IEnumerable<NonReturnableAssetType> asset)
    {
        public int PageCount { get; set; } = pageCount;
        public IEnumerable<NonReturnableAssetType> AssetAllocation { get; set; } = asset;
    }
}