using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace CNPM
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DevExpress.XtraReports.Web.ASPxWebDocumentViewer.StaticInitialize();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }
    }
}