using System.Collections.Generic;

namespace OnlineStore.Web.API.Models
{
    public class ErrorResponse
    {
        public string StatusCode { get; }
        public IEnumerable<Error> Errors { get; set; }

        public ErrorResponse(string statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
