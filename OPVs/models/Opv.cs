using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPVs.models
{
    public class Opv
    {
        public DateTime fecha { get; set; }
        public string empresa { get; set; }
        public string mercado { get; set; }
        public string valor { get; set; }
        public float precioSalida { get; set; }
        public float precioUltimoCierre { get; set; }

    }
}
