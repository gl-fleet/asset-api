using asset_allocation_api.Controller;
using asset_allocation_api.Model.Output_Model;

namespace asset_allocation_api.Model;

public partial class AssetView
{
    public int Id { get; set; }

    public string? Rfid { get; set; }

    public string? Serial { get; set; }
    public string? Mac2 { get; set; }
    public string? Mac3 { get; set; }

    public int? AssetTypeId { get; set; }
    public string? TypeName { get; set; }
    public string? DepartmentId { get; set; }

    public string? Country { get; set; }

    public string? Description { get; set; }

    public DateTime? RegisteredDate { get; set; }

    public DateOnly? ManufacturedDate { get; set; }

    public DateOnly? StartedUsingDate { get; set; }
    public DateTime? LastUsingDate { get; set; }

    public DateOnly? ExpireDate { get; set; }

    public int? LastMaintenanceId { get; set; }
    public string? Status { get; set; }

    public int? LastAllocationId { get; set; }

    public int ModifiedUserId { get; set; }

    public string? ModifiedUserName { get; set; }

    public DateTime? ModifiedDate { get; set; }
    public IEnumerable<AssetInspectionResult>? AssetInspectionResult {get; set;}
}