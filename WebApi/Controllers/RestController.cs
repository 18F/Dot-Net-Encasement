using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApi.Connectors;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RestController : Controller
    {
        private string packageSearch = "/api/3/action/package_search";
        // Path to show package details.
        private string packageDetails = "/api/3/action/package_show?id={0}";

        private IRestConnector _connector;

        public RestController(IRestConnector connector)
        {
            _connector = connector;
        }

        // GET api/rest/search
        [HttpGet("search")]
        public ContentResult Get()
        {
            string response = GetPackageData(packageSearch);
            return Content(response, "application/json");
        }

        // GET api/rest/details?id=
        [HttpGet("details")]
        public ContentResult Get(string id)
        {
            string path = String.Format(packageDetails, id);
            string response = GetPackageData(path);
            return Content(response, "application/json");
        }

        /**
         * Private method to invoke RestConnector and make API call.       
         */
        private string GetPackageData(string path)
        {
            // Make the API call using the psecificed path.
            var response = _connector.CallServiceAsync(path).Result;

            // Parse the raw API response.
            JObject result = JObject.Parse(response);

            // Return the result.
            return result.GetValue("result").ToString();
        }
    }
}