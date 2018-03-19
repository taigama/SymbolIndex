﻿using System;
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
            //var dict = ListFont(3);


            return View();
        }

        public ActionResult FontList()
        {
            return View(db.Fonts.ToList());
        }

        [HttpGet]
        public ActionResult ViewFont(int? Id)
        {
            var font = db.Fonts.Find(Id ?? 3);
            ViewBag.Symbols = font.Symbols;
            ViewBag.FontCurrent = font;

            return View();
        }

        [HttpPost]
        public ActionResult ViewFont(int? Id, bool isOverwite = false)
        {
            ListFont(Id, isOverwite);

            var font = db.Fonts.Find(Id ?? 3);
            ViewBag.Symbols = font.Symbols;
            ViewBag.FontCurrent = font;

            return View();
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

        public void ListFont(int? fontId, bool? isOverwite = false)
        {
            if (fontId == null)
                return; 

            // info of the font in my database, just a name of font and url of that font
            var fontStoredInfo = db.Fonts.Find(fontId);
            

            string customStr = Server.MapPath("~/fonts/") +  fontStoredInfo.FontUrl.Replace("fonts/", "");
            //uri = new Uri();
            // if the uri is invalid, or the font not found, the web will become not responding
            var families = Fonts.GetFontFamilies(customStr);
            ViewBag.fontCount = families.Count;

            //List<KeyValuePair<int, ushort>> lst = new List<KeyValuePair<int, ushort>>();
            //Dictionary<int, ushort> dict = new Dictionary<int, ushort>();
            //var symbols = fontStoredInfo.Symbols.ToList();

            List<Symbol> newSymbols = new List<Symbol>(); 

            foreach (FontFamily family in families)
            {
                var typefaces = family.GetTypefaces();

                GlyphTypeface glyph;
                typefaces.ElementAt(0).TryGetGlyphTypeface(out glyph);
                IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;

                foreach (KeyValuePair<int, ushort> kvp in characterMap)
                {
                    //lst.Add(new KeyValuePair<int, ushort>(kvp.Key, kvp.Value));
                    newSymbols.Add(new Symbol
                    {
                        FontId = fontId.Value,
                        Content = "" + Convert.ToChar(kvp.Value)
                    });
                }

                fontStoredInfo.Symbols = newSymbols;
                db.Entry(fontStoredInfo).State = System.Data.Entity.EntityState.Modified;
            }
        }


    }
}