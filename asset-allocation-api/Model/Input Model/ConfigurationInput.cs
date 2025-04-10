using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace asset_allocation_api.Model;

public partial class ConfigurationInput
{

    public int[] DepartmentId { get; set; } = [];

    public string? Category { get; set; }

    public string? ConfigDesc { get; set; }

    public string? ConfigValue { get; set; }

    public int? IsEnabled { get; set; }

    public int? ModifiedUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }
}
