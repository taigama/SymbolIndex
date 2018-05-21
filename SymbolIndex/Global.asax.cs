using SymbolIndex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SymbolIndex
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(ViewSymbolsBinder), new ViewSymbolsBinder());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _SetupRefreshJob();
        }

        // This keep our website alive, thank to:
        //https://www.codeproject.com/Articles/39118/Keep-Your-Website-Alive-Don-t-Let-IIS-Recycle-Your
        private static void _SetupRefreshJob()
        {
            //remove a previous job
            Action remove = HttpContext.Current.Cache["Refresh"] as Action;
            if (remove is Action)
            {
                HttpContext.Current.Cache.Remove("Refresh");
                remove.EndInvoke(null);
            }

            //get the worker
            Action work = () => {
                while (true)
                {
                    Thread.Sleep(60000);

                    WebClient refresh = new WebClient();
                    try
                    {
                        refresh.UploadString("http://www.website.com/", string.Empty);
                    }
                    catch (Exception) {}
                    finally
                    {
                        refresh.Dispose();
                    }
                }
            };
            work.BeginInvoke(null, null);

            //add this job to the cache
            HttpContext.Current.Cache.Add(
                "Refresh",
                work,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                (s, o, r) => { _SetupRefreshJob(); }
                );
        }
    }
}
