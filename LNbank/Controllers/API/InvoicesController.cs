using Microsoft.AspNetCore.Mvc;

namespace LNbank.Controllers.API
{
    public class InvoicesController : BaseApiController
    {
        public InvoicesController()
        {
        }

        [HttpGet("{id}")]
        public IActionResult Index(string id)
        {
            return Ok(id);
        }
    }
}
