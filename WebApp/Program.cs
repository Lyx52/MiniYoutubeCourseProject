namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        host.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStaticWebAssets();
                webBuilder.UseStartup<Startup>();
            });
}