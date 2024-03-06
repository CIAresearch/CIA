namespace CIAResearch.Helpers
{
    public class ERelease
    {
        public string EmailBody { get; set; }
        public string TrackingNumber { get; set; }
        public string RefNumber { get; set; }
        public string OrderedBy { get; set; }
        public string OrderedByEmail { get; set; }
        public Subject Subject { get; set; }
        public string PackageChoice { get; set; }
        public string afterRelease { get; set; } = "QU";
        public string releasetype { get; set; } = "Volunteer";
    }
}
