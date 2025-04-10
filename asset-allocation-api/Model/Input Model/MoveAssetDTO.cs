namespace asset_allocation_api.Model.Input_Model;

public class MoveAssetDTO
{
    public int[] AssetIDs { get; set; } = [];
    public int ToDepartmentID { get; set; }
    public int ToAssetTypeID { get; set; }
    public int ModifiedUserId { get; set; }

}