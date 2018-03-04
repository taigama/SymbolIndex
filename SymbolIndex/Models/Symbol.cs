using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SymbolIndex.Models
{
    public class Symbol : BaseModel
    {
        public string Content { get; set; }
        public int FontId { get; set; }

        public virtual IList<Tag> Tags { get; set; }
        public virtual Font Font { get; set; }
    }
}