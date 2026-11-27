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
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndOptionsObject_ShouldSelectAndDisableMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                IEnumerable<int> selectedValues = new[] { 1, 3 };
                IEnumerable<int> disabledValues = new[] { 2, 3 };
                SelectItemValues<int> options = new SelectItemValues<int>()
                    .SetSelectValues(selectedValues)
                    .SetDisabledValues(disabledValues);

                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, options);

                AssertSelectionAndDisabledValues(result);
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithEntityFrameworkInMemoryAndOptionsAction_ShouldSelectAndDisableMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                IEnumerable<int> selectedValues = new[] { 1, 3 };
                IEnumerable<int> disabledValues = new[] { 2, 3 };

                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(
                        user => user.Id,
                        user => user.Name,
                        options =>
                        {
                            options.SetSelectValues(selectedValues);
                            options.SetDisabledValues(disabledValues);
                        });

                AssertSelectionAndDisabledValues(result);
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsObjectAndOnlySelectedValues_ShouldSelectMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                SelectItemValues<int> options = new SelectItemValues<int>()
                    .SetSelectValues(new[] { 2, 4 });

                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, options);

                Assert.Equal(new[] { false, true, false, true }, result.Select(item => item.Selected));
                Assert.All(result, item => Assert.False(item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsObjectAndOnlyDisabledValues_ShouldDisableMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                SelectItemValues<int> options = new SelectItemValues<int>()
                    .SetDisabledValues(new[] { 1, 4 });

                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, options);

                Assert.All(result, item => Assert.False(item.Selected));
                Assert.Equal(new[] { true, false, false, true }, result.Select(item => item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsObjectAndNullCollections_ShouldLeaveAllItemsUnselectedAndEnabled()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, new SelectItemValues<int>());

                Assert.All(result, item =>
                {
                    Assert.False(item.Selected);
                    Assert.False(item.Disabled);
                });
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionAndOnlySelectedValues_ShouldSelectMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(
                        user => user.Id,
                        user => user.Name,
                        options => options.SetSelectValues(new[] { 1, 4 }));

                Assert.Equal(new[] { true, false, false, true }, result.Select(item => item.Selected));
                Assert.All(result, item => Assert.False(item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionAndOnlyDisabledValues_ShouldDisableMatchingItems()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(
                        user => user.Id,
                        user => user.Name,
                        options => options.SetDisabledValues(new[] { 2, 3 }));

                Assert.All(result, item => Assert.False(item.Selected));
                Assert.Equal(new[] { false, true, true, false }, result.Select(item => item.Disabled));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithOptionsActionWithoutConfiguration_ShouldLeaveAllItemsUnselectedAndEnabled()
        {
            using (TestDbContext context = CreateContext())
            {
                List<SelectItem<int>> result = await context.Users
                    .ToSelectItemAsync(user => user.Id, user => user.Name, options => { });

                Assert.All(result, item =>
                {
                    Assert.False(item.Selected);
                    Assert.False(item.Disabled);
                });
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullOptionsObject_ShouldThrowArgumentNullException()
        {
            using (TestDbContext context = CreateContext())
            {
                SelectItemValues<int> options = null;

                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    context.Users.ToSelectItemAsync(user => user.Id, user => user.Name, options));
            }
        }

        [Fact]
        public async Task ToSelectItemAsync_WithNullOptionsAction_ShouldThrowArgumentNullException()
        {
            using (TestDbContext context = CreateContext())
            {
                Action<SelectItemValues<int>> options = null;

                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    context.Users.ToSelectItemAsync(user => user.Id, user => user.Name, options));
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

        private static void AssertSelectionAndDisabledValues(IReadOnlyList<SelectItem<int>> result)
        {
            Assert.True(result[0].Selected);
            Assert.False(result[0].Disabled);
            Assert.False(result[1].Selected);
            Assert.True(result[1].Disabled);
            Assert.True(result[2].Selected);
            Assert.True(result[2].Disabled);
            Assert.False(result[3].Selected);
            Assert.False(result[3].Disabled);
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
