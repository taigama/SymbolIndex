using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SymbolIndex.Models
{
    public class SymbolView
    {
        public int Sid { get; set; }
        public string OName { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
    }
}