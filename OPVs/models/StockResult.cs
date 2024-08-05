using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPVs.models
{
    internal class StockResult
    {
        public long t { get; set; }
        public float o { get; set; }
        public float c { get; set; }
        public float h { get; set; }
        public float l { get; set; }
        public float vw { get; set; }
        public int v { get; set; }
        public int n { get; set; }
    }
}
