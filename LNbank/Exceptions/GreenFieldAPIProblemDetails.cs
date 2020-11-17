using BTCPayServer.Client;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;

namespace LNbank.Exceptions
{
    public class GreenFieldAPIProblemDetails : StatusCodeProblemDetails
    {
        public string ErrorCode { get; set; }

        public GreenFieldAPIProblemDetails(GreenFieldAPIException ex) : base(StatusCodes.Status400BadRequest)
        {
            Detail = ex.APIError.Message;
            ErrorCode = ex.APIError.Code;
        }
    }
}
