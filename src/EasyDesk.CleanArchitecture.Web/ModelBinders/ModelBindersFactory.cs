using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace EasyDesk.CleanArchitecture.Web.ModelBinders;

public static class ModelBindersFactory
{
    public static IModelBinder FromParser<T>(Func<string, T> parser) =>
        new ModelBinderFromParser<T>(parser);

    public static IModelBinder ForDate() => FromParser(Date.Parse);

    public static IModelBinder ForTimestamp() => FromParser(Timestamp.Parse);

    public static IModelBinder ForDuration() => FromParser(Duration.Parse);

    public static IModelBinder ForTimeOfDay() => FromParser(TimeOfDay.Parse);
}
