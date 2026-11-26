using System;
using System.Collections.Generic;
using System.Linq;
using Canducci.Select2;
using Canducci.Select2.Extensions;
using Canducci.Select2.Tests.Models;
using Xunit;

namespace Canducci.Select2.Tests.Extensions
{
    public class SelectItemEnumerableTests
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

        [Fact]
        public void ToSelectItem_WithoutSelection_ShouldMapAllItems()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name);

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
        public void ToSelectItem_WithSingleSelectedValue_ShouldSelectOnlyMatchingIdentifier()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, 2);

            Assert.False(result[0].Selected);
            Assert.True(result[1].Selected);
            Assert.False(result[2].Selected);
            Assert.False(result[3].Selected);
        }

        [Fact]
        public void ToSelectItem_WithSelectedExpression_ShouldUseExpressionResult()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, x => x.IsSelected);

            Assert.True(result[0].Selected);
            Assert.False(result[1].Selected);
            Assert.True(result[2].Selected);
            Assert.False(result[3].Selected);
            Assert.All(result, item => Assert.False(item.Disabled));
        }

        [Fact]
        public void ToSelectItem_WithSelectedAndDisabledExpressions_ShouldUseBothExpressions()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, x => x.IsSelected, x => x.IsDisabled);

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
        public void ToSelectItem_WithIdentifierCollections_ShouldSelectAndDisableMatchingItems()
        {
            int[] selectedIds = { 1, 3 };
            int[] disabledIds = { 2, 3 };

            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, selectedIds, disabledIds);

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
        public void ToSelectItem_WithNullIdentifierCollections_ShouldNotSelectOrDisableItems()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, null, null);

            Assert.All(result, item =>
            {
                Assert.False(item.Selected);
                Assert.False(item.Disabled);
            });
        }

        [Fact]
        public void ToSelectItem_WithOnlySelectedIdentifiers_ShouldNotDisableItems()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, new[] { 2 });

            Assert.True(result[1].Selected);
            Assert.All(result, item => Assert.False(item.Disabled));
        }

        [Fact]
        public void ToSelectItem_WithDuplicateIdentifiersInCollections_ShouldNotDuplicateResultItems()
        {
            List<SelectItem<int>> result = CreateUsers().ToSelectItem(x => x.Id, x => x.Name, new[] { 1, 1, 3 }, new[] { 2, 2 });

            Assert.Equal(4, result.Count);
            Assert.True(result[0].Selected);
            Assert.True(result[2].Selected);
            Assert.True(result[1].Disabled);
        }

        [Fact]
        public void ToSelectItem_WithEmptySource_ShouldReturnEmptyList()
        {
            List<SelectItem<int>> result = new List<TestUser>().ToSelectItem(x => x.Id, x => x.Name);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ToSelectItem_WithStringIdentifiers_ShouldUseStringEquality()
        {
            List<TestEntity> entities = new List<TestEntity>
            {
                new TestEntity { Id = "A", Name = "Alpha" },
                new TestEntity { Id = "a", Name = "Lowercase" }
            };

            List<SelectItem<string>> result = entities.ToSelectItem(x => x.Id, x => x.Name, new[] { "A" });

            Assert.True(result[0].Selected);
            Assert.False(result[1].Selected);
        }

        [Fact]
        public void ToSelectItem_ShouldMaterializeSourceImmediately()
        {
            int enumerationCount = 0;
            IEnumerable<TestUser> source = EnumerateUsers(() =>
            {
                enumerationCount++;
            });

            List<SelectItem<int>> result = source.ToSelectItem(x => x.Id, x => x.Name);

            Assert.Equal(1, enumerationCount);
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void ToSelectItem_WhenItemsIsNull_ShouldThrowArgumentNullException()
        {
            IEnumerable<TestUser> items = null;

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => items.ToSelectItem(x => x.Id, x => x.Name));

            Assert.Equal("items", exception.ParamName);
        }

        [Fact]
        public void ToSelectItem_WhenValueIsNull_ShouldThrowArgumentNullException()
        {
            ExpressionHolder<TestUser, int> holder = new ExpressionHolder<TestUser, int>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CreateUsers().ToSelectItem(holder.Value, x => x.Name));

            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void ToSelectItem_WhenDescriptionIsNull_ShouldThrowArgumentNullException()
        {
            ExpressionHolder<TestUser, string> holder = new ExpressionHolder<TestUser, string>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CreateUsers().ToSelectItem(x => x.Id, holder.Value));

            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void ToSelectItem_WhenSelectedExpressionIsNull_ShouldThrowArgumentNullException()
        {
            ExpressionHolder<TestUser, bool> holder = new ExpressionHolder<TestUser, bool>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CreateUsers().ToSelectItem(x => x.Id, x => x.Name, holder.Value));

            Assert.Equal("selected", exception.ParamName);
        }

        [Fact]
        public void ToSelectItem_WhenDisabledExpressionIsNull_ShouldThrowArgumentNullException()
        {
            ExpressionHolder<TestUser, bool> holder = new ExpressionHolder<TestUser, bool>();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CreateUsers().ToSelectItem(x => x.Id, x => x.Name, x => x.IsSelected, holder.Value));

            Assert.Equal("disabled", exception.ParamName);
        }

        [Fact]
        public void ToSelectItem_WhenSelectorThrows_ShouldPropagateException()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                CreateUsers().ToSelectItem(x => x.Id, x => ThrowDescription()));

            Assert.Equal("Description failure.", exception.Message);
        }

        [Fact]
        public void ToSelectItem_WithNullIdentifier_ShouldSupportNullValuesInSelectedSet()
        {
            List<TestEntity> entities = new List<TestEntity>
            {
                new TestEntity { Id = null, Name = "No identifier" },
                new TestEntity { Id = "A", Name = "Alpha" }
            };

            List<SelectItem<string>> result = entities.ToSelectItem(x => x.Id, x => x.Name, new string[] { null });

            Assert.True(result[0].Selected);
            Assert.False(result[1].Selected);
        }

        private static IEnumerable<TestUser> EnumerateUsers(Action onEnumeration)
        {
            foreach (TestUser user in CreateUsers())
            {
                onEnumeration();
                yield return user;
            }
        }

        private static string ThrowDescription()
        {
            throw new InvalidOperationException("Description failure.");
        }

        private sealed class ExpressionHolder<T, TResult>
        {
            public System.Linq.Expressions.Expression<Func<T, TResult>> Value { get; set; }
        }
    }
}