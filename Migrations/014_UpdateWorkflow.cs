using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 14, "1.14.0" )]
    public partial class UpdateWorkflow : Migration
    {
        public override void Up()
        {
            Sql( @" UPDATE [WorkflowActionFormAttribute]
  SET
	IsVisible = 1,
	IsReadOnly = 0
  WHERE Attributeid IN (SELECT [Id] FROM [Attribute] WHERE [Guid] IN ('1DB2B369-0448-46FD-B226-CB620CA81279','A0A179BF-2DA5-41D7-B74C-DE1A42C39501') )
  AND [WorkflowActionFormId] IN (SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] IN ('75F4CA7B-5EEC-4FB0-B691-7A7D42DC60D9','C840696E-7E80-4E42-AF5A-C84621338573'))

  DELETE FROM [workflowaction] WHERE ActionTypeId IN (select id from [WorkflowActionType] WHERE [Guid] ='BB31D841-AD63-4D1C-B856-EEF3059C17E4')
  DELETE FROM [WorkflowActionType] WHERE [Guid] = 'BB31D841-AD63-4D1C-B856-EEF3059C17E4'
" );

        }

        public override void Down()
        {
        }
    }
}
