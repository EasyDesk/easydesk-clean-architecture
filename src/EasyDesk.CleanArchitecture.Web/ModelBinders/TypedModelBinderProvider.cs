using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Web.ModelBinders;

public class TypedModelBinderProvider : IModelBinderProvider
{
    private readonly IDictionary<Type, Func<IModelBinder>> _bindersByType;

    public TypedModelBinderProvider(IDictionary<Type, Func<IModelBinder>> bindersByType)
    {
        _bindersByType = bindersByType;
    }

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var modelType = context.Metadata.UnderlyingOrModelType;
        return _bindersByType
            .GetOption(modelType)
            .Map(p => p())
            .OrElseDefault();
    }
}
