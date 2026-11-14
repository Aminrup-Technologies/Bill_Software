<%@ Page Title="Leave Balance Admin" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminLeaveBalance.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminLeaveBalance" %>

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
        .form-group { flex: 1; min-width: 180px; display: flex; flex-direction: column; }
        .form-group label { font-weight: bold; margin-bottom: 8px; color: #555; font-size: 14px; }
        .form-control {
            padding: 12px; border: 1px solid #ccc; border-radius: 6px;
            font-size: 14px; transition: border-color 0.3s;
        }
        .form-control:focus { border-color: #19658A; outline: none; }
        .btn-action {
            padding: 12px 24px; font-size: 15px; font-weight: bold; border: none; border-radius: 8px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white;
            background: linear-gradient(135deg, #19658A, #124B68);
        }
        .btn-action:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 6px 15px rgba(0,0,0,0.2); }
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }
        .hint { color: #666; font-size: 13px; margin-bottom: 12px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        <div class="section-card">
            <h2 class="section-title">Leave Balance Lifecycle</h2>
            <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Style="display:block; margin-bottom: 15px;"></asp:Label>
            <p class="hint">
                Seed or re-run allocation via <code>sp_AllocateEmployeeLeaves</code> (same path as Add User),
                then adjust <b>Total Allotted</b> for the selected company and financial year. Used days stay approval-driven.
            </p>

            <div class="form-row">
                <div class="form-group" style="flex: 2;">
                    <label>Employee</label>
                    <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="form-group" style="max-width: 160px;">
                    <label>Financial Year</label>
                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
            </div>

            <div style="text-align: right; margin-top: 10px;">
                <asp:Button ID="btnLoad" runat="server" Text="Load Balances" CssClass="btn-action"
                    Style="margin-right: 8px; background: #6c757d;" OnClick="btnLoad_Click" formnovalidate="true" />
                <asp:Button ID="btnAllocate" runat="server" Text="Allocate / Re-seed (SP)" CssClass="btn-action"
                    OnClick="btnAllocate_Click"
                    OnClientClick="return confirm('Run sp_AllocateEmployeeLeaves for this employee and year?');" />
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-title">Balances</h3>
            <asp:GridView ID="gvBalances" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None"
                EmptyDataText="No leave balances for this employee/year." DataKeyNames="BalanceID" OnRowCommand="gvBalances_RowCommand">
                <Columns>
                    <asp:BoundField DataField="LeaveName" HeaderText="Leave Type" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="UsedDays" HeaderText="Used" DataFormatString="{0:F1}" />
                    <asp:TemplateField HeaderText="Total Allotted">
                        <ItemTemplate>
                            <asp:TextBox ID="txtAllotted" runat="server" CssClass="form-control"
                                Style="padding: 6px; max-width: 100px;" TextMode="Number" step="0.5"
                                Text='<%# Eval("TotalAllotted", "{0:0.##}") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnSaveAllotted" runat="server" CommandName="SaveAllotted"
                                CommandArgument="<%# Container.DataItemIndex %>" Text="Save"
                                CssClass="btn-action" Style="padding: 5px 12px; font-size: 12px;" formnovalidate="true" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
