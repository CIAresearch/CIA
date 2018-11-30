using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class XMLRequest
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string TrackingNumber { get; set; }
        public string Error { get; set; }
        public List<Report> Reports { get; set; }
    }
}
