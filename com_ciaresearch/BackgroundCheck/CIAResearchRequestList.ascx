<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CIAResearchRequestList.ascx.cs" Inherits="com_ciaresearch.Blocks.BackgroundCheck.CIAResearchRequestList" %>

<style>
    .clearinside a{
        visibility:hidden
    }
</style>


<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-text-o"></i>Requests</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="fRequest" runat="server">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:DateRangePicker ID="drpRequestDates" runat="server" Label="Requested Date Range" />
                            <Rock:DateRangePicker ID="drpResponseDates" runat="server" Label="Completed Date Range" />
                        </Rock:GridFilter>

                        <Rock:Grid ID="gRequest" runat="server" AllowSorting="true" PersonIdField="PersonId" OnRowDataBound="gRequest_RowDataBound">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" HtmlEncode="false" />
                                <Rock:RockBoundField DataField="TrackingNumber" HeaderText="Tracking Number" />
                                <Rock:RockBoundField DataField="RequestDate" HeaderText="Requested" />
                                <Rock:RockBoundField DataField="ResponseDate" HeaderText="Completed" />
                                <Rock:LinkButtonField HeaderText="Report" Text="<i class='fa fa-file-pdf-o fa-lg'></i>" OnClick="gRequest_Data" />
                                <Rock:LinkButtonField HeaderText="Link" Text="<i class='fa fa-file-alt fa-lg'></i>" OnClick="gRequest_Link" />
                                <Rock:LinkButtonField HeaderText="Clear" Text="<i class='fa fa-redo-alt fa-lg'></i>" OnClick="gRequest_Rerequest" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
