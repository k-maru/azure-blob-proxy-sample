using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ocelotsample
{
    public class AppendAccountNameToTopSegmentHandler : DelegatingHandler
    {
        private readonly IConfiguration configuration;

        public AppendAccountNameToTopSegmentHandler(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accountName = configuration["Blob:AccountName"];

            var builder = new UriBuilder(request.RequestUri);
            builder.Path = $"/{accountName}{request.RequestUri.AbsolutePath}";
            request.RequestUri = builder.Uri;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
