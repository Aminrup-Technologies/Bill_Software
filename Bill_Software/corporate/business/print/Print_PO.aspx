<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Print_PO.aspx.cs" EnableViewState="false" Inherits="Bill_Software.corporate.business.print.Print_PO" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Purchase Order</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>

    <style type="text/css">
        /* ================= RESET & FONTS ================= */
        body { font-family: "Segoe UI", Arial, sans-serif; font-size: 11px; margin: 0; padding: 0; color: #000; background: #525659; }
        
        /* ================= A4 PAGE CONTAINER ================= */
        .page-container {
            width: 210mm;
            min-height: 297mm;
            margin: 20px auto;
            background: #fff;
            padding: 12mm; /* Balanced padding */
            box-sizing: border-box;
            box-shadow: 0 0 10px rgba(0,0,0,0.5);
        }

        /* ================= UTILS ================= */
        .full-width { width: 100%; border-collapse: collapse; }
        .text-right { text-align: right; }
        .text-center { text-align: center; }
        .text-left { text-align: left; }
        .bold { font-weight: bold; }
        .mt-10 { margin-top: 10px; }
        .mb-10 { margin-bottom: 10px; }
        .valign-top { vertical-align: top; }

        /* ================= TABLES & BOXES ================= */
        .std-table td, .std-table th { border: 1px solid #000; padding: 4px; vertical-align: top; }
        .grid-header { background-color: #f0f0f0; text-align: center; font-weight: bold; }
        
        .section-box { border: 1px solid #000; padding: 0; margin-bottom: 10px; }
        .section-header {
            background-color: #e0e0e0;
            font-weight: bold;
            padding: 4px 6px;
            border-bottom: 1px solid #000;
            font-size: 12px;
            text-transform: uppercase;
        }
        .box-content { padding: 6px; }

        /* Internal layout for Vendor/Addresses */
        .info-label { width: 100px; font-weight: bold; display: inline-block; }
        
        /* ================= PRINT RULES ================= */
        @media print {
            body { background: #fff; margin: 0; }
            .page-container { width: 100%; margin: 0; padding: 8mm; box-shadow: none; border: none; }
            .no-print { display: none !important; }
            thead { display: table-header-group; }
            tr { page-break-inside: avoid; }
            .avoid-break { page-break-inside: avoid; }
        }
    </style>

    <script>
        function downloadPDF() {
            var element = document.getElementById('print-area');
            var opt = {
                margin:       [8, 8, 8, 8],
                filename:     'PO_<%= lblPONo.Text %>.pdf',
                image:        { type: 'jpeg', quality: 0.98 },
                html2canvas:  { scale: 2, useCORS: true },
                jsPDF:        { unit: 'mm', format: 'a4', orientation: 'portrait' },
                pagebreak:    { mode: ['avoid-all', 'css', 'legacy'] }
            };
            html2pdf().set(opt).from(element).save();
        }
    </script>
</head>
<body>
    <form runat="server">
        <div class="no-print" style="text-align:center; padding:10px; background:#333; position:sticky; top:0; z-index:999;">
            <button type="button" onclick="window.print()" style="padding:6px 20px; font-weight:bold; cursor:pointer;">🖨 Print Page</button>
            <button type="button" onclick="downloadPDF()" style="padding:6px 20px; display:none; font-weight:bold; background:#d9534f; color:#fff; border:none; margin-left:10px; cursor:pointer;">⬇ Download PDF</button>
        </div>

        <div class="page-container" id="print-area">
            
            <table class="full-width" style="border-bottom: 2px solid #000; margin-bottom: 10px; padding-bottom: 5px;">
                <tr>
                    <td width="20%" valign="top"><img src="../WebImages/aagrouplogo.png" style="max-height: 70px;" /></td>
                    <td width="60%" class="text-center">
                        <div style="font-size: 22px; font-weight: bold; letter-spacing: 1px;">FLAME-EX</div>
                        <div style="font-size: 11px; margin-top: 3px;">
                            Bagbera Colony, Block No-35/2/4, Road No.2<br />
                            Jamshedpur – 831002, Jamshedpur, Jharkhand<br />
                            <b>GSTIN: 20AESPD7535D1ZS</b>
                        </div>
                        <div style="margin-top: 6px; font-size: 14px; font-weight: bold; border: 1px solid #000; padding: 3px 15px; display: inline-block; background: #eee;">PURCHASE ORDER</div>
                    </td>
                    <td width="20%"></td>
                </tr>
            </table>

            <table class="full-width std-table mb-10">
                <tr>
                    <td width="15%" class="bold" style="background:#f9f9f9;">Order No.:</td>
                    <td width="35%"><b><asp:Label ID="lblPONo" runat="server" /></b></td>
                    <td width="15%" class="bold" style="background:#f9f9f9;">Order Date:</td>
                    <td width="35%"><asp:Label ID="lblPODate" runat="server" /></td>
                </tr>
                <tr>
                    <td class="bold" style="background:#f9f9f9;">Req No.:</td>
                    <td><asp:Label ID="lblReqNo" runat="server" /></td>
                    <td class="bold" style="background:#f9f9f9;">Engineer Name:</td>
                    <td><asp:Label ID="lblEngineer" runat="server" /></td>
                </tr>
            </table>

            <div class="section-box avoid-break">
                <div class="section-header">Vendor Details</div>
                <div class="box-content">
                    <table class="full-width">
                        <tr>
                            <td width="50%" class="valign-top" style="padding-right: 10px; border-right: 1px dashed #ccc;">
                                <asp:Literal ID="litVendorLeft" runat="server" />
                            </td>
                            <td width="50%" class="valign-top" style="padding-left: 10px;">
                                <asp:Literal ID="litVendorRight" runat="server" />
                            </td>
                        </tr>
                    </table>
                </div>
            </div>

            <table class="full-width mb-10 avoid-break">
                <tr>
                    <td width="50%" class="valign-top" style="padding-right: 5px;">
                        <div class="section-box" style="height: 100%;">
                            <div class="section-header">Bill To</div>
                            <div class="box-content"><asp:Literal ID="litBillTo" runat="server" /></div>
                        </div>
                    </td>
                    <td width="50%" class="valign-top" style="padding-left: 5px;">
                        <div class="section-box" style="height: 100%;">
                            <div class="section-header">Consignee Details (Ship To)</div>
                            <div class="box-content"><asp:Literal ID="litShipTo" runat="server" /></div>
                        </div>
                    </td>
                </tr>
            </table>

            <div class="section-header" style="border: 1px solid #000; border-bottom: none;">Materials Ordered</div>
            <asp:GridView ID="gvItems" runat="server" CssClass="full-width std-table" AutoGenerateColumns="False" 
                OnRowDataBound="gvItems_RowDataBound1" ShowFooter="true">
                <HeaderStyle CssClass="grid-header" />
                <Columns>
                    <asp:BoundField HeaderText="Sr.No." DataField="ItemOrder" ItemStyle-Width="5%" ItemStyle-CssClass="text-center" />
                    <asp:BoundField HeaderText="Product Description" DataField="ProductName" ItemStyle-Width="35%" />
                    <asp:BoundField HeaderText="Qty" DataField="Quantity" DataFormatString="{0:N2}" ItemStyle-Width="8%" ItemStyle-CssClass="text-center" />
                    <asp:BoundField HeaderText="Rate" DataField="Rate" DataFormatString="{0:N2}" ItemStyle-Width="10%" ItemStyle-CssClass="text-right" />
                    <asp:BoundField HeaderText="Disc" DataField="DiscountAmount" DataFormatString="{0:N2}" ItemStyle-Width="10%" ItemStyle-CssClass="text-right" />
                    <asp:BoundField HeaderText="Taxable" DataField="TaxableAmount" DataFormatString="{0:N2}" ItemStyle-Width="10%" ItemStyle-CssClass="text-right" />
                    <asp:BoundField HeaderText="GST" DataField="TaxAmount" DataFormatString="{0:N2}" ItemStyle-Width="10%" ItemStyle-CssClass="text-right" />
                    <asp:BoundField HeaderText="Total" DataField="NetAmount" DataFormatString="{0:N2}" ItemStyle-Width="12%" ItemStyle-CssClass="text-right" />
                </Columns>
            </asp:GridView>

            <table class="full-width avoid-break mt-10">
                <tr>
                    <td width="60%" class="valign-top" style="padding-right: 10px;">
                        <div style="border: 1px solid #000; padding: 8px; background: #f9f9f9;">
                            <b>Amount in Words:</b><br />
                            <asp:Label ID="lblAmountInWords" runat="server" />
                        </div>
                    </td>
                    <td width="40%" class="valign-top">
                        <table class="full-width std-table">
                            <tr class="grid-header"><td colspan="3">Tax Summary</td></tr>
                            <asp:PlaceHolder ID="phGST" runat="server" />
                            <tr style="background:#eee; border-top: 2px solid #000;">
                                <td colspan="2" class="bold text-right">Grand Total</td>
                                <td class="bold text-right"><asp:Label ID="lblGrandTotal" runat="server" /></td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

            <div class="section-box mt-10 avoid-break">
                <div class="section-header">Commercial / Logistics Terms</div>
                <table class="full-width std-table" style="border:none;">
                    <tr>
                        <td width="20%" class="bold" style="background:#f9f9f9; border-left:none;">Rates are as per:</td>
                        <td width="30%"><asp:Label ID="lblRateRef" runat="server" /></td>
                        <td width="20%" class="bold" style="background:#f9f9f9;">Special Rates By:</td>
                        <td width="30%" style="border-right:none;"><asp:Label ID="lblApprovedBy" runat="server" /></td>
                    </tr>
                    <tr>
                        <td class="bold" style="background:#f9f9f9; border-left:none;">Freight Charges:</td>
                        <td><asp:Label ID="lblFreight" runat="server" /></td>
                        <td class="bold" style="background:#f9f9f9;">Despatch Mode:</td>
                        <td style="border-right:none;"><asp:Label ID="lblDispatchMode" runat="server" /></td>
                    </tr>
                    <tr>
                        <td class="bold" style="background:#f9f9f9; border-left:none; border-bottom:none;">Despatch Upto:</td>
                        <td style="border-bottom:none;"><asp:Label ID="lblDispatchUpto" runat="server" /></td>
                        <td class="bold" style="background:#f9f9f9; border-bottom:none;">Delivery Basis:</td>
                        <td style="border-right:none; border-bottom:none;"><asp:Label ID="lblDeliveryBasis" runat="server" /></td>
                    </tr>
                </table>
            </div>

            <div class="section-box mt-10 avoid-break">
                <div class="section-header">Payment Details</div>
                <table class="full-width std-table" style="border:none; text-align:center;">
                    <tr style="background:#f9f9f9; font-weight:bold;">
                        <td style="border-top:none; border-left:none;">Mode</td>
                        <td style="border-top:none;">Cheque/Ref No.</td>
                        <td style="border-top:none;">Date</td>
                        <td style="border-top:none;">Amount</td>
                        <td style="border-top:none; border-right:none;">Bank Name</td>
                    </tr>
                    <tr>
                        <td style="border-bottom:none; border-left:none;"><asp:Label ID="lblPayMode" runat="server" Text="-" /></td>
                        <td style="border-bottom:none;"><asp:Label ID="lblChequeNo" runat="server" Text="-" /></td>
                        <td style="border-bottom:none;"><asp:Label ID="lblChequeDate" runat="server" Text="-" /></td>
                        <td style="border-bottom:none;" class="text-right"><asp:Label ID="lblPayAmount" runat="server" Text="-" /></td>
                        <td style="border-bottom:none; border-right:none;"><asp:Label ID="lblBankName" runat="server" Text="-" /></td>
                    </tr>
                </table>
            </div>

            <div class="section-box mt-10 avoid-break">
                <div class="section-header">Remarks</div>
                <div class="box-content" style="min-height: 40px; font-weight:bold;">
                    <asp:Label ID="lblRemarks" runat="server" />
                </div>
            </div>

            <table class="full-width mt-10 avoid-break">
                <tr>
                    <td width="60%" valign="bottom">
                        <div style="font-size:10px; border:1px dashed #999; padding:5px; color:#555;">
                            <b>Note:</b> This is a computer generated document to avoid confusion at the time of booking and dispatches. No physical signature is required.
                        </div>
                    </td>
                    <td width="40%" class="text-right" valign="bottom">
                        <b>Prepared By:</b> <asp:Label ID="lblPreparedBy" runat="server" /><br /><br /><br />
                        __________________________<br />
                        <b>Authorized Signatory</b><br />
                        (For FLAME-EX)
                    </td>
                </tr>
            </table>

        </div>
    </form>
</body>
</html>