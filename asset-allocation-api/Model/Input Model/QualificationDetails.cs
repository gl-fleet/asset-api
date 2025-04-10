namespace asset_allocation_api.Model.Input_Model
{
    public class Attendance
    {
        public int qualification { get; set; }
        public bool valid { get; set; }
    }

    public class Data
    {
        public List<Personnel> personnel { get; set; }
        public string option { get; set; }
    }

    public class Personnel
    {
        public int sap { get; set; }
        public List<Attendance> attendances { get; set; }
    }

    public class QualificationDetails
    {
        public Data Data { get; set; }
        public string Message { get; set; }
        public Status Status { get; set; }
    }

    public class Status
    {
        public int Code { get; set; }
        public bool Success { get; set; }
    }
}
