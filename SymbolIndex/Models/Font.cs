using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SymbolIndex.Models
{
    public class Font : BaseModel
    {
        public string FontName { get; set; }
        public string FontUrl { get; set; }

        public virtual IList<Symbol> Symbols { get; set; }
    }
}