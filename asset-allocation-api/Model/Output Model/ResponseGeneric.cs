namespace asset_allocation_api.Model.Output_Model
{
    public class ResponseGeneric
    {
        public object? Data { get; set; }
        public List<CaplampAssignData?> data { get; set; }
        public string Message { get; set; }
        public Status Status { get; set; }
        public Status status { get; set; }
    }

    public class Status
    {
        public bool Success { get; set; }
        public bool success { get; set; }
        public int Code { get; set; }
        public int code { get; set; }
    }
    
    public class CaplampAssignData
    {
        public string data { get; set; }
        public string message { get; set; }
    }
}