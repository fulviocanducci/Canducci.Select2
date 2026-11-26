using Canducci.Select2;
using Xunit;

namespace Canducci.Select2.Tests.Models
{
    public class SelectTermTests
    {
        [Fact]
        public void SelectTerm_ShouldImplementISelectTerm()
        {
            SelectTerm term = new SelectTerm();

            Assert.IsAssignableFrom<ISelectTerm>(term);
        }

        [Fact]
        public void SelectTerm_ShouldStoreAllProperties()
        {
            SelectTerm term = new SelectTerm
            {
                Term = "term",
                Q = "query",
                Type = "query_append",
                Page = 4
            };

            Assert.Equal("term", term.Term);
            Assert.Equal("query", term.Q);
            Assert.Equal("query_append", term.Type);
            Assert.Equal(4, term.Page);
        }

        [Fact]
        public void SelectTerm_ShouldUseDefaultPropertyValues()
        {
            SelectTerm term = new SelectTerm();

            Assert.Null(term.Term);
            Assert.Null(term.Q);
            Assert.Null(term.Type);
            Assert.Equal(0, term.Page);
        }
    }
}