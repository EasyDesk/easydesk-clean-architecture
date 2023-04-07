namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IOverrideAuditProperties
{
    void ConfigureProperties(IDictionary<string, string> properties);
}
