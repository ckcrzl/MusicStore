using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using MusicStore.Models;

namespace MusicStore.Spa
{
    public class Startup
    {
        public void Configure(IBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions { FileSystem = new PhysicalFileSystem("public") });

            app.UseServices(services =>
            {
                services.AddMvc();
                services.AddEntityFramework()
                    .AddSqlServer()
                    .AddInMemoryStore();
                services.AddScoped<MusicStoreContext>();
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
        }
    }
}
