namespace asset_allocation_api.Model.Output_Model
{
    public class AssetInspectionResult
    {
        public string? InspectionType { get; set; }
        public int? InspectionTypeId { get; set; }
        public DateTime? LastInspectionDate { get; set; }    
        public DateTime? NextInspectionDate { get; set; }
    }
}
