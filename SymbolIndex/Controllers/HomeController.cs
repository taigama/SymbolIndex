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

            var font = db.Fonts.Find(fontId ?? 1);
            ViewBag.Symbols = font.Symbols;
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}