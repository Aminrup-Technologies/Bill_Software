<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RequisitionNew.aspx.cs" Inherits="Bill_Software.corporate.business.print.RequisitionNew" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Aminrup Technologies.</title>
    <link rel="shortcut icon" href="corporate/business/WebImages/i2i_logo.png" />
<style type="text/css">
            .style1{width: 100%;}
            
 
        
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

               .jkhj {
                background: url("http://i2isoft.aminruptechnologies.co.in/corporate/business/WebImages/I2Ilogo10.png");
                /*width: 100%;
                height:100%;
                background-size: 100%;*/
                /*background-size: contain;*/
                background-size: 100% 100%;
            }
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
            .auto-style1 {
                font-weight: bold;
                font-size: 13px;
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
                            Width="844px" Height="140px" ImageUrl="~/corporate/business/WebImages/flame-ex_hdrtop.png" />
                </td>
                
            </tr>
            <tr>
                <td>

                </td>
                <td style="text-decoration:underline; font-weight:bold; font-size:16px;">
                    PURCHASE REQUISITION
                </td>
            </tr>
            <tr>
                <td style="width:50%; padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    &nbsp;</td>
                <td style="text-align:right; width:50%; padding:0px 5px 0px 0px; font:normal 14px/16px Calibri;">
                    Date: <asp:Label ID="lbldate" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="style11" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    Company Name: <asp:Label ID="lblcompanyName" runat="server"></asp:Label>
                </td>
            </tr>
           
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    Company Address: <asp:Label ID="lbladdress1" runat="server"></asp:Label>
                    <br />
                </td>
            </tr>

             <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    <asp:Label ID="lblVendor" runat="server"></asp:Label>
                    <br />
                </td>
            </tr>
           
           
            
            
            
            <tr>
                <td colspan="2" style="padding:0px 0px 0px 5px; font:normal 14px/16px Calibri;">
                    &nbsp;</td>
            </tr>
          
            <tr>
                <td colspan="2" style="">
                    <asp:Label ID="lblProductList" runat="server"></asp:Label>
                   
                        <%-- <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>
                        
                        <td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:37%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Description</td>
                        <td class='auto-style1' style='border-left: 1px solid #bfbfbf; border-top: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align:center; padding:2px 0px 2px 0px; width:5%; border-right-style: none; border-right-color: inherit; border-right-width: medium;'>Size</td>
                        <td class='auto-style1'  style='border-left: 1px solid #bfbfbf; border-top: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align:center; padding:2px 0px 2px 0px; width:10%; border-right-style: none; border-right-color: inherit; border-right-width: medium;'>Qnty</td>
                        <td class='auto-style1' style='border-left: 1px solid #bfbfbf; border-top: 1px solid #bfbfbf; border-bottom: 1px solid #bfbfbf; text-align:center; padding:2px 0px 2px 0px; width:5%; border-right-style: none; border-right-color: inherit; border-right-width: medium;'>Rate</td>
                        

                        
                        <td class='auto-style1' style='text-align:center; padding:2px 0px 2px 0px; width:10%; border:1px solid #bfbfbf;'>Amount</td>
                        </tr>
                        </table>
						
						
						<table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>
                        
                        <td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:37%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>
                        <td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>
                        <td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>
                        <td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>

                         <td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px;; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'></td>
                        </tr>
                        </table>--%>
						

                       <%-- <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Paid By Cash:</td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>Gross:</td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>

                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' colspan='6'></td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>Vat:</td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>

                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' colspan='6'>&nbsp;</td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>Total:</td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>--%>

                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' colspan='6'>Cheque No: <asp:Label ID="lblCheckNo" runat="server"></asp:Label></td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>&nbsp;</td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:0px;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>

                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;'colspan='6'>Issue Date: <asp:Label ID="lblIssueDate" runat="server"></asp:Label></td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'></td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:0px;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>

                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;'colspan='6'>Bank: <asp:Label ID="lblBankName" runat="server"></asp:Label></td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'></td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:0px;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>

                       
                        <table cellpadding='0' cellspacing='0' class='style1'>
                        <tr>
                        <td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;'colspan='6'>IFS Code: <asp:Label ID="lblIFSCode" runat="server"></asp:Label></td>
                        <td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'></td>
                        <td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:0px;border-top:none;font-weight:bold'></td>
                        </tr>
                        </table>
                       

                       
						
						
                    </td>
            </tr>
            <tr id="AMODETAILS" runat="server" Visible="false">
                <td colspan="2">
                    &nbsp;</td>
            </tr>
          
         
            <tr>
                <td colspan="2" style="text-align:right; padding:4px 30px 4px 2px;">
                    <span lang="en-us">&nbsp;</span>&nbsp;</td>
            </tr>
          
            <tr>
                <td colspan="2" class="style16" style="text-align:right; padding:4px 7px 4px 2px;">
                    HR & ADMIN</td>
            </tr>
            
           <%-- <tr>
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
            </tr>--%>

           <%-- <tr>
                <td colspan="2">
                    &nbsp;</td>
            </tr>--%>
            
                    
        </table>
   
    </div>
    </form>
</body>
</html>
