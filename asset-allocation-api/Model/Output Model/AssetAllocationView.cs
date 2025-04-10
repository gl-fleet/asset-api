namespace asset_allocation_api.Model;

public partial class AssetAllocationView
{
    public int Id { get; set; }
    public int? AssetId { get; set; }
    public string? AssetRfid { get; set; }
    public string? AssetSerial { get; set; }
    public int? AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }
    public int? PersonnelNo { get; set; }
    public string? PersonnelFirstName { get; set; }
    public string? PersonnelLastName { get; set; }
    public int? AssignedUserId { get; set; }
    public string? AssignedUserFirstName { get; set; }
    public string? AssignedUserLastName { get; set; }
    public DateTime? AssignedDate { get; set; }
    public int? ReturnedUserId { get; set; }
    public string? ReturnedUserFirstName { get; set; }
    public string? ReturnedUserLastName { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Description { get; set; }
}