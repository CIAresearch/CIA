using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CIAResearch.Helpers;
using CIAResearch.Utilities;
using NReco.PdfGenerator;
using RestSharp;
using RestSharp.Authenticators;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace CIAResearch
{
    [Description( "CIA Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "CIA" )]

    [TextField( "UserName", "Your CIA authentication UserName", order: 0 )]
    [EncryptedTextField( "Password", "CIA authentication Password", isPassword: true, order: 1 )]

    [TextField( "Client Name", "Background check client account name", order: 2 )]
    [TextField( "Branch Name", "Client branch", required: false, order: 4 )]
    [TextField( "Client Contact", "Point of contact for your organization", order: 5 )]
    [TextField( "Client Contact Email", "Email for the point of contact for your organization", order: 6 )]

    [IntegerField( "Expiration Days", "The number of days to keep a background check open until expiring it.", defaultValue: 30, order: 7 )]
    public class CIAResearch : BackgroundCheckComponent
    {
        #region BackgroundCheckComponent Implementation

        /// <summary>
        /// Sends a background request to CIA.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not.
        /// </returns>
        public override bool SendRequest( RockContext rockContext, Rock.Model.Workflow workflow,
                    AttributeCache personAttribute, AttributeCache ssnAttribute, AttributeCache requestTypeAttribute,
                    AttributeCache billingCodeAttribute, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                Person person;
                int? personAliasId;
                if ( !GetPerson( rockContext, workflow, personAttribute, out person, out personAliasId, errorMessages ) )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", "Unable to get Person." );
                    return true;
                }

                var package = GetPackageName( rockContext, workflow, requestTypeAttribute, errorMessages );
                string previousTrackingNumber = string.Empty;
                //Only pull old tracking number for refresh if package is not specified
                if ( package == null )
                {
                    previousTrackingNumber = GetLastTrackingNumber( person );
                }

                if ( package == null && previousTrackingNumber.IsNullOrWhiteSpace() )
                {
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", "Unable to find background check package." );
                    return true;
                }

                string ssn = Encryption.DecryptString( workflow.GetAttributeValue( ssnAttribute.Key ) );

                string campusName = "";
                var campusGuid = workflow.GetAttributeValue( billingCodeAttribute.Key );
                var campus = CampusCache.Get( campusGuid );
                if ( campus != null )
                {
                    campusName = campus.Name;
                    if ( campusName.Length > 20 )
                    {
                        campusName = campusName.Substring( 0, 20 );
                    }
                }

                var emailBody = workflow.GetAttributeValue( "EmailBody" );
                if ( emailBody.IsNullOrWhiteSpace() )
                {
                    emailBody = "";
                }

                string trackingNumber;
                var haveConsent = workflow.GetAttributeValue( "HaveConsent" ).AsBoolean();
                var packageName = package?.GetAttributeValue( "PackageName" );
                var orderedBy = workflow.InitiatorPersonAlias?.Person;

                var requestOptions = new RequestOptions
                {
                    PackageName = packageName,
                    CampusName = campusName,
                    SSN = ssn,
                    OrderedBy = orderedBy,
                    Person = person,
                    EmailBody = emailBody,
                    PreviousTrackingNumber = previousTrackingNumber
                };

                if ( previousTrackingNumber.IsNotNullOrWhiteSpace() )
                {
                    if ( !CreateNewRequest( requestOptions, out trackingNumber, errorMessages ) )
                    {
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", $"Was not able to create background check. {string.Join( " ", errorMessages )}" );
                        return true;
                    }
                }
                else if ( !haveConsent && package != null )
                {
                    if ( !CreateNewEConsent( requestOptions, out trackingNumber, errorMessages ) )
                    {
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", $"Was not able to create background check. {string.Join( " ", errorMessages )}" );
                        return true;
                    }
                }
                else
                {
                    if ( package.GetAttributeValue( "IsSsnRequired" ).AsBoolean() && !SsnValid( ssn ) )
                    {
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", "Package requires a valid SSN." );
                        return true;
                    }

                    if ( !CreateNewRequest( requestOptions, out trackingNumber, errorMessages ) )
                    {
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", "Was not able to create background check." );
                        return true;
                    }
                }


                workflow.SetAttributeValue( ssnAttribute.Key, "" );
                workflow.SaveAttributeValue( ssnAttribute.Key );

                using ( var newRockContext = new RockContext() )
                {
                    var backgroundCheckService = new BackgroundCheckService( newRockContext );
                    var backgroundCheck = backgroundCheckService.Queryable()
                            .Where( c =>
                                c.WorkflowId.HasValue &&
                                c.WorkflowId.Value == workflow.Id )
                            .FirstOrDefault();

                    if ( backgroundCheck == null )
                    {
                        backgroundCheck = new Rock.Model.BackgroundCheck();
                        backgroundCheck.WorkflowId = workflow.Id;
                        backgroundCheckService.Add( backgroundCheck );
                    }

                    backgroundCheck.PersonAliasId = personAliasId.Value;
                    backgroundCheck.ProcessorEntityTypeId = EntityTypeCache.Get( typeof( CIAResearch ) ).Id;
                    backgroundCheck.PackageName = packageName;
                    backgroundCheck.RequestDate = RockDateTime.Now;
                    backgroundCheck.RequestId = trackingNumber;
                    newRockContext.SaveChanges();

                    UpdateWorkflowRequestStatus( workflow, newRockContext, "SUCCESS", backgroundCheck.RequestId );
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL", ex.Message );
                return true;
            }
        }



        private bool CreateNewEConsent( RequestOptions requestOptions, out string trackingNumber, List<string> errorMessages )
        {
            trackingNumber = "";

            var eRelease = new NewErelease
            {
                Login = GetLogin(),
                Client = GetClient( "(***USE_PACKAGE_ID***)" ),
                ERelease = new ERelease
                {
                    RefNumber = requestOptions.CampusName,
                    OrderedBy = requestOptions.OrderedBy?.FullName ?? "Unknown",
                    OrderedByEmail = requestOptions.Person?.Email ?? GlobalAttributesCache.Value( "OrganizationEmail" ),
                    Subject = GetSubject( requestOptions ),
                    EmailBody = requestOptions.EmailBody,
                    PackageChoice = requestOptions.PackageName
                }
            };

            var client = new RestClient( "https://www.ciaresearch.com/system/erelease.nsf/(einit)?OpenAgent" );
            var request = new RestRequest( Method.POST );
            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
            request.AddXmlBody( eRelease );
            request.RequestFormat = RestSharp.DataFormat.Xml;
            var response = client.Execute<XMLRequest>( request );
            var data = response.Data;

            if ( !string.IsNullOrWhiteSpace( response.ErrorMessage ) )
            {
                errorMessages.Add( response.ErrorMessage );
                return false;
            }

            if ( !string.IsNullOrWhiteSpace( data.Error ) )
            {
                errorMessages.Add( data.Error );
                return false;
            }

            if ( string.IsNullOrWhiteSpace( data.TrackingNumber ) )
            {
                errorMessages.Add( "No tracking number returned" );
                return false;
            }

            trackingNumber = data.TrackingNumber;
            return true;
        }

        private bool CreateNewRequest( RequestOptions requestOptions, out string trackingNumber, List<string> errorMessages )
        {
            trackingNumber = "";

            var bgcRequest = new NewRequest
            {
                Login = GetLogin(),
                Client = GetClient(),
                BackgroundCheck = new Helpers.BackgroundCheck
                {
                    Subject = GetSubject( requestOptions ),
                    Search = new Search
                    {
                        OrderMore = "Yes",

                        RefNumber = requestOptions.CampusName,
                        OrderedBy = requestOptions.Person?.FullName ?? "Unknown",
                        OrderedByEmail = requestOptions.Person?.Email ?? GlobalAttributesCache.Value( "OrganizationEmail" )
                    }
                }
            };

            if ( requestOptions.PreviousTrackingNumber.IsNotNullOrWhiteSpace() )
            {
                bgcRequest.BackgroundCheck.Search.TrackingNumberPrevious = requestOptions.PreviousTrackingNumber;
            }
            else
            {
                bgcRequest.BackgroundCheck.Search.Type = requestOptions.PackageName;
            }

            var client = new RestClient( "https://www.ciaresearch.com/system/center.nsf/(RequestBackgroundCheck)?OpenAgent" );
            var request = new RestRequest( Method.POST );
            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
            request.AddXmlBody( bgcRequest );
            request.RequestFormat = RestSharp.DataFormat.Xml;
            var response = client.Execute<XMLRequest>( request );
            var data = response.Data;

            if ( !string.IsNullOrWhiteSpace( response.ErrorMessage ) )
            {
                errorMessages.Add( response.ErrorMessage );
                return false;
            }

            if ( !string.IsNullOrWhiteSpace( data.Error ) )
            {
                errorMessages.Add( data.Error );
                return false;
            }

            if ( string.IsNullOrWhiteSpace( data.TrackingNumber ) )
            {
                errorMessages.Add( "No tracking number returned" );
                return false;
            }

            trackingNumber = data.TrackingNumber;
            return true;
        }


        Subject GetSubject( RequestOptions requestOptions )
        {
            var subject = new Subject
            {
                LastName = requestOptions.Person.LastName,
                FirstName = requestOptions.Person.FirstName,
                ContactEmail = requestOptions.Person.Email,
            };

            if ( requestOptions.Person.MiddleName.IsNotNullOrWhiteSpace() )
            {
                subject.MiddleInitial = requestOptions.Person.MiddleName[0].ToString();
            }

            var phone = requestOptions.Person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phone == null )
            {
                phone = requestOptions.Person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            }
            if ( phone != null )
            {
                subject.Phone = phone.Number;
            }

            var address = requestOptions.Person.GetHomeLocation();

            if ( address != null )
            {
                subject.Address = address.Street1 + " " + address.Street2;
                subject.City = address.City;
                subject.State = address.State;
                subject.ZipCode = address.PostalCode;
            }

            if ( requestOptions.Person.BirthDate != null )
            {
                subject.DOB = new DOB
                {
                    DOBYEAR = requestOptions.Person.BirthDate.Value.ToString( "yyyy" ),
                    DOBMONTH = requestOptions.Person.BirthDate.Value.ToString( "MM" ),
                    DOBDAY = requestOptions.Person.BirthDate.Value.ToString( "dd" )
                };
            }

            if ( requestOptions.SSN.IsNotNullOrWhiteSpace() )
            {
                subject.SSN = new SSN
                {
                    SSN1 = requestOptions.SSN.Substring( 0, 3 ),
                    SSN2 = requestOptions.SSN.Substring( 4, 2 ),
                    SSN3 = requestOptions.SSN.Substring( 7, 4 )
                };
            }
            return subject;
        }

        public static bool Validate( Login login, Client client, out string error )
        {
            error = "";

            var validation = new ValidationRequest
            {
                Login = login,
                Client = client
            };

            var restClient = new RestClient( "https://www.ciaresearch.com/system/erelease.nsf/vc?OpenAgent" );
            var request = new RestRequest( Method.POST );
            request.AddXmlBody( validation );
            request.RequestFormat = RestSharp.DataFormat.Xml;
            var response = restClient.Execute<XMLRequest>( request );
            var xmlRequest = response.Data;

            if ( !string.IsNullOrWhiteSpace( xmlRequest.Error ) )
            {
                error = xmlRequest.Error;
                return false;
            }
            return true;
        }

        internal static int GetExpirationDays()
        {
            var attributes = AttributeUtilities.GetSettings( new RockContext() );

            var expiration = AttributeUtilities.GetSettingValue( attributes, "ExpirationDays" ).AsInteger();
            if ( expiration < 1 )
            {
                expiration = 30;
            }

            return expiration;
        }

        internal static Client GetClient( string clientId = null )
        {
            var attributes = AttributeUtilities.GetSettings( new RockContext() );

            var client = new Client
            {
                BranchName = AttributeUtilities.GetSettingValue( attributes, "BranchName" ),
                ClientContact = AttributeUtilities.GetSettingValue( attributes, "ClientContact" ),
                ClientContactEmail = AttributeUtilities.GetSettingValue( attributes, "ClientContactEmail" ),
                ClientName = AttributeUtilities.GetSettingValue( attributes, "ClientName" )
            };
            if ( !string.IsNullOrWhiteSpace( clientId ) )
            {
                client.ClientID = clientId;
            }
            return client;
        }

        internal static Login GetLogin()
        {
            var attributes = AttributeUtilities.GetSettings( new RockContext() );

            return new Login
            {
                UserName = AttributeUtilities.GetSettingValue( attributes, "UserName" ),
                Password = AttributeUtilities.GetSettingValue( attributes, "Password", true )
            };
        }
        #endregion


        #region Internal Methods


        /// <summary>
        /// Updates the workflow.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="recommendation">The recommendation.</param>
        /// <param name="reportLink">The report link.</param>
        /// <param name="reportStatus">The report status.</param>
        /// <param name="rockContext">The rock context.</param>
        internal static void UpdateWorkflow( Rock.Model.BackgroundCheck backgroundCheck, Helpers.Report report, RockContext rockContext )
        {
            var workflowService = new WorkflowService( rockContext );
            var workflow = new WorkflowService( rockContext ).Get( backgroundCheck.WorkflowId ?? 0 );
            if ( workflow != null && workflow.IsActive )
            {
                workflow.LoadAttributes();

                UpdateWorkflowReportStatus( workflow, rockContext, "Review" );

                SaveAttributeValue( workflow, "ReportLink", GenerateReportUrl( backgroundCheck.RequestId ),
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.URL_LINK.AsGuid() ), rockContext );

                SaveAttributeValue( workflow, "Report", backgroundCheck.ResponseDocument.Guid.ToString(),
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() ), rockContext );

                SaveAttributeValue( workflow, "NumberRecords", report.NumberRecords,
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext );

                SaveAttributeValue( workflow, "TotalCost", report.TotalCost,
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext );

                workflow.Status = "Ready For Review";

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );
                    foreach ( var activity in workflow.Activities )
                    {
                        activity.SaveAttributeValues( rockContext );
                    }
                } );
            }

            rockContext.SaveChanges();

            workflowService.Process( workflow, out List<string> workflowErrors );
        }

        public static void ClearReport( Rock.Model.BackgroundCheck backgroundCheck, RockContext rockContext )
        {
            var workflowService = new WorkflowService( rockContext );
            var workflow = new WorkflowService( rockContext ).Get( backgroundCheck.WorkflowId ?? 0 );
            if ( workflow != null && workflow.IsActive )
            {
                workflow.LoadAttributes();
                workflow.Status = "Waiting For Updated Report";

                SaveAttributeValue( workflow, "ReportLink", "",
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.URL_LINK.AsGuid() ), rockContext );

                SaveAttributeValue( workflow, "Report", "",
                       FieldTypeCache.Get( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() ), rockContext );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );
                    foreach ( var activity in workflow.Activities )
                    {
                        activity.SaveAttributeValues( rockContext );
                    }
                } );
            }
        }

        internal static void UpdateWorkflowRequestStatus( Rock.Model.BackgroundCheck backgroundCheck, string status )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                WorkflowService workflowService = new WorkflowService( rockContext );
                var workflow = workflowService.Get( backgroundCheck.WorkflowId ?? 0 );
                if ( workflow != null && workflow.IsActive )
                {
                    workflow.Status = status;
                }
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Sets the workflow requeststatus attribute.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="requestStatus">The request status.</param>
        private void UpdateWorkflowRequestStatus( Workflow workflow, RockContext rockContext, string requestStatus, string requestMessage )
        {
            if ( SaveAttributeValue( workflow, "RequestStatus", requestStatus,
                FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
            }

            if ( SaveAttributeValue( workflow, "RequestMessage", requestMessage,
               FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
            }
        }

        private static void UpdateWorkflowReportStatus( Workflow workflow, RockContext rockContext, string requestStatus )
        {
            if ( SaveAttributeValue( workflow, "ReportStatus", requestStatus,
                FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext, null ) )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get the background check type that the request is for.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="packageName"></param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>True/False value of whether the request was successfully sent or not.</returns>
        private DefinedValueCache GetPackageName( RockContext rockContext, Workflow workflow, AttributeCache requestTypeAttribute, List<string> errorMessages )
        {
            if ( requestTypeAttribute == null )
            {
                return null;
            }

            return DefinedValueCache.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
        }

        private bool GetPerson( RockContext rockContext, Workflow workflow, AttributeCache personAttribute, out Person person, out int? personAliasId, List<string> errorMessages )
        {
            person = null;
            personAliasId = null;
            if ( personAttribute != null )
            {
                Guid? personAliasGuid = workflow.GetAttributeValue( personAttribute.Key ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    person = new PersonAliasService( rockContext ).Queryable()
                        .Where( p => p.Guid.Equals( personAliasGuid.Value ) )
                        .Select( p => p.Person )
                        .FirstOrDefault();
                    person.LoadAttributes( rockContext );
                }
            }

            if ( person == null )
            {
                errorMessages.Add( "This background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                return false;
            }

            personAliasId = person.PrimaryAliasId;
            if ( !personAliasId.HasValue )
            {
                errorMessages.Add( "This background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                return false;
            }

            return true;
        }

        private static bool SaveAttributeValue( Rock.Model.Workflow workflow, string key, string value,
            FieldTypeCache fieldType, RockContext rockContext, Dictionary<string, string> qualifiers = null )
        {
            bool createdNewAttribute = false;

            if ( workflow.Attributes.ContainsKey( key ) )
            {
                workflow.SetAttributeValue( key, value );
            }
            else
            {
                // Read the attribute
                var attributeService = new AttributeService( rockContext );
                var attribute = attributeService
                    .Get( workflow.TypeId, "WorkflowTypeId", workflow.WorkflowTypeId.ToString() )
                    .Where( a => a.Key == key )
                    .FirstOrDefault();

                // If workflow attribute doesn't exist, create it
                // ( should only happen first time a background check is processed for given workflow type)
                if ( attribute == null )
                {
                    attribute = new Rock.Model.Attribute();
                    attribute.EntityTypeId = workflow.TypeId;
                    attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                    attribute.EntityTypeQualifierValue = workflow.WorkflowTypeId.ToString();
                    attribute.Name = key.SplitCase();
                    attribute.Key = key;
                    attribute.FieldTypeId = fieldType.Id;
                    attributeService.Add( attribute );

                    if ( qualifiers != null )
                    {
                        foreach ( var keyVal in qualifiers )
                        {
                            var qualifier = new AttributeQualifier();
                            qualifier.Key = keyVal.Key;
                            qualifier.Value = keyVal.Value;
                            attribute.AttributeQualifiers.Add( qualifier );
                        }
                    }

                    createdNewAttribute = true;
                }

                // Set the value for this attribute
                var attributeValue = new AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = value;
                new AttributeValueService( rockContext ).Add( attributeValue );
            }

            return createdNewAttribute;
        }

        internal static byte[] GetPDF( string trackingNumber )
        {
            var login = GetLogin();
            var client = new RestClient( GenerateReportUrl( trackingNumber ) );
            client.Authenticator = new HttpBasicAuthenticator( login.UserName, login.Password );
            var request = new RestRequest( Method.GET );
            var response = client.Execute( request );
            var html = response.Content;

            var htmlToPdf = new HtmlToPdfConverter();
            return htmlToPdf.GeneratePdf( html );
        }

        public static string GenerateReportUrl( string reportKey )
        {
            return "https://www.ciaresearch.com/system/center.nsf/BackgroundFullReport?OpenForm&HIDEPII=Y&TNUM=" + reportKey;
        }

        public override string GetReportUrl( string reportKey )
        {
            return GetReportUrl( reportKey );
        }

        public static void CancelBackgroundCheck( int backgroundCheckId, string status )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                BackgroundCheckService backgroundCheckService = new BackgroundCheckService( rockContext );
                var backgroundCheck = backgroundCheckService.Get( backgroundCheckId );

                backgroundCheck.ResponseDate = RockDateTime.Today;
                backgroundCheck.Status = status;
                if ( backgroundCheck.Workflow != null )
                {
                    backgroundCheck.Workflow.MarkComplete( status );
                }
                rockContext.SaveChanges();
            }
        }

        private string GetLastTrackingNumber( Person person )
        {
            RockContext rockContext = new RockContext();
            var backgroundCheckService = new BackgroundCheckService( rockContext );

            var bgCheck = backgroundCheckService.Queryable()
                .Where( b => b.PersonAlias.PersonId == person.Id )
                .OrderByDescending( b => b.ResponseDate )
                .FirstOrDefault();

            if ( bgCheck == null )
            {
                return null;
            }

            return bgCheck.RequestId;
        }


        public void RefreshPackages()
        {
            var attributes = AttributeUtilities.GetSettings( new RockContext() );


            var clientName = AttributeUtilities.GetSettingValue( attributes, "ClientName" );
            var branchName = AttributeUtilities.GetSettingValue( attributes, "BranchName" );

            var xmlSOAP = $@"<soapenv:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:urn=""urn:DefaultNamespace"">
   <soapenv:Header/>
   <soapenv:Body>
      <urn:GETSERVICESLISTDATA soapenv:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
         <CLIENTID xsi:type=""xsd:string"">{clientName}_{branchName}</CLIENTID>
      </urn:GETSERVICESLISTDATA>
   </soapenv:Body>
</soapenv:Envelope>";


            var task = Task.Run( async () =>
            {
                var url = "https://www.ciaresearch.com:443/system/center.nsf/RetrievePackagesWS?OpenWebService";
                return await PostSOAPRequestAsync( url, xmlSOAP );
            } );
            var result = task.Result;

            //I really tried to do this the right way, but it fought me the entire time.
            //Instead of parsing the xml I'm just going right for what I actually need.
            int pFrom = result.IndexOf( "<GETSERVICESLISTDATAReturn xsi:type=\"xsd:string\">" ) + "<GETSERVICESLISTDATAReturn xsi:type=\"xsd:string\">".Length;
            int pTo = result.LastIndexOf( "</GETSERVICESLISTDATAReturn>" );

            var xresult = result.Substring( pFrom, pTo - pFrom );

            var contentXML = System.Net.WebUtility.HtmlDecode( xresult );
            var services = FromXML<ServicesList>( contentXML );

            UpdateDefinedValues( services );
        }

        private void UpdateDefinedValues( ServicesList services )
        {
            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            var definedTypeGuid = new Guid( Constants.DEFINED_TYPE );
            var definedValues = definedValueService.Queryable().Where( dv => dv.DefinedType.Guid == definedTypeGuid ).ToList();

            var toRemove = definedValues.Where( dv => !services.Service.Select( s => s.Name ).Contains( dv.Value ) ).ToList();
            foreach ( var item in toRemove )
            {
                definedValueService.Delete( item );
                rockContext.SaveChanges();
            }

            foreach ( var service in services.Service )
            {
                var definedValue = definedValues.FirstOrDefault( dv => dv.Value == service.Name );

                if ( definedValue == null )
                {
                    definedValue = new DefinedValue
                    {
                        DefinedTypeId = DefinedTypeCache.Get( definedTypeGuid ).Id,
                        Value = service.Name,
                        IsActive = true
                    };
                    definedValueService.Add( definedValue );

                    rockContext.SaveChanges();
                }

                var ssnRequired = service.FieldRequirements.Subject.FieldRequirement
                    .Any( r => r.Property == "SSN1" && r.Required == 1 );

                definedValue.LoadAttributes();
                definedValue.SetAttributeValue( "IsEConsent", "True" );
                definedValue.SetAttributeValue( "IsSsnRequired", ssnRequired ? "True" : "False" );
                definedValue.SetAttributeValue( "PackageName", service.PackageInfo.PackageID );
                definedValue.SaveAttributeValues();
            }
        }

        private bool SsnValid( string ssn )
        {
            return ssn.IsNotNullOrWhiteSpace() && Regex.IsMatch( ssn, "^\\d{3}-\\d{2}-\\d{4}$" );
        }

        private static async Task<string> PostSOAPRequestAsync( string url, string text )
        {
            var httpClient = new HttpClient();
            using ( HttpContent content = new StringContent( text, Encoding.UTF8, "text/xml" ) )
            using ( HttpRequestMessage request = new HttpRequestMessage( HttpMethod.Post, url ) )
            {
                request.Headers.Add( "SOAPAction", "GETSERVICESLISTDATA" );
                request.Content = content;
                using ( HttpResponseMessage response = await httpClient.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ) )
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private T FromXML<T>( string xml )
        {
            var serializer = new System.Xml.Serialization.XmlSerializer( typeof( T ) );
            using ( StringReader reader = new StringReader( xml ) )
            {
                return ( T ) serializer.Deserialize( reader );
            }
        }


        #endregion
    }

    class RequestOptions
    {
        public Person Person { get; set; }
        public string SSN { get; set; }
        public string CampusName { get; set; }
        public string PackageName { get; set; }
        public Person OrderedBy { get; set; }
        public string EmailBody { get; set; }
        public string PreviousTrackingNumber { get; set; }
    }
}