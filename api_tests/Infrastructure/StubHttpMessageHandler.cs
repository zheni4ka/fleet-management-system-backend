using System.Net;
using System.Text;

namespace api_tests.Infrastructure
{
    public sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private const string JsonMediaType = "application/json";

        private readonly HttpStatusCode _responseStatusCode;
        private readonly string _responseJson;

        public StubHttpMessageHandler(HttpStatusCode responseStatusCode, string responseJson)
        {
            _responseStatusCode = responseStatusCode;
            _responseJson = responseJson;
        }

        public List<Uri> RequestedUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestedUris.Add(request.RequestUri!);
            return Task.FromResult(new HttpResponseMessage(_responseStatusCode)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, JsonMediaType)
            });
        }
    }
}
