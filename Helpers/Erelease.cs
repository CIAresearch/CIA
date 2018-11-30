using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class ERelease
    {
        public string TrackingNumber { get; set; }
        public string RefNumber { get; set; }
        public string OrderedBy { get; set; }
        public string OrderedByEmail { get; set; }
        public Subject Subject { get; set; }
    }
}
