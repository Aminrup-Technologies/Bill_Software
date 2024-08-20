<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Requisition.aspx.cs" Inherits="Bill_Software.corporate.business.print.Requisition" %>

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
                position: fixed;
                bottom: 0;
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

        .auto-style1 {
            font-family: "Century Gothic";
            font-size: medium;
            font-weight: bold;
            color: #1c3564;
            height: 19px;
        }
        .tdsty {
            font-family:"Century Gothic";
            /*font-size: 12px/14px;*/
            color: #1c3564;
            padding:2px 1px 2px 1px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="printable" style="width: 844px;">

            <table cellpadding="0" cellspacing="0" class="style1">

               <%-- <tr>
                    <td id="Hederprint" runat="server">
                        <asp:Image ID="Image21" runat="server"
                            Width="844px" Height="106px" ImageUrl="~/corporate/business/WebImages/i2i_lh.jpg" /></td>
                </tr>--%>


                <tr>
                    <td class="style5">&nbsp;</td>
                </tr>

                <tr>
                    <td style="border:0px;">
                        <table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class="tdsty"  style="text-align:center; width:30%">
                                    <img src="../../WebProperty/images/i2i_logo.png" /></td>
                                <td class="tdsty" style="text-align:left; font-weight:bold; font-size:13px; padding-left:6px;">PURCHASE REQUISITION</td>
                                <td class="tdsty" style="text-align:left; font-size:13px; padding-left:6px">DATE: <asp:Label ID="rDate" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                    </td>
                </tr>
                 <tr>
                    <td class="style5">&nbsp;</td>
                </tr>
                <tr>
                    <td style="border:0px;">
                        <table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class="tdsty"  style="text-align:left; width:30%; font-weight:bold; font-size:12px; padding-left:20px">COMPANY NAME</td>
                                <td class="tdsty" style="text-align:left; font-size:14px;">
                                    <asp:Label ID="lblCompanyname" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td class="style5">&nbsp;</td>
                </tr>
                <tr>
                    <td style="border:0px;">
                        <table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class="tdsty"  style="text-align:left; width:30%; font-weight:bold; font-size:12px;padding-left:20px">COMPANY ADDRESS</td>
                                <td class="tdsty" style="text-align:left; font-size:14px;">
                                    <asp:Label ID="lblAddress" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                    </td>
                </tr>

                <tr>
                    <td class="style5">&nbsp;</td>
                </tr>
                <tr>
                    <td>
                        <table cellpadding="0" cellspacing="0" class="style1">

                            <%--For cgst/sgst--%>
                      <tr><td colspan="10">
                    <%--<table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>
                                <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>
                                <td class='tdsty'  style='text-align:center; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>
                                <td class='tdsty'  style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>

                                <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>CGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:4%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:10%; font-size:13px;font-weight:bold'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>SGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:4%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:10%; font-size:13px;font-weight:bold'>AMOUNT</td>                                           
                                        </tr>
                                    </table>
                                </td>
                               

                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>
                            </tr>
				  </table>--%>

                    <%--<table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none;border-top:none;'>1</td>
                                <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>998311</td>
                                <td class='tdsty'  style='text-align:center; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>100 MM PIPE WITH CLOUR (BANSAL)</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>18</td>
                                <td class='tdsty'  style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>1250.00</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>12 %</td>


                                <td class='tdsty'  style='text-align:center; width:4%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>6 %</td>
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>1250.00</td>
                                <td class='tdsty'  style='text-align:center; width:4%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>6 %</td>
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>1250.00</td>


                               

                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>125450.00</td>
                            </tr>
				  </table>--%>
                    <%--For Igst--%>
                    <%--<table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>
                                <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>
                                <td class='tdsty'  style='text-align:center; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>
                                <td class='tdsty'  style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>

                                <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>IGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:4%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:10%; font-size:13px;font-weight:bold'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>
                            </tr>
				  </table>--%>

                     <%--<table cellpadding="0" cellspacing="0" class="style1">
                            <tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none;border-top:none;'>1</td>
                                <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>998311</td>
                                <td class='tdsty'  style='text-align:center; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>100 MM PIPE WITH CLOUR (BANSAL)</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>18</td>
                                <td class='tdsty'  style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>1250.00</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>12 %</td>


                                <td class='tdsty'  style='text-align:center; width:4%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>6 %</td>
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>1250.00</td>


                               

                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>125450.00</td>
                            </tr>
				  </table>--%>

                          <asp:Label ID="lblProductlist" runat="server"></asp:Label>

                          </td>
                            </tr>


                            <%--<tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>
                                <td class='tdsty' style='text-align:center; width:20%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>DESCRIPTION</td>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SIZE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>AMOUNT</td>

                                <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='3'>CGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;font-weight:bold'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='3'>SGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;font-weight:bold'>AMOUNT</td>                                           
                                        </tr>
                                    </table>
                                </td>
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='3'>IGST</td>
                                        </tr>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;font-weight:bold'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>

                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>TOTAL AMOUNT</td>
                            </tr>--%>

                            <%--<asp:Label ID="lblProductlist" runat="server"></asp:Label>--%>
                            <%--<tr>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:12px; border:1px solid #bfbfbf;'>SL NO</td>
                                <td class='tdsty' style='text-align:center; width:20%; font-size:12px;border:1px solid #bfbfbf;'>DESCRIPTION</td>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:12px;border:1px solid #bfbfbf;'>SIZE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:12px;border:1px solid #bfbfbf;'>QNTY</td>
                                <td class='tdsty'  style='text-align:center; width:5%; font-size:12px;border:1px solid #bfbfbf;'>RATE</td>
                                <td class='tdsty' style='text-align:center; width:5%; font-size:12px;border:1px solid #bfbfbf;'>AMOUNT</td>

                                 <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border:1px solid #bfbfbf;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                 <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border:1px solid #bfbfbf;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                 <td class='tdsty' style='text-align:center; width:15%; font-size:13px;border:1px solid #bfbfbf;'>
                                    <table cellpadding='0' cellspacing='0' class='style1'>
                                        <tr>
                                            <td class='tdsty' style='text-align:center; width:7%; font-size:13px;border-right:1px solid #bfbfbf;'>RATE</td>
                                            <td class='tdsty' style='text-align:center; width:8%; font-size:13px;'>AMOUNT</td>
                                            
                                        </tr>
                                    </table>
                                </td>
                                        
                                <td class='tdsty' style='text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>TOTAL AMOUNT</td>
                            </tr>--%>


                           <%-- <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    TOTAL AMOUNT BEFORE SERVICE TAX:
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lblbeftax" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border-bottom:0px;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    CGST:</td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lblcgstTotal" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border-bottom:0px;" colspan="6">
                                    In Word:<asp:Label ID="lblWord" runat="server"></asp:Label>
                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    SGST:
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lblsgstTotal" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:0px;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    IGST:
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lbligstTotal" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:0px;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    TAX AMOUNT: GST
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lblGstTotal" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="3">
                                    TOTAL AMOUNT AFTER TAX:
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;">
                                    <asp:Label ID="lblTotalAmoGst" runat="server"></asp:Label>
                                </td>
                            </tr>--%>



                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:0px;" colspan="6">

                                </td>
                                <td class="tdsty" style="text-align:right; width:10%; font-size:13px;border:0px;" colspan="3">
                                   
                                </td>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:0px;">

                                </td>
                            </tr>

                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                    CHEQUE&nbsp; NO:</td>
                                <td class="tdsty" style="text-align:left; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="4">
                                    <asp:Label ID="lblCheckNo" runat="server"></asp:Label>
                                </td>
                                
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                    BANK NAME:</td>
                                <td class="tdsty" style="text-align:left; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="4">
                                    <asp:Label ID="lblBankName" runat="server"></asp:Label>
                                </td>
                                
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                    IFS CODE:</td>
                                <td class="tdsty" style="text-align:left; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="4">
                                    <asp:Label ID="lblIfcCode" runat="server"></asp:Label>
                                </td>
                                
                            </tr>
                            <tr>
                                <td class="tdsty" style="text-align:center; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="6">

                                    DATE:</td>
                                <td class="tdsty" style="text-align:left; width:10%; font-size:13px;border:1px solid #bfbfbf;" colspan="4">
                                    <asp:Label ID="lblIssueDate" runat="server"></asp:Label>
                                </td>
                                
                            </tr>
                           
                        </table>
                    </td>
                </tr>
               
                <%--<tr>
                    <td class="style5">&nbsp;</td>
                </tr>--%>

                <tr>
                    <td style="text-align: right; padding: 4px 30px 4px 2px;">
                        <span lang="en-us">&nbsp;<asp:Image ID="Image4" runat="server"
                            Height="73px" Width="119px" ImageUrl="~/corporate/business/WebImages/Stamp.jpg" />
                        </span>&nbsp;</td>
                </tr>

                <tr>
                    <td class="auto-style1" style="text-align: right; padding: 4px 7px 4px 2px;">Authorized Signatory</td>
                </tr>

                <%--<tr>
                    <td>
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
                </tr>--%>

                <tr>
                    <td id="footerrprint" runat="server">

                        <asp:Image ID="Image22" runat="server"
                            Width="844px" Height="100px" ImageUrl="~/corporate/business/WebImages/i2i_lh_b.jpg" />

                    </td>
                </tr>


            </table>

        </div>
    </form>
</body>
</html>
