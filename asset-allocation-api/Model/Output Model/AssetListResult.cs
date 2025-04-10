using asset_allocation_api.Model;

namespace asset_allocation_api.Models
{
    public class AssetListResult(int pageCount, int assetCount, IEnumerable<AssetView> asset)
    {
        public int PageCount { get; set; } = pageCount;
        public int AssetCount { get; set; } = assetCount;
        public IEnumerable<AssetView> Asset { get; set; } = asset;
    }
}