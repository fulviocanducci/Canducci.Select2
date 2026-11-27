using System;
using System.Collections.Generic;

namespace Canducci.Select2
{
    public sealed class Select2Result<TId>
    {
        public Select2Result(List<SelectItem<TId>> results)
        {
            Results = results ?? throw new ArgumentNullException(nameof(results));
        }

        public List<SelectItem<TId>> Results { get; }
    }
}
