using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class NewRequest
    {
        public Login Login { get; set; }
        public Client Client { get; set; }
        public BackgroundCheck BackgroundCheck { get; set; }
    }
}