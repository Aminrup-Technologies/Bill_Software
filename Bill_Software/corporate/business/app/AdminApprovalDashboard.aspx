<%@ Page Title="Approvals Dashboard" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminApprovalDashboard.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminApprovalDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .dashboard-container { max-width: 950px; margin: 30px auto; font-family: 'Segoe UI', Arial, sans-serif; }
        .section-card { 
            background: #ffffff; padding: 25px; border-radius: 10px; 
            box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; margin-bottom: 30px; 
        }
        .section-title { 
            color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; 
            padding-bottom: 10px; margin-bottom: 20px; font-size: 22px; 
        }
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }
        .btn-action-small {
            padding: 8px 15px; font-size: 13px; font-weight: bold; border: none; border-radius: 6px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white; margin-right: 5px; margin-bottom: 5px;
        }
        .btn-action-small:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 4px 10px rgba(0,0,0,0.15); }
        .btn-approve { background: linear-gradient(135deg, #34ce57, #28a745); }
        .btn-reject { background: linear-gradient(135deg, #ff6b6b, #dc3545); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        
       <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" style="margin-bottom: 20px; border-radius: 6px; text-align: center;">
            <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
        </asp:Panel>
        
        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" BackColor="#FFDDDD" style="margin-bottom: 20px; border-radius: 6px; text-align: center;">
            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
        </asp:Panel>

        <div class="section-card">
            <h2 class="section-title">⏱️ Pending Attendance Corrections</h2>
            <asp:GridView ID="gvRegularizations" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending attendance corrections." OnRowCommand="gvRegularizations_RowCommand" DataKeyNames="RequestID">
                <Columns>
                    <asp:BoundField DataField="UserCode" HeaderText="Emp ID"/>
                    <asp:BoundField DataField="Name" HeaderText="Employee Name" />
                    <asp:BoundField DataField="AttendanceDate" HeaderText="Missed Date" DataFormatString="{0:dd-MMM-yyyy}" />
                    <asp:BoundField DataField="RequestedInTime" HeaderText="Req. IN" />
                    <asp:BoundField DataField="RequestedOutTime" HeaderText="Req. OUT" />
                    <asp:BoundField DataField="Reason" HeaderText="Reason" />
                    
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnApproveReg" runat="server" Text="✔ Approve" CssClass="btn-action-small btn-approve" CommandName="ApproveReq" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Are you sure you want to approve this correction?');" />
                            <asp:Button ID="btnRejectReg" runat="server" Text="✖ Reject" CssClass="btn-action-small btn-reject" CommandName="RejectReq" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Are you sure you want to reject this correction?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="section-card">
            <h3 class="section-title">🏖️ Pending Leave Applications</h3>
            <asp:GridView ID="gvLeaves" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending leave applications." OnRowCommand="gvLeaves_RowCommand" DataKeyNames="RequestID">
                <Columns>
                    <asp:BoundField DataField="UserCode" HeaderText="Emp ID"/>
                    <asp:BoundField DataField="Name" HeaderText="Employee Name" />
                    <asp:BoundField DataField="LeaveName" HeaderText="Leave Type" />
                    <asp:BoundField DataField="StartDate" HeaderText="Start Date" DataFormatString="{0:dd-MMM-yyyy}" />
                    <asp:BoundField DataField="TotalDays" HeaderText="Days" />
                    <asp:BoundField DataField="Reason" HeaderText="Reason" />
                    
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnApproveLeave" runat="server" Text="✔ Approve" CssClass="btn-action-small btn-approve" CommandName="ApproveLeave" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Approve this leave and deduct from balance?');" />
                            <asp:Button ID="btnRejectLeave" runat="server" Text="✖ Reject" CssClass="btn-action-small btn-reject" CommandName="RejectLeave" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Reject this leave?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

    </div>
</asp:Content>