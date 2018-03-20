using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using SymbolIndex.Data;
using SymbolIndex.Models;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace SymbolIndex.Controllers
{
    public class APIController : Controller
    {
        SIContext db = new SIContext();

        //[Route("API/GetFontInfo/{id}")]
        [HttpGet]
        public ActionResult GetFontInfo(int id, bool more = false)
        {
            object theFont = null;

            if(!more)
            {
                string strQuery = "select * from dbo.Font where Id=" + id.ToString();
                theFont = db.Database.SqlQuery<FontSimple>(strQuery);
            }
            else
            {
                theFont = db.Fonts.Find(id);
            }
            return ParseJson(theFont??"Font info not found", 3);
        }

        [HttpGet]
        public ActionResult GetOverall()
        {
            object data = new
            {
                Fonts = db.Database.SqlQuery<FontSimple>("select * from dbo.Font"),
                Symbols = db.Database.SqlQuery<SymbolSimple>("select * from dbo.Symbol"),
                Tags = db.Database.SqlQuery<TagSimple>("select * from dbo.Tag"),
                TagSymbols = db.Database.SqlQuery<TagSymbolSimple>("select Tag_Id,Symbol_Id from dbo.TagSymbol")
            };
            
            return ParseJson(data, 2);
        }

        [HttpGet]
        public ActionResult GetTable(string id)
        {
            object data = null;
            switch (id)
            {
                case "fonts":
                    var fdata = db.Fonts.ToList();
                    foreach(var font in fdata)
                    {
                        font.Symbols = null;
                    }
                    data = fdata;
                break;
                case "tags":
                    var tdata = db.Tags.ToList();
                    foreach (var tag in tdata)
                    {
                        tag.Symbols = null;
                    }
                    data = tdata;
                    break;
                default:
                    break;
            }

            if (data == null)
                return HttpNotFound("This table are not avaiable");

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonNetResult ParseJson(object data, int depth = 1
            , ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore)
        {
            var result = new JsonNetResult
            {
                Data = data,
                JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
                Settings = {
            ReferenceLoopHandling = referenceLoopHandling,
            MaxDepth = depth// product, productdetail, author
                }
            };
            return result;
        }
    }
}
