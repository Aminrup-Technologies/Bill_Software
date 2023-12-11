<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Patty_cash_expencess_voutcher.aspx.cs" Inherits="Bill_Software.corporate.business.print.Patty_cash_expencess_voutcher" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>i2i Inc</title>
    <link rel="shortcut icon" href="../WebImages/KCGC.png" />
    <style type="text/css">

    *{ margin:0px; padding:0px; list-style:none; text-decoration:none; }
 body{  font:normal 12px/18px Arial, Helvetica, sans-serif; color:#000;}
.clear{ line-height:0px; font-size:0px; clear:both; }
.main_div{width:844px; height:590px;  margin:0 auto; }
.tablestyle { border:1px solid #000; border-collapse:collapse;}
.tablestyle  td {font:normal 12px/24px Verdana, Geneva, sans-serif;  border:1px solid #000; padding:0  20px;}
    .style1
    {
        width: 339px;
    }
</style>
</head>

<body>
    <form id="form1" runat="server">
    <div>
    <div class="main_div">


<table width="100%" border="0" cellspacing="0" cellpadding="0" style="margin:0 0 0 0;">
  <tr>
    <td width="100%">
        <asp:Image ID="Image21" runat="server" 
                         Width="844px" Height="150px" ImageUrl="~/corporate/business/WebImages/i2i_lh.jpg" />
      <%--<img src="../images/logo1.jpg" width="826" height="129" />--%></td>
    </tr>
</table>
<br />


  <table border="0" cellspacing="0" cellpadding="0" align="left"  class="tablestyle" width="844">
    <tr>
      <td height="50" colspan="4" align="center" ><h3>PAYMENTS  MADE AGAINST PETTY    CASH</h3></td>
    </tr>
    <tr>
      <td colspan="2" align="center"  style="background:#d8d8d8;"><strong>PARTICULARS</strong></td>
      <td width="144" align="center"  bgcolor="#d8d8d8"><strong>AMOUNT<br />
        (In Rupees)</strong></td>
      <td width="64" rowspan="10" align="center" valign="bottom" style="padding:0;">
      <img src="../WebImages/recieved.jpg" width="23" height="95" /></td>
    </tr>
    <tr>
      <td width="200"  valign="bottom"><strong>Payment Category</strong></td>
      <td  valign="bottom" class="style1">
          <asp:Label ID="lblcash_catagory" runat="server"></asp:Label>
                        </td>
      <td width="144" rowspan="8" align="right" valign="top">
          <asp:Label ID="lblamount" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200" valign="bottom"><strong>Payment Date</strong></td>
      <td  valign="bottom" class="style1">
          <asp:Label ID="lblpayment_date" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200" valign="bottom"><strong>Payment Made To</strong></td>
      <td valign="bottom" class="style1">
          <asp:Label ID="lblpayment_made_to" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200"  valign="bottom"><strong>Expense Head</strong></td>
      <td  valign="bottom" class="style1">
          <asp:Label ID="lblexpences_head" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200" ><strong>Narration</strong></td>
      <td valign="bottom" class="style1">
          <asp:Label ID="lblnaration_head" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200" valign="bottom"><strong>Payment Type</strong></td>
      <td  valign="bottom" class="style1">
          <asp:Label ID="lblpayment_type" runat="server"></asp:Label>
                        </td>
    </tr>
    <tr>
      <td width="200"  valign="bottom"><strong>Instrument No</strong></td>
      <td  valign="bottom" class="style1">
          &nbsp;</td>
    </tr>
    <tr>
      <td width="200"  valign="bottom"><strong>Instrument Date </strong></td>
      <td  valign="bottom" class="style1">
          &nbsp;</td>
    </tr>
    <tr>
      <td width="200"  valign="top"><strong>Amount  (In    Words)</strong></td>
      <td  valign="top" class="style1"><asp:Label ID="lblword" runat="server"></asp:Label>
          </td>
      <td width="144"  valign="top" align="right">
          <asp:Label ID="lblamount1" runat="server"></asp:Label>
                        </td>
    </tr>
  </table>
  <br class="clear" />
<br />
 


</div><!--end of main div-->
    </div>
    </form>
</body>
</html>
