namespace freeDPPapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        // see https://riptutorial.com/asp-net-core/example/4816/middleware-to-set-response-contenttype
        // Middleware for response.contenttype
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                   webBuilder.UseContentRoot(Directory.GetCurrentDirectory()); 
                   //in IIS: web.config -> hostingModel=inprocess
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
