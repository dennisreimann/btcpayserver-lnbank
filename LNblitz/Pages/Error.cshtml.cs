using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages
{
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
