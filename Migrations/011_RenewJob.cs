using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 11, "1.14.0" )]
    public partial class RenewJob : Migration
    {
        public override void Up()
        {
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'CIAResearch.Jobs.AutoRenewBackgroundChecks' AND [Guid] = 'F789059E-B035-4AD0-9F91-B9FB4D81C6ED' )              BEGIN                 INSERT INTO [ServiceJob] (                    [IsSystem]                    ,[IsActive]                    ,[Name]                    ,[Description]                    ,[Class]                    ,[CronExpression]                    ,[NotificationStatus]                    ,[Guid] )                 VALUES (                     0                    ,1                    ,'Auto Renew Background Checks'                    ,'Automatically renews CIA background checks based on the ''Auto Renewal Days'' person attribute'                    ,'CIAResearch.Jobs.AutoRenewBackgroundChecks'                    ,'0 0 0 1/1 * ? *'                    ,1                    ,'F789059E-B035-4AD0-9F91-B9FB4D81C6ED'                    );              END" );

        }

        public override void Down()
        {
        }
    }
}
