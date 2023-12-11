<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Purches_new_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm10" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style type="text/css">
    .style1
    {
        width: 100%;
    }
    .style2
    {
        color: #FFFFFF;
        font-weight: bold;
    }
    .style3
    {
        color: #FF3300;
    }
        .style4
        {
            text-align: center;
        }
         .auto-style1 {
             width: 100%;
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
         .redio
          {
             border:none;
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
    prm.add_endRequest(function () {
        $(".datepicker").datepicker({
            dateFormat: 'dd-M-yy',

            changeMonth: true,
            changeYear: true
        });
    });
	</script>

    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtvendorName.ClientID%>').value == "") {
                alert("Provide Vendor Name.");
                document.getElementById('<%=txtvendorName.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=txtAddress1.ClientID%>').value == "") {
                alert("Provide Vendor Address ");
                document.getElementById('<%=txtAddress1.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=cmbcity.ClientID%>').selectedIndex == 0) {
                alert("Please Select City.");
                document.getElementById('<%=cmbcity.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) {
                alert("Please Select State.");
                document.getElementById('<%=cmbState.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtPin.ClientID%>').value == "") {
                alert("Provide Vendor Pin");
                document.getElementById('<%=txtPin.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=txtRepresentativeName.ClientID%>').value == "") {
                alert("Provide Representatives Name");
                document.getElementById('<%=txtRepresentativeName.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresantativeDesig.ClientID%>').value == "") {
                alert("Provide Representatives Designation.");
                document.getElementById('<%=txtRepresantativeDesig.ClientID%>').focus();
                return false;
            }




        }
</script>


    <script type="text/javascript">
        function ValidateField10() {



            var objSource1 = document.getElementById("<%=listProduct_Service.ClientID%>");
            if (objSource1.selectedIndex < 0 || objSource1.options.length < 0) {
                alert("Please Select one or more Product OR Services....");
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
<script type="text/javascript">
    function ValidateDataField10() {





        var objSource4 = document.getElementById("<%=listProduct_Service.ClientID%>");
        if (objSource4.selectedIndex < 0 || objSource4.options.length < 0) {
            alert("Select one or more Service Or Products....");
            return false;
        }




    }

</script>

     <script type="text/javascript">
         function ValidateDataField11() {



             if (document.getElementById('<%=txtpaymentamount.ClientID%>').value == "") {
                alert("Provide Payment amount.");
                document.getElementById('<%=txtpaymentamount.ClientID%>').focus();
                return false;
            }




        }

</script>

    <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
     <table class="style1">
    <tr>
        <td bgcolor="#19658A" colspan="6">
            &nbsp;<span class="style2">Fix Purchasse Price To New Vendor</span>&nbsp;</td>
    </tr>
    <tr>
        <td width="10%">
            &nbsp;</td>
        <td colspan="2" width="40%">
            <asp:Label ID="lblvendor_id" runat="server" Visible="False"></asp:Label>
        </td>
        <td colspan="2" width="40%">
            &nbsp;</td>
        <td width="10%">
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="4">
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
        <td width="15%">
            <span class="style3">*</span>Vendor Name&nbsp;</td>
        <td width="25%">
            <asp:TextBox ID="txtvendorName" runat="server" CssClass="textbox_style" 
                Width="250px"></asp:TextBox>
        </td>
        <td width="15%">
            &nbsp;<span class="style3">*</span>Address 1</td>
        <td width="25%">
            <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" 
                Width="250px"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
   
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Address 2</td>
        <td width="25%">
            <asp:TextBox ID="txtAddress2" runat="server" CssClass="textbox_style" 
                Width="250px"></asp:TextBox>
        </td>
        <td width="15%">
            <span class="style3">*</span>City</td>
        <td width="25%">
            <asp:DropDownList ID="cmbcity" runat="server" CssClass="dropdown_style">
            </asp:DropDownList>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            <span class="style3">*</span>State&nbsp;</td>
        <td width="25%">
            <asp:DropDownList ID="cmbState" runat="server" CssClass="dropdown_style">
            </asp:DropDownList>
        </td>
        <td width="15%">
            <span class="style3">*</span>Pin&nbsp;&nbsp;</td>
        <td width="25%">
            <asp:TextBox ID="txtPin" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Company <span>Website</span></td>
        <td width="25%">
            <asp:TextBox ID="txtWebsite" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            <span>Company Email ID</span></td>
        <td width="25%">
            <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            <span>Company Phone No</span></td>
        <td width="25%">
            <asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            <span>Company Fax Number</span></td>
        <td width="25%">
            <asp:TextBox ID="txtFax" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            <span class="style3">*</span>Representatives Name</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            <span class="style3">*</span>Designation</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Phone No.</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
        </td>
        <td width="15%">
            Email</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Service Tax No</td>
        <td width="25%">
            <asp:TextBox ID="txtservicetaxNo" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            Pan No</td>
        <td width="25%">
            <asp:TextBox ID="txtpanNo" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>
         <tr>
             <td>&nbsp;</td>
             <td width="15%">Vat No</td>
             <td width="25%">
                 <asp:TextBox ID="txtvat" runat="server" CssClass="textbox_style"></asp:TextBox>
             </td>
             <td width="15%">&nbsp;</td>
             <td width="25%">&nbsp;</td>
             <td>&nbsp;</td>
         </tr>
         <tr>
             <td>&nbsp;</td>
             <td width="15%">&nbsp;</td>
             <td width="25%">&nbsp;</td>
             <td width="15%">&nbsp;</td>
             <td width="25%">&nbsp;</td>
             <td>&nbsp;</td>
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
        <td colspan="4" class="style4">
            <asp:Button ID="btnSave" runat="server" CssClass="btn_style" 
                onclick="btnSave_Click" Text="Save" onclientclick="return ValidateField();"/>
&nbsp;
            </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td>
            <asp:Label ID="Label1" runat="server" Text="Purchasse Type" Visible="False"></asp:Label>
        </td>
        <td>
            <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" Visible="False">
                <asp:ListItem Selected="True">Product</asp:ListItem>
                <asp:ListItem>Service</asp:ListItem>
            </asp:RadioButtonList>
        </td>
        <td colspan="2">
            <asp:Button ID="Button1" runat="server" Text="Purchasse" CssClass="btn_style" Visible="False" OnClick="Button1_Click"/>
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
            <asp:Panel ID="Panel1" runat="server" Visible="False">
                <table class="auto-style1">
                    <tr>
                        <td style="width:10%;">&nbsp;</td>
                        <td style="width:40%;">Product / Servive List</td>
                        <td style="width:40%;">
                            <asp:ListBox ID="listProduct_Service" runat="server" CssClass="textbox_style" Height="250px" SelectionMode="Multiple" Width="300px"></asp:ListBox>
                        </td>
                        <td style="width:10%;">&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td colspan="2" style="text-align: center">
                            <asp:Button ID="Button2" runat="server" Text="Purchasse Details" CssClass="btn_style" Width="110px" OnClick="Button2_Click" onclientclick="return ValidateDataField10();"/>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </asp:Panel>
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
        <td colspan="6">
            <asp:Panel ID="Panel2" runat="server" Visible="false">
                <table cellpadding="0" cellspacing="0" class="auto-style1">
                    <tr>
                        <td width="15%">&nbsp;</td>
                        <td width="35%">&nbsp;</td>
                        <td width="35%">&nbsp;</td>
                        <td width="15%">&nbsp;</td>
                    </tr>
                    <tr>
                        <td colspan="4">
                            <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%" OnRowDataBound="gd_Service_Product_RowDataBound">
                                <RowStyle BackColor="#94B8FF" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Service/Product Code">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Ser_pro_code") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Bind("Ser_pro_code") %>'></asp:Label>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Service/Product Name">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:Label>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Vendor Rate">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Vendor_rate" runat="server" Text='<%# Bind("Vendor_rate") %>'></asp:Label>
                                            <%--<asp:TextBox ID="Vendor_rate" runat="server" CssClass="textbox_style21" onkeypress="return validate(event)" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Height="22px"></asp:TextBox>--%>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Tax Applicable">
                                        
                                        <ItemTemplate>
                                                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem>Yes</asp:ListItem>
                    <asp:ListItem Selected="True">No</asp:ListItem>
                </asp:RadioButtonList>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Input %(VAT/SERVICE TAX)">
                                        
                                        <ItemTemplate>
                                            <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="dropdown_style">
            </asp:DropDownList>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                     <asp:TemplateField HeaderText="Quantity">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                             <asp:TextBox ID="Quantity" runat="server" CssClass="textbox_style21" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Sail Rate">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Sale_rate" runat="server" Text='<%# Bind("Sale_rate") %>'></asp:Label>
                                            <%--<asp:TextBox ID="Sale_rate" runat="server" CssClass="textbox_style21" onkeypress="return validate(event)" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Height="22px"></asp:TextBox>--%>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Output %(VAT/SERVICE TAX)">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="service_Tax_Rate" runat="server" Text='<%# Bind("service_Tax_Rate") %>'></asp:Label>
                                             <%--<asp:DropDownList ID="service_Tax_Rate" runat="server" CssClass="dropdown_style">
            </asp:DropDownList>--%>
                                        </ItemTemplate>
                                        
                                    </asp:TemplateField>
                                   
                                   
                                </Columns>
                                <FooterStyle BackColor="#CCCC99" />
                                <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                            </asp:GridView>
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
                        <td>Purchasse Date</td>
                        <td>
                            <asp:TextBox ID="txtPurchesDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    
                    <tr>
                        <td>&nbsp;</td>
                        <td>Payment </td>
                        <td>
                            <asp:RadioButtonList ID="RadioButtonList3" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Selected="True">Yes</asp:ListItem>
                                <asp:ListItem>No</asp:ListItem>
                            </asp:RadioButtonList>
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
                        <td>&nbsp;</td>
                        <td colspan="2" style="text-align: center">
                            <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Purchasse" CssClass="btn_style"/>
                        </td>
                        <td>&nbsp;</td>
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
    </tr>
    <tr>
        <td colspan="6">
            <asp:Panel ID="Panel3" runat="server" Visible="false">
                <table cellpadding="0" cellspacing="0" class="auto-style1">
                    <tr>
                        <td width="15%">&nbsp;</td>
                        <td width="35%">Purchesse ID</td>
                        <td width="35%">
                            <asp:Label ID="lblpuechess_id" runat="server"></asp:Label>
                        </td>
                        <td width="15%">&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>Total Purchesse Amount</td>
                        <td>
                            <asp:Label ID="lblpaayment_amount" runat="server"></asp:Label>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>Payment Date</td>
                        <td>
                            <asp:TextBox ID="txtpaymentdate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>Payment Amount</td>
                        <td>
                            <asp:TextBox ID="txtpaymentamount" runat="server" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td>Payment Mode</td>
                        <td>
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
                        <td>&nbsp;</td>
                        <td>
                             <div style="width:100%;" id="First" runat="server" visible="true">
                                Dated:<asp:TextBox ID="txtcashDate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
&nbsp;</div>
                            <div id="Second" runat="server" visible="false" class="style2">
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
                        <td>&nbsp;</td>
                        <td>
                        </td>
                        <td>&nbsp;&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td colspan="2" style="text-align: center">
                            <asp:Button ID="btnpurchess_save" runat="server" OnClick="btnpurchess_save_Click" Text="Save" CssClass="btn_style" onclientclick="return ValidateDataField11();"/>
                        </td>
                        <td>&nbsp;</td>
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
