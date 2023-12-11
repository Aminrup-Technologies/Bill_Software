<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm16" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
        .style2
    {
        color: #FFFFFF;
        font-weight: bold;
    }
         .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">
                &nbsp;<span class="style2">View Client</span>&nbsp;
            </td>
        </tr>
                <tr>
        <td width="10%">
            &nbsp;</td>
        <td  width="40%">
            &nbsp;</td>
        <td  width="40%">
            &nbsp;</td>
        <td width="10%">
            &nbsp;</td>
    </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Select Client</td>
            <td>
                <asp:DropDownList ID="cmbvendor" runat="server" AutoPostBack="True" CssClass="dropdown_style" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged">
                </asp:DropDownList>
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
            <td colspan="2">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" onitemcommand="DataList1_ItemCommand" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="showid" runat="server" Text="Client ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="showrm" runat="server" Text="Client Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="Label2" runat="server" Text="Industry type"></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="Label1" runat="server" Text="Factory Details"></asp:Label>
                                </td>
                               <td style="text-align:center; width:16%;">
                                    <asp:Label ID="Label3" runat="server" Text="Representatives Details"></asp:Label>
                                </td>
                                <td style="text-align:center; width:16%;">
                                    <asp:Label ID="edit" runat="server" Text="Edit"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Client_Id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Industry") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:17%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Factioy" CommandArgument='<%# Eval("Client_Id") %>' 
                                        ImageUrl="~/corporate/business/WebImages/factory.png" ToolTip="Show Factory" Height="35px" Width="35px"/>
                                </td>
                                 <td style="text-align:center; width:16%;">
                                    <asp:ImageButton ID="ImageButton2" runat="server" CommandName="Representative" CommandArgument='<%# Eval("Client_Id") %>' 
                                        ImageUrl="~/corporate/business/WebImages/representative.png" ToolTip="Show Representatives" Height="35px" Width="35px"/>
                                </td>
                           
                                <td style="text-align:center; width:16%;">
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandArgument='<%# Eval("Client_Id") %>' CommandName="Edit" ImageUrl="~/corporate/business/WebImages/edit_icon.png" ToolTip="Edit" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
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
    </table>
</asp:Content>
