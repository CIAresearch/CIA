using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 8, "1.10.0" )]
    public partial class EmailBody : Migration
    {
        private const string EMAIL_BODY_GUID = "A34BCDD2-CF06-42C7-86AA-0F93E4169B26";
        private const string NUMBER_OF_ROWS = "numberofrows";
        private const string ALLOW_HTML = "allowhtml";
        private const string MAX_CHARACTERS = "maxcharacters";
        private const string SHOW_COUNT_DOWN = "showcountdown";
        public override void Up()
        {


            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63947DA6-427C-4800-9AA4-8CEFAA1264E5", Rock.SystemGuid.FieldType.MEMO, "Custom Message (EConsent only)", "EmailBody",
                    "Add an optional message that will be send with your request.", 21, "", EMAIL_BODY_GUID, false );
            RockMigrationHelper.AddAttributeQualifier( EMAIL_BODY_GUID, NUMBER_OF_ROWS, "6", "E8C4878B-C7CA-4BC7-94F4-86875E39348E" );
            RockMigrationHelper.AddAttributeQualifier( EMAIL_BODY_GUID, ALLOW_HTML, "False", "A53C1853-BCD1-4ED9-8781-4EC8D24393C8" );
            RockMigrationHelper.AddAttributeQualifier( EMAIL_BODY_GUID, MAX_CHARACTERS, "2500", "52FBC684-6D65-400A-8B70-A11906550D31" );
            RockMigrationHelper.AddAttributeQualifier( EMAIL_BODY_GUID, SHOW_COUNT_DOWN, "True", "9BD71E56-8F9B-4EF8-9CA0-267DB4DE05D7" );

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4616559F-4824-4CD3-B193-A07440CF05B2", EMAIL_BODY_GUID, 12, true, false, false, "AA5DFD0E-EF29-4D11-9D48-76E216F0E549" );
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "75F4CA7B-5EEC-4FB0-B691-7A7D42DC60D9", EMAIL_BODY_GUID, 8, true, false, false, "421C7D96-178C-4B09-9FBE-D267F85FFD0E" );

        }

        public override void Down()
        {
            //Nope nope nope.
        }
    }
}
