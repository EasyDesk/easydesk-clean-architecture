using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

internal class OptionBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType.IsGenericType
            && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Option<>))
        {
            var type = context.Metadata.ModelType.GetGenericArguments()[0];
            var metadata = context.MetadataProvider.GetMetadataForType(type);
            return new OptionBinder(metadata, context.CreateBinder(metadata));
        }
        return null;
    }

    private class OptionBinder : IModelBinder
    {
        private readonly ModelMetadata _innerModelMetadata;
        private readonly IModelBinder _innerModelBinder;

        public OptionBinder(ModelMetadata innerModelMetadata, IModelBinder innerModelBinder)
        {
            _innerModelMetadata = innerModelMetadata;
            _innerModelBinder = innerModelBinder;
        }

        private Type InnerType => _innerModelMetadata.ModelType;

        private Option<T> InnerNone<T>() => NoneT<T>();

        private Option<T> InnerSome<T>(T obj) => Some(obj);

        private object EmptyOption() => GetType()
            .GetMethod(nameof(InnerNone), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(InnerType)
            .Invoke(this, Array.Empty<object>())!;

        private object OptionWithSome(object obj)
        {
            var method = GetType()
                .GetMethod(nameof(InnerSome), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(InnerType);
            var arguments = new[] { obj };
            return method.Invoke(this, arguments)!;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(EmptyOption());
                return;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(EmptyOption());
                return;
            }

            var newBindingContext = DefaultModelBindingContext.CreateBindingContext(
                bindingContext.ActionContext,
                bindingContext.ValueProvider,
                _innerModelMetadata,
                bindingInfo: null,
                bindingContext.ModelName);

            await _innerModelBinder.BindModelAsync(newBindingContext);

            if (newBindingContext.Result.IsModelSet && newBindingContext.Result.Model is not null)
            {
                var option = OptionWithSome(newBindingContext.Result.Model);
                bindingContext.Result = ModelBindingResult.Success(option);

                // Setting the ValidationState ensures properties on derived types are correctly validated
                bindingContext.ValidationState[newBindingContext.Result.Model] = new ValidationStateEntry
                {
                    Metadata = _innerModelMetadata,
                };
            }
            else if (newBindingContext.Result.Model is null)
            {
                bindingContext.Result = ModelBindingResult.Success(EmptyOption());
                return;
            }
        }
    }
}
