using asset_allocation_api.Model;

namespace asset_allocation_api.Models
{
    public class AssetCheckHistoryListResult(int pageCount, IEnumerable<AssetCheckHistoryView> asset)
    {
        public int PageCount { get; set; } = pageCount;
        public IEnumerable<AssetCheckHistoryView> AssetCheckHistory { get; set; } = asset;
    }
}