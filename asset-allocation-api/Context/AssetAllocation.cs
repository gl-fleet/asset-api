using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class AssetAllocation
{
    public int Id { get; set; }

    public int? AssetId { get; set; }

    public int? PersonnelNo { get; set; }

    public int? AssignedUserId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public int? ReturnedUserId { get; set; }

    public DateTime? ReturnedDate { get; set; }

    public virtual Asset? Asset { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
