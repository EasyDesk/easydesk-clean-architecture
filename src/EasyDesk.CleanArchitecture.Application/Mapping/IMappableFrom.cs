namespace EasyDesk.CleanArchitecture.Application.Mapping;

public interface IMappableFrom<F, T>
    where T : IMappableFrom<F, T>
{
    static abstract T MapFrom(F src);
}
