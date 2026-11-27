using Canducci.Select2.Extensions;
using Canducci.Select2.Tests.Infrastructure;
using Canducci.Select2.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class SelectItemAsyncEdgeCasesTests
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
        public async Task ToSelectItemAsync_WithoutSelection_ShouldSupportComputedValueAndDescriptionExpressions()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Id * 10, user => user.Name.ToUpper());

            Assert.Equal(new[] { 10, 20, 30, 40 }, result.Select(item => item.Id));
            Assert.Equal(new[] { "ANA", "BRUNO", "CARLOS", "DANIELA" }, result.Select(item => item.Text));
            Assert.All(result, item =>
            {
                Assert.False(item.Selected);
                Assert.False(item.Disabled);
            });
        }

        [Fact]
        public async Task ToSelectItemAsync_WithoutSelection_ShouldSupportStringIdentifiers()
        {
            List<SelectItem<string>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Name, user => user.Name);

            Assert.Equal(new[] { "Ana", "Bruno", "Carlos", "Daniela" }, result.Select(item => item.Id));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldSupportComputedPredicate()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.Id % 2 == 0);

            Assert.Equal(new[] { false, true, false, true }, result.Select(item => item.Selected));
            Assert.All(result, item => Assert.False(item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldSupportComputedPredicates()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.Id > 2, user => user.Name.StartsWith("B"));

            Assert.Equal(new[] { false, false, true, true }, result.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, false, false }, result.Select(item => item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldReturnAllFalseWhenPredicateNeverMatches()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.Id < 0);

            Assert.All(result, item => Assert.False(item.Selected));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldReturnAllTrueWhenPredicatesAlwaysMatch()
        {
            List<SelectItem<int>> result = await CreateQuery()
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.Id > 0, user => user.Id > 0);

            Assert.All(result, item =>
            {
                Assert.True(item.Selected);
                Assert.True(item.Disabled);
            });
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullItems_ShouldThrowArgumentNullExceptionForSelectedOverload()
        {
            IQueryable<TestUser> items = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                items.ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected));

            Assert.Equal("items", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullItems_ShouldThrowArgumentNullExceptionForSelectedAndDisabledOverload()
        {
            IQueryable<TestUser> items = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                items.ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled));

            Assert.Equal("items", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullValue_ShouldThrowArgumentNullExceptionForSelectedOverload()
        {
            Expression<Func<TestUser, int>> value = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync(value, user => user.Name, user => user.IsSelected));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullValue_ShouldThrowArgumentNullExceptionForSelectedAndDisabledOverload()
        {
            Expression<Func<TestUser, int>> value = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync(value, user => user.Name, user => user.IsSelected, user => user.IsDisabled));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullDescription_ShouldThrowArgumentNullExceptionForSelectedOverload()
        {
            Expression<Func<TestUser, string>> description = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync(user => user.Id, description, user => user.IsSelected));

            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullDescription_ShouldThrowArgumentNullExceptionForSelectedAndDisabledOverload()
        {
            Expression<Func<TestUser, string>> description = null;

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                CreateQuery().ToSelectItemAsync(user => user.Id, description, user => user.IsSelected, user => user.IsDisabled));

            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEmptyQuery_ShouldReturnEmptyListForSelectedOverload()
        {
            List<SelectItem<int>> result = await new TestAsyncQueryable<TestUser>(new List<TestUser>())
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEmptyQuery_ShouldReturnEmptyListForSelectedAndDisabledOverload()
        {
            List<SelectItem<int>> result = await new TestAsyncQueryable<TestUser>(new List<TestUser>())
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ToSelectItem_WithSynchronousProvider_ShouldNotRequireAsyncEnumeration()
        {
            IQueryable<TestUser> query = CreateUsers().AsQueryable();

            List<SelectItem<int>> result = query.ToSelectItem(
                user => user.Id,
                user => user.Name,
                user => user.IsSelected,
                user => user.IsDisabled);

            Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(item => item.Id));
            Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
        }

        [Fact]
        public void ToSelectItem_ShouldUseOverloadMatchingDeclaredSourceType()
        {
            IQueryable<TestUser> querySource = CreateQuery();
            TestAsyncQueryProvider queryProvider = (TestAsyncQueryProvider)querySource.Provider;

            List<SelectItem<int>> queryResult = querySource.ToSelectItem(user => user.Id, user => user.Name);

            MethodCallExpression queryProjection = Assert.IsAssignableFrom<MethodCallExpression>(queryProvider.LastCreatedExpression);
            Assert.Equal(nameof(Queryable.Select), queryProjection.Method.Name);

            IQueryable<TestUser> enumerableSource = CreateQuery();
            TestAsyncQueryProvider enumerableProvider = (TestAsyncQueryProvider)enumerableSource.Provider;
            IEnumerable<TestUser> enumerableView = enumerableSource;

            List<SelectItem<int>> enumerableResult = enumerableView.ToSelectItem(user => user.Id, user => user.Name);

            Assert.Null(enumerableProvider.LastCreatedExpression);
            Assert.Equal(queryResult.Select(item => item.Id), enumerableResult.Select(item => item.Id));
            Assert.Equal(queryResult.Select(item => item.Text), enumerableResult.Select(item => item.Text));
        }

        [Fact]
        public void ToSelectItem_WithSynchronousProvider_ShouldSelectByIdentifierAndIdentifierCollections()
        {
            IQueryable<TestUser> query = CreateUsers().AsQueryable();

            List<SelectItem<int>> selectedById = query.ToSelectItem(user => user.Id, user => user.Name, 2);
            List<SelectItem<int>> selectedByCollections = query.ToSelectItem(
                user => user.Id,
                user => user.Name,
                new[] { 1, 3 },
                new[] { 2, 3 });

            Assert.Equal(new[] { false, true, false, false }, selectedById.Select(item => item.Selected));
            Assert.Equal(new[] { true, false, true, false }, selectedByCollections.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, true, false }, selectedByCollections.Select(item => item.Disabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSynchronousProvider_ShouldThrowForSelectedOverload()
        {
            IQueryable<TestUser> query = CreateUsers().AsQueryable();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                query.ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSynchronousProvider_ShouldThrowForSelectedAndDisabledOverload()
        {
            IQueryable<TestUser> query = CreateUsers().AsQueryable();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                query.ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldPropagateExceptionFromExpression()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateQuery().ToSelectItemAsync(user => user.Id, user => user.Name, user => ThrowPredicate()));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithDisabledExpression_ShouldPropagateExceptionFromExpression()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateQuery().ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => ThrowPredicate()));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldProjectBothPredicates()
        {
            IQueryable<TestUser> query = CreateQuery();
            TestAsyncQueryProvider provider = (TestAsyncQueryProvider)query.Provider;

            await query.ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

            MethodCallExpression select = Assert.IsAssignableFrom<MethodCallExpression>(provider.LastCreatedExpression);
            UnaryExpression quote = Assert.IsType<UnaryExpression>(select.Arguments[1]);
            LambdaExpression selector = Assert.IsAssignableFrom<LambdaExpression>(quote.Operand);
            NewExpression projection = Assert.IsType<NewExpression>(selector.Body);

            Assert.Equal(typeof(SelectItem<int>), projection.Type);
            Assert.Equal("IsSelected", Assert.IsAssignableFrom<MemberExpression>(projection.Arguments[2]).Member.Name);
            Assert.Equal("IsDisabled", Assert.IsAssignableFrom<MemberExpression>(projection.Arguments[3]).Member.Name);
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedExpression_ShouldApplyBeforeAsyncEnumeration()
        {
            IQueryable<TestUser> query = CreateQuery();
            List<SelectItem<int>> result = await query
                .Where(user => user.Id >= 2)
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected);

            Assert.Equal(new[] { 2, 3, 4 }, result.Select(item => item.Id));
            Assert.Equal(new[] { false, true, false }, result.Select(item => item.Selected));
        }

        [Fact]
        public async Task ToSelectItemAsync_WithSelectedAndDisabledExpressions_ShouldApplyBeforeAsyncEnumeration()
        {
            IQueryable<TestUser> query = CreateQuery();
            List<SelectItem<int>> result = await query
                .Where(user => user.Id <= 3)
                .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

            Assert.Equal(new[] { 1, 2, 3 }, result.Select(item => item.Id));
            Assert.Equal(new[] { true, false, true }, result.Select(item => item.Selected));
            Assert.Equal(new[] { false, true, true }, result.Select(item => item.Disabled));
        }

        private static bool ThrowPredicate()
        {
            throw new InvalidOperationException("Predicate failure.");
        }
    }
}
