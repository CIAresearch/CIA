using System;
using System.Linq;
using CIAResearch.Helpers;
using Quartz;
using RestSharp;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace CIAResearch.Jobs
{
    /// <summary>
    /// Job to update CIA Research requests
    /// </summary>
    [DisallowConcurrentExecution]
    public class UpdateRequests : IJob
    {
        public UpdateRequests()
        {
        }

        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var collected = 0;
            var errors = 0;
            var expirationDays = CIAResearch.GetExpirationDays();

            RockContext rockContext = new RockContext();
            BackgroundCheckService backgroundCheckService = new BackgroundCheckService( rockContext );
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService( rockContext );

            //This binary file type (background check) should be in the database, but we don't want to throw an error
            var defaultBinaryFileType = binaryFileTypeService.Get( "5C701472-8A6B-4BBE-AEC6-EC833C859F2D".AsGuid() );
            if ( defaultBinaryFileType == null )
            {
                defaultBinaryFileType = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );
            }

            var componentId = EntityTypeCache.Get( typeof( CIAResearch ) ).Id;
            var backgroundChecks = backgroundCheckService.Queryable()
                .Where( b => b.ProcessorEntityTypeId == componentId )
                .Where( b => b.ResponseDate == null )
                .Where( b => b.RequestId != null && b.RequestId != "" )
                .ToList();

            int page = 0;
            // CIA Research requested we batch the requests in 20s
            while ( backgroundChecks.Count > page * 20 )
            {
                var queryRequest = new QueryRequest
                {
                    Source = "ROCKRMS",
                    Client = CIAResearch.GetClient(),
                    Login = CIAResearch.GetLogin(),
                    QueryIDs = backgroundChecks.Skip( page * 20 ).Take( 20 ).Select( b => b.RequestId ).ToList()
                };

                var client = new RestClient( "https://www.ciaresearch.com/system/xml.nsf/QueryResults?OpenAgent" );
                var request = new RestRequest( Method.POST );
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddXmlBody( queryRequest );
                request.RequestFormat = RestSharp.DataFormat.Xml;
                var response = client.Execute<XMLRequest>( request );
                var xmlRequest = response.Data;

                if ( !string.IsNullOrWhiteSpace( response.ErrorMessage ) )
                {
                    context.Result = "Error while parsing response: " + response.ErrorMessage;
                    ExceptionLogService.LogException( new Exception( "CIA Error: " + response.ErrorMessage ) );
                    errors++;
                    page++;
                    continue;
                }

                if ( !string.IsNullOrWhiteSpace( xmlRequest.Error ) )
                {
                    context.Result = "CIA Error: " + xmlRequest.Error;
                    ExceptionLogService.LogException( new Exception( "CIA Error: " + xmlRequest.Error ) );
                    errors++;
                    page++;
                    continue;
                }

                foreach ( var report in xmlRequest.Reports )
                {
                    // get the related database entry
                    var backgroundCheck = backgroundChecks.FirstOrDefault( b => b.RequestId == report.TrackingNumber );

                    if ( backgroundCheck == null )
                    {
                        ExceptionLogService.LogException( new Exception( "CIA Error: Unable to match" + report.TrackingNumber ) );
                        continue;
                    }

                    //If the status does not equal 1 it's not done yet
                    if ( report.ReportStatus != "1" )
                    {
                        //Check to see if the background check is older than allowed
                        if ( backgroundCheck.CreatedDateTime == null
                            || ( RockDateTime.Now - backgroundCheck.CreatedDateTime.Value ).Days > expirationDays )
                        {
                            try
                            {
                                CIAResearch.CancelBackgroundCheck( backgroundCheck.Id, "Expired" );
                            }
                            catch (Exception ex)
                            {
                                ExceptionLogService.LogException( new Exception( "Could not expire background check.", ex ) );
                            }
                        }
                        else
                        {
                            //Update the status of the workflow to the status of the status text
                            CIAResearch.UpdateWorkflowRequestStatus( backgroundCheck, report.ReportStatusText );
                        }
                        continue;
                    }

                    BinaryFile reportFile = new BinaryFile();
                    try
                    {

                        var pdf = CIAResearch.GetPDF( backgroundCheck.RequestId );
                        BinaryFileData reportData = new BinaryFileData()
                        {
                            Content = pdf,
                        };

                        var person = backgroundCheck.PersonAlias.Person;
                        reportFile.FileName = string.Format( $"BackgroundCheck_{person.FirstName}_{person.LastName}.pdf" );
                        reportFile.IsTemporary = false;
                        reportFile.DatabaseData = reportData;
                        reportFile.MimeType = "application/pdf";
                        reportFile.FileSize = pdf.Length;
                        reportFile.BinaryFileType = defaultBinaryFileType;

                        binaryFileService.Add( reportFile );
                        rockContext.SaveChanges();
                    }
                    catch
                    {
                        ExceptionLogService.LogException( new Exception( "Could not save report for tracking number: " + backgroundCheck.RequestId ) );
                    }

                    backgroundCheck.ResponseData = report.ReportStatusText;
                    backgroundCheck.ResponseDate = RockDateTime.Today;
                    backgroundCheck.ResponseDocument = reportFile;
                    rockContext.SaveChanges();

                    CIAResearch.UpdateWorkflow( backgroundCheck, report, rockContext );
                    collected++;
                }
                page++;
            }
            context.Result = string.Format( "Requested update on {0} background check{1}. {2} completed.",
                backgroundChecks.Count,
                backgroundChecks.Count == 1 ? "" : "s",
                collected );
            if ( errors > 0 )
            {
                context.Result += string.Format( " There were {0} errors that occured. Please check your log for information", errors );
            }
        }

    }
}
