using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class AssetCheckHistory : AuditableEntity
{
    public int Id { get; set; }

    public int? AssetId { get; set; }

    public int? AssetCheckTypeId { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }

    public int? CheckedUserId { get; set; }

    public DateTime? CheckedDate { get; set; }

    public virtual Asset? Asset { get; set; }

    public virtual AssetCheckType? AssetCheckType { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
