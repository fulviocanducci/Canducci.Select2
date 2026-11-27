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
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldUseExpressionResult()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected);

            Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
            Assert.All(result, item => Assert.False(item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldUseBothExpressionResults()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected, x => x.IsDisabled);

            Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullDisabledExpression_ShouldThrowArgumentNullException()
        {
            Expression<Func<TestUser, bool>> disabled = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected, disabled));

            Assert.Equal("disabled", exception.ParamName);
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

        [Fact]
        public async Task ToSelectItemAsync_ShouldProjectOnlyRequestedFields()
        {
            IQueryable<TestUser> query = CreateQuery();
            TestAsyncQueryProvider provider = (TestAsyncQueryProvider)query.Provider;

            await query.ToSelectItemAsync(x => x.Id, x => x.Name);

            MethodCallExpression select = Assert.IsAssignableFrom<MethodCallExpression>(provider.LastCreatedExpression);
            Assert.Equal(nameof(Queryable.Select), select.Method.Name);
            UnaryExpression quote = Assert.IsType<UnaryExpression>(select.Arguments[1]);
            LambdaExpression selector = Assert.IsAssignableFrom<LambdaExpression>(quote.Operand);
            NewExpression projection = Assert.IsType<NewExpression>(selector.Body);

            Assert.Equal(typeof(SelectItem<int>), projection.Type);
            Assert.Equal("Id", Assert.IsAssignableFrom<MemberExpression>(projection.Arguments[0]).Member.Name);
            Assert.Equal("Name", Assert.IsAssignableFrom<MemberExpression>(projection.Arguments[1]).Member.Name);
            Assert.Equal(ExpressionType.Constant, projection.Arguments[2].NodeType);
            Assert.Equal(ExpressionType.Constant, projection.Arguments[3].NodeType);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldIncludeItInProjection()
        {
            IQueryable<TestUser> query = CreateQuery();
            TestAsyncQueryProvider provider = (TestAsyncQueryProvider)query.Provider;

            await query.ToSelectItemAsync(x => x.Id, x => x.Name, x => x.IsSelected);

            MethodCallExpression select = Assert.IsAssignableFrom<MethodCallExpression>(provider.LastCreatedExpression);
            UnaryExpression quote = Assert.IsType<UnaryExpression>(select.Arguments[1]);
            LambdaExpression selector = Assert.IsAssignableFrom<LambdaExpression>(quote.Operand);
            NewExpression projection = Assert.IsType<NewExpression>(selector.Body);

            Assert.Equal("IsSelected", Assert.IsAssignableFrom<MemberExpression>(projection.Arguments[2]).Member.Name);
            Assert.Equal(ExpressionType.Constant, projection.Arguments[3].NodeType);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsAction_ShouldUseSelectedAndDisabledIdentifiers()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync<TestUser, int>(options =>
            {
                options.Value = x => x.Id;
                options.Description = x => x.Name;
                options.Selected = new[] { 1, 3 };
                options.Disabled = new[] { 2, 3 };
            });

            Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionAndNoOptionalCollections_ShouldSetAllStatesToFalse()
        {
            List<SelectItem<int>> result = await CreateQuery().ToSelectItemAsync<TestUser, int>(options =>
            {
                options.Value = x => x.Id;
                options.Description = x => x.Name;
            });

            Assert.All(result, item =>
            {
                Assert.False(item.Selected);
                Assert.False(item.Disabled);
            });
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionAndNullValue_ShouldThrowArgumentNullException()
        {
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync<TestUser, int>(options => options.Description = x => x.Name));

            Assert.Equal("Value", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionAndNullDescription_ShouldThrowArgumentNullException()
        {
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync<TestUser, int>(options => options.Value = x => x.Id));

            Assert.Equal("Description", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullOptionsAction_ShouldThrowArgumentNullException()
        {
            Action<SelectItemOptions<TestUser, int>> options = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => CreateQuery().ToSelectItemAsync(options));

            Assert.Equal("options", exception.ParamName);
        }

        private static string ThrowDescription()
        {
            throw new InvalidOperationException("Description failure.");
        }
    }
}
