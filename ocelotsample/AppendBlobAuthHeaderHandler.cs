using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ocelotsample
{
    public class AppendBlobAuthHeaderHandler : DelegatingHandler
    {
        private readonly IConfiguration configuration;

        public AppendBlobAuthHeaderHandler(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accountName = configuration["Blob:AccountName"];
            var accountKey = configuration["Blob:AccountKey"];

            request.Headers.Add("x-ms-date", DateTime.Now.ToString("R", CultureInfo.InvariantCulture));
            request.Headers.Add("x-ms-version", "2020-02-10");
            request.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                accountName, accountKey, request);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
