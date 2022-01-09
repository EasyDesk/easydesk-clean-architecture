using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate IActionResult ActionResultProvider<T>(ResponseDto body, T arg);
