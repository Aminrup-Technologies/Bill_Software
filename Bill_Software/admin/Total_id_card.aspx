<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Total_id_card.aspx.cs" Inherits="Bill_Software.admin.Total_id_card" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
   <div>
    
        <asp:DataList ID="DataList1" runat="server" RepeatColumns="5" RepeatDirection="Horizontal">
            <ItemTemplate>
                        
        <table style="width:192px; height:279px; border:solid 1px #000; border-collapse:collapse;" cellpadding="0" cellspacing="0">
            <tr>
                <td style="text-align:center; border-collapse:collapse;">
                    <div>
                       
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="height:10px; width:10px;">
                                    &nbsp;</td>
                                <td style="height:10px; width:172px;">&nbsp;</td>
                                <td style="height:10px; width:10px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td style="width:10px;height:259px;">&nbsp;</td>
                                <td style="height:259px; width:172px;">
                                   <table style="text-align:center;height:100%; width:100%">
                                       <tr>
                                           <td valign="top" style="text-align:left;">
                                               <asp:Image ID="Image1" runat="server" Height="36px" Width="65px" ImageUrl='<%# "Company.ashx?ComID=" + Eval("ComID")%>'/>
                                               </td>
                                           <td style="font:normal 10px/14px arial; font-weight:bold;">
                                               <asp:Label ID="lblcompanyname" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                               
                                           </td>
                                       </tr>
                                       <tr>
                                           <td colspan="2" style="font:normal 9px/12px arial;">
                                               <asp:Label ID="Label1" runat="server" Text='<%# Eval("Address") %>'></asp:Label>
                                               <br />
                                           </td>
                                       </tr>
                                       <tr>
                                           <td colspan="2" style="font:normal 10px/16px arial; font-weight:bold;">
                                               <asp:Image ID="Image2" runat="server" Height="142px" Width="100px" ImageUrl='<%# "personal_image.ashx?ID=" + Eval("ID")%>'/>
                                               <br />
                                               EMPID:-<asp:Label ID="Label2" runat="server"  Text='<%# Eval("Emp_ID") %>'></asp:Label>
                                           </td>
                                       </tr>
                                       <tr>
                                           <td style="text-align:left; font:normal 10px/16px arial; font-weight:bold;" colspan="2">
                                               Name :-<asp:Label ID="lblname" runat="server"  Text='<%# Eval("EmployeeName") %>'></asp:Label>
                                               
                                               
                                               <br />
                                               Depertment :-<asp:Label ID="lbldepertment" runat="server"  Text='<%# Eval("Depertment") %>'></asp:Label>
                                                
                                               <br />
                                               Unit :-<asp:Label ID="lbldesignation" runat="server" Text='<%# Eval("Designation") %>'></asp:Label>
                                           </td>
                                       </tr>
                                   </table> 
                                </td>
                                <td style="width:12px; height:301px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td style="height:10px; width:10px;">&nbsp;</td>
                                <td style="height:10px;">&nbsp;</td>
                                <td style="height:10px; width:10px;">&nbsp;</td>
                            </tr>
                        </table>
                       
                    </div>
                    </td>
                </tr>
            </table>
                    </ItemTemplate>
        </asp:DataList>
    
    </div>
    </form>
</body>
</html>
