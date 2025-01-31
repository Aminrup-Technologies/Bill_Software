<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewProformaInvoice.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewProformaInvoice" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PRO FORMA INVOICE</title>
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
            /*#footer:after {
                text-align: center;
                counter-increment: page;
                content: counter(page);
            }*/

            /*#footer:after {
                counter-increment: page;
                content: "Page " counter(page);
                right: 0;
                top: 100%;
                white-space: nowrap;
                z-index: 10px;
                -moz-border-radius: 5px;
                -moz-box-shadow: 0px 0px 4px #222;
                background-image: -moz-linear-gradient(top, #eeeeee, #cccccc);
                background-image: -moz-linear-gradient(top, #eeeeee, #cccccc);
            }*/


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
                        <asp:Image ID="Image21" runat="server"
                            Width="844px" Height="140px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrtop.png" /></th>
                </tr>
                <tr>
                    <th>
                        <%--<hr style="color: #000080" />--%>
                    </th>
                </tr>
                <tr>
                    <th>
                        <%--<hr style="color: #000080" />--%>
                    </th>
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
                                <td class="gap" style="text-align: center; font-weight: bold;">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="sub" style="text-align: right; font-weight: bold; font-size: 25px; color: #c8152a;">PRO FORMA INVOICE</td>
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
                                <td class="add" style="vertical-align: top" width="51%">
                                    <table border="0" width="100%" class="address">
                                        <tr>
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
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Bill To</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="clientName" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 30%; vertical-align: top; padding: 1px 5px;">Address</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 68%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="txtaddres" runat="server"></asp:Label><br />
                                                <asp:Label ID="lblcity" runat="server"></asp:Label>-<asp:Label ID="lblpincode" runat="server"></asp:Label>
                                            </td>
                                        </tr>

                                        <%-- <tr>
                                            <td class="" style="vertical-align: top">ASSIGNED FACTORY</td>
                                            <td class="">:<asp:Label ID="lblfactoryaddress" runat="server"></asp:Label><br />
                                                <asp:Label ID="lblfactorycity" runat="server"></asp:Label>
                                            -<asp:Label ID="lblfactorypin" runat="server"></asp:Label>
                                            </td>
                                        </tr>--%>
                                    </table>
                                </td>
                                <td style="vertical-align: top;" width="2%"></td>
                                <td class="qno" style="vertical-align: top; background-color: #d9d3d3;" width="47%">
                                    <table border="0" width="100%" class="quotation">
                                        <tr>
                                            <td class="" style="vertical-align: top; padding: 1px 5px; width: 38%">Proforma Invoice Date</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="vertical-align: top; padding: 1px 5px; width: 60%">
                                                <asp:Label ID="lblinvdate" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">&nbsp;Invoice Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblinvno" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">Quotation Number</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblqnumber" runat="server"></asp:Label></td>
                                        </tr>
                                      <%--  <tr>
                                            <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">Client Code</td>
                                            <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                            <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                <asp:Label ID="lblClientCode" runat="server"></asp:Label></td>
                                        </tr>--%>
                                        
                                        <asp:Panel ID="pnlTasGst" runat="server">
                                            <tr>
                                                <td class="" style="width: 38%; vertical-align: top; padding: 1px 5px;">Client GST Number</td>
                                                <td class="" style="width: 2%; vertical-align: top; padding: 1px 5px;">:</td>
                                                <td class="" style="width: 60%; vertical-align: top; padding: 1px 5px;">
                                                    <asp:Label ID="lblGstno" runat="server"></asp:Label></td>
                                            </tr>
                                        </asp:Panel>
                                        <tr>
                                            <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup1" runat="server"></asp:Label></td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup2" runat="server"></asp:Label></td>
                                            <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblplaceofsup3" runat="server"></asp:Label></td>

                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>

                        <%--<table border="0" width="100%" class="fees">

                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" style="">
                                    <table class="fees" style='border: 0' width="100%">

                                        ===============
                                    </table>
                                </td>
                            </tr>
                            <tr id="AMODETAILS" runat="server" Visible="false">
                                <td class="" style="">
                                   ===========
                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>--%>

                       <%-- =======================================--%>

                        <asp:Label ID="lblserviceamo" runat="server"></asp:Label>


                        <table border="0" width="100%">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="qno" width="100%">
                                    <%--<asp:DataList ID="DataList1" runat="server" Visible="false">
                                            <HeaderTemplate>
                                                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                                    <tr>
                                                        <td style="width: 8%; text-align: center; font: arial; font-family: Arial; font-size: small; font-weight: bold;"><%= psid%></td>
                                                        <td style="width: 47%; text-align: center; font: arial;">
                                                            <asp:Label runat="server" ID="no_of_sur" Text="Particulars" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                                        <td style="width: 10%; text-align: center; font: arial;">
                                                            <asp:Label runat="server" ID="Label4" Text="Qnty" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                                        <td style="width: 10%; text-align: center; font: arial;">
                                                            <asp:Label runat="server" ID="Label8" Text="Rate" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                                        <td style="width: 10%; text-align: center; font: arial; font-family: Arial; font-size: small; font-weight: bold;"><%= taxorvat%></td>
                                                        <td style="width: 15%; text-align: center; font: arial;">
                                                            <asp:Label runat="server" ID="Label1" Text="Amount" Font-Bold="true" Style="font-family: Arial; font-size: small; font-weight: bold;"></asp:Label></td>
                                                    </tr>
                                                </table>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                                    <tr>
                                                        <td style="width: 8%; border-top: none; text-align: center; font: arial;">
                                                            <asp:Label ID="qtation_survice" runat="server" Text='<%# Eval("Sl_no") %>' Style="font-family: Arial; font-size: small;"></asp:Label></td>
                                                        <td style="width: 47%; border-top: none; text-align: left; padding: 0px 2px 0px 15px; font: arial;">
                                                            <asp:Label ID="survice_month" runat="server" Text='<%# Eval("Product_name") %>' Style="font-family: Arial; font-size: small;"></asp:Label></td>
                                                        <td style="width: 10%; border-top: none; text-align: center; font: arial;">
                                                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("Quantity") %>' Style="font-family: Arial; font-size: small;"></asp:Label></td>
                                                        <td style="width: 10%; border-top: none; text-align: right; padding: 0px 20px 0px 2px; font: arial;">
                                                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("sail_rate") %>' Style="font-family: Arial; font-size: small; text-align: center;"></asp:Label>

                                                        </td>
                                                        <td style="width: 10%; border-top: none; text-align: center; font: arial;">
                                                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("Service_tax_rate") %>' Style="font-family: Arial; font-size: small;"></asp:Label>
                                                            %
                                                        </td>
                                                        <td style="width: 15%; border-top: none; text-align: right; padding: 0px 20px 0px 2px; font: arial;">
                                                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("Total_sail_rate2") %>' Style="font-family: Arial; font-size: small;"></asp:Label></td>
                                                    </tr>
                                                </table>
                                            </ItemTemplate>
                                        </asp:DataList>--%>
                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>

                       <%-- <table border="0" width="100%">
                            <tr>
                                <td style="width:100%">
                                    
                                </td>
                            </tr>
                        </table>--%>

                        

                        <%--<table border="0" width="100%";>
                            <tr id="AMODETAILS" runat="server" Visible="false">
                                <td>
                                <table>
                                     <tr>
                                <td width="55%">
                                    &nbsp;</td>
                                <td width="30%" style="text-align:right; padding:5px 20px 5px 0;" 
                                    class="style17">
                                    
                                    <span class="style10"><span class="style18">Sub Total</span></span></td>
                                <td width="15%" 
                                    style=" border-right:1px solid #bfbfbf; border-left:1px solid #bfbfbf; border-bottom:1px solid #bfbfbf;text-align:right; padding:0px 20px 0px 2px; text-align:right; padding:0px 20px 0px 2px;" 
                                    class="style7">
                                    <asp:Label ID="lblSubtotal" runat="server" CssClass="style17" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td width="55%">
                                    &nbsp;</td>
                                <td width="30%" style="" colspan="2">
                                <asp:DataList ID="DataList2" runat="server" Width="100%" >
                                    <ItemTemplate>
                                    <table width="100%"  border="0" cellpadding="0" cellspacing="0" class="table1">
                                      <tr>
                                            <td style="width:66.7%;border:none; text-align:right; font:arial; padding:5px 20px 5px 0;"><asp:Label ID="qtation_survice" runat="server" Text='<%# Eval("rete") %>' style="font-family: Arial; font-weight:bold;" CssClass="style17"></asp:Label></td>
                                            <td style="width:33.3%;border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;"><asp:Label ID="survice_month" runat="server" Text='<%# Eval("Vat_amount") %>' style="font-family: Arial; font-weight:bold;" CssClass="style17"></asp:Label></td>
                                     </tr>
                                   </table>
                                   </ItemTemplate>
                               </asp:DataList>
                               </td>
                            </tr>
                            <tr runat="server" id="discount_row">
                                <td width="55%">
                                    &nbsp;</td>
                                <td width="30%" style="text-align:right; padding:5px 20px 5px 0;" 
                                    class="style17">
                                    <span class="style10"><span class="style18">Discount</span></span></td>
                                <td width="15%" 
                                    style=" border-right:1px solid #bfbfbf; border-left:1px solid #bfbfbf; border-bottom:1px solid #bfbfbf;text-align:right; padding:0px 20px 0px 2px; text-align:right; padding:0px 20px 0px 2px;" 
                                    class="style7">
                                    <asp:Label ID="lbldiscount" runat="server" CssClass="style17" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td rowspan="2" bgcolor="#dbe5f1" style="padding:0px 0px 0px 5px;">
                                    <b><span class="style17">Amount (In Words):</span><span lang="en-us"> </span><asp:Label ID="lblword" 
                                        runat="server" Font-Bold="False" CssClass="style22" 
                                        style="font-family: Arial; font-size: small; font-weight:bold"></asp:Label>
                                    </b>
                                    </td>
                                <td style="text-align:right; padding:5px 20px 5px 0;">
                                    
                                    <span class="style10"><span class="style18">Round off(+-&nbsp;<asp:Label ID="lblstax" runat="server" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                    
                                    )</span></span></td>
                                <td style=" border-right:1px solid #bfbfbf; border-left:1px solid #bfbfbf; border-bottom:1px solid #bfbfbf;text-align:right; padding:0px 20px 0px 2px;" 
                                    class="style7">
                                    
                                    <span class="style10"><span class="style18"><asp:Label ID="lblstax0" runat="server" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                    
                                    </span>
                                    
                                    </span>
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align:right; padding:5px 20px 5px 0;" class="style17">
                                    Grand
                                    Total</td>
                                <td style=" border-right:1px solid #bfbfbf; border-left:1px solid #bfbfbf; border-bottom:1px solid #bfbfbf;text-align:right; padding:0px 20px 0px 2px;" 
                                    class="style7">
                                    <asp:Label ID="lblnetamount" runat="server" CssClass="style17" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                </td>
                            </tr>
                                </table>
                                </td>
                            </tr>
                        </table>--%>


                         <table class="PaymentPhase pagebrake" border="0" width="100%" id="BackData" runat="server">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" width="100%">

                                    <asp:Label ID="lblbackdata" runat="server"></asp:Label>

                                </td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>
                                    
                        

                                

                        <table border="0" width="100%">
                            <tr>
                                <td class="add" style="vertical-align: top" width="53%">
                                    <table border="0" width="100%" class="address">
                                        <tr>
                                            <td class="" style="background-color: #e31e24; color: white; text-align: center;">PAYMENT TERMS</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">All Advance Payments shall be made through Demand Draft/Pay Orders/At Par Payable Cheques/Telegraphic Transfer in favour of “Aminrup Technologies” at the Account Details Provided.</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">All Invoices shall be paid by the Client within Seven (7) Days of the Date of Invoice. In the event of Late Payment, the Company shall be entitled to charge Interest on any Outstanding Amounts at a rate of 1.5% per Month. GST at Current Rates is payable in addition to the Amount Quoted in accordance with the HSN/SAC Code.</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">The Company reserves the Right to Withdraw or Suspend all or any of its Services to the Client, till such time that the raised Invoice is settled.</td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="vertical-align: top;" width="2%"></td>
                                <td class="qno" style="vertical-align: top;" width="45%">
                                    <table border="0" width="100%" class="quotation">
                                        <tr>
                                            <td class="" style="background-color: #e31e24; color: white; text-align: center;">ACCOUNT DETAILS FOR BANK TRANSFER</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">BANK: ICICI BANK</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">BANK ACCOUNT NUMBER: 012805007421</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">IFSC CODE: ICIC0000001</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">BRANCH: Sakchi, Jamshedpur - 831001</td>
                                        </tr>
                                        <tr>
                                            <td class="gap" style="text-align: justify; border: 0;">&nbsp</td>
                                        </tr>
                                        <tr>
                                            <td class="" style="background-color: #e31e24; color: white; text-align: center;">COMPANY REGISTRATION DETAILS</td>
                                        </tr>
                                        <asp:Panel ID="PnlGstKvqa" runat="server">
                                            <tr>
                                                <td class="" style="text-align: justify; border: 1px solid #bfbfbf; padding: 2px 5px">GST NUMBER: 19AAEFI5315E1ZL</td>
                                            </tr>
                                            <%--<tr>
                                                <td class="" style="text-align: justify; border: 1px solid #bfbfbf; padding: 2px 5px">HSN CODE/SAC: 998214</td>
                                            </tr>--%>
                                        </asp:Panel>
                                        <asp:Panel ID="PnlTaxKvqa" runat="server">
                                            <tr>
                                                <td class="" style="text-align: justify; border: 1px solid #bfbfbf; padding: 2px 5px">SERVICE TAX NUMBER: AAEFI5315ESD001</td>
                                            </tr>
                                        </asp:Panel>
                                        <tr>
                                            <td class="" style="text-align: justify; border: 1px solid #bfbfbf;">PAN NUMBER: ADF56JNB2</td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>


                        <table border="0" width="100%">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                           <%-- <tr>
                                <td class="qno" width="100%">
                                    <table border="0" width="100%" class="">
                                        <tr>
                                            <td class="" style="background-color: #e31e24; color: white; text-align: center;">If you have any Question about this Invoice, Please Contact Ms Das at +91 9674897316 or info@aminruptechnologies.co.in</td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>--%>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                             <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                             <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                        </table>

                         <table border="0" width="100%" class="FORKVQAEAST">
                            <tr class="trheight">
                                <td class="" style="text-align: left; font-weight: bold;">FOR FLAME-EX</td>
                            </tr>
                            <%--<tr>
                                <td><img src="../WebImages/Stamp.jpg" width="100PX" /></td>
                            </tr>--%>
                            <tr>
                                <td><img src="../WebImages/flmx_authsign.png" width="150PX" /></td>
                            </tr>
                            <tr class="trheight">
                                <td class="" style="text-align: left; font-weight: bold;">Authorized Signatory</td>
                            </tr>
                        </table>

<%--                        <table border="0" width="100%" class="FORKVQAEAST">
                            <tr>
                                <td class="" style="text-align: left; font-weight: bold;">FOR FLAME-EX</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>


                            <tr>
                                <td class="" style="text-align: justify; font-weight: 100">
                                    <img src="../WebImages/Stamp.jpg" width="100PX" /></td>
                            </tr>

                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                            <tr>
                                <td class="" style="text-align: left; font-weight: bold;">Authorized Signatory</td>
                            </tr>
                        </table>--%>

                        <table border="0" width="100%" class="FORKVQAEAST">
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                           <%-- <tr>
                                <td class="" style="text-align: center; font-weight: bold; font-size: 14px; color: #c8152a; font-style: italic">_______________________ Thank You For Your Business_______________________</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>--%>
                        </table>
                    </td>
                </tr>
            </tbody>
        </table>
        <table id="footer" border="0" width="844px">
            <tr>
                <td style="height: auto; text-align: center; font-weight: bold; font-size: 14px; font-style: italic" width="100%">
                    <span style="padding-right:10px; color:#c8152a">------------------------------Thank You For Your Business------------------------------</span>
                    <asp:Image ID="Image22" runat="server"
                            Width="844px" Height="180px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrbtm.png" />
                </td>
            </tr>
        </table>

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" OnClientClick="document.getElementById('header').className ='header'; document.getElementById('footer').className ='footer'; window.print()" Text="Print Without Header & Footer" />
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" OnClientClick="window.print()" Text="Print With Header & Footer" />

    </form>
</body>
</html>
