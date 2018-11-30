using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using System.ComponentModel.Composition;
using Rock;
using CIAResearch.Helpers;
using RestSharp;
using CIAResearch.Utilities;
using RestSharp.Authenticators;
using NReco.PdfGenerator;

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
                    errorMessages.Add( "Unable to get Person." );
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                    return true;
                }


                DefinedValueCache package;
                if ( !GetPackageName( rockContext, workflow, requestTypeAttribute, out package, errorMessages ) )
                {
                    errorMessages.Add( "Unable to find background check package." );
                    UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
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

                string trackingNumber;
                var isEConsent = package.GetAttributeValue( "IsEConsent" ).AsBoolean();
                var packageName = package.GetAttributeValue( "PackageName" );
                var orderedBy = workflow.InitiatorPersonAlias.Person;


                if ( isEConsent )
                {
                    if ( !CreateNewEConsent( person, ssn, campusName, packageName, orderedBy, out trackingNumber, errorMessages ) )
                    {
                        errorMessages.Add( "Was not able to create background check." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                        return true;
                    }
                }
                else
                {
                    if ( !CreateNewRequest( person, ssn, campusName, packageName, orderedBy, out trackingNumber, errorMessages ) )
                    {
                        errorMessages.Add( "Was not able to create background check." );
                        UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
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

                    UpdateWorkflowRequestStatus( workflow, newRockContext, "SUCCESS" );
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                UpdateWorkflowRequestStatus( workflow, rockContext, "FAIL" );
                return true;
            }
        }

        private bool CreateNewEConsent( Person person, string ssn, string campusName, string packageName, Person orderedBy, out string trackingNumber, List<string> errorMessages )
        {
            trackingNumber = "";

            var eRelease = new NewErelease
            {
                Login = GetLogin(),
                Client = GetClient( packageName ),
                ERelease = new ERelease
                {
                    RefNumber = campusName,
                    OrderedBy = orderedBy.FullName,
                    OrderedByEmail = orderedBy.Email,
                    Subject = GetSubject( person, ssn )
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

        private bool CreateNewRequest( Person person, string ssn, string campusName, string packageName, Person orderedBy, out string trackingNumber, List<string> errorMessages )
        {
            trackingNumber = "";

            var bgcRequest = new NewRequest
            {
                Login = GetLogin(),
                Client = GetClient(),
                BackgroundCheck = new Helpers.BackgroundCheck
                {
                    Subject = GetSubject( person, ssn ),
                    Search = new Search
                    {
                        OrderMore = "Yes",
                        Type = packageName,
                        RefNumber = campusName,
                        OrderedBy = orderedBy.FullName,
                        OrderedByEmail = orderedBy.Email
                    }
                }
            };

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


        Subject GetSubject( Person person, string ssn )
        {
            var subject = new Subject
            {
                LastName = person.LastName,
                FirstName = person.FirstName,
                ContactEmail = person.Email,
            };

            if ( !string.IsNullOrWhiteSpace( person.MiddleName ) )
            {
                subject.MiddleInitial = person.MiddleName[0].ToString();
            }

            var phone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phone == null )
            {
                phone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            }
            if ( phone != null )
            {
                subject.Phone = phone.Number;
            }

            var address = person.GetHomeLocation();

            if ( address != null )
            {
                subject.Address = address.Street1 + " " + address.Street2;
                subject.City = address.City;
                subject.State = address.State;
                subject.ZipCode = address.PostalCode;
            }

            if ( person.BirthDate != null )
            {
                subject.DOB = new DOB
                {
                    DOBYEAR = person.BirthDate.Value.ToString( "yyyy" ),
                    DOBMONTH = person.BirthDate.Value.ToString( "MM" ),
                    DOBDAY = person.BirthDate.Value.ToString( "dd" )
                };
            }

            if ( !string.IsNullOrWhiteSpace( ssn ) )
            {
                subject.SSN = new SSN
                {
                    SSN1 = ssn.Substring( 0, 3 ),
                    SSN2 = ssn.Substring( 4, 2 ),
                    SSN3 = ssn.Substring( 7, 4 )
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
        /// Gets the person that is currently logged in.
        /// </summary>
        /// <returns></returns>
        private Person GetCurrentPerson()
        {
            using ( var rockContext = new RockContext() )
            {
                var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
                return currentUser != null ? currentUser.Person : null;
            }
        }

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


        /// <summary>
        /// Sets the workflow requeststatus attribute.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="requestStatus">The request status.</param>
        private void UpdateWorkflowRequestStatus( Workflow workflow, RockContext rockContext, string requestStatus )
        {
            if ( SaveAttributeValue( workflow, "RequestStatus", requestStatus,
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
        private bool GetPackageName( RockContext rockContext, Workflow workflow, AttributeCache requestTypeAttribute, out DefinedValueCache package, List<string> errorMessages )
        {
            package = null;
            if ( requestTypeAttribute == null )
            {
                errorMessages.Add( "This background check provider requires a background check type." );
                return false;
            }

            DefinedValueCache pkgTypeDefinedValue = DefinedValueCache.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
            if ( pkgTypeDefinedValue == null )
            {
                errorMessages.Add( "This background check provider couldn't load background check type." );
                return false;
            }

            if ( pkgTypeDefinedValue.Attributes == null )
            {
                return false;
            }

            package = pkgTypeDefinedValue;
            return true;
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
        #endregion
    }
}