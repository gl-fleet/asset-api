namespace asset_allocation_api.Model.Input_Model
{
    public class AssetAllocationAssignInput
    {
        public int PersonnelNo { get; set; }
        public int PersonnelId { get; set; }
        public int AssetId { get; set; }
        public int AssignedUserId { get; set; }
    }
}