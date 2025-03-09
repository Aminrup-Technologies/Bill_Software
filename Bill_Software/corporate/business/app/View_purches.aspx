<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_purches.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm20" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Purchase</span></td>
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
                                <td style="text-align: center; width: 15%;">
                                    <asp:Label ID="showid" runat="server" Text="Purchase ID"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="showrm" runat="server" Text="Purchase Date"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 25%;">
                                    <asp:Label ID="Label2" runat="server" Text="Vendor Name"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 20%;">
                                    <asp:Label ID="Label6" runat="server" Text="Total Purchase Rate"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 20%;">
                                    <asp:Label ID="Label3" runat="server" Text="Total TAX"></asp:Label>
                                </td>

                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align: center; width: 15%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Purches_Id") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Purches_date") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 25%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 20%;">Rs. 
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("Total_purches_rate") %>'></asp:Label>
                                    /-
                                </td>
                                <td style="text-align: center; width: 20%;">Rs. 
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("Total_Tax_rate") %>'></asp:Label>
                                    /-
                                </td>

                                <td style="text-align: center; width: 10%;">
                                    <a href="#" title="Print Purchasse Bill..." onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# DataBinder.Eval (Container.DataItem,"Purches_Id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
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
