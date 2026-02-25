<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewPurchaseOrder.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewPurchaseOrder" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Purchase Order Page</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    <style type="text/css">
        table {
            border-collapse: collapse;
        }

        th, td {
            border: 0px solid #c6c7cc;
            font-family: 'Century Gothic';
            font-size: 12px;
            padding: 3px 5px;
        }

        .bold {
            font-weight: bold;
        }

        .gap {
            line-height: 0.5px;
        }

        .gap1 {
            padding: 15px 5px;
        }

        .trheight {
            line-height: 0.5px;
        }

        .header, .hide {
            visibility: hidden;
            height: 120px;
        }

        .footer, .hide {
            visibility: hidden;
        }



        @media print {
            #footer {
                display: block;
                position: fixed;
                bottom: 0px;
            }

            #bodycontain {
                padding-bottom: 25px;
                overflow-y: auto;
            }

            #Button1 {
                visibility: hidden;
            }

            #Button2 {
                visibility: hidden;
            }

            .pagebrake {
                page-break-inside: avoid;
            }

            .pagebrake1 {
                page-break-before: always;
            }
        }

        @page {
            margin: 6mm 6mm 6mm 16mm;
        }

        .auto-style2 {
            line-height: 0.5px;
            height: 7px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <table border='0' width='844px'>
            <thead id='header'>
                <tr>
                    <th style='width: 100%'>
                        <%--<img src="../WebImages/flame-ex_hdrtop.png" width="100%" height="150px">--%>
                        <h1>
                            <asp:Label ID="Label1" runat="server" Font-Bold="true" ForeColor="DarkBlue"></asp:Label>&nbsp;[<asp:Label ID="Label2" runat="server" Visible="true"></asp:Label>]</h1>
                    </th>
                </tr>
                <tr>
                    <td></td>
                </tr>
                <tr>
                    <td colspan='4' style="text-align: right; font-weight: bold; font-size: 30px; color: #e31e24;">PURCHASE / DELIVERY ORDER</td>
                </tr>
                <tr>
                    <td class="sub" style="text-align: right; font-weight: bold; font-size: 15px;">Received from the Customer</td>
                </tr>
            </thead>

            <tfoot style='width: 100%;'>
                <tr>
                    <td width='100%'>
                        <table width='100%' border='0'>
                            <tr>
                                <td colspan='4'>
                                    <br>
                                    &nbsp;</td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </tfoot>

            <tbody style="font-family: 'Century Gothic'; font-size: 12px; padding: 3px 5px; border: 0px solid #c6c7cc;">
                <tr>
                    <td id='bodycontain' width='100%' style='font-weight: bold'>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                            </tr>
                        </table>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                            </tr>
                        </table>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='add' style='vertical-align: top' width='53%'>
                                    <table border='0' width='100%' class='address'>
                                        <tr>
                                            <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>From,</td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'></td>
                                            <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                                </td>
                            </tr>
                            <%--<tr>
                                <td class='add' style='vertical-align: top' width='53%'>
                                    <table border='0' width='100%' class='address'>
                                        <tr>
                                            <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Kind Attention</td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                            <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                                </td>
                            </tr>--%>
                            <%--<tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Kind Attention</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                            </tr>--%>
                            <tr id="ref_desg" runat="server" visible="false">
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'></td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>&nbsp;</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbldeg" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Company Name</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblClient" runat="server"></asp:Label>&nbsp;<asp:Label ID="lblClientCode" runat="server" Visible="false"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Address</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="txtaddres" runat="server"></asp:Label><br />
        <asp:Label ID="lblcity" runat="server"></asp:Label>-<asp:Label ID="lblpincode" runat="server"></asp:Label><br />
        <asp:Label ID="lblContact" runat="server" Font-Size="10px" ForeColor="Gray"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style='vertical-align: top;' width='2%'></td>
                    <td class='qno' style='vertical-align: top; background-color: #d9d3d3;' width='45%'>
                        <table border='0' width='100%' class='quotation'>
                            <tr id="Tr1" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Delivery Order/ P.O. No</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_donumber" runat="server" ForeColor="Red" Text="D.O. No"></asp:Label>
                                </td>
                            </tr>
                            <tr id="client_code" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>ARC No</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_ponumber" runat="server" ForeColor="DarkBlue" Text="ARC / P.O. No"></asp:Label>&nbsp[<asp:Label ID="lbl_podate" runat="server" Text="ARC / PO Date"></asp:Label>]</td>
                            </tr>
                            <tr>
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>P.O. / D.O. Date</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbldate" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class="" style='width: 38%; vertical-align: top; padding: 1px 5px;'>ERP Record Number</td>
                                <td class="" style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class="" style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblqnumber" runat="server"></asp:Label></td>
                            </tr>

                            <tr>
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup1" runat="server"></asp:Label></td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup2" runat="server"></asp:Label></td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup3" runat="server"></asp:Label></td>
                            </tr>

                            <tr id="Tr2" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Reference ID</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_refid" runat="server"></asp:Label></td>
                            </tr>
                            <tr id="Tr3" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Reference Date</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_refdate" runat="server"></asp:Label></td>
                            </tr>
                            <asp:Panel ID="pnlPanGst" runat="server" Visible="false">
                                <tr>
                                    <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Client PAN Number</td>
                                    <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                    <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                        <asp:Label ID="lblPanno" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Client GST Number</td>
                                    <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                    <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                        <asp:Label ID="lblGstno" runat="server"></asp:Label></td>
                                </tr>
                            </asp:Panel>

                        </table>
                    </td>
                </tr>
            </tbody>
        </table>

        <table border='0' width='100%'>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
        </table>

        <table border='0' width='100%'>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
            <tr>
                <td class='sub' style='text-align: center; font-weight: bold; font-size: 12px; text-decoration: underline;'>Sub: Commercial Requiremnts for
                                    <asp:Label ID='lblservice' runat='server'></asp:Label>
                    <asp:Label ID='lblprimary_service' runat='server'></asp:Label>
                    delivery</td>
            </tr>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
        </table>

        <table border='0' width='100%' class='bodytext pagebrake'>
            <tr>
                <td class='' style='text-align: left; font-weight: bold;'>To Flame-ex Team,
                                    <asp:Label ID='lbltital' runat='server'></asp:Label>&nbsp;<asp:Label ID="lbllname" runat="server"></asp:Label></td>
            </tr>
            <%--<tr>
                <td class="gap" style="">&nbsp</td>
            </tr>--%>
            <tr>
                <td class='' style='text-align: justify; font-weight: 100'>
                    <span class='bold'>Please arrange to deliver below Line Items</span>
                </td>
            </tr>
            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>
            <%--<tr>
                                <td class='' style='text-align: justify; font-weight: 100'>We are pleased to <span class='bold'>offer</span> our <span class='bold'>Quote</span> detailing the <span class='bold'>Technical & Commercial Terms</span> for the <span class='bold'>
                                    <asp:Label ID="lblPrimaryService" runat="server"></asp:Label>.</span>
                                </td>
                            </tr>--%>
            <tr>
                <td class='' style='text-align: justify; font-weight: 100'>This is with reference to our discussion for 
                                    <asp:Label ID="lblPrimaryService" runat="server"></asp:Label>
                    against above mentioned ARC/ P.O. & D.O. Number, We are pleased to submit our requirements specifications as below:
                </td>
            </tr>


            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>

            <%--<tr>
                <td class="" style="">OUR CLIENTS</td>
            </tr>--%>

            <%--<tr id="clients_img" runat="server" visible="false">
                <td class="" style="height: 250px">
                    <img src="../WebImages/clientsbg.png" width='100%' height='250px' />
                </td>
            </tr>--%>

            <%--<tr>
                <td class="gap" style="">&nbsp</td>
            </tr>--%>
        </table>

        <%--    <table border="0" width="100%" class="fees pagebrake">
                            <tr>
                                <td class="" style="text-align: left; font-weight: bold;">OUR FEES</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                           
                            <tr>
                                <td class="" style="">
                                        <asp:Label ID="lblcgstsgstOrigst" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="auto-style1"></td>
                            </tr>
                        </table>--%>

        <%--<table border="0" width="100%" class='Payment pagebrake'>
                            <tr>
                                <td>
                                    <asp:Label ID="lblserviceamo" runat="server"></asp:Label>
                                </td>
                            </tr>
                        </table>--%>


        <br />
        <asp:Label ID="lblserviceamo" runat="server"></asp:Label>

        <br />
        <table border="0" width="100%">
            <tr>
                <td colspan='4' style="text-align: right; font-weight: bold; font-size: 30px; color: #e31e24;">Delivery Schedules</td>
            </tr>
        </table>
        <asp:Label ID="lblProductDetails" runat="server"></asp:Label>


        <br />

        <asp:Label ID="lblPayment" runat="server"></asp:Label>

        <br />

        <asp:Label ID="lblPrimaryServicePoint" runat="server"></asp:Label>


        <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_VALIDITYOFTHEOFFER" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">VALIDITYOF THE OFFER</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">This Offer is valid for
                    <asp:Label ID="lbl_val_default_text" runat="server" Text="This Offer is valid for "></asp:Label>
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>
                    <asp:Label ID="lbl_val_days_text" runat="server" Text=" Days from the Date of Submission."></asp:Label>
                    <asp:Label ID="lbl_val_dates" runat="server" Visible="false" Font-Bold="true"></asp:Label><br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">This Offer is valid for
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Days from the Date of Submission.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_tx" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">GST APPLICABILITY</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;"><span>GST will be <asp:Label ID="Label3" runat="server" Font-Bold="true" Text="charged extra"></asp:Label>&nbsp;item-wise as applicable under the prevailing GST laws based on HSN/SAC classification.</span><br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">This Offer is valid for
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Days from the Date of Submission.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table1" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">DELIVERY TERMS</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Within 
                    <asp:Label ID="lbl_deliverytrms" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Weeks from the Date of Receipt of all Technical Clearance.<br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Within 
                    <asp:Label ID="lbl_deliverytrms" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Weeks from the Date of Receipt of all Technical Clearance.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table2" runat="server" visible="false">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">MATERIAL ACCEPTANCE</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Material once invoiced cannot be returned back
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Material once invoiced cannot be returned back
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table3" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">PACKING & FORWARDING</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Charges will be 
                    <asp:Label ID="lbl_pkging" runat="server" Text="15" Font-Bold="true"></asp:Label><br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Charges will be 
                    <asp:Label ID="lbl_pkging" runat="server" Text="15" Font-Bold="true"></asp:Label><br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table4" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold;">SPECIAL NOTE / INSTRUCTIONS</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">
                    <asp:Label ID="lbl_remarks" runat="server" Text="N/A"></asp:Label>&nbsp;<br />
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td colspan="2" class="" style="text-align: justify; font-weight: 100">We trust the above offer is in line with your requirement and we are looking forward to receive your valued order at the earliest.<br />
                    Please feel free to contact us for any further clarifications in this regard.<br />
                    <br />
                    Thanking you and assuring you of our best and prompt services always.<br />
                    <br />
                    <br />
                    <br />
                    Thanks & Regards,
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="FORKVQAEAST">
            <tr class="trheight">
                <td class="" style="text-align: left; font-weight: bold;">FOR FLAME-EX</td>
            </tr>

            <tr>
                <td>
                    <img src="../WebImages/flmx_authsign.png" width="150PX" /></td>
            </tr>

            <tr class="trheight">
                <td class="" style="text-align: left; font-weight: bold;">Authorized Signatory</td>
            </tr>
        </table>

        <table id='footer' border='0' width='844px'>
            <tr>
                <td style='height: auto;' width='100%'>
                    <img src="../WebImages/flame-ex_hdrbtm.png" width='100%' />
                </td>
            </tr>
        </table>

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" OnClientClick="document.getElementById('header').className ='header'; document.getElementById('footer').className ='footer'; window.print()" Text="Print Without Header & Footer" />
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" OnClientClick="window.print()" Text="Print With Header & Footer" />

    </form>
</body>
</html>
