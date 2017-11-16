using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace connect.strava
{
    static class UriHelper
    {

        public static bool HasParameter(this NameValueCollection parameters, string parameterName)
        {
            return parameters != null && parameters.Count > 0 && parameters.AllKeys.Contains(parameterName);            
        }


        public static string GetParameter(this NameValueCollection parameters, string parameterName)
        {
            if (parameters.HasParameter(parameterName))
            {
                return parameters.Get(parameterName);
            }
            return null;
        }
        

        public static bool HasParameter(this Uri uri, string parameterName)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(uri.Query);
            return parameters.HasParameter(parameterName);
        }


        public static string GetParamerer(this Uri uri, string parameterName)
        {
            
            NameValueCollection parameters = HttpUtility.ParseQueryString(uri.Query);            
            return parameters.GetParameter(parameterName);
        }


    }
}
