using asset_allocation_api.Config;

namespace asset_allocation_api.Models;

public class Response<T>
{
    public T? Data { get; set; }
    public Dictionary<string, string> Message { get; set; } = new Dictionary<string, string>() { { AssetAllocationConfig.ConstResultString, "Nothing worked yet!" } };
    public Status Status { get; set; } = new Status(102, false);
    public DateTime ExecuteStart { get; set; } = DateTime.Now;
    public DateTime? ExecuteEnd { get; set; } = null;
}

public class Status(int code, bool success)
{
    public int Code { get; set; } = code;
    public bool Success { get; set; } = success;
}