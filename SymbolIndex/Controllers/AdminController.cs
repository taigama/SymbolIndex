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

            var fontStoredInfo = db.Fonts.Find(fontId);

            var urlBuilder =
                new System.UriBuilder(Request.Url.AbsoluteUri)
                {
                    Path = fontStoredInfo.FontUrl,
                    Query = null,
                };
            Uri uri = urlBuilder.Uri;
            //string url = urlBuilder.ToString();

            var families = Fonts.GetFontFamilies(uri);


            Dictionary<int, ushort> dict = new Dictionary<int, ushort>();
            
            foreach (FontFamily family in families)
            {
                var typefaces = family.GetTypefaces();
                foreach (Typeface typeface in typefaces)
                {
                    GlyphTypeface glyph;
                    typeface.TryGetGlyphTypeface(out glyph);
                    IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;

                    foreach (KeyValuePair<int, ushort> kvp in characterMap)
                    {
                        Console.WriteLine(String.Format("{0}:{1}", kvp.Key, kvp.Value));
                        dict.Add(kvp.Key, kvp.Value);
                    }

                }
            }

            return dict;
        }
    }
}