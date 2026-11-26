using Canducci.Select2;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class SelectItemConstructorTests
    {
        [Fact]
        public void Constructor_ShouldAssignAllProperties()
        {
            SelectItem<int> item = new SelectItem<int>(10, "Fulvio", true, true);

            Assert.Equal(10, item.Id);
            Assert.Equal("Fulvio", item.Text);
            Assert.True(item.Selected);
            Assert.True(item.Disabled);
        }

        [Fact]
        public void Constructor_ShouldUseFalseAsDefaultForSelectedAndDisabled()
        {
            SelectItem<int> item = new SelectItem<int>(10, "Fulvio");

            Assert.False(item.Selected);
            Assert.False(item.Disabled);
        }

        [Fact]
        public void Constructor_ShouldAllowNullText()
        {
            SelectItem<int> item = new SelectItem<int>(10, null);

            Assert.Null(item.Text);
        }

        [Fact]
        public void Constructor_ShouldSupportNullableIdentifier()
        {
            SelectItem<int?> item = new SelectItem<int?>(null, "Without identifier");

            Assert.Null(item.Id);
        }

        [Fact]
        public void Constructor_ShouldSupportStringIdentifier()
        {
            SelectItem<string> item = new SelectItem<string>("user-1", "Fulvio");

            Assert.Equal("user-1", item.Id);
        }
    }
}