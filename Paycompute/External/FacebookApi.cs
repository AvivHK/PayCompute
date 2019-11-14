using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PayCompute.External
{
    public class FacebookApi
    {
        private readonly string FB_ACCESS_TOKEN = "EAAJsCF7Ek8cBAPSYz9ns4KG3ReqZC16CgJGIyuPpFZArxC2nqYLZB06upZAjHToSESw0nJYCGAusO0BibckDatut2WNJw138RxHuZC5zSB8ZAFWlOiqt85ZBqhoAVFMrLIWESrNap3ZCZBgV6YtCaksZBZC2veG4qB9uUbxzwAx5iYfEwZDZD";
        private const string FB_WELCOME_PHOTO = "http://blog.clickdimensions.com/wp-content/uploads/2017/04/BlogFeatureImage-NewEmployees.png";

        public FacebookApi() { }

        public string PublishMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine("Got request to post to FB page");
            var client = new RestClient("https://graph.facebook.com/v2.11/me/photos");
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(new { url = FB_WELCOME_PHOTO, caption = message, access_token = FB_ACCESS_TOKEN });
            IRestResponse response = client.Execute(request);
            System.Diagnostics.Debug.WriteLine("response: " + response.Content);

            return null;
        }
    }
}
