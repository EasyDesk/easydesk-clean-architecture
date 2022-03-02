using EasyDesk.Tools.Results;
using System;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InternalError(Exception Exception) : Error;
