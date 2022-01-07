using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record DomainErrorWrapper(DomainError Error) : Error;
