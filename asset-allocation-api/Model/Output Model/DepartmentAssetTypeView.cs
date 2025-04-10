using asset_allocation_api.Context;

namespace asset_allocation_api.Model.custom_model;

public partial class DepartmentAssetTypeView
{
    public int Id { get; set; }

    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public int? AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }

    public int? ModifiedUserId { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<AssetCheckTypeSetting> AssetCheckTypeSettings { get; set; } = new List<AssetCheckTypeSetting>();
}