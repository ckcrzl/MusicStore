using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Security;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using MusicStore.Models;

namespace MusicStore.Spa
{
    public class Startup
    {
        private readonly IApplicationEnvironment _applicationEnvironment;

        public Startup(IApplicationEnvironment applicationEnvironment)
        {
            _applicationEnvironment = applicationEnvironment;
        }

        public void Configure(IBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions { FileSystem = new PhysicalFileSystem("wwwroot") });

            app.UseServices(services =>
            {
                services.AddInstance<IConfiguration>(new Configuration()
                    .AddJsonFile(Path.Combine(_applicationEnvironment.ApplicationBasePath, "LocalConfig.json"))
                    .AddEnvironmentVariables());
                
                services.AddMvc();
                services.AddEntityFramework()
                    .AddSqlServer()
                    .AddInMemoryStore();
                services.AddScoped<MusicStoreContext>();

                // Add all Identity related services to IoC. 
                // Using an InMemory store to store membership data until SQL server is available. 
                // Users created will be lost on application shutdown.
                services.AddTransient<DbContext, ApplicationDbContext>();

                services.AddIdentity<ApplicationUser, IdentityRole>(builder =>
                {
                    //s.UseDbContext(() => context);
                    //s.UseUserStore(() => new UserStore(context));
                    builder.AddEntity();
                    builder.AddUserManager<ApplicationUserManager>();
                    builder.AddRoleManager<ApplicationRoleManager>();
                });
                services.AddTransient<ApplicationSignInManager>();
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Notifications = new CookieAuthenticationNotifications
                {
                    //OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    //        validateInterval: TimeSpan.FromMinutes(30),
                    //        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseMvc(routes =>
            {
                // TODO: Move these back to attribute routes when they're available
                routes.MapRoute(null, "api/genres/menu", new { controller = "GenresApi", action = "GenreMenuList" });
                routes.MapRoute(null, "api/genres", new { controller = "GenresApi", action = "GenreList" });
                routes.MapRoute(null, "api/genres/{genreId}/albums", new { controller = "GenresApi", action = "GenreAlbums" });
                routes.MapRoute(null, "api/albums/mostPopular", new { controller = "AlbumsApi", action = "MostPopular" });
                routes.MapRoute(null, "api/albums/all", new { controller = "AlbumsApi", action = "All" });
                routes.MapRoute(null, "api/albums/{albumId}", new { controller = "AlbumsApi", action = "Details" });
                routes.MapRoute(null, "{controller}/{action}", new { controller = "Home", action = "Index" });
            });

            SampleData.InitializeMusicStoreDatabaseAsync(app.ApplicationServices).Wait();
            SampleData.InitializeIdentityDatabaseAsync(app.ApplicationServices).Wait();

            //Creates a Store manager user who can manage the store.
            CreateAdminUser(app.ApplicationServices).Wait();
        }

        private async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();
            var userName = configuration.Get("DefaultAdminUsername");
            var password = configuration.Get("DefaultAdminPassword");
            //const string adminRole = "Administrator";

            var userManager = serviceProvider.GetService<ApplicationUserManager>();
            // TODO: Identity SQL does not support roles yet
            //var roleManager = serviceProvider.GetService<ApplicationRoleManager>();
            //if (!await roleManager.RoleExistsAsync(adminRole))
            //{
            //    await roleManager.CreateAsync(new IdentityRole(adminRole));
            //}

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = userName };
                await userManager.CreateAsync(user, password);
                //await userManager.AddToRoleAsync(user, adminRole);
                await userManager.AddClaimAsync(user, new Claim("ManageStore", "Allowed"));
            }
        }
    }
}
