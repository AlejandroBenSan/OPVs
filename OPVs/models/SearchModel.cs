using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPVs.models
{
    internal class SearchModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public SearchModel()
        {
            StartDate = DateTime.Today.AddDays(-1);
            EndDate = DateTime.Today;
        }
    }
}
