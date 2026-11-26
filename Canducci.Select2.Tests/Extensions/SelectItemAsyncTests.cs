using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Canducci.Select2;
using Canducci.Select2.Extensions;
using Canducci.Select2.Tests.Infrastructure;
using Canducci.Select2.Tests.Models;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class SelectItemAsyncTests
    {
        private static List<TestUser> CreateUsers()
        {
            return new List<TestUser>
            {
                new TestUser { Id = 1, Name = "Ana", IsSelected = true, IsDisabled = false },
                new TestUser { Id = 2, Name = "Bruno", IsSelected = false, IsDisabled = true },
                new TestUser { Id = 3, Name = "Carlos", IsSelected = true, IsDisabled = true },
                new TestUser { Id = 4, Name = "Daniela", IsSelected = false, IsDisabled = false }
            };
        }

        private static IQueryable<TestUser> CreateQuery()
        {
            return new TestAsyncQueryable<TestUser>(CreateUsers());
        }

        [Fact]
        public async Task ToSelectItemAsync_WithoutSelection_ShouldMapAllItems()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name);

            Assert.Equal(4, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Ana", result[0].Text);
            Assert.All(result, item =>
            {
                Assert.False(item.Selected);
                Assert.False(item.Disabled);
            });
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSingleSelectedValue_ShouldSelectMatchingItem()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, 3);

            Assert.False(result[0].Selected);
            Assert.False(result[1].Selected);
            Assert.True(result[2].Selected);
            Assert.False(result[3].Selected);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldUseExpressionResult()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected);

            Assert.True(result[0].Selected);
            Assert.False(result[1].Selected);
            Assert.True(result[2].Selected);
            Assert.False(result[3].Selected);
            Assert.All(result, item => Assert.False(item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldUseBothExpressions()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected, x => x.IsDisabled);

            Assert.True(result[0].Selected);
            Assert.False(result[0].Disabled);
            Assert.False(result[1].Selected);
            Assert.True(result[1].Disabled);
            Assert.True(result[2].Selected);
            Assert.True(result[2].Disabled);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithIdentifierCollections_ShouldSelectAndDisableMatchingItems()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, new[] { 1, 3 }, new[] { 2, 3 });

            Assert.True(result[0].Selected);
            Assert.False(result[0].Disabled);
            Assert.False(result[1].Selected);
            Assert.True(result[1].Disabled);
            Assert.True(result[2].Selected);
            Assert.True(result[2].Disabled);
            Assert.False(result[3].Selected);
            Assert.False(result[3].Disabled);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullIdentifierCollections_ShouldNotSelectOrDisableItems()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, null, null);

            Assert.All(result, item =>
            {
                Assert.False(item.Selected);
                Assert.False(item.Disabled);
            });
        }

        [Fact]
        public async Task ToSelectItemAsync_WithWhereQuery_ShouldEnumerateFilteredResults()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .Where(x => x.Id > 2)
                .ToSelectItemAsync(x => x.Id, x => x.Name);

            Assert.Equal(2, result.Count);
            Assert.Equal(new[] { 3, 4 }, result.Select(x => x.Id));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEmptyQuery_ShouldReturnEmptyList()
        {
            List<SelectItem<int>> result = await new TestAsyncQueryable<TestUser>(new List<TestUser>())
                .ToSelectItemAsync(x => x.Id, x => x.Name);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullItems_ShouldThrowArgumentNullException()
        {
            IQueryable<TestUser> items = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => items.ToSelectItemAsync(x => x.Id, x => x.Name));

            Assert.Equal("items", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullValue_ShouldThrowArgumentNullException()
        {
            Expression<Func<TestUser, int>> value = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(value, x => x.Name));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullDescription_ShouldThrowArgumentNullException()
        {
            Expression<Func<TestUser, string>> description = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(x => x.Id, description));

            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullSelectedExpression_ShouldThrowArgumentNullException()
        {
            Expression<Func<TestUser, bool>> selected = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, selected));

            Assert.Equal("selected", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullDisabledExpression_ShouldThrowArgumentNullException()
        {
            Expression<Func<TestUser, bool>> disabled = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected, disabled));

            Assert.Equal("disabled", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSynchronousProvider_ShouldThrowInvalidOperationException()
        {
            IQueryable<TestUser> query = CreateUsers().AsQueryable();

            await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToSelectItemAsync(x => x.Id, x => x.Name));
        }

        [Fact]
        public async Task ToSelectItemAsync_WhenSelectorThrows_ShouldPropagateException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateQuery().ToSelectItemAsync(x => x.Id, x => ThrowDescription()));
        }

        private static string ThrowDescription()
        {
            throw new InvalidOperationException("Description failure.");
        }
    }
}