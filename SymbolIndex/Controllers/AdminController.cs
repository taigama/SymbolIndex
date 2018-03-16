using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Windows.Media;
using SymbolIndex.Data;
using SymbolIndex.Models;

namespace SymbolIndex.Controllers
{
    public class AdminController : Controller
    {
        SIContext db = new SIContext();

        // GET: Admin
        public ActionResult Index()
        {
            var dict = ListFont(3);


            return View();
        }


        public IDictionary<int, ushort> ListFont(int? fontId)
        {
            if (fontId == null)
                return null;

            // info of the font in my database, just a name of font and url of that font
            var fontStoredInfo = db.Fonts.Find(fontId);
            

            string customStr = Server.MapPath("~/fonts/") +  fontStoredInfo.FontUrl.Replace("fonts/", "");
            //uri = new Uri();
            // if the uri is invalid, or the font not found, the web will become not responding
            var families = Fonts.GetFontFamilies(customStr);
            ViewBag.fontCount = families.Count;

            Dictionary<int, ushort> dict = new Dictionary<int, ushort>();
            
            foreach (FontFamily family in families)
            {
                var typefaces = family.GetTypefaces();

                GlyphTypeface glyph;
                typefaces.ElementAt(0).TryGetGlyphTypeface(out glyph);
                IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;

                foreach (KeyValuePair<int, ushort> kvp in characterMap)
                {
                    Console.WriteLine(String.Format("{0}:{1}", kvp.Key, kvp.Value));
                    dict.Add(kvp.Key, kvp.Value);
                }
            }

            return dict;
        }
    }
}