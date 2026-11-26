using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Canducci.Select2;
using Canducci.Select2.Binders;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Canducci.Select2.Tests.Binders
{
    public class SelectTermModelBinderTests
    {
        [Fact]
        public async Task BindModelAsync_WithAllQueryValues_ShouldBindSelectTerm()
        {
            ModelBindingContext context = CreateBindingContext(new Dictionary<string, StringValues>
            {
                ["term"] = "fulvio",
                ["q"] = "fulvio",
                ["_type"] = "query",
                ["page"] = "3"
            });

            SelectTermModelBinder binder = new SelectTermModelBinder();

            await binder.BindModelAsync(context);

            Assert.True(context.Result.IsModelSet);
            ISelectTerm result = Assert.IsAssignableFrom<ISelectTerm>(context.Result.Model);
            Assert.IsType<SelectTerm>(result);
            Assert.Equal("fulvio", result.Term);
            Assert.Equal("fulvio", result.Q);
            Assert.Equal("query", result.Type);
            Assert.Equal(3, result.Page);
        }

        [Fact]
        public async Task BindModelAsync_WithMissingValues_ShouldUseNullAndZeroDefaults()
        {
            ModelBindingContext context = CreateBindingContext(new Dictionary<string, StringValues>());

            await new SelectTermModelBinder().BindModelAsync(context);

            ISelectTerm result = Assert.IsAssignableFrom<ISelectTerm>(context.Result.Model);
            Assert.Null(result.Term);
            Assert.Null(result.Q);
            Assert.Null(result.Type);
            Assert.Equal(0, result.Page);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("1", 1)]
        [InlineData("25", 25)]
        [InlineData("-2", -2)]
        [InlineData("invalid", 0)]
        [InlineData("", 0)]
        [InlineData("2147483648", 0)]
        public async Task BindModelAsync_WithPageValue_ShouldParseOrDefault(string pageValue, int expectedPage)
        {
            ModelBindingContext context = CreateBindingContext(new Dictionary<string, StringValues>
            {
                ["page"] = pageValue
            });

            await new SelectTermModelBinder().BindModelAsync(context);

            ISelectTerm result = Assert.IsAssignableFrom<ISelectTerm>(context.Result.Model);
            Assert.Equal(expectedPage, result.Page);
        }

        [Fact]
        public async Task BindModelAsync_WithMultipleValues_ShouldUseFirstValue()
        {
            ModelBindingContext context = CreateBindingContext(new Dictionary<string, StringValues>
            {
                ["term"] = new StringValues(new[] { "first", "second" }),
                ["page"] = new StringValues(new[] { "2", "5" })
            });

            await new SelectTermModelBinder().BindModelAsync(context);

            ISelectTerm result = Assert.IsAssignableFrom<ISelectTerm>(context.Result.Model);
            Assert.Equal("first", result.Term);
            Assert.Equal(2, result.Page);
        }

        [Fact]
        public async Task BindModelAsync_WithNullContext_ShouldThrowArgumentNullException()
        {
            SelectTermModelBinder binder = new SelectTermModelBinder();

            await Assert.ThrowsAsync<System.ArgumentNullException>(() => binder.BindModelAsync(null));
        }

        private static ModelBindingContext CreateBindingContext(IDictionary<string, StringValues> values)
        {
            QueryCollection query = new QueryCollection(new Dictionary<string, StringValues>(values));
            QueryStringValueProvider valueProvider = new QueryStringValueProvider(BindingSource.Query, query, CultureInfo.InvariantCulture);
            EmptyModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(ISelectTerm));
            ActionContext actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

            return DefaultModelBindingContext.CreateBindingContext(actionContext, valueProvider, metadata, null, "filter");
        }
    }
}