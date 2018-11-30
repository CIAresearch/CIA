using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 3, "1.8.0" )]
    public partial class PageMigration : Migration
    {
        public override void Up()
        {
            // Page: CIA Research
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "CIA", "", "03E94095-6868-45B9-8115-9D1374645775", "fa fa-user-shield" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "CIA Request List", "Lists all the CIA background check requests.", "~/Plugins/com_ciaresearch/BackgroundCheck/CIAResearchRequestList.ascx", "CIA", "632419F6-FEEE-4D43-A8A6-DAB23E91329B" );
            RockMigrationHelper.UpdateBlockType( "CIA Settings", "Block for updating the settings used by the CIA integration.", "~/Plugins/com_ciaresearch/BackgroundCheck/CIAResearchSettings.ascx", "CIA", "08EAC65F-D289-4AA2-B313-24D06591A666" );
            // Add Block to Page: CIA Research, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03E94095-6868-45B9-8115-9D1374645775", "", "632419F6-FEEE-4D43-A8A6-DAB23E91329B", "CIA Research Request List", "Main", "", "", 1, "A00E73B8-4559-4FC0-A69C-E6213E7C93FD" );
            // Add Block to Page: CIA Research, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03E94095-6868-45B9-8115-9D1374645775", "", "08EAC65F-D289-4AA2-B313-24D06591A666", "CIA Research Settings", "Main", "", "", 0, "1B9D74B1-52F9-4808-9370-F0856D62B907" );
            // Attrib for BlockType: CIA Research Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "632419F6-FEEE-4D43-A8A6-DAB23E91329B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "3838846A-6F64-4890-B22B-1277EC108F22" );
            // Attrib for BlockType: CIA Research Request List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "632419F6-FEEE-4D43-A8A6-DAB23E91329B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "36441C21-02C2-473E-9B92-0F2606003667" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "36441C21-02C2-473E-9B92-0F2606003667" );
            RockMigrationHelper.DeleteAttribute( "3838846A-6F64-4890-B22B-1277EC108F22" );
            RockMigrationHelper.DeleteBlock( "1B9D74B1-52F9-4808-9370-F0856D62B907" );
            RockMigrationHelper.DeleteBlock( "A00E73B8-4559-4FC0-A69C-E6213E7C93FD" );
            RockMigrationHelper.DeleteBlockType( "08EAC65F-D289-4AA2-B313-24D06591A666" );
            RockMigrationHelper.DeleteBlockType( "632419F6-FEEE-4D43-A8A6-DAB23E91329B" );
            RockMigrationHelper.DeletePage( "03E94095-6868-45B9-8115-9D1374645775" ); //  Page: CIA Research
        }
    }
}
