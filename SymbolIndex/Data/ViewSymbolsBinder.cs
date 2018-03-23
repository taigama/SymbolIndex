using Newtonsoft.Json.Linq;
using SymbolIndex.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SymbolIndex.Data
{
    public class ViewSymbolsBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                                ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            
            var form = request.Form;

            string[] raw = form.GetValues(0);
            JObject jObject = JObject.Parse(raw[0]);
            JArray jArray = (JArray)jObject["viewSymbols"];

            //dynamic 
            IList<SymbolView> symbols = jArray.ToObject<IList<SymbolView>>();

            return symbols;
        }
    }
}