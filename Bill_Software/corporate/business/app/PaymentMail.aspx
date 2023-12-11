<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="PaymentMail.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm85" %>
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

   <script type = "text/javascript">
function checkAll(objRef)
{
    var GridView = objRef.parentNode.parentNode.parentNode;
    var inputList = GridView.getElementsByTagName("input");
    for (var i=0;i<inputList.length;i++)
    {
        //Get the Cell To find out ColumnIndex
        var row = inputList[i].parentNode.parentNode;
        if(inputList[i].type == "checkbox"  && objRef != inputList[i])
        {
            if (objRef.checked)
            {
                //If the header checkbox is checked
                //check all checkboxes
                //and highlight all rows
                row.style.backgroundColor = "#FFFF99";
                inputList[i].checked=true;
            }
            else
            {
                //If the header checkbox is checked
                //uncheck all checkboxes
                //and change rowcolor back to original
                if(row.rowIndex % 2 == 0)
                {
                   //Alternating Row Color
                   row.style.backgroundColor = "#D5D5BF";
                }
                else
                {
                   row.style.backgroundColor = "white";
                }
                inputList[i].checked=false;
            }
        }
    }
}
</script>
 <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Search Payment</span></td>
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
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
            </td>
            <td>&nbsp;</td>
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
                    ForeColor="#2D2D2D" GridLines="Both" Width="140%" OnItemCommand="DataList1_ItemCommand">
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
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/bill.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                <td style="text-align:center; width:6%;">
                                   <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Payment_ID") %>' CommandName="Select" ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>--%>

                    <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="140%" OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label7" runat="server" Text="CLIENT NAME"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label8" runat="server" Text="PAYMENT ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label12" runat="server" Text="PAYMENT INVOICE NUMBER"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="showrm" runat="server" Text="PAYMENT INVOICE DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="showid" runat="server" Text="QUOTATION NUMBER"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label2" runat="server" Text="QUOTATION DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label13" runat="server" Text="PRODUCT CATEGORY"></asp:Label>
                                </td>
                               <%-- <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label19" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label6" runat="server" Text="GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label14" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                </td>--%>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label6" runat="server" Text="INVOICE AMOUNT WITH GST(INR)"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label24" runat="server" Text="PAYMENT AMOUNT"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label25" runat="server" Text="PAYMENT MODE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label26" runat="server" Text="INSTRUMENT NUMBER"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label27" runat="server" Text="INSTRUMENT DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label33" runat="server" Text="TDS VALUE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label5" runat="server" Text="LAST MAILER DATE"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label1" runat="server" Text="Buyers View"></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label3" runat="server" Text="Sellers View"></asp:Label>
                                </td>

                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label23" runat="server" Text="Select"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Payment_ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Invoice_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Quotation_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label17" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                </td>
                               <%-- <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label18" runat="server" Text='<%# Eval("subtotal") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label20" runat="server" Text='<%# Eval("Gst") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label21" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                </td>--%>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label28" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label29" runat="server" Text='<%# Eval("Given_amount") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label30" runat="server" Text='<%# Eval("type") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label31" runat="server" Text='<%# Eval("Ch_no") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label32" runat="server" Text='<%# Eval("Ch_date") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label34" runat="server" Text='<%# Eval("tds") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="Label22" runat="server" Text='<%# Eval("mailDate") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:5%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewPaymentInvoice.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                <td style="text-align:center; width:5%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/NewPaymentInvoiceDuplicate.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>

                                <td style="text-align:center; width:5%;">
                                   <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Payment_ID") %>' CommandName="Select" ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
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
            <td colspan="6" >

                <asp:Panel ID="PanelRep" runat="server" Visible="false">
            <tr>
                <td colspan="6">
                    
             <tr>
            <td colspan="6">
                <asp:GridView ID="GridRep" runat="server" AutoGenerateColumns="False" 
                    CellPadding="4" ForeColor="#333333" GridLines="None" Width="100%">
                    <RowStyle BackColor="#EFF3FB" />
                    <Columns>
                     <asp:TemplateField HeaderText="Action">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <HeaderTemplate>
                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="chk" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                       <%-- <asp:TemplateField HeaderText="Company Name">
                            <ItemTemplate>
                                <asp:Label ID="cname" runat="server" Text='<%# Bind("cname") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("cname") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>--%>
                        <asp:TemplateField HeaderText="Tital" Visible="False">
                            <ItemTemplate>
                                <asp:Label ID="re_tilal" runat="server" Text='<%# Bind("RepTitle") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="re_tilal" runat="server" Text='<%# Bind("RepTitle") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="last Name" Visible="False">
                            <ItemTemplate>
                                <asp:Label ID="re_lname" runat="server" Text='<%# Bind("RepLastName") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="re_lname" runat="server" Text='<%# Bind("RepLastName") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Representative Name">
                            <ItemTemplate>
                                <asp:Label ID="re_name" runat="server" Text='<%# Bind("Representative_name") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="re_name" runat="server" Text='<%# Bind("Representative_name") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Designation">
                            <ItemTemplate>
                                <asp:Label ID="re_deg" runat="server" Text='<%# Bind("Designation") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="re_deg" runat="server" Text='<%# Bind("Designation") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Email ID">
                            <ItemTemplate>
                                <asp:Label ID="re_email" runat="server" Text='<%# Bind("Email") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="re_email" runat="server" Text='<%# Bind("Email") %>'></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Left" />
                    <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                    <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <EditRowStyle BackColor="#2461BF" />
                    <AlternatingRowStyle BackColor="#DFDFDF" />
                </asp:GridView>
            </td>
        </tr>
            </asp:Panel>

            </td>
            
           
        </tr>

            <tr>
                <td colspan="6">
                    &nbsp;
                </td>

                </tr>
            <tr>
                <td colspan="6">
                    &nbsp;
                </td>

                </tr>
        <tr>
            <td colspan="6" style="text-align:center;"><asp:Button ID="BtnSendMail" runat="server" CssClass="btn_style" OnClick="BtnSendMail_Click" Text="Send Mail" />
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
         </ContentTemplate>
         <Triggers>
                <asp:PostBackTrigger ControlID="BtnSendMail"/>
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
