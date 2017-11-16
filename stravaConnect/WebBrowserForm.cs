
namespace connect.strava
{
    using System;
    using System.Windows.Forms;
    using System.Diagnostics;
    using Microsoft.Win32;

    public partial class WebBrowserForm : Form
    {
        #region Members

        private bool _FinishFederation = false;

        #endregion Members

        #region Constructors

        public WebBrowserForm()
        {
            InitializeComponent();
          
            SetBrowserEmulation();

            webBrowser.DocumentCompleted += webBrowser_DocumentCompleted;
            webBrowser.StatusTextChanged += webBrowser_StatusTextChanged;
        }
        
        #endregion Constructors

        #region Public Atributtes

        public Uri UriNavigate
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

     

        public string State
        {
            get;
            set;
        }

        #endregion Public Atributtes

        #region Events

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            //if (_FinishFederation || webBrowser.StatusText.Contains("dnserror"))
            //{
                string escaped = Uri.EscapeUriString(Request.CALLBACK_URL);
            if (e.Url.AbsoluteUri.Contains(escaped))
            {
                ;
            }
                if (e.Url.AbsoluteUri.StartsWith(escaped))
                {                    
                    GetCodeWebBrowserHandler(e.Url);
                    //webBrowser.Visible = false;
                }

            //}
        }

        void webBrowser_StatusTextChanged(object sender, EventArgs e)
        {
            if (webBrowser.StatusText.Contains("dnserror"))
            {
                _FinishFederation = true;
            }
        }

        private void WebBrowserForm_Shown(object sender, EventArgs e)
        {
            this.webBrowser.Navigate(this.UriNavigate);
        }

        #endregion Events

        #region Private Methods

        private static void SetBrowserEmulation()
        {
            int version;
            int regValue;

            try
            {                
                using (WebBrowser Wb = new WebBrowser())
                    version = Wb.Version.Major;
                
                if (version >= 11)
                    regValue = 11001;
                else if (version == 10)
                    regValue = 10001;
                else if (version == 9)
                    regValue = 9999;
                else if (version == 8)
                    regValue = 8888;
                else
                    regValue = 7000;

                string path;
                if (Environment.Is64BitOperatingSystem)
                    path = @"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION";
                else
                    path = @"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path, true))
                {                    
                    key.SetValue(Process.GetCurrentProcess().ProcessName + ".exe", regValue, RegistryValueKind.DWord);                    
                }
            }
            catch (Exception Ex)
            {
            }
        }

        


        private void GetCodeWebBrowserHandler(Uri uri)
        {
            string queryString = uri.Query;
            

            if (uri.HasParameter("code"))
            {
                this.Code = uri.GetParamerer("code");//absoluteUri.Split(new string[] { "?code=", "&" }, StringSplitOptions.RemoveEmptyEntries)[1];
                this.State = uri.GetParamerer("state");//absoluteUri.Split(new string[] { "&state=" }, StringSplitOptions.RemoveEmptyEntries)[1];

                this.DialogResult = DialogResult.OK;
            }
            else if (uri.HasParameter("error"))
            {
                this.Code = "";
                this.DialogResult = DialogResult.Cancel;
            }
            else if (uri.AbsoluteUri.ToLowerInvariant().Contains("consent/deny"))
            {
                this.Code = "";
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                ;
            }
        }

        #endregion Private Methods
    }
}
