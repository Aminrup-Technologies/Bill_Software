<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewPurchaseOrder_Print.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewPurchaseOrder_Print" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Purchase Order Print</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    <link rel="stylesheet" href="DocumentPrint.css" />
</head>
<body class="<%= ShowLetterhead ? "" : "no-letterhead" %>">
    <form id="form1" runat="server">

        <nav id="client-toolbar" class="client-toolbar preview-toolbar" aria-label="Purchase order print actions">
            <button type="button" class="tb-btn tb-btn-secondary" onclick="goBack()">← Back</button>
            <button type="button" class="tb-btn tb-btn-primary" onclick="window.print()">Print / Save PDF</button>
            <span class="tb-hint">Use the browser Print dialog → Destination: Save as PDF</span>
        </nav>

        <div class="page-shell">
        <div class="a4-container a4-preview a4-paper document-shadow">
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
        function goBack() {
            if (window.history.length > 1) {
                history.back();
            } else {
                window.location.href = '../app/View_PurchaseOrder.aspx';
            }
        }

        <% if (AutoPrint) { %>
        window.addEventListener('load', function () {
            window.print();
        });
        <% } %>
    </script>
</body>
</html>
