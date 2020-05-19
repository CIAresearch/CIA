using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 5, "1.10.0" )]
    public partial class SsnNotRequired : Migration
    {
        public override void Up()
        {
            Sql( "UPDATE [WorkflowActionFormAttribute] SET IsRequired = 0 WHERE [Guid] = '7330C696-AEB3-4748-A607-83C1B7C322D2'" );
        }

        public override void Down()
        {
            Sql( "UPDATE [WorkflowActionFormAttribute] SET IsRequired = 1 WHERE [Guid] = '7330C696-AEB3-4748-A607-83C1B7C322D2'" );
        }
    }
}
