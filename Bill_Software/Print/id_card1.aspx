<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="id_card1.aspx.cs" Inherits="Bill_Software.Print.id_card1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <title></title>
     <style type="text/css">

    *{ margin:0px; padding:0px; list-style:none; text-decoration:none; }
 body{  font:normal 12px/18px Arial, Helvetica, sans-serif; color:#000;}
.clear{ line-height:0px; font-size:0px; clear:both; }
.main_div{width:208px; height:337px;  margin:0 auto; }
.tablestyle { border:1px solid #000; border-collapse:collapse;}
.tablestyle  td {font:normal 12px/24px Verdana, Geneva, sans-serif;  border:1px solid #000; padding:0  20px;}
    .style1
    {
        width: 339px;
    }
        .auto-style3 {
            width: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="main_div">
        <table class="auto-style3">
            <tr>
                <td style="text-align:center;">
                    <div style="border:solid 1px #000; width:208px; height:337px;">
                       
                        <table cellpadding="0" cellspacing="0" class="auto-style3">
                            <tr>
                                <td style="height:12px; width:12px;">&nbsp;</td>
                                <td style="height:12px;">&nbsp;</td>
                                <td style="height:12px; width:12px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td style="width:12px;height:301px;">&nbsp;</td>
                                <td>
                                    <table cellpadding="0" cellspacing="0" class="auto-style3">
                                        <tr>
                                            <td valign="top">
                                                <asp:Image ID="Image1" runat="server" Height="36px" Width="65px" />
                                            
                                                &nbsp;<asp:Label ID="lblcompanyname" runat="server" style="font-weight: 700"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lbladdress" runat="server"></asp:Label>
                                                <br />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Image ID="Image2" runat="server" Height="142px" Width="100px" />
                                                <br />
                                                &nbsp;
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:left;"><strong>Name :-</strong><asp:Label ID="lblname" runat="server" style="font-weight: 700"></asp:Label>
                                                <br />
                                                <strong>Depertment :-<asp:Label ID="lbldepertment" runat="server" style="font-weight: 700"></asp:Label>
                                                </strong>
                                                <br />
                                                <strong>Unit :-</strong><asp:Label ID="lbldesignation" runat="server" style="font-weight: 700"></asp:Label>
                                                <br />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="width:12px; height:301px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td style="height:12px; width:12px;">&nbsp;</td>
                                <td style="height:12px;">&nbsp;</td>
                                <td style="height:12px; width:12px;">&nbsp;</td>
                            </tr>
                        </table>
                       
                    </div>

                </td>
                
            </tr>
    
    </div>
    </form>
</body>
</html>
