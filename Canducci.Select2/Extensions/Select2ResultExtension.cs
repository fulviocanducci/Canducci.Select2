using System;
using System.Collections.Generic;

namespace Canducci.Select2.Extensions
{
    public static class Select2ResultExtension
    {
        public static Select2Result<TId> ToSelect2<TId>(this List<SelectItem<TId>> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return new Select2Result<TId>(results);
        }
    }
}
