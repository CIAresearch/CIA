using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 12, "1.14.0" )]
    public partial class AddSsnOptionalAttribute
        : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "855D493C-5C22-4FCA-AADF-3C742FA8389B", Rock.SystemGuid.FieldType.BOOLEAN, "SSN Required", "IsSsnRequired", "", 3, "", "A1F65352-D636-4333-A916-A0A45D179256" );
        }

        public override void Down()
        {
        }
    }
}
