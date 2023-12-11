<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Payment_due.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm52" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2
        {
            width: 100%;
        }
        .style3
        {
            color: #FFFFFF;
            font-weight: bold;
        }
         .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <script src="calender/jquery-1.7.1.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.core.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.widget.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.datepicker.js" type="text/javascript" language="javascript"></script>
	
<link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

<script type="text/javascript" language="javascript">
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_pageLoaded(function () {
        $(".datepicker").datepicker({
            dateFormat: 'dd-M-yy',

            changeMonth: true,
            changeYear: true
        });
    });
	</script>
     <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>

    <table cellpadding="0" cellspacing="0" class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp; <span class="style3">Payments Due</span>&nbsp;</td>
        </tr>
        <tr>
            <td width="15%">
                &nbsp;</td>
            <td width="35%">
                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="35%">
                &nbsp;</td>
            <td width="15%">
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
        
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                Part Payment Due</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
         <tr>
                <td>&nbsp;</td>
                <td colspan="1">Client Name</td>
                <td colspan="2">
                    <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                    </asp:DropDownList>
                </td>
                <td>&nbsp;</td>
            </tr>
             <tr>
                <td>&nbsp;</td>
                <td>From Date(Payment)</td>
                <td>
                    <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                </td>
                <td>To Date(Payment)</td>
                <td>
                    <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                </td>
                <td>&nbsp;</td>
            </tr>
             <tr>
                <td>&nbsp;</td>
                <td colspan="2">Search Type</td>
                <td colspan="2">
                    <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem>Only Client</asp:ListItem>
                        <asp:ListItem Selected="True">Only Date</asp:ListItem>
                        <asp:ListItem>Client &amp; Date</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="4" style="text-align: center">
                    <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" onclick="btnSertch_Click" Text="Search" />
                    &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" onclick="btnreset_Click" Text="Reset" />
                </td>
                <td>&nbsp;</td>
            </tr>
            <td colspan="4">
            <div id="main_div" runat="server" style="width:100%; overflow:auto;">
            <div id="first_div" runat="server">
                <table class="style2">
                    <tr>
                        <td width="15%">
                            &nbsp;</td>
                        <td width="35%">
                            &nbsp;</td>
                        <td width="35%">
                            &nbsp;</td>
                        <td width="15%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                        <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label7" runat="server" Text="Invoice No"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label8" runat="server" Text="Invoice Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label12" runat="server" Text="Quotation No"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:7%;">
                                    <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                
                                
                                <td style="text-align:center; width:23%;">
                                    <asp:Label ID="showid" runat="server" Text="Client Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label2" runat="server" Text="Invoice Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label13" runat="server" Text="Due Amount"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label1" runat="server" Text="View"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                            <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Invoice_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_Date") %>'></asp:Label>
                                </td>
                               
                                 <td style="text-align:center; width:23%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">Rs.
                                    <asp:Label ID="Label24" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:10%;">Rs. 
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Due_amount") %>'></asp:Label> /-
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/Invoice.aspx?Invoice_No=<%# DataBinder.Eval (Container.DataItem,"Invoice_No")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                </td>
                                
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                    </tr>
                </table>
            </div>
            <div id="Second_div" runat="server">
            <table class="style2">
                    <tr>
                        <td width="15%">
                            &nbsp;</td>
                        <td width="35%">
                            &nbsp;Full Payment Due&nbsp;</td>
                        <td width="35%">
                            &nbsp;</td>
                        <td width="15%">
                            &nbsp;</td>
                    </tr>
                    <tr>
                <td>&nbsp;</td>
                <td colspan="1">Client Name</td>
                <td colspan="2">
                    <asp:DropDownList ID="cmbvendor1" runat="server" CssClass="dropdown_style">
                    </asp:DropDownList>
                </td>
                <td>&nbsp;</td>
            </tr>
             <tr>
                <td>&nbsp;</td>
                <td>From Date(Payment)</td>
                <td>
                    <asp:TextBox ID="txttodate1" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                </td>
                <td>To Date(Payment)</td>
                <td>
                    <asp:TextBox ID="txtfromDate1" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                </td>
                <td>&nbsp;</td>
            </tr>
             <tr>
                <td>&nbsp;</td>
                <td colspan="2">Search Type</td>
                <td colspan="2">
                    <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem>Only Client</asp:ListItem>
                        <asp:ListItem Selected="True">Only Date</asp:ListItem>
                        <asp:ListItem>Client &amp; Date</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="4" style="text-align: center">
                    <asp:Button ID="btnSertch1" runat="server" CssClass="btn_style" onclick="btnSertch_Click" Text="Search" />
                    &nbsp;<asp:Button ID="btnreset2" runat="server" CssClass="btn_style" onclick="btnreset_Click" Text="Reset" />
                </td>
                <td>&nbsp;</td>
            </tr>
                    <tr>
                        <td colspan="4">
                <asp:DataList ID="DataList2" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label7" runat="server" Text="Invoice No"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label8" runat="server" Text="Invoice Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label12" runat="server" Text="Quotation No"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:7%;">
                                    <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                
                                
                                <td style="text-align:center; width:23%;">
                                    <asp:Label ID="showid" runat="server" Text="Client Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label2" runat="server" Text="Invoice Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label13" runat="server" Text="Due Amount"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label1" runat="server" Text="View"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                            <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Invoice_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_Date") %>'></asp:Label>
                                </td>
                               
                                 <td style="text-align:center; width:23%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">Rs.
                                    <asp:Label ID="Label24" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:10%;">Rs. 
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label> /-
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/Invoice.aspx?Invoice_No=<%# DataBinder.Eval (Container.DataItem,"Invoice_No")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                </td>
                                
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                        <td>
                            &nbsp;</td>
                    </tr>
                </table>
            </div>
            </div>
                </td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
</asp:Content>
