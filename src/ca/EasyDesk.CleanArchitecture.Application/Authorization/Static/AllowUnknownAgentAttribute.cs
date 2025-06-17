namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowUnknownAgentAttribute : Attribute;
