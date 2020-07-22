using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages
{
    public class Error : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
