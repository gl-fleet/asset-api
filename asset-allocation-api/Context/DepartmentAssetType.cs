using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class DepartmentAssetType
{
    public int Id { get; set; }

    public int? DepartmentId { get; set; }

    public int? AssetTypeId { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AssetType? AssetType { get; set; }

    public virtual Department? Department { get; set; }
}
