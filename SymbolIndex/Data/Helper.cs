using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;





/////////////////////////////////////////////////////
/// -- template --
///

//public ActionResult GetOrders()
//{
//    var orders = db.Orders;
//    var result = new JsonNetResult
//    {
//        Data = order,
//        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
//        Settings = {
//            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//            MaxDepth = 3
//                }
//    };
//    return result;
//}

namespace SymbolIndex.Data
{


    #region Json Net Result
    /// <summary> Our custom json result
    /// <para> you can set serialize depth
    /// </para>
    /// <para> => mega-boost in performance 
    /// </para>
    /// <para> 50KB => 2.7KB
    /// </para>
    /// </summary>
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public JsonSerializerSettings Settings { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("JSON GET is not allowed");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            if (this.ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (this.Data == null)
                return;

            var scriptSerializer = JsonSerializer.Create(this.Settings);

            using (var sw = new StringWriter())
            {
                if (Settings.MaxDepth != null)
                {
                    using (var jsonWriter = new JsonNetTextWriter(sw))
                    {
                        Func<bool> include = () => jsonWriter.CurrentDepth <= Settings.MaxDepth;
                        var resolver = new JsonNetContractResolver(include);
                        this.Settings.ContractResolver = resolver;
                        var serializer = JsonSerializer.Create(this.Settings);
                        serializer.Serialize(jsonWriter, Data);
                    }
                    response.Write(sw.ToString());
                }
                else
                {
                    scriptSerializer.Serialize(sw, this.Data);
                    response.Write(sw.ToString());
                }
            }
        }
    }

    public class JsonNetTextWriter : JsonTextWriter
    {
        public JsonNetTextWriter(TextWriter textWriter) : base(textWriter) { }

        public int CurrentDepth { get; private set; }

        public override void WriteStartObject()
        {
            CurrentDepth++;
            base.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            CurrentDepth--;
            base.WriteEndObject();
        }
    }

    public class JsonNetContractResolver : DefaultContractResolver
    {
        private readonly Func<bool> _includeProperty;

        public JsonNetContractResolver(Func<bool> includeProperty)
        {
            _includeProperty = includeProperty;
        }

        protected override JsonProperty CreateProperty(
            MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = obj => _includeProperty() &&
                                              (shouldSerialize == null ||
                                               shouldSerialize(obj));
            return property;
        }
    }

    public class JsonpResult : JsonNetResult
    {
        object data = null;

        public JsonpResult()
        {
        }

        public JsonpResult(object data)
        {
            this.data = data;
        }

        public override void ExecuteResult(ControllerContext controllerContext)
        {
            if (controllerContext != null)
            {
                HttpResponseBase Response = controllerContext.HttpContext.Response;
                HttpRequestBase Request = controllerContext.HttpContext.Request;

                string callbackfunction = Request["callback"];
                if (string.IsNullOrEmpty(callbackfunction))
                {
                    throw new Exception("Callback function name must be provided in the request!");
                }
                Response.ContentType = "application/x-javascript";
                if (data != null)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Response.Write(string.Format("{0}({1});", callbackfunction, serializer.Serialize(data)));
                }
            }
        }
    }

    #endregion

    #region Static extension
    public static class SIExtension
    {
        /// <summary>
        /// Auto call ToString() per object, and create separator between them
        /// </summary>
        public static string ToString(IList<object> list, string separator = ", ")
        {
            string result = "";
            if (list != null)
            {
                if(list.Count > 0)
                {
                    result = list[0].ToString();
                }

                for(int i = 1; i < list.Count; i++)
                {
                    result += separator + list[i].ToString(); 
                }
            }
            return result;
        }

        /// <summary>
        /// Auto get tags, and create separator between them
        /// </summary>
        public static string ToString(IList<SymbolIndex.Models.Tag> list, string separator = ", ")
        {
            string result = "";
            if (list != null)
            {
                if (list.Count > 0)
                {
                    result = list[0].TagString;
                }

                for (int i = 1; i < list.Count; i++)
                {
                    result += separator + list[i].TagString;
                }
            }
            return result;
        }
    }
    #endregion
}