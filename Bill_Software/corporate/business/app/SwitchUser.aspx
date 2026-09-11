<%@ Page Title="Switch User" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="SwitchUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.SwitchUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .switch-user-container {
            max-width: 700px;
            margin: 30px auto;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 12px rgba(0,0,0,0.1);
            padding: 30px;
        }
        .switch-user-container h2 { margin: 0 0 16px 0; color: #2268a9; font-size: 20px; }
        .search-box {
            width: 100%;
            padding: 10px 14px;
            border: 2px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
            margin-bottom: 15px;
            box-sizing: border-box;
        }
        .user-results {
            max-height: 400px;
            overflow-y: auto;
            border: 1px solid #eee;
            border-radius: 6px;
        }
        .user-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 10px 14px;
            border-bottom: 1px solid #f0f0f0;
        }
        .user-name { font-weight: bold; color: #333; font-size: 14px; }
        .user-id { font-size: 12px; color: #888; }
        .user-role { font-size: 11px; color: #2268a9; }
        .btn-switch {
            background: #2268a9;
            color: #fff;
            border: none;
            padding: 5px 14px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 12px;
            font-weight: bold;
        }
        .no-results { padding: 20px; text-align: center; color: #999; }
        .search-hint { font-size: 12px; color: #888; margin-bottom: 10px; }
        .status-msg { display: block; margin-top: 12px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="switch-user-container">
        <h2>Switch User</h2>

        <asp:Panel ID="pnlDisabled" runat="server" Visible="true">
            <p style="margin:0;color:#555;">This function is not available.</p>
        </asp:Panel>

        <asp:Panel ID="pnlActive" runat="server" Visible="false">
            <p class="search-hint">Search by name or User ID. Start uses the impersonation runtime (INV-13–17).</p>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="search-box" placeholder="Search users by name or ID..." AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
            <div class="user-results">
                <asp:Repeater ID="rptUsers" runat="server">
                    <ItemTemplate>
                        <div class="user-row">
                            <div>
                                <span class="user-name"><%# Eval("Name") %></span>
                                <div class="user-id">ID: <%# Eval("User_Id") %></div>
                                <div class="user-role"><%# Eval("RoleName") %></div>
                            </div>
                            <asp:Button ID="btnSwitch" runat="server" Text="Switch" CssClass="btn-switch"
                                CommandArgument='<%# Eval("Id") %>' OnClick="btnSwitch_Click" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoResults" runat="server" CssClass="no-results" Visible="false" Text="No users found." />
            </div>
            <asp:Label ID="lblStatus" runat="server" CssClass="status-msg" Visible="false" />
        </asp:Panel>
    </div>
</asp:Content>
