using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 7, "1.10.0" )]
    public partial class RemovePMMRef : Migration
    {
        public override void Up()
        {
            Sql( "UPDATE [WorkflowActionForm] SET [Header] = '<h1>Background Request Details</h1> <div class=\"alert alert-danger\">An error occurred when submitting this request to CIA. See details below. </div>' WHERE [Guid] = 'C840696E-7E80-4E42-AF5A-C84621338573'" );
        }

        public override void Down()
        {
            //Nope nope nope.
        }
    }
}
