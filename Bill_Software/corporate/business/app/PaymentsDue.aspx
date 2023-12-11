<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="PaymentsDue.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm88" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
         .style2
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

   
 <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Payment Due</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%" colspan="2">
                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="35%" colspan="2">&nbsp;</td>
            <td width="15%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                </asp:Panel>
            </td>
            <td>&nbsp;</td>
        </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="4">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">Client Name</td>
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
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <div style="width:100%; overflow:auto;">
                <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="140%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label7" runat="server" Text="Payment ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label8" runat="server" Text="Payment Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label12" runat="server" Text="Invoice No"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:11%;">
                                    <asp:Label ID="showrm" runat="server" Text="Quotation No"></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="showid" runat="server" Text="Client Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label2" runat="server" Text="Invoice Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label13" runat="server" Text="Payment Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label19" runat="server" Text="Payment Mode"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label6" runat="server" Text="Instrument no"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label14" runat="server" Text="Instrument Date"></asp:Label>
                                </td>
                                
                                 <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label1" runat="server" Text="Buyers View"></asp:Label>
                                </td>
                                <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label3" runat="server" Text="Sellers View"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                            <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Payment_ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:6%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Payment_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:11%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:18%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">Rs. 
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:7%;">Rs. 
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Given_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("type") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label17" runat="server" Text='<%# Eval("Ch_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label18" runat="server" Text='<%# Eval("Ch_date") %>'></asp:Label>
                                </td>
                                
                              
                                <td style="text-align:center; width:6%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewPaymentInvoice.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                <td style="text-align:center; width:6%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewPaymentInvoiceDuplicate.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>--%>


                    <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="140%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label7" runat="server" Text="CLIENT NAME"></asp:Label>
                                </td>
                              
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="showid" runat="server" Text="QUOTATION NUMBER"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label2" runat="server" Text="QUOTATION DATE"></asp:Label>
                                </td>
                                <%--<td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label13" runat="server" Text="PRODUCT CATEGORY"></asp:Label>
                                </td>--%>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label19" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label6" runat="server" Text="GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label14" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label4" runat="server" Text="PAYMENT AMOUNT RECEIVED (INR)"></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label5" runat="server" Text="PAYMENT AMOUNT DUE (INR)"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                <%--<td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label17" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                </td>--%>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label18" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label20" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label21" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("givenamo") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("dueamo") %>'></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
            </div>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
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
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
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
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
         </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
