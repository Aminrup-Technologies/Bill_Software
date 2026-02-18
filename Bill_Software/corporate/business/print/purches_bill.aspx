<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="purches_bill.aspx.cs" Inherits="Bill_Software.corporate.business.print.purches_bill" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/i2i_logo.png" />
    <style type="text/css">
        /* --- ERROR PANEL STYLE --- */
        .error-box {
            width: 80%; margin: 50px auto; padding: 20px;
            border: 1px solid #d8000c; background-color: #ffbaba; color: #d8000c;
            text-align: center; font-family: Arial, sans-serif; font-size: 14px; border-radius: 5px;
        }

        /* --- YOUR EXISTING STYLES (UNCHANGED) --- */
        .style1 { width: 100%; }
        * { padding: 0; margin: 0; border: none; list-style: none; text-decoration: none; }
        .style5 { text-align: right; padding: 0px 5px 0px 0px; }
        .style6 { font-family: Arial, Helvetica, sans-serif; font-weight: bold; font-size: medium; color: #000037; }
        .tableOne, .tableTwo { margin: 0; border: solid 1px #bfbfbf; }
        .tableOne td { font: normal 14px/20px Calibri; background: #dbe5f1; border: solid 1px #bfbfbf; padding: 2px 0 2px 5px; }
        .tableTwo td { font: normal 12px/16px Calibri; background: #dbe5f1; border: solid 1px #bfbfbf; padding: 2px 5px 2px 5px; }
        .table_border td { border: 2px solid #bfbfbf; }
        .table1 { border-collapse: collapse; }
        .table1 td { text-align: left; border: 1px solid #bfbfbf; width: 50%; }
        .table2 { border-collapse: collapse; }
        .table2 td { text-align: left; border: 1px solid #bfbfbf; padding: 2px 0 2px 20px; }
        .style7 { font-family: Arial; font-size: small; font-weight: bold; text-align: center; }
        .style8 { text-align: center; color: #FFFFFF; font-weight: bold; font-family: Calibri; }
        .style9 { font-family: Arial, Helvetica, sans-serif; font-weight: bold; font-size: 20px; color: #e36c0a; }
        .style10 { font-family: Arial; font-size: small; font-weight: bold; }
        .style11 { font-weight: bold; }
        .style12 { font-family: "Century Gothic"; font-size: medium; font-weight: bold; }
        .style13 { color: #1c3564; }
        .style14 { color: #1c3564; }
        .style15 { color: #e36c0a; }
        .style16 { font-family: "Century Gothic"; font-size: medium; font-weight: bold; color: #1c3564; }
        .style17 { font-family: Arial; font-size: small; font-weight: bold; }
        .style18 { font-family: Arial; font-size: small; font-weight: bold; }
        .sssss { position: fixed; bottom: 0; }
        
        @media print {
            thead { display: table-header-group; }
            tfoot { display: table-footer-group; }
            .header, .hide { visibility: hidden; height: 100px; }
            .header1, .show { }
            .Foter, .hide { visibility: hidden; height: 60px; }
            .Foter1, .show { position: fixed; bottom: 0; }
        }
        @media screen {
            thead { display: block; }
            tfoot { display: block; }
        }
        @media print1 {
            #non-printable { display: none; }
            #printable { display: block; }
        }
        #footerrprint img { max-width: 100%; height: auto; }
        .auto-style2 { font-size: medium; }
        .auto-style3 { height: 17px; }
        .auto-style4 { font-family: Arial; font-size: small; font-weight: bold; height: 26px; }
        .auto-style5 { font-family: Arial; font-size: small; font-weight: bold; text-align: center; height: 26px; }
        .auto-style6 { font-family: Arial; font-size: small; font-weight: bold; height: 27px; }
        .auto-style7 { font-family: Arial; font-size: small; font-weight: bold; text-align: center; height: 27px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="error-box">
                <h3>Error Loading Invoice</h3>
                <asp:Label ID="lblErrorMsg" runat="server" Text=""></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlContent" runat="server">
            <div id="printable" style="width: 844px;">
                <table cellpadding="0" cellspacing="0" class="style1">
                    <tr>
                        <td id="Hederprint" runat="server" colspan="2">
                            <asp:Image ID="Image21" runat="server" Width="844px" Height="140px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrtop.png" />
                        </td>
                    </tr>
                    <tr><td class="style5" colspan="2">&nbsp;</td></tr>
                    <tr>
                        <td class="style5" colspan="2">
                            <span lang="en-us"><span class="style6">&nbsp;</span><span class="style9">PURCHASE INVOICE</span></span>
                        </td>
                    </tr>
                    <tr><td colspan="2">&nbsp;</td></tr>
                    <tr>
                        <td colspan="2">
                            <table width="100%" cellpadding="0" cellspacing="0" class="tableOne">
                                <tr>
                                    <td style="width: 50%">Record No:<span lang="en-us"> </span><asp:Label ID="lblpurches_id" runat="server"></asp:Label></td>
                                    <td style="width: 50%; padding: 0px 5px 0px 0px;"><span lang="en-us">&nbsp;</span>Created Date:<span lang="en-us"> </span><asp:Label ID="lblpurches_date" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td style="width: 50%">Invoice No:<span lang="en-us"> </span><asp:Label ID="lbl_invoiceno" runat="server"></asp:Label></td>
                                    <td style="width: 50%; padding: 0px 5px 0px 0px;"><span lang="en-us">&nbsp;</span>Invoice Date:<span lang="en-us"> </span><asp:Label ID="lbl_invoicedate" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td style="width: 50%">Order No:<span lang="en-us"> </span><asp:Label ID="Label15" runat="server"></asp:Label></td>
                                    <td style="width: 50%; padding: 0px 5px 0px 0px;"><span lang="en-us">&nbsp;</span>Order Date:<span lang="en-us"> </span><asp:Label ID="Label16" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td style="width: 50%">Stock Added On:<span lang="en-us"> </span><asp:Label ID="lbl_stockaddedon" runat="server" Text="No Data"></asp:Label></td>
                                    <td style="width: 50%; padding: 0px 5px 0px 0px;"><span lang="en-us">&nbsp;</span>Shipped To:<span lang="en-us"> </span><asp:Label ID="Label10" runat="server"></asp:Label></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr><td colspan="2">&nbsp;</td></tr>
                    <tr>
                        <td style="width: 50%; padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;"><span lang="en-us" class="style11">From</span><span lang="en-us">,</span>&nbsp;</td>
                        <td style="width: 50%; padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;"><span lang="en-us" class="style11">To</span><span lang="en-us">,</span>&nbsp;</td>
                    </tr>
                    <tr>
                        <td style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;"><asp:Label ID="lblcompanyName" runat="server" CssClass="style11"></asp:Label></td>
                        <td style="padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;"><asp:Label ID="lblcompanyNameTo" runat="server" CssClass="style11"></asp:Label></td>
                    </tr>
                    <tr>
                        <td style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;"><asp:Label ID="lbladdress1" runat="server" CssClass="style11"></asp:Label><br /><asp:Label ID="lbladdress2" runat="server" CssClass="style11"></asp:Label></td>
                        <td style="padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;"><asp:Label ID="lbladdress1To" runat="server" CssClass="style11"></asp:Label><br /><asp:Label ID="lbladdress2To" runat="server" CssClass="style11"></asp:Label></td>
                    </tr>
                    <tr>
                        <td style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;"><asp:Label ID="lblcity" runat="server" CssClass="style11"></asp:Label> - <asp:Label ID="lblPin" runat="server" CssClass="style11"></asp:Label></td>
                        <td style="padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;"><asp:Label ID="lblcityTo" runat="server" CssClass="style11"></asp:Label> - <asp:Label ID="lblPinTo" runat="server" CssClass="style11"></asp:Label></td>
                    </tr>
                    <tr>
                        <td style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;"><asp:Label ID="lblstate" runat="server" CssClass="style11"></asp:Label></td>
                        <td style="padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;"><asp:Label ID="lblstateTo" runat="server" CssClass="style11"></asp:Label></td>
                    </tr>
                    <tr><td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">&nbsp;</td></tr>

                    <tr>
                        <td colspan="2" style="border: 1px solid #bfbfbf;">
                            <asp:DataList ID="DataList1" runat="server" Width="100%" OnItemDataBound="DataList1_ItemDataBound">
                                <HeaderTemplate>
                                    <table width="100%" border="0" cellpadding="5" cellspacing="0" class="table1" style="background-color: #e8f0fe; border-bottom: 2px solid #ccc;">
                                        <tr style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 12px; color: #333; text-transform: uppercase; letter-spacing: 0.5px;">
                                            <th style="width: 3%; text-align: center; border-right: 1px solid #ccc;">Sl</th>
                                            <th style="width: 8%; text-align: center; border-right: 1px solid #ccc;">Product ID</th>
                                            <th style="width: 28%; text-align: center; border-right: 1px solid #ccc;">Particulars</th>
                                            <th style="width: 5%; text-align: center; border-right: 1px solid #ccc;">Qnty</th>
                                            <th style="width: 6%; text-align: center; border-right: 1px solid #ccc;">Rate / Unit</th>
                                            <th style="width: 7%; text-align: center; border-right: 1px solid #ccc;">Total</th>
                                            <th style="width: 5%; text-align: center; border-right: 1px solid #ccc;">Disc %</th>
                                            <th style="width: 8%; text-align: center; border-right: 1px solid #ccc;">Disc Amt</th>
                                            <th style="width: 10%; text-align: center; border-right: 1px solid #ccc;">Taxable Amt</th>
                                            <th style="width: 5%; text-align: center; border-right: 1px solid #ccc;">TAX %</th>
                                            <th style="width: 7.5%; text-align: center; border-right: 1px solid #ccc;">TAX Amt</th>
                                            <th style="width: 7.5%; text-align: center;">Net Total</th>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table width="100%" border="0" cellpadding="5" cellspacing="0" class="table1" style="font-family: Arial; font-size: 13px; color: #333;">
                                        <tr style="border-bottom: 1px solid #ccc;">
                                            <td style="width: 3%; text-align: center;"><%# Eval("sl_no") %></td>
                                            <td style="width: 8%; text-align: center;"><%# Eval("Product_id") %></td>
                                            <td style="width: 28%; text-align: left; padding-left: 5px; white-space: normal;"><%# Eval("Product_name") %></td>
                                            <td style="width: 5%; text-align: center;"><asp:Label ID="Label5" runat="server" Text='<%# Eval("Quantity") %>' /></td>
                                            <td style="width: 6%; text-align: right;"><%# Eval("vendor_rate") %></td>
                                            <td style="width: 7%; text-align: right;"><%# Eval("purches_rate") %></td>
                                            <td style="width: 5%; text-align: center;"><%# Eval("DiscountPercent") %></td>
                                            <td style="width: 8%; text-align: right;"><%# Eval("DiscountAmount") %></td>
                                            <td style="width: 10%; text-align: right;"><asp:Label ID="Label10" runat="server" Text='<%# Eval("TaxableAmount") %>' /></td>
                                            <td style="width: 5%; text-align: center;"><%# Eval("tax_rate") %></td>
                                            <td style="width: 7.5%; text-align: right;"><asp:Label ID="Label7" runat="server" Text='<%# Eval("vat_amount") %>' /></td>
                                            <td style="width: 7.5%; text-align: right; font-weight: bold;"><asp:Label ID="Label14" runat="server" Text='<%# Eval("total_purches_rate") %>' /></td>
                                        </tr>
                                    </table>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                        <tr style="font-weight: bold; background-color: #f1f1f1;">
                                            <td style="width: 3%;"></td><td style="width: 8%;"></td>
                                            <td style="width: 28%; text-align: left;">Total</td>
                                            <td style="width: 5%; text-align: center;"><asp:Label ID="lblTotalQuantity" runat="server" Font-Size="Small" /></td>
                                            <td style="width: 6%;"></td><td style="width: 7%;"></td><td style="width: 5%;"></td><td style="width: 8%;"></td>
                                            <td style="width: 10%; text-align: right;"><asp:Label ID="lblTotalTaxableAmount" runat="server" Font-Size="Small" /></td>
                                            <td style="width: 5%;"></td>
                                            <td style="width: 7.5%; text-align: right;"><asp:Label ID="lblTotalTaxAmount" runat="server" Font-Size="Small" /></td>
                                            <td style="width: 7.5%; text-align: right;"><asp:Label ID="lblGrandTotal" runat="server" Font-Size="Small" /></td>
                                        </tr>
                                    </table>
                                </FooterTemplate>
                            </asp:DataList>
                        </td>
                    </tr>

                    <tr><td colspan="2"><table cellpadding="0" cellspacing="0" width="100%">
                        <tr><td style="padding: 2px;"></td></tr>
                        <tr>
                            <td rowspan="6" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                <b><span class="style17">Amount (In Words):</span><br />
                                <asp:Label ID="lbl_ttl1word" runat="server" Font-Bold="False" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label></b>
                            </td>
                            <td colspan="3" class="auto-style6" style="text-align: right; color: blue; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;">Freight Charges&nbsp;@&nbsp;<asp:Label ID="lbl_frtrate" runat="server" Text=""></asp:Label>&nbsp;%&nbsp;</td>
                            <td class="auto-style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"><asp:Label ID="lblFreightCharges" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="3" class="style17" style="text-align: right; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;"><asp:Label ID="lblOtherCharges1name" runat="server" Text="Label" ForeColor="Blue"></asp:Label></td>
                            <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"><asp:Label ID="lblOtherCharges1" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="3" class="style17" style="text-align: right; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;">Taxable Value</td>
                            <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"><asp:Label ID="lblTaxableValue" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0;"><span class="style18">GST Amount</span></td>
                            <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                <asp:Label ID="lblstax0" runat="server" Visible="true" Text="0.00"></asp:Label>&nbsp;+
                                <asp:Label ID="lblfttax" runat="server" Visible="true" Text="0.00"></asp:Label>&nbsp;+
                                <asp:Label ID="lbl_othr1_tax" runat="server" Visible="true" Text="0.00"></asp:Label>&nbsp;=
                                <asp:Label ID="lbl_ttltax" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3" class="style17" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Total Purchase</td>
                            <td class="style7" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"><asp:Label ID="lblnetamount" runat="server"></asp:Label></td>
                        </tr>
                        <tr><td colspan="5" style="padding: 2px;"></td></tr>
                        
                        <tr>
                            <td rowspan="4" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                <b><span class="style17">Amount (In Words):</span><br />
                                <asp:Label ID="lbl_ttl2word" runat="server" Font-Bold="False" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label></b>
                            </td>
                            <td colspan="3" class="style17" style="text-align: right; color: blue; padding: 5px 20px 5px 0;">TCS Amount&nbsp;@&nbsp;<asp:Label ID="lbl_tcsrate" runat="server" Text=""></asp:Label>&nbsp;%&nbsp;</td>
                            <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"><asp:Label ID="lblTCSAmount" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="3" class="style17" style="text-align: right; color: blue; padding: 5px 20px 5px 0;"><asp:Label ID="lblOtherCharges2name" runat="server" Text="Label" ForeColor="Blue"></asp:Label></td>
                            <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"><asp:Label ID="lblOtherCharges2" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="3" class="style17" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Total 1</td>
                            <td class="style7" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"><asp:Label ID="lbl_ttl2amnt" runat="server"></asp:Label></td>
                        </tr>
                        <tr><td colspan="5" style="padding: 2px;"></td></tr>
                        
                        <tr>
                            <td rowspan="2" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                <b><span class="style17">Amount (In Words):</span><br />
                                <asp:Label ID="lblGrandTotalWord" runat="server" Font-Bold="False" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label></b>
                            </td>
                            <td colspan="3" class="auto-style4" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Grand Total</td>
                            <td class="auto-style5" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"><asp:Label ID="lblGrandTotalMain" runat="server"></asp:Label></td>
                        </tr>
                        <tr><td colspan="5" style="padding: 2px;"></td></tr>
                    </table></td></tr>

                    <tr><td colspan="2" class="auto-style3"></td></tr>
                    <tr><td colspan="2">&nbsp;<strong><span class="auto-style2">Narration:-</span><asp:Label ID="lbl_narration" runat="server"></asp:Label></strong></td></tr>
                    <tr><td colspan="2" class="style12">&nbsp;</td></tr>
                    <tr><td colspan="2" class="style12">&nbsp;</td></tr>
                    <tr><td colspan="2" class="style12">&nbsp;</td></tr>
                    <tr>
                        <td colspan="2" style="text-align: right; padding: 4px 30px 4px 2px;">
                            <span lang="en-us">&nbsp;<asp:Image ID="Image4" runat="server" Width="150PX" ImageUrl="~/corporate/business/WebImages/flmx_authsign.png" /></span>&nbsp;
                        </td>
                    </tr>
                    <tr><td colspan="2" class="style12" style="text-align: right; padding: 4px 7px 4px 2px;">Authorized Signatory</td></tr>
                    <tr>
                        <td id="footerrprint" runat="server" colspan="2">
                            <asp:Image ID="Image22" runat="server" Width="844px" Height="180px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrbtm.png" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Button ID="Button1" runat="server" class="hide" OnClick="Button1_Click" OnClientClick="document.getElementById('Hederprint').className ='header' ;document.getElementById('footerrprint').className ='Foter'; window.print()" Text="Print Without Header" BackColor="#005886" BorderStyle="Outset" ForeColor="White" />
                            <span lang="en-us">&nbsp;<asp:Button ID="Button2" runat="server" class="hide" OnClick="Button2_Click" OnClientClick="document.getElementById('Hederprint').className ='header1' ;document.getElementById('footerrprint').className ='Foter1'; window.print()" Text="Print With Header" BackColor="#005886" BorderStyle="Outset" ForeColor="White" /></span>
                        </td>
                    </tr>
                </table>
            </div>
        </asp:Panel>
    </form>
</body>
</html>