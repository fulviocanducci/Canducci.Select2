using Canducci.Select2.Binders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace Canducci.Select2.Extensions
{
    public static class Select2Extension
    {
        /// <summary>
        /// Adds Select2 services and configures the required model binder.
        /// </summary>
        /// <remarks>
        /// This method must be called before AddControllersWithViews().
        /// </remarks>
        public static IServiceCollection AddSelect2(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<MvcOptions>(options =>
            {
                options.ModelBinderProviders.Add(new SelectTermModelBinderProvider());
            });

            return services;
        }
    }
}
