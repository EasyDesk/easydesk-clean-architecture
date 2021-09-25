using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.ModelBinders
{
    public abstract class AbstractModelBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var modelState = bindingContext.ModelState;
            modelState.SetModelValue(modelName, valueProviderResult);

            var metadata = bindingContext.ModelMetadata;
            var type = metadata.UnderlyingOrModelType;

            var value = valueProviderResult.FirstValue;
            var model = type == typeof(T) ? ParseModelHandlingNull(value) : throw new InvalidOperationException();

            if (model == null && !metadata.IsReferenceOrNullableType)
            {
                modelState.TryAddModelError(
                    modelName,
                    metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                        valueProviderResult.ToString()));
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }

            return Task.CompletedTask;
        }

        private object ParseModelHandlingNull(string value) => value is null ? null : ParseModel(value);

        protected abstract T ParseModel(string value);
    }
}
