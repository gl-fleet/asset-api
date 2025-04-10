using System;
using System.Collections.Generic;
using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Context;

public partial class Configuration : AuditableEntity
{
    public int ConfigId { get; set; }

    public int? DepartmentId { get; set; }

    public string? Category { get; set; }

    public string? ConfigDesc { get; set; }

    public string? ConfigValue { get; set; }

    public int? IsEnabled { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Department? Department { get; set; }
}
