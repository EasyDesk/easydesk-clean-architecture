namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UseMultitenantPolicyAttribute : Attribute
{
    public UseMultitenantPolicyAttribute(MultitenantPolicy policy)
    {
        Policy = policy;
    }

    public MultitenantPolicy Policy { get; }
}
