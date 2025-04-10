using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class AssetInspectionHistory
{
    public int Id { get; set; }

    public int? AssetId { get; set; }

    public int? CheckTypeId { get; set; }

    public int? CheckStatus { get; set; }

    public string? Description { get; set; }

    public DateTime? CheckedDateTime { get; set; }

    public int? CheckedUserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedUserId { get; set; }

    public virtual Asset? Asset { get; set; }

    public virtual AssetCheckType? CheckType { get; set; }
}
