<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PR_Details.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PR_Details" %>

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

        .textbox_style21 {
            text-align: center;
        }

        .auto-style3 {
            width: 10%;
            height: 24px;
        }

        .auto-style4 {
            width: 40%;
            height: 24px;
        }

        .field-error {
            border: 2px solid #d9534f !important;
            background-color: #fff0f0;
        }

        .pr-summary {
            margin-top: 10px;
            padding: 10px;
            background: #f4f9ff;
            border: 1px solid #cfe3ff;
            font-size: 12px;
        }

        .delete-link {
            color: #d9534f;
            font-weight: bold;
        }
        .delete-link:hover {
            color: #a94442;
            text-decoration: underline;
        }

        .Grid tr {
            transition: opacity 0.2s ease-in-out;
        }

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
    <script type="text/javascript">
        var serviceSearchTimer = null;

        function debouncedSearchServiceGrid() {
            clearTimeout(serviceSearchTimer);
            serviceSearchTimer = setTimeout(function () {
                searchServiceGrid();
            }, 300);
        }

        function getRowSearchText(row) {
            var text = row.innerText || "";
            var inputs = row.querySelectorAll("input[type='text']");
            inputs.forEach(function (inp) {
                text += " " + inp.value;
            });

            return text.toLowerCase();
        }

        function searchServiceGrid() {
            var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
            var filter = input.value.trim().toLowerCase();
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.getElementsByTagName("tr");
            var matchCount = 0;

            for (var i = 1; i < rows.length; i++) { // skip header
                var row = rows[i];
                var rowText = getRowSearchText(row);

                if (filter === "" || rowText.indexOf(filter) > -1) {
                    row.style.display = "";
                    if (filter !== "") matchCount++;
                } else {
                    row.style.display = "none";
                }
            }

            document.getElementById("lblNoServiceRecords").style.display =
                (filter !== "" && matchCount === 0) ? "block" : "none";
        }

        function clearServiceGridSearch() {
            var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.getElementsByTagName("tr");

            input.value = "";

            for (var i = 1; i < rows.length; i++) {
                rows[i].style.display = "";
            }

            document.getElementById("lblNoServiceRecords").style.display = "none";
            input.focus();
        }
    </script>

    <script type="text/javascript">
        var modifiedCount = 0;

        function markRowModified(ctrl) {

            // 🚫 IGNORE description / specification edits
            if (ctrl && ctrl.id && ctrl.id.indexOf("sepecification") !== -1) {
                return;
            }

            var row = ctrl;
            while (row && row.tagName !== "TR") {
                row = row.parentNode;
            }
            if (!row) return;

            var hdn = row.querySelector("input[type='hidden'][id*='hdnIsModified']");
            if (hdn && hdn.value !== "1") {
                hdn.value = "1";
                modifiedCount++;
            }

            var badge = row.querySelector("span[data-modified='1']");
            if (badge) badge.style.display = "inline";

            row.style.backgroundColor = "#fff7cc";
            row.style.opacity = "1";

            if (typeof updateModifiedCounter === "function") {
                updateModifiedCounter();
            }
            if (typeof recalcSummary === "function") {
                recalcSummary();
            }
        }


        function updateModifiedCounter() {
            var lbl = document.getElementById("lblModifiedCount");
            if (lbl) {
                lbl.innerHTML = modifiedCount;
            }
        }


        function recalcSummary() {

            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;

            var gross = 0, discount = 0, taxable = 0, gst = 0;
            var rows = grid.getElementsByTagName("tr");

            for (var i = 1; i < rows.length; i++) {

                var row = rows[i];

                var qty = row.querySelector("[id*='Quantity']");
                var rate = row.querySelector("[id*='Vendor_rate']");
                var disc = row.querySelector("[id*='DiscountAmount']");
                var tax = row.querySelector("[id*='TaxableAmount']");
                var gstDDL = row.querySelector("[id*='vat_parsentage']");

                var radios = row.querySelectorAll("input[type='radio']");
                var taxYes = false;
                for (var r = 0; r < radios.length; r++) {
                    if (radios[r].checked && radios[r].value === "Yes") {
                        taxYes = true;
                        break;
                    }
                }

                if (!qty || !rate) continue;

                var q = parseFloat(qty.value) || 0;
                var r1 = parseFloat(rate.value) || 0;
                var d = parseFloat(disc ? disc.value : 0) || 0;
                var t = parseFloat(tax ? tax.value : 0) || 0;
                var g = parseFloat(gstDDL ? gstDDL.value : 0) || 0;

                gross += q * r1;
                discount += d;
                taxable += t;

                if (taxYes) {
                    gst += (t * g / 100);
                }
            }

            document.getElementById('<%= lblGross.ClientID %>').innerHTML = gross.toFixed(2);
            document.getElementById('<%= lblDiscount.ClientID %>').innerHTML = discount.toFixed(2);
            document.getElementById('<%= lblTaxable.ClientID %>').innerHTML = taxable.toFixed(2);
            document.getElementById('<%= lblGST.ClientID %>').innerHTML = gst.toFixed(2);
            document.getElementById('<%= lblNet.ClientID %>').innerHTML = (taxable + gst).toFixed(2);
        }


    </script>

    <script type="text/javascript">
        function showModifiedOnly() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");

            let found = false;

            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[type='hidden']");
                if (hdn && hdn.value === "1") {
                    rows[i].style.display = "";
                    found = true;
                } else {
                    rows[i].style.display = "none";
                }
            }

            document.getElementById("lblNoServiceRecords").style.display =
                found ? "none" : "inline";
        }

        function showAllRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");
            for (let i = 1; i < rows.length; i++) {
                rows[i].style.display = "";
            }

            document.getElementById("lblNoServiceRecords").style.display = "none";
        }
    </script>

    <script type="text/javascript">
        function validate1(key) {
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('txtfillrequar');
            if ((keycode == 39)) {
                return false;
            }
            else {
                return true;

            }
        }

        function ValidateDataField10() {

            var ddl = document.getElementById('<%= cmbproduct_service.ClientID %>');
            if (ddl === null) {
                alert('Product / Service control not found.');
                return false;
            }
            // Case 1: No selection
            if (ddl.selectedIndex === 0 || ddl.value === "" || ddl.value === "0") {
                alert('Please select a Product / Service before adding items.');
                ddl.focus();
                return false;
            }
            return true; // allow postback
        }

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

        function calculateDiscount(changedInput) {
            console.log("Triggered calculateDiscount");

            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.getElementsByTagName("tr");

            for (var i = 1; i < rows.length; i++) {
                if (rows[i].contains(changedInput)) {
                    var row = rows[i];

                    var rateInput = row.querySelector("input[id*='Vendor_rate']");
                    var qtyInput = row.querySelector("input[id*='Quantity']");
                    var percentInput = row.querySelector("input[id*='DiscountPercent']");
                    var amountInput = row.querySelector("input[id*='DiscountAmount']");
                    var taxableAmountInput = row.querySelector("input[id*='TaxableAmount']");

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

                    markRowModified(changedInput);
                    recalcSummary();

                    break;
                }
            }
        }

        function beforeSubmitPR() {
            if (modifiedCount > 0) {
                if (!confirm("You have " + modifiedCount +
                    " modified items. Review only modified items?")) {
                    return true;
                }
                showModifiedOnly();
                return false;
            }
            return true;
        }

    </script>

    <script type="text/javascript">
        function validateModifiedRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");

            let hasError = false;
            let firstErrorRow = null;

            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const hdn = row.querySelector("input[type='hidden']");

                // ✅ validate only modified rows
                if (!hdn || hdn.value !== "1") continue;

                const qty = row.querySelector("[id*='Quantity']");
                const rate = row.querySelector("[id*='Vendor_rate']");
                const gst = row.querySelector("[id*='vat_parsentage']");
                const order = row.querySelector("[id*='txtOrder']");

                // ---- TAX RADIO BUTTON (SAFE WAY) ----
                let taxApplicable = "";
                const radios = row.querySelectorAll("input[type='radio']");
                for (let r of radios) {
                    if (r.checked) {
                        taxApplicable = r.value;
                        break;
                    }
                }

                // ---- RESET STYLES ----
                [qty, rate, gst, order].forEach(c => {
                    if (c) c.classList.remove("field-error");
                });

                let rowHasError = false;

                // ---- QUANTITY ----
                if (!qty || qty.value.trim() === "" || Number(qty.value) <= 0) {
                    qty.classList.add("field-error");
                    rowHasError = true;
                }

                // ---- RATE ----
                if (!rate || rate.value.trim() === "" || Number(rate.value) <= 0) {
                    rate.classList.add("field-error");
                    rowHasError = true;
                }

                // ---- TAX % REQUIRED IF TAX = YES ----
                if (taxApplicable === "Yes") {
                    if (!gst || gst.value === "" || gst.value === "NA") {
                        gst.classList.add("field-error");
                        rowHasError = true;
                    }
                }

                // ---- ORDER (MANDATORY & POSITIVE) ----
                if (!order || order.value.trim() === "" || Number(order.value) <= 0) {
                    order.classList.add("field-error");
                    rowHasError = true;
                }

                if (rowHasError) {
                    hasError = true;
                    if (!firstErrorRow) firstErrorRow = row;
                }
            }

            if (hasError) {
                if (typeof showModifiedOnly === "function") {
                    showModifiedOnly(); // 🔍 focus only modified rows
                }
                if (firstErrorRow) {
                    firstErrorRow.scrollIntoView({ behavior: "smooth", block: "center" });
                }
                alert("Please complete Quantity, Rate, GST and Order only for the rows you edited.");
                return false;
            }

            if (!hasDuplicateOrders()) {
                alert("Duplicate Order numbers detected. Order must be unique.");
                return false;
            }

            return true;
        }

        function applyInactiveRowStyle() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;

            const rows = grid.getElementsByTagName("tr");

            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[type='hidden'][id*='hdnIsModified']");
                if (hdn && hdn.value !== "1") {
                    rows[i].style.opacity = "0.85";   // inactive look
                }
            }
        }

        window.onload = function () {
            applyInactiveRowStyle();
        };

        function autoAssignOrder(ctrl) {
            if (ctrl.value.trim() !== "") return;
            ctrl.value = getNextOrderValue();
            markRowModified(ctrl);
        }
    </script>

    <script type="text/javascript">
        function hasDuplicateOrders() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");

            const orderMap = {};
            let duplicatesFound = false;
            let firstDuplicateRow = null;

            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[type='hidden']");
                if (!hdn || hdn.value !== "1") continue; // only modified rows

                const orderBox = rows[i].querySelector("[id*='txtOrder']");
                if (!orderBox || orderBox.value.trim() === "") continue;

                const orderVal = orderBox.value.trim();

                orderBox.classList.remove("field-error");

                if (orderMap[orderVal]) {
                    orderBox.classList.add("field-error");
                    orderMap[orderVal].classList.add("field-error");
                    duplicatesFound = true;
                    if (!firstDuplicateRow) firstDuplicateRow = rows[i];
                } else {
                    orderMap[orderVal] = orderBox;
                }
            }

            if (duplicatesFound && firstDuplicateRow) {
                firstDuplicateRow.scrollIntoView({ behavior: "smooth", block: "center" });
            }

            return !duplicatesFound;
        }
    </script>

    <script type="text/javascript">
        function autoReorderAll() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");

            let seq = 1;
            for (let i = 1; i < rows.length; i++) {
                const orderBox = rows[i].querySelector("[id*='txtOrder']");
                if (orderBox) {
                    orderBox.value = seq++;
                    markRowModified(orderBox);
                }
            }
        }
    </script>

    <script type="text/javascript">
        function autoReorderModifiedOnly() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            const rows = grid.getElementsByTagName("tr");

            let modifiedRows = [];

            // 1️⃣ Collect modified rows only
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const hdn = row.querySelector("input[type='hidden']");
                if (hdn && hdn.value === "1") {
                    const orderBox = row.querySelector("[id*='txtOrder']");
                    if (orderBox) {
                        modifiedRows.push({
                            row: row,
                            orderBox: orderBox,
                            currentOrder: parseInt(orderBox.value) || 0
                        });
                    }
                }
            }

            if (modifiedRows.length === 0) {
                alert("No modified rows to reorder.");
                return;
            }

            // 2️⃣ Sort modified rows by current order (stable)
            modifiedRows.sort((a, b) => a.currentOrder - b.currentOrder);

            // 3️⃣ Find next available order slot
            const usedOrders = new Set();
            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[type='hidden']");
                const orderBox = rows[i].querySelector("[id*='txtOrder']");
                if (orderBox && (!hdn || hdn.value !== "1")) {
                    const val = parseInt(orderBox.value);
                    if (!isNaN(val)) usedOrders.add(val);
                }
            }

            let nextOrder = 1;
            function getNextFreeOrder() {
                while (usedOrders.has(nextOrder)) {
                    nextOrder++;
                }
                return nextOrder++;
            }

            // 4️⃣ Assign new unique orders ONLY to modified rows
            modifiedRows.forEach(item => {
                const newOrder = getNextFreeOrder();
                item.orderBox.value = newOrder;
                markRowModified(item.orderBox); // reinforce modified state
            });

            alert("Order updated for modified rows only.");
        }
    </script>

    <script type="text/javascript">
        function scrollToMessage() {
            const ok = document.getElementById('<%= PanelOK.ClientID %>');
            const err = document.getElementById('<%= PanelError.ClientID %>');

            if (ok && ok.style.display !== "none") {
                ok.scrollIntoView({ behavior: "smooth", block: "center" });
            }
            if (err && err.style.display !== "none") {
                err.scrollIntoView({ behavior: "smooth", block: "center" });
            }
        }
    </script>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="style1">
                <tr>
                    <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">Modify Purchase Requisition</span>&nbsp;</td>
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
                        <table width="100%" cellpadding="4" cellspacing="0"
                            style="border: 1px solid #006699; background-color: #F4FAFF;">
                            <tr>
                                <td style="width: 20%; font-weight: bold;">PR No</td>
                                <td style="width: 30%;">
                                    <asp:Label ID="lblReqNo" runat="server"
                                        Text="(Not Generated)"
                                        Style="font-weight: bold; color: #003366;" />
                                </td>

                                <td style="width: 20%; font-weight: bold;">Status</td>
                                <td style="width: 30%;">
                                    <asp:Label ID="lblStatus" runat="server"
                                        Text="Draft"
                                        Style="font-weight: bold; color: #CC6600;" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td>&nbsp;</td>
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
                        <asp:Label ID="lblLog" runat="server" Text="" Visible="true"></asp:Label>
                    </td>
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
                        <asp:Label ID="lbl_vendordbid" runat="server" Text="0" Visible="false"></asp:Label>
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
                <tr id="PurchaseType_Row" runat="server" visible="False">
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
                        <asp:Button ID="Button1" runat="server" Text="Modify PR" CssClass="btn_style" Visible="False" OnClick="Button1_Click" />
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
                                    <td class="auto-style3"></td>
                                    <td class="auto-style4">Product / Servive List</td>
                                    <td class="auto-style4">
                                        <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style">
                                        </asp:DropDownList>
                                    </td>
                                    <td class="auto-style3"></td>
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
                                        <asp:Button ID="Button2" runat="server" Text="Add Items" CssClass="btn_style" Width="110px" OnClick="Button2_Click" OnClientClick="return ValidateDataField10();" />
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


                <tr id="gridtable" runat="server" visible="true">
                    <td colspan="6">
                        <asp:Panel ID="Panel2" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr id="SearchBox_Row" runat="server" visible="false">
                                    <td colspan="4" style="text-align: center; padding: 10px 0;">
                                        <asp:TextBox ID="txtServiceSearch" runat="server"
                                            CssClass="textbox_U_style"
                                            Width="260px"
                                            placeholder="Search service / product..."
                                            onkeyup="debouncedSearchServiceGrid()" />

                                        &nbsp;

                                    <asp:Button ID="btnClearServiceSearch" runat="server"
                                        Text="Clear"
                                        CssClass="btn btn-primary btn_style"
                                        OnClientClick="clearServiceGridSearch(); return false;" />
                                    </td>
                                </tr>

                                <tr id="SearchBox_Msg" runat="server" visible="false">
                                    <td colspan="4" style="text-align: center; padding-bottom: 5px;">
                                        <span id="lblNoServiceRecords"
                                            style="color: red; display: none; font-weight: bold;">No records found
                                        </span>
                                    </td>
                                </tr>

                                <tr id="Modifier_Msg_Row" runat="server" visible="false">
                                    <td colspan="4" style="text-align: right; padding: 5px 10px;">
                                        <span style="font-weight: bold; color: #d9534f;">Modified Items :
                                            <span id="lblModifiedCount">0</span>
                                        </span>
                                        <span style="margin-left: 15px;">
                                            <asp:Button ID="btnShowModified" runat="server"
                                                Text="Show Modified"
                                                CssClass="btn btn-warning btn-sm btn_style"
                                                OnClientClick="showModifiedOnly(); return false;" />
                                            &nbsp; &nbsp;

                                            <asp:Button ID="btnShowAll" runat="server" Text="Show All" CssClass="btn btn-secondary btn-sm btn_style" OnClientClick="showAllRows(); return false;" />
                                        </span>

                                    </td>
                                </tr>

                                <tr id="Old_Logic" runat="server" visible="true">
                                    <td colspan="4" style="padding-top: 5px;">
                                        <%--<asp:TextBox ID="txtServiceSearch" runat="server"
                                            CssClass="textbox_U_style"
                                            Width="250px"
                                            placeholder="Search service / product..."
                                            onkeyup="debouncedSearchServiceGrid()" />
                                        <asp:Button ID="btnClearServiceSearch" runat="server"
                                            Text="Clear"
                                            CssClass="btn btn-primary btn_style"
                                            OnClientClick="clearServiceGridSearch(); return false;" />
                                        <br />
                                        <span id="lblNoServiceRecords" style="color: red; display: none; font-weight: bold;">No records found
                                        </span>--%>
                                        <%--<br />

                                        <span style="font-weight: bold; color: #d9534f;">Modified Items: <span id="lblModifiedCount">0</span>
                                        </span>
                                        <br />--%>

                                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%" OnRowDataBound="gd_Service_Product_RowDataBound" OnRowCommand="gd_Service_Product_RowCommand">
                                            <RowStyle BackColor="#94B8FF" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="Status">
                                                    <ItemTemplate>
                                                        <span data-modified="1" style="display: none; font-weight: bold; color: red;">✱</span>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Service/Product Code">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Ser_pro_code") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Eval("Ser_pro_code") %>'></asp:Label>
                                                        <asp:HiddenField ID="hdnIsModified" runat="server" Value="0" />
                                                    </ItemTemplate>

                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Service/Product Name">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:Label>
                                                        <asp:HiddenField ID="hdnParentCategoryId" runat="server" Value='<%# Eval("ParentCategoryId") %>' />
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
                                                <asp:TemplateField HeaderText="Quantity">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox9" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Quantity" runat="server" CssClass="textbox_style21" onkeyup="calculateDiscount(this)" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Vendor Rate">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Vendor_rate" runat="server" CssClass="textbox_style21" onkeyup="calculateDiscount(this)" onkeypress="return validate(event)" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Dis. %">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="DiscountPercent" runat="server" CssClass="textbox_style21"
                                                            onkeyup="calculateDiscount(this)"
                                                            BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" Width="60px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Disc. Amount">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="DiscountAmount" runat="server" CssClass="textbox_style21"
                                                            onkeyup="calculateDiscount(this)"
                                                            BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Height="22px" Width="80px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Taxable Amount">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="TaxableAmount" runat="server" CssClass="textbox_style21"
                                                            ReadOnly="true" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid"
                                                            Height="22px" Width="100px"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Tax Applicable">
                                                    <ItemTemplate>
                                                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem>No</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Input %">
                                                    <ItemTemplate>
                                                        <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="dropdown_style">
                                                        </asp:DropDownList>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Order">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtOrder" runat="server" ToolTip="Leave blank to auto-assign. Must be unique." onfocus="autoAssignOrder(this)" onkeyup="markRowModified(this)" CssClass="textbox_style21" BorderColor="#333333" BorderWidth="1px" BorderStyle="Solid" Width="50px" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Action">
                                                    <ItemTemplate>
                                                        <asp:HiddenField ID="hdnRowId" runat="server"
                                                            Value='<%# Eval("id") %>' />

                                                        <asp:LinkButton ID="lnkDelete"
                                                            runat="server"
                                                            Text="Delete"
                                                            CommandName="DeleteItem"
                                                            CommandArgument='<%# Eval("id") %>'
                                                            OnClientClick="return confirm('Delete this item?');"
                                                            CssClass="delete-link" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#CCCC99" />
                                            <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                            <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                            <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                        </asp:GridView>
                                        <br />
                                        <asp:Panel ID="pnlSummary" runat="server" CssClass="pr-summary">
                                            <table width="100%">
                                                <tr>
                                                    <td align="right">Gross Amount :</td>
                                                    <td>
                                                        <asp:Label ID="lblGross" runat="server" /></td>

                                                    <td align="right">Discount :</td>
                                                    <td>
                                                        <asp:Label ID="lblDiscount" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td align="right">Taxable Amount :</td>
                                                    <td>
                                                        <asp:Label ID="lblTaxable" runat="server" /></td>

                                                    <td align="right">GST Amount :</td>
                                                    <td>
                                                        <asp:Label ID="lblGST" runat="server" /></td>
                                                </tr>
                                                <tr style="font-weight: bold">
                                                    <td align="right">Net Amount :</td>
                                                    <td>
                                                        <asp:Label ID="lblNet" runat="server" /></td>
                                                    <td></td>
                                                    <td></td>
                                                </tr>
                                            </table>

                                        </asp:Panel>

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
                                    <td>&nbsp;</td>
                                    <td colspan="4" style="text-align: center; padding: 10px;">

                                        <asp:Button ID="btnReorder" runat="server"
                                            Text="Auto Reorder"
                                            CssClass="btn btn-info btn-sm btn_style"
                                            OnClientClick="autoReorderModifiedOnly(); return false;" />

                                        &nbsp;&nbsp;
                                        <asp:Button ID="btnSaveDraft" runat="server"
                                            Text="Save Draft"
                                            CssClass="btn btn-warning btn_style"
                                            OnClientClick="return validateModifiedRows();"
                                            OnClick="btnSaveEdit_Click" />

                                        &nbsp;&nbsp;

                                        <asp:Button ID="Button3" runat="server"
                                            Text="Submit PR"
                                            CssClass="btn btn-success btn_style"
                                            OnClientClick="return validateModifiedRows();"
                                            OnClick="Button3_Click" />

                                        &nbsp; &nbsp;

                                        <asp:Button ID="btnCancelPR" runat="server"
                                            Text="Cancel PR"
                                            CssClass="btn btn-danger btn_style"
                                            OnClick="btnCancelPR_Click" />
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

                        <asp:Panel ID="pnlApproval" runat="server" Visible="false" CssClass="approval-box">

                            <h4>Approval Action</h4>

                            <asp:TextBox ID="txtApprovalRemarks" runat="server"
                                TextMode="MultiLine"
                                Width="400px"
                                Height="80px"
                                placeholder="Remarks (optional)" />

                            <br />
                            <br />

                            <asp:Button ID="btnApprove" runat="server"
                                Text="Approve"
                                CssClass="btn btn-success btn_style"
                                OnClick="btnApprove_Click" />

                            <asp:Button ID="btnReject" runat="server"
                                Text="Reject"
                                CssClass="btn btn-danger btn_style"
                                OnClick="btnReject_Click" />

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
