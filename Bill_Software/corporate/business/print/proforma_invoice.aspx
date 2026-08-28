<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="proforma_invoice.aspx.cs" Inherits="Bill_Software.corporate.business.print.proforma_invoice" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/i2i_logo.png" />
        <style type="text/css">
            .style1
            {
                width: 100%;
                
            }
 
        
*{ padding:0; margin:0; border:none; list-style:none; text-decoration:none;}
        
        
*{
    border-style: none;
	border-color: inherit;
	border-width: medium;
	margin: 0px;
	padding: 0px;
	list-style: none;
	text-decoration:none;
}

            .style5
            {
                text-align: right;
                padding:0px 5px 0px 0px;
            }
            .style6
            {
                font-family: Arial, Helvetica, sans-serif;
                font-weight: bold;
                font-size: medium;
                color: #000037;
            }
            .tableOne{ margin:0; border:solid 1px #bfbfbf;}
        .tableOne td{font:normal 14px/20px Calibri; background:#dbe5f1; border:solid 1px #bfbfbf; padding:2px 0 2px 5px;}
        
        .tableTwo{ margin:0; border:solid 1px #bfbfbf;}
        .tableTwo td{font:normal 12px/16px Calibri; background:#dbe5f1; border:solid 1px #bfbfbf; padding:2px 5px 2px 5px;}
        
        .table_border td{ border:2px solid #bfbfbf;}
        .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #bfbfbf; width:50%;}
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #bfbfbf;  padding:2px 0 2px 20px;}
            .style7
            {
                font-family: "Arial";
                font-size: small;
                font-weight: bold;
            }
            .style8
            {
                text-align: center;
                color: #FFFFFF;
                font-weight: bold;
                font-family: Calibri;
            }
            .style9
            {
                font-family: Arial, Helvetica, sans-serif;
                font-weight: bold;
                font-size:20px;
                color: #e36c0a;
            }
            .style10
            {
                font-family: "Arial";
                font-size: small;
                font-weight: bold;
                
            }
            .style11
            {
                font-weight: bold;
            }
            .style12
            {
                font-family: "Century Gothic";
                font-size: medium;
                font-weight: bold;
            }
            .style13
            {
                color: #1c3564;
            }
            .style14
            {
                color: #1c3564;
            }
            .style15
            {
                color: #e36c0a;
            }
            .style16
            {
                font-family: "Arial";
                font-size: medium;
                font-weight: bold;
                color: #1c3564;
            }
            .style17
            {
                font-family: "Arial";
                font-size: small;
                font-weight: bold;
            }
            .style18
            {
                font-family: "Arial";
                font-size: small;
                font-weight: bold;
            }
            .sssss
            {
            	position:fixed;
            	 bottom:0;
            }
    @media print {
  thead { display: table-header-group; }
  tfoot { display: table-footer-group;}
  .header, .hide { visibility: hidden;height:100px; }
  
  .header1, .show {}
  .Foter, .hide { visibility: hidden;height:60px  }
  .Foter1, .show { position:fixed; bottom:0;  }
   }
  @media screen {
  thead { display: block; }
  tfoot { display: block; }
  }
    @media print1
    {
	#non-printable { display: none; }
	#printable { display: block; }
	
    }
            </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="printable" style="width:844px;">
    
        <table cellpadding="0" cellspacing="0" class="style1">
        
            <tr>
                <td id="Hederprint" runat="server" colspan="2">
                    <asp:Image ID="Image21" runat="server" 
                         Width="844px" Height="106px" ImageUrl="~/corporate/business/WebImages/i2i_lh.jpg" /></td>
            </tr>
           
          
            <tr>
                <td class="style5" colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="style5" colspan="2">
                    <span lang="en-us"><span class="style6">&nbsp;</span><span class="style9">PRO FORMA INVOICE</span></span></td>
            </tr>
            <tr>
                <td colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td colspan="2">
                    <table width="100%" cellpadding="0" cellspacing="0" class="tableOne">
                        <tr>
                            <td style="width:50%">
                                Quotation No:<span lang="en-us"> </span><asp:Label ID="lblQno" runat="server"></asp:Label>
                            </td>
                            <td style="text-align:right; width:50%; padding:0px 5px 0px 0px;">
                                <span lang="en-us">&nbsp;</span>Invoice No:<span lang="en-us"> </span>
                                <asp:Label ID="lblInvoiceNo" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="width:50%">
                                <asp:Label ID="lbltaxstring" runat="server"></asp:Label>
                                <asp:Label ID="lbltaxno" runat="server"></asp:Label>
                                </td>
                            <td style="text-align:right; width:50%; padding:0px 5px 0px 0px;">
                                Pan No:
                                AAEF15315E</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    &nbsp;</td>
            </tr>
            <tr>
                <td style="width:50%; padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <span lang="en-us" class="style11">To</span><span lang="en-us">,</span>&nbsp;</td>
                <td style="text-align:right; width:50%; padding:0px 5px 0px 0px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lbldate" 
                        runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
            
            <%--<tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblrepresentativeName" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblrepresentativedesignation" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>--%>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblcompanyName" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lbladdress1" runat="server" CssClass="style11"></asp:Label>
                    <br />
                    <asp:Label ID="lbladdress2" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblcity" runat="server" CssClass="style11"></asp:Label>
                    -<asp:Label ID="lblPin" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           
            
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblstate" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
           <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblClientVat" runat="server" CssClass="style11"></asp:Label>
                </td>
            </tr>
            
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    &nbsp;</td>
            </tr>
          
            <tr>
                <td colspan="2" style="border:1px solid #bfbfbf; border-bottom:none; border-right:none">
                    <asp:Label ID="lblProductList" runat="server" Visible="false"></asp:Label>
                    <asp:DataList ID="DataList1" runat="server" Visible="false" Width="100%" >
                     <HeaderTemplate>
                        <table width="100%" border="0" cellpadding="0" cellspacing="0" class="table1">
                                <tr>
                                        <td style="width:8%;text-align:center; font:arial;"><asp:Label runat="server" ID="tupe_ofcirtificate" Text="S.No." Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                        <td style="width:47%;text-align:center; font:arial;"><asp:Label runat="server" ID="no_of_sur" Text="Particulars" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                        <td style="width:10%;text-align:center; font:arial;"><asp:Label runat="server" ID="Label4" Text="Qnty" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                        <td style="width:10%;text-align:center; font:arial;"><asp:Label runat="server" ID="Label8" Text="Rate" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                    <td style="width:10%;text-align:center; font:arial;"><asp:Label runat="server" ID="Label3" Text="GST" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                    <%--<td style="width:10%;text-align:center; font:arial;"><asp:Label runat="server" ID="Label6" Text="Rate With Tax" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>--%>
                                        <td style="width:15%;text-align:center; font:arial;"><asp:Label runat="server" ID="Label1" Text="Amount" Font-Bold="true" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label></td>
                                </tr>
                        </table>
                </HeaderTemplate>
                        <ItemTemplate>
                        <table width="100%"  border="0" cellpadding="0" cellspacing="0" class="table1">
                            <tr>
                                <td style="width:8%;border-top:none; text-align:center; font:arial;"><asp:Label ID="qtation_survice" runat="server" Text='<%# Eval("Sl_no") %>' style="font-family: Arial; font-size: small;"></asp:Label></td>
                                <td style="width:47%;border-top:none; text-align:left; padding:0px 2px 0px 15px; font:arial;"><asp:Label ID="survice_month" runat="server" Text='<%# Eval("Product_name") %>' style="font-family: Arial; font-size: small;"></asp:Label></td>
                                <td style="width:10%;border-top:none; text-align:center; font:arial;"><asp:Label ID="Label5" runat="server" Text='<%# Eval("Quantity") %>' style="font-family: Arial; font-size: small;"></asp:Label></td>
                                <td style="width:10%;border-top:none;  text-align:right; padding:0px 20px 0px 2px; font:arial;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("sail_rate") %>' style="font-family: Arial; font-size: small; text-align:center;"></asp:Label>
            
                                </td>
                                <td style="width:10%;border-top:none; text-align:center; font:arial;">
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("Service_tax_rate") %>' style="font-family: Arial; font-size: small;"></asp:Label>
                                       %
                                </td>
                                <%--<td style="width:10%;border-top:none; text-align:center; font:arial;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Total_sail_rate") %>' style="font-family: Arial; font-size: small;"></asp:Label>
            
                                </td>--%>
                                <td style="width:15%;border-top:none; text-align:right; padding:0px 20px 0px 2px; font:arial;"><asp:Label ID="Label2" runat="server" Text='<%# Eval("Total_sail_rate2") %>' style="font-family: Arial; font-size: small;"></asp:Label></td>
                            </tr>
                            </table>
                        </ItemTemplate>
                    </asp:DataList>
                   
                   
                    </td>
            </tr>
            <tr id="AMODETAILS" runat="server" Visible="false">
                <td colspan="2">
                <table cellpadding="0" cellspacing="0" width="100%";>
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
                                <td width="30%" style="width: 45%;" 
                                     colspan="2">
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
                                    &nbsp;Grand
                                    Total</td>
                                <td style=" border-right:1px solid #bfbfbf; border-left:1px solid #bfbfbf; border-bottom:1px solid #bfbfbf;text-align:right; padding:0px 20px 0px 2px;" 
                                    class="style7">
                                    <asp:Label ID="lblnetamount" runat="server" CssClass="style17" style="font-family: Arial; font-size: small; font-weight:bold;"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </td>
            </tr>
            <tr>
                <td colspan="2">
                &nbsp;
                    </td>
            </tr>
            <tr>
                <%--<td colspan="2" bgcolor="#002060" class="style8">
                    Payment <span lang="en-us">T</span>erms</td>--%>
                <td colspan="2">&nbsp;</td>
            </tr>
            <tr>
                <td colspan="2">
                <%--<table width="100%" cellpadding="0" cellspacing="0" class="tableTwo">
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                Payment for service shall be payable in advance/within 15 days issue of Invoice. 
                                by  
                                <asp:Label ID="lblcompanyshortname1" runat="server"></asp:Label>
                                &amp; is Subject to Kolkata Jurisdiction</td>
                            <td style="width:50%; text-align:justify;">
                                <span lang="en-us"><asp:Label ID="lblcompanyshortname2" runat="server"></asp:Label>
                                do not accept payment made in Cash. Please pay by 
                                Cheque/DD in favour of </span>
                                <asp:Label ID="lblcompantfullname2" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                In case of non payment by the customer within the stipulated period,  
                                <asp:Label ID="lblcompanyshortname3" runat="server"></asp:Label>
                                may 
                                stop the service and suspend the certificate after giving a notice to the 
                                customer for a period of 15 days.</td>
                            <td style="width:50%; text-align:justify;">
                                All outstation cheque will be given credit only after being credited to 
                                <asp:Label ID="lblcompantfullname1" runat="server"></asp:Label>
                                bank account. Bank 
                                charges, if any, shall be borne by the Customer.A </td>
                        </tr>
                        <tr>
                            <td style="width:50%; text-align:justify;">
                                All disputes subject to Kolkata High Court Jurisdiction only.</td>
                            <td style="width:50%; text-align:justify;">
                                The Application Fee for the Certification is Non Refundable.</td>
                        </tr>
                    </table>--%>
                    </td>
            </tr>
            <tr>
                <td colspan="2" bgcolor="#002060" class="style8">
                    Account Details for Bank Transfer</td>
            </tr>
            <tr>
                <td colspan="2">
                <table width="100%" cellpadding="0" cellspacing="0" class="tableTwo">
                        <tr>
                            <td style="width:50%">
                                A/C Name:
                                Aminrup Technologies</td>
                            <td style="width:50%;">
                                <span lang="en-us">Bank: </span>ICICI<span lang="en-us"> BANK, A/C No-</span>012805007421</td>
                        </tr>
                        <tr>
                            <td style="width:50%">
                                Branch:
                                Sakchi, Jamshedpur-831001</td>
                            <td style="width:50%;">
                                IFSC Code: ICIC0000001</td>
                        </tr>
                        
                    </table>
                    </td>
            </tr>
            <tr>
                <td colspan="2">
                    &nbsp;</td>
            </tr>
            
           
          
            <tr>
                <td colspan="2" class="style12">
                    &nbsp;</td>
            </tr>

            <tr>
                <td colspan="2" class="style12">
                    &nbsp;</td>
            </tr>

            <tr>
                <td colspan="2" class="style12">
                    &nbsp;</td>
            </tr>
            
            <tr>
                <td colspan="2" style="text-align:right; padding:4px 30px 4px 2px;">
                    <span lang="en-us">&nbsp;<asp:Image ID="Image4" runat="server" 
                        Height="73px" Width="119px" ImageUrl="~/corporate/business/WebImages/Stamp.jpg" />
                        </span>&nbsp;</td>
            </tr>
          
            <tr>
                <td colspan="2" class="style16" style="text-align:right; padding:4px 7px 4px 2px;">
                    Authorized Signatory</td>
            </tr>
            
            <tr>
                <td colspan="2">
                    <asp:Button ID="Button1" runat="server" class="hide" onclick="Button1_Click" 
                        OnClientClick="document.getElementById('Hederprint').className ='header' ;document.getElementById('footerrprint').className ='Foter'; window.print()" 
                        Text="Print Without Header" BackColor="#005886" BorderStyle="Outset" 
                        ForeColor="White" />
                        <span lang="en-us">&nbsp;<asp:Button ID="Button2" runat="server" 
                        class="hide" onclick="Button2_Click" 
                        OnClientClick="document.getElementById('Hederprint').className ='header1' ;document.getElementById('footerrprint').className ='Foter1'; window.print()" 
                        Text="Print With Header" BackColor="#005886" BorderStyle="Outset" 
                        ForeColor="White" />
                        </span></td>
            </tr>
            
         <tr>
                <td id="footerrprint" runat="server" colspan="2">
                
                <asp:Image ID="Image22" runat="server" 
                         Width="844px" Height="100px" ImageUrl="~/corporate/business/WebImages/i2i_lh_b.jpg" />
                        
                    </td>
            </tr>
            
           
        </table>
   
    </div>
    </form>
</body>
</html>
