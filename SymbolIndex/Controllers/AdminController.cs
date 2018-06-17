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
        private static readonly string TEXT_LOCATION = "~/App_Data/";
        private static readonly string CREDIT_FILE_NAME = "credits";

        SIContext db = new SIContext();

        #region Views

        [HttpGet]
        public ActionResult Login(string url)
        {
            if(Verifier.CheckLogin(Request))
            {
                return Redirect(url);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string url, string username, string password)
        {
            if(Verifier.Login(username, password, Response))
            {
                return Redirect(url);
            }
            else
            {
                ViewData["msg"] = "Login failed!";
                return View();
            }

        }

        public ActionResult Index()
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            return View(db.Feeds.ToList());
        }


        public ActionResult FontList()
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            return View(db.Fonts.ToList());
        }

        [HttpGet]
        public ActionResult ViewFont(int Id = 4)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            var font = db.Fonts.Find(Id);
            if(font == null)
            {
                return HttpNotFound("font id was not found");
            }


            ViewBag.Symbols = font.Symbols;
            ViewBag.FontCurrent = font;
            return View();
        }

        #endregion

        #region Post & Partial
        #region of-ViewFont
        [HttpPost]
        public ActionResult AddSymbols(int fontId, List<string> symbolsInXUTF)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

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
            ViewData["symbols"] = db.Symbols.Where(x => x.FontId == font.Id).ToList();
            return PartialView("_FontSymbolsTBody");
        }

        [HttpPost]
        public ActionResult ClearFontSymbols(int fontId)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

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

        [HttpPost]
        public ActionResult CommitTags(int fontId, [ModelBinder(typeof(ViewSymbolsBinder))] List<SymbolView> viewSymbols)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            var font = db.Fonts.Find(fontId);
            if (font == null)
            {
                return HttpNotFound("font id was not found");
            }

            List<string> strsTemp;
            var tagsInDb = db.Tags.ToList();
            var symbols = font.Symbols.ToList();
            Symbol symTemp;
            Tag tagTemp;

            foreach (var child in viewSymbols)
            {
                if (string.IsNullOrEmpty(child.Tags))
                    continue;

                symTemp = symbols.Find(x => x.Id == child.Sid);
                if (symTemp == null)
                    continue;

                if(symTemp.Tags == null)
                {
                    symTemp.Tags = new List<Tag>();
                }
                else if(symTemp.Tags.Count > 0)
                {
                    symTemp.Tags.Clear();
                }


                strsTemp = child.Tags.Split(',').Select(s => s.Trim()).Distinct().ToList();
                foreach(var tagInClient in strsTemp)
                {
                    tagTemp = tagsInDb.Find(x => x.TagString == tagInClient);
                    if(tagTemp == null)
                    {
                        tagTemp = new Tag()
                        {
                            TagString = tagInClient
                        };

                        db.Tags.Add(tagTemp);
                        db.SaveChanges();
                    }
                    
                    symTemp.Tags.Add(tagTemp);
                }


                db.Entry(symTemp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                tagsInDb = db.Tags.ToList();
            }


            ViewData["fontCurrent"] = font;
            ViewData["symbols"] = db.Symbols.Where(x => x.FontId == font.Id).ToList();
            return PartialView("_FontSymbolsTBody");
        }


        [HttpPost]
        public ActionResult DeleteSymbol(int? Id)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            if (Id == null)
            {
                return HttpNotFound("symbol id can not be null");
            }

            var symbol = db.Symbols.Find(Id);
            if (symbol == null)
            {
                return HttpNotFound("symbol id was not found");
            }

            var font = symbol.Font;
            ViewData["fontCurrent"] = font;

            db.Symbols.Remove(symbol);
            db.SaveChanges();

            ViewData["symbols"] = db.Symbols.Where(x => x.FontId == font.Id).ToList();
            return PartialView("_FontSymbolsTBody");
        }

        #endregion

        #region of-ListFont
        [HttpPost]
        public ActionResult UploadFont()
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                //  Get all files from Request object  
                HttpFileCollectionBase files = Request.Files;
                string name = Request.Form.Get("name");

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

                    fpath = /*RandomString(10) +*/ fname;
                    fstorePath = Path.Combine(Server.MapPath("~/fonts/"), fpath);
                    fpath = @"/fonts/" + fpath;
                    if(System.IO.File.Exists(fstorePath))
                    {
                        Response.StatusCode = 400;
                        Response.StatusDescription = "font has exist!";
                        return null;
                    }

                    file.SaveAs(fstorePath);
                    Font fontInfo = new Font
                    {
                        FontName = name,
                        FontUrl = fpath
                    };
                    db.Fonts.Add(fontInfo);
                    db.SaveChanges();
                }

                return PartialView("_FontsTBody", db.Fonts.ToList());
            }
            else
            {
                Response.StatusCode = 400;
                return PartialView("_FontsTBody", db.Fonts.ToList());
            }
        }

        [HttpPost]
        public ActionResult DeleteFont(int? id)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            if (id == null)
            {
                Response.StatusCode = 400;
                Response.StatusDescription = "must submit id!";
                return null;
            }

            Font fontInfo = db.Fonts.Find(id);
            if (fontInfo == null)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = "the font with id: " + id.ToString() +" was not found!";
                return null;
            }

            string fullPath = Request.MapPath("~" + fontInfo.FontUrl);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            db.Fonts.Remove(fontInfo);
            db.SaveChanges();


            return PartialView("_FontsTBody", db.Fonts.ToList());
        }

        #endregion


        #endregion



        //public static string RandomString(int length)
        //{
        //    Random random = new Random();
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //    return new string(Enumerable.Repeat(chars, length)
        //      .Select(s => s[random.Next(s.Length)]).ToArray());
        //}

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

        [HttpGet]
        public ActionResult Credits()
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            string content = "";

            string storePath = Path.Combine(Server.MapPath(TEXT_LOCATION), CREDIT_FILE_NAME);

            if (System.IO.File.Exists(storePath))
            {
                try
                {
                    using (TextReader tr = new StreamReader(storePath))
                    {
                        string line = "";
                        while ((line = tr.ReadLine()) != null)
                        {
                            content += line + "\n";
                        }
                    }
                }
                catch (Exception)
                {
                    content = "CAN NOT OPEN THE FILE";
                }
            }

            ViewData["content"] = content;
            return View();
        }

        [HttpPost]
        public ActionResult Credits(string content)
        {
            if (!Verifier.CheckLogin(Request))
            {
                return RedirectToAction("Login", new { url = Request.Url.AbsoluteUri });
            }

            string storePath = Path.Combine(Server.MapPath(TEXT_LOCATION), CREDIT_FILE_NAME);            
            if (System.IO.File.Exists(storePath))
            {
                System.IO.File.Delete(storePath);
            }

            try
            {
                System.IO.File.Create(storePath).Dispose();

                using (StreamWriter sw = new StreamWriter(storePath))
                {
                    sw.Write(content);
                }
            }
            catch (Exception)
            {
                content = "CAN NOT OPEN THE FILE";
            }

            ViewData["content"] = content;
            return View();
        }
    }
}