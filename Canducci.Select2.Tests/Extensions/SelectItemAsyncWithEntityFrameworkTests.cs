using Canducci.Select2;
using Canducci.Select2.Extensions;
using Canducci.Select2.Tests.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class SelectItemAsyncWithEntityFrameworkTests
    {
        private static TestDbContext CreateContext()
        {
            DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            TestDbContext context = new TestDbContext(options);
            context.Users.AddRange(
                new TestUser { Id = 1, Name = "Ana", IsSelected = true, IsDisabled = false },
                new TestUser { Id = 2, Name = "Bruno", IsSelected = false, IsDisabled = true },
                new TestUser { Id = 3, Name = "Carlos", IsSelected = true, IsDisabled = true },
                new TestUser { Id = 4, Name = "Daniela", IsSelected = false, IsDisabled = false });
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemory_ShouldMapAllItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name);

                Assert.Equal(4, result.Count);
                Assert.Equal(new[] { "Ana", "Bruno", "Carlos", "Daniela" }, result.Select(item => item.Text));
                Assert.All(result, item =>
                {
                    Assert.False(item.Selected);
                    Assert.False(item.Disabled);
                });
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectedExpression_ShouldUseExpressionResult()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected);

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.All(result, item => Assert.False(item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectedAndDisabledExpressions_ShouldUseBothExpressionResults()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndWhereQuery_ShouldReturnFilteredItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .Where(user => user.Id > 2)
                    .ToSelectItemAsync(user => user.Id, user => user.Name);

                Assert.Equal(2, result.Count);
                Assert.Equal(new[] { 3, 4 }, result.Select(item => item.Id));
                Assert.Equal(new[] { "Carlos", "Daniela" }, result.Select(item => item.Text));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectedIdentifiers_ShouldSelectMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<long>> result = await context.Users
                    .AsNoTracking()
                    .ToSelectItemAsync(user => (long)user.Id, user => user.Name, new long[] { 1, 3 });

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.All(result, item => Assert.False(item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectedAndDisabledIdentifiers_ShouldSetBothStates()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<long>> result = await context.Users
                    .AsNoTracking()
                    .ToSelectItemAsync(user => (long)user.Id, user => user.Name, new long[] { 1, 3 }, new long[] { 2, 3 });

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
            }
        }

        [Fact]
        public void ToSelectItem_WithEntityFrameworkInMemory_ShouldMapAllItemsSynchronously()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = context.Users
                    .AsNoTracking()
                    .ToSelectItem(user => user.Id, user => user.Name);

                Assert.Equal(4, result.Count);
                Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(item => item.Id));
                Assert.Equal(new[] { "Ana", "Bruno", "Carlos", "Daniela" }, result.Select(item => item.Text));
                Assert.All(result, item =>
                {
                    Assert.False(item.Selected);
                    Assert.False(item.Disabled);
                });
            }
        }

        [Fact]
        public void ToSelectItem_WithEntityFrameworkInMemoryAndSelectedAndDisabledExpressions_ShouldUseBothStatesSynchronously()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = context.Users
                    .Where(user => user.Id > 1)
                    .ToSelectItem(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

                Assert.Equal(new[] { 2, 3, 4 }, result.Select(item => item.Id));
                Assert.Equal(new[] { false, true, false }, result.Select(item => item.Selected));
                Assert.Equal(new[] { true, true, false }, result.Select(item => item.Disabled));
            }
        }

        [Fact]
        public void ToSelectItem_WithEntityFrameworkInMemoryAndSelectedIdentifiers_ShouldSelectMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<long>> result = context.Users
                    .AsNoTracking()
                    .ToSelectItem(user => (long)user.Id, user => user.Name, new long[] { 1, 3 });

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.All(result, item => Assert.False(item.Disabled));
            }
        }

        [Fact]
        public void ToSelectItem_WithEntityFrameworkInMemoryAndIdentifierCollections_ShouldApplyStatesInQuery()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<long>> result = context.Users
                    .ToSelectItem(user => (long)user.Id, user => user.Name, new long[] { 1, 3 }, new long[] { 2, 3 });

                Assert.Equal(new[] { true, false, true, false }, result.Select(item => item.Selected));
                Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
            }
        }

        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options)
            {
            }

            public DbSet<TestUser> Users { get; set; }
        }
    }
}
