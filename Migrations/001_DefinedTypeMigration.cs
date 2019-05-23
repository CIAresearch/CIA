using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    public partial class DefinedTypeMigration : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "CIA Background Check Packages", "List of available packages for use in a CIA Background Check", "855D493C-5C22-4FCA-AADF-3C742FA8389B", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "855D493C-5C22-4FCA-AADF-3C742FA8389B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is E-Consent", "IsEConsent", "", 0, "", "3045594F-7048-428A-A279-9CEF0FEE7040" );
            RockMigrationHelper.AddDefinedTypeAttribute( "855D493C-5C22-4FCA-AADF-3C742FA8389B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "PackageName / E-Consent ID", "PackageName", "Name of the package for non E-Consent packages, or the E-Consent ID if it is.", 1, "", "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E" );
            RockMigrationHelper.AddAttributeQualifier( "3045594F-7048-428A-A279-9CEF0FEE7040", "falsetext", "No", "D883CE90-270B-4B88-9762-B4F0A720330A" );
            RockMigrationHelper.AddAttributeQualifier( "3045594F-7048-428A-A279-9CEF0FEE7040", "truetext", "Yes", "89E71078-63ED-4299-838F-60CF27AE7D13" );
            RockMigrationHelper.AddAttributeQualifier( "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E", "ispassword", "False", "191E1B3E-69C6-4395-9F7E-C630AACBDED2" );
            RockMigrationHelper.AddAttributeQualifier( "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E", "maxcharacters", "", "845D43DD-5408-467E-888B-35D2BC4ABD26" );
            RockMigrationHelper.AddAttributeQualifier( "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E", "showcountdown", "False", "41C86388-0184-4C78-9159-A116AB6ACC8C" );
            RockMigrationHelper.UpdateDefinedValue( "855D493C-5C22-4FCA-AADF-3C742FA8389B", "CIA E-Consent Volunteer", "A CIA E-Consent handles both the background check as well as the authorization to perform the background check.", "DF61D6A0-F236-4959-B611-5439A8A7A172", false );
            RockMigrationHelper.UpdateDefinedValue( "855D493C-5C22-4FCA-AADF-3C742FA8389B", "Social Security Trace", "Criminal background check based on SSN.", "875AC446-FA69-4AA2-9E40-315E078380B9", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "875AC446-FA69-4AA2-9E40-315E078380B9", "3045594F-7048-428A-A279-9CEF0FEE7040", @"False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "875AC446-FA69-4AA2-9E40-315E078380B9", "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E", @"Social Security Trace" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DF61D6A0-F236-4959-B611-5439A8A7A172", "3045594F-7048-428A-A279-9CEF0FEE7040", @"True" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DF61D6A0-F236-4959-B611-5439A8A7A172", "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E", @"" );

            Sql( @"UPDATE Attribute SET [IsRequired] = 1 WHERE Guid in ('3045594F-7048-428A-A279-9CEF0FEE7040'  ,'4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E')" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "4D8FBA0B-85BB-4177-9D46-C6C4D5E98F4E" ); // PackageName	0
            RockMigrationHelper.DeleteDefinedValue( "875AC446-FA69-4AA2-9E40-315E078380B9" ); // Social Security Trace	1
            RockMigrationHelper.DeleteDefinedValue( "DF61D6A0-F236-4959-B611-5439A8A7A172" ); // ERelease	1
            RockMigrationHelper.DeleteDefinedType( "855D493C-5C22-4FCA-AADF-3C742FA8389B" ); // CIA Research Background Check Packages	2
        }
    }
}
