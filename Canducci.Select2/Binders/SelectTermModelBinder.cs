using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
namespace Canducci.Select2.Binders
{
    public sealed class SelectTermModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string term = bindingContext.ValueProvider.GetValue("term").FirstValue;
            string q = bindingContext.ValueProvider.GetValue("q").FirstValue;
            string type = bindingContext.ValueProvider.GetValue("_type").FirstValue;
            string pageValue = bindingContext.ValueProvider.GetValue("page").FirstValue;

            int.TryParse(pageValue, out int page);

            ISelectTerm result = new SelectTerm
            {
                Term = term,
                Q = q,
                Type = type,
                Page = page
            };

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
