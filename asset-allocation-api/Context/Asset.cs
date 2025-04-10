using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class Asset : AuditableEntity
{
    public int Id { get; set; }

    public string? Rfid { get; set; }

    public string? Serial { get; set; }

    public string? Mac2 { get; set; }

    public string? Mac3 { get; set; }

    public int? AssetTypeId { get; set; }

    public int? DepartmentId { get; set; }

    public string? Country { get; set; }

    public DateTime? RegisteredDate { get; set; }

    public DateOnly? ManufacturedDate { get; set; }

    public DateOnly? StartedUsingDate { get; set; }

    public DateOnly? ExpireDate { get; set; }

    public int? LastMaintenanceId { get; set; }

    public int? LastAllocationId { get; set; }

    public int ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();

    public virtual ICollection<AssetAttachment> AssetAttachments { get; set; } = new List<AssetAttachment>();

    public virtual ICollection<AssetCheckHistory> AssetCheckHistories { get; set; } = new List<AssetCheckHistory>();

    public virtual ICollection<AssetInspectionHistory> AssetInspectionHistories { get; set; } = new List<AssetInspectionHistory>();

    public virtual AssetType? AssetType { get; set; }

    public virtual Department? Department { get; set; }

    public virtual AssetAllocation? LastAllocation { get; set; }

    public virtual AssetCheckHistory? LastMaintenance { get; set; }
}
