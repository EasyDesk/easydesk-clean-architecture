namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public class SagaCorrelationSelector<TNext, T, TId>
{
    private readonly Func<Func<T, TId>, TNext> _nextStep;

    internal SagaCorrelationSelector(Func<Func<T, TId>, TNext> nextStep)
    {
        _nextStep = nextStep;
    }

    public TNext CorrelateWith(Func<T, TId> correlationProperty) => _nextStep(correlationProperty);
}
