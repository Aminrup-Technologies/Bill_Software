<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Print_PO.aspx.cs" EnableViewState="false" Inherits="Bill_Software.corporate.business.print.Print_PO" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Print PO</title>
    <style type="text/css">
        /* ================= PAGE SETUP ================= */
        @page {
            size: A4 portrait;
            margin: 15mm;
        }

        /* ================= BASE ================= */
        body {
            font-family: Arial, Helvetica, sans-serif;
            font-size: 11px;
            color: #000;
            margin: 0;
            padding: 0;
        }

        /* ================= CONTAINER ================= */
        .po-container {
            width: 100%;
        }

        /* ================= HEADER ================= */
        .po-header {
            text-align: center;
            border-bottom: 1px solid #000;
            padding: 6px 0;
        }

            .po-header h2 {
                margin: 0;
                font-size: 18px;
                letter-spacing: 1px;
            }

        /* ================= META ================= */
        .po-meta {
            width: 100%;
            margin-top: 8px;
            border-collapse: collapse;
        }

            .po-meta td {
                padding: 5px;
                vertical-align: top;
            }

        /* ================= PARTY BOXES ================= */
        .po-box {
            border: 1px solid #000;
            padding: 6px;
            min-height: 100px;
            page-break-inside: avoid;
        }

        .po-section-title {
            font-weight: bold;
            font-size: 11px;
            border-bottom: 1px solid #000;
            margin-bottom: 4px;
            padding-bottom: 2px;
        }

        /* ================= ITEM TABLE ================= */
        .po-table {
            width: 100%;
            margin-top: 10px;
            border-collapse: collapse;
            border: 1px solid #000;
            page-break-inside: auto;
        }

            .po-table th,
            .po-table td {
                border: 1px solid #000;
                padding: 5px;
                font-size: 11px;
            }

            .po-table th {
                background-color: #f2f2f2;
                text-align: center;
                font-weight: bold;
            }

            /* Numeric alignment */
            .po-table td {
                text-align: center;
            }

                .po-table td:nth-child(2) {
                    text-align: left;
                }

            /* Footer row */
            .po-table tfoot td {
                font-weight: bold;
                border-top: 2px solid #000;
                background-color: #fafafa;
            }

        /* ================= FOOTER ================= */
        .po-footer {
            margin-top: 25px;
            page-break-inside: avoid;
        }

        /* ================= UTIL ================= */
        .text-right {
            text-align: right;
        }

        .text-left {
            text-align: left;
        }

        /* ================= PRINT RULES ================= */
        @media print {
            .no-print {
                display: none !important;
            }

            table, tr, td, th {
                page-break-inside: avoid;
            }
        }
    </style>
</head>
<body>
    <form runat="server">
        <div class="po-container">
            <!-- ================= LETTERHEAD ================= -->
            <table width="100%" style="border-bottom: 1px solid #000;">
                <tr>
                    <td width="20%" valign="top">
                        <img src="/assets/logo/flamex_logo.png" runat="server" visible="false"
                            style="max-height: 70px;" />
                    </td>

                    <td width="60%" align="center">
                        <div style="font-size: 16px; font-weight: bold;">
                            FLAME-EX
                        </div>
                        <div style="font-size: 11px;">
                            Bagbera Colony, Block No-35/2/4, Road No.2<br />
                            Jamshedpur – 831012, Jharkhand<br />
                            GSTIN: 20ABCDE1234F1Z5
                        </div>
                    </td>

                    <td width="20%"></td>
                </tr>
            </table>

            <!-- ================= HEADER ================= -->
            <div class="po-header">
                <h2>PURCHASE ORDER</h2>
                <asp:Label ID="lblCompanyName" runat="server" />
            </div>

            <!-- ================= PO META ================= -->
            <table class="po-meta">
                <tr>
                    <td width="50%">
                        <b>PO No:</b>
                        <asp:Label ID="lblPONo" runat="server" /><br />
                        <b>PO Date:</b>
                        <asp:Label ID="lblPODate" runat="server" /><br />
                        <b>Req No:</b>
                        <asp:Label ID="lblReqNo" runat="server" />
                    </td>
                    <td width="50%">
                        <b>Engineer:</b>
                        <asp:Label ID="lblEngineer" runat="server" /><br />
                        <b>Dispatch Mode:</b>
                        <asp:Label ID="lblDispatchMode" runat="server" /><br />
                        <b>Delivery Basis:</b>
                        <asp:Label ID="lblDeliveryBasis" runat="server" />
                    </td>
                </tr>
            </table>

            <!-- ================= PARTY DETAILS ================= -->

            <!-- VENDOR (FULL WIDTH) -->
            <table class="po-meta">
                <tr>
                    <td>
                        <div class="po-box">
                            <div class="po-section-title">Vendor</div>
                            <asp:Literal ID="litVendor" runat="server" />
                        </div>
                    </td>
                </tr>
            </table>

            <!-- BILL TO + SHIP TO (2 COLUMNS) -->
            <table class="po-meta">
                <tr>
                    <td width="50%">
                        <div class="po-box">
                            <div class="po-section-title">Bill To</div>
                            <asp:Literal ID="litBillTo" runat="server" />
                        </div>
                    </td>

                    <td width="50%">
                        <div class="po-box">
                            <div class="po-section-title">Ship To</div>
                            <asp:Literal ID="litShipTo" runat="server" />
                        </div>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:GridView ID="gvItems" runat="server"
                            CssClass="po-table" OnRowDataBound="gvItems_RowDataBound1"
                            AutoGenerateColumns="False"
                            ShowFooter="true">
                            <Columns>

                                <asp:BoundField HeaderText="Sl" DataField="ItemOrder" />

                                <asp:BoundField HeaderText="Item Description"
                                    DataField="ProductName"
                                    ItemStyle-HorizontalAlign="Left" />

                                <asp:BoundField HeaderText="HSN"
                                    DataField="HSNCode"
                                    ItemStyle-HorizontalAlign="Center" />


                                <asp:BoundField HeaderText="Qty" DataField="Quantity"
                                    DataFormatString="{0:N2}" />

                                <asp:BoundField HeaderText="Rate" DataField="Rate"
                                    DataFormatString="{0:N2}" />

                                <asp:BoundField HeaderText="Disc Amt" DataField="DiscountAmount"
                                    DataFormatString="{0:N2}" />

                                <asp:BoundField HeaderText="Taxable" DataField="TaxableAmount"
                                    DataFormatString="{0:N2}" />

                                <asp:BoundField HeaderText="GST" DataField="TaxAmount"
                                    DataFormatString="{0:N2}" />

                                <asp:BoundField HeaderText="Net Amt" DataField="NetAmount"
                                    DataFormatString="{0:N2}" />
                            </Columns>
                        </asp:GridView>
                        <!-- ================= GST SPLIT SUMMARY ================= -->
                        <!-- ================= GST SUMMARY ROW ================= -->
                        <table class="po-meta" style="margin-top: 12px; width: 100%;">
                            <tr>
                                <!-- LEFT EMPTY SPACE (anchors layout) -->
                                <td width="60%"></td>

                                <!-- RIGHT GST SUMMARY -->
                                <td width="40%" valign="top">
                                    <table width="100%" class="po-table">
                                        <tr>
                                            <th colspan="2">GST Summary</th>
                                        </tr>

                                        <asp:PlaceHolder ID="phCGSTSGST" runat="server" Visible="false">
                                            <tr>
                                                <td class="text-left">CGST</td>
                                                <td class="text-right">
                                                    <asp:Label ID="lblCGST" runat="server" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="text-left">SGST</td>
                                                <td class="text-right">
                                                    <asp:Label ID="lblSGST" runat="server" />
                                                </td>
                                            </tr>
                                        </asp:PlaceHolder>

                                        <asp:PlaceHolder ID="phIGST" runat="server" Visible="false">
                                            <tr>
                                                <td class="text-left">IGST</td>
                                                <td class="text-right">
                                                    <asp:Label ID="lblIGST" runat="server" />
                                                </td>
                                            </tr>
                                        </asp:PlaceHolder>

                                        <tr>
                                            <td class="text-left"><b>Total GST</b></td>
                                            <td class="text-right">
                                                <b>
                                                    <asp:Label ID="lblTotalGST" runat="server" /></b>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>

                        <div style="height: 12px;"></div>


                        <!-- ================= COMMERCIAL / LOGISTICS TERMS (4 COLUMN) ================= -->
                        <table class="po-meta" style="margin-top: 12px; border: 1px solid #000; width: 100%; border-collapse: collapse;">
                            <tr>
                                <td width="20%"><b>Rates are as per :</b></td>
                                <td width="30%">
                                    <asp:Label ID="lblRateRef" runat="server" /></td>

                                <td width="20%"><b>Special Rates Approved By :</b></td>
                                <td width="30%">
                                    <asp:Label ID="lblSpecialRateApprovedBy" runat="server" /></td>
                            </tr>

                            <tr>
                                <td><b>Freight Charges :</b></td>
                                <td>
                                    <asp:Label ID="lblFreightTerms" runat="server" /></td>

                                <td><b>Mode of Despatch :</b></td>
                                <td>
                                    <asp:Label ID="lblDispatchModeText" runat="server" /></td>
                            </tr>

                            <tr>
                                <td><b>Despatch Upto :</b></td>
                                <td>
                                    <asp:Label ID="lblDispatchUptoText" runat="server" /></td>

                                <td><b>Delivery Basis :</b></td>
                                <td>
                                    <asp:Label ID="lblDeliveryBasisText" runat="server" /></td>
                            </tr>

                            <tr>
                                <td><b>Bill Sent To :</b></td>
                                <td>
                                    <asp:Label ID="lblBillSentTo" runat="server" /></td>

                                <td><b>LR Sent To :</b></td>
                                <td>
                                    <asp:Label ID="lblLRSentTo" runat="server" /></td>
                            </tr>
                        </table>



                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <div style="margin-top: 10px; font-size: 11px;">
                            <b>Amount in Words:</b>
                            <asp:Label ID="lblAmountInWords" runat="server" />
                        </div>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <div class="po-footer">
                            <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td width="100%" valign="top" colspan="2">
                                        <b>Remarks:</b><br />
                                        <asp:Label ID="lblRemarks" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td width="50%">
                                        <b>Prepared By:</b>
                                        <asp:Label ID="lblPreparedBy" runat="server" />
                                    </td>
                                    <td width="50%" class="text-right">
                                        <b>Authorised Signatory</b><br />
                                        <br />
                                        ______________________
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div style="text-align: center; font-size: 10px; margin-top: 10px;">
            This is a computer generated Purchase Order and does not require a physical signature.
        </div>

        <div class="no-print" style="margin-top: 10px; text-align: right;">
            <button onclick="window.print();return false;">Print</button>
        </div>
    </form>

</body>
</html>
