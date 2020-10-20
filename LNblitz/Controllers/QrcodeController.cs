using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace LNblitz.Controllers
{
    public class QrcodeController : Controller
    {
        [AllowAnonymous]
        [HttpGet("~/QR/{encode}")]
        public IActionResult Details(string encode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(encode, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeAsSvg = qrCode.GetGraphic(10);
            return Content(qrCodeAsSvg, "image/svg+xml");
        }
    }
}
