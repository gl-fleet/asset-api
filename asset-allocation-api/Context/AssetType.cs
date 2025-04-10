using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class AssetType : AuditableEntity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ReturnHour { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<AssetCheckTypeSetting> AssetCheckTypeSettings { get; set; } = new List<AssetCheckTypeSetting>();

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<DepartmentAssetType> DepartmentAssetTypes { get; set; } = new List<DepartmentAssetType>();

    public virtual ICollection<TypeTraining> TypeTrainings { get; set; } = new List<TypeTraining>();
}
