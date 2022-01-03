using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate IActionResult ResultProvider<T>(object body, T arg);
