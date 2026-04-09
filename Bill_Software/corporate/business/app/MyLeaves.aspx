<%@ Page Title="My Leaves" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="MyLeaves.aspx.cs" Inherits="Bill_Software.corporate.business.app.MyLeaves" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* Strictly matched to the 950px uniform container */
        .dashboard-container { max-width: 950px; margin: 30px auto; font-family: 'Segoe UI', Arial, sans-serif; }
        
        /* Matched Card Styling */
        .section-card { 
            background: #ffffff; padding: 25px; border-radius: 10px; 
            box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; margin-bottom: 30px; 
        }
        
        /* Matched Header Styling */
        .section-title { 
            color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; 
            padding-bottom: 10px; margin-bottom: 20px; font-size: 22px; 
        }

        /* Form Layout */
        .form-row { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 15px; }
        .form-group { flex: 1; min-width: 200px; display: flex; flex-direction: column; }
        .form-group label { font-weight: bold; margin-bottom: 8px; color: #555; font-size: 14px; }
        
        .form-control { 
            padding: 12px; border: 1px solid #ccc; border-radius: 6px; 
            font-size: 14px; transition: border-color 0.3s; 
        }
        .form-control:focus { border-color: #19658A; outline: none; }

        /* Unified Button Styling */
        .btn-action { 
            padding: 12px 30px; font-size: 16px; font-weight: bold; border: none; border-radius: 8px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white; 
            background: linear-gradient(135deg, #19658A, #124B68); 
        }
        .btn-action:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 6px 15px rgba(0,0,0,0.2); }

        /* Matched GridView Styling */
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }

        /* Status Badge Colors for the History Grid */
        .status-pending { color: #fd7e14; font-weight: bold; }
        .status-approved { color: #28a745; font-weight: bold; }
        .status-rejected { color: #dc3545; font-weight: bold; }
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
            <h2 class="section-title">📝 Apply for Leave</h2>
            <div class="form-row">
                <div class="form-group">
                    <label>Leave Type</label>
                    <asp:DropDownList ID="ddlLeaveType" runat="server" CssClass="form-control" Required="true"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Start Date</label>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>End Date</label>
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" Required="true"></asp:TextBox>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group" style="flex: 100%;">
                    <label>Reason for Leave</label>
                    <asp:TextBox ID="txtReason" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="Please provide a brief reason..." Required="true"></asp:TextBox>
                </div>
            </div>
            <div style="text-align: right;">
                <asp:Button ID="btnSubmitLeave" runat="server" Text="🚀 Submit Application" CssClass="btn-action" OnClick="btnSubmitLeave_Click" />
            </div>
        </div>

        <div style="display: flex; gap: 30px; flex-wrap: wrap;">
            
            <div class="section-card" style="flex: 1; min-width: 300px;">
                <h3 class="section-title">📊 My Leave Balances</h3>
                <asp:GridView ID="gvBalances" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No balances found for this year.">
                    <Columns>
                        <asp:BoundField DataField="LeaveName" HeaderText="Leave Type" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="TotalAllotted" HeaderText="Allotted" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="UsedDays" HeaderText="Used" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="BalanceDays" HeaderText="Available" DataFormatString="{0:F1}" ItemStyle-ForeColor="#19658A" ItemStyle-Font-Bold="true" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="section-card" style="flex: 2; min-width: 400px;">
                <h3 class="section-title">🕒 Application History</h3>
                <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="You haven't submitted any leave requests yet.">
                    <Columns>
                        <asp:BoundField DataField="AppliedOn" HeaderText="Applied Date" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="LeaveName" HeaderText="Type" />
                        <asp:TemplateField HeaderText="Duration">
                            <ItemTemplate>
                                <%# Eval("StartDate", "{0:dd-MMM}") %> to <%# Eval("EndDate", "{0:dd-MMM}") %> (<%# Eval("TotalDays", "{0:F1}") %> days)
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# GetStatusCssClass(Eval("RequestStatus").ToString()) %>'>
                                    <%# Eval("RequestStatus") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            
        </div>
    </div>
</asp:Content>