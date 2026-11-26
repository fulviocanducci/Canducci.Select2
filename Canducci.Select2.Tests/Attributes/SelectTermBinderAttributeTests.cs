using Canducci.Select2.Attributes;
using Canducci.Select2.Binders;
using Xunit;

namespace Canducci.Select2.Tests.Attributes
{
    public class SelectTermBinderAttributeTests
    {
        [Fact]
        public void Constructor_ShouldConfigureSelectTermModelBinder()
        {
            SelectTermBinderAttribute attribute = new SelectTermBinderAttribute();

            Assert.Equal(typeof(SelectTermModelBinder), attribute.BinderType);
        }
    }
}