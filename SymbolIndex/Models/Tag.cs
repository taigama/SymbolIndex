using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SymbolIndex.Models
{
    public class Tag : BaseModel
    {
        public string TagString { get; set; }

        public virtual IList<Symbol> Symbols { get; set; }

        public override string ToString()
        {
            return TagString;
        }
    }
}