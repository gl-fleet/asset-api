using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class NonReturnableAssetPersonnelPermit : AuditableEntity
{
    public int Id { get; set; }

    public int? PersonnelNo { get; set; }

    public int? AssetTypeId { get; set; }

    public short? Enabled { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? PersonnelId { get; set; }

    public virtual NonReturnableAssetType? AssetType { get; set; }

    public virtual ICollection<NonReturnableAssetAllocation> NonReturnableAssetAllocations { get; set; } = new List<NonReturnableAssetAllocation>();
}
