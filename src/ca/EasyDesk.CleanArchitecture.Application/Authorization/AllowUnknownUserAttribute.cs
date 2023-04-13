namespace EasyDesk.CleanArchitecture.Application.Authorization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowUnknownUserAttribute : Attribute
{
}
