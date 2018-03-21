using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpGet]
        public ActionResult ViewFont(int Id = 4)
        {
            var font = db.Fonts.Find(Id);
            if(font == null)
            {
                return HttpNotFound("font id was not found");
            }


            ViewBag.Symbols = font.Symbols;
            ViewBag.FontCurrent = font;
            return View();
        }

        [HttpPost]
        public ActionResult AddSymbols(int fontId, List<string> symbolsInXUTF)
        {
            var font = db.Fonts.Find(fontId);
            if (font == null)
            {
                return HttpNotFound("font id was not found");
            }

            foreach (var child in symbolsInXUTF)
            {
                if (child == null)
                    continue;

                db.Symbols.Add(
                    new Symbol
                    {
                        FontId = fontId,
                        Content = child
                    });
            }
            db.SaveChanges();

            ViewData["fontCurrent"] = font;
            ViewData["symbols"] = font.Symbols.ToList();
            return PartialView("_FontSymbolsTBody");
        }

        [HttpPost]
        public ActionResult ClearFontSymbols(int fontId)
        {
            var font = db.Fonts.Find(fontId);
            if (font == null)
            {
                return HttpNotFound("font id was not found");
            }

            try
            {
                var willBeDeletedSymbols = db.Symbols.Where(x => x.FontId == fontId).ToList();
                if (willBeDeletedSymbols != null && willBeDeletedSymbols.Count > 0)
                {
                    db.Symbols.RemoveRange(willBeDeletedSymbols);
                    db.SaveChanges();
                }
                return Json( new { success = true, message = "Clear complete!"}, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return new HttpStatusCodeResult(500 ,e.InnerException.Message);
            }
        }

        public ActionResult FontList()
        {
            return View(db.Fonts.ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname = String.Empty;
                        string fpath = String.Empty;
                        string fstorePath = String.Empty;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  

                        do
                        {
                            fpath = RandomString(10) + fname;
                            fstorePath = Path.Combine(Server.MapPath("~/fonts/"), fpath);
                            fpath = @"~/UploadFiles/" + fpath;
                        } while (System.IO.File.Exists(fstorePath));

                        file.SaveAs(fstorePath);
                    }

                    
                    return Json("fpath", JsonRequestBehavior.AllowGet);
                }
                catch
                {
                    return Json("SOMETHING_NOT_RIGHT", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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