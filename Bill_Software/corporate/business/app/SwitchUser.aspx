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
        .switch-user-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 2px solid #2268a9;
        }
        .switch-user-header h2 {
            margin: 0;
            color: #2268a9;
            font-size: 20px;
        }
        .impersonation-banner {
            background: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 6px;
            padding: 12px 16px;
            margin-bottom: 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .impersonation-banner span {
            color: #856404;
            font-weight: bold;
        }
        .btn-switch-back {
            background: #dc3545;
            color: #fff;
            border: none;
            padding: 6px 16px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
        }
        .btn-switch-back:hover { background: #c82333; }
        .search-box {
            width: 100%;
            padding: 10px 14px;
            border: 2px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
            margin-bottom: 15px;
            box-sizing: border-box;
        }
        .search-box:focus {
            border-color: #2268a9;
            outline: none;
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
            cursor: pointer;
            transition: background 0.15s;
        }
        .user-row:hover { background: #f0f7ff; }
        .user-row:last-child { border-bottom: none; }
        .user-info { display: flex; flex-direction: column; }
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
            white-space: nowrap;
        }
        .btn-switch:hover { background: #1a4f80; }
        .no-results {
            padding: 20px;
            text-align: center;
            color: #999;
            font-style: italic;
        }
        .search-hint {
            font-size: 12px;
            color: #888;
            margin-bottom: 10px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="switch-user-container">
        <div class="switch-user-header">
            <h2>🔄 Switch User</h2>
        </div>

        <asp:Panel ID="pnlImpersonating" runat="server" Visible="false">
            <div class="impersonation-banner">
                <span>⚠️ You are currently impersonating: <asp:Label ID="lblImpersonatingUser" runat="server" /></span>
                <asp:Button ID="btnSwitchBack" runat="server" Text="↩ Switch Back" CssClass="btn-switch-back" OnClick="btnSwitchBack_Click" />
            </div>
        </asp:Panel>

        <p class="search-hint">Search by name or User ID to find the account you want to switch to.</p>
        <asp:TextBox ID="txtSearch" runat="server" CssClass="search-box" placeholder="Search users by name or ID..." AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
        
        <div class="user-results">
            <asp:Repeater ID="rptUsers" runat="server">
                <ItemTemplate>
                    <div class="user-row">
                        <div class="user-info">
                            <span class="user-name"><%# Eval("Name") %></span>
                            <span class="user-id">ID: <%# Eval("User_Id") %></span>
                            <span class="user-role"><%# Eval("RoleName") %></span>
                        </div>
                        <asp:Button ID="btnSwitch" runat="server" Text="Switch" CssClass="btn-switch"
                            CommandArgument='<%# Eval("User_Id") %>' OnClick="btnSwitch_Click"
                            OnClientClick="return confirm('Switch to <%# Eval("Name") %> (<%# Eval("User_Id") %>)?');" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoResults" runat="server" CssClass="no-results" Visible="false" Text="No users found. Try a different search term." />
        </div>

        <asp:Label ID="lblStatus" runat="server" CssClass="no-results" Visible="false" />
    </div>
</asp:Content>
