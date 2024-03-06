using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 10, "1.14.0" )]
    public partial class RenewAttribute : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.ATTRIBUTE, "Safety & Security", "fa fa-medkit", "Information related to safety and security of organization", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Auto Renewal Days", "AutoRenewalDays", "", "The number of days to wait before auto renewing a background check.", 0, "", "d0852aff-8dd8-4f44-8218-d1342a39ff64" );
        }

        public override void Down()
        {
            //Nope nope nope.
        }
    }
}
