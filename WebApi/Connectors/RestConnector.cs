using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApiTutorial.Connectors
{
    public interface IRestConnector
    {
        Task<string> CallServiceAsync(string path);
    }
    public class RestConnector : IRestConnector
    {
        private Uri _endpoint;
        public RestConnector(Uri endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<string> CallServiceAsync(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = _endpoint;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    return await client.GetStringAsync(path);
                }
                catch (HttpRequestException ex)
                {
                    return String.Format(@"{{""result"": {{""error"": ""{0}""}}}}", ex.Message);
                }
            }
        }
    }
}