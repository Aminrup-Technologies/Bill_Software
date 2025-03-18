<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="purches_bill.aspx.cs" Inherits="Bill_Software.corporate.business.print.purches_bill" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/i2i_logo.png" />
    <style type="text/css">
        .style1 {
            width: 100%;
        }


        * {
            padding: 0;
            margin: 0;
            border: none;
            list-style: none;
            text-decoration: none;
        }


        * {
            border-style: none;
            border-color: inherit;
            border-width: medium;
            margin: 0px;
            padding: 0px;
            list-style: none;
            text-decoration: none;
        }

        .style5 {
            text-align: right;
            padding: 0px 5px 0px 0px;
        }

        .style6 {
            font-family: Arial, Helvetica, sans-serif;
            font-weight: bold;
            font-size: medium;
            color: #000037;
        }

        .tableOne {
            margin: 0;
            border: solid 1px #bfbfbf;
        }

            .tableOne td {
                font: normal 14px/20px Calibri;
                background: #dbe5f1;
                border: solid 1px #bfbfbf;
                padding: 2px 0 2px 5px;
            }

        .tableTwo {
            margin: 0;
            border: solid 1px #bfbfbf;
        }

            .tableTwo td {
                font: normal 12px/16px Calibri;
                background: #dbe5f1;
                border: solid 1px #bfbfbf;
                padding: 2px 5px 2px 5px;
            }

        .table_border td {
            border: 2px solid #bfbfbf;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #bfbfbf;
                width: 50%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #bfbfbf;
                padding: 2px 0 2px 20px;
            }

        .style7 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
            text-align: center;
        }

        .style8 {
            text-align: center;
            color: #FFFFFF;
            font-weight: bold;
            font-family: Calibri;
        }

        .style9 {
            font-family: Arial, Helvetica, sans-serif;
            font-weight: bold;
            font-size: 20px;
            color: #e36c0a;
        }

        .style10 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
        }

        .style11 {
            font-weight: bold;
        }

        .style12 {
            font-family: "Century Gothic";
            font-size: medium;
            font-weight: bold;
        }

        .style13 {
            color: #1c3564;
        }

        .style14 {
            color: #1c3564;
        }

        .style15 {
            color: #e36c0a;
        }

        .style16 {
            font-family: "Century Gothic";
            font-size: medium;
            font-weight: bold;
            color: #1c3564;
        }

        .style17 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
        }

        .style18 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
        }

        .sssss {
            position: fixed;
            bottom: 0;
        }

        @media print {
            thead {
                display: table-header-group;
            }

            tfoot {
                display: table-footer-group;
            }

            .header, .hide {
                visibility: hidden;
                height: 100px;
            }

            .header1, .show {
            }

            .Foter, .hide {
                visibility: hidden;
                height: 60px;
            }

            .Foter1, .show {
                position: fixed;
                bottom: 0;
            }
        }

        @media screen {
            thead {
                display: block;
            }

            tfoot {
                display: block;
            }
        }

        @media print1 {
            #non-printable {
                display: none;
            }

            #printable {
                display: block;
            }
        }

        #footerrprint img {
            max-width: 100%;
            height: auto;
        }

        .auto-style2 {
            font-size: medium;
        }

        .auto-style3 {
            height: 17px;
        }
        .auto-style4 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
            height: 26px;
        }
        .auto-style5 {
            font-family: Arial;
            font-size: small;
            font-weight: bold;
            text-align: center;
            height: 26px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="printable" style="width: 844px;">

            <table cellpadding="0" cellspacing="0" class="style1">

                <tr>
                    <td id="Hederprint" runat="server" colspan="2">
                        <asp:Image ID="Image21" runat="server"
                            Width="844px" Height="140px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrtop.png" /></td>
                </tr>


                <tr>
                    <td class="style5" colspan="2">&nbsp;</td>
                </tr>

                <tr>
                    <td class="style5" colspan="2">
                        <span lang="en-us"><span class="style6">&nbsp;</span><span class="style9">PURCHASE VOUCHER</span></span></td>
                </tr>

                <tr>
                    <td colspan="2">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2">
                        <table width="100%" cellpadding="0" cellspacing="0" class="tableOne">
                            <tr>
                                <td style="width: 50%">Purchasse No:<span lang="en-us"> </span>
                                    <asp:Label ID="lblpurches_id" runat="server"></asp:Label>
                                </td>
                                <td style="text-align: right; width: 50%; padding: 0px 5px 0px 0px;">
                                    <span lang="en-us">&nbsp;</span>Created Date:<span lang="en-us"> </span>
                                    <asp:Label ID="lblpurches_date" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 50%">Invoice No:<span lang="en-us"> </span>
                                    <asp:Label ID="lbl_invoiceno" runat="server"></asp:Label>
                                </td>
                                <td style="text-align: right; width: 50%; padding: 0px 5px 0px 0px;">
                                    <span lang="en-us">&nbsp;</span>Invoice Date:<span lang="en-us"> </span>
                                    <asp:Label ID="lbl_invoicedate" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 50%">Stock Added On:<span lang="en-us"> </span>
                                    <asp:Label ID="lbl_stockaddedon" runat="server" Text="No Data"></asp:Label>
                                </td>
                                <td style="text-align: right; width: 50%; padding: 0px 5px 0px 0px;">
                                    <%--<span lang="en-us">&nbsp;</span>Invoice Date:<span lang="en-us"> </span>
                                    <asp:Label ID="Label10" runat="server"></asp:Label>--%>
                                </td>
                            </tr>
                            <%--<tr>
                            <td style="width:50%">
                                Service Tax Registration No:
                                <asp:Label ID="lblservicetax_re" runat="server"></asp:Label>
                            </td>
                            <td style="text-align:right; width:50%; padding:0px 5px 0px 0px;">
                                Pan No:
                                <asp:Label ID="lblpan" runat="server"></asp:Label>
                            </td>
                        </tr>--%>
                        </table>
                    </td>
                </tr>

                <tr>
                    <td colspan="2">&nbsp;</td>
                </tr>

                <tr>
                    <td style="width: 50%; padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">
                        <span lang="en-us" class="style11">To</span><span lang="en-us">,</span>&nbsp;</td>
                    <td style="text-align: right; width: 50%; padding: 0px 5px 0px 0px; font: normal 14px/16px Calibri;">&nbsp;</td>
                </tr>

                <%-- <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblrepresentativeName" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblrepresentativedesignation" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>--%>

                <tr>
                    <td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">
                        <asp:Label ID="lblcompanyName" runat="server" CssClass="style11"></asp:Label>
                    </td>
                </tr>

                <tr>
                    <td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">
                        <asp:Label ID="lbladdress1" runat="server" CssClass="style11"></asp:Label>
                        <br />
                        <asp:Label ID="lbladdress2" runat="server" CssClass="style11"></asp:Label>
                    </td>
                </tr>

                <tr>
                    <td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">
                        <asp:Label ID="lblcity" runat="server" CssClass="style11"></asp:Label>
                        -<asp:Label ID="lblPin" runat="server" CssClass="style11"></asp:Label>
                    </td>
                </tr>


                <tr>
                    <td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">
                        <asp:Label ID="lblstate" runat="server" CssClass="style11"></asp:Label>
                    </td>
                </tr>


                <tr>
                    <td colspan="2" style="padding: 0px 0px 0px 5px; font: normal 14px/16px Calibri;">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2" style="border: 1px solid #bfbfbf;">
                        <asp:DataList ID="DataList1" runat="server" Width="100%" OnItemDataBound="DataList1_ItemDataBound">
                            <HeaderTemplate>
                                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                    <tr>
                                        <td style="width: 5%; text-align: center;">
                                            <asp:Label runat="server" ID="tupe_ofcirtificate" Text="S.No." Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 10%; text-align: center;">
                                            <asp:Label runat="server" ID="Label11" Text="Product ID" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 30%; text-align: center;">
                                            <asp:Label runat="server" ID="no_of_sur" Text="Particulars" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 8%; text-align: center;">
                                            <asp:Label runat="server" ID="Label1" Text="Qnty" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 7%; text-align: center;">
                                            <asp:Label runat="server" ID="Label4" Text="Rate / Unit" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>

                                        <td style="width: 12%; text-align: center;">
                                            <asp:Label runat="server" ID="Label3" Text="Taxable Amount" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 5%; text-align: center;">
                                            <asp:Label runat="server" ID="Label8" Text="TAX %" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 12%; text-align: center;">
                                            <asp:Label runat="server" ID="Label6" Text="TAX Amount" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                        <td style="width: 21%; text-align: center;">
                                            <asp:Label runat="server" ID="Label13" Text="Total Amount" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                    </tr>
                                </table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                    <tr>
                                        <td style="width: 5%; border-top: none; text-align: center;">
                                            <asp:Label ID="qtation_survice" runat="server" Text='<%# Eval("sl_no") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 10%; border-top: none; text-align: left; padding: 0px 2px 0px 5px;">
                                            <asp:Label ID="survice_month" runat="server" Text='<%# Eval("Product_id") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 30%; border-top: none; text-align: left; padding: 0px 2px 0px 5px;">
                                            <asp:Label ID="Label12" runat="server" Text='<%# Eval("Product_name") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 8%; border-top: none; text-align: center;">
                                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("Quantity") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 7%; border-top: none; text-align: center;">
                                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("vendor_rate") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>

                                        <td style="width: 12%; border-top: none; text-align: right; padding-right: 5px;">
                                            <asp:Label ID="Label10" runat="server" Text='<%# Eval("purches_rate") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 5%; border-top: none; text-align: center;">
                                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("tax_rate") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 12%; border-top: none; text-align: right; padding-right: 5px;">
                                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("vat_amount") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                        <td style="width: 21%; border-top: none; text-align: right; padding-right: 5px; font-weight: bold;">
                                            <asp:Label ID="Label14" runat="server" Text='<%# Eval("total_purches_rate") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                        </td>
                                    </tr>

                                </table>
                            </ItemTemplate>

                            <FooterTemplate>
                                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                    <tr style="font-weight: bold; background-color: #f1f1f1;">
                                        <td style="width: 5%; text-align: center;"></td>
                                        <td style="width: 10%; text-align: center;"></td>
                                        <td style="width: 30%; text-align: center;">Total</td>
                                        <td style="width: 8%; text-align: center;">
                                            <asp:Label ID="lblTotalQuantity" runat="server"></asp:Label>
                                        </td>
                                        <td style="width: 7%; text-align: center;"></td>
                                        <td style="width: 12%; text-align: right; padding-right: 5px;">
                                            <asp:Label ID="lblTotalTaxableAmount" runat="server"></asp:Label>
                                        </td>
                                        <td style="width: 5%; text-align: center;"></td>
                                        <td style="width: 12%; text-align: right; padding-right: 5px;">
                                            <asp:Label ID="lblTotalTaxAmount" runat="server"></asp:Label>
                                        </td>
                                        <td style="width: 21%; text-align: right; padding-right: 5px;">
                                            <asp:Label ID="lblGrandTotal" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </FooterTemplate>

                        </asp:DataList>
                    </td>
                </tr>

                <%--<tr>
                    <td colspan="2">
                        <table cellpadding="0" cellspacing="0" width="100%">
                            <tr>
                                <td rowspan="3" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span>
                                        <span lang="en-us"></span>
                                        <asp:Label ID="lblword" runat="server" Font-Bold="False" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>

                                <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0;">
                                    <span class="style10">
                                        <span class="style18">GST Amount</span>
                                    </span>
                                </td>

                                <td style="border-right:1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align: right; padding: 0px 20px 0px 2px;" class="style7">
                                    <span class="style10"><span class="style18">
                                        <asp:Label ID="lblstax0" runat="server"></asp:Label></span></span>
                                </td>
                            </tr>

                            <tr>
                                <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0;" class="style17">Total Purchase</td>
                                <td colspan="3" style="border-right: 1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align: right; padding: 0px 20px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblnetamount" runat="server" CssClass="style17"></asp:Label>
                                </td>
                            </tr>
                        </table>

                        <table cellpadding="0" cellspacing="0" width="100%" border="1">
                            <tr>
                                <td style="text-align: right; padding: 5px 20px 5px 0;" class="style17"><b>Total Quantity:</b></td>
                                <td style="text-align: right; padding: 5px 20px 5px 0;">
                                    <asp:Label ID="lblTotalQnty" runat="server" Font-Bold="true" CssClass="style17"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding: 5px 20px 5px 0;" class="style17"><b>Total Taxable Amount:</b></td>
                                <td style="text-align: right; padding: 5px 20px 5px 0;">
                                    <asp:Label ID="lblTotalTaxable" runat="server" Font-Bold="true" CssClass="style17"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding: 5px 20px 5px 0;" class="style17"><b>Total Tax Amount:</b></td>
                                <td style="text-align: right; padding: 5px 20px 5px 0;">
                                    <asp:Label ID="lblTotalTaxAmount" runat="server" Font-Bold="true" CssClass="style17"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: right; padding: 5px 20px 5px 0; background-color: #dbe5f1;" class="style17">
                                    <b>Grand Total:</b>
                                </td>
                                <td style="text-align: right; padding: 5px 20px 5px 0; background-color: #dbe5f1;">
                                    <asp:Label ID="lblGrandTotal" runat="server" Font-Bold="true" CssClass="style17"></asp:Label>
                                </td>
                            </tr>
                        </table>

                    </td>
                </tr>--%>

                <%--<tr>
                    <td colspan="2">
                        <table cellpadding="0" cellspacing="0" width="100%">

                            <tr>
                                <td style="padding: 2px;"></td>
                            </tr>

                            <tr>
                                <td rowspan="5" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span>
                                        </br>
                                        <asp:Label ID="lbl_ttl1word" runat="server" Font-Bold="False" CssClass="style22"
                                            Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>

                                <!-- Freight Charges Row -->
                            <tr>
                                <td colspan="3" style="text-align: right; color: blue; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;" class="style17">Freight Charges
                                </td>
                                <td style="border-right: 1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; border-top: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblFreightCharges" runat="server"></asp:Label>
                                </td>
                            </tr>

                                <!-- Taxable Value Row -->
                                <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;" class="style17">Taxable Value
                                </td>
                                <td style="border-right: 1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblTaxableValue" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- GST Amount Row -->
                            <tr>
                                <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0;">
                                    <span class="style10">
                                        <span class="style18">GST Amount</span>
                                    </span>
                                </td>
                                <td style="border-right: 1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblstax0" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Total Purchase Row -->

                            <tr>
                                <td colspan="3" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;" class="style17">Total Purchase</td>
                                <td style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"
                                    class="style7">
                                    <asp:Label ID="lblnetamount" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Blank Row for Spacing -->
                            <tr>
                                <td colspan="5" style="padding: 2px;"></td>
                            </tr>

                            <td rowspan="4" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span>
                                        <asp:Label ID="lbl_ttl2word" runat="server" Font-Bold="False" CssClass="style22"
                                            Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>

                            <!-- TCS Amount Row -->
                            <tr>
                                <td colspan="3" style="text-align: right; color: blue; padding: 5px 20px 5px 0;" class="style17">TCS Amount
                                </td>
                                <td style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblTCSAmount" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Other Charges Row -->
                            <tr>
                                <td colspan="3" style="text-align: right; color: blue; padding: 5px 20px 5px 0;" class="style17">Other Charges
                                </td>
                                <td style="border-right: 1px solid #bfbfbf; border-left: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;"
                                    class="style7">
                                    <asp:Label ID="lblOtherCharges" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <tr>
                                <td colspan="3" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;" class="style17">Total 1</td>
                                <td style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"
                                    class="style7">
                                    <asp:Label ID="lbl_ttl2amnt" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Blank Row for Spacing -->
                            <tr>
                                <td colspan="4" style="padding: 5px;"></td>
                            </tr>

                            <!-- Grand Total Row -->

                            <tr>
                                <td rowspan="2" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span>
                                        <asp:Label ID="lblGrandTotalWord" runat="server" Font-Bold="False" CssClass="style22"
                                            Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>

                                <td colspan="3" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;" class="style17">Grand Total</td>
                                <td style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;"
                                    class="style7">
                                    <asp:Label ID="lblGrandTotal" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <tr>
                                <td colspan="4"></td>
                            </tr>
                        </table>
                    </td>
                </tr>--%>

                <tr>
                    <td colspan="2">
                        <table cellpadding="0" cellspacing="0" width="100%">

                            <tr>
                                <td style="padding: 2px;"></td>
                            </tr>

                            <!-- Amount in Words -->
                            <tr>
                                <td rowspan="5" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span><br />
                                        <asp:Label ID="lbl_ttl1word" runat="server" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>
                            </tr>

                            <!-- Freight Charges Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; color: blue; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;">Freight Charges&nbsp;@&nbsp;<asp:Label ID="lbl_frtrate" runat="server" Text=""></asp:Label>&nbsp;%&nbsp;
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                    <asp:Label ID="lblFreightCharges" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Taxable Value Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; padding: 5px 20px 5px 0; border-top: 1px solid #bfbfbf;">Taxable Value
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                    <asp:Label ID="lblTaxableValue" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- GST Amount Row -->
                            <tr>
                                <td colspan="3" style="text-align: right; padding: 5px 20px 5px 0;">
                                    <span class="style18">GST Amount</span>
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                    <asp:Label ID="lblstax0" runat="server"></asp:Label> + <asp:Label ID="lblfttax" runat="server"></asp:Label> = <asp:Label ID="lbl_ttltax" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Total Purchase Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Total Purchase
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;">
                                    <asp:Label ID="lblnetamount" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Blank Row for Spacing -->
                            <tr>
                                <td colspan="5" style="padding: 2px;"></td>
                            </tr>

                            <tr>
                                <td rowspan="4" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px; border-top: 1px solid #bfbfbf;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span><br />
                                        <asp:Label ID="lbl_ttl2word" runat="server" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>
                            </tr>

                            <!-- TCS Amount Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; color: blue; padding: 5px 20px 5px 0;">TCS Amount&nbsp;@&nbsp;<asp:Label ID="lbl_tcsrate" runat="server" Text=""></asp:Label>&nbsp;%&nbsp;
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                    <asp:Label ID="lblTCSAmount" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Other Charges Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; color: blue; padding: 5px 20px 5px 0;">
                                    <asp:Label ID="lblOtherCharges1name" runat="server" Text="Label" ForeColor="Blue"></asp:Label>
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; text-align: right; padding: 0px 5px 0px 2px;">
                                    <asp:Label ID="lblOtherCharges" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Total 1 Row -->
                            <tr>
                                <td colspan="3" class="style17" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Total 1
                                </td>
                                <td class="style7" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;">
                                    <asp:Label ID="lbl_ttl2amnt" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Blank Row for Spacing -->
                            <tr>
                                <td colspan="5" style="padding: 2px;"></td>
                            </tr>

                            <!-- Grand Total Row -->
                            <tr>
                                <td rowspan="2" bgcolor="#dbe5f1" style="padding: 0px 0px 0px 5px;">
                                    <b>
                                        <span class="style17">Amount (In Words):</span><br />
                                        <asp:Label ID="lblGrandTotalWord" runat="server" CssClass="style22" Style="font-family: Arial; font-size: small; font-weight: bold"></asp:Label>
                                    </b>
                                </td>

                                <td colspan="3" class="auto-style4" style="text-align: right; background-color: #dbe5f1; font-weight: bold; padding: 5px 20px 5px 0;">Grand Total
                                </td>
                                <td class="auto-style5" style="border: 1px solid #bfbfbf; background-color: #dbe5f1; text-align: right; padding: 0px 5px 0px 2px; font-weight: bold;">
                                    <asp:Label ID="lblGrandTotal" runat="server"></asp:Label>
                                </td>
                            </tr>

                            <!-- Blank Row for Spacing -->
                            <tr>
                                <td colspan="5" style="padding: 2px;"></td>
                            </tr>

                        </table>
                    </td>
                </tr>


                <%--<tr>
                    <td colspan="2">&nbsp; <strong>
                        <asp:Label ID="labeltax1" runat="server"></asp:Label>
                        :-<asp:Label ID="lblsail_rate" runat="server"></asp:Label>
                    </strong>
                    </td>
                </tr>

                <tr>
                    <td colspan="2">&nbsp;
                    <strong><span class="auto-style2">Total Purchasse Rate:-</span><asp:Label ID="lblpurches_rate" runat="server"></asp:Label>
                    </strong>
                    </td>
                </tr>--%>
                <tr>
                    <td colspan="2" class="auto-style3"></td>
                </tr>

                <tr>
                    <td colspan="2">&nbsp;
                    <strong><span class="auto-style2">Narration:-</span><asp:Label ID="lbl_narration" runat="server"></asp:Label>
                    </strong>
                    </td>
                </tr>

                <%--<tr>
                    <td colspan="2" bgcolor="#002060" class="style8">
                    Payment <span lang="en-us">T</span>erms</td>
                    <td colspan="2">&nbsp;</td>
                </tr>--%>

                <%--<tr>
                    <td colspan="2">
                        <table width="100%" cellpadding="0" cellspacing="0" class="tableTwo">
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                Payment for service shall be payable in advance/within 15 days issue of Invoice. 
                                by  
                                <asp:Label ID="lblcompanyshortname1" runat="server"></asp:Label>
                                &amp; is Subject to Kolkata Jurisdiction</td>
                            <td style="width:50%; text-align:justify;">
                                <span lang="en-us"><asp:Label ID="lblcompanyshortname2" runat="server"></asp:Label>
                                do not accept payment made in Cash. Please pay by 
                                Cheque/DD in favour of </span>
                                <asp:Label ID="lblcompantfullname2" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                In case of non payment by the customer within the stipulated period,  
                                <asp:Label ID="lblcompanyshortname3" runat="server"></asp:Label>
                                may 
                                stop the service and suspend the certificate after giving a notice to the 
                                customer for a period of 15 days.</td>
                            <td style="width:50%; text-align:justify;">
                                All outstation cheque will be given credit only after being credited to 
                                <asp:Label ID="lblcompantfullname1" runat="server"></asp:Label>
                                bank account. Bank 
                                charges, if any, shall be borne by the Customer.A </td>
                        </tr>
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                All disputes subject to Kolkata High Court Jurisdiction only.</td>
                            <td style="width:50%; text-align:justify;">
                                The Application Fee for the Certification is Non Refundable.</td>
                        </tr>
                    </table>
                    </td>
                </tr>--%>

                <tr>
                    <td colspan="2">&nbsp;</td>
                </tr>



                <tr>
                    <td colspan="2">&nbsp;</td>
                </tr>



                <tr>
                    <td colspan="2" class="style12">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2" class="style12">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2" class="style12">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2" style="text-align: right; padding: 4px 30px 4px 2px;">
                        <span lang="en-us">&nbsp;<asp:Image ID="Image4" runat="server" Width="150PX" ImageUrl="~/corporate/business/WebImages/flmx_authsign.png" />
                        </span>&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="2" class="style12" style="text-align: right; padding: 4px 7px 4px 2px;">Authorized Signatory</td>
                </tr>
                <tr>
                    <td id="footerrprint" runat="server" colspan="2">

                        <asp:Image ID="Image22" runat="server"
                            Width="844px" Height="180px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrbtm.png" />

                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:Button ID="Button1" runat="server" class="hide" OnClick="Button1_Click"
                            OnClientClick="document.getElementById('Hederprint').className ='header' ;document.getElementById('footerrprint').className ='Foter'; window.print()"
                            Text="Print Without Header" BackColor="#005886" BorderStyle="Outset"
                            ForeColor="White" />
                        <span lang="en-us">&nbsp;<asp:Button ID="Button2" runat="server"
                            class="hide" OnClick="Button2_Click"
                            OnClientClick="document.getElementById('Hederprint').className ='header1' ;document.getElementById('footerrprint').className ='Foter1'; window.print()"
                            Text="Print With Header" BackColor="#005886" BorderStyle="Outset"
                            ForeColor="White" />
                        </span></td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
