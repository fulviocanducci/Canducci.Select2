using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Canducci.Select2;
using Canducci.Select2.Extensions;
using Canducci.Select2.Tests.Models;
using Microsoft.EntityFrameworkCore;
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
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectedValue_ShouldSelectMatchingItem()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, 3);

                Assert.False(result[0].Selected);
                Assert.False(result[1].Selected);
                Assert.True(result[2].Selected);
                Assert.False(result[3].Selected);
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndSelectionExpressions_ShouldUseBothExpressions()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, user => user.IsSelected, user => user.IsDisabled);

                Assert.True(result[0].Selected);
                Assert.False(result[0].Disabled);
                Assert.False(result[1].Selected);
                Assert.True(result[1].Disabled);
                Assert.True(result[2].Selected);
                Assert.True(result[2].Disabled);
                Assert.False(result[3].Selected);
                Assert.False(result[3].Disabled);
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndIdentifierCollections_ShouldSelectAndDisableMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, new[] { 1, 3 }, new[] { 2, 3 });

                Assert.True(result[0].Selected);
                Assert.False(result[0].Disabled);
                Assert.False(result[1].Selected);
                Assert.True(result[1].Disabled);
                Assert.True(result[2].Selected);
                Assert.True(result[2].Disabled);
                Assert.False(result[3].Selected);
                Assert.False(result[3].Disabled);
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
