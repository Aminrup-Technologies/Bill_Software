<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="add_payment_purchess.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm46" %>
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
                .Grid td
        {
            
            text-align:center;
            font-size: 10px;
            line-height:200%;
			border-color:#2D2D2D;
            border-width:1px;
            border-style: solid;
        }
                 .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
         .auto-style2 {
             text-decoration: underline;
         }
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
        function ValidateField() {


            if (document.getElementById('<%=txtpaymentamount.ClientID%>').value == "") {
                alert("Provide Payment Amount ");
                document.getElementById('<%=txtpaymentamount.ClientID%>').focus();
                return false;
            }






        }
</script>

    
<script type="text/javascript">
    //Function to allow only numbers to textbox
    function validate(key) {
        //getting key code of pressed key
        var keycode = (key.which) ? key.which : key.keyCode;
        var phn = document.getElementById('txtfillrequar');
        //comparing pressed keycodes
        if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
            return false;
        }
        else {
            //Condition to check textbox contains ten numbers or not
            if (phn.value.length < 50) {
                return true;
            }
            else {
                return false;
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
            <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Add Purchase Payment</span>>&nbsp;</td>
        </tr>
        <tr>
            <td width="10%">&nbsp;</td>
            <td width="40%" colspan="2">
                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="40%" colspan="2">&nbsp;</td>
            <td width="10%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
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
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
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
            <td>From Date(Purchese)</td>
            <td>
                <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
            </td>
            <td>To Date(Purchese)</td>
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
            <td colspan="6">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                   <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label10" runat="server" Text="Purchase Id"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label11" runat="server" Text="Purchase Date"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:40%;">
                                    <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label>
                                </td>
                             
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label12" runat="server" Text="Invoice Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit0" runat="server" Text="Select"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label14" runat="server" Text='<%# Eval("Purches_Id") %>'></asp:Label>
                                     </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Purches_date") %>'></asp:Label>
                                     </td>
                                
                                <td style="text-align:center; width:40%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">Rs.
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Total_purches_rate") %>'></asp:Label>
                                    /- </td>
                                <td style="text-align:center; width:10%;">

                                    <asp:ImageButton ID="ImageButton1" runat="server" 
                                            CommandArgument='<%# Eval("Purches_Id") %>' CommandName="Select"  
                                            ImageUrl="~/corporate/business/WebImages/tick-icon.png" 
                                             ToolTip="Select" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
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
            <td colspan="4">
                <asp:Panel ID="Panel1" runat="server" Visible="false">
                <table class="auto-style1">
                    <tr>
                        <td width="13%">Purchese ID</td>
                        <td width="37%">
                            <asp:Label ID="lblpuechess_id" runat="server"></asp:Label>
                        </td>
                        <td width="13%">Purchese Date</td>
                        <td width="37%">
                            <asp:Label ID="lblpuechess_Date" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;Vendor ID</td>
                        <td>
                            <asp:Label ID="lblvendor_id" runat="server"></asp:Label>
                        </td>
                        <td>&nbsp;Vendor Name</td>
                        <td>
                            <asp:Label ID="lblvendor_Name" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>Invoice Amount</td>
                        <td>
                            <asp:Label ID="lblpaayment_amount" runat="server"></asp:Label>
                        </td>
                        <td>Due Amount</td>
                        <td>
                            <asp:Label ID="lbldue_amount" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>Payment Amount</td>
                        <td>
                            <asp:TextBox ID="txtpaymentamount" runat="server" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
                        </td>
                        <td>Payment Date</td>
                        <td>
                            <asp:TextBox ID="txtpaymentdate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Payment Mode</td>
                        <td colspan="2">
                            <asp:RadioButtonList ID="RadioButtonList2" runat="server" AutoPostBack="True" onselectedindexchanged="RadioButtonList2_SelectedIndexChanged" RepeatDirection="Horizontal">
                                <asp:ListItem Selected="True">Cash</asp:ListItem>
                                <asp:ListItem>Cheque</asp:ListItem>
                                <asp:ListItem>DD</asp:ListItem>
                                <asp:ListItem>Online Transaction</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td colspan="2">
                            <div style="width:100%;" id="First" runat="server" visible="true">
                                Dated:<asp:TextBox ID="txtcashDate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
&nbsp;</div>
                            <div id="Second" runat="server" visible="false">
                                Cheque/DD No.<asp:TextBox ID="txtDDno" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Drawee Bank&nbsp;
                                <asp:TextBox ID="txtBankName" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Dated:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtdddate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
                            </div>
                            <div style="width:100%;" id="Third" runat="server" visible="false">
                                NEFT Reference Number: <asp:TextBox ID="txtneftnumber" runat="server" 
                                    CssClass="textbox_style"></asp:TextBox>
                                <br />
                                From Account :&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtbankname1" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Dated:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtneftdate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
                            </div>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td colspan="4" style="text-align: center"><strong class="auto-style2">Previous Payment Details</strong></td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td colspan="4">
                            <asp:DataList ID="DataList2" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                                <FooterStyle BackColor="White" ForeColor="#000066" />
                                <AlternatingItemStyle BackColor="#94B8FF" />
                                <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                                <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                <HeaderTemplate>
                                    <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                        <tr>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="showid1" runat="server" Text="Payment ID"></asp:Label>
                                            </td>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="Label8" runat="server" Text="Payment Date"></asp:Label>
                                            </td>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="Label17" runat="server" Text="Payment Amount"></asp:Label>
                                            </td>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="showrm1" runat="server" Text="Payment Type"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                        <tr>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="ID1" runat="server" Text='<%# Eval("Payment_ID") %>'></asp:Label>
                                            </td>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="Label18" runat="server" Text='<%# Eval("Payment_Date") %>'></asp:Label>
                                            </td>
                                            <td style="text-align:center; width:25%;">Rs.
                                                <asp:Label ID="addshowname1" runat="server" Text='<%# Eval("Given_amount") %>'></asp:Label>
                                                /- </td>
                                            <td style="text-align:center; width:25%;">
                                                <asp:Label ID="Label19" runat="server" Text='<%# Eval("type") %>'></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </ItemTemplate>
                            </asp:DataList>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td colspan="4" style="text-align: center">
                            <asp:Button ID="Button1" runat="server" CssClass="btn_style" Text="Save" OnClick="Button1_Click" onclientclick="return ValidateField();"/>
                        </td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
                    </asp:Panel>
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
