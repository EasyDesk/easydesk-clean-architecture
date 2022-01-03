using System;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InternalError(Exception Exception) : Error(
    $"Internal server error: {Exception.Message}",
    Errors.Codes.Internal);
