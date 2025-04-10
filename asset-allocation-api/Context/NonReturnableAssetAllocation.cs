using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class NonReturnableAssetAllocation
{
    public int Id { get; set; }

    public int? PermitId { get; set; }

    public int? AssignedUserId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public virtual NonReturnableAssetPersonnelPermit? Permit { get; set; }
}
