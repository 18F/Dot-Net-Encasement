using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using SoapHttpClient;
using SoapHttpClient.Extensions;
using SoapHttpClient.Enums;

namespace WebApiTutorial.Connectors
{
    public interface ISoapConnector
    {
        Task<string> CallServiceAsync(string method);
    }
    public class SoapConnector: ISoapConnector
    {
        private XNamespace _ns;
        private Uri _endpoint;

        public SoapConnector(XNamespace ns, Uri endpoint)
        {
            _ns = ns;
            _endpoint = endpoint;
        }

        public async Task<string> CallServiceAsync(string method)
        {
            // Construct the SOAP body
            var body = new XElement(_ns.GetName(method));

            // Make the call to the SOAP service.
            using (var soapClient = new SoapClient())
            {
                var response =
                  await soapClient.PostAsync(
                          endpoint: _endpoint,
                          soapVersion: SoapVersion.Soap11,
                          body: body);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}