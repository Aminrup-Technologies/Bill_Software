<%@ Page Title="" Language="C#" MasterPageFile="~/admin/card.Master" AutoEventWireup="true" CodeBehind="Setting.aspx.cs" Inherits="Bill_Software.admin.WebForm8" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1
        {
            width: 100%;
        }
        .style2
        {
            height: 24px;
        }
        .style3
        {
            height: 25px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#CCCCCC" colspan="4">
                &nbsp;Settings</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
            <td width="25%">
                &nbsp;</td>
        </tr>
        <tr>
            <td width="25%">
                </td>
            <td width="25%" bgcolor="#E8E8E8">
                Name</td>
            <td width="25%" bgcolor="#E8E8E8">
                <asp:Image ID="imgName" runat="server" style="float:right"
            onclick="window.open('/admin/Update/name.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png" 
                    ToolTip="Update Your Name.." />
                &nbsp;<asp:Label ID="lblName" runat="server"></asp:Label>
                
                </td>
            <td width="25%">
                            &nbsp;</td>
        </tr>
        <tr>
            <td width="25%" class="style2">
                </td>
            <td width="25%" bgcolor="#DDDDDD" class="style2">
                &nbsp;Password&nbsp;</td>
            <td width="25%" bgcolor="#DDDDDD" class="style2">
                <asp:Image ID="imgPassword" runat="server" style="float:right"
             onclick="window.open('/admin/Update/password.aspx','popupwindow','width=520px,height=320px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png" 
                    ToolTip="Update Your Password.." />
                &nbsp;●●●●●●●●&nbsp;</td>
            <td width="25%" class="style2">
                &nbsp;</td>
        </tr>
        <tr>
            <td class="style2">
                </td>
            <td bgcolor="#E8E8E8" class="style2">
                &nbsp;Contact No.&nbsp;</td>
            <td bgcolor="#E8E8E8" class="style2">
                <asp:Image ID="imgContactNo" runat="server" style="float:right"
             onclick="window.open('/admin/Update/contactno.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png" 
                    ToolTip="Update Your Contact No.." />
                &nbsp;<asp:Label ID="lblContactNo" runat="server"></asp:Label>
                &nbsp;</td>
            <td class="style2">
                </td>
        </tr>
        <tr>
            <td class="style2">
                </td>
            <td bgcolor="#DDDDDD" class="style2">
                &nbsp;Email ID&nbsp;</td>
            <td bgcolor="#DDDDDD" class="style2">
                <asp:Image ID="imgEmailID" runat="server" style="float:right"
            onclick="window.open('/admin/Update/emailid.aspx','popupwindow','width=510px,height=310px,scrollbars=yes');return true"
                    ImageUrl="~/corporate/business/WebImages/settings_icon.png" 
                    ToolTip="Update Your Email ID.." />
                &nbsp;<asp:Label ID="lblEmailID" runat="server"></asp:Label>
                &nbsp;</td>
            <td class="style2">
                </td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
</asp:Content>
