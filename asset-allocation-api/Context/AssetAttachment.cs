using System;
using System.Collections.Generic;

namespace asset_allocation_api.Context;

public partial class AssetAttachment
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public string? FilePath { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual Asset Asset { get; set; } = null!;
}
