using EasyDesk.Commons.Options;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public delegate Option<T> ClaimsPrincipalParser<T>(ClaimsPrincipal claimsPrincipal);
