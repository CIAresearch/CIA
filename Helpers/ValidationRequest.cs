using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class ValidationRequest
    {
        public Login Login { get; set; }
        public string Source { get; set; } = "RockRMS";
        public Client Client { get; set; }
    }
}
