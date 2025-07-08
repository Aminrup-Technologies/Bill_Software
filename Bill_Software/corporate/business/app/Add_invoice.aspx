<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Add_invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm26" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }

        .auto-style2 {
            height: 20px;
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
        function validate2(event, input) {
            var keycode = (event.which) ? event.which : event.keyCode;
            //console.log("Key pressed:", keycode);

            // Allow only numbers (0-9), backspace (8), and delete (46)
            if (!(keycode == 8 || keycode == 46 || (keycode >= 48 && keycode <= 57))) {
                //console.warn("Invalid key pressed.");
                return false;
            }

            // Delay validation to allow input update
            setTimeout(function () {
                validateQuantity(input);
            }, 100);

            return true;
        }

        function validateQuantity(input) {
            console.log("Validating input:", input);

            if (!input) {
                console.error("Input field is null or undefined.");
                return;
            }

            var row = input.closest("tr"); // Find the row of the current input
            console.log("Found row:", row);

            if (!row) {
                console.error("Could not find parent row.");
                return;
            }

            // Find Stock Qty in the same row
            var stockQtyElement = row.querySelector("[id*='SQuantity']");
            console.log("Stock Quantity Element:", stockQtyElement);

            if (!stockQtyElement) {
                console.error("Stock quantity element not found in the row.");
                return;
            }

            var stockQty = stockQtyElement.innerText.trim();
            var enteredQty = input.value.trim();
            console.log("Stock Quantity:", stockQty, "Entered Quantity:", enteredQty);

            if (enteredQty === "" || isNaN(enteredQty)) {
                console.warn("Entered quantity is invalid. Resetting to 0.");
                input.value = "0";
                return;
            }

            stockQty = parseFloat(stockQty) || 0;
            enteredQty = parseFloat(enteredQty) || 0;

            console.log("Parsed Values - Stock:", stockQty, "Entered:", enteredQty);

            var errorSpan = row.querySelector(".error-message");
            console.log("Error message span:", errorSpan);

            if (enteredQty > stockQty) {
                console.warn("Entered quantity exceeds stock quantity.");
                if (errorSpan) {
                    errorSpan.style.display = "inline";
                }
                input.style.borderColor = "red";
            } else {
                console.log("Valid quantity entered.");
                if (errorSpan) {
                    errorSpan.style.display = "none";
                }
                input.style.borderColor = "#CCCCCC";
            }
        }

        function ValidateField() {
            console.log("Validation started...");
            var isValid = true;

            // GridView checkbox validation
            var gridView = document.getElementById('<%= Gridview_Product.ClientID %>');
            if (!gridView) {
                console.error("GridView not found.");
                return false;
            }

            var checkboxes = gridView.getElementsByTagName("input");
            var isChecked = false;

            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].type === "checkbox" && checkboxes[i].id.indexOf("chk") !== -1) {
                    if (checkboxes[i].checked) {
                        isChecked = true;
                        break;
                    }
                }
            }

            if (!isChecked) {
                alert("Please select at least one item from the GridView.");
                return false;
            }

            // Factory Address validation
            var listBox = document.getElementById('<%= FactoryAddress.ClientID %>');
            if (!listBox) {
                console.error("Factory Address ListBox not found.");
                return false;
            }

            var selectedCount = 0;
            for (var i = 0; i < listBox.options.length; i++) {
                if (listBox.options[i].selected) {
                    selectedCount++;
                }
            }

            if (selectedCount === 0) {
                alert("Please select at least one Factory Address.");
                return false;
            }

            // Linked fields
            var txtInvNo = document.getElementById('<%= txtInvoiceNo.ClientID %>');
            var txtTcsAmnt = document.getElementById('<%= txt_tcs_amnt.ClientID %>');
            var txtTcsPercent = document.getElementById('<%= txt_tcs_percent.ClientID %>');
            var txtDeliveryAmnt = document.getElementById('<%= txt_delivery_amnt.ClientID %>');
            var ddlVatPercentage = document.getElementById('<%= DDL_vat_parsentage.ClientID %>');
            var txtOthrAmnt = document.getElementById('<%= txt_othr_amnt.ClientID %>');
            var txtOtherCharges = document.getElementById('<%= TextBox1.ClientID %>');

            // 1. Invoice No is mandatory
            if (txtInvNo && txtInvNo.value.trim() === "") {
                alert("TAX Invoice No is required.");
                txtInvNo.focus();
                return false;
            }

            // 2. TCS Amount >= 1 requires TCS Percent
            if (txtTcsAmnt && txtTcsPercent) {
                var tcsAmt = parseFloat(txtTcsAmnt.value.trim()) || 0;
                var tcsPer = txtTcsPercent.value.trim();

                if (tcsAmt >= 1 && tcsPer === "") {
                    alert("TCS Percent is required when TCS Amount is 1 or more.");
                    txtTcsPercent.focus();
                    return false;
                }
            }

            // 3. Freight Charges >= 1 require VAT %
            if (txtDeliveryAmnt && ddlVatPercentage) {
                var freightAmt = parseFloat(txtDeliveryAmnt.value.trim()) || 0;
                var vatIndex = ddlVatPercentage.selectedIndex;

                if (freightAmt >= 1 && (vatIndex === 0 || ddlVatPercentage.value === "")) {
                    alert("TAX Percentage is required when Freight Charges are 1 or more.");
                    ddlVatPercentage.focus();
                    return false;
                }
            }

            // 4. Other Charges >= 1 require description
            if (txtOthrAmnt && txtOtherCharges) {
                var otherAmt = parseFloat(txtOthrAmnt.value.trim()) || 0;
                var otherDesc = txtOtherCharges.value.trim();

                if (otherAmt >= 1 && otherDesc === "") {
                    alert("Other Charges description is required when amount is 1 or more.");
                    txtOtherCharges.focus();
                    return false;
                }
            }

            console.log("All validations passed.");
            return true;
        }

        function Check_Click(objRef) {
            //Get the Row based on checkbox
            var row = objRef.parentNode.parentNode;
            if (objRef.checked) {
                //If checked change color to Aqua
                row.style.backgroundColor = "#84e26e";
            }
            else {
                //If not checked change back to original color
                if (row.rowIndex % 2 == 0) {
                    //Alternating Row Color
                    row.style.backgroundColor = "#C2D69B";
                }
                else {
                    row.style.backgroundColor = "white";
                }
            }

            //Get the reference of GridView
            var GridView = row.parentNode;
            //Get all input elements in Gridview
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                //The First element is the Header Checkbox
                var headerCheckBox = inputList[0];
                //Based on all or none checkboxes
                //are checked check/uncheck Header Checkbox
                var checked = true;
                if (inputList[i].type == "checkbox" && inputList[i] != headerCheckBox) {
                    if (!inputList[i].checked) {
                        checked = false;
                        break;
                    }
                }
            }
            headerCheckBox.checked = checked;
        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                //Get the Cell To find out ColumnIndex
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {
                        //If the header checkbox is checked
                        //check all checkboxes
                        //and highlight all rows
                        row.style.backgroundColor = "#84e26e";
                        inputList[i].checked = true;
                    }
                    else {
                        //If the header checkbox is checked
                        //uncheck all checkboxes
                        //and change rowcolor back to original
                        if (row.rowIndex % 2 == 0) {
                            //Alternating Row Color
                            row.style.backgroundColor = "#C2D69B";
                        }
                        else {
                            row.style.backgroundColor = "white";
                        }
                        inputList[i].checked = false;
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
                    <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Add Invoice</span>>&nbsp;</td>
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
                        <asp:Panel ID="Panel3" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="image3" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="Label5" runat="server"></asp:Label>
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
                    <td colspan="4">
                        <asp:Panel ID="Panel2" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="Image2" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="Label3" runat="server"></asp:Label>
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
                    <td>From Date(Quotation)</td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>To Date(Quotation)</td>
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
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" />
                        &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />
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
                        <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="showid0" runat="server" Text="Quotation no"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showrm0" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label12" runat="server" Text="Net Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="edit0" runat="server" Text="Select"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="ID0" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="addshowname0" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">Rs.
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                    /- </td>
                                <td style="text-align:center; width:15%;">

                                    <asp:ImageButton ID="ImageButton1" runat="server" 
                                            CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select"  
                                            ImageUrl="~/corporate/business/WebImages/tick-icon.png" 
                                             ToolTip="Select" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>--%>

                        <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#94B8FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <HeaderTemplate>
                                <table border="0" cellpadding="5" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 18%;">
                                            <asp:Label ID="Label2" runat="server" Text="Client Name"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%;">
                                            <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%;">
                                            <asp:Label ID="showid" runat="server" Text="Quotation Number"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 20%;">
                                            <asp:Label ID="Label6" runat="server" Text="Product Category"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%;">
                                            <asp:Label ID="Label7" runat="server" Text="Amount Before GST (INR)"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%;">
                                            <asp:Label ID="Label9" runat="server" Text="GST (INR)"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 14%;">
                                            <asp:Label ID="Label1" runat="server" Text="Total Amount (INR)"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="edit" runat="server" Text="Select"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <table border="0" cellpadding="5" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 18%; padding: 5px;">
                                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%; padding: 5px;">
                                            <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%; padding: 5px;">
                                            <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 20%; padding: 5px;">
                                            <asp:Label ID="Label10" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%; padding: 5px;">
                                            <asp:Label ID="Label11" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%; padding: 5px;">
                                            <asp:Label ID="Label12" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 14%; padding: 5px;">Rs.
                                            <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>/-</td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select" ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>--%>

                        <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px"
                            Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#94B8FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />

                            <HeaderTemplate>
                                <table border="0" cellpadding="2" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 5%;">
                                            <asp:Label Text="Sl No." runat="server" /></td>
                                        <td style="text-align: center; width: 14%;">
                                            <asp:Label Text="Client Name" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="Quotation Date" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="Quotation No" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="DO Number" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="PO Number" runat="server" /></td>
                                        <td style="text-align: center; width: 14%;">
                                            <asp:Label Text="Product Category" runat="server" /></td>
                                        <td style="text-align: center; width: 9%;">
                                            <asp:Label Text="Sub Total (INR)" runat="server" /></td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label Text="GST (INR)" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="Net Amount (INR)" runat="server" /></td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label Text="Select" runat="server" /></td>
                                    </tr>
                                </table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table border="0" cellpadding="5" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 5%; padding: 5px;">
                                            <asp:Label ID="LabelSL" runat="server" Text='<%# Container.ItemIndex + 1 %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 14%; padding: 5px;">
                                            <asp:Label ID="LabelClient" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:Label ID="LabelDate" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:Label ID="LabelQuoNo" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:Label ID="LabelDONo" runat="server" Text='<%# Eval("DO_Number") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:Label ID="LabelPONo" runat="server" Text='<%# Eval("PO_Number") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 14%; padding: 5px;">
                                            <asp:Label ID="LabelService" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 9%; padding: 5px;">
                                            <asp:Label ID="LabelSubtotal" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%; padding: 5px;">
                                            <asp:Label ID="LabelTax" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">Rs.
                                            <asp:Label ID="LabelNet" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                            /-
                                        </td>
                                        <td style="text-align: center; width: 10%; padding: 5px;">
                                            <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select"
                                                ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
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
                    <td colspan="6">
                        <asp:GridView ID="Gridview_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                            <RowStyle BackColor="#94B8FF" />
                            <Columns>
                                <asp:TemplateField HeaderText="Quotation No">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Quotation_no" runat="server" Text='<%# Bind("Quotation_no") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Quotation_no" runat="server" Text='<%# Bind("Quotation_no") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Product Code">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Product_id" runat="server" Text='<%# Bind("Product_id") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Product_id" runat="server" Text='<%# Bind("Product_id") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Product Name">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Product_name" runat="server" Text='<%# Bind("Product_name") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Product_name" runat="server" Text='<%# Bind("Product_name") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="HSN Code">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Product_Code" runat="server" Text='<%# Bind("Product_Code") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Product_Code" runat="server" Text='<%# Bind("Product_Code") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Quoted Qty">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Invoiced Qty">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="DeliveredQnt" runat="server" Text='<%# Bind("DeliveredQnt") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="DeliveredQnt" runat="server" Text='<%# Bind("DeliveredQnt") %>'></asp:Label>
                                    </ItemTemplate>
                                    <HeaderStyle Width="10%" />
                                    <ItemStyle Width="10%" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Stock Qty" HeaderStyle-BackColor="Green" ItemStyle-BackColor="LightGreen">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="SQuantity" runat="server" Text='<%# Bind("SQuantity") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="SQuantity" runat="server" Text='<%# Bind("SQuantity") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Qty Due for Invoicing">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="Qty" runat="server" Text='<%# Bind("RemainQny") %>' BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" onkeypress="return validate2(event, this)">></asp:TextBox>
                                        <span class="error-message" style="color: red; display: none;">Invalid Quantity</span>
                                    </ItemTemplate>
                                    <HeaderStyle Width="10%" />
                                    <ItemStyle Width="10%" />
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Sail Rate">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="sail_rate" runat="server" Text='<%# Bind("sail_rate") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="sail_rate" runat="server" Text='<%# Bind("sail_rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>


                                <asp:TemplateField HeaderText="Gst Rate">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Service_tax_rate" runat="server" Text='<%# Bind("Service_tax_rate") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Service_tax_rate" runat="server" Text='<%# Bind("Service_tax_rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Specific" Visible="false">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="specification" runat="server" Text='<%# Bind("specification") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="specification" runat="server" Text='<%# Bind("specification") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>


                                <asp:TemplateField HeaderText="Amount With Gst">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Total_sail_rate1" runat="server" Text='<%# Bind("Total_sail_rate1") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Total_sail_rate1" runat="server" Text='<%# Bind("Total_sail_rate1") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Amount WithOut Gst">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="Total_sail_rate2" runat="server" Text='<%# Bind("Total_sail_rate2") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Total_sail_rate2" runat="server" Text='<%# Bind("Total_sail_rate2") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Inv Status">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="InvStatus" runat="server" Text='<%# Bind("InvStatus") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="InvStatus" runat="server" Text='<%# Bind("InvStatus") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Select">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="TextBox6" runat="server" checked="true" Enabled="true"></asp:TextBox>
                                    </EditItemTemplate>

                                    <HeaderTemplate>
                                        <asp:CheckBox ID="checkAll" runat="server" Checked="true" Enabled="true" onclick="checkAll(this);" />
                                    </HeaderTemplate>

                                    <ItemTemplate>
                                        <asp:CheckBox ID="chk" runat="server" Checked="true" Enabled="true" onclick="Check_Click(this)" />
                                    </ItemTemplate>
                                    <HeaderStyle Width="3%" />
                                    <ItemStyle Width="3%" />
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
                    <td colspan="4">
                        <asp:Panel ID="Panel1" runat="server" Visible="false">
                            <table class="auto-style1">
                                <tr>
                                    <td width="13%">&nbsp;</td>
                                    <td width="37%">&nbsp;</td>
                                    <td width="13%">Invoice Date</td>
                                    <td width="37%">
                                        <asp:TextBox ID="txtinvoiceDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Quotation No</td>
                                    <td>
                                        <asp:Label ID="lblQuotation_no" runat="server" Font-Bold="true" ForeColor="Blue"></asp:Label>
                                        |
                                        <asp:Label ID="lbl_servicename" runat="server" Font-Bold="true" ForeColor="Blue"></asp:Label>
                                    </td>
                                    <td>Quotation Date</td>
                                    <td>
                                        <asp:Label ID="lblQuotation_date" runat="server"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Client ID</td>
                                    <td>
                                        <asp:Label ID="lblClient_Id" runat="server"></asp:Label>
                                    </td>
                                    <td>Client Name</td>
                                    <td>
                                        <asp:Label ID="lblClientName" runat="server" Font-Bold="true" ForeColor="Blue"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Quotation Amount</td>
                                    <td>
                                        <asp:Label ID="lblGross_amount" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblNet_amount" runat="server"></asp:Label>
                                    </td>
                                    <td>&nbsp;</td>
                                    <td>
                                        <asp:Label ID="lblservicetax" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblservicetax0" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblsubtotal" runat="server" Visible="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr id="dis" runat="server" visible="false">
                                    <td>Discount Amount</td>
                                    <td>
                                        <asp:TextBox ID="txtDiscount" runat="server" Text="0" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
                                    </td>
                                    <td>Address For</td>
                                    <td>
                                        <asp:DropDownList ID="cmbaddressfor" runat="server" CssClass="dropdown_style">
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Delivery Address</td>
                                    <td>
                                        <asp:ListBox ID="FactoryAddress" runat="server" AutoPostBack="True" BorderStyle="Solid" BorderWidth="1px" Font-Size="10px" multiple="true" Rows="3" SelectionMode="Multiple" Width="550px"></asp:ListBox>
                                    </td>
                                    <td style="font-weight: bold;">&nbsp;&nbsp;Invoice No</td>
                                    <td>
                                        <asp:TextBox ID="txtInvoiceNo" runat="server" Text="" CssClass="textbox_style"></asp:TextBox></td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;TCS Amount</td>
                                    <td>
                                        <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;@&nbsp;
                                        <%--<asp:DropDownList ID="DDL_tcspercent" runat="server" CssClass="dropdown_style"></asp:DropDownList>--%>
                                        <asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_U_style" Width="50px" Text=""></asp:TextBox>
                                        %</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Freight Charges</td>
                                    <td>
                                        <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;@&nbsp;
                                        <asp:DropDownList ID="DDL_vat_parsentage" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                                        %</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges &nbsp;
                                        <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="txt_othr_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <%--<tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges-2 &nbsp;
                                        <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="TextBox3" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>--%>
                                <tr>
                                    <td class="auto-style2"></td>
                                    <td class="auto-style2"></td>
                                    <td class="auto-style2"></td>
                                    <td class="auto-style2"></td>
                                </tr>
                                <tr>
                                    <td colspan="4" style="text-align: center">
                                        <asp:Button ID="Button1" runat="server" CssClass="btn_style" Text="Create Invoice" OnClientClick="if (!ValidateField()) return false;" OnClick="Button1_Click" />

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
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="Button1" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
