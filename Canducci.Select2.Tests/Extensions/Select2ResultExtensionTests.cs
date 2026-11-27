using System;
using System.Collections.Generic;
using System.Text.Json;
using Canducci.Select2.Extensions;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class Select2ResultExtensionTests
    {
        [Fact]
        public void ToSelect2_ShouldWrapResultsInSelect2ResponseShape()
        {
            List<SelectItem<int>> items = new List<SelectItem<int>>
            {
                new SelectItem<int>(1, "Ana"),
                new SelectItem<int>(2, "Bruno", selected: true)
            };

            Select2Result<int> result = items.ToSelect2();
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.Same(items, result.Results);
            Assert.Equal("{\"results\":[{\"id\":1,\"text\":\"Ana\",\"selected\":false,\"disabled\":false},{\"id\":2,\"text\":\"Bruno\",\"selected\":true,\"disabled\":false}]}", json);
        }

        [Fact]
        public void ToSelect2_WithNullResults_ShouldThrowArgumentNullException()
        {
            List<SelectItem<int>> results = null;

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => results.ToSelect2());

            Assert.Equal("results", exception.ParamName);
        }
    }
}
