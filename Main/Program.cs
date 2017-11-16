


namespace connect.strava.Console
{
    using System;

    class Program
    {


        public static object ConfigurationManager { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["clientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["clientSecret"];
            string callback = System.Configuration.ConfigurationManager.AppSettings["callback"];
            string subscriptionKey = "";

            //string clientId = "21497";
            //string clientSecret = "0c849fb764af52b82a0081c1cd28ae67be02e136";            



            string token;
            string siteId;

            connect.strava.Request request = new connect.strava.Request();
            bool tokenOK = request.GetAccessToken(subscriptionKey, clientId, clientSecret, out token, out siteId);
            if (tokenOK)
            {
                Console.WriteLine("Now you're logged in. do something really interesting...");
                Console.ReadLine();
            }

        }
    }
}
