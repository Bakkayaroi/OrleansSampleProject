using Orleans.Configuration;

namespace Server
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.UseLocalhostClustering();
                    siloBuilder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "OrleansGame";
                    });
                    siloBuilder.AddMemoryGrainStorage("Default");
                    siloBuilder.ConfigureLogging(logging => logging.AddConsole());
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await host.RunAsync();
        }
    }
}