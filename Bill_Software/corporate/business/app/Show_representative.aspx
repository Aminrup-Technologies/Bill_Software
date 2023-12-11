<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Show_representative.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm60" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2
        {
            width: 100%;
        }
        .style3
        {
            color: #FFFFFF;
            font-weight: bold;
        }
        .style4
        {
            text-align: center;
        }
         .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
        .style5
        {
            height: 20px;
        }
       
    </style>
     <script type="text/javascript">
         function ValidateDelete1() {
             var answer = confirm("Want to Delete this Representative?");
             if (!answer) {
                 return false;
             }
         }
</script>

 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <table class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp;<span class="style3">Show Representative Details Of </span>
                <asp:Label ID="lblComId" runat="server" Font-Bold="True" ForeColor="White"></asp:Label>
                &nbsp;</td>
        </tr>
        <tr>
            <td width="20%">
                &nbsp;</td>
            <td width="30%">
                &nbsp;</td>
            <td width="30%">
                &nbsp;</td>
            <td width="20%">
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
        
            </td>
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
            <td class="style5">
                </td>
            <td class="style5">
                &nbsp;Client Name&nbsp;</td>
            <td class="style5">
                <asp:Label ID="lblCompanyGroupName" runat="server"></asp:Label>
            </td>
            <td class="style5">
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
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%" 
                    onitemcommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="showrm" runat="server" Text="Representative Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label9" runat="server" Text="Designation"></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label1" runat="server" Text="Phone No"></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label2" runat="server" Text="Email ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:16%;">
                                    <asp:Label ID="Label3" runat="server" Text="Edit"></asp:Label>
                                </td>
                                <td style="text-align:center; width:16%;">
                                    <asp:Label ID="Label5" runat="server" Text="Delete"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Representative_name") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Designation") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Phone_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:14%;">
                                    <asp:Label ID="Label6" runat="server" Text='<%# Eval("Email") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:16%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("ID") %>' CommandName="Edit" ImageUrl="~/corporate/business/WebImages/edit_icon.png" ToolTip="Edit" />
                                </td>
                                <td style="text-align:center; width:16%;">
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandName="Delete" CommandArgument='<%# Eval("ID") %>' 
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" onclientclick="return ValidateDelete1();"/>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
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
            <td class="style4" colspan="2">
                <asp:Button ID="btnBack" runat="server" CssClass="btn_style" 
                    onclick="btnBack_Click" Text="Back" />
            </td>
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
