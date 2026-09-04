<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewPurchaseOrder.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewPurchaseOrder" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Purchase Order Page</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />

    <style type="text/css">
        /* --- Screen/Browser View Styles --- */
        html, body {
            overflow-x: clip;
        }

        body {
            font-family: 'Century Gothic', sans-serif;
            font-size: 13px;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #e8eaed; /* Soft gray workspace for A4 preview */
        }

        .preview-toolbar {
            position: sticky;
            top: 0;
            z-index: 1000;
            text-align: center;
            padding: 12px 16px;
            margin-bottom: 0;
            background-color: #fff;
            border-bottom: 1px solid #d0d5dd;
            box-shadow: 0 1px 4px rgba(0,0,0,0.08);
        }

        .page-shell {
            display: flex;
            justify-content: center;
            align-items: flex-start;
            width: 100%;
            padding: 24px 16px 40px;
            overflow-x: hidden;
            box-sizing: border-box;
        }

        .a4-container {
            max-width: 844px; /* Print / PDF baseline */
            margin: 0 auto;
            background-color: #fff;
            padding: 20px 40px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1); 
            box-sizing: border-box;
        }

        .a4-preview {
            width: 210mm;
            min-height: 297mm;
            max-width: 100%;
            box-sizing: border-box;
        }

        /* Adaptive A4 viewer — screen only. Print and PDF capture use 210mm A4. */
        @media screen {
            .page-shell {
                display: flex;
                justify-content: center;
                align-items: flex-start;
                padding: 20px 4vw 40px;
            }

            .a4-container,
            .a4-container.a4-preview {
                width: min(94vw, 1120px);
                max-width: 1120px;
                box-sizing: border-box;
                padding: 8px 22px 20px;
            }

            .a4-preview {
                width: 100%;
                aspect-ratio: 210 / 297;
                min-height: auto;
                height: auto;
                font-size: 14px;
            }

            .a4-preview thead th {
                padding-top: 0;
                padding-bottom: 4px !important;
            }

            .a4-preview thead table {
                margin-top: 4px !important;
            }

            .a4-preview #bodycontain > tr > td {
                padding-top: 10px !important;
            }

            .a4-preview thead img,
            .a4-preview tfoot img {
                width: 100%;
                height: auto;
                display: block;
            }

            .a4-preview h2 {
                font-size: 17px !important;
            }

            .a4-preview h3 {
                font-size: 16px;
            }

            .a4-preview .term-title {
                font-size: 13px;
            }

            .a4-preview #bodycontain tfoot td {
                font-size: 15px !important;
            }

            .a4-preview .master-table,
            .a4-preview .content-table,
            .a4-preview .PaymentPhase,
            .a4-preview table[style*="border:2px solid"] {
                width: 100%;
                max-width: 100%;
                table-layout: fixed;
            }

            .a4-preview table.FORKVQAEAST,
            .a4-preview table.info-split,
            .a4-preview table.info-split table {
                max-width: 100%;
                table-layout: auto;
            }

            .a4-preview td,
            .a4-preview th {
                overflow-wrap: break-word;
            }

            .a4-preview .content-table td,
            .a4-preview .content-table th,
            .a4-preview .PaymentPhase td,
            .a4-preview .PaymentPhase th,
            .a4-preview table[style*="border:2px solid"] td,
            .a4-preview table[style*="border:2px solid"] th {
                overflow-wrap: anywhere;
                word-break: break-word;
            }

            .a4-preview img {
                max-width: 100%;
                height: auto;
            }
        }

        @media screen and (max-width: 640px) {
            .a4-preview table.info-split,
            .a4-preview table.info-split > tbody,
            .a4-preview table.info-split > tbody > tr,
            .a4-preview table.info-split > tbody > tr > td {
                display: block;
                width: 100% !important;
                box-sizing: border-box;
            }

            .a4-preview table.info-split > tbody > tr > td.info-split-gap {
                display: none;
            }

            .a4-preview table.info-split > tbody > tr > td + td.info-split-card {
                margin-top: 10px;
            }
        }

        .document-shadow {
            box-shadow: 0 4px 24px rgba(0,0,0,0.12);
        }

        .client-toolbar {
            display: flex;
            justify-content: center;
            align-items: center;
            flex-wrap: wrap;
            gap: 10px;
        }

        .client-toolbar[hidden],
        .js-hidden {
            display: none !important;
        }

        .tb-btn {
            padding: 10px 20px;
            border: none;
            cursor: pointer;
            color: #fff;
            font-family: inherit;
            font-size: 13px;
            border-radius: 4px;
        }

        .tb-btn-primary { background: #007bff; }
        .tb-btn-secondary { background: #555; }
        .tb-btn-pdf { background: #e31e24; }

        /* Base Table Styling */
        .master-table { width: 100%; border-collapse: collapse; }
        .content-table { border-collapse: collapse; width: 100%; }
        th, td { padding: 4px 6px; vertical-align: top; }
        
        .bold { font-weight: bold; }
        .gap { line-height: 5px; height: 5px; }
        
        .term-title { font-weight: bold; width: 30%; color: #444; }
        .term-desc { font-weight: normal; width: 70%; text-align: justify; }

        /* Headers & Footers Visibility Logic (Triggered by JS on buttons) */
        .header, .footer, .hide { visibility: hidden; }

        /* --- THE PRINT MAGIC FIX --- */
        @media print {
            html, body, form {
                display: block !important;
                width: auto !important;
                height: auto !important;
                overflow: visible !important;
                margin: 0 !important;
                padding: 0 !important;
                background: transparent !important;
            }

            body { 
                background-color: transparent; 
                padding: 0; 
            }
            .a4-container { 
                box-shadow: none; 
                padding: 0; 
                max-width: 210mm;
                width: 210mm;
                aspect-ratio: auto;
            }
            
            * {
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }

            /* Hide both toolbars when printing */
            #print-controls,
            .preview-toolbar,
            .client-toolbar { display: none !important; }

            .page-shell {
                display: block !important;
                flex-direction: unset;
                justify-content: unset;
                align-items: unset;
                width: auto;
                padding: 0 !important;
                background: transparent;
                overflow: visible;
            }

            .a4-container.a4-preview,
            .a4-preview {
                display: block;
                width: 210mm !important;
                max-width: 210mm !important;
                min-height: 0 !important;
                height: auto !important;
                aspect-ratio: auto !important;
                margin: 0;
                font-size: 13px;
            }

            .master-table,
            .content-table,
            .PaymentPhase,
            table[style*="border:2px solid"],
            table:not(.FORKVQAEAST) {
                table-layout: auto;
            }

            td, th {
                overflow-wrap: normal;
                word-break: normal;
            }

            table.info-split,
            table.info-split > tbody {
                display: table;
                width: 100%;
            }

            table.info-split > tbody > tr {
                display: table-row;
            }

            table.info-split > tbody > tr > td {
                display: table-cell;
            }

            .document-shadow {
                box-shadow: none !important;
            }

            /* Native HTML repeating headers and footers without overlapping */
            .master-table { page-break-inside: auto; }
            .master-table > tbody > tr,
            .master-table > tbody > tr > td {
                page-break-inside: auto;
                page-break-after: auto;
            }
            .a4-preview table:not(.master-table) tr {
                page-break-inside: avoid;
                page-break-after: auto;
            }
            
            thead { display: table-header-group; }
            tfoot { display: table-footer-group; }
            
            /* Ensure images scale correctly within A4 boundaries */
            thead img, tfoot img { width: 100%; max-width: 844px; display: block; }

            .pagebrake { page-break-inside: avoid; }
            .pagebrake1 { page-break-before: always; }
        }

        @page {
            size: A4 portrait;
            margin: 8mm 10mm;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <nav id="client-toolbar" class="client-toolbar preview-toolbar" aria-label="Purchase order actions" hidden="hidden">
            <button type="button" class="tb-btn tb-btn-secondary" onclick="goBack()">← Back</button>
            <button type="button" class="tb-btn tb-btn-primary" onclick="printWithLetterhead()">Print With Letterhead</button>
            <button type="button" class="tb-btn tb-btn-secondary" onclick="printWithoutLetterhead()">Print Without Letterhead</button>
            <button type="button" class="tb-btn tb-btn-pdf" onclick="exportPdf()">Export PDF</button>
        </nav>

        <div id="print-controls" class="preview-toolbar">
            <asp:Button ID="Button1" runat="server" UseSubmitBehavior="false" CausesValidation="false" OnClientClick="openPrintRenderer(0); return false;" Text="Print Without Letterhead" style="padding: 10px 20px; background: #555; color: #fff; border: none; cursor: pointer; margin-right: 10px;" />
            <asp:Button ID="Button2" runat="server" UseSubmitBehavior="false" CausesValidation="false" OnClientClick="openPrintRenderer(1); return false;" Text="Print With Letterhead" style="padding: 10px 20px; background: #007bff; color: #fff; border: none; cursor: pointer;" />
        </div>

        <div class="page-shell">
        <div class="a4-container a4-preview document-shadow">
            <table class="master-table">
                
                <thead id="header">
                    <tr>
                        <th style="padding-bottom: 20px; border-bottom: 2px solid #e31e24; font-weight: normal;">
                            
                            <img src="../WebImages/flame-ex_hdrtop.png" alt="Header Image" />
                            
                            <table width="100%" style="margin-top: 15px;">
                                <tr>
                                    <td style="text-align: left; vertical-align: middle;">
                                        <h1 style="margin: 0; font-size: 22px;">
                                            <asp:Label ID="Label1" runat="server" Font-Bold="true" ForeColor="DarkBlue"></asp:Label>&nbsp;[<asp:Label ID="Label2" runat="server" Visible="true"></asp:Label>]
                                        </h1>
                                    </td>
                                    <td style="text-align: right; vertical-align: middle;">
                                        <div style="font-weight: bold; font-size: 24px; color: #e31e24; text-transform: uppercase;">PURCHASE / DELIVERY ORDER</div>
                                        <div style="font-weight: bold; font-size: 14px; color: #555; margin-top: 4px;">Received from the Customer</div>
                                    </td>
                                </tr>
                            </table>
                        </th>
                    </tr>
                </thead>

                <tbody id="bodycontain">
                    <tr>
                        <td style="padding-top: 20px;">
                            
                            <table border="0" width="100%" class="info-split">
                                <tr>
                                    <td class="info-split-card" style="width: 48%; border: 1px solid #dcdcdc; background-color: #ffffff; padding: 12px; border-radius: 4px;">
                                        <div style="font-size: 14px; font-weight: bold; margin-bottom: 10px; border-bottom: 1px solid #eaeaea; padding-bottom: 5px; color: #222;">
                                            Customer Details (From)
                                        </div>
                                        <table border="0" width="100%" cellpadding="2">
                                            <tr>
                                                <td style="width: 30%; font-weight: bold; color: #555;">Name:</td>
                                                <td style="width: 70%;">
                                                    <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label>
                                                    <asp:Label ID="lbl_refname" runat="server"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr id="ref_desg" runat="server" visible="false">
                                                <td style="font-weight: bold; color: #555;">Designation:</td>
                                                <td><asp:Label ID="lbldeg" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Company:</td>
                                                <td>
                                                    <asp:Label ID="lblClient" runat="server" Font-Bold="true"></asp:Label>
                                                    <asp:Label ID="lblClientCode" runat="server" Visible="false"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Address:</td>
                                                <td>
                                                    <asp:Label ID="txtaddres" runat="server"></asp:Label><br />
                                                    <asp:Label ID="lblcity" runat="server"></asp:Label> - <asp:Label ID="lblpincode" runat="server"></asp:Label><br />
                                                    <asp:Label ID="lblContact" runat="server" Font-Size="11px" ForeColor="Gray"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>

                                    <td class="info-split-gap" style="width: 4%;"></td>

                                    <td class="info-split-card" style="width: 48%; border: 1px solid #dcdcdc; background-color: #f9f9f9; padding: 12px; border-radius: 4px;">
                                        <div style="font-size: 14px; font-weight: bold; margin-bottom: 10px; border-bottom: 1px solid #eaeaea; padding-bottom: 5px; color: #222;">
                                            Document Information
                                        </div>
                                        <table border="0" width="100%" cellpadding="2">
                                            <tr id="Tr1" runat="server" visible="true">
                                                <td style="width: 38%; font-weight: bold; color: #555;">D.O. / P.O. No:</td>
                                                <td style="width: 62%;">
                                                    <asp:Label ID="lbl_donumber" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr id="client_code" runat="server" visible="true">
                                                <td style="font-weight: bold; color: #555;">ARC No:</td>
                                                <td>
                                                    <asp:Label ID="lbl_ponumber" runat="server" ForeColor="DarkBlue" Font-Bold="true"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Date:</td>
                                                <td><asp:Label ID="lbl_podate" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">ERP Record:</td>
                                                <td>
                                                    <asp:Label ID="lblqnumber" runat="server" Font-Bold="true"></asp:Label> 
                                                    <span style="color:#666; font-size:11px;">[<asp:Label ID="lbldate" runat="server"></asp:Label>]</span>
                                                </td>
                                            </tr>
                                            <tr id="Tr2" runat="server" visible="true">
                                                <td style="font-weight: bold; color: #555;">Ref ID & Date:</td>
                                                <td>
                                                    <asp:Label ID="lbl_refid" runat="server"></asp:Label> 
                                                    <span style="color:#ccc;">|</span> 
                                                    <asp:Label ID="lbl_refdate" runat="server"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Supply Place:</td>
                                                <td>
                                                    <asp:Label ID="lblplaceofsup1" runat="server"></asp:Label>&nbsp;
                                                    <asp:Label ID="lblplaceofsup2" runat="server"></asp:Label>&nbsp;
                                                    <asp:Label ID="lblplaceofsup3" runat="server"></asp:Label>
                                                </td>
                                            </tr>

                                            <asp:Panel ID="pnlPanGst" runat="server" Visible="false">
                                                <tr><td colspan="2"><hr style="border-top: 1px dashed #ccc; margin: 4px 0;" /></td></tr>
                                                <tr>
                                                    <td style="font-weight: bold; color: #555;">Client PAN:</td>
                                                    <td><asp:Label ID="lblPanno" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr>
                                                    <td style="font-weight: bold; color: #555;">Client GST:</td>
                                                    <td><asp:Label ID="lblGstno" runat="server"></asp:Label></td>
                                                </tr>
                                            </asp:Panel>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <table border="0" width="100%" style="margin-top: 25px;">
                                <tr>
                                    <td style="text-align: center; font-weight: bold; font-size: 13px; text-decoration: underline; padding: 10px 0;">
                                        Sub: Commercial Requirements for <asp:Label ID='lblservice' runat='server'></asp:Label> <asp:Label ID='lblprimary_service' runat='server'></asp:Label> delivery
                                    </td>
                                </tr>
                            </table>

                            <div class="pagebrake" style="margin-top: 10px;">
                                <div style="font-weight: bold; margin-bottom: 10px;">
                                    To Flame-ex Team, <asp:Label ID='lbltital' runat='server'></asp:Label>&nbsp;<asp:Label ID="lbllname" runat="server"></asp:Label>
                                </div>
                                <div style="text-align: justify; margin-bottom: 10px;">
                                    <span class="bold">Please arrange to deliver below Line Items.</span>
                                </div>
                                <div style="text-align: justify;">
                                    This is with reference to our discussion for <asp:Label ID="lblPrimaryService" runat="server" Font-Bold="true"></asp:Label> 
                                    against above mentioned ARC/ P.O. & D.O. Number. We are pleased to submit our requirements specifications as below:
                                </div>
                            </div>

                            <div style="margin-top: 20px;">
                                <asp:Label ID="lblserviceamo" runat="server"></asp:Label>
                                
                                <h2 style="text-align: right; font-weight: bold; font-size: 20px; color: #e31e24; border-bottom: 2px solid #ccc; padding-bottom: 5px; margin-top: 30px;">
                                    Delivery Schedules
                                </h2>
                                
                                <asp:Label ID="lblProductDetails" runat="server"></asp:Label>
                                <div style="margin-top: 15px;"><asp:Label ID="lblPayment" runat="server"></asp:Label></div>
                                <div style="margin-top: 15px;"><asp:Label ID="lblPrimaryServicePoint" runat="server"></asp:Label></div>
                            </div>

                            <div style="margin-top: 30px; border-top: 2px solid #333; padding-top: 15px;">
                                <h3 style="margin-top: 0; color: #333;">Terms & Conditions</h3>
                                
                                <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_VALIDITYOFTHEOFFER" runat="server" visible="true">
                                    <tr>
                                        <td class="term-title">VALIDITY OF OFFER</td>
                                        <td class="term-desc">
                                            <asp:Label ID="lbl_val_default_text" runat="server" Text="This Offer is valid for "></asp:Label>
                                            <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>
                                            <asp:Label ID="lbl_val_days_text" runat="server" Text=" Days from the Date of Submission."></asp:Label>
                                            <asp:Label ID="lbl_val_dates" runat="server" Visible="false" Font-Bold="true"></asp:Label>
                                        </td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_tx" runat="server" visible="true">
                                    <tr>
                                        <td class="term-title">GST APPLICABILITY</td>
                                        <td class="term-desc">
                                            GST will be <asp:Label ID="Label3" runat="server" Font-Bold="true" Text="charged extra"></asp:Label> item-wise as applicable under the prevailing GST laws based on HSN/SAC classification.
                                        </td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="DELIVERY pagebrake" id="Table1" runat="server" visible="true">
                                    <tr>
                                        <td class="term-title">DELIVERY TERMS</td>
                                        <td class="term-desc">
                                            Within <asp:Label ID="lbl_deliverytrms" runat="server" Text="15" Font-Bold="true"></asp:Label> Weeks from the Date of Receipt of all Technical Clearance.
                                        </td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="DELIVERY pagebrake" id="Table2" runat="server" visible="false">
                                    <tr>
                                        <td class="term-title">MATERIAL ACCEPTANCE</td>
                                        <td class="term-desc">Material once invoiced cannot be returned back.</td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="DELIVERY pagebrake" id="Table3" runat="server" visible="true">
                                    <tr>
                                        <td class="term-title">PACKING & FORWARDING</td>
                                        <td class="term-desc">
                                            Charges will be <asp:Label ID="lbl_pkging" runat="server" Text="15" Font-Bold="true"></asp:Label>
                                        </td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="DELIVERY pagebrake" id="Table4" runat="server" visible="true">
                                    <tr>
                                        <td class="term-title">SPECIAL INSTRUCTIONS</td>
                                        <td class="term-desc">
                                            <asp:Label ID="lbl_remarks" runat="server" Text="N/A"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <div class="pagebrake" style="margin-top: 30px; text-align: justify;">
                                We trust the above offer is in line with your requirement and we are looking forward to receive your valued order at the earliest.<br />
                                Please feel free to contact us for any further clarifications in this regard.<br /><br />
                                Thanking you and assuring you of our best and prompt services always.<br /><br />
                                
                                <div style="margin-top: 20px;">
                                    Thanks & Regards,
                                </div>
                                <table border="0" class="FORKVQAEAST" style="margin-top: 20px; width: 300px;">
                                    <tr>
                                        <td style="font-weight: bold;">FOR FLAME-EX</td>
                                    </tr>
                                    <tr>
                                        <td style="padding: 10px 0;">
                                            <img src="../WebImages/flmx_authsign.png" width="150px" alt="Signature" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="font-weight: bold; color: #555;">Authorized Signatory</td>
                                    </tr>
                                </table>
                            </div>
                        </td>
                    </tr>
                </tbody>

                <tfoot id="footer">
                    <tr>
                        <td style="padding-top: 20px; text-align: center;">
                            <img src="../WebImages/flame-ex_hdrbtm.png" alt="Footer Image" />
                        </td>
                    </tr>
                </tfoot>

            </table>
        </div>
        </div>
    </form>
    <script type="text/javascript">
        (function () {
            var client = document.getElementById('client-toolbar');
            var fallback = document.getElementById('print-controls');
            if (client) { client.removeAttribute('hidden'); }
            if (fallback) { fallback.className = fallback.className + ' js-hidden'; }
        })();

        function goBack() {
            if (window.history.length > 1) {
                history.back();
            } else {
                window.location.href = '../app/View_PurchaseOrder.aspx';
            }
        }

        function getPoId() {
            var params = new URLSearchParams(window.location.search);
            return params.get('ID') || params.get('id') || '';
        }

        function openPrintRenderer(letterhead) {
            var url = 'NewPurchaseOrder_Print.aspx?ID=' + encodeURIComponent(getPoId())
                + '&letterhead=' + letterhead
                + '&autoprint=1';
            window.location.href = url;
        }

        function printWithLetterhead() {
            openPrintRenderer(1);
        }

        function printWithoutLetterhead() {
            openPrintRenderer(0);
        }

        function exportPdf() {
            openPrintRenderer(1);
        }
    </script>
</body>
</html>
