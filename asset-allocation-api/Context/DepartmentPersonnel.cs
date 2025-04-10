using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class DepartmentPersonnel
{
    public int Id { get; set; }

    public int? DepartmentId { get; set; }

    public int? PersonnelNo { get; set; }

    public string? UserGroup { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
