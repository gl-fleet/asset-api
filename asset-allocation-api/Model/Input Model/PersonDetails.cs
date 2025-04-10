namespace asset_allocation_api.Model.Input_Model
{
    public class PersonDetails
    {
        public IMS? IMS { get; set; }
        public Tyco? Tyco { get; set; }
    }

    public class Tyco
    {
        public int? PersonnelID { get; set; }
        public string? CardNum { get; set; }
        public int? HotStampNum { get; set; }
        public int? PersonnelSer { get; set; }
        public string? Face { get; set; }
        public string? CardStatus { get; set; }
    }

    public class IMS
    {
        public string? FirstName { get; set; }
        public int? EmploymentStatus { get; set; }
        public int? PersonnelID { get; set; }
        public string? ContactNumber { get; set; }
        public int? JobCode { get; set; }
        public string? Company { get; set; }
        public string? JobDesc { get; set; }
        public string? Email { get; set; }
        public string? LastName { get; set; }
        public int? PersonnelNo { get; set; }
        public string? Department { get; set; }
        public string? Address { get; set; }
        public string? WorkPhone { get; set; }
    }

    public class UgAccessAllocation
    {
        public int? PersonnelId { get; set; }
        public int? SAP { get; set; }
        public int? HotstampNo { get; set; }
        public int? PersonnelSer { get; set; }
        public string?  DeviceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Face { get; set; }
        public string? CardNum { get; set; }
        public string? ShaftId { get; set; }
        public bool? Ims { get; set; }
        public bool? Pli { get; set; }
        public bool? TopVu { get; set; }
        public bool? Blacklist { get; set; }
        public string? TriggerSource { get; set; }
        public bool? AccessGranted { get; set; }
        public string? AccessGrantedNote { get; set; }
        public string? ImsNote { get; set; }
        public string? PliTag { get; set; }
        public string? PliNote { get; set; }
        public string? TopVuNote { get; set; }
        public string? BlacklistNote { get; set; }
        public Attendances? Attendances { get; set; }
    }

    public class Attendances
    {
        public bool? OtInduction { get; set; }
        public string? OtInductionEnd { get; set; }
        public string? OtInductionCode { get; set; }
        public string? OtInductionNote { get; set; }
        public bool? RcInduction { get; set; }
        public string? RcInductionEnd { get; set; }
        public string? RcInductionCode { get; set; }
        public string? RcInductionNote { get; set; }
        public bool? UgAlz { get; set; }
        public string? UgAlzEnd { get; set; }
        public string? UgAlzCode { get; set; }
        public string? UgAlzNote { get; set; }
    }
}