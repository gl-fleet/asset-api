using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class NonReturnableAssetField : AuditableEntity
{
    public int Id { get; set; }

    public int? AssetTypeId { get; set; }

    public string? FieldName { get; set; }

    public string? ValueType { get; set; }

    public virtual NonReturnableAssetType? AssetType { get; set; }

    public virtual ICollection<NonReturnableAssetPersonnelSetting> NonReturnableAssetPersonnelSettings { get; set; } = new List<NonReturnableAssetPersonnelSetting>();
}
