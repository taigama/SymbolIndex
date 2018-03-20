using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SymbolIndex.Models
{
    [System.Serializable]
    public class TagSymbolSimple
    {
        public int Tag_Id { get; set; }
        public int Symbol_Id { get; set; }
    }
}