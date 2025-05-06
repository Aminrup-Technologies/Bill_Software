<%@ Page Title="Create Quotations" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Create_quotation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm19" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .auto-style1 tr {
            height: 10px;
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

        .center {
            text-align: center;
        }

        .auto-style2 {
            height: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
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

        function ValidateField() {
            if (document.getElementById('<%=cmbClient.ClientID%>').selectedIndex == 0) {
                alert("Please select a Client.");
                document.getElementById('<%=cmbClient.ClientID%>').focus();
                return false;
            }

            var clientRefName = document.getElementById('<%=txt_clientrefname.ClientID%>').value.trim();
            if (clientRefName === "") {
                alert("Please enter Client Reference Name.");
                document.getElementById('<%=txt_clientrefname.ClientID%>').focus();
                return false;
            }

            var clientRefId = document.getElementById('<%=txt_clientrefid.ClientID%>').value.trim();
            if (clientRefId === "") {
                alert("Please enter Client Reference ID.");
                document.getElementById('<%=txt_clientrefid.ClientID%>').focus();
                return false;
            }

            var clientRefDate = document.getElementById('<%=txt_clientrefdate.ClientID%>').value.trim();
            if (clientRefDate === "") {
                alert("Please enter Client Reference Date.");
                document.getElementById('<%=txt_clientrefdate.ClientID%>').focus();
                return false;
            }

            var quotationDate = document.getElementById('<%=txtquotationDate.ClientID%>').value.trim();
            if (quotationDate === "") {
                alert("Please enter Quotation Date.");
                document.getElementById('<%=txtquotationDate.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=ddlPlaceOfSupply.ClientID%>').selectedIndex == 0) {
                alert("Please select Place of Supply.");
                document.getElementById('<%=ddlPlaceOfSupply.ClientID%>').focus();
                return false;
            }

            var gstOptions = document.getElementById('<%=RadioButtonGst.ClientID%>').getElementsByTagName('input');
            var gstSelected = false;
            for (var i = 0; i < gstOptions.length; i++) {
                if (gstOptions[i].checked) {
                    gstSelected = true;
                    break;
                }
            }
            if (!gstSelected) {
                alert("Please select a GST option.");
                return false;
            }
            return true;
        }


        function validateRowSelection(gridViewId) {
            //console.log("GridView ID being passed: " + gridViewId);
            var gridView = document.getElementById('ContentPlaceHolder1_gridProdWithCat');
            if (!gridView) {
                //console.log("GridView not found. Exiting validation.");
                alert("GridView not found.");
                return false;
            }

            //console.log("GridView found. Proceeding with validation.");

            // Step 2: Get all the rows in the GridView (including header row)
            var rows = gridView.getElementsByTagName('tr');
            var isRowSelected = false;

            // Loop through each row to check if any row is selected
            for (var i = 0; i < rows.length; i++) {
                // Find the checkbox inside the row
                var checkBox = rows[i].querySelector("input[type='checkbox']");

                if (checkBox && checkBox.checked) {
                    isRowSelected = true;
                    break;  // Exit the loop if at least one row is selected
                }
            }

            // Log the result of the row selection
            if (isRowSelected) {
                console.log("At least one row is selected.");
                return true;  // At least one row is selected
            } else {
                console.log("No row is selected.");
                alert("Please select at least one row.");
                return false;  // No row is selected
            }
        }

        function validateGridView(gridViewId) {
            var gridView = document.getElementById(gridViewId); // Get the GridView by its Client ID

            if (!gridView) {
                alert("GridView not found.");
                return false;
            }

            // Check if there are rows (subtract 1 for the header row)
            if (gridView.rows.length <= 1) {
                alert("No rows are present in the GridView.");
                return false;
            }

            return true; // Validation passed
        }

        function validateListBox(listBoxId) {
            var listBox = document.getElementById('ContentPlaceHolder1_listPhaseType');

            if (!listBox) {
                alert("ListBox not found.");
                return false;
            }

            // Check if at least one item is selected
            var isSelected = false;
            for (var i = 0; i < listBox.options.length; i++) {
                if (listBox.options[i].selected) {
                    isSelected = true;
                    break;
                }
            }

            if (!isSelected) {
                alert("Please select at least one item from the Payment Phase.");
                return false;
            }

            return true; // Validation passed
        }

        function handlePackageForwardingChange(dropdown) {
            var selectedValue = dropdown.value;
            var manualInputPkgRow = document.getElementById("manualInputPkgRow");

            if (selectedValue == "3") { // Manual Input selected
                manualInputPkgRow.style.display = "table-row"; // Show the textbox row
            } else {
                manualInputPkgRow.style.display = "none"; // Hide the textbox row
            }
        }

        function validateButtonClick() {
            if (!validateListBox('<%= listPhaseType.ClientID %>')) {
                return false;
            }

            var ddlPkgFrwd = document.getElementById('<%= DDL_ItemViewType.ClientID %>');
            if (ddlPkgFrwd && ddlPkgFrwd.value === "0") {
                alert("Please select Particular View type");
                ddlPkgFrwd.focus();
                return false;
            }

            var validDays = document.getElementById('<%= txt_valdays.ClientID %>');
            if (validDays && (isNaN(validDays.value) || validDays.value <= 0)) {
                alert("Please enter a valid number of days greater than 0.");
                validDays.focus();
                return false;
            }

            var ddlDeliveryTerms = document.getElementById('<%= DDL_DeliveryTerms.ClientID %>');
            if (ddlDeliveryTerms && ddlDeliveryTerms.value === "0") {
                alert("Please select a valid delivery tenure.");
                ddlDeliveryTerms.focus();
                return false;
            }

            if (ddlDeliveryTerms && ddlDeliveryTerms.value === "4") {
                var manualInput = document.getElementById('<%= txt_deltrms.ClientID %>');
                var regex = /^[0-9]+-[0-9]+$/;
                if (manualInput && !regex.test(manualInput.value)) {
                    alert("Please enter a valid delivery tenure in the format 'value1-value2'.");
                    manualInput.focus();
                    return false;
                }
            }

            var ddlPkgFrwd = document.getElementById('<%= DDL_pkgfrwd.ClientID %>');
            if (ddlPkgFrwd && ddlPkgFrwd.value === "0") {
                alert("Please select a valid package forwarding option.");
                ddlPkgFrwd.focus();
                return false;
            }

            if (ddlPkgFrwd && ddlPkgFrwd.value === "3") {
                var pkgInput = document.getElementById('<%= txt_pkgfrwd.ClientID %>');
                if (pkgInput && pkgInput.value.trim() === "") {
                    alert("Please enter a valid package forwarding input.");
                    pkgInput.focus();
                    return false;
                }
            }

            var remarks = document.getElementById('<%= txt_remarks.ClientID %>');
            if (remarks && remarks.value.trim() === "") {
                alert("Please enter your remarks or comments.");
                remarks.focus();
                return false;
            }
            if (remarks && remarks.value.length > 200) {
                alert("Remarks or comments cannot exceed 200 characters.");
                remarks.focus();
                return false;
            }
            var grid = document.getElementById('<%= GridView3.ClientID %>');
            var rows = grid.getElementsByTagName('tr');
            var isValid = true;

            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var amountPerInput = row.querySelector('[id$="AmountPer"]');

                if (amountPerInput) {
                    var amount = parseFloat(amountPerInput.value);
                    if (isNaN(amount) || amount < 0 || amount > 100) {
                        alert("Please enter a valid Payment Percentage (0-100) for each row.");
                        amountPerInput.focus();
                        isValid = false;
                        break;
                    }

                    // If only one row, set AmountPer to 100
                    if (rows.length === 1 && amount !== 100) {
                        alert("If only one row is present, AmountPer must be 100.");
                        amountPerInput.focus();
                        isValid = false;
                        break;
                    }
                }
            }

            if (!isValid) {
                return false;
            }

            return true;
        }

        function validate(key, element) {
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = element;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
                return false;
            } else {
                return phn.value.length < 50;
            }
        }

        function validate1(key) {
            var keycode = (key.which) ? key.which : key.keyCode;
            return keycode !== 39;  // Block right arrow key
        }

        function ValidateDataField10() {

            if (document.getElementById('<%=cmbproduct_service.ClientID%>').selectedIndex == 0) {
                alert("Please Select Product OR Service.");
                document.getElementById('<%=cmbproduct_service.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=ddlPlaceOfSupply.ClientID%>').selectedIndex == 0) {
                alert("Please Select Place Of Supply.");
                document.getElementById('<%=cmbproduct_service.ClientID%>').focus();
                return false;
            }
        }

        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;
            console.log("row", row);
            if (objRef.checked) {
                row.style.backgroundColor = "#84e26e";
            }

            else {
                if (row.rowIndex % 2 == 0) {
                    row.style.backgroundColor = "#C2D69B";
                }
                else {
                    row.style.backgroundColor = "white";
                }
            }

            var GridView = row.parentNode;
            console.log("GridView", GridView);
            var inputList = GridView.getElementsByTagName("input");
            console.log("GridView-inputList", inputList);
            for (var i = 0; i < inputList.length; i++) {
                var headerCheckBox = inputList[0];
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
            console.log("GridView", GridView);
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {
                        row.style.backgroundColor = "#84e26e";
                        inputList[i].checked = true;
                    }

                    else {
                        if (row.rowIndex % 2 == 0) {
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

        function handleDeliveryTermChange(dropdown) {
            var selectedValue = dropdown.value;
            var manualInputRow = document.getElementById("manualInputRow");

            if (selectedValue == "4") {
                manualInputRow.style.display = "table-row";
            } else {
                manualInputRow.style.display = "none";
            }
        }

        function validateRowSelectionForAnotherGrid(gridViewId) {
            console.log("GridView ID being passed: " + gridViewId);
            var gridView = document.getElementById('ContentPlaceHolder1_gd_Service_Product');

            if (!gridView) {
                console.log("GridView not found. Exiting validation.");
                alert("GridView not found.");
                return false;
            }

            console.log("GridView found. Proceeding with validation.");
            var rows = gridView.getElementsByTagName('tr');
            var isRowSelected = false;
            var isAllFieldsFilled = true;

            for (var i = 0; i < rows.length; i++) {
                var checkBox = rows[i].querySelector("input[type='checkbox']");

                if (checkBox && checkBox.checked) {
                    isRowSelected = true;

                    // Get required input fields inside the row
                    var sailRate = rows[i].querySelector("input[id*='Sail_Rate']");
                    var quantity = rows[i].querySelector("input[id*='Quantity']");
                    var discountRate = rows[i].querySelector("input[id*='Discount_Rate']");

                    // Check if any required field is empty
                    //if (!sailRate || sailRate.value.trim() === "") {
                    //    alert("Base Rate (RS) is required.");
                    //    isAllFieldsFilled = false;
                    //    break;
                    //}
                    if (!quantity || quantity.value.trim() === "") {
                        alert("Quantity is required.");
                        isAllFieldsFilled = false;
                        break;
                    }
                    //if (!discountRate || discountRate.value.trim() === "") {
                    //    alert("Discount (%) is required.");
                    //    isAllFieldsFilled = false;
                    //    break;
                    //}
                }
            }

            if (!isRowSelected) {
                alert("Please select at least one row.");
                return false;
            }

            if (!isAllFieldsFilled) {
                return false;
            }

            console.log("Validation passed.");
            return true;
        }

        function toggleReferenceFields(value) {
            document.getElementById('<%= hdnRefOption.ClientID %>').value = value;

            var nameField = document.getElementById('<%= txt_clientrefname.ClientID %>');
            var idField = document.getElementById('<%= txt_clientrefid.ClientID %>');
            var dateField = document.getElementById('<%= txt_clientrefdate.ClientID %>');

            if (value === 'Yes') {
                nameField.readOnly = false;
                idField.readOnly = false;
                dateField.readOnly = false;
            } else {
                nameField.value = "N/A";
                idField.value = "N/A";
                dateField.value = "01-Jan-2000";
                nameField.readOnly = true;
                idField.readOnly = true;
                dateField.readOnly = true;
            }
        }


        function togglePanel() {
            var rbQt = document.getElementById('<%= rbQt.ClientID %>');
            var panel = document.getElementById('<%= PO_DataInputs.ClientID %>');
            var poFields = document.querySelectorAll('.po-mandatory'); // Select all mandatory fields

            if (rbQt.checked) {
                panel.style.display = 'none'; // Hide panel if Quotation is selected

                // Remove 'required' attribute from PO fields
                poFields.forEach(function (field) {
                    field.removeAttribute('required');
                });
            } else {
                panel.style.display = 'block'; // Show panel if Purchase Order is selected

                // Add 'required' attribute to PO fields
                poFields.forEach(function (field) {
                    field.setAttribute('required', 'required');
                });
            }
        }

        // Call function on page load to set initial visibility and field validation
        window.onload = function () {
            togglePanel();
        };

    </script>

    <asp:HiddenField ID="hdnRefOption" runat="server" />
    <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server">--%>
    <%--<ContentTemplate>--%>

    <table cellpadding="1" cellspacing="1" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A"><span class="style2">&nbsp;Create Quotation</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">
                <asp:Label ID="lblclientID" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                <asp:Label ID="lblqno" runat="server" Visible="False"></asp:Label>
            </td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">
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
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select Client Name</td>
            <td>
                <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style"></asp:DropDownList>
            </td>
            <td>
                <asp:Label ID="Label1" runat="server" Text="1" Visible="False"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="auto-style2"></td>
            <td style="text-align: right;" class="auto-style2">Enable Reference Details&nbsp;:&nbsp;</td>
            <td class="auto-style2">
                <asp:RadioButton ID="rbYes" runat="server" GroupName="referenceOption" Text="Yes" onclick="toggleReferenceFields('Yes')" />
                <asp:RadioButton ID="rbNo" runat="server" GroupName="referenceOption" Text="No" Checked="true" onclick="toggleReferenceFields('No')" />
            </td>
            <td class="auto-style2"></td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;&nbsp;Reference Person Name&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefname" runat="server" CssClass="textbox_style" Width="110px" ReadOnly="true"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;Reference ID&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefid" runat="server" CssClass="textbox_style" Width="110px" ReadOnly="true"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;Reference Date&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefdate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"
                    class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px" ReadOnly="true"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;<asp:Label ID="Label2" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Reference Person Name&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefname" runat="server" CssClass="textbox_style" Width="110px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;Reference ID&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefid" runat="server" CssClass="textbox_style" Width="110px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td style="text-align: right;">&nbsp;Reference Date&nbsp;:&nbsp;</td>
            <td>
                <asp:TextBox ID="txt_clientrefdate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label3" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Quotation Date&nbsp;</td>
            <td>
                <asp:TextBox ID="txtquotationDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
            <td>Quotation Type</td>
            <td>
                <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">New</asp:ListItem>
                    <asp:ListItem>Old</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Sale Type&nbsp;</td>
            <td>
                <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">Product / Service</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>&nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label4" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Place Of Supply</td>
            <td>
                <asp:DropDownList ID="ddlPlaceOfSupply" runat="server" CssClass="dropdown_style">
                </asp:DropDownList>
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
            <td>Select CGST/SGST for Intra-State OR IGST for Inter-State</td>
            <td>
                <asp:Panel ID="panelGst" runat="server">
                    <%-- <asp:CheckBox ID="CHKCGSTSGST" runat="server" Enabled="true" Text="CGST/SGST" />
                    &nbsp;<asp:CheckBox ID="CHKIGST" runat="server" Text="IGST" />--%>

                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Value="1"> CGST/SGST </asp:ListItem>
                        <asp:ListItem Value="0"> IGST </asp:ListItem>
                    </asp:RadioButtonList>

                </asp:Panel>
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
            <td>&nbsp;<asp:Label ID="lbl_recordtype" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>Select Record / Document Type</td>
            <td>&nbsp;
                <asp:RadioButton ID="rbQt" runat="server" GroupName="recordOption" Text="Quotation" Checked="true" AutoPostBack="false" OnClick="togglePanel()" />&nbsp;&nbsp;
                <asp:RadioButton ID="rbPo" runat="server" GroupName="recordOption" Text="Purchase Order" AutoPostBack="false" OnClick="togglePanel()" />
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
            <td colspan="2">
                <asp:Panel ID="PO_DataInputs" runat="server" Visible="true">
                    <table cellpadding="2" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label2" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Delivery Order No.</td>
                            <td width="50%">
                                <asp:TextBox ID="txb_donumber" runat="server" CssClass="textbox_style po-mandatory" Width="110px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label8" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Ref. Contract No.</td>
                            <td width="50%">
                                <asp:TextBox ID="txb_ponumber" runat="server" CssClass="textbox_style po-mandatory" Width="110px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label5" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase Order Date</td>
                            <td width="50%">
                                <asp:TextBox ID="txb_podate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label6" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Validity Start Date</td>
                            <td width="50%">
                                <asp:TextBox ID="txb_strtdt" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label7" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Validity End Date</td>
                            <td width="50%">
                                <asp:TextBox ID="txb_enddt" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
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
                <asp:Button ID="Button1" runat="server" Text="Click to Retrieve Product/Service Category" CssClass="btn_style" OnClientClick="return ValidateField();" OnClick="Button1_Click" Width="300px" />
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
            <td colspan="2">
                <asp:Panel ID="Panel1" runat="server" Visible="False">
                    <table cellpadding="0" cellspacing="0" class="auto-style1">
                        <tr>
                            <td width="50%">Select Product &/or Service Category One by One</td>
                            <td width="50%">
                                <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <%-- <tr>
                            <td width="50%">Factory Address</td>
                            <td width="50%">
                                <asp:ListBox ID="listOffactory" runat="server"></asp:ListBox>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>--%>

                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" Text="Click to Retrieve Product &/or Service from the selected Category" OnClientClick="return ValidateDataField10();" OnClick="Button2_Click" Width="400px" />
                            </td>
                        </tr>
                        <tr>
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
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:Panel ID="Panel2" runat="server" Visible="False">
                    <table cellpadding="0" cellspacing="0" class="auto-style1">
                        <%--<asp:Panel ID="Panel3" runat="server">--%>
                        <tr>
                            <td style="color: red; text-align: center; font-weight: bold;" colspan="4"><span style="font-weight: 900; font-size: 14px;">*</span>Click the Checkbox to Select the Desired Product/Service</td>
                        </tr>
                        <tr>

                            <td colspan="4">
                                <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="HSN CODE">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product ID">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service" Visible="false">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category" Visible="false">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Name">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Extra Specifications">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Unit">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>



                                        <%--   <asp:TemplateField HeaderText="EXTRA SPECIFICATIONS">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="specification" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="250px" onkeypress="return validate1(event)"></asp:TextBox>
                                               </ItemTemplate>
                                        </asp:TemplateField>--%>

                                        <asp:TemplateField HeaderText="Base Rate (RS)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' onkeypress="return validate(event, this)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="GST Rate">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Tax_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Quantity">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Quantity" runat="server" onkeypress="return validate(event, this)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Select">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>

                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>

                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" />
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
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="btnAddProduct" runat="server" CssClass="btn_style" Text="Add Required Product &/or Service  against the Selected Category from the above Table" OnClientClick="return validateRowSelection('<%= gridProdWithCat.ClientID %>');" Width="500px" OnClick="btnAddProduct_Click" />
                            </td>
                            <td colspan="2" style="text-align: center; color: red; font-weight: bold;">Go back to the Select Product/Service Category in case more Product/Service Categories need to be added</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <%-- </asp:Panel>--%>

                        <%--<asp:Panel ID="Panel4" runat="server" Visible="False">--%>
                        <tr>
                            <td style="color: red; text-align: center; font-weight: bold;" colspan="4"><span style="font-weight: bold; font-size: 13px;">*</span>After Selection of the Desired Product Category/s, Change the Base Rate as required and add the required Quantity</td>
                        </tr>

                        <tr>
                            <td colspan="4">
                                <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="HSN CODE" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product ID" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category" Visible="false" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Name" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Extra Specifications" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>' onkeypress="return validate(event, this)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Unit" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%-- <asp:TemplateField HeaderText="Specification">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="specification" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="250px" onkeypress="return validate1(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>
                                        <%-- <asp:TemplateField HeaderText="Tax Applicable">
                                            <ItemTemplate>
                                                <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal">
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
                                        </asp:TemplateField>--%>

                                        <asp:TemplateField HeaderText="Item No" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="ItemNo" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Material No" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="MaterialNo" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Pack Size" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="PackSize" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Base Rate (RS)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Width="80%" CssClass="center textbox_style" Height="22px" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="GST Rate (%)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Tax_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Discount (%)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Discount_Rate" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Text="0" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Quantity" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Quantity" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Remarks" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox9" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="ItemRemarks" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Delivery Date" HeaderStyle-Width="10%" ItemStyle-Width="10%" Visible="false">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="DeliveryDate" runat="server" CssClass="datepicker"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="DeliveryDate" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="datepicker center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfvDeliveryDate" runat="server" ControlToValidate="DeliveryDate" ErrorMessage="*" ForeColor="Red" Display="Dynamic" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Department" HeaderStyle-Width="10%" ItemStyle-Width="10%" Visible="false">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Department" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Department" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfvDepartment" runat="server" ControlToValidate="Department" ErrorMessage="*" ForeColor="Red" Display="Dynamic" />
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Select" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>

                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>

                                            <ItemTemplate>
                                                <asp:CheckBox ID="chk" runat="server" onclick="Check_Click(this)" />
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
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button4" runat="server" CssClass="btn_style" Text="Add Required Product &/or Service  against the Selected Category from the above Table" OnClientClick="return validateRowSelectionForAnotherGrid('<%= gd_Service_Product.ClientID %>');" Width="500px" />
                            </td>
                            <td colspan="2" style="text-align: center; color: red; font-weight: bold;">Before proceeding with Payment Phase and other Terms & Conditions, Please select the final list of products</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <%--</asp:Panel>--%>

                        <tr>
                            <td colspan="4">
                                <table>
                                    <tr>
                                        <td width="20%" style="font-weight: bold;">Add Payment Phase & Payment %age<br />
                                            (Select Payment Phase One By One)</td>
                                        <td width="5%"></td>
                                        <td width="30%">
                                            <asp:ListBox ID="listPhaseType" runat="server" Font-Size="14px" multiple="true" SelectionMode="Multiple" Rows="7" Width="250px" BackColor="#94b8ff" OnTextChanged="listPhaseType_TextChanged" AutoPostBack="True"></asp:ListBox></td>
                                        <td width="5%"></td>
                                        <td width="40%">
                                            <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" BorderWidth="1px" BackColor="#94b8ff" CellPadding="3" CellSpacing="2" BorderStyle="None" BorderColor="#DEBA84" OnRowDeleting="GridView3_RowDeleting">
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Payment Phase">
                                                        <EditItemTemplate>
                                                            <asp:TextBox ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:TextBox>
                                                        </EditItemTemplate>
                                                        <ItemTemplate>
                                                            <asp:Label ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Phase Description">
                                                        <EditItemTemplate>
                                                            <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' TextMode="MultiLine"></asp:TextBox>
                                                        </EditItemTemplate>
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' TextMode="MultiLine"></asp:TextBox>
                                                        </ItemTemplate>

                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Payment %age">
                                                        <%--<EditItemTemplate>
                                                            <asp:TextBox ID="AmountPer" runat="server"  Text=""></asp:TextBox>
                                                        </EditItemTemplate>--%>
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="AmountPer" runat="server" AutoPostBack="true" Text='<%# Bind("AmountPer") %>' OnTextChanged="AmountPer_TextChanged"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:CommandField ButtonType="Button" HeaderText="Delete" ShowDeleteButton="True" />
                                                </Columns>
                                            </asp:GridView>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <%--<tr>
                            
                            <td style="font-weight:bold; border:1px;"></td>
                            <td colspan="2" style="border:1px;">
                                
                             </td>
                            <td style=""></td>
                        </tr>--%>

                        <tr>
                            <td>&nbsp;</td>
                            <td colspan="2"></td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Validay Day Input (Days) :</td>
                            <td>&nbsp;<asp:TextBox ID="txt_valdays" runat="server" Text="0" CssClass="textbox_style" TextMode="Number" MaxLength="3"></asp:TextBox></td>
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
                            <td style="text-align: right;">&nbsp;Particulars View Type :</td>
                            <td>
                                <asp:DropDownList ID="DDL_ItemViewType" runat="server" CssClass="dropdown_style">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Simple" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Detailed" Value="2"></asp:ListItem>
                                </asp:DropDownList></td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Delivery Tenure Selection (Weeks) :</td>
                            <td>
                                <asp:DropDownList ID="DDL_DeliveryTerms" runat="server" CssClass="dropdown_style" onchange="handleDeliveryTermChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="10-12" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="3-4" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="1-2" Value="3"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="4"></asp:ListItem>
                                </asp:DropDownList></td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr id="manualInputRow" style="display: none;">
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Delivery Tenure Input (Weeks) :</td>
                            <td>&nbsp;<asp:TextBox ID="txt_deltrms" runat="server" Text="0" CssClass="textbox_style" TextMode="SingleLine" MaxLength="5" placeholder="e.g., 1-2"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RFV_txt_deltrms" runat="server" ErrorMessage="Required" ControlToValidate="txt_deltrms" Display="Dynamic" InitialValue="0"></asp:RequiredFieldValidator>
                            </td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Package Forwarding Option :</td>
                            <td>
                                <asp:DropDownList ID="DDL_pkgfrwd" runat="server" CssClass="dropdown_style" onchange="handlePackageForwardingChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="NILL" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="At Actuals" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="3"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr id="manualInputPkgRow" style="display: none;">
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Package Forwarding Input :</td>
                            <td>
                                <asp:TextBox ID="txt_pkgfrwd" runat="server" Text="" CssClass="textbox_style" TextMode="SingleLine" MaxLength="50" placeholder="Enter package details"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RFV_txt_pkgfrwd" runat="server" ErrorMessage="Required" ControlToValidate="txt_pkgfrwd" Display="Dynamic"></asp:RequiredFieldValidator>
                            </td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td style="text-align: right;">&nbsp;Custom Remarks / Comments :</td>
                            <td>&nbsp;<asp:TextBox ID="txt_remarks" runat="server" Text="" CssClass="textbox_style" TextMode="MultiLine" MaxLength="200" Rows="4" Columns="50" placeholder="Enter your remarks or comments..."></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RFV_txt_remarks" runat="server" ErrorMessage="Remarks are required." ControlToValidate="txt_remarks" Display="Dynamic"></asp:RequiredFieldValidator>
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
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>
                                <asp:GridView ID="gridps" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Visible="false" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Service/ProductCatagory">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductCatagory" runat="server" Text='<%# Bind("ProductCatagory") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductCatagory" runat="server" Text='<%# Bind("ProductCatagory") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Select">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox9" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkp" runat="server" Checked="true" onclick="Check_Click(this)" />
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
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button3" runat="server" CssClass="btn_style" Text="Save" CausesValidation="true" ValidationGroup="Final" OnClientClick="return validateButtonClick();" OnClick="Button3_Click" />
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
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>

    <%--</ContentTemplate>--%>
    <%--<Triggers>
                <asp:PostBackTrigger ControlID="Button1"  />
                <asp:PostBackTrigger ControlID="Button2" />
                <asp:PostBackTrigger ControlID="Button3" />
              <asp:PostBackTrigger ControlID="GridView3" />
            </Triggers>--%>
    <%--</asp:UpdatePanel>--%>
</asp:Content>
