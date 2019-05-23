<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CIAResearchSettings.ascx.cs" Inherits="com_ciaresearch.Blocks.BackgroundCheck.CIAResearchSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maUpdated" runat="server" />
        <asp:Panel ID="pnlWrapper" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user-shield"></i>CIA Background Checks</h1>
                <div class="pull-right">
                    <asp:LinkButton ID="btnDefault" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnDefault_Click">Enable As Default Background Check Provider</asp:LinkButton>
                </div>
            </div>
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbNotification" runat="server" Title="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
            <div class="panel-body">
                <asp:Panel ID="pnlToken" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:Lava ID="lavaDescription" runat="server">
                                    <img src="{{ '~/Plugins/com_ciaresearch/BackgroundCheck/CIAResearch.png' | ResolveRockUrl }}" style="max-width: 35%;margin:0 auto 16px;display:block;">
                                    <p><a href="https://ciaresearch.com" target="_blank">CIA</a> offers customized volunteer and employee background check packages. If you have not already discussed the available options for your organization please contact our sales department at 214-382-2727 ext. 2 or <a href="mailto:Sales@ciaresearch.com">Sales@ciaresearch.com</a>.</p>
                                    </Rock:Lava>
                                    <asp:LinkButton runat="server" ID="btnShowNew" CssClass="btn btn-primary" OnClick="btnShowNew_Click" CausesValidation="false" ValidationGroup="None">Account Request</asp:LinkButton>
                                    <asp:LinkButton runat="server" ID="btnShowEdit" CssClass="btn btn-primary" Visible="false" OnClick="btnShowEdit_Click" CausesValidation="false" ValidationGroup="None">Enter CIA Credentials</asp:LinkButton>
                                    <br />
                                </div>
                                <div class="col-md-6 ">
                                    <br />
                                    <Rock:NotificationBox runat="server" ID="nbValidate" Visible="false" />
                                    <asp:Panel ID="pnlNew" runat="server" Visible="false" CssClass="alert alert-info">
                                        <br />
                                        <h3>Account Request </h3>
                                        <Rock:RockDropDownList runat="server" ID="ddlNewRequestType" Label="Request Type" Required="true">
                                            <asp:ListItem Text="New Account" Value="NEW" />
                                            <asp:ListItem Text="Update Existing Account" Value="UPDATE" />
                                        </Rock:RockDropDownList>
                                        <Rock:RockTextBox ID="tbNewUserName" runat="server" Label="UserName" Required="true" />
                                        <Rock:RockTextBox ID="tbNewPassword" runat="server" Label="Password" Required="true" TextMode="Password" />
                                        <Rock:RockTextBox ID="tbNewPassword2" runat="server" Label="Re-Enter Password" Required="true" TextMode="Password" />
                                        <Rock:RockTextBox ID="tbNewClientName" runat="server" Label="Client Name" Required="true" />
                                        <Rock:RockTextBox ID="tbNewBranchName" runat="server" Label="Branch Name" Required="true" />
                                        <Rock:RockTextBox ID="tbNewFirstName" runat="server" Label="First Name" Required="true" />
                                        <Rock:RockTextBox ID="tbNewMiddleInitial" runat="server" Label="Middle Initial" />
                                        <Rock:RockTextBox ID="tbNewLastName" runat="server" Label="Last Name" Required="true" />
                                        <Rock:RockTextBox ID="tbNewEmail" runat="server" Label="Client Email" Required="true" />
                                        <Rock:RockTextBox ID="tbNewEmail2" runat="server" Label="Alternate Email" Required="true" />
                                        <Rock:AddressControl ID="aNewAddress" runat="server" Label="Address" Required="true" />
                                        <Rock:PhoneNumberBox ID="pnNewPhoneNumber" runat="server" Label="Phone Number" />
                                        <div class="actions">
                                            <asp:LinkButton ID="btnSaveNew" runat="server" CssClass="btn btn-primary" OnClick="btnSaveNew_Click">Submit</asp:LinkButton>
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="pnlEdit" runat="server">
                                        <Rock:RockTextBox ID="tbUserName" runat="server" Label="UserName" Required="true" />
                                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" TextMode="Password" />
                                        <Rock:RockTextBox ID="tbClientName" runat="server" Label="Default Client Name" Required="true" />
                                        <Rock:RockTextBox ID="tbBranchName" runat="server" Label="Default Branch Name" Required="true" />
                                        <Rock:RockTextBox ID="tbClientContact" runat="server" Label="Default Client Contact" Required="true" />
                                        <Rock:RockTextBox ID="tbClientContactEmail" runat="server" Label="Default Client Contact Email" Required="true" />
                                        <div class="actions">
                                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click">Save</asp:LinkButton>
                                            <asp:LinkButton ID="btnValidate" runat="server" CssClass="btn btn-default" OnClick="btnValidate_Click">Validate</asp:LinkButton>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlPackages" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Literal ID="lViewColumnLeft" runat="server" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-primary" OnClick="btnEdit_Click">Edit</asp:LinkButton>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Background Check Types" />
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
