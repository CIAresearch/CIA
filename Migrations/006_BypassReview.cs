using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 6, "1.10.0" )]
    public partial class BypassReview : Migration
    {
        public override void Up()
        {
            //Add new attributes for handling bypass
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63947DA6-427C-4800-9AA4-8CEFAA1264E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Complete If No Hits", "CompleteIfNoHits", "Should the workflow complete without further review if there are no hits?", 20, @"False", "1B66E433-AEE8-4DE6-8F8C-77D3AFA0AE5A", false ); // Background Check (CIA):Complete If No Hits
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "63947DA6-427C-4800-9AA4-8CEFAA1264E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BypassReview", "BypassReview", "", 21, @"False", "B64E7380-6671-4E70-9B5A-85814DF23E6D", false ); // Background Check (CIA):BypassReview
            RockMigrationHelper.AddAttributeQualifier( "1B66E433-AEE8-4DE6-8F8C-77D3AFA0AE5A", "BooleanControlType", @"0", "9B324C4C-C630-4E7C-98D6-C2223842EA6F" ); // Background Check (CIA):Complete If No Hits:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "1B66E433-AEE8-4DE6-8F8C-77D3AFA0AE5A", "falsetext", @"No", "D9D4EC04-D1D4-4E20-B910-DBD1DD0BAFA1" ); // Background Check (CIA):Complete If No Hits:falsetext
            RockMigrationHelper.AddAttributeQualifier( "1B66E433-AEE8-4DE6-8F8C-77D3AFA0AE5A", "truetext", @"Yes", "2CB58773-1C2A-4CC9-9638-A7E836969FA8" ); // Background Check (CIA):Complete If No Hits:truetext
            RockMigrationHelper.AddAttributeQualifier( "B64E7380-6671-4E70-9B5A-85814DF23E6D", "BooleanControlType", @"0", "4EAAA7DA-AC33-46F0-B186-0919BF25E6D5" ); // Background Check (CIA):BypassReview:BooleanControlType
            RockMigrationHelper.AddAttributeQualifier( "B64E7380-6671-4E70-9B5A-85814DF23E6D", "falsetext", @"No", "D86F8EDA-0701-4CBD-9601-9EF9BE6B55E3" ); // Background Check (CIA):BypassReview:falsetext
            RockMigrationHelper.AddAttributeQualifier( "B64E7380-6671-4E70-9B5A-85814DF23E6D", "truetext", @"Yes", "83D197C3-B3DE-4B4C-956A-A9EE15B0E042" ); // Background Check (CIA):BypassReview:truetext
                     
            //Add allow bypass option to form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "75F4CA7B-5EEC-4FB0-B691-7A7D42DC60D9", "1B66E433-AEE8-4DE6-8F8C-77D3AFA0AE5A", 23, true, false, true, false, @"", @"", "F8905175-2CB3-4C55-BD6A-0E7A7C00441B" ); // Background Check (CIA):Approve Request:Approve or Deny:Complete If No Hits
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "75F4CA7B-5EEC-4FB0-B691-7A7D42DC60D9", "B64E7380-6671-4E70-9B5A-85814DF23E6D", 24, false, true, false, false, @"", @"", "ABC7BC72-E6BB-4C42-8490-A1A9BD04BAFC" ); // Background Check (CIA):Approve Request:Approve or Deny:BypassReview
            
            //Add in new actions (and reorder old actions)
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Detect If Can Bypass Review", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "772095FA-64A8-49DB-8CEC-45B0564C5F1B" ); // Background Check (CIA):Review Result:Detect If Can Bypass Review
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "(If Can Bypass) Set Status To Pass", 1, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "B64E7380-6671-4E70-9B5A-85814DF23E6D", 1, "True", "E365FF55-CC23-4CC7-B4CB-409222A0E0A3" ); // Background Check (CIA):Review Result:(If Can Bypass) Set Status To Pass
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "(If Can Bypass) Activate Complete", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "B64E7380-6671-4E70-9B5A-85814DF23E6D", 1, "True", "CC1DFCD0-B636-47DC-9B93-5A2CDC124129" ); // Background Check (CIA):Review Result:(If Can Bypass) Activate Complete
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Set Status", 3, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "078C0009-0388-4594-9D44-CE361F58B938" ); // Background Check (CIA):Review Result:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Assign Activity", 4, "08189B3F-B506-45E8-AA68-99EC51085CF3", true, false, "", "", 1, "", "931D4CA3-3B93-4032-83CD-B797B8754284" ); // Background Check (CIA):Review Result:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Review Results", 5, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "DA546B9F-27CC-462A-9139-A85A145BDD1B", "", 1, "", "9A469B25-B141-4919-88C2-DD2D03C6BAA9" ); // Background Check (CIA):Review Result:Review Results
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Update Result", 6, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "AB036583-848B-400A-A81E-1905714D19BE" ); // Background Check (CIA):Review Result:Update Result
            RockMigrationHelper.UpdateWorkflowActionType( "F4CF865F-9DB5-448A-AFAF-B491A591083C", "Activate Complete", 7, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 1, "", "003C9B47-A6F9-438E-94E8-97CD9727A1C1" ); // Background Check (CIA):Review Result:Activate Complete
            
            //Update action attributes
            RockMigrationHelper.AddActionTypeAttributeValue( "772095FA-64A8-49DB-8CEC-45B0564C5F1B", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign hits = Workflow | Attribute:'NumberRecords' | AsInteger -%} {% assign canBypass = Workflow | Attribute:'CompleteIfNoHits' -%} {% if hits == 0 and canBypass == 'Yes' -%} True {% else -%} False {% endif -%}" ); // Background Check (CIA):Review Result:Detect If Can Bypass Review:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "772095FA-64A8-49DB-8CEC-45B0564C5F1B", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Background Check (CIA):Review Result:Detect If Can Bypass Review:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "772095FA-64A8-49DB-8CEC-45B0564C5F1B", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"b64e7380-6671-4e70-9b5a-85814df23e6d" ); // Background Check (CIA):Review Result:Detect If Can Bypass Review:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E365FF55-CC23-4CC7-B4CB-409222A0E0A3", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Background Check (CIA):Review Result:(If Can Bypass) Set Status To Pass:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E365FF55-CC23-4CC7-B4CB-409222A0E0A3", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"5f840fc7-1fef-433d-bf4e-52a39f0bbcd7" ); // Background Check (CIA):Review Result:(If Can Bypass) Set Status To Pass:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E365FF55-CC23-4CC7-B4CB-409222A0E0A3", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"Pass" ); // Background Check (CIA):Review Result:(If Can Bypass) Set Status To Pass:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CC1DFCD0-B636-47DC-9B93-5A2CDC124129", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (CIA):Review Result:(If Can Bypass) Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CC1DFCD0-B636-47DC-9B93-5A2CDC124129", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"6F6CAE3B-7656-4FC9-90B8-43926BAAFEBF" ); // Background Check (CIA):Review Result:(If Can Bypass) Activate Complete:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "078C0009-0388-4594-9D44-CE361F58B938", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check (CIA):Review Result:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "078C0009-0388-4594-9D44-CE361F58B938", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Waiting for Review" ); // Background Check (CIA):Review Result:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "078C0009-0388-4594-9D44-CE361F58B938", "AE8C180C-E370-414A-B10D-97891B95D105", @"" ); // Background Check (CIA):Review Result:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "931D4CA3-3B93-4032-83CD-B797B8754284", "27BAC9C8-2BF7-405A-AA01-845A3D374295", @"False" ); // Background Check (CIA):Review Result:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "931D4CA3-3B93-4032-83CD-B797B8754284", "D53823A1-28CB-4BA0-A24C-873ECF4079C5", @"a6bcc49e-103f-46b0-8bac-84ea03ff04d5" ); // Background Check (CIA):Review Result:Assign Activity:Security Role
            RockMigrationHelper.AddActionTypeAttributeValue( "931D4CA3-3B93-4032-83CD-B797B8754284", "120D39B5-8D2A-4B96-9419-C73BE0F2451A", @"" ); // Background Check (CIA):Review Result:Assign Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9A469B25-B141-4919-88C2-DD2D03C6BAA9", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check (CIA):Review Result:Review Results:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9A469B25-B141-4919-88C2-DD2D03C6BAA9", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Background Check (CIA):Review Result:Review Results:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AB036583-848B-400A-A81E-1905714D19BE", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Background Check (CIA):Review Result:Update Result:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AB036583-848B-400A-A81E-1905714D19BE", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"d07ecd05-a82c-4284-acd2-3a63916ece82" ); // Background Check (CIA):Review Result:Update Result:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AB036583-848B-400A-A81E-1905714D19BE", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Background Check (CIA):Review Result:Update Result:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "AB036583-848B-400A-A81E-1905714D19BE", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"5f840fc7-1fef-433d-bf4e-52a39f0bbcd7" ); // Background Check (CIA):Review Result:Update Result:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "003C9B47-A6F9-438E-94E8-97CD9727A1C1", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check (CIA):Review Result:Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "003C9B47-A6F9-438E-94E8-97CD9727A1C1", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Background Check (CIA):Review Result:Activate Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "003C9B47-A6F9-438E-94E8-97CD9727A1C1", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"6F6CAE3B-7656-4FC9-90B8-43926BAAFEBF" ); // Background Check (CIA):Review Result:Activate Complete:Activity
        }

        public override void Down()
        {
            //Nope nope nope.
        }
    }
}
