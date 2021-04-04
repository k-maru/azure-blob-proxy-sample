using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ocelotsample
{
    public class RemoveUnexpectedBlobResponseHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            foreach (var resHeader in response.Headers)
            {
                if (resHeader.Key.StartsWith("x-ms-", StringComparison.InvariantCultureIgnoreCase) 
                    || resHeader.Key.Equals("server", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.Headers.Remove(resHeader.Key);
                }
            }
            return response;
        }
    }
}
