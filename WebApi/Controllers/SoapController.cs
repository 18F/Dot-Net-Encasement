using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiTutorial.Connectors;

namespace WebApiTutorial.Controllers
{
    [Route("api/[controller]")]
    public class SoapController : Controller
    {
        // Private string to hold the name of the SOAP method we want to invoke.
        private string _methodName = "GetLanguageList";

        // Private member to hold SOPA connector.
        private ISoapConnector _connector;

        public SoapController(ISoapConnector connector)
        {
            _connector = connector;
        }

        // GET api/soap
        [HttpGet]
        public ContentResult Get()
        {
            // Create a new XML document to hold the response from the SOAP service.
            XmlDocument doc = new XmlDocument();

            try
            {
                // Load the response from the SOAP service.
                doc.LoadXml(GetSoapResponse(_methodName));

                //
                var languageList = doc.GetElementsByTagName("GetLanguageListResult");

                // Convert the XML document to JSON format.
                return Content(JsonConvert.SerializeObject(languageList), "application/json");

            }
            catch (XmlException ex)
            {
                var exception = "<error>" + ex.Message + "</error>";
                doc.LoadXml(exception);
                return Content(JsonConvert.SerializeObject(doc), "application/json");
            }


        }

        /**
         * Private method to invoke SoapConnector and make service call.       
        */
        private string GetSoapResponse(string methodName)
        {
            var response = _connector.CallServiceAsync(methodName);
            return response.Result;
        }
    }
}