namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowNoTenantAttribute : Attribute
{
}
