using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaliBotDotNet.Models
{
    public class AlternativeFact
    {
        public int FactID { get; set; }
        public string Description { get; set; }
        public ulong AuthorID { get; set; }
    }
}
