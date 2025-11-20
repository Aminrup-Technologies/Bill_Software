<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="settings.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp; <span class="style2">Settings</span> </td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%" bgcolor="#94B8FF">&nbsp;Name&nbsp;</td>
            <td width="25%" bgcolor="#94B8FF">
                <asp:Image ID="imgName" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/name.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Name.." />
                &nbsp;<asp:Label ID="lblName" runat="server"></asp:Label>

            </td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td width="25%">&nbsp;</td>
            <td width="25%">&nbsp;Password&nbsp;</td>
            <td width="25%">
                <asp:Image ID="imgPassword" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/password.aspx','popupwindow','width=520px,height=320px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Password.." />
                &nbsp;●●●●●●●●
            </td>
            <td width="25%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td bgcolor="#94B8FF">&nbsp;Contact No.&nbsp;</td>
            <td bgcolor="#94B8FF">
                <asp:Image ID="imgContactNo" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/contactno.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Contact No.." />
                &nbsp;<asp:Label ID="lblContactNo" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Email ID&nbsp;</td>
            <td>
                <asp:Image ID="imgEmailID" runat="server" Style="float: right"
                    onclick="window.open('/corporate/business/app/Update/emailid.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png"
                    ToolTip="Update Your Email ID.." />
                &nbsp;<asp:Label ID="lblEmailID" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
