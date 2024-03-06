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
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "6F8A431C-BEBD-4D33-AAD6-1D70870329C2", "Auto Renewal Days", "AutoRenewalDays", "", "The number of days to wait before auto renewing a background check.", 0, "", "d0852aff-8dd8-4f44-8218-d1342a39ff64" );
        }

        public override void Down()
        {
            //Nope nope nope.
        }
    }
}
