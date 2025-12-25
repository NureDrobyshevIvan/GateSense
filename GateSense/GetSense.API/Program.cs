using GetSense.API.Configurations;

namespace GetSense.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT");
        var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
        
        if (!string.IsNullOrEmpty(port))
        {
            builder.WebHost.UseUrls($"http://+:{port}");
        }
        else if (string.IsNullOrEmpty(httpPorts))
        {
            builder.WebHost.UseUrls("http://+:8080");
        }
        
        builder.Configure();

        var app = builder.Build();

        await app.Configure();

        await app.RunAsync();
    }
}