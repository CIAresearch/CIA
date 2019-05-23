using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Checkr.Constants;
using Rock.Data;
using Rock.Migrations;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.SystemKey;
using CIAResearch.Helpers;
using RestSharp;

namespace com_ciaresearch.Blocks.BackgroundCheck
{
    [DisplayName( "CIA Settings" )]
    [Category( "CIA" )]
    [Description( "Block for updating the settings used by the CIA integration." )]

    public partial class CIAResearchSettings : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            nbNotification.Visible = false;
            pnlToken.Visible = true;
            pnlPackages.Visible = false;
            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                SetSettingValue( rockContext, settings, "UserName", tbUserName.Text );
                SetSettingValue( rockContext, settings, "Password", tbPassword.Text, true );
                SetSettingValue( rockContext, settings, "ClientName", tbClientName.Text );
                SetSettingValue( rockContext, settings, "BranchName", tbBranchName.Text );
                SetSettingValue( rockContext, settings, "ClientContact", tbClientContact.Text );
                SetSettingValue( rockContext, settings, "ClientContactEmail", tbClientContactEmail.Text );


                rockContext.SaveChanges();

                BackgroundCheckContainer.Instance.Refresh();
            }

            pnlToken.Visible = false;
            pnlPackages.Visible = true;
            HideSecondaryBlocks( false );
            ShowDetail();
        }

        /// <summary>
        /// Handles the Click event of the btnDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDefault_Click( object sender, EventArgs e )
        {
            var bioBlock = BlockCache.Get( Rock.SystemGuid.Block.BIO.AsGuid() );
            // Record an exception if the stock Bio block has been deleted but continue processing
            // the remaining settings.
            if ( bioBlock == null )
            {
                var errorMessage = string.Format( "Stock Bio block ({0}) is missing.", Rock.SystemGuid.Block.BIO );
                ExceptionLogService.LogException( new Exception( errorMessage ) );
            }
            else
            {
                List<Guid> workflowActionGuidList = bioBlock.GetAttributeValues( "WorkflowActions" ).AsGuidList();
                if ( workflowActionGuidList == null || workflowActionGuidList.Count == 0 )
                {
                    // Add CIA Research to Bio Workflow Actions
                    bioBlock.SetAttributeValue( "WorkflowActions", CIAResearch.Utilities.Constants.WORKFLOW_TYPE );
                }
                else
                {
                    Guid guid = CIAResearch.Utilities.Constants.WORKFLOW_TYPE.AsGuid();
                    if ( !workflowActionGuidList.Any( w => w == guid ) )
                    {
                        // Add CIA Research to Bio Workflow Actions
                        workflowActionGuidList.Add( guid );
                    }

                    // Remove PMM from Bio Workflow Actions
                    guid = Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == guid );
                    bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );

                    // Remove Checkr from Bio Workflow Actions
                    guid = CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == guid );
                    bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );
                }

                bioBlock.SaveAttributeValue( "WorkflowActions" );
            }

            string typeName = ( typeof( CIAResearch.CIAResearch ).FullName );
            var component = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == typeName );
            component.Value.SetAttributeValue( "Active", "True" );
            component.Value.SaveAttributeValue( "Active" );
            // Set as the default provider in the system setting
            SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, typeName );

            ShowDetail();
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Display the packages.
        /// </summary>
        private void DisplayPackages()
        {
            using ( var rockContext = new RockContext() )
            {
                var packages = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( CIAResearch.Utilities.Constants.DEFINED_TYPE.AsGuid() )
                    .Select( v => v.Value )
                    .ToList();

                lPackages.Text = packages.AsDelimited( "<br/>" );
            }
        }

        private bool IsDefaultProvider()
        {
            string providerTypeName = ( typeof( CIAResearch.CIAResearch ) ).FullName;
            string defaultProvider = Rock.Web.SystemSettings.GetValue( SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER ) ?? string.Empty;
            return providerTypeName == defaultProvider;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            if ( IsDefaultProvider() )
            {
                btnDefault.Visible = false;
            }
            else
            {
                btnDefault.Visible = true;
            }

            nbValidate.Visible = false;

            string userName = null;
            string password = null;
            string clientName = null;
            string branchName = null;
            string clientContact = null;
            string clientContactEmail = null;
            using ( RockContext rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                if ( settings != null )
                {
                    userName = GetSettingValue( settings, "UserName" );
                    password = GetSettingValue( settings, "Password", true );
                    clientName = GetSettingValue( settings, "ClientName" );
                    branchName = GetSettingValue( settings, "BranchName" );
                    clientContact = GetSettingValue( settings, "ClientContact" );
                    clientContactEmail = GetSettingValue( settings, "ClientContactEmail" );
                }
            }

            if ( password.IsNullOrWhiteSpace() )
            {
                btnDefault.Visible = false;
                pnlToken.Visible = true;
                pnlPackages.Visible = false;
                HideSecondaryBlocks( true );
            }
            else
            {
                if ( IsDefaultProvider() )
                {
                    btnDefault.Visible = false;
                    pnlPackages.Enabled = true;
                }
                else
                {
                    btnDefault.Visible = true;
                    pnlPackages.Enabled = false;
                }

                tbUserName.Text = userName;
                tbPassword.Text = password;
                tbClientName.Text = clientName;
                tbBranchName.Text = branchName;
                tbClientContact.Text = clientContact;
                tbClientContactEmail.Text = clientContactEmail;

                lViewColumnLeft.Text = new DescriptionList()
                    .Add( "UserName", userName )
                    .Add( "Client Name", clientName )
                    .Add( "Client ID", clientName + "_" + branchName )
                    .Add( "Branch Name", branchName )
                    .Add( "Client Contact", clientContact )
                    .Add( "Client Contact Email", clientContactEmail )
                    .Html;
                DisplayPackages();
            }
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var entityType = EntityTypeCache.Get( typeof( CIAResearch.CIAResearch ) );
            if ( entityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == entityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value, bool encryptValue = false )
        {
            if ( encryptValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                { value = Encryption.EncryptString( value ); }
                catch { }
            }

            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();
            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var entityType = EntityTypeCache.Get( typeof( CIAResearch.CIAResearch ) );
                if ( entityType != null )
                {
                    var attribute = new AttributeService( rockContext )
                        .Queryable()
                        .Where( a =>
                            a.EntityTypeId == entityType.Id &&
                            a.Key == key
                        )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        new AttributeValueService( rockContext ).Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValue.EntityId = 0;
                    }
                }
            }
        }

        #endregion

        protected void btnValidate_Click( object sender, EventArgs e )
        {
            var client = new Client
            {
                BranchName = tbBranchName.Text,
                ClientContact = tbClientContact.Text,
                ClientContactEmail = tbClientContactEmail.Text,
                ClientName = tbClientName.Text
            };
            var login = new Login
            {
                UserName = tbUserName.Text,
                Password = tbPassword.Text
            };
            string error = "";
            nbValidate.Visible = true;
            if ( CIAResearch.CIAResearch.Validate( login, client, out error ) )
            {
                nbValidate.Title = "Success";
                nbValidate.Text = "Credentials Validated Properly";
                nbValidate.NotificationBoxType = NotificationBoxType.Success;
            }
            else
            {
                nbValidate.Title = "Validation Failed";
                nbValidate.Text = error;
                nbValidate.NotificationBoxType = NotificationBoxType.Warning;
            }
        }

        protected void btnSaveNew_Click( object sender, EventArgs e )
        {
            if ( tbNewPassword.Text != tbNewPassword2.Text )
            {
                nbValidate.Visible = true;
                nbValidate.Text = "Passwords did not match.";
                return;
            }

            var accessRequest = new CIAAccessRequest
            {
                RequestType = ddlNewRequestType.SelectedValue,
                UserName = tbNewUserName.Text,
                Password = tbNewPassword.Text,
                FirstName = tbNewFirstName.Text,
                MiddleInitial = tbNewMiddleInitial.Text,
                LastName = tbNewLastName.Text,
                BranchName = tbNewBranchName.Text,
                ClientName = tbNewClientName.Text,
                ClientEmail = tbNewEmail.Text,
                ClientEmail2 = tbNewEmail2.Text,
                StreetAddress1 = aNewAddress.Street1,
                StreetAddress2 = aNewAddress.Street2,
                City = aNewAddress.City,
                State = aNewAddress.State,
                ZipCode = aNewAddress.PostalCode,
                PhoneNumber = pnNewPhoneNumber.Text
            };

            var client = new RestClient( "http://www.ciaresearch.com/system/website.nsf/CIASetup?OpenAgent" );
            var request = new RestRequest( Method.POST );
            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
            request.AddXmlBody( accessRequest );
            request.RequestFormat = RestSharp.DataFormat.Xml;
            var response = client.Execute<XMLRequest>( request );
            var data = response.Data;

            if ( data != null && string.IsNullOrWhiteSpace( data.Error ) )
            {
                if ( data.Status != "Rejected" )
                {
                    pnlEdit.Visible = true;
                    pnlNew.Visible = false;
                    btnShowNew.Visible = true;
                    btnShowEdit.Visible = false;
                    nbValidate.NotificationBoxType = NotificationBoxType.Info;
                }
                else
                {
                    nbValidate.NotificationBoxType = NotificationBoxType.Warning;
                }

                nbValidate.Visible = true;
                nbValidate.Title = data.Status;
                nbValidate.Text = data.Message;
            }
            else
            {
                nbValidate.Visible = true;
                nbValidate.Text = "There was an issue with your request. Please contact CIA for assistance.";
            }
        }

        protected void btnShowNew_Click( object sender, EventArgs e )
        {
            pnlEdit.Visible = false;
            pnlNew.Visible = true;
            btnShowNew.Visible = false;
            btnShowEdit.Visible = true;
        }

        protected void btnShowEdit_Click( object sender, EventArgs e )
        {
            pnlEdit.Visible = true;
            pnlNew.Visible = false;
            btnShowNew.Visible = true;
            btnShowEdit.Visible = false;
        }
    }
}