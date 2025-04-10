using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class Department : AuditableEntity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();

    public virtual ICollection<DepartmentAssetType> DepartmentAssetTypes { get; set; } = new List<DepartmentAssetType>();

    public virtual ICollection<NonReturnableAssetType> NonReturnableAssetTypes { get; set; } = new List<NonReturnableAssetType>();
}
