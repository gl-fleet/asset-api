namespace asset_allocation_api.Model.Input_Model
{
    public class TycoAggregatorInput
    {
        public IMS IMS { get; set; }    
        public Tyco Tyco { get; set; }    

    }    
    
    public class ImsData {
        public string Email { get; set; }
        public string JobDesc { get; set; }
    }
    
    public class TycoData {
        public string Face { get; set; }
    }

}

