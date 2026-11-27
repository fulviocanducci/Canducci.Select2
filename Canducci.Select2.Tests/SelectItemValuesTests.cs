using Canducci.Select2;
using Xunit;

namespace Canducci.Select2.Tests
{
    public class SelectItemValuesTests
    {
        [Fact]
        public void Create_WithSelectedValues_ShouldSetSelectedValuesOnly()
        {
            int[] selectedValues = { 1, 2, 3 };

            SelectItemValues<int> result = SelectItemValues<int>.Create(selectedValues);

            Assert.Same(selectedValues, result.SelectedValues);
            Assert.Null(result.DisabledValues);
        }

        [Fact]
        public void Create_WithSelectedAndDisabledValues_ShouldSetBothCollections()
        {
            int[] selectedValues = { 1, 2, 3 };
            int[] disabledValues = { 4, 5 };

            SelectItemValues<int> result = SelectItemValues<int>.Create(selectedValues, disabledValues);

            Assert.Same(selectedValues, result.SelectedValues);
            Assert.Same(disabledValues, result.DisabledValues);
        }
    }
}
