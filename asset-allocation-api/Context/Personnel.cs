using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class Personnel
{
    public int PersonnelNo { get; set; }

    public int? PersonnelId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FullName { get; set; }

    public int? HotstampNo { get; set; }

    public string? CardNum { get; set; }

    public int? PersonnelSer { get; set; }

    public string? Email { get; set; }

    public string? DepartmentDesc { get; set; }

    public string? PositionDesc { get; set; }

    public string? CompanyDesc { get; set; }

    public string? ContactNumber { get; set; }

    public string? WorkPhone { get; set; }

    public int? EmploymentStatus { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
