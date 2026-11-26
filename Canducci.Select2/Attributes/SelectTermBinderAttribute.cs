using Canducci.Select2.Binders;
using Microsoft.AspNetCore.Mvc;
using System;
namespace Canducci.Select2.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class SelectTermBinderAttribute : ModelBinderAttribute
    {
        public SelectTermBinderAttribute() : base(typeof(SelectTermModelBinder)) { }
    }
}