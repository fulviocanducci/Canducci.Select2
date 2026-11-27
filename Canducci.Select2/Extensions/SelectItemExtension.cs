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
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            return ToSelectItemCore(items, value, description, selected.Compile(), null);
        }

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
        /// Projects only the requested fields in the query provider before asynchronously enumerating the results.
        /// </summary>
        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description)
        {
            IQueryable<SelectItem<TId>> query = CreateSelectQuery(items, value, description, null, null);
            return ToSelectItemAsyncCore(query);
        }

        public static Task<List<SelectItem<TId>>> ToSelectItemAsync<T, TId>(this IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }
            IQueryable<SelectItem<TId>> query = CreateSelectQuery(items, value, description, selected, null);
            return ToSelectItemAsyncCore(query);
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

        private static IQueryable<SelectItem<TId>> CreateSelectQuery<T, TId>(IQueryable<T> items, Expression<Func<T, TId>> value, Expression<Func<T, string>> description, Expression<Func<T, bool>> selected, Expression<Func<T, bool>> disabled)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            Validate(value, nameof(value));
            Validate(description, nameof(description));

            ParameterExpression parameter = Expression.Parameter(typeof(T), "item");
            Expression valueBody = ReplaceParameter(value.Body, value.Parameters[0], parameter);
            Expression descriptionBody = ReplaceParameter(description.Body, description.Parameters[0], parameter);
            Expression selectedBody = selected == null
                ? Expression.Constant(false)
                : ReplaceParameter(selected.Body, selected.Parameters[0], parameter);
            Expression disabledBody = disabled == null
                ? Expression.Constant(false)
                : ReplaceParameter(disabled.Body, disabled.Parameters[0], parameter);

            var constructor = typeof(SelectItem<TId>).GetConstructor(new[]
            {
                typeof(TId),
                typeof(string),
                typeof(bool),
                typeof(bool)
            });
            NewExpression projection = Expression.New(constructor, valueBody, descriptionBody, selectedBody, disabledBody);
            Expression<Func<T, SelectItem<TId>>> selector = Expression.Lambda<Func<T, SelectItem<TId>>>(projection, parameter);
            return items.Select(selector);
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
        {
            return new ParameterReplaceVisitor(source, target).Visit(expression);
        }

        private static async Task<List<SelectItem<TId>>> ToSelectItemAsyncCore<TId>(IQueryable<SelectItem<TId>> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            IAsyncEnumerable<SelectItem<TId>> asyncItems = items as IAsyncEnumerable<SelectItem<TId>>;
            if (asyncItems == null)
            {
                throw new InvalidOperationException("The query provider does not support asynchronous enumeration.");
            }
            List<SelectItem<TId>> result = new List<SelectItem<TId>>();
            await foreach (SelectItem<TId> item in asyncItems)
            {
                result.Add(item);
            }
            return result;
        }

        private static Func<T, TResult> ValidateAndCompile<T, TResult>(Expression<Func<T, TResult>> expression, string parameterName)
        {
            Validate(expression, parameterName);
            return expression.Compile();
        }

        private static void Validate<T, TResult>(Expression<Func<T, TResult>> expression, string parameterName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private sealed class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source ? _target : base.VisitParameter(node);
            }
        }

        #endregion
    }
}
