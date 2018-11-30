using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 4, "1.8.0" )]
    public partial class JobMigration : Migration
    {
        public override void Up()
        {
            // add ServiceJob: Update CIA Requests
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'CIAResearch.Jobs.UpdateRequests' AND [Guid] = 'D5601E26-6C13-4293-9F81-E542F2394ECC' )              BEGIN                 INSERT INTO [ServiceJob] (                    [IsSystem]                    ,[IsActive]                    ,[Name]                    ,[Description]                    ,[Class]                    ,[CronExpression]                    ,[NotificationStatus]                    ,[Guid] )                 VALUES (                     0                    ,1                    ,'Update CIA Requests'                    ,'Queries the CIA servers for updates on outstanding background checks. Downloads data if complete and resumes background check workflow.'                    ,'CIAResearch.Jobs.UpdateRequests'                    ,'0 0 0/4 1/1 * ? *'                    ,1                    ,'D5601E26-6C13-4293-9F81-E542F2394ECC'                    );              END" );

        }

        public override void Down()
        {
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql  
            // remove ServiceJob: Update CIA Requests
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'CIAResearch.Jobs.UpdateRequests' AND [Guid] = 'D5601E26-6C13-4293-9F81-E542F2394ECC' )              BEGIN                 DELETE [ServiceJob]  WHERE [Guid] = 'D5601E26-6C13-4293-9F81-E542F2394ECC';              END" );
        }
    }
}
