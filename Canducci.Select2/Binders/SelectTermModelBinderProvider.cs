using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
namespace Canducci.Select2.Binders
{
    public sealed class SelectTermModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(ISelectTerm))
            {
                return new SelectTermModelBinder();
            }

            return null;
        }
    }
}