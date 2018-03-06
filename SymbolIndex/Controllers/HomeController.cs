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


        public ActionResult Index(int? fontId)
        {
            var fonts = db.Fonts.ToList();
            ViewBag.Fonts = fonts;
            ViewBag.Symbols = fonts.ElementAt(fontId ?? 0).Symbols;
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