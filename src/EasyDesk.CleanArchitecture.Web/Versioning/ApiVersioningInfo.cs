using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public record ApiVersioningInfo(IImmutableSet<ApiVersion> SupportedVersions);
