
namespace connect.strava
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Security.Cryptography;

    public class Request
    {
        #region Constants

        public const string AUTHORIZE_URL = "https://www.strava.com/oauth/authorize";
        public const string CALLBACK_URL = "http://o.duhart.free.fr";
        public const string ACCESS_TOKEN_URL = "https://www.strava.com/oauth/token";

        #endregion Constants

        #region Public Methods

        public bool GetAccessToken(string subscriptionKey, string clientId, string clientSecret, out string token, out string siteId)
        {
            string code;
            string country;

            token = "";
            siteId = "";

            if (GetAuthorization(clientId, out code))
            {
                return GetToken(subscriptionKey, clientId, clientSecret, code,  out token, out siteId);
            }

            return false;            
        }

        public string Get(string subscriptionKey, string token, string siteID, string signingSecret, string uri)
        {           
            WebRequest webRequest = new WebRequest();
            string result = webRequest.GetData(subscriptionKey, new Uri(uri), token, siteID, signingSecret);

            var obj = JsonConvert.DeserializeObject(result);
            return  JsonConvert.SerializeObject(obj, Formatting.Indented);            
        }

        public string Post(string subscriptionKey, string token, string siteID, string signingSecret, string uri, string requestBody)
        {            
            WebRequest webRequest = new WebRequest();
            string result = webRequest.PostData(subscriptionKey, new Uri(uri), requestBody, token, siteID, signingSecret);

            var obj = JsonConvert.DeserializeObject(result);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public string Put(string subscriptionKey, string token, string siteID, string signingSecret, string uri, string requestBody)
        {
            WebRequest webRequest = new WebRequest();
            string result = webRequest.PutData(subscriptionKey, new Uri(uri), requestBody, token, siteID, signingSecret);

            var obj = JsonConvert.DeserializeObject(result);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public string Delete(string subscriptionKey, string token, string siteID, string signingSecret, string uri, string requestBody)
        {
            WebRequest webRequest = new WebRequest();
            string result = webRequest.DeleteData(subscriptionKey, new Uri(uri), token, siteID, signingSecret);

            var obj = JsonConvert.DeserializeObject(result);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

       

        #endregion Public Methods

        #region Private Methods

        private string GenerateStateParameter(string seed)
        {
            string state = null;
            Random rnd = new Random();
            double d = rnd.NextDouble();
            string tmp = $"{seed}#{d}#{seed}";

            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = Encoding.UTF8.GetBytes(tmp);

            byte[] hash = md5.ComputeHash(inputBytes);

            
            state = Convert.ToBase64String(hash);

            return state;

        }

        private bool GetAuthorization(string clientId, out string code)
        {
            string state = GenerateStateParameter("sd4l");
            code = "";

            StringBuilder url = new StringBuilder(AUTHORIZE_URL);
            url.Append('?');
            url.Append("response_type=code");
            url.Append("&client_id=");
            url.Append(clientId);
            url.Append("&redirect_uri=");
            url.Append(CALLBACK_URL);
            url.Append("&scope=write");
            url.Append("&state=");
            url.Append(state);

            WebBrowserForm form = new WebBrowserForm();
            form.UriNavigate = new Uri(url.ToString());
            if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }

            if (form.State != state)
            {
                return false;
            }

            code = form.Code;
            return true;
        }

        private bool GetToken(string subscriptionKey, string clientId, string clientSecret, string code, out string token, out string siteId)
        {           
            WebRequest request = new WebRequest();

            Uri accesstokenURI = new Uri(ACCESS_TOKEN_URL);
            StringBuilder postData = new StringBuilder();
            postData.Append("client_id=" + clientId + "&");
            postData.Append("client_secret=" + clientSecret + "&");
            postData.Append("code=" + HttpUtility.UrlEncode(code) + "&");
            postData.Append("grant_type=authorization_code&");            
            postData.Append("redirect_uri=" + HttpUtility.UrlEncode(CALLBACK_URL));            

            string response = request.PostData(subscriptionKey, accesstokenURI, postData.ToString());

            token = "";
            siteId = "";

            if (response.Length > 0)
            {
                JObject jObject = JObject.Parse(response);
                string access_token = (string)jObject["access_token"];
                string site_id = (string)jObject["resource_owner_id"];

                if (access_token != null)
                {
                    token = access_token;
                }

                if (site_id != null)
                { 
                    siteId = site_id;
                }

                return true;
            }

            return false;
        }

        #endregion Private Methods
    }
}
