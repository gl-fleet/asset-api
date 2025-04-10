using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class AssetCheckTypeSetting
{
    public int Id { get; set; }

    public int? AssetTypeId { get; set; }

    public int? CheckTypeId { get; set; }

    public int? CheckPeriod { get; set; }

    public int? Expirelimit { get; set; }

    public string? CheckNear { get; set; }

    public string? ExpireNear { get; set; }

    public int? CreatedUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? Enabled { get; set; }

    public DateTime? StartDate { get; set; }

    public virtual AssetType? AssetType { get; set; }

    public virtual AssetCheckType? CheckType { get; set; }
}
