using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class NonReturnableAssetPersonnelSetting : AuditableEntity
{
    public int Id { get; set; }

    public int? PersonnelNo { get; set; }

    public int? FieldId { get; set; }

    public string? Value { get; set; }

    public int? PersonnelId { get; set; }

    public virtual NonReturnableAssetField? Field { get; set; }
}
