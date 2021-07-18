using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Versioning
{
    public static class VersioningUtils
    {
        public const string VersionHeader = "api-version";
        public const string VersionQueryParam = "version";

        public static Option<ApiVersion> GetControllerVersion(this Type controllerType)
        {
            var ns = controllerType.Namespace.Split('.').Last();
            var match = Regex.Match(ns, @"^V_(\d+)_(\d+)$");
            if (!match.Success)
            {
                return None;
            }
            else
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                return Some(new ApiVersion(major, minor));
            }
        }

        public static IEnumerable<ApiVersion> GetSupportedVersions(params Type[] controllersAssemblyTypes)
        {
            return ReflectionUtils.InstantiableSubtypesOf<AbstractController>(controllersAssemblyTypes)
                .SelectMany(t => t.GetControllerVersion())
                .OrderBy(x => x)
                .Distinct();
        }

        public static string ToDisplayString(this ApiVersion apiVersion)
        {
            return $"v{apiVersion}";
        }
    }
}
