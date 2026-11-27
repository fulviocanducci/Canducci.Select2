using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Canducci.Select2
{
    /// <summary>
    /// Defines the expressions and identifier collections used to create Select2 items from an IQueryable source.
    /// </summary>
    public sealed class SelectItemOptions<T, TId>
    {
        public Expression<Func<T, TId>> Value { get; set; }
        public Expression<Func<T, string>> Description { get; set; }
        public IEnumerable<TId> Selected { get; set; }
        public IEnumerable<TId> Disabled { get; set; }
    }
}
