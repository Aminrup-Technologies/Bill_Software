<%@ Page Title="Holiday Setup" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminHolidaySetup.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminHolidaySetup" %>

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
            <h2 class="section-title">Company Holiday Calendar</h2>
            <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Style="display:block; margin-bottom: 15px;"></asp:Label>

            <div class="form-row">
                <div class="form-group">
                    <label>Holiday Date</label>
                    <asp:TextBox ID="txtHolidayDate" runat="server" CssClass="form-control" TextMode="Date" Required="true"></asp:TextBox>
                </div>
                <div class="form-group" style="flex: 2;">
                    <label>Holiday Name</label>
                    <asp:TextBox ID="txtHolidayName" runat="server" CssClass="form-control" placeholder="e.g., Republic Day" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Holiday Type</label>
                    <asp:DropDownList ID="ddlHolidayType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Public" Value="Public"></asp:ListItem>
                        <asp:ListItem Text="Restricted" Value="Restricted"></asp:ListItem>
                        <asp:ListItem Text="Company" Value="Company"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div style="text-align: right; margin-top: 15px;">
                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel Edit" CssClass="btn-action"
                    Style="background: #6c757d; display:none; margin-right: 8px;" OnClick="btnCancelEdit_Click" formnovalidate="true" />
                <asp:Button ID="btnSaveHoliday" runat="server" Text="Save Holiday" CssClass="btn-action" OnClick="btnSaveHoliday_Click" />
            </div>
        </div>

        <div class="section-card">
            <div class="form-row" style="align-items: flex-end;">
                <div class="form-group" style="max-width: 160px;">
                    <label>Filter Year</label>
                    <asp:TextBox ID="txtFilterYear" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                </div>
                <div class="form-group" style="flex: 0; min-width: auto;">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filter" CssClass="btn-action"
                        Style="padding: 12px 18px;" OnClick="btnFilter_Click" formnovalidate="true" />
                </div>
            </div>

            <h3 class="section-title">Holidays</h3>
            <asp:GridView ID="gvHolidays" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None"
                EmptyDataText="No holidays configured for this company/year." DataKeyNames="HolidayID" OnRowCommand="gvHolidays_RowCommand">
                <Columns>
                    <asp:BoundField DataField="HolidayDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="HolidayName" HeaderText="Name" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="HolidayType" HeaderText="Type" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" CommandName="EditHoliday" CommandArgument="<%# Container.DataItemIndex %>"
                                Text="Edit" CssClass="btn-action" Style="padding: 5px 10px; font-size: 12px; background: #f39c12;" formnovalidate="true" />
                            <asp:Button ID="btnDelete" runat="server" CommandName="DeleteHoliday" CommandArgument="<%# Container.DataItemIndex %>"
                                Text="Delete" CssClass="btn-action" Style="padding: 5px 10px; font-size: 12px; background: #dc3545; margin-left: 6px;"
                                OnClientClick="return confirm('Delete this holiday for the current company?');" formnovalidate="true" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
