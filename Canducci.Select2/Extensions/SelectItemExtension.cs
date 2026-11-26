
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Canducci.Select2.Extensions
{
    /// <summary>
    /// Provides extension methods for converting collections and queries into SelectItem collections.
    /// </summary>
    public static class SelectItemExtension
    {
        #region IEnumerable

        /// <summary>
        /// Converts a collection into a list of SelectItem objects.
        /// No items are selected or disabled by default.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <returns>A list of SelectItem objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            return ToSelectItem(items, value, description, (IEnumerable<TId>)null, null);
        }

        /// <summary>
        /// Converts a collection into a list of SelectItem objects and selects an item by its identifier.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selectedValue">The identifier of the item to be selected.</param>
        /// <returns>A list of SelectItem objects with the matching item selected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, TId selectedValue)
        {
            return ToSelectItem(items, value, description, new[] { selectedValue }, null);
        }

        /// <summary>
        /// Converts a collection into a list of SelectItem objects using a selector to determine selected items.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selected">An expression that determines whether an item is selected.</param>
        /// <returns>A list of SelectItem objects with the selected states defined by the expression.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value, description or selected is null.</exception>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            return ToSelectItemCore(items, value, description, selected.Compile(), null);
        }

        /// <summary>
        /// Converts a collection into a list of SelectItem objects using selectors to determine selected and disabled items.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selected">An expression that determines whether an item is selected.</param>
        /// <param name="disabled">An expression that determines whether an item is disabled.</param>
        /// <returns>A list of SelectItem objects with the specified selected and disabled states.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value, description, selected or disabled is null.</exception>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected, Expression<Func<T, bool>> disabled)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            if (disabled == null)
            {
                throw new ArgumentNullException(nameof(disabled));
            }
            return ToSelectItemCore(items, value, description, selected.Compile(), disabled.Compile());
        }

        /// <summary>
        /// Converts a collection into a list of SelectItem objects, allowing multiple items to be selected or disabled.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selectedValues">The identifiers of the items that should be selected.</param>
        /// <param name="disabledValues">The identifiers of the items that should be disabled.</param>
        /// <returns>A list of SelectItem objects with the specified selection and disabled states.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues = null)
        {
            Func<T, TId> valueSelector = ValidateAndCompile(value, nameof(value));
            Func<T, string> descriptionSelector = ValidateAndCompile(description, nameof(description));
            HashSet<TId> selected = selectedValues == null ? new HashSet<TId>() : new HashSet<TId>(selectedValues);
            HashSet<TId> disabled = disabledValues == null ? new HashSet<TId>() : new HashSet<TId>(disabledValues);
            return ToSelectItemCore(items, valueSelector, descriptionSelector, item => selected.Contains(valueSelector(item)), item => disabled.Contains(valueSelector(item)));
        }

        #endregion

        #region IQueryable Async

        /// <summary>
        /// Converts an asynchronously enumerable query into a list of SelectItem objects.
        /// No items are selected or disabled by default.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source query.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <returns>A task containing a list of SelectItem objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not support asynchronous enumeration.</exception>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            return ToSelectItemAsync(items, value, description, (IEnumerable<TId>)null, null);
        }

        /// <summary>
        /// Converts an asynchronously enumerable query into a list of SelectItem objects and selects an item by its identifier.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source query.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selectedValue">The identifier of the item to be selected.</param>
        /// <returns>A task containing a list of SelectItem objects with the matching item selected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not support asynchronous enumeration.</exception>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, TId selectedValue)
        {
            return ToSelectItemAsync(items, value, description, new[] { selectedValue }, null);
        }

        /// <summary>
        /// Converts an asynchronously enumerable query into a list of SelectItem objects using a selector to determine selected items.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source query.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selected">An expression that determines whether an item is selected.</param>
        /// <returns>A task containing a list of SelectItem objects with the selected states defined by the expression.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value, description or selected is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not support asynchronous enumeration.</exception>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            return ToSelectItemAsyncCore(items, value, description, selected.Compile(), null);
        }

        /// <summary>
        /// Converts an asynchronously enumerable query into a list of SelectItem objects using selectors to determine selected and disabled items.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source query.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selected">An expression that determines whether an item is selected.</param>
        /// <param name="disabled">An expression that determines whether an item is disabled.</param>
        /// <returns>A task containing a list of SelectItem objects with the specified selected and disabled states.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value, description, selected or disabled is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not support asynchronous enumeration.</exception>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected, Expression<Func<T, bool>> disabled)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            if (disabled == null)
            {
                throw new ArgumentNullException(nameof(disabled));
            }
            return ToSelectItemAsyncCore(items, value, description, selected.Compile(), disabled.Compile());
        }

        /// <summary>
        /// Converts an asynchronously enumerable query into a list of SelectItem objects, allowing multiple items to be selected or disabled.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <typeparam name="TId">The type of the item identifier.</typeparam>
        /// <param name="items">The source query.</param>
        /// <param name="value">An expression that selects the identifier of each item.</param>
        /// <param name="description">An expression that selects the text displayed for each item.</param>
        /// <param name="selectedValues">The identifiers of the items that should be selected.</param>
        /// <param name="disabledValues">The identifiers of the items that should be disabled.</param>
        /// <returns>A task containing a list of SelectItem objects with the specified selection and disabled states.</returns>
        /// <exception cref="ArgumentNullException">Thrown when items, value or description is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the query does not support asynchronous enumeration.</exception>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues = null)
        {
            Func<T, TId> valueSelector = ValidateAndCompile(value, nameof(value));
            Func<T, string> descriptionSelector = ValidateAndCompile(description, nameof(description));
            HashSet<TId> selected = selectedValues == null ? new HashSet<TId>() : new HashSet<TId>(selectedValues);
            HashSet<TId> disabled = disabledValues == null ? new HashSet<TId>() : new HashSet<TId>(disabledValues);
            return ToSelectItemAsyncCore(items, valueSelector, descriptionSelector, item => selected.Contains(valueSelector(item)), item => disabled.Contains(valueSelector(item)));
        }

        #endregion

        #region Private Methods

        private static List<SelectItem<TId>> ToSelectItemCore<T, TId>(IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Func<T, bool> selected, Func<T, bool> disabled)
        {
            Func<T, TId> valueSelector = ValidateAndCompile(value, nameof(value));
            Func<T, string> descriptionSelector = ValidateAndCompile(description, nameof(description));
            return ToSelectItemCore(items, valueSelector, descriptionSelector, selected, disabled);
        }

        private static List<SelectItem<TId>> ToSelectItemCore<T, TId>(IEnumerable<T> items, Func<T, TId> valueSelector, Func<T, string> descriptionSelector, Func<T, bool> selected, Func<T, bool> disabled)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            var sources = items.Select(item =>
            {
                TId id = valueSelector(item);
                string name = descriptionSelector(item);
                bool isSelected = selected != null && selected(item);
                bool isDisabled = disabled != null && disabled(item);
                return new SelectItem<TId>(id, name, isSelected, isDisabled);
            });
            return sources.ToList();
        }

        private static async Task<List<SelectItem<TId>>> ToSelectItemAsyncCore<T, TId>(IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Func<T, bool> selected, Func<T, bool> disabled)
        {
            Func<T, TId> valueSelector = ValidateAndCompile(value, nameof(value));
            Func<T, string> descriptionSelector = ValidateAndCompile(description, nameof(description));
            return await ToSelectItemAsyncCore(items, valueSelector, descriptionSelector, selected, disabled);
        }

        private static async Task<List<SelectItem<TId>>> ToSelectItemAsyncCore<T, TId>(IQueryable<T> items, Func<T, TId> valueSelector, Func<T, string> descriptionSelector, Func<T, bool> selected, Func<T, bool> disabled)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            IAsyncEnumerable<T> asyncItems = items as IAsyncEnumerable<T>;
            if (asyncItems == null)
            {
                throw new InvalidOperationException("The query provider does not support asynchronous enumeration.");
            }
            List<SelectItem<TId>> result = new List<SelectItem<TId>>();
            await foreach (T item in asyncItems)
            {
                TId id = valueSelector(item);
                string name = descriptionSelector(item);
                bool isSelected = selected != null && selected(item);
                bool isDisabled = disabled != null && disabled(item);
                result.Add(new SelectItem<TId>(id, name, isSelected, isDisabled));
            }
            return result;
        }

        private static Func<T, TResult> ValidateAndCompile<T, TResult>(Expression<Func<T, TResult>> expression, string parameterName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return expression.Compile();
        }

        #endregion
    }
}
