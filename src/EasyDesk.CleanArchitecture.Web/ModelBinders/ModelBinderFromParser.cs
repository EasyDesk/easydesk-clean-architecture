using System;

namespace EasyDesk.CleanArchitecture.Web.ModelBinders;

public class ModelBinderFromParser<T> : AbstractModelBinder<T>
{
    private readonly Func<string, T> _parser;

    public ModelBinderFromParser(Func<string, T> parser)
    {
        _parser = parser;
    }

    protected override T ParseModel(string value) => _parser(value);
}
