using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosSharp
{
    public class HttpHandler : IHttpHandler
    {
        async Task<TResponseData> IHttpHandler.GetJsonAsync<TResponseData>(string url, Dictionary<string, string> headerKeyValues, CancellationToken cancellationToken)
        {
            // TODO: RestClient --> HttpClient
            var client = new RestClient(url);
            var request = new RestRequest();
            if (headerKeyValues != null && headerKeyValues.Count > 0)
            {
                foreach (var (key, value) in headerKeyValues)
                {
                    request.AddHeader(key, value);
                }
            }
            var response = await client.ExecuteGetAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw GetApiError(new Uri(url), request, response);
            }

            var result = JsonConvert.DeserializeObject<TResponseData>(response.Content);
            return result;
        }

        async Task<TResponseData> IHttpHandler.PostJsonAsync<TResponseData, TRequestData>(string url, Dictionary<string, string> headerKeyValues, TRequestData requestBody)
        {
            // TODO: RestClient --> HttpClient
            var client = new RestClient(url);
            var request = new RestRequest();
            var body= JsonConvert.SerializeObject(requestBody);
            if (headerKeyValues != null && headerKeyValues.Count > 0)
            {
                foreach (var (key, value) in headerKeyValues)
                {
                    request.AddHeader(key, value);
                }
            }
            request.AddJsonBody(body);
            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw GetApiError(new Uri(url), request, response);
            }

            if(response.Content == null) throw new Exception($"Response is null. Status code: {response.StatusCode}");
            
            var result = JsonConvert.DeserializeObject<TResponseData>(response.Content);
            return result;
        }

        private Exception GetApiError(Uri BaseUrl, RestRequest request, RestResponse response)
        {
            //Get the values of the parameters passed to the API
            string parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + ((x.Value == null) ? "NULL" : x.Value)).ToArray());

            //Set up the information message with the URL, the status code, and the parameters.
            string info = "Request to " + BaseUrl.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ", parameters: "
            + parameters + ", and content: " + response.Content;

            //Acquire the actual exception
            Exception ex;
            if (response != null && response.ErrorException != null)
            {
                ex = response.ErrorException;
            }
            else
            {
                ex = new Exception(info);
                info = string.Empty;
            }

            return ex;
        }
    }
}
