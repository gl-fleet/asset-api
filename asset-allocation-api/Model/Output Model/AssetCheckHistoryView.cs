namespace asset_allocation_api.Model;

public partial class AssetCheckHistoryView
{
    public int Id { get; set; }
    public int? AssetId { get; set; }
    public string? AssetRfid { get; set; }
    public string? AssetSerial { get; set; }
    public int? AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }
    public int? AssetCheckTypeId { get; set; }
    public string? AssetCheckTypeName { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public int? CheckedUserId { get; set; }
    public string? CheckedUserFirstName { get; set; }
    public string? CheckedUserLastName { get; set; }
    public DateTime? CheckedDate { get; set; }
}