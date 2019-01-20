using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIAResearch.Helpers
{
    public class CIAAccessRequest
    {
        public string RequestType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientName { get; set; }
        public string BranchName { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientEmail2 { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string ReferredBy { get; set; } = "Rock RMS";
    }
}
