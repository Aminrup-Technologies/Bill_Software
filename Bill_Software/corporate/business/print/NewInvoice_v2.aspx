<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewInvoice_v2.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewInvoice_v2" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Enterprise Tax Invoice</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.min.js"></script>
    
    <style type="text/css">
        body {
            font-family: 'Century Gothic', 'Segoe UI', Arial, sans-serif;
            font-size: 10px;
            color: #111;
            background-color: #555;
            margin: 0;
            padding: 20px 0;
        }

        .no-print-bar {
            background: #19658A;
            padding: 12px;
            text-align: center;
            color: white;
            margin-bottom: 20px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }

        .btn-action {
            background-color: #ff9800;
            color: white;
            border: none;
            padding: 8px 18px;
            font-size: 13px;
            cursor: pointer;
            border-radius: 4px;
            font-weight: bold;
            margin: 0 8px;
        }

        .a4-container {
            width: 794px; 
            margin: 0 auto;
            background: #fff;
            box-shadow: 0 0 15px rgba(0,0,0,0.3);
            box-sizing: border-box;
        }

        .content-wrap {
            padding: 5px 25px 15px 25px;
        }

        .info-table, .grid-table {
            width: 100%;
            border-collapse: collapse;
        }

        .info-table td {
            vertical-align: top;
            padding: 3px 5px;
            font-size: 11px;
            line-height: 1.4;
        }

        .grid-table {
            margin-top: 8px;
            margin-bottom: 12px;
            border: 1px solid #333;
        }

        .grid-table th {
            background-color: #f1f5f9;
            color: #000;
            border: 1px solid #333;
            padding: 5px 2px;
            text-align: center;
            font-size: 9px;
            text-transform: uppercase;
            font-weight: bold;
        }

        .grid-table td {
            border: 1px solid #333;
            padding: 5px 2px;
            vertical-align: middle;
            font-size: 10px;
        }

        .text-right { text-align: right !important; }
        .text-center { text-align: center !important; }
        .bold { font-weight: bold; }

        .qr-box {
            width: 65px;
            height: 65px;
            border: 1px solid #999;
            padding: 2px;
            float: right;
        }

        .invoice-banner {
            display: flex; 
            justify-content: space-between; 
            border-bottom: 2px solid #19658A; 
            padding-bottom: 6px; 
            margin-bottom: 10px;
            font-size: 13px;
        }

        @media print {
            body { background: white; margin: 0; padding: 0; }
            .no-print-bar { display: none !important; }
            .a4-container { box-shadow: none; width: 100%; margin: 0; }
            .grid-table thead { display: table-header-group; }
            tr { page-break-inside: avoid; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <div class="no-print-bar">
            <label style="font-weight:bold; margin-right:8px;">Select Copy:</label>
            <select id="ddlCopyType" onchange="updateCopyType()" style="padding:5px; border-radius:3px; margin-right: 15px;">
                <option value="ORIGINAL BUYER'S COPY">Original Buyer's Copy</option>
                <option value="DUPLICATE FOR TRANSPORTER">Duplicate for Transporter</option>
                <option value="TRIPLICATE FOR SUPPLIER">Triplicate for Supplier</option>
            </select>
            <button type="button" class="btn-action" onclick="window.print()">🖨️ Print Invoice</button>
            <button type="button" class="btn-action" style="background-color:#28a745;" onclick="downloadPDF()">📄 Download PDF</button>
        </div>

        <div id="invoice-doc" class="a4-container">
            
            <asp:Image ID="Image21" runat="server" Width="100%" Height="120px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrtop.png" Style="display:block;" />
            
            <div class="content-wrap">
                <div style="text-align:center; margin: 6px 0 10px 0;">
                    <h2 style="margin:0; font-size: 18px; color:#c8152a; letter-spacing: 0.5px;">TAX INVOICE</h2>
                    <span id="lblCopyTypeDisplay" style="font-weight:bold; font-size:10px; border:1px solid #333; padding:1px 8px; border-radius:10px; display:inline-block; margin-top:2px;">ORIGINAL BUYER'S COPY</span>
                </div>

                <div class="invoice-banner">
                    <div><span class="bold" style="color:#666;">Invoice No:</span> <asp:Label ID="lblInvoiceNo" runat="server" ForeColor="#c8152a" Font-Bold="true"></asp:Label></div>
                    <div><span class="bold" style="color:#666;">Invoice Date:</span> <asp:Label ID="lblInvoiceDate" runat="server" Font-Bold="true"></asp:Label></div>
                </div>

                <table class="info-table" style="border: 1px solid #333; margin-bottom: 10px; background: #fafafa;">
                    <tr>
                        <td style="width: 55%; border-right: 1px solid #333; padding: 8px;">
                            <table class="info-table">
                                <tr><td class="bold" style="width: 90px;">Bill To</td><td>: <asp:Label ID="lblClientName" runat="server"></asp:Label></td></tr>
                                <tr><td class="bold">Billing Address</td><td>: <asp:Label ID="lblBillingAddress" runat="server"></asp:Label></td></tr>
                                <tr><td class="bold" style="padding-top:6px;">Delivery Address</td><td style="padding-top:6px;">: <asp:Label ID="lblShippingAddress" runat="server"></asp:Label></td></tr>
                            </table>
                        </td>

                        <td style="width: 45%; padding: 8px;">
                            <div id="qrcode" class="qr-box"></div>
                            <table class="info-table" style="width: calc(100% - 75px);">
                                <tr><td class="bold" style="width: 80px;">Cust. PO No</td><td>: <asp:Label ID="lblPODONo" runat="server"></asp:Label> <asp:Label ID="lblPODate" runat="server" Font-Size="9px" ForeColor="#555"></asp:Label></td></tr>
                                <tr><td class="bold">ERP Ref.</td><td>: <asp:Label ID="lblQuoteRef" runat="server"></asp:Label></td></tr>
                                <tr><td class="bold">Client GST</td><td class="bold">: <asp:Label ID="lblGSTIN" runat="server"></asp:Label></td></tr>
                                <tr><td class="bold">Place Supply</td><td>: <asp:Label ID="lblPOS" runat="server"></asp:Label> [Code: <asp:Label ID="lblStateCode" runat="server"></asp:Label>]</td></tr>
                            </table>
                        </td>
                    </tr>
                </table>

                <table class="grid-table">
                    <thead>
                        <tr>
                            <th rowspan="2" style="width:3%;">S.NO</th>
                            <th rowspan="2" style="width:21%;">PARTICULARS</th>
                            <th rowspan="2" style="width:6%;">HSN</th>
                            <th rowspan="2" style="width:4%;">QTY</th>
                            <th rowspan="2" style="width:6%;">RATE</th>
                            <th rowspan="2" style="width:7%;">GROSS (₹)</th>
                            <th rowspan="2" style="width:6%;">DISC (₹)</th>
                            <th rowspan="2" style="width:7%;">TAXABLE</th>
                            <th colspan="2" style="width:10%;">CGST</th>
                            <th colspan="2" style="width:10%;">SGST</th>
                            <th colspan="2" style="width:10%;">IGST</th>
                            <th rowspan="2" style="width:10%; text-align:right;">TOTAL (₹)</th>
                        </tr>
                        <tr>
                            <th style="width:4%;">%</th><th style="width:6%;">AMT</th>
                            <th style="width:4%;">%</th><th style="width:6%;">AMT</th>
                            <th style="width:4%;">%</th><th style="width:6%;">AMT</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Literal ID="litInvoiceItems" runat="server"></asp:Literal>
                    </tbody>
                </table>

                <table class="info-table" style="margin-bottom: 10px;">
                    <tr>
                        <td style="width: 55%; padding-right: 15px;">
                            <div style="font-size: 11px; margin-bottom: 6px;">
                                <span class="bold">Amount (In Words):</span><br />
                                <i style="color:#222;"><asp:Label ID="lblAmountInWords" runat="server"></asp:Label></i>
                            </div>
                            
                            <div style="border: 1px solid #bbb; padding: 5px; background: #fafafa; font-size: 9px; line-height: 1.3;">
                                <span class="bold" style="color: #c8152a;">PAYMENT TERMS & BANK DETAILS</span><br />
                                All Payments shall be made through Demand Draft / NEFT / RTGS in favour of "FLAME-EX".<br />
                                <strong>Bank:</strong> HDFC Bank Ltd. | <strong>A/C No:</strong> 502000XXXXXXX | <strong>IFSC:</strong> HDFC0001234
                            </div>
                        </td>
                        
                        <td style="width: 45%;">
                            <table style="width:100%; border:1px solid #333; border-collapse:collapse; font-size: 10px;">
                                <tr><td class="bold" style="padding:4px; border-bottom:1px solid #ccc;">TOTAL TAXABLE VALUE:</td><td class="text-right bold" style="padding:4px; border-bottom:1px solid #ccc;"><asp:Label ID="lblTotalTaxable" runat="server"></asp:Label></td></tr>
                                <asp:Literal ID="litTaxBreakdown" runat="server"></asp:Literal>
                                <tr><td class="bold" style="padding:4px; border-bottom:1px solid #333;">ADD: FREIGHT CHARGES:</td><td class="text-right bold" style="padding:4px; border-bottom:1px solid #333;"><asp:Label ID="lblFreight" runat="server" Text="0.00"></asp:Label></td></tr>
                                <tr style="background-color: #e2e8f0;"><td class="bold" style="padding:6px; font-size:12px;">GRAND TOTAL:</td><td class="text-right bold" style="padding:6px; font-size:12px; color:#c8152a;">₹ <asp:Label ID="lblGrandTotal" runat="server"></asp:Label></td></tr>
                            </table>
                        </td>
                    </tr>
                </table>

                <table class="info-table" style="margin-bottom: 5px;">
                    <tr>
                        <td style="width: 70%;"></td>
                        <td style="width: 30%; text-align: right;">
                            <span class="bold" style="font-size: 11px;">For FLAME-EX</span><br />
                            <img src="../WebImages/flmx_authsign.png" width="110" alt="Sign" style="margin: 2px 0;" /><br />
                            <span style="border-top: 1px solid #333; padding-top: 2px; display:inline-block; font-size:10px;">Authorized Signatory</span>
                        </td>
                    </tr>
                </table>
            </div>

            <asp:Image ID="Image22" runat="server" Width="80%" Height="130px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrbtm.png" Style="display:block;" />
            
            <asp:HiddenField ID="hdnQRPayload" ClientIDMode="Static" runat="server" />
            <asp:HiddenField ID="hdnInvoiceFileName" ClientIDMode="Static" runat="server" />
        </div>

        <script>
            function updateCopyType() {
                document.getElementById("lblCopyTypeDisplay").innerText = document.getElementById("ddlCopyType").value;
            }

            window.onload = function () {
                var payload = document.getElementById("hdnQRPayload").value;
                if (payload) {
                    new QRCode(document.getElementById("qrcode"), { text: payload, width: 65, height: 65, correctLevel: QRCode.CorrectLevel.M });
                }
            };

            function downloadPDF() {
                var invNoClean = document.getElementById("hdnInvoiceFileName").value || "TaxInvoice";
                var element = document.getElementById('invoice-doc');
                var opt = {
                    margin:       0,
                    filename:     invNoClean + '.pdf',
                    image:        { type: 'jpeg', quality: 0.98 },
                    html2canvas:  { scale: 2, useCORS: true },
                    jsPDF:        { unit: 'in', format: 'A4', orientation: 'portrait' }
                };
                html2pdf().set(opt).from(element).save();
            }
        </script>
    </form>
</body>
</html>