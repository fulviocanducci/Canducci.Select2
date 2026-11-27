using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Canducci.Select2.Utils
{
    internal static class SelectItemUtility
    {
        internal static List<SelectItem<TId>> ToSelectItemCore<T, TId>(
            IEnumerable<T> items,
            Func<T, TId> valueSelector,
            Func<T, string> descriptionSelector,
            Func<T, TId, bool> selected,
            Func<T, TId, bool> disabled)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            return items.Select(item =>
            {
                TId id = valueSelector(item);
                string text = descriptionSelector(item);
                bool isSelected = selected != null && selected(item, id);
                bool isDisabled = disabled != null && disabled(item, id);
                return new SelectItem<TId>(id, text, isSelected, isDisabled);
            }).ToList();
        }

        internal static IQueryable<SelectItem<TId>> CreateSelectQuery<T, TId>(
            IQueryable<T> items,
            Expression<Func<T, TId>> value,
            Expression<Func<T, string>> description,
            Expression<Func<T, bool>> selected,
            Expression<Func<T, bool>> disabled)
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

        internal static IQueryable<SelectItem<TId>> CreateSelectQueryByIdentifiers<T, TId>(
            IQueryable<T> items,
            Expression<Func<T, TId>> value,
            Expression<Func<T, string>> description,
            IEnumerable<TId> selectedValues,
            IEnumerable<TId> disabledValues)
        {
            Validate(value, nameof(value));
            Validate(description, nameof(description));

            Expression<Func<T, bool>> selected = CreateContainsPredicate(value, selectedValues);
            Expression<Func<T, bool>> disabled = CreateContainsPredicate(value, disabledValues);
            return CreateSelectQuery(items, value, description, selected, disabled);
        }

        internal static async Task<List<SelectItem<TId>>> ToSelectItemAsyncCore<TId>(IQueryable<SelectItem<TId>> items)
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

        internal static Func<T, TResult> ValidateAndCompile<T, TResult>(Expression<Func<T, TResult>> expression, string parameterName)
        {
            Validate(expression, parameterName);
            return expression.Compile();
        }

        private static Expression<Func<T, bool>> CreateContainsPredicate<T, TId>(Expression<Func<T, TId>> value, IEnumerable<TId> identifiers)
        {
            TId[] values = identifiers == null ? Array.Empty<TId>() : identifiers.ToArray();
            if (values.Length == 0)
            {
                return null;
            }

            MethodCallExpression contains = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                new[] { typeof(TId) },
                Expression.Constant(values),
                value.Body);
            return Expression.Lambda<Func<T, bool>>(contains, value.Parameters);
        }

        private static void Validate<T, TResult>(Expression<Func<T, TResult>> expression, string parameterName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
        {
            return new ParameterReplaceVisitor(source, target).Visit(expression);
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
    }
}
