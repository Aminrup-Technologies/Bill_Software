<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GlobalNotification.ascx.cs" Inherits="Bill_Software.corporate.business.app.GlobalNotification" %>

<asp:Repeater ID="rptNotifications" runat="server" OnItemCommand="rptNotifications_ItemCommand">
    <ItemTemplate>
        <div class="erp-alert erp-alert-<%# Eval("Severity").ToString().ToLower() %>">
            <strong><%# Eval("Title") %></strong><br />
            <span class="erp-alert-msg"><%# Eval("Message") %></span>

            <asp:LinkButton
    ID="btnDismiss"
    runat="server"
    Text="✕"
    CssClass="erp-alert-close"
    OnClick="btnDismiss_Click" />

        </div>
    </ItemTemplate>
</asp:Repeater>