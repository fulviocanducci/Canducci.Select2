using System;
using System.Linq;
using Canducci.Select2.Binders;
using Canducci.Select2.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class Select2ExtensionTests
    {
        [Fact]
        public void AddSelect2_ShouldReturnSameServiceCollection()
        {
            IServiceCollection services = new ServiceCollection();

            IServiceCollection result = services.AddSelect2();

            Assert.Same(services, result);
        }

        [Fact]
        public void AddSelect2_ShouldRegisterSelectTermModelBinderProvider()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMvcCore();
            services.AddSelect2();

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                MvcOptions options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

                Assert.Contains(options.ModelBinderProviders, item => item is SelectTermModelBinderProvider);
            }
        }

        [Fact]
        public void AddSelect2_ShouldPreserveExistingModelBinderProviders()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMvcCore();
            services.AddSelect2();

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                MvcOptions options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

                Assert.True(options.ModelBinderProviders.Count > 1);
                Assert.Contains(options.ModelBinderProviders, item => item is SelectTermModelBinderProvider);
            }
        }

        [Fact]
        public void AddSelect2_BeforeAddMvcCore_ShouldRegisterSelectTermModelBinderProvider()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSelect2();
            services.AddMvcCore();

            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                MvcOptions options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

                Assert.Contains(options.ModelBinderProviders, item => item is SelectTermModelBinderProvider);
            }
        }

        [Fact]
        public void AddSelect2_WithNullServices_ShouldThrowArgumentNullException()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => Select2Extension.AddSelect2(null));

            Assert.Equal("services", exception.ParamName);
        }
    }
}