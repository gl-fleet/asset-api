namespace asset_allocation_api.Models
{
    public class DashboardListResult(IEnumerable<Dashboard> Dashboard)
    {
        public IEnumerable<Dashboard> Dashboard { get; set; } = Dashboard;
    }

    public class Dashboard
    {
        public DateTime Date { get; set; }
        public string? Shift { get; set; }
        public string? Name { get; set; }
        public int Count { get; set; }
    }
}