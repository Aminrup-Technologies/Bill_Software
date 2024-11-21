<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewChhalan.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewChhalan" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Chhalan Page</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    <style type="text/css">
        table {
            border-collapse: collapse;
            /*border:0px;*/
            /*width:100%;*/
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

        .trheight {
            line-height: 0.5px;
        }

        .gap {
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
           
        }

        @page {
            margin: 8mm 8mm 2mm 8mm;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <table border="0" width="844px">
            <thead id="header">
                <tr>
                    <th style="width: 100%">
                        <img src="../WebImages/I2ILHHeader1.png" width="100%" /></th>
                </tr>
                <tr>
                    <th></th>
                </tr>
                <tr>
                    <th></th>
                </tr>
            </thead>
            <tfoot style="width: 100%;">
                <tr>
                    <td width="100%">
                        <table width="100%" border="0">
                            <tr>
                                <td colspan="4">
                                    <br>
                                    &nbsp;</td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </tfoot>
            <tbody>
                <tr>
                    <td id="bodycontain" width="100%" style="font-weight: bold">

                        <table border="0" width="100%">
                            
                            <tr>
                                <td class="sub" style="text-align: right; font-weight: bold; font-size: 30px; color: #e31e24;">DELIVERY CHALLAN</td>
                            </tr>
                             <tr class="trheight">
                               <td class="sub" style="text-align: right; font-weight: bold;  font-size: 15px;">Original Copy for Consignee</td>
                            </tr>
                            <tr>
                                <td class="gap" style="text-align: center; font-weight: bold;">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="text-align: center; font-weight: bold;">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="text-align: center; font-weight: bold;">&nbsp</td>
                            </tr>
                        </table>

                        <table border="0" width="100%">
                            <tr>
                                <td class="add" style="vertical-align: top" width="58%">
                                    <table border="0" width="100%" class="address">
                                        <%--<tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Kind Attention</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="vertical-align: top; padding: 1px 5px; width: 68%">
                                                <asp:Label ID="lblrename" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;"></td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lbldeg" runat="server"></asp:Label></td>
                                        </tr>--%>

                                       
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Bill To</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="clientName" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Billing Address</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="txtaddres" runat="server"></asp:Label><br />
                                                <asp:Label ID="lblcity" runat="server"></asp:Label>-<asp:Label ID="lblpincode" runat="server"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Delivery Address</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblAddress" runat="server"></asp:Label>
                                            </td>
                                        </tr>
                                        <asp:Panel ID="pnlTasGst" runat="server">
                                        <tr>
                                                <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">Client GST Number</td>
                                                <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                                <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                    <asp:Label ID="lblGstno" runat="server"></asp:Label></td>
                                            </tr>
                                         <tr>
                                                <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">Client Pan Number</td>
                                                <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                                <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                    <asp:Label ID="lblClientPan" runat="server"></asp:Label></td>
                                            </tr>
                                            </asp:Panel>
                                    </table>
                                </td>
                                <td style="vertical-align: top;" width="2%"></td>
                                <td class="qno" style="vertical-align: top; background-color: #d9d3d3;" width="40%">
                                    <table border="0" width="100%" class="quotation">
                                        <tr>
                                            <td class="" style="width: 45%; vertical-align: top; padding: 1px 5px;" >Challan Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 53%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblChano" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="vertical-align: top; padding: 1px 5px; width: 45%">Challan Date</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="vertical-align: top; padding: 1px 5px; width: 53%">
                                                <asp:Label ID="lblChadate" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 45%; vertical-align: top; padding: 1px 5px;">Quotation Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 53%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblqnumber" runat="server"></asp:Label></td>
                                        </tr>
                                       <%-- <tr>
                                            <td class="" style="width: 45%; vertical-align: top; padding: 1px 5px;">Client Code</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 53%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblClientCode" runat="server"></asp:Label></td>
                                        </tr>--%>
                                         <tr>
                                            <td class='' style='width: 45%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup1" runat="server"></asp:Label></td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup2" runat="server"></asp:Label></td>
                                            <td class='' style='width: 53%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup3" runat="server"></asp:Label></td>

                                        </tr>

                                            <tr>
                                            <td class="" style="width: 45%; vertical-align: top; padding: 1px 5px;">Aminrup Technologies GST Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 53%; vertical-align: top; padding: 1px 5px;">19AAEF15315E1ZL</td>
                                        </tr>
                                            <tr>
                                            <td class="" style="width: 45%; vertical-align: top; padding: 1px 5px;">Aminrup Technologies Pan Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 53%; vertical-align: top; padding: 1px 5px;">AAEF15315E</td>
                                        </tr>
                                        
                                       
                                    </table>
                                </td>
                            </tr>
                        </table>

                        <table class="" border="0" width="100%">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" width="100%">

                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>

                        <asp:Label ID="lblProductDetails" runat="server"></asp:Label>

                        <table border="0" width="100%" class="TERMS">
                            <tr>
                                <td colspan="2" class="" style="text-align: left; font-weight: bold;">TERMS & CONDITIONS</td>
                            </tr>
                            <tr>
                                <td colspan="2" class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" style="text-align: justify; font-weight: 100; font-size:10px;padding:0.5px 2px 0.5px 2px;   vertical-align: top">
                                    1.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     	The Quality & Quantity of Materials delivered needs to be inspected by the Client at the time of Delivery itself.
                                </td>
                            </tr>
                            <tr>
                                <td class="" style="text-align: justify; font-weight: 100;font-size:10px; padding:0.5px 2px 0.5px 2px;  vertical-align: top">
                                    2.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     	Any Shortages or Damages or Quality Issues must be notified on the Delivery Challan with the Authorized Signatory’s Signature & Corporate Stamp at the time of Delivery itself.
                                </td>
                            </tr>
                            <tr>
                                <td class="" style="text-align: justify; font-weight: 100;font-size:10px;padding:0.5px 2px 0.5px 2px;  vertical-align: top">
                                    3.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     	Unless the Customer has inspected the Delivered Goods and given written notice to the Company on the day of Material Delivery that the Goods do not comply with the relevant Specifications or Quality Descriptions, the Goods are deemed to have been accepted in Good Order & Condition by the Client.</td>
                            </tr>
                             <tr>
                                <td class="" style="text-align: justify; font-weight: 100;font-size:10px;padding:0.5px 2px 0.5px 2px;  vertical-align: top">
                                    4.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     	If the Client observes any Quality Related Issues with the Delivered Materials, it is recommended not to use the Defective Goods till Return of the Damaged Goods is executed.</td>
                            </tr>
                             <tr>
                                <td class="" style="text-align: justify; font-weight: 100;font-size:10px;padding:0.5px 2px 0.5px 2px;  vertical-align: top">
                                    5.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     	All Defective Items requested for Replacement or Return by the Client shall be thoroughly inspected and a determination will be made if eligibility of requirements is met for Replacement or Exchange.</td>
                            </tr>
                             <tr>
                                <td class="" style="text-align: justify; font-weight: 100;font-size:10px;padding:0.5px 2px 0.5px 2px;  vertical-align: top">
                                    6.</td>
                                <td class="" style="text-align: justify;  font-weight: 100; font-size:10px; padding:0.5px 2px 0.5px 2px;">
                                     In case the Company finds the Delivered Items proposed for Replacement or Return by the Client in a tampered condition, the Return Request shall not be entertained.</td>
                            </tr>
                            <tr>
                                <td colspan="2" class="gap" style="">&nbsp</td>
                            </tr>
                        </table>

                        <table class="" border="0" width="100%">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" width="100%">

                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>


                        <%--<table border="0" width="100%">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="qno" width="100%">
                                    <table border="0" width="100%" class="">
                                        <tr>
                                            <td class="" style="background-color: #c8152a; color: white; text-align: center;">If you have any Question about this Chhalan, Please Contact Ms Das at +91 9674897316 or info@aminruptechnologies.co.in</td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>--%>

                        <table border="0" width="100%" class="FORKVQAEAST">
                            <tr class="trheight">
                                <td class="" style="text-align: left; font-weight: bold;">FOR Aminrup Technologies</td>
                                <td class="" style="text-align: right; font-weight: bold;"></td>
                            </tr>
                           

                            <tr>
                                <%--<td class="" style="text-align: left; font-weight: 100">
                                    <img src="../WebImages/Stamp.jpg" width="100PX" />
                                </td>--%>
                                
                                <td><img src="../WebImages/i2i_LOGO_ad_sig.png" width="150PX" /></td>
                            
                                <td class="" style="text-align: right; font-weight: bold;"></td>
                            </tr>

                            <tr class="trheight">
                                <td class="" style="text-align: left; font-weight: bold;">Authorized Signatory</td>
                                <td class="" style="text-align: right; font-weight: bold; padding-right:75px">Receiver's Signature</td>
                            </tr>
                        </table>

                        <table border="0" width="100%" class="FORKVQAEAST">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                           <%-- <tr>
                                <td class="" style="text-align: center; font-weight: bold; font-size: 14px; color: #c8152a; font-style: italic">--------------------------------Thank You For Your Business--------------------------------</td>
                            </tr>--%>
                        </table>
                    </td>
                </tr>
            </tbody>
        </table>
        <table id="footer" border="0" width="844px">
            <tr>
                <td style="height: auto; text-align: center; font-weight: bold; font-size: 14px; font-style: italic" width="100%">
                   <%-- <span style="padding-right:10px; color:#0026ff">Thank You For Your Business!</span>--%>
                    <span style="padding-right:10px; color:#c8152a">------------------------------Thank You For Your Business------------------------------</span>
                    <img src='../WebImages/I2ILHFooter.png' width='100%' />
                </td>
            </tr>
        </table>

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" OnClientClick="document.getElementById('header').className ='header'; document.getElementById('footer').className ='footer'; window.print()" Text="Print Without Header & Footer" />
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" OnClientClick="window.print()" Text="Print With Header & Footer" />

    </form>
</body>
</html>
