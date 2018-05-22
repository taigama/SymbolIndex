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
        public ActionResult Index(int? fontId = 3)
        {
            var fonts = db.Fonts.ToList();
            ViewBag.Fonts = fonts;

            Font font = fonts.Find(x=>x.Id == fontId);
            ViewBag.Symbols = null;
            ViewBag.FontCurrent = font;
            return View();
        }

        [HttpPost]
        public ActionResult Index(int? fontId = 3, string key = "")
        {
            var fonts = db.Fonts.ToList();
            ViewBag.Fonts = fonts;
            var font = db.Fonts.Find(fontId ?? 1);
            ViewBag.FontCurrent = font;

            List<Tag> tagsSearched = db.Tags.Where(x => x.TagString.Contains(key)).ToList();
            List<Symbol> symbols = new List<Symbol>();
            foreach(Tag tag in tagsSearched)
            {
                symbols.AddRange(tag.Symbols);
            }
            symbols = symbols.Distinct().ToList();

            //          var queryList = _db.Laundries
            //.Include(l => l.LaundryMachines)
            //.Include(l => l.LaundryMachines.Select(lm => lm.Reservations))
            //.ToList();
            symbols.RemoveAll(x => x.FontId != fontId);


            ViewBag.Symbols = symbols;
            return View();
        }
        
        public ActionResult _FontStyle(int? fontId = 3)
        {
            var font = db.Fonts.Find(fontId);
            ViewBag.FontCurrent = font;

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
    }
}