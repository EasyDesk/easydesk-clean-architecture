namespace EasyDesk.CleanArchitecture.Application.Authorization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllowUnknownUserAttribute : Attribute
{
}
