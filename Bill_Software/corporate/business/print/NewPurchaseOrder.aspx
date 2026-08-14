<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewPurchaseOrder.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewPurchaseOrder" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Purchase Order Page</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    
    <style type="text/css">
        /* --- Screen/Browser View Styles --- */
        body {
            font-family: 'Century Gothic', sans-serif;
            font-size: 13px;
            color: #333;
            margin: 0;
            padding: 20px 0;
            background-color: #f4f4f4; /* Gray background to make A4 stand out on screen */
        }

        .a4-container {
            max-width: 844px; /* Exact A4 width approximation */
            margin: 0 auto;
            background-color: #fff;
            padding: 20px 40px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1); 
        }

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
            body { 
                background-color: transparent; 
                padding: 0; 
            }
            .a4-container { 
                box-shadow: none; 
                padding: 0; 
                max-width: 100%; 
            }
            
            * {
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }

            /* Hide the print buttons when printing */
            #print-controls { display: none !important; }

            /* Native HTML repeating headers and footers without overlapping */
            .master-table { page-break-inside: auto; }
            tr { page-break-inside: avoid; page-break-after: auto; }
            
            thead { display: table-header-group; }
            tfoot { display: table-footer-group; }
            
            /* Ensure images scale correctly within A4 boundaries */
            thead img, tfoot img { width: 100%; max-width: 844px; display: block; }

            .pagebrake { page-break-inside: avoid; }
            .pagebrake1 { page-break-before: always; }
        }

        @page {
            margin: 8mm 10mm; /* Standardized uniform margins */
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <div style="text-align: center; margin-bottom: 20px;" id="print-controls">
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" OnClientClick="document.getElementById('header').className ='header'; document.getElementById('footer').className ='footer'; window.print()" Text="Print Without Letterhead" style="padding: 10px 20px; background: #555; color: #fff; border: none; cursor: pointer; margin-right: 10px;" />
            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" OnClientClick="window.print()" Text="Print With Letterhead" style="padding: 10px 20px; background: #007bff; color: #fff; border: none; cursor: pointer;" />
        </div>

        <div class="a4-container">
            <table class="master-table">
                
                <thead id="header">
                    <tr>
                        <th style="padding-bottom: 20px; border-bottom: 2px solid #e31e24; font-weight: normal;">
                            
                            <img src="../WebImages/flame-ex_hdrtop.png" alt="Header Image" />
                            
                            <table width="100%" style="margin-top: 15px;">
                                <tr>
                                    <td style="text-align: left; vertical-align: middle;">
                                        <h1 style="margin: 0; font-size: 22px; color: #24285F;">Procurement Department</h1>
                                    </td>
                                    <td style="text-align: right; vertical-align: middle;">
                                        <div style="font-weight: bold; font-size: 24px; color: #e31e24; text-transform: uppercase;">PURCHASE ORDER</div>
                                        <div style="font-weight: bold; font-size: 14px; color: #555; margin-top: 4px;">Issued to Vendor</div>
                                    </td>
                                </tr>
                            </table>
                        </th>
                    </tr>
                </thead>

                <tbody id="bodycontain">
                    <tr>
                        <td style="padding-top: 20px;">
                            
                            <table border="0" width="100%">
                                <tr>
                                    <td style="width: 48%; border: 1px solid #dcdcdc; background-color: #ffffff; padding: 12px; border-radius: 4px;">
                                        <div style="font-size: 14px; font-weight: bold; margin-bottom: 10px; border-bottom: 1px solid #eaeaea; padding-bottom: 5px; color: #222;">
                                            Vendor Details (To)
                                        </div>
                                        <table border="0" width="100%" cellpadding="2">
                                            <tr>
                                                <td style="width: 30%; font-weight: bold; color: #555;">Name:</td>
                                                <td style="width: 70%;">
                                                    <asp:Label ID="lblVendorName" runat="server" Font-Bold="true"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Address:</td>
                                                <td>
                                                    <asp:Label ID="lblVendorAddress" runat="server"></asp:Label><br />
                                                    <asp:Label ID="lblVendorCity" runat="server"></asp:Label>,
                                                    <asp:Label ID="lblVendorState" runat="server"></asp:Label> -
                                                    <asp:Label ID="lblVendorPincode" runat="server"></asp:Label><br />
                                                    <asp:Label ID="lblVendorContact" runat="server" Font-Size="11px" ForeColor="Gray"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">PAN:</td>
                                                <td><asp:Label ID="lblVendorPAN" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">GST No:</td>
                                                <td><asp:Label ID="lblVendorGST" runat="server"></asp:Label></td>
                                            </tr>
                                        </table>
                                    </td>

                                    <td style="width: 4%;"></td>

                                    <td style="width: 48%; border: 1px solid #dcdcdc; background-color: #f9f9f9; padding: 12px; border-radius: 4px;">
                                        <div style="font-size: 14px; font-weight: bold; margin-bottom: 10px; border-bottom: 1px solid #eaeaea; padding-bottom: 5px; color: #222;">
                                            Document Information
                                        </div>
                                        <table border="0" width="100%" cellpadding="2">
                                            <tr>
                                                <td style="width: 38%; font-weight: bold; color: #555;">P.O. No:</td>
                                                <td style="width: 62%;">
                                                    <asp:Label ID="lblPONo" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">P.O. Date:</td>
                                                <td><asp:Label ID="lblPODate" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Requisition No:</td>
                                                <td><asp:Label ID="lblReqNo" runat="server" Font-Bold="true"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="font-weight: bold; color: #555;">Engineer:</td>
                                                <td><asp:Label ID="lblEngineerName" runat="server"></asp:Label></td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <div class="pagebrake" style="margin-top: 20px; text-align: justify;">
                                This Purchase Order is issued for the supply of goods/services detailed below.
                                <span class="bold">Kindly acknowledge receipt and proceed strictly as per the terms and conditions mentioned herein.</span>
                            </div>

                            <div style="margin-top: 20px;">
                                <h2 style="text-align: right; font-weight: bold; font-size: 20px; color: #e31e24; border-bottom: 2px solid #ccc; padding-bottom: 5px;">
                                    Purchase Order Items
                                </h2>
                                
                                <asp:Label ID="lblPOItems" runat="server"></asp:Label>
                            </div>

                            <div style="margin-top: 30px; border-top: 2px solid #333; padding-top: 15px;">
                                <h3 style="margin-top: 0; color: #333;">Terms &amp; Conditions</h3>
                                
                                <table border="0" width="100%" class="pagebrake" id="tblDispatchMode" runat="server">
                                    <tr>
                                        <td class="term-title">DISPATCH MODE</td>
                                        <td class="term-desc"><asp:Label ID="lblDispatchMode" runat="server" Text="N/A"></asp:Label></td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="pagebrake" id="tblDeliveryBasis" runat="server">
                                    <tr>
                                        <td class="term-title">DELIVERY BASIS</td>
                                        <td class="term-desc"><asp:Label ID="lblDeliveryBasis" runat="server" Text="N/A"></asp:Label></td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="pagebrake" id="tblFreightTerms" runat="server">
                                    <tr>
                                        <td class="term-title">FREIGHT TERMS</td>
                                        <td class="term-desc"><asp:Label ID="lblFreightTerms" runat="server" Text="N/A"></asp:Label></td>
                                    </tr>
                                </table>

                                <table border="0" width="100%" class="pagebrake" id="tblSpecialInstructions" runat="server">
                                    <tr>
                                        <td class="term-title">SPECIAL INSTRUCTIONS</td>
                                        <td class="term-desc"><asp:Label ID="lblRemarks" runat="server" Text="N/A"></asp:Label></td>
                                    </tr>
                                </table>
                            </div>

                            <div class="pagebrake" style="margin-top: 30px; text-align: justify;">
                                Please treat this as a formal authorization to proceed with the supply/service detailed above, strictly in accordance with the terms and conditions mentioned herein.<br />
                                Kindly acknowledge receipt of this Purchase Order at the earliest and revert to us in case of any discrepancy.<br /><br />
                                
                                <div style="margin-top: 20px;">
                                    Thanks &amp; Regards,
                                </div>
                                <table border="0" style="margin-top: 20px; width: 300px;">
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
    </form>
</body>
</html>
