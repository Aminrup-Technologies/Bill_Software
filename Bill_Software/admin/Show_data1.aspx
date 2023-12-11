<%@ Page Title="" Language="C#" MasterPageFile="~/admin/card.Master" AutoEventWireup="true" CodeBehind="Show_data1.aspx.cs" Inherits="Bill_Software.admin.WebForm9" %>
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
        .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
    </style>
        
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#CCCCCC" colspan="4">
                &nbsp;Show Data</td>
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
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" onitemcommand="DataList1_ItemCommand" Width="100%">
                    <FooterStyle BackColor="#CCCCCC" />
                    <AlternatingItemStyle BackColor="#CCCCCC" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#000099" ForeColor="White" />
                    <HeaderStyle BackColor="#3C3C3C" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Type" runat="server" Text="Employee Id"></asp:Label>
                                </td>
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="showrm" runat="server" Text="Employee Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="address" runat="server" Text="Designation"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="VATno" runat="server" Text="Depertment"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="CSTno" runat="server" Text="Image Status"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit" runat="server" Text="Image Upload"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label2" runat="server" Text="Print"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="showroomid" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("Emp_ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="proname" runat="server" Text='<%# Eval("Designation") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("Depertment") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("image_status") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:10%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("ID") %>' CommandName="Edit" ImageUrl="~/Images/edit_icon.png" ToolTip="Edit" />
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <a href = "#" title="Print ID Card..." onclick="window.open('/Print/id_card1.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="/Images/print.png" /></a>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
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
            <td>
                &nbsp;</td>
            <td colspan="2" style="text-align: center">
                <%--<a href = "#" title="Print ID Card..." onclick="window.open('/admin/Total_id_card.aspx?ID=, 'popupwindow','scrollbars=yes');return true">
                                                <img alt="" height="25px" src="/Images/print.png" /></a>--%>
                <asp:Image ID="Image2" runat="server" Height="49px" 
                    ImageUrl="~/Images/print.png" 
                    onclick="window.open('/admin/Total_id_card.aspx','popupwindow',',scrollbars=yes');return true" 
                    ToolTip="Print ID Card.." Width="55px" />
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
