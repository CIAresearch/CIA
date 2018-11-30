using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class Report
    {
        public string TrackingNumber { get; set; }
        public string DateRequested { get; set; }
        public string TimeRequested { get; set; }
        public string ReportStatus { get; set; }
        public string ReportStatusText { get; set; }
        public string NumberRequests { get; set; }
        public string NumberRequestsCompleted { get; set; }
        public string NumberRecords { get; set; }
        public string TotalCost { get; set; }

    }
}
