using Microsoft.AspNetCore.HttpOverrides;

namespace freeDPPapi
{

    public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    // Configuration
    public IConfigurationRoot Config { get; set; }

    public IWebHostEnvironment _env;

         public Microsoft.AspNetCore.Http.IHttpContextAccessor myHttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
    {

      //  https://blog.ppedv.de/post/wie-funktioniert-die-asp-net-core-2-1-middleware-pipeline
     //   https://stackoverflow.com/questions/37918547/middleware-to-set-response-contenttype
            services.AddControllers();
        // CORS allowed for all:
        services.AddCors(options => {
            options.AddPolicy("myCorsPolicy", builder => builder
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed((host) => true)
            .AllowAnyHeader());
        });

        services.AddDistributedMemoryCache();


            // Attributes with null value are not output at all during JSON serialization
            // not tested yet, but should work with .net 6.0 and above, see https://stackoverflow.com/questions/70346092/how-to-ignore-null-values-in-json-serialization-in-net-6
            // services.AddControllers()
            //    .AddJsonOptions(options =>
            //    {
            //        options.JsonSerializerOptions.DefaultIgnoreCondition
            //            = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            //    });


            services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential 
            // cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            // requires using Microsoft.AspNetCore.Http;
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(3600); // session cleared after time without click 3600 -> one hour no click -> session cleared
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = "jubaCookie";
        });

            // for IP-adress, call only possible in controller probably because of pipeline
            services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        // [inject] dependency injection for read of Header 
        services.AddHttpContextAccessor();
            // alternative: services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>(); ?


        //services.AddHttpsRedirection(); needs to be implemented correctly 

    }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _env = env;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Build the configuration from appsettings.json

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Config = builder.Build();

            //  app.UseHttpsRedirection(); no, done below in middleware, because we need to read the mandant settings from database first
            app.UseStaticFiles(); // from wwwroot, should be shut off in production, but for testing we need it, and also for CSS and JS files in wwwroot, see below for CSS and JS files in wwwroot
                                  // test form: http://test.jubacon.net/getfile/abauapp.txt

            // implement own middleware to redirect to https if not already, but only if mandant settings require it
            // as early we can read metadata from server, we can check if redirect to https is needed, and if so, redirect to https://maindomain.com

            app.Use(async (context, next) => {
                // context.Response.Clear(); //   Current.Response.Clear();
                // context.Response.Headers.Add("Content-Type", "text/css"); - but no output then, because next.Invoke() is not called
                //await context.Response.WriteAsync("hello papa"); //- but no output then.
                // doesnt work: string lsSQLserverIP = Config.GetConnectionString("Global:SQLserverIP");

                // if (context.Request.Query.Keys.Contains("yourkey"))
                if ((AssistInclude.GetAbsoluteUri(myHttpContextAccessor, "scheme") != "https")||1==2) //myJuba.HttpContextAccessor
                {
                    // if not https, check if redirect to ssl is needed

                    string lsMandantenCode = AssistInclude.FuncGetAppsetting(_env, "Global:code");
                    string lsSQLserverIP = AssistInclude.FuncGetAppsetting(_env, "Global:SQLserverIP");  // not necessary, is in funcGetSqlConnection
                    string lsSQLconnection = AssistInclude.FuncGetAppsetting(_env, "Global:SQLconnection"); // not necessary, is in funcGetSqlConnection

                    await next.Invoke();

                }
                else
                {
                    await next.Invoke();
                }
            }); 



        app.UseCors("myCorsPolicy"); // MyAllowSpecificOrigins); // allow CORS see ConfigureServices, in _called_ Page!
            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


        }
}
}