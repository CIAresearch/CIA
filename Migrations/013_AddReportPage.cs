using System.Linq;
using Rock.Plugin;
using Rock.Web.Cache;

namespace CIAResearch.Migrations
{
    [MigrationNumber( 13, "1.14.0" )]
    public partial class AddReportPage
        : Migration
    {
        public override void Up()
        {
            // Page: CIA
            RockMigrationHelper.AddPage("5B6DBC42-8B03-4D15-8D92-AAFA28FD8616","D65F783D-87A9-4CC9-8110-E83466A0EADB","CIA","","F9659AB0-FD85-4F03-B036-D9A7C68218A3","fa fa-user-shield"); // Site:Rock RMS
            // Page: Cleared and Serving Report
            RockMigrationHelper.AddPage("F9659AB0-FD85-4F03-B036-D9A7C68218A3","D65F783D-87A9-4CC9-8110-E83466A0EADB","Cleared and Serving Report","","820FEA27-C025-4580-8F6D-E50151E61ADD",""); // Site:Rock RMS
            RockMigrationHelper.AddBlock( "F9659AB0-FD85-4F03-B036-D9A7C68218A3", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549" );
            RockMigrationHelper.AddBlock( "820FEA27-C025-4580-8F6D-E50151E61ADD", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Cleared And Serving Report", "Main", "", "", 0, "717DEDAD-9C45-4979-B7D3-B555A26DA224" );
            RockMigrationHelper.AddBlockAttributeValue( "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters  
            RockMigrationHelper.AddBlockAttributeValue( "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" ); // Template  
            RockMigrationHelper.AddBlockAttributeValue( "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" ); // Number of Levels  
            RockMigrationHelper.AddBlockAttributeValue( "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString  
            RockMigrationHelper.AddBlockAttributeValue( "1AF5EAC7-AAF8-4D92-882C-9C4C4FAF2549", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" ); // Update Page  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" ); // Query Params  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"Id" ); // Columns  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"SELECT  p.Id,  p.LastName,  p.FirstName,  avDate.ValueAsDateTime AS [LastClearedDate],  CAST (CASE    WHEN COUNT(gm.Id) > 0 THEN 1   Else 0  END AS bit) AS [InServingGroup] FROM PERSON P JOIN [AttributeValue] avDate ON avDate.EntityId = p.Id AND  avDate.AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F') JOIN [AttributeValue] avResult ON avResult.EntityId = p.Id AND avResult.AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '44490089-E02C-4E54-A456-454845ABBC9D') LEFT JOIN [GroupMember] gm ON gm.PersonId = p.Id   AND gm.GroupMemberStatus = 1 and gm.IsArchived != 1   AND gm.GroupTypeId IN (SELECT Id FROM [GroupType] WHERE [GroupTypePurposeValueId] IN (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '36A554CE-7815-41B9-A435-93F3D52A2828')) WHERE  avResult.Value = 'Pass' GROUP BY  p.Id, p.LastName, p.FirstName, avDate.ValueAsDateTime" ); // Query  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"" ); // Url Mask  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "202A82BF-7772-481C-8419-600012607972", @"False" ); // Show Columns  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" ); // Merge Fields  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" ); // Formatted Output  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"True" ); // Person Report  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" ); // Communication Recipient Person Id Columns  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" ); // Show Excel Export  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"True" ); // Show Communicate  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"True" ); // Show Merge Person  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"True" ); // Show Bulk Update  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "A4439703-5432-489A-9C14-155903D6A43E", @"False" ); // Stored Procedure  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" ); // Show Merge Template  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" ); // Paneled Grid  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" ); // Show Grid Filter  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" ); // Timeout  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8", @"" ); // Page Title Lava  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "AF7714D4-D825-419A-B136-FF8293396635", @"" ); // Encrypted Fields  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "36BC9741-C4BA-4B1E-BE1C-E64491781900", @"" ); // Panel Title  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "28208FC3-BB71-4400-AF3A-862167D3E493", @"" ); // Panel Icon CSS Class  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "132C3810-0D6B-4DD7-BE4C-287E6769A274", @"False" ); // Wrap In Panel  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "EB24917C-FE59-4D80-B088-1D280F3D364B", @"True" ); // Show Launch Workflow  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "0E6BDD50-E1FA-4EB8-A6EC-9466D9F63131", @"False" ); // Enable Quick Return  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "4710072D-6243-42E1-9398-223A857C3482", @"" ); // Grid Header Content  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "D86D202B-98D7-4F1F-A856-0ADABB4AEFA8", @"" ); // Grid Footer Content  
            RockMigrationHelper.AddBlockAttributeValue( "717DEDAD-9C45-4979-B7D3-B555A26DA224", "F438B5A5-78F3-44EF-9926-77193BAC0EF2", @"False" ); // Enable Sticky Header on Grid  
        }

        public override void Down()
        {
        }
    }
}
