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
                                    <img src="{{ '~/Assets/Images/CIAResearch.png' | ResolveRockUrl }}" style="max-width: 35%;margin:0 auto 16px;display:block;">
                                    <p><a href="https://ciaresearch.com" target="_blank">CIA</a> utilizes the latest technology to exchange up-to-date information and ensure prompt turn-around. We succeed by combining our experienced personnel with our highly efficient technology which allows us to qualify, deliver, and manage every step of the background screening process.</p>
                                    </Rock:Lava>
                                </div>
                                <div class="col-md-5 col-md-offset-1 col-sm-6">
                                    <Rock:NotificationBox runat="server" ID="nbValidate" Visible="false" />
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
