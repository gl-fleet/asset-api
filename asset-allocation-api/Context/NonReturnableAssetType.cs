using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class NonReturnableAssetType : AuditableEntity
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? IconName { get; set; }

    public int? Limit { get; set; }

    public int? CooldownDays { get; set; }

    public int? Stock { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? DepartmentId { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<NonReturnableAssetField> NonReturnableAssetFields { get; set; } = new List<NonReturnableAssetField>();

    public virtual ICollection<NonReturnableAssetPersonnelPermit> NonReturnableAssetPersonnelPermits { get; set; } = new List<NonReturnableAssetPersonnelPermit>();

    public virtual ICollection<TypeTraining> TypeTrainings { get; set; } = new List<TypeTraining>();
}
