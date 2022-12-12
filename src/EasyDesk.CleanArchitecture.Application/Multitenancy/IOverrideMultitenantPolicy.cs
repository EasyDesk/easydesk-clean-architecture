namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IOverrideMultitenantPolicy
{
    MultitenantPolicy MultitenantPolicy { get; }
}
