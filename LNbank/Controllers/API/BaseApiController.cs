using LNbank.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Controllers.API
{
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Api)]
    [Route("~/api/[controller]")]
    public abstract class BaseApiController : Controller
    {
    }
}
