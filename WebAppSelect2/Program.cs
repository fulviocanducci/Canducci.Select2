using Canducci.Select2.Extensions;
using Microsoft.EntityFrameworkCore;
using Refit;
using WebAppSelect2.Models;

namespace WebAppSelect2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ReflectionSupport.EnsureRegistered();
            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .AddRefitGeneratedClient<Interfaces.IUserApi>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://api.apimocker.com");
                });
            var configuration = builder.Configuration;
            builder.Services.AddSelect2();
            builder.Services.AddDbContextPool<DataContext>(options =>
            {
                options
                    .UseSqlServer(configuration.GetConnectionString("DatabaseConnection"))
                    .LogTo(Console.Write);
            });
            builder.Services.AddControllersWithViews();
            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
            app.Run();

        }
    }
}
