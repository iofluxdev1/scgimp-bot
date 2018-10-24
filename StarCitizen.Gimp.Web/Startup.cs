using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarCitizen.Gimp.Data;

namespace StarCitizen.Gimp.Web
{
    public class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            HostingEnvironment = env;
            Configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{HostingEnvironment.EnvironmentName}.json", true, reloadOnChange: true)
                    .AddEnvironmentVariables();

            if (HostingEnvironment.IsDevelopment())
            {
                configBuilder.AddUserSecrets<Startup>();
            }

            ScGimpWebConfig config = new ScGimpWebConfig(configBuilder);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.OnAppendCookie = cookie => {

                    //if 
                    //(
                    //    !cookie.HasConsent && cookie.IsConsentNeeded &&
                    //    !cookie.CookieName.ToLowerInvariant().Contains(".aspnetcore.antiforgery") &&
                    //    !cookie.CookieName.ToLowerInvariant().Contains(".aspnet.consent")
                    //)
                    //{
                    //    cookie.IssueCookie = false;
                    //}

                    //System.Threading.Thread.Sleep(0);
                };
                options.OnDeleteCookie = cookie => {

                    //System.Threading.Thread.Sleep(0);
                };
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.HttpOnly = HttpOnlyPolicy.None;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMemoryCache();

            services.AddMvc
            (
                options =>
                {
                    options.CacheProfiles.Add
                    (
                        "Default",
                        new CacheProfile()
                        {
                            Duration = 86400,
                            Location = ResponseCacheLocation.Any,
                            NoStore = false
                        }
                    );
                    options.CacheProfiles.Add
                    (
                        "Never",
                        new CacheProfile()
                        {
                            Duration = null,
                            Location = ResponseCacheLocation.None,
                            NoStore = true
                        }
                    );
                }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<ScGimpContext>
            (
                options => options.UseSqlServer
                (
                    config.ScGimpContext,
                    ScGimpContext.GetSqlServerOptions
                )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();

                // https://theludditedeveloper.wordpress.com/2016/01/06/iis-url-rewrite-gotcha-2/
                string iisUrlRewriteConfig = "<rewrite><rules>" +
                    "<rule name=\"Redirect requests to default azure websites domain to custom domain\" stopProcessing=\"true\"><match url=\"(.*)\" ignoreCase=\"true\" /><conditions logicalGrouping=\"MatchAny\"><add input=\"{HTTP_HOST}\" pattern=\"^scgimp\\.azurewebsites\\.net$\" /></conditions><action type=\"Redirect\" url=\"https://scgimp.com/{R:0}\" appendQueryString=\"true\" redirectType=\"Permanent\" /></rule>" +
                    "<rule name=\"Redirect www to root\" stopProcessing=\"true\"><match url=\"(.*)\" ignoreCase=\"true\" /><conditions logicalGrouping=\"MatchAll\"><add input=\"{HTTP_HOST}\" pattern=\"^www\\.(.+)$\" /></conditions><action type=\"Redirect\" url=\"https://{C:1}/{R:0}\" appendQueryString=\"true\" redirectType=\"Permanent\" /></rule>" +
                    "</rules></rewrite>";

                using (TextReader iisUrlRewriteTextReader = new StringReader(iisUrlRewriteConfig))
                {
                    RewriteOptions options = new RewriteOptions()
                        .AddRedirectToHttpsPermanent()
                        .AddIISUrlRewrite(iisUrlRewriteTextReader);

                    app.UseRewriter(options);
                }
            }

            //app.UseHttpsRedirection(); // Already set it up above with the other rewrite options.
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Auto migrate on app start.
            //using (ScGimpContext context = new ScGimpContext())
            //{
            //    context.Database.Migrate();
            //}
        }
    }
}
