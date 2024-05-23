using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Web.Versioning;

public record ApiVersioningInfo(IFixedSet<ApiVersion> SupportedVersions);
