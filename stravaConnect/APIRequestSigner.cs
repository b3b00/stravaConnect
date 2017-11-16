namespace connect.strava
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    internal class APIRequestSigner
    {
        #region Internal Methods

        internal static string GenerateSignature(string httpMethod, Uri url, string requestBody, string signingSecret, string token, string siteID, string nonce)
        {
            httpMethod = httpMethod.ToUpper();

            string encodedParams = NormalizeParams(httpMethod, url, requestBody);

            string encodedUri = Uri.EscapeDataString(url.GetLeftPart(UriPartial.Path).ToLower()); // this needs to be ToLower() to match the request check at the server

            string encodedNonce = Uri.EscapeDataString(nonce);

            string signingKey = Uri.EscapeDataString(signingSecret) + "&" + Uri.EscapeDataString(token);

            string encodedResourceOwnerID = Uri.EscapeDataString(siteID);
            
            string baseString = String.Format("{0}&{1}&{2}&{3}&{4}", httpMethod, encodedUri, encodedParams, encodedNonce, encodedResourceOwnerID);
         
            return GenerateHmac(signingKey, baseString);
        }

        #endregion Internal Methods

        #region Private Methods

        private static string GenerateHmac(string signingKey, string baseString)
        {
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));

            return Convert.ToBase64String(
                hasher.ComputeHash(
                new ASCIIEncoding().GetBytes(baseString)));
        }

        private static string NormalizeParams(string httpMethod, Uri url, string requestBody)
        {
            IEnumerable<KeyValuePair<string, string>> kvpParams = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrWhiteSpace(url.Query))
            {
                IEnumerable<KeyValuePair<string, string>> queryParams =
                  from p in url.Query.Substring(1).Split('&').AsEnumerable()
                  let key = Uri.EscapeDataString(p.Substring(0, p.IndexOf("=")))
                  let value = Uri.EscapeDataString(p.Substring(p.IndexOf("=") + 1))
                  select new KeyValuePair<string, string>(key, value);

                kvpParams = kvpParams.Union(queryParams);
            }

            List<KeyValuePair<string, string>> encodedrequestBodyParams = new List<KeyValuePair<string, string>>();

            if (requestBody != null)
            {
                requestBody = Uri.EscapeDataString(Base64Encode(requestBody));
            }

            encodedrequestBodyParams.Add(new KeyValuePair<string, string>("body", requestBody));
            kvpParams = kvpParams.Union(encodedrequestBodyParams);

            IEnumerable<string> sortedParams =
              from p in kvpParams
              orderby p.Key ascending, p.Value ascending
              select p.Key + "=" + p.Value;

            string encodedParams = String.Join("&", sortedParams);
            encodedParams = Uri.EscapeDataString(encodedParams);
            return encodedParams;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
        #endregion Private Methods
    }
}
