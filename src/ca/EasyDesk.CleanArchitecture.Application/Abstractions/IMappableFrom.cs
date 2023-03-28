namespace EasyDesk.CleanArchitecture.Application.Abstractions;

public interface IMappableFrom<F, T>
    where T : IMappableFrom<F, T>
{
    static abstract T MapFrom(F src);
}
