<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm27" %>

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
                <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
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
                </asp:DataList>--%>

                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemDataBound="DataList1_ItemDataBound">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table class="table1" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <th style="text-align: center; width: 3%;">Sl</th>
                                <th style="text-align: center; width: 14%;">Customer Name</th>
                                <th style="text-align: center; width: 10%;">Invoice Date</th>
                                <th style="text-align: center; width: 16%;">Invoice / Quotation Info</th>
                                <th style="text-align: center; width: 12%;">ARC / PO / DO</th>
                                <th style="text-align: center; width: 12%;">Amount Summary</th>
                                <th style="text-align: center; width: 10%;">Validity Period</th>
                                <th style="text-align: center; width: 10%;">Created By</th>
                                <th style="text-align: center; width: 4%;">Buyer View</th>
                                <th style="text-align: center; width: 4%;">Seller View</th>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table class="table2" width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="text-align: center; width: 3%;">
                                    <asp:Label ID="lblSlNo" runat="server"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 14%;">
                                    <asp:Label ID="LabelClient" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="LabelInvoiceDate" runat="server" Text='<%# Eval("Invoice_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 16%;">
                                    <strong>Inv:</strong>
                                    <asp:Label ID="LabelInvoiceNo" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label><br />
                                    <strong>Quot:</strong>
                                    <asp:Label ID="LabelQuotationNo" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label><br />
                                    <asp:Label ID="LabelServiceName" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">ARC:
                                    <asp:Label ID="LabelPO" runat="server" Text='<%# Eval("PO_Number") %>'></asp:Label><br />
                                    PO/DO:
                                    <asp:Label ID="LabelDO" runat="server" Text='<%# Eval("DO_Number") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">
                                    <span>Taxable:</span> Rs.<asp:Label ID="LabelSubtotal" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label><br />
                                    <span>Tax:</span> Rs.<asp:Label ID="LabelGST" runat="server" Text='<%# Eval("Gst") %>'></asp:Label><br />
                                    <span>Total:</span> Rs.<asp:Label ID="LabelNet" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="LabelValidityStart" runat="server" Text='<%# Eval("Validity_StartDate") %>'></asp:Label><br />
                                    to<br />
                                    <asp:Label ID="LabelValidityEnd" runat="server" Text='<%# Eval("Validity_EndDate") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="LabelAddedBy" runat="server" Text='<%# Eval("AddedByName") %>'></asp:Label><br />
                                    on<br />
                                    <asp:Label ID="LabelTimestamp" runat="server" Text='<%# Convert.ToDateTime(Eval("TimeStamp")).ToString("dd-MMM-yyyy hh:mm tt") %>' />
                                </td>
                                <td style="text-align: center; width: 4%;">
                                    <a href="#" title="Buyer View" onclick="window.open('/corporate/business/print/NewInvoice.aspx?ID=<%# DataBinder.Eval(Container.DataItem,"ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                        <img alt="Buyer View" height="25px" src="../WebImages/viewicon.png" />
                                    </a>
                                </td>
                                <td style="text-align: center; width: 4%;">
                                    <a href="#" title="Seller View" onclick="window.open('/corporate/business/print/NewInvoiceDuplicate.aspx?ID=<%# DataBinder.Eval(Container.DataItem,"ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                        <img alt="Seller View" height="25px" src="../WebImages/viewicon.png" />
                                    </a>
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
