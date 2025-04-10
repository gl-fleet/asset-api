namespace asset_allocation_api.Model.Input_Model
{
    public class MinlogAssignInput
    {
        public MinlogCheckWifiData Data { get; set; }
        public string Message { get; set; }
        public Status Status { get; set; }
    }
    
    public class MinlogErrorData
    {
        public string Data { get; set; }
        public string Message { get; set; }
        public Status Status { get; set; }
    }

    public class MinlogCheckWifiData {
        public string rfid { get; set; }
        public string result { get; set; }
        public string wifiId { get; set; }
        public string rsuId { get; set; }
        public string detectionTime { get; set; }
        public string responseTime { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
}