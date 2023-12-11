<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="view_patty_cash_expenses.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm57" %>
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
        .style4
        {
            text-align: center;
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
    <script type="text/javascript">
        function ValidateDelete() {


            

         }
</script>
	<asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
    <table cellpadding="0" cellspacing="0" class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="6">
                &nbsp;<span class="style3"> View Patty Cash Expenses</span>&nbsp;</td>
        </tr>
        <tr>
            <td width="15%">
                &nbsp;</td>
            <td width="35%" colspan="2">
                &nbsp;</td>
            <td width="35%" colspan="2">
                &nbsp;</td>
            <td width="15%">
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" 
                    BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" 
                        ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                </asp:Panel>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;Cash Status&nbsp;</td>
            <td colspan="2">
                <asp:DropDownList ID="cmbcashstatus" runat="server" CssClass="dropdown_style">
                    <asp:ListItem>Cash In</asp:ListItem>
                    <asp:ListItem>Cash Out</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td width="15%">
                &nbsp;From Date&nbsp;</td>
            <td width="20%">
                <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" 
                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                    Width="110px"></asp:TextBox>
            </td>
            <td width="15%">
                &nbsp;To Date&nbsp;</td>
            <td width="20%">
                <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" 
                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                    Width="110px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;Seartch Type&nbsp;</td>
            <td colspan="2">
                <asp:RadioButtonList ID="RadioButtonList1" runat="server" 
                    RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">Only Date</asp:ListItem>
                    <asp:ListItem>Cash Status &amp; Date</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td class="style4" colspan="4">
                <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" 
                    onclick="btnSertch_Click" Text="Search" onclientclick="return ValidateDelete();"/>
                &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" 
                    onclick="btnreset_Click" Text="Reset" />
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" 
                    Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showid" runat="server" Text="Payment ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="showrm" runat="server" Text="Payment Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label3" runat="server" Text="Cash Status"></asp:Label>
                                </td>
                                <td style="text-align:center; width:19%;">
                                    <asp:Label ID="Label2" runat="server" Text="Expense Head"></asp:Label>
                                </td>
                                <td style="text-align:center; width:9%;">
                                    <asp:Label ID="Label6" runat="server" Text="Payment Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label11" runat="server" Text="Payment Mode"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label1" runat="server" Text="Payment Made To"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label8" runat="server" Text="Employee ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("payment_id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("payment_date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label12" runat="server" Text='<%# Eval("cash_status") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:19%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("expences_head") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:9%;">Rs.
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("payment_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("payment_mode") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("payment_made_to") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("emp_id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <a href = "#" title="Print Voutcher..." onclick="window.open('/corporate/business/print/Patty_cash_expencess_voutcher.aspx?payment_id=<%# DataBinder.Eval (Container.DataItem,"payment_id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
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
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
     </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
