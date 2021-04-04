using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ocelotsample
{
    internal static class AzureStorageAuthenticationHelper
    {
        internal static AuthenticationHeaderValue GetAuthorizationHeader(
           string storageAccountName, string storageAccountKey,
           HttpRequestMessage httpRequestMessage)
        {
            var contentEncoding = GetContentHeaderValue(httpRequestMessage, "Content-Encoding");
            var contentLanguage = GetContentHeaderValue(httpRequestMessage, "Content-Language");
            var contentLength = GetContentHeaderValue(httpRequestMessage, "Content-Length");
            var contentMD5 = GetContentHeaderValue(httpRequestMessage, "Content-MD5");
            var contentType = GetContentHeaderValue(httpRequestMessage, "Content-Type");
            var ifModifiedSince = GetHeaderValue(httpRequestMessage, "If-Modified-Since");
            var ifMatch = GetHeaderValue(httpRequestMessage, "If-Match");
            var ifNoneMatch = GetHeaderValue(httpRequestMessage, "If-None-Match");
            var ifUnmodifiedSince = GetHeaderValue(httpRequestMessage, "If-Unmodified-Since");
            var range = GetHeaderValue(httpRequestMessage, "Range");

            var message = string.Join("\n",
                httpRequestMessage.Method.ToString().ToUpperInvariant(),
                contentEncoding ?? "",
                contentLanguage ?? "",
                contentLength == "0" ? "" : contentLength ?? "",
                contentMD5 ?? "", // todo: fix base 64 VALUE
                contentType ?? "",
                "", // Empty date because x-ms-date is expected (as per web page above)
                ifModifiedSince ?? "",
                ifMatch ?? "",
                ifNoneMatch ?? "",
                ifUnmodifiedSince ?? "",
                range ?? "",
                GetCanonicalizedHeaders(httpRequestMessage),
                GetCanonicalizedResource(httpRequestMessage.RequestUri, storageAccountName));

            var signature = Convert.ToBase64String(
                (new HMACSHA256(Convert.FromBase64String(storageAccountKey)))
                    .ComputeHash(Encoding.UTF8.GetBytes(message)));

            return new AuthenticationHeaderValue("SharedKey", storageAccountName + ":" + signature);
        }

        private static string GetHeaderValue(HttpRequestMessage message, string headerName)
            => message.Headers.Contains(headerName) ? message.Headers.GetValues(headerName).FirstOrDefault() : null;
        private static string GetContentHeaderValue(HttpRequestMessage message, string headerName)
            => message.Content.Headers.Contains(headerName) ? message.Headers.GetValues(headerName).FirstOrDefault() : null;

        private static string ConcatWith<T>(this IEnumerable<T> source, char separator)
            => string.Join(separator, source);

        private static string GetCanonicalizedHeaders(HttpRequestMessage httpRequestMessage)
            => httpRequestMessage.Headers
                .Where(h => h.Key.StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase))
                .Select(h => new { Key = h.Key.Trim().ToLowerInvariant(), Values = h.Value })
                .OrderBy(h => h.Key)
                .Select(header =>
                    $"{header.Key}:{header.Values.Select(v => v.TrimStart().Replace("\r", string.Empty).Replace("\n", string.Empty)).ConcatWith(',')}"
                ).ConcatWith('\n')
                .Trim();

        private static string GetCanonicalizedResource(Uri address, string storageAccountName)
        {
            var sb = new StringBuilder("/").Append(storageAccountName);
            if (address.AbsolutePath.Length > 0)
            {
                sb.Append(address.AbsolutePath);
            }
            else
            {
                sb.Append('/');
            }

            var values = HttpUtility.ParseQueryString(address.Query);

            foreach (var item in values.AllKeys.OrderBy(k => k))
            {
                sb.Append('\n').Append(item.ToLowerInvariant()).Append(':').Append(values[item]);
            }

            return sb.ToString();

        }
    }
}
