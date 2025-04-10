namespace asset_allocation_api.Model.CustomModel;

public class AssignmentSetting
{
    public string SystemIdentityName { get; set; }
    public string BaseURL { get; set; }
    public bool MustSuccess { get; set; }
    public bool Assignable { get; set; }
    public bool Unassignable { get; set; }
    public string UnAssignMethod { get; set; }
    public string AssignMethod { get; set; }

}