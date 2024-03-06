using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CIAResearch.Helpers
{
    [XmlRoot( ElementName = "PackageInfo" )]
    public class PackageInfo
    {

        [XmlElement( ElementName = "PackageID" )]
        public string PackageID { get; set; }
    }

    [XmlRoot( ElementName = "FieldRequirement" )]
    public class FieldRequirement
    {

        [XmlElement( ElementName = "DataType" )]
        public string DataType { get; set; }

        [XmlElement( ElementName = "Property" )]
        public string Property { get; set; }

        [XmlElement( ElementName = "DisplayName" )]
        public string DisplayName { get; set; }

        [XmlElement( ElementName = "Required" )]
        public int Required { get; set; }
    }

    [XmlRoot( ElementName = "Subject" )]
    public class PRSubject
    {

        [XmlElement( ElementName = "FieldRequirement" )]
        public List<FieldRequirement> FieldRequirement { get; set; }
    }

    [XmlRoot( ElementName = "Search" )]
    public class PRSearch
    {

        [XmlElement( ElementName = "FieldRequirement" )]
        public List<FieldRequirement> FieldRequirement { get; set; }
    }

    [XmlRoot( ElementName = "FieldRequirements" )]
    public class FieldRequirements
    {

        [XmlElement( ElementName = "Subject" )]
        public PRSubject Subject { get; set; }

        [XmlElement( ElementName = "Search" )]
        public PRSearch Search { get; set; }
    }

    [XmlRoot( ElementName = "Service" )]
    public class Service
    {

        [XmlElement( ElementName = "PackageInfo" )]
        public PackageInfo PackageInfo { get; set; }

        [XmlElement( ElementName = "FieldRequirements" )]
        public FieldRequirements FieldRequirements { get; set; }

        [XmlAttribute( AttributeName = "Type" )]
        public string Type { get; set; }

        [XmlAttribute( AttributeName = "Name" )]
        public string Name { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot( ElementName = "ServicesList" )]
    public class ServicesList
    {

        [XmlElement( ElementName = "Service" )]
        public List<Service> Service { get; set; }
    }


}
