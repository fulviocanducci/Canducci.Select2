using Canducci.Select2.Utils;
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

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            return ToSelectItem(items, value, description, (IEnumerable<TId>)null, null);
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, TId selectedValue)
        {
            return ToSelectItem(items, value, description, new[] { selectedValue }, null);
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            Func<T, bool> selectedSelector = SelectItemUtility.ValidateAndCompile(selected, nameof(selected));
            return SelectItemUtility.ToSelectItemCore(
                items,
                SelectItemUtility.ValidateAndCompile(value, nameof(value)),
                SelectItemUtility.ValidateAndCompile(description, nameof(description)),
                (item, id) => selectedSelector(item),
                null);
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected, Expression<Func<T, bool>> disabled)
        {
            Func<T, bool> selectedSelector = SelectItemUtility.ValidateAndCompile(selected, nameof(selected));
            Func<T, bool> disabledSelector = SelectItemUtility.ValidateAndCompile(disabled, nameof(disabled));
            return SelectItemUtility.ToSelectItemCore(
                items,
                SelectItemUtility.ValidateAndCompile(value, nameof(value)),
                SelectItemUtility.ValidateAndCompile(description, nameof(description)),
                (item, id) => selectedSelector(item),
                (item, id) => disabledSelector(item));
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IEnumerable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues = null)
        {
            Func<T, TId> valueSelector = SelectItemUtility.ValidateAndCompile(value, nameof(value));
            Func<T, string> descriptionSelector = SelectItemUtility.ValidateAndCompile(description, nameof(description));
            HashSet<TId> selected = selectedValues == null ? new HashSet<TId>() : new HashSet<TId>(selectedValues);
            HashSet<TId> disabled = disabledValues == null ? new HashSet<TId>() : new HashSet<TId>(disabledValues);
            return SelectItemUtility.ToSelectItemCore(
                items,
                valueSelector,
                descriptionSelector,
                (item, id) => selected.Contains(id),
                (item, id) => disabled.Contains(id));
        }

        #endregion

        #region IQueryable Sync

        /// <summary>
        /// Projects the requested fields in the query provider and enumerates the results synchronously.
        /// </summary>
        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            return SelectItemUtility.CreateSelectQuery(items, value, description, null, null).ToList();
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, TId selectedValue)
        {
            return SelectItemUtility.CreateSelectQueryByIdentifiers(items, value, description, new[] { selectedValue }, null).ToList();
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }

            return SelectItemUtility.CreateSelectQuery(items, value, description, selected, null).ToList();
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected, Expression<Func<T, bool>> disabled)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            if (disabled == null)
            {
                throw new ArgumentNullException(nameof(disabled));
            }

            return SelectItemUtility.CreateSelectQuery(items, value, description, selected, disabled).ToList();
        }

        public static List<SelectItem<TId>> ToSelectItem<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues = null)
        {
            return SelectItemUtility.CreateSelectQueryByIdentifiers(items, value, description, selectedValues, disabledValues).ToList();
        }

        #endregion

        #region IQueryable Async

        /// <summary>
        /// Projects only the requested fields in the query provider before asynchronously enumerating the results.
        /// </summary>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            IQueryable<SelectItem<TId>> query = SelectItemUtility.CreateSelectQuery(items, value, description, null, null);
            return SelectItemUtility.ToSelectItemAsyncCore(query);
        }

        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }

            IQueryable<SelectItem<TId>> query = SelectItemUtility.CreateSelectQuery(items, value, description, selected, null);
            return SelectItemUtility.ToSelectItemAsyncCore(query);
        }

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

            IQueryable<SelectItem<TId>> query = SelectItemUtility.CreateSelectQuery(items, value, description, selected, disabled);
            return SelectItemUtility.ToSelectItemAsyncCore(query);
        }

        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues = null)
        {
            IQueryable<SelectItem<TId>> query = SelectItemUtility.CreateSelectQueryByIdentifiers(items, value, description, selectedValues, disabledValues);
            return SelectItemUtility.ToSelectItemAsyncCore(query);
        }

        #endregion
    }
}
