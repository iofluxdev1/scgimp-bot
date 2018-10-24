using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarCitizen.Gimp.Data;

namespace StarCitizen.Gimp.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfigurationRoot configuration = configBuilder.Build();
            
            services.AddDbContext<ScGimpContext>(options => options.UseSqlServer(configuration["ScGimpContext"], b => b.MigrationsAssembly("StarCitizen.Gimp.Web")));
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
            }

            string iisUrlRewriteConfig = "<rewrite><rules><rule name=\"Redirect requests to default azure websites domain\" stopProcessing=\"true\"><match url=\"(.*)\" ignoreCase=\"true\" /><conditions logicalGrouping=\"MatchAny\"><add input=\"{HTTP_HOST}\" pattern=\"^scgimp\\.azurewebsites\\.net$\" /></conditions><action type=\"Redirect\" url=\"https://scgimp.com/{R:0}\" appendQueryString=\"true\" redirectType=\"Permanent\" /></rule><rule name=\"Redirect www to root\" stopProcessing=\"true\"><match url=\"(.*)\" ignoreCase=\"true\" /><conditions logicalGrouping=\"MatchAll\"><add input=\"{HTTP_HOST}\" pattern=\"^www\\.(.+)$\" /></conditions><action type=\"Redirect\" url=\"https://{C:1}/{R:0}\" appendQueryString=\"true\" redirectType=\"Permanent\" /></rule></rules></rewrite>";

            using (TextReader iisUrlRewriteTextReader = new StringReader(iisUrlRewriteConfig))
            {
                RewriteOptions options = new RewriteOptions()
                    .AddRedirectToHttpsPermanent()
                    .AddIISUrlRewrite(iisUrlRewriteTextReader);

                app.UseRewriter(options);
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Auto migrate on app start.
            using (ScGimpContext context = new ScGimpContext())
            {
                context.Database.Migrate();
            }
        }
    }
}
