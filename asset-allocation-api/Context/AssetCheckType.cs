using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace asset_allocation_api.Context;

public partial class AssetCheckType
{
    public int Id { get; set; }

    public string? CheckName { get; set; }

    public int? ModifiedUserId { get; set; }
    
    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<AssetCheckHistory> AssetCheckHistories { get; set; } = new List<AssetCheckHistory>();
    

    public virtual ICollection<AssetInspectionHistory> AssetInspectionHistories { get; set; } = new List<AssetInspectionHistory>();
}
