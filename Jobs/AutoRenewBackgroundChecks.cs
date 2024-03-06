using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace CIAResearch.Jobs
{

    [DisallowConcurrentExecution]

    [DisplayName( "Auto Renew Background Checks" )]
    [Description( "Automatically renews CIA background checks based on the 'Auto Renewal Days' person attribute" )]

    [AttributeField( "Auto Renewal Days Attribute",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        DefaultValue = "d0852aff-8dd8-4f44-8218-d1342a39ff64",
        Key = AttributeKeys.Attribute,
        Order = 0,
        IsRequired = true
        )]

    [WorkflowTypeField( "Workflow Type",
        Description = "The workflow type to run when it's time to auto renew the background check.",
        Order = 1,
        Key = AttributeKeys.WorkflowType,
        DefaultValue = "144F2D19-6B01-46D9-9123-A9688EFEB40C",
        IsRequired = true )]

    public class AutoRenewBackgroundChecks : IJob
    {
        public static class AttributeKeys
        {
            public const string Attribute = "Attribute";
            public const string WorkflowType = "WorkflowType";
        }


        public virtual void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;

            var attributeGuid = dataMap.GetString( AttributeKeys.Attribute ).AsGuid();
            var workflowTypeGuid = dataMap.GetString( AttributeKeys.WorkflowType ).AsGuid();

            var attribute = AttributeCache.Get( attributeGuid );
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );

            if ( attribute == null || workflowType == null )
            {
                throw new Exception( "Attribut and Workflow Type cannot be null" );
            }

            RockContext rockContext = new RockContext();
            BackgroundCheckService backgroundCheckService = new BackgroundCheckService( rockContext );

            var backgroundCheckProviderId = EntityTypeCache.Get( typeof( CIAResearch ) ).Id;

            var peopleToCheck = backgroundCheckService.Queryable()
                 .Where( b => b.ProcessorEntityTypeId == backgroundCheckProviderId)
                 .OrderByDescending( b => b.RequestDate )
                 .DistinctBy( b => b.PersonAlias.Person.Id )
                 .ToList();

            var launchCount = 0;

            foreach ( var bc in peopleToCheck )
            {
                var person = bc.PersonAlias.Person;
                person.LoadAttributes();

                var days = person.GetAttributeValue( attribute.Key ).AsIntegerOrNull();

                if ( days == null )
                {
                    continue;
                }

                if ( bc.ResponseDate < RockDateTime.Today.AddDays( days.Value * -1 ) )
                {
                    person.LaunchWorkflow( workflowType.Guid, "Auto Renewal", new Dictionary<string, string>(), null );
                    launchCount++;
                }

            }

            context.UpdateLastStatusMessage( $"Started {launchCount} Auto Renewals" );
        }
    }
}
