using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 15, "1.14.0" )]
    public partial class RenewalNotification
        : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1> <div class='alert alert-danger'>An error occurred when submitting this request to CIA. See details below. </div>", @"", "Re-Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^70EE2369-2691-4043-8CD5-68E8B6C07102^Your information has been submitted successfully.|Cancel^8cf6e927-4fa5-4241-991c-391038b79631^^The request has been canceled.|", 
                Rock.SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION, true, "", "3F72654C-60AA-4941-ACAC-A54021F7FD8A" ); // Automatic Background Check Renewal (CIA):Request Error:Display Error Message  

        }

        public override void Down()
        {
        }
    }
}
