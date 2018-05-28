using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SymbolIndex.Data;
using SymbolIndex.Models;

namespace SymbolIndex.Controllers
{
    
    public class HomeController : Controller
    {
        SIContext db = new SIContext();

        [HttpGet]
        public ActionResult Index(int? fontId)
        {
            if(fontId == null)
            {
                fontId = CheckCookie(this)??3;
            }

            var fonts = db.Fonts.ToList();
            ViewBag.Fonts = fonts;

            Font font = fonts.Find(x=>x.Id == fontId);
            ViewBag.Symbols = null;
            ViewBag.FontCurrent = font;

            SetCookie(this, fontId.Value, 30);

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        
        public ActionResult _FontStyle(int? fontId)
        {
            if (fontId == null)
            {
                fontId = CheckCookie(this) ?? 3;
            }

            var font = db.Fonts.Find(fontId);
            ViewBag.FontCurrent = font;

            SetCookie(this, fontId.Value, 30);

            return PartialView();
        }

        public ActionResult _Items(int? fontId = 3, string key = null)
        {
            var font = db.Fonts.Find(fontId);
            ViewBag.FontCurrent = font;

            if(string.IsNullOrEmpty(key))
            {
                ViewBag.Symbols = font.Symbols.ToList();                
            }
            else
            {
                List<Tag> tagsSearched = db.Tags.Where(x => x.TagString.Contains(key)).ToList();
                List<Symbol> symbols = new List<Symbol>();
                foreach (Tag tag in tagsSearched)
                {
                    symbols.AddRange(tag.Symbols);
                }
                symbols = symbols.Distinct().ToList();
                symbols.RemoveAll(x => x.FontId != fontId);

                ViewBag.Symbols = symbols;
            }
            return PartialView();
        }

        //[HttpPost]
        public ActionResult Feedback(string content)
        {
            if(string.IsNullOrWhiteSpace(content))
            {
                return Json(
                new
                {
                    success = false,
                    text = "Bạn đã phản hồi nội dung rỗng"
                },
                JsonRequestBehavior.AllowGet);
            }

            db.Feeds.Add( new FeedbackModel { Content = content });
            db.SaveChanges();

            return Json(
                new
                {
                    success = true,
                    text = "Cảm ơn bạn đã gửi phản hồi về cho chúng tôi." +
                    " Chúng tôi sẽ cải thiện website tốt hơn dựa trên phản hồi từ bạn. ^^"
                },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Set cookie stored the last accessed fontId
        /// </summary>
        /// <param name="controller">context</param>
        /// <param name="fontId">the fontId will be stored into cookie</param>
        /// <param name="timeLife">the life-time of the cookie in days</param>
        private void SetCookie(Controller controller, int fontId, int timeLife)
        {
            controller.Response.Cookies.Add(new HttpCookie("fontId", fontId.ToString()));
            controller.Response.Cookies["fontId"].Expires = DateTime.Now.AddDays(30);
        }

        /// <summary>
        /// Get the cookie
        /// </summary>
        private int? CheckCookie(Controller controller)
        {
            var cookieFontId = controller.Request.Cookies["fontId"];
            if (cookieFontId == null)
                return null;
            try
            {
                return int.Parse(cookieFontId.Value);
            }
            catch (Exception)
            {
                return null;
            }            
        }
    }
}