using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class TypeTraining : AuditableEntity
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public int? AssetTypeId { get; set; }

    public int TrainingId { get; set; }

    public int? NonReturnableAssetTypeId { get; set; }

    public virtual AssetType? AssetType { get; set; }

    public virtual NonReturnableAssetType? NonReturnableAssetType { get; set; }
}
