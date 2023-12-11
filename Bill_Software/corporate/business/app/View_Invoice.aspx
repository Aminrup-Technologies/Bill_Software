<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm27" %>
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
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Invoice</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
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
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label2" runat="server" Text="CLIENT NAME"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;"> 
                                    <asp:Label ID="Label6" runat="server" Text="TAX INVOICE NUMBER"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label3" runat="server" Text="TAX INVOICE DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showid" runat="server" Text="QUOTATION NUMBER"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showrm" runat="server" Text="QUOTATION DATE"></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label11" runat="server" Text="PRODUCT CATEGORY"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label12" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label13" runat="server" Text="GST (INR)"></asp:Label>
                                </td>
                                
                                
                                <td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label1" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label10" runat="server" Text="LAST MAILER DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:4%;">
                                    <asp:Label ID="edit" runat="server" Text="Buyers View"></asp:Label>
                                </td>
                                <td style="text-align:center; width:4%;">
                                    <asp:Label ID="Label9" runat="server" Text="Sellers View"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;"> 
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;"> 
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("Invoice_Date") %>'></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_Date") %>'></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;"> 
                                    <asp:Label ID="Label14" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Gst") %>'></asp:Label>
                                </td>
                               
                                
                                <td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:6%;">
                                    <asp:Label ID="status2" runat="server" Text='<%# Eval("mailDate") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:4%;">
                                    <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewInvoice.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                <td style="text-align:center; width:4%;">
                                    <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewInvoiceDuplicate.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
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
