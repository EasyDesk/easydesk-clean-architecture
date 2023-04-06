using EasyDesk.CleanArchitecture.Infrastructure.Versioning;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public record ApiVersioningInfo(IImmutableSet<ApiVersion> SupportedVersions);
