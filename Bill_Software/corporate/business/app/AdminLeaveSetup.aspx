<%@ Page Title="Leave Policy Setup" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminLeaveSetup.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminLeaveSetup" %>

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
        
        .form-row { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 15px; }
        .form-group { flex: 1; min-width: 200px; display: flex; flex-direction: column; }
        .form-group label { font-weight: bold; margin-bottom: 8px; color: #555; font-size: 14px; }
        
        .form-control { 
            padding: 12px; border: 1px solid #ccc; border-radius: 6px; 
            font-size: 14px; transition: border-color 0.3s; 
        }
        .form-control:focus { border-color: #19658A; outline: none; }
        
        .btn-action { 
            padding: 12px 30px; font-size: 16px; font-weight: bold; border: none; border-radius: 8px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white; 
            background: linear-gradient(135deg, #19658A, #124B68); 
        }
        .btn-action:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 6px 15px rgba(0,0,0,0.2); }
        
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        
        <div class="section-card">
            <h2 class="section-title">🏖️ Configure Leave Policy</h2>
            
            <asp:Label ID="lblMessage" runat="server" Font-Bold="true" style="display:block; margin-bottom: 15px;"></asp:Label>

            <div class="form-row">
                <div class="form-group" style="flex: 2;">
                    <label>Leave Type Name (e.g., Sick Leave, Maternity Leave)</label>
                    <asp:TextBox ID="txtLeaveName" runat="server" CssClass="form-control" placeholder="Enter Leave Name" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Leave Category</label>
                    <asp:DropDownList ID="ddlIsPaid" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Paid Leave" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Unpaid Leave (Loss of Pay)" Value="0"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Maximum Days Per Year</label>
                    <asp:TextBox ID="txtMaxDays" runat="server" CssClass="form-control" TextMode="Number" step="0.5" Text="12.0" Required="true"></asp:TextBox>
                </div>
            </div>

            <div style="text-align: right; margin-top: 15px;">
                <asp:Button ID="btnSaveLeave" runat="server" Text="💾 Save Leave Type" CssClass="btn-action" OnClick="btnSaveLeave_Click" />
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-title">📋 Active Leave Categories</h3>
            <asp:GridView ID="gvLeaveTypes" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No leave policies configured.">
                <Columns>
                    <asp:BoundField DataField="LeaveID" HeaderText="ID" />
                    <asp:BoundField DataField="LeaveName" HeaderText="Leave Name" ItemStyle-Font-Bold="true" />
                    <asp:TemplateField HeaderText="Category">
                        <ItemTemplate>
                            <span style='<%# Convert.ToBoolean(Eval("IsPaid")) ? "color: #28a745; font-weight:bold;" : "color: #dc3545; font-weight:bold;" %>'>
                                <%# Convert.ToBoolean(Eval("IsPaid")) ? "Paid" : "Unpaid" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="MaxDaysPerYear" HeaderText="Max Days / Year" DataFormatString="{0:F1} Days" />
                </Columns>
            </asp:GridView>
        </div>

    </div>
</asp:Content>
