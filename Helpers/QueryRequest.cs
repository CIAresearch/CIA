using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CIAResearch.Helpers
{
    public class QueryRequest
    {
        public Login Login { get; set; }
        public Client Client { get; set; }

        [XmlArrayItem( "TrackingNumber" )]
        public List<string> QueryIDs { get; set; }
    }
}
