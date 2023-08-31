using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.SampleApp.Domain.DomainErrors;

public record TestDomainError(string Content) : DomainError;
