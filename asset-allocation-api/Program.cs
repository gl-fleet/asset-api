namespace ot_api_asset_allocation;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            /*.ConfigureLogging(logging =>
            {
                logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
            })*/
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}