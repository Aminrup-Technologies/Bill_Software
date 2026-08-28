<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Purchess_due.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm53" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2 {
            width: 100%;
        }

        .style3 {
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
    <table cellpadding="0" cellspacing="0" class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp; <span class="style3">Purchesse Due</span>&nbsp;</td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD"
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server"
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>

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
            <td colspan="4">
                <div id="main_div" runat="server" style="width: 100%; overflow: auto;">
                    <div id="first_div" runat="server">
                        <table class="style2">
                            <tr>
                                <td colspan="4">
                                    <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                                        BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                                        ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                                        <FooterStyle BackColor="White" ForeColor="#000066" />
                                        <AlternatingItemStyle BackColor="#94B8FF" />
                                        <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                                        <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                                        <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                                        <HeaderTemplate>
                                            <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                                <tr>
                                                <td style="text-align:center; width:13%;">
                                                        <asp:Label ID="Label7" runat="server" Text="Purchesse ID"></asp:Label>
                                                    </td>
                                                    <td style="text-align:center; width:12%;">
                                                        <asp:Label ID="Label8" runat="server" Text="Purchesse Date"></asp:Label>
                                                    </td>
                                
                                
                                
                                                    <td style="text-align:center; width:25%;">
                                                        <asp:Label ID="showid" runat="server" Text="Vendor Name"></asp:Label>
                                                    </td>
                               
                                                    <td style="text-align:center; width:20%;">
                                                        <asp:Label ID="Label2" runat="server" Text="Total Amount"></asp:Label>
                                                    </td>
                                                    <td style="text-align:center; width:20%;">
                                                        <asp:Label ID="Label13" runat="server" Text="Due Amount"></asp:Label>
                                                    </td>
                                
                                                    <td style="text-align:center; width:10%;">
                                                        <asp:Label ID="Label1" runat="server" Text="View"></asp:Label>
                                                    </td>
                                
                                                </tr>
                                            </table>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                                <tr>
                                                <td style="text-align:center; width:13%;">
                                                        <asp:Label ID="Label9" runat="server" Text='<%# Eval("Purches_Id") %>'></asp:Label>
                                                    </td>
                                                    <td style="text-align:center; width:12%;">
                                                        <asp:Label ID="Label10" runat="server" Text='<%# Eval("Purches_date") %>'></asp:Label>
                                                    </td>
                               
                                
                                                     <td style="text-align:center; width:25%;">
                                                        <asp:Label ID="Label4" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                                    </td>
                                                    <td style="text-align:center; width:20%;">Rs.
                                                        <asp:Label ID="Label24" runat="server" Text='<%# Eval("Total_purches_rate") %>'></asp:Label> /-
                                                    </td>
                                                    <td style="text-align:center; width:20%;">Rs. 
                                                        <asp:Label ID="Label11" runat="server" Text='<%# Eval("Due_amount") %>'></asp:Label> /-
                                                    </td>
                                
                                                    <td style="text-align:center; width:10%;">
                                                         <a href = "#" title="Print Purchasse Bill..." onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# DataBinder.Eval (Container.DataItem,"Purches_Id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                                    <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                                    </td>
                                
                                                </tr>
                                            </table>
                                        </ItemTemplate>
                    
                                    </asp:DataList>--%>


                                    <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666"
                                        BorderStyle="Solid" BorderWidth="1px" Font-Size="11px"
                                        ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                                        <FooterStyle BackColor="White" ForeColor="#000066" />
                                        <AlternatingItemStyle BackColor="#94B8FF" />
                                        <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                                        <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                                        <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />

                                        <HeaderTemplate>
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                                <tr>
                                                    <td style="text-align: center; width: 12%;">Purchase Details</td>
                                                    <td style="text-align: center; width: 15%;">Order Details</td>
                                                    <td style="text-align: center; width: 20%;">Vendor & Destination</td>
                                                    <td style="text-align: center; width: 23%;">Amounts</td>
                                                    <td style="text-align: center; width: 10%;">Due Amount</td>
                                                    <td style="text-align: center; width: 12%;">Added By</td>
                                                    <td style="text-align: center; width: 8%;">View</td>
                                                </tr>
                                            </table>
                                        </HeaderTemplate>

                                        <ItemTemplate>
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                                <tr>
                                                    <td style="text-align: center; width: 12%;">ID: <%# Eval("Purches_Id") %><br />
                                                        Date: <%# Eval("Purches_date", "{0:dd-MM-yyyy}") %>
                                                    </td>

                                                    <td style="text-align: center; width: 15%;">No: <%# Eval("OrderNo") %><br />
                                                        Date: <%# Eval("OrderDate", "{0:dd-MM-yyyy}") %>
                                                    </td>

                                                    <td style="text-align: center; width: 20%;">
                                                        <%# Eval("Vendor_Name") %><br />
                                                        <small>Dest: <%# Eval("Destination") %></small>
                                                    </td>

                                                    <td style="text-align: center; width: 23%;">Taxable: ₹<%# Eval("TaxableAmount") %><br />
                                                        Tax: ₹<%# Eval("TaxAmount") %><br />
                                                        <b>Total: ₹<%# Eval("TotalAmount") %></b>
                                                    </td>

                                                    <td style="text-align: center; width: 10%;">₹<%# Eval("Due_amount") %>
                                                    </td>

                                                    <td style="text-align: center; width: 12%;">
                                                        <%# Eval("AddedByName") %><br />
                                                        <small><%# Eval("CreatedOn", "{0:dd-MM-yyyy HH:mm}") %></small>
                                                    </td>

                                                    <td style="text-align: center; width: 8%;">
                                                        <a href="#" title="Print Purches Bill"
                                                            onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# Eval("Purches_Id") %>', 
                        'popupwindow','width=900px,height=800px,scrollbars=yes');return false;">
                                                            <img alt="View" height="25px" src="../WebImages/viewicon.png" />
                                                        </a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </ItemTemplate>
                                    </asp:DataList>--%>

                                    <%--<asp:DataList ID="DataList1" runat="server"
                                        RepeatLayout="Table" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" GridLines="Both" Font-Size="11px" ForeColor="#2D2D2D" Width="100%">

                                        <HeaderTemplate>
                                            <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse; border-color: #000000; width:100%; border:1px solid black;">
                                                <tr style="background-color: #006699; color: white; font-weight: bold; text-align: center;">
                                                    <th style="width: 5%">Sl</th>
                                                    <th style="width: 15%">Purchase Details</th>
                                                    <th style="width: 20%">Order Details</th>
                                                    <th style="width: 20%">Vendor & Destination</th>
                                                    <th style="width: 15%">Amounts</th>
                                                    <th style="width: 10%">Due Amount</th>
                                                    <th style="width: 10%">Added By</th>
                                                    <th style="width: 5%">View</th>
                                                </tr>
                                        </HeaderTemplate>

                                        <ItemTemplate>
                                            <tr style='<%# Container.ItemIndex % 2 == 0 ? "background-color:#ffffff;": "background-color:#EAF3FF;" %>'>
                                                <td style="text-align: center;">
                                                    <%# Container.ItemIndex + 1 %>
                                                </td>
                                                <td>ID: <%# Eval("Purches_Id") %><br />
                                                    Date: <%# Eval("Purches_date", "{0:dd-MMM-yyyy}") %>
                                                </td>
                                                <td>No: <%# Eval("OrderNo") %><br />
                                                    Date: <%# Eval("OrderDate", "{0:dd-MMM-yyyy}") %>
                                                </td>
                                                <td>
                                                    <%# Eval("Vendor_Name") %><br />
                                                    <small>Dest: <%# Eval("Destination") %></small>
                                                </td>
                                                <td>Taxable: ₹<%# Eval("TotalAmount") %><br />
                                                    Tax: ₹<%# Eval("TaxAmount") %><br />
                                                    <b>Total: ₹<%# Eval("TaxableAmount") %></b>
                                                </td>
                                                <td>₹<%# Eval("Due_amount") %></td>
                                                <td>
                                                    <%# Eval("AddedByName") %><br />
                                                    <small><%# Eval("CreatedOn", "{0:dd-MMM-yyyy HH:mm}") %></small>
                                                </td>
                                                <td style="text-align: center;">
                                                    <a href="#" onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# Eval("Purches_Id") %>',
                    'popupwindow','width=900,height=800,scrollbars=yes');return false;">
                                                        <img alt="View" height="25px" src="../WebImages/viewicon.png" />
                                                    </a>
                                                </td>
                                            </tr>
                                        </ItemTemplate>

                                        <FooterTemplate>
                                            </table>
                                        </FooterTemplate>
                                    </asp:DataList>--%>

                                    <asp:DataList ID="DataList1" runat="server"
                                        RepeatLayout="Table" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" GridLines="Both" Font-Size="11px" ForeColor="#2D2D2D" Width="100%">
                                        <HeaderTemplate>
                                            <table style="border-collapse: collapse; width: 100%; border: 1px solid #000;">
                                                <tr style="background: #006699; color: #fff; font-weight: bold; text-align: center;">
                                                    <th style="border: 1px solid #000; width: 5%;">Sl. No.</th>
                                                    <th style="border: 1px solid #000; width: 18%;">Purchase</th>
                                                    <th style="border: 1px solid #000; width: 18%;">Order</th>
                                                    <th style="border: 1px solid #000; width: 20%;">Vendor & Ship To</th>
                                                    <th style="border: 1px solid #000; width: 22%;">Amounts</th>
                                                    <th style="border: 1px solid #000; width: 9%;">Due</th>
                                                    <th style="border: 1px solid #000; width: 8%;">Added By</th>
                                                    <th style="border: 1px solid #000; width: 8%;">View</th>
                                                </tr>
                                        </HeaderTemplate>

                                        <ItemTemplate>
                                            <tr style='<%# (Container.ItemIndex % 2 == 0) ? "background:#FFFFFF;": "background:#EAF3FF;" %>'>
                                                <!-- Sl. No. -->
                                                <td style="border: 1px solid #000; text-align: center;">
                                                    <%# Container.ItemIndex + 1 %>
                                                </td>

                                                <!-- Purchase (only tbl_Purches fields) -->
                                                <td style="border: 1px solid #000;">
                                                    <b>ID:</b> <%# Eval("Purches_Id") %><br />
                                                    <b>Date:</b> <%# Eval("Purches_date", "{0:dd-MMM-yyyy}") %><br />
                                                    <b>Type:</b> <%# Eval("Purches_Type") %><br />
                                                    <b>Invoice #:</b> <%# Eval("Invoice_No") %>
                                                </td>

                                                <!-- Order (only tbl_Purches fields) -->
                                                <td style="border: 1px solid #000;">
                                                    <b>No:</b> <%# Eval("OrderNo") %><br />
                                                    <b>Date:</b> <%# Eval("OrderDate", "{0:dd-MMM-yyyy}") %><br />
                                                    <b>Stock Added:</b> <%# Eval("Stock_Add_Date", "{0:dd-MMM-yyyy}") %>
                                                </td>

                                                <!-- Vendor + Ship To (Vendor name is from join; rest from tbl_Purches) -->
                                                <td style="border: 1px solid #000;">
                                                    <b>Vendor/:</b> <%# Eval("Vendor_Name") %><br />
                                                    <b>Ship To:</b> <%# Eval("Destination") %>
                                                    <asp:Panel runat="server" Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("Narration"))) %>'>
                                                        <br />
                                                        <b>Narration:</b> <%# Eval("Narration") %>
                                                    </asp:Panel>
                                                </td>

                                                <!-- Amounts (corrected label mapping + extra charges that exist in tbl_Purches) -->
                                                <td style="border: 1px solid #000; text-align: right;">Taxable: ₹<%# Eval("TaxableAmount", "{0:N2}") %><br />
                                                    Tax: ₹<%# Eval("TaxAmount", "{0:N2}") %><br />
                                                    <b>Total: ₹<%# Eval("TotalAmount", "{0:N2}") %></b>
                                                    <asp:Panel runat="server">
                                                        <br />
                                                        Delivery: ₹<%# Eval("Delivery_Amount", "{0:N2}") %> (<%# Eval("Delivery_Rate", "{0:N2}") %>%)
                                                        <br />
                                                        TCS: ₹<%# Eval("TCS_Amount", "{0:N2}") %> (<%# Eval("TCS_Rate", "{0:N2}") %>%)
                                                        <br />
                                                        <%# Eval("otherAmount1_name") %>: ₹<%# Eval("otherAmount1", "{0:N2}") %>
                                                        <br />
                                                        <%# Eval("otherAmount2_name") %>: ₹<%# Eval("otherAmount2", "{0:N2}") %>
                                                    </asp:Panel>
                                                </td>

                                                <!-- Due (from join you already use) -->
                                                <td style="border: 1px solid #000; text-align: center;">₹<%# Eval("Due_amount", "{0:N2}") %>
                                                </td>

                                                <!-- Added By -->
                                                <td style="border: 1px solid #000;">
                                                    <%# Eval("AddedByName") %><br />
                                                    <small><%# Eval("CreatedOn", "{0:dd-MMM-yyyy HH:mm tt}") %></small>
                                                </td>

                                                <!-- View -->
                                                <td style="border: 1px solid #000; text-align: center;">
                                                    <a href="#" title="Print Purchase Bill"
                                                        onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# Eval("Purches_Id") %>',
                   'popupwindow','width=900,height=800,scrollbars=yes');return false;">
                                                        <img alt="View" height="22" src="../WebImages/viewicon.png" />
                                                    </a>
                                                </td>
                                            </tr>
                                        </ItemTemplate>

                                        <FooterTemplate>
                                            </table>
                                        </FooterTemplate>
                                    </asp:DataList>


                                </td>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                    </div>

                </div>
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
