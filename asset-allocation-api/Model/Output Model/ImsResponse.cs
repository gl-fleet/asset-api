using asset_allocation_api.Model.Input_Model;

namespace asset_allocation_api.Model.Output_Model
{
    public class ImsResponse
    {
        public ImsResponseData Data { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
    }

    public class ImsResponseData
    {
        public UgAccessAllocation UgAccessAllocation { get; set; }
        public string Face { get; set; }
        public string Email { get; set; }
        public string JobDesc { get; set; }
    }
}

