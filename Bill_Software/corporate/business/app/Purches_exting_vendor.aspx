<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Purches_exting_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm11" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .style3 {
            color: #FF3300;
        }

        .style4 {
            text-align: center;
        }

        .auto-style1 {
            width: 100%;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
        }

        .redio {
            border: none;
        }

        .auto-style2 {
            height: 24px;
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

        function validateForm() {
            var invNo = document.getElementById('<%= txt_invno.ClientID %>').value.trim();
            var purchDate = document.getElementById('<%= txtPurchesDate.ClientID %>').value.trim();
            var invAmount = document.getElementById('<%= txt_inv_amount.ClientID %>').value.trim();
            var tcsAmount = document.getElementById('<%= txt_tcs_amnt.ClientID %>').value.trim();
            var deliveryAmount = document.getElementById('<%= txt_delivery_amnt.ClientID %>').value.trim();

            var otherAmount1 = document.getElementById('<%= txt_othr_amnt1.ClientID %>').value.trim();
            var otherAmount2 = document.getElementById('<%= txt_othr_amnt2.ClientID %>').value.trim();
            var textBox1 = document.getElementById('<%= TextBox1.ClientID %>').value.trim();
            var textBox2 = document.getElementById('<%= TextBox2.ClientID %>').value.trim();

            var vatDropdown = document.getElementById('<%= DDL_vat_parsentage.ClientID %>');

            // Validate Invoice Number (Should not be empty)
            if (invNo === "") {
                alert("Please enter the Invoice Number.");
                return false;
            }

            // Validate Purchase Date (Should not be empty)
            if (purchDate === "") {
                alert("Please select a valid Purchase Date.");
                return false;
            }

            // Validate Invoice Amount
            //if (isNaN(invAmount) || invAmount === "") {
            //    alert("Please enter a valid Invoice Amount.");
            //    return false;
            //}

            // Validate TCS Amount
            if (isNaN(tcsAmount) || tcsAmount === "") {
                alert("Please enter a valid TCS Amount.");
                return false;
            }

            // Validate VAT Percentage: Required if TCS Amount > 0
            if (parseFloat(tcsAmount) > 0 && vatDropdown.value === "NA") {
                alert("Please select a TCS Percentage since TCS is applied.");
                return false;
            }


            // Validate Delivery Amount
            if (isNaN(deliveryAmount) || deliveryAmount === "") {
                alert("Please enter a valid Delivery Amount.");
                return false;
            }

            // Validate Other Charges-1: If txt_othr_amnt1 > 0, TextBox1 is required
            if (parseFloat(otherAmount1) > 0 && textBox1 === "") {
                alert("Please enter a description for Other Charges-1.");
                return false;
            }

            // Validate Other Charges-2: If txt_othr_amnt2 > 0, TextBox2 is required
            if (parseFloat(otherAmount2) > 0 && textBox2 === "") {
                alert("Please enter a description for Other Charges-2.");
                return false;
            }

            return true; // If all validations pass
        }

        function ValidateField10() {

            if (document.getElementById('<%=cmbvendor.ClientID%>').selectedIndex == 0) {
                alert("Please Select Vendor.");
                document.getElementById('<%=cmbvendor.ClientID%>').focus();
                return false;
            }
        }

        //Function to allow only numbers to textbox
        //function validate(key) {
        //    //getting key code of pressed key
        //    var keycode = (key.which) ? key.which : key.keyCode;
        //    var phn = document.getElementById('txtfillrequar');
        //    //comparing pressed keycodes
        //    if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
        //        return false;
        //    }
        //    else {
        //        //Condition to check textbox contains ten numbers or not
        //        if (phn.value.length < 50) {
        //            return true;
        //        }
        //        else {
        //            return false;
        //        }
        //    }
        //}

        function validate(key) {
            var keycode = key.which ? key.which : key.keyCode;
            var inputField = key.target || key.srcElement; // Get the textbox that triggered the event

            // Allow only numbers (0-9), backspace, delete, and arrow keys
            if ((keycode < 48 || keycode > 57) && keycode !== 8 && keycode !== 46 && keycode !== 37 && keycode !== 39) {
                return false;
            }

            // Allow max 10 digits (Modify if needed)
            if (inputField.value.length >= 10 && keycode >= 48 && keycode <= 57) {
                return false;
            }

            return true;
        }



        function validate1(key) {
            //getting key code of pressed key
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('txtfillrequar');
            //comparing pressed keycodes
            if ((keycode == 39)) {
                return false;
            }
            else {
                return true;

            }

        }

        function ValidateDataField10() {

        }

        <%--function ValidateDataField11() {
            if (document.getElementById('<%=txtpaymentamount.ClientID%>').value == "") {
                alert("Provide Payment amount.");
                document.getElementById('<%=txtpaymentamount.ClientID%>').focus();
                return false;
            }
        }--%>

        function ValidateDataField11() {
            var radioList = document.getElementById('<%= RadioButtonList3.ClientID %>'); // Get RadioButtonList
            var selectedValue = radioList.querySelector('input[type="radio"]:checked').value; // Get selected value

            if (selectedValue === "Yes") { // Run validation only if "Yes" is selected
                if (document.getElementById('<%= txtpaymentamount.ClientID %>').value.trim() === "") {
                    alert("Provide Payment amount.");
                    document.getElementById('<%= txtpaymentamount.ClientID %>').focus();
                    return false;
                }
            }
        }

    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="style1">
                <tr>
                    <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">Create Purchase Request aginst Existing Vendor</span>&nbsp;</td>
                </tr>
                <tr>
                    <td width="10%">&nbsp;</td>
                    <td colspan="2" width="40%">
                        <asp:Label ID="lblvendor_id" runat="server" Visible="False"></asp:Label>
                    </td>
                    <td colspan="2" width="40%">&nbsp;</td>
                    <td width="10%">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD"
                            BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server"
                                ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
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
                    <td>
                        <asp:Label ID="Label2" runat="server" Text="1" Visible="False"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">
                        <span class="style3">*</span>Vendor Name&nbsp;</td>
                    <td width="25%">
                        <asp:DropDownList ID="cmbvendor" runat="server" AutoPostBack="True" CssClass="dropdown_style" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged">
                        </asp:DropDownList>
                    </td>
                    <td width="15%">&nbsp;<span class="style3">*</span>Address 1</td>
                    <td width="25%">
                        <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style"
                            Width="250px" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">Address 2</td>
                    <td width="25%">
                        <asp:TextBox ID="txtAddress2" runat="server" CssClass="textbox_style"
                            Width="250px" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">
                        <span class="style3">*</span>City</td>
                    <td width="25%">
                        <asp:DropDownList ID="cmbcity" runat="server" CssClass="dropdown_style" Enabled="False">
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">
                        <span class="style3">*</span>State&nbsp;</td>
                    <td width="25%">
                        <asp:DropDownList ID="cmbState" runat="server" CssClass="dropdown_style" Enabled="False">
                        </asp:DropDownList>
                    </td>
                    <td width="15%">
                        <span class="style3">*</span>Pin&nbsp;&nbsp;</td>
                    <td width="25%">
                        <asp:TextBox ID="txtPin" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">Company <span>Website</span></td>
                    <td width="25%">
                        <asp:TextBox ID="txtWebsite" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">
                        <span>Company Email ID</span></td>
                    <td width="25%">
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">
                        <span>Company Phone No</span></td>
                    <td width="25%">
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">
                        <span>Company Fax Number</span></td>
                    <td width="25%">
                        <asp:TextBox ID="txtFax" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">
                        <span class="style3">*</span>Representatives Name</td>
                    <td width="25%">
                        <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">
                        <span class="style3">*</span>Designation</td>
                    <td width="25%">
                        <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">Phone No.</td>
                    <td width="25%">
                        <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="textbox_style" onkeypress="return validate(event)" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">Email</td>
                    <td width="25%">
                        <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">Service Tax No</td>
                    <td width="25%">
                        <asp:TextBox ID="txtservicetaxNo" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td width="15%">Pan No</td>
                    <td width="25%">
                        <asp:TextBox ID="txtpanNo" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td width="15%">Vat No</td>
                    <td width="25%">
                        <asp:TextBox ID="txtvat" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
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
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" class="style4">&nbsp;
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
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
                        <asp:Button ID="Button1" runat="server" Text="Purchasse" CssClass="btn_style" Visible="False" OnClick="Button1_Click" />
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
                        <asp:Panel ID="Panel1" runat="server" Visible="False">
                            <table class="auto-style1">
                                <tr>
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%;">Product / Servive List</td>
                                    <td style="width: 40%;">
                                        <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style">
                                        </asp:DropDownList>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
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
                                        <asp:Button ID="Button2" runat="server" Text="Add" CssClass="btn_style" Width="110px" OnClick="Button2_Click" OnClientClick="return ValidateDataField10();" />
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:Panel ID="Panel2" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td colspan="4">
                                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%" OnRowDataBound="gd_Service_Product_RowDataBound">
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
                                                <asp:TemplateField HeaderText="Specification">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="sepecification" runat="server" CssClass="textbox_style21" onkeypress="return validate1(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" Width="250px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Vendor Rate">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <%--<asp:Label ID="Vendor_rate" runat="server" Text='<%# Bind("Vendor_rate") %>'></asp:Label>--%>
                                                        <asp:TextBox ID="Vendor_rate" runat="server" CssClass="textbox_style21" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px"></asp:TextBox>
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
                                    <td>&nbsp;<asp:Label ID="Label3" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase / Invoice Number</td>
                                    <td>
                                        <asp:TextBox ID="txt_invno" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;&nbsp;Purchase / Invoice Amount</td>
                                    <td>
                                        <asp:TextBox ID="txt_inv_amount" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;TCS Amount</td>
                                    <td>
                                        <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Delivery Charges</td>
                                    <td>
                                        <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;&nbsp;@&nbsp;&nbsp;
                                        <asp:DropDownList ID="DDL_vat_parsentage" runat="server" CssClass="dropdown_style"></asp:DropDownList> %
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges-1 &nbsp; <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="txt_othr_amnt1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges-2 &nbsp; <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="txt_othr_amnt2" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
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
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label4" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase Date / Invoice Date</td>
                                    <td>
                                        <asp:TextBox ID="txtPurchesDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;&nbsp;Received Date (Stock Added) </td>
                                    <td>
                                        <asp:TextBox ID="txt_stockadddate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td class="auto-style2"></td>
                                    <td class="auto-style2">&nbsp;&nbsp;Narration Box</td>
                                    <td class="auto-style2">
                                        <asp:TextBox ID="txt_narration" runat="server" CssClass="textbox_U_style" Width="200px" Text="N/A" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                    </td>
                                    <td class="auto-style2"></td>
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
                                    <td>&nbsp;</td>
                                    <td>Payment </td>
                                    <td>
                                        <asp:RadioButtonList ID="RadioButtonList3" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem>Yes</asp:ListItem>
                                            <asp:ListItem Selected="True">No</asp:ListItem>
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
                                        <asp:Button ID="Button3" runat="server" OnClientClick="return validateForm();" OnClick="Button3_Click" Text="Add Purchasse" CssClass="btn_style" />
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
                                    <td>Total Purchess Amount</td>
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
                                        <asp:RadioButtonList ID="RadioButtonList2" runat="server" AutoPostBack="True" OnSelectedIndexChanged="RadioButtonList2_SelectedIndexChanged" RepeatDirection="Horizontal">
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
                                        <div style="width: 100%;" id="First" runat="server" visible="true">
                                            Dated:<asp:TextBox ID="txtcashDate" runat="server" BorderColor="#CCCCCC"
                                                BorderStyle="Solid" BorderWidth="1px" class="datepicker"
                                                Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px"
                                                Width="110px"></asp:TextBox>
                                            &nbsp;
                                        </div>
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
                                        <div style="width: 100%;" id="Third" runat="server" visible="false">
                                            NEFT Reference Number:
                                            <asp:TextBox ID="txtneftnumber" runat="server"
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
                                    <td></td>
                                    <td>&nbsp;&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td colspan="2" style="text-align: center">
                                        <asp:Button ID="btnpurchess_save" runat="server" OnClientClick="return ValidateDataField11();" OnClick="btnpurchess_save_Click" Text="Save" CssClass="btn_style"  />
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
