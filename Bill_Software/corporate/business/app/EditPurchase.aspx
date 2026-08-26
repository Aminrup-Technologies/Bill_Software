<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="EditPurchase.aspx.cs" Inherits="Bill_Software.corporate.business.app.EditPurchase" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
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

        .center {
            text-align: center;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
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

        function calculateDiscount(changedInput) {
            console.log("Triggered calculateDiscount");

            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.getElementsByTagName("tr");

            for (var i = 1; i < rows.length; i++) {
                if (rows[i].contains(changedInput)) {
                    var row = rows[i];

                    //var rateInput = row.querySelector("input[id*='Vendor_rate']");
                    //var qtyInput = row.querySelector("input[id*='Quantity']");
                    //var percentInput = row.querySelector("input[id*='DiscountPercent']");
                    //var amountInput = row.querySelector("input[id*='DiscountAmount']");
                    //var taxableAmountInput = row.querySelector("input[id*='TaxableAmount']");

                    //var rate = rateInput && rateInput.value ? parseFloat(rateInput.value) : 0;
                    //var qty = qtyInput && qtyInput.value ? parseFloat(qtyInput.value) : 0;
                    //var total = rate * qty;

                    //var percent = percentInput && percentInput.value ? parseFloat(percentInput.value) : 0;
                    //var amount = amountInput && amountInput.value ? parseFloat(amountInput.value) : 0;

                    // Use CSS classes instead of partial IDs
                    var rateInput = row.querySelector(".txtVendorRate");
                    var qtyInput = row.querySelector(".txtQuantity");
                    var percentInput = row.querySelector(".txtDiscountPercent");
                    var amountInput = row.querySelector(".txtDiscountAmount");
                    var taxableAmountInput = row.querySelector(".txtTaxableAmount");

                    var rate = rateInput && rateInput.value ? parseFloat(rateInput.value) : 0;
                    var qty = qtyInput && qtyInput.value ? parseFloat(qtyInput.value) : 0;
                    var total = rate * qty;

                    var percent = percentInput && percentInput.value ? parseFloat(percentInput.value) : 0;
                    var amount = amountInput && amountInput.value ? parseFloat(amountInput.value) : 0;

                    if (changedInput === percentInput) {
                        amount = ((percent / 100) * total);
                        amountInput.value = amount.toFixed(2);
                        console.log("Discount % changed. Amount:", amount.toFixed(2));
                    } else if (changedInput === amountInput) {
                        percent = total !== 0 ? ((amount / total) * 100) : 0;
                        percentInput.value = percent.toFixed(2);
                        console.log("Discount Amount changed. %:", percent.toFixed(2));
                    }

                    // Update Taxable Amount
                    var taxable = total - amount;
                    if (taxableAmountInput) {
                        taxableAmountInput.value = taxable.toFixed(2);
                        console.log("Taxable Amount:", taxable.toFixed(2));
                    }

                    break;
                }
            }
        }

        function validate(key) {
            var keycode = key.which ? key.which : key.keyCode;
            var inputField = key.target || key.srcElement;
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
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('sepecification');
            if ((keycode == 39)) {
                return false;
            }
            else {
                return true;
            }
        }

        function validateForm() {
            var invNo = document.getElementById('<%= txt_invno.ClientID %>').value.trim();
            var purchDate = document.getElementById('<%= txtPurchesDate.ClientID %>').value.trim();
            var invAmount = document.getElementById('<%= txt_inv_amount.ClientID %>').value.trim();

            var otherAmount1 = document.getElementById('<%= txt_othr_amnt1.ClientID %>').value.trim();
            var otherAmount2 = document.getElementById('<%= txt_othr_amnt2.ClientID %>').value.trim();
            var textBox1 = document.getElementById('<%= TextBox1.ClientID %>').value.trim();
            var textBox2 = document.getElementById('<%= TextBox2.ClientID %>').value.trim();

            var deliveryAmount = document.getElementById('<%= txt_delivery_amnt.ClientID %>').value.trim();
            var vatDropdown = document.getElementById('<%= DDL_vat_parsentage.ClientID %>');

            var tcsAmount = document.getElementById('<%= txt_tcs_amnt.ClientID %>').value.trim();
            var vattcspercent = document.getElementById('<%= txt_tcs_percent.ClientID %>').value.trim();

            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.querySelectorAll("tbody tr");

            var isQuantityValid = false;

            //for (var i = 0; i < rows.length; i++) {
            //    var qtyInput = rows[i].querySelector("input[id*='Quantity']");
            //    if (qtyInput && parseFloat(qtyInput.value.trim()) >= 1) {
            //        isQuantityValid = true;
            //        break;
            //    }
            //}

            //if (!isQuantityValid) {
            //    alert("At least one row in the Product List must have Quantity >= 1.");
            //    return false;
            //}

            //for (var i = 0; i < rows.length; i++) {
            //    var row = rows[i];
            //    var qtyInput = row.querySelector("input[id*='Quantity']");
            //    var rateInput = row.querySelector("input[id*='Vendor_rate']");
            //    var discInput = row.querySelector("input[id*='Disc']");
            //    var taxRadioSelected = row.querySelector("input[id*='RadioButtonList1']:checked");
            //    var vatDropdownRow = row.querySelector("select[id*='vat_parsentage']");
            //    var ordertxt = row.querySelector("input[id*='txtOrder']");
            //    // Skip completely empty rows (template/footer/header etc.)
            //    if (!qtyInput && !rateInput && !discInput && !ordertxt) continue;
            //    var serProCodeLabel = row.querySelector("span[id*='Ser_pro_code']");
            //    var serProCodeTextbox = row.querySelector("input[id*='TextBox2']");

            //    var serProCode = `Row ${i + 1}`;
            //    if (serProCodeTextbox && serProCodeTextbox.value) {
            //        serProCode = serProCodeTextbox.value.trim();
            //    } else if (serProCodeLabel && serProCodeLabel.innerText) {
            //        serProCode = serProCodeLabel.innerText.trim();
            //    }

            //    var qty = qtyInput ? qtyInput.value.trim() : "";
            //    var rate = rateInput ? rateInput.value.trim() : "";
            //    var disc = discInput ? discInput.value.trim() : "";
            //    var vatValue = vatDropdownRow ? vatDropdownRow.value : "NA";
            //    var taxApplicable = taxRadioSelected ? taxRadioSelected.value : "";
            //    var order = ordertxt ? ordertxt.value.trim() : "";

            //    //var isRowTouched = (qty !== "" && parseFloat(qty) > 0) ||
            //    //   (rate !== "" && parseFloat(rate) > 0) ||
            //    //   (disc !== "" && parseFloat(disc) > 0) ||
            //    //   (vatValue !== "NA") ||
            //    //   (order !== "" && parseFloat(order) >= 0);

            //    //if (isRowTouched) {
            //    //    console.log(`Row ${i + 1} | Product Code: ${serProCode} | Qty: ${qty} | Rate: ${rate} | Disc: ${disc} | Tax Applicable: ${taxApplicable} | VAT: ${vatValue} | Order: ${order}`);

            //    //    if (qty === "" || isNaN(qty) || parseFloat(qty) <= 0) {
            //    //        alert(`Please enter a valid Quantity for Product Code ${serProCode}.`);
            //    //        return false;
            //    //    }

            //    //    if (rate === "" || isNaN(rate) || parseFloat(rate) < 0) {
            //    //        alert(`Please enter a valid Rate for Product Code: ${serProCode}.`);
            //    //        return false;
            //    //    }

            //    //    if (!taxApplicable) {
            //    //        alert(`Please select Tax Applicable for Product Code: ${serProCode}.`);
            //    //        return false;
            //    //    }

            //    //    if (taxApplicable === "Yes" && (vatValue === "" || vatValue === "NA")) {
            //    //        alert(`Please select VAT Percentage for Product Code: ${serProCode} since Tax is applicable.`);
            //    //        return false;
            //    //    }

            //    //    if (order === "" || isNaN(order) || parseFloat(order) < 0) {
            //    //        alert(`Please enter a valid Order / SL for Product Code: ${serProCode}.`);
            //    //        return false;
            //    //    }
            //    //}
            //}

            isQuantityValid = true;

            // Final checks
            if (invNo === "") {
                alert("Please enter the Invoice Number.");
                return false;
            }
            if (purchDate === "") {
                alert("Please select a valid Purchase Date.");
                return false;
            }
            if (tcsAmount === "" || isNaN(tcsAmount)) {
                alert("Please enter a valid TCS Amount.");
                return false;
            }
            if (parseFloat(tcsAmount) > 0 && vattcspercent === "") {
                alert("Please input a TCS Percentage since TCS is applied.");
                return false;
            }
            if (deliveryAmount === "" || isNaN(deliveryAmount)) {
                alert("Please enter a valid Delivery Amount.");
                return false;
            }
            if (parseFloat(deliveryAmount) > 0 && vatDropdown.value === "NA") {
                alert("Please select a Freight VAT Percentage since Freight is applied.");
                return false;
            }
            if (parseFloat(otherAmount1) > 0 && textBox1 === "") {
                alert("Please enter a description for Other Charges-1.");
                return false;
            }
            if (parseFloat(otherAmount2) > 0 && textBox2 === "") {
                alert("Please enter a description for Other Charges-2.");
                return false;
            }

            return true;
        }


        function ValidateField10() {

            if (document.getElementById('<%=cmbvendor.ClientID%>').selectedIndex == 0) {
                alert("Please Select Vendor.");
                document.getElementById('<%=cmbvendor.ClientID%>').focus();
                return false;
            }
        }

        function ValidateField_11() {

            if (document.getElementById('<%=cmbproduct_service.ClientID%>').selectedIndex == 0) {
                alert("Please Select Product Category.");
                document.getElementById('<%=cmbproduct_service.ClientID%>').focus();
                return false;
            }
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
    </script>


    <script type="text/javascript">

    var purchaseSearchTimer = null;

    function debouncedPurchaseSearch() {
        clearTimeout(purchaseSearchTimer);
        purchaseSearchTimer = setTimeout(function () {
            searchPurchaseGrid();
        }, 300);
    }

    function getPurchaseRowText(row) {
        var text = row.innerText || "";

        // include textbox values inside grid
        var inputs = row.querySelectorAll("input[type='text']");
        inputs.forEach(function (inp) {
            text += " " + inp.value;
        });

        return text.toLowerCase();
    }

    function searchPurchaseGrid() {
        var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
        var filter = input.value.trim().toLowerCase();
        var grid = document.getElementById('<%= gridProdWithCat.ClientID %>');
        var rows = grid.getElementsByTagName("tr");
        var matchCount = 0;

        for (var i = 1; i < rows.length; i++) {
            var row = rows[i];
            var rowText = getPurchaseRowText(row);

            if (filter === "" || rowText.indexOf(filter) > -1) {
                row.style.display = "";
                if (filter !== "") matchCount++;
            } else {
                row.style.display = "none";
            }
        }

        document.getElementById("lblNoRecords").style.display =
            (filter !== "" && matchCount === 0) ? "block" : "none";
    }

    function clearPurchaseSearch() {
        var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
        var grid = document.getElementById('<%= gridProdWithCat.ClientID %>');
        var rows = grid.getElementsByTagName("tr");

        input.value = "";

        for (var i = 1; i < rows.length; i++) {
            rows[i].style.display = "";
        }

        document.getElementById("lblNoRecords").style.display = "none";
        input.focus();
    }

</script>


    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Edit Purchase</span></td>
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
                    <td colspan="2">&nbsp;Client Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date(Quotataion)</td>
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

                <tr id="SelectorGridRow" runat="server" visible="true">
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
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="showid" runat="server" Text="Purchase ID"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="showrm" runat="server" Text="Creation Date"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label2" runat="server" Text="Vendor Name"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="lblInvoiceNo" runat="server" Text="Invoice No"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblInvoiceDate" runat="server" Text="Invoice Date"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label1" runat="server" Text="Order No."></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label8" runat="server" Text="Order Date"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label12" runat="server" Text="Shipped To"></asp:Label>
                                        </td>

                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="Label3" runat="server" Text="Total TAX"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="Label6" runat="server" Text="Total Purchase Rate"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="Label13" runat="server" Text="Edit"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="ID" runat="server" Text='<%# Eval("Purches_Id") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="addshowname" runat="server" Text='<%# Eval("TimeStamp") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="lblInvNo" runat="server" Text='<%# Eval("Invoice_No") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblInvDate" runat="server" Text='<%# Eval("Purches_date") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("BuyerOrderNo") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label10" runat="server" Text='<%# Eval("OrderDate") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label11" runat="server" Text='<%# Eval("ShippedToStoreName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">Rs.
                                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("Total_Tax_rate") %>'></asp:Label>/-</td>
                                        <td style="text-align: center; width: 8%;">Rs.
                                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("Total_purches_rate") %>'></asp:Label>/-</td>

                                        <td style="text-align: center; width: 6%;">
                                            <a href="#" title="Print Purchase Bill..." onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# DataBinder.Eval(Container.DataItem,"Purches_Id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="22px" src="../WebImages/viewicon.png" />
                                            </a>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Purches_Id") %>' CommandName="Select" ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
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
                <tr id="ModifierGridRow" runat="server" visible="false">
                    <td colspan="6">
                        <asp:Panel ID="Panel_Edit" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Label ID="Label15" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Vendor Name & ID</td>
                                    <td width="35%">&nbsp;<asp:Label ID="lbl_vendorname" runat="server" Text="Vendor Name" Font-Bold="true" ForeColor="DarkBlue"></asp:Label>&nbsp;-&nbsp;[<asp:Label ID="lblvendor_id" runat="server" Text="Vendor ID"></asp:Label>]</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Label ID="Label14" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase Type </td>
                                    <td width="35%">
                                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" Visible="true">
                                            <asp:ListItem Selected="True">Product</asp:ListItem>
                                            <asp:ListItem>Service</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase ID </td>
                                    <td width="35%">&nbsp;<asp:Label ID="lbl_purchaseid" runat="server" Text="Purchase ID" Font-Bold="true" ForeColor="DarkBlue"></asp:Label></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label3" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase / Invoice Number </td>
                                    <td>
                                        <asp:TextBox ID="txt_invno" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>
                                        &nbsp;&nbsp;Ref. / Buyer's Order No :&nbsp;&nbsp;
                                        <asp:TextBox ID="txt_reforder" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;(optional)
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label4" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase Date / Invoice Date</td>
                                    <td>
                                        <asp:TextBox ID="txtPurchesDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                        &nbsp;&nbsp;Ref. / Buyer's Order Date :&nbsp;&nbsp;
                                        <asp:TextBox ID="txt_refordrdate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>&nbsp;(optional)
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
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;TCS Amount</td>
                                    <td>
                                        <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;&nbsp;@&nbsp;&nbsp;
                                        <%--<asp:DropDownList ID="DDL_tcspercent" runat="server" CssClass="dropdown_style"></asp:DropDownList>--%>
                                        <asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_U_style" Width="50px" Text=""></asp:TextBox>
                                        %
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Delivery Charges</td>
                                    <td>
                                        <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;&nbsp;@&nbsp;&nbsp;
                                        <asp:DropDownList ID="DDL_vat_parsentage" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                                        %
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges-1 &nbsp;
                                        <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="txt_othr_amnt1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;(Taxable)
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Other Charges-2 &nbsp;
                                        <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                                    <td>
                                        <asp:TextBox ID="txt_othr_amnt2" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox>&nbsp;(Non-Taxable)
                                    </td>
                                    <td></td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
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
                                    <td>&nbsp;</td>
                                    <td>&nbsp;&nbsp;Received Location (Shipped To) </td>
                                    <td>
                                        <asp:DropDownList ID="DDL_ShippedTo" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td width="15%"></td>
                                    <td width="35%">&nbsp;&nbsp;Narration Box</td>
                                    <td width="35%">
                                        <asp:TextBox ID="txt_narration" runat="server" CssClass="textbox_U_style" Width="200px" Text="N/A" TextMode="MultiLine" Rows="3"></asp:TextBox>
                                    </td>
                                    <td width="15%"></td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Button ID="btn_UpdatePurchase" runat="server" Width="110px" OnClientClick="return validateForm();" OnClick="btn_UpdatePurchase_Click" Text="Update Purchase" CssClass="btn_style" /></td>
                                    <td width="15%">&nbsp;<asp:Label ID="msg_updt_purches" runat="server" Text="" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
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

                <tr id="DB_DataGrid" runat="server" visible="false">
                    <td colspan="6">
                        <asp:Panel ID="Panel_DBDataItems" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td colspan="4">
                                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" CellPadding="1" DataKeyNames="Id,Purches_id,Ser_pro_code,Ser_pro_Name" OnRowEditing="gd_Service_Product_RowEditing" OnRowUpdating="gd_Service_Product_RowUpdating" OnRowCancelingEdit="gd_Service_Product_RowCancelingEdit" OnRowDeleting="gd_Service_Product_RowDeleting">
                                            <Columns>
                                                <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="true" />
                                                <asp:BoundField DataField="Purches_id" HeaderText="Purches ID" ReadOnly="true" />
                                                <asp:BoundField DataField="Ser_pro_code" HeaderText="Service/Product Code" ReadOnly="true" />
                                                <asp:BoundField DataField="Ser_pro_Name" HeaderText="Service/Product Name" ReadOnly="true" />

                                                <asp:TemplateField HeaderText="Specification">
                                                    <ItemTemplate><%# Eval("Specification") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtSpecification" runat="server" Text='<%# Bind("Specification") %>' Width="250px" CssClass="textbox_style21" onkeypress="return validate1(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Quantity">
                                                    <ItemTemplate><%# Eval("Quantity") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtQuantity" runat="server" Text='<%# Bind("Quantity") %>' CssClass="textbox_style21 txtQuantity" onkeyup="calculateDiscount(this)" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Vendor Rate">
                                                    <ItemTemplate><%# Eval("vendor_rate") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtVendorRate" runat="server" Text='<%# Bind("vendor_rate") %>' CssClass="textbox_style21 txtVendorRate" onkeyup="calculateDiscount(this)" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Discount %">
                                                    <ItemTemplate><%# Eval("DiscountPercent") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtDiscountPercent" runat="server" Text='<%# Bind("DiscountPercent") %>' onkeyup="calculateDiscount(this)" Width="60px" CssClass="textbox_style21 txtDiscountPercent" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Disc. Amount">
                                                    <ItemTemplate><%# Eval("DiscountAmount") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtDiscountAmount" runat="server" Text='<%# Bind("DiscountAmount") %>' onkeyup="calculateDiscount(this)" Width="80px" CssClass="textbox_style21 txtDiscountAmount" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Taxable Amount">
                                                    <ItemTemplate><%# Eval("TaxableAmount") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtTaxableAmount" runat="server" Text='<%# Bind("TaxableAmount") %>' CssClass="textbox_style21 txtTaxableAmount" ReadOnly="true" Width="100px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Tax Applicable">
                                                    <ItemTemplate><%# Eval("TaxApplicable") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:RadioButtonList ID="rblTaxApplicable" runat="server" RepeatDirection="Horizontal"
                                                            SelectedValue='<%# Bind("TaxApplicable") %>'>
                                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                                            <asp:ListItem Value="No">No</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Input %">
                                                    <ItemTemplate><%# Eval("VatPercent") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddlVatPercentage" runat="server"
                                                            SelectedValue='<%# Bind("VatPercent") %>'>
                                                            <asp:ListItem Value="0">0%</asp:ListItem>
                                                            <asp:ListItem Value="5">5%</asp:ListItem>
                                                            <asp:ListItem Value="5.00">5%</asp:ListItem>
                                                            <asp:ListItem Value="12">12%</asp:ListItem>
                                                            <asp:ListItem Value="18">18%</asp:ListItem>
                                                            <asp:ListItem Value="28">28%</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Order">
                                                    <ItemTemplate><%# Eval("OrderNo") %></ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtOrder" runat="server" Text='<%# Bind("OrderNo") %>' CssClass="textbox_style21" Width="50px" />
                                                    </EditItemTemplate>
                                                </asp:TemplateField>

                                                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                                            </Columns>

                                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                        </asp:GridView>
                                    </td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Button ID="btn_AddProducts" runat="server" Text="Add More Products" CssClass="btn_style" OnClick="btn_AddProducts_Click" Width="150px" />&nbsp;&nbsp;<asp:Button ID="btn_submit" runat="server" Text="Submit" CssClass="btn_style" OnClick="btn_submit_Click" /></td>
                                    <td width="15%">&nbsp;<asp:Label ID="msg_products" runat="server" Text="" Font-Bold="true"></asp:Label></td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr id="AddProdcuts" runat="server" visible="false">
                    <td colspan="6">
                        <asp:Panel ID="Panel_Selector" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;Product / Servive List</td>
                                    <td width="35%">&nbsp;<asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style">
                                    </asp:DropDownList></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Button ID="btn_viewProds" runat="server" Text="View Products" CssClass="btn_style" Width="110px" OnClick="btn_viewProds_Click" OnClientClick="return ValidateField_11();" /></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr id="ProductSelector_row" runat="server" visible="false">
                                    <td width="15%" colspan="4">
                                        <span style="font-weight:bold;">Search Box (Search the below data by any keywords) : &nbsp;</span>
                                        <asp:TextBox ID="txtServiceSearch" runat="server"
                                            CssClass="textbox_U_style"
                                            Width="250px"
                                            placeholder="Search service / product..."
                                            onkeyup="debouncedPurchaseSearch()" />

                                        <asp:Button ID="btnClearServiceSearch" runat="server"
                                            Text="Clear"
                                            CssClass="btn btn-primary btn_style"
                                            OnClientClick="clearPurchaseSearch(); return false;" />

                                        <br />
                                        <span id="lblNoServiceRecords" style="color: red; display: none; font-weight: bold;">No records found
                                        </span>

                                        <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%" EnableViewState="true">
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

                                                <asp:TemplateField HeaderText="Brand Name">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
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

                                                <asp:TemplateField HeaderText="Specification">
                                                    <EditItemTemplate>
                                                        <asp:Label ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:Label>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:Label>
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
                                    <%--<td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>--%>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr id="ProductSelector_btnrow" runat="server" visible="false">
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;<asp:Button ID="btn_selector" runat="server" Text="Add Selected Products" CssClass="btn_style" Width="150px" OnClick="btn_selector_Click" /></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
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

        <Triggers>
            <asp:PostBackTrigger ControlID="btnSertch" />
            <%--<asp:PostBackTrigger ControlID="Button2" />
            <asp:PostBackTrigger ControlID="btnAddProduct" />--%>
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
