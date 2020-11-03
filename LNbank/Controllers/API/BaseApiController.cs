using System.Linq;
using LNbank.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Controllers.API
{
    [ApiController]
    [Route("~/api/[controller]")]
    [Authorize(AuthenticationSchemes=AuthenticationSchemes.Api)]
    public abstract class BaseApiController : Controller
    {
        protected string UserId => User?.Claims.First(c => c.Type == "UserId").Value;
    }
}
