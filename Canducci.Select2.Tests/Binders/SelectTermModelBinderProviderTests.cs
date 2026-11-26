using System;
using Canducci.Select2;
using Canducci.Select2.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Canducci.Select2.Tests.Binders
{
    public class SelectTermModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WithISelectTermMetadata_ShouldReturnSelectTermModelBinder()
        {
            EmptyModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(ISelectTerm));
            TestModelBinderProviderContext context = new TestModelBinderProviderContext(metadata, metadataProvider);

            IModelBinder result = new SelectTermModelBinderProvider().GetBinder(context);

            Assert.IsType<SelectTermModelBinder>(result);
        }

        [Fact]
        public void GetBinder_WithOtherMetadata_ShouldReturnNull()
        {
            EmptyModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(string));
            TestModelBinderProviderContext context = new TestModelBinderProviderContext(metadata, metadataProvider);

            IModelBinder result = new SelectTermModelBinderProvider().GetBinder(context);

            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_WithConcreteSelectTermMetadata_ShouldReturnNull()
        {
            EmptyModelMetadataProvider metadataProvider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(SelectTerm));
            TestModelBinderProviderContext context = new TestModelBinderProviderContext(metadata, metadataProvider);

            IModelBinder result = new SelectTermModelBinderProvider().GetBinder(context);

            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_WithNullContext_ShouldThrowArgumentNullException()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new SelectTermModelBinderProvider().GetBinder(null));

            Assert.Equal("context", exception.ParamName);
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(ModelMetadata metadata, IModelMetadataProvider metadataProvider)
            {
                Metadata = metadata;
                MetadataProvider = metadataProvider;
                BindingInfo = new BindingInfo();
            }

            public override BindingInfo BindingInfo { get; }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                return null;
            }
        }
    }
}