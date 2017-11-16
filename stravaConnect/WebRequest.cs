

namespace connect.strava
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;

    internal class WebRequest
    {

        #region Public Methods

        public string GetData(string subscriptionKey, Uri url, string token, string siteID, string signingSecret)
        {
            string nonce = GenerateNonce();

            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            string signature = APIRequestSigner.GenerateSignature("GET", url, null, signingSecret, token, siteID, nonce);

            SetHeaders(subscriptionKey, MethodEnum.GET, webRequest, token, siteID, signature, nonce, true);

            return GetRequest(webRequest);
        }

        public string PostData(string subscriptionKey, Uri url, string requestBody)
        {
            try
            {                
                string nonce = GenerateNonce();

                HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
                
                SetHeaders(subscriptionKey, MethodEnum.POST, webRequest, "", "", "", nonce, false);
         
                return SendRequest(webRequest, requestBody);
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public string PostData(string subscriptionKey, Uri url, string requestBody, string token, string siteID, string signingSecret)
        {            
            try
            {         
                string nonce = GenerateNonce();

                HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

                string signature = APIRequestSigner.GenerateSignature("POST", url, requestBody, signingSecret, token, siteID, nonce);

                SetHeaders(subscriptionKey, MethodEnum.POST, webRequest, token, siteID, signature, nonce, true);

                return SendRequest(webRequest, requestBody);
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public string PutData(string subscriptionKey, Uri url, string requestBody, string token, string siteID, string signingSecret)
        {
            string nonce = GenerateNonce();

            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;

            string signature = APIRequestSigner.GenerateSignature("PUT", url, requestBody, signingSecret, token, siteID, nonce);

            SetHeaders(subscriptionKey, MethodEnum.PUT, webRequest, token, siteID, signature, nonce, true);
            
            return SendRequest(webRequest, requestBody);
        }

        public string DeleteData(string subscriptionKey, Uri baseurl, string token, string siteID, string signingSecret)
        {
            string nonce = GenerateNonce();

            HttpWebRequest webRequest = System.Net.WebRequest.Create(baseurl) as HttpWebRequest;

            string signature = APIRequestSigner.GenerateSignature("DELETE", baseurl, null, signingSecret, token, siteID, nonce);
            
            SetHeaders(subscriptionKey, MethodEnum.DELETE, webRequest, token, siteID, signature, nonce, true);

            return GetRequest(webRequest);
        }

        #endregion Public Methods

        #region Private Methods

        private string SendRequest(HttpWebRequest webRequest, string postData)
        {
            StreamWriter requestWriter = null;
            requestWriter = new StreamWriter(webRequest.GetRequestStream());

            try
            {
                requestWriter.Write(postData);
            }
            catch
            {
                throw;
            }
            finally
            {
                requestWriter.Close();
                requestWriter = null;
            }
            return GetRequest(webRequest);
        }

        private string GetRequest(HttpWebRequest webRequest)
        {
            string responseData = "";
            responseData = GetWebResponse(webRequest);
            webRequest = null;
            return responseData;
        }

        public string GenerateNonce()
        {
            RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();
            Byte[] output = new Byte[32];
            rng.GetBytes(output);
            return Convert.ToBase64String(output);
        }

        private string GetWebResponse(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            WebResponse response;
            string responseData = "";

            try
            {
                response = webRequest.GetResponse();
                responseReader = new StreamReader(response.GetResponseStream());
                responseData = responseReader.ReadToEnd();

                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }
            catch (WebException webex)
            {
                string text;

                using (var sr = new StreamReader(webex.Response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                responseData = text + webex.InnerException;
            }
            catch (Exception ex)
            {
                responseData = ex.Message;
                responseReader.Close();
                responseReader = null;
            }

            return responseData;
        }

        private void SetHeaders(string subscriptionKey, MethodEnum method, HttpWebRequest webRequest, string accessToken, string siteID, string signature, string nonce, bool json)
        {
            webRequest.AllowAutoRedirect = true;
            webRequest.Accept = "*/*";
            webRequest.UserAgent = "CSharp Test";
            webRequest.Headers.Add("X-Signature", signature);
            webRequest.Headers.Add("X-Nonce", nonce);
            webRequest.Headers.Add("ocp-apim-subscription-key", subscriptionKey);
            webRequest.Timeout = 100000;

            if (json)
            {
                webRequest.ContentType = "application/json";
            }
            else
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
            }

            if (accessToken != "")
            {
                string authorization = String.Concat("Bearer ", accessToken);
                webRequest.Headers.Add("Authorization", authorization);
            }

            if (siteID != "")
            {

                webRequest.Headers.Add("X-Site", siteID);
            }

            switch (method)
            {
                case MethodEnum.GET:
                    webRequest.Method = "GET";
                    break;

                case MethodEnum.POST:
                    webRequest.Method = "POST";
                    break;

                case MethodEnum.PUT:
                    webRequest.Method = "PUT";
                    break;

                case MethodEnum.DELETE:
                    webRequest.Method = "DELETE";
                    break;
            }
        }
        
        #endregion Private Methods
    }
}
