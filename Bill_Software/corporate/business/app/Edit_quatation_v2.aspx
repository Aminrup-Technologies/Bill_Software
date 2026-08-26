<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Edit_quatation_v2.aspx.cs" Inherits="Bill_Software.corporate.business.app.Edit_quatation_v2" %>

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
            font-size: 11px;
            line-height: 1.5;
            border: 1px solid #2D2D2D;
            padding: 4px;
        }

        .center {
            text-align: center;
        }

        .wizard-steps {
            margin-bottom: 20px;
            padding: 10px;
            background-color: #f4f4f4;
            border-radius: 5px;
            text-align: center;
            font-weight: bold;
            color: #19658A;
            font-size: 16px;
            border: 1px solid #ccc;
        }

        /* Select2 CSS Fixes for Whiteout Issue */
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            color: #333 !important;
            line-height: 26px !important;
            font-weight: bold;
        }

        .select2-container .select2-selection--single {
            height: 30px !important;
            border: 1px solid #aaa !important;
        }

        .select2-results__option {
            color: #333 !important;
        }

        /* Scrollable Grid CSS */
        .scrollable-grid {
            max-height: 400px;
            overflow-y: auto;
            border: 1px solid #ccc;
        }

            .scrollable-grid th {
                position: sticky;
                top: 0;
                background-color: #006699;
                color: white;
                z-index: 10;
                box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.4);
            }

        /* Footer styling for totals */
        .grid-footer td {
            font-weight: bold;
            background-color: #e9ecef;
            color: #333;
            font-size: 12px;
        }

        /* Horizontal Scroll & Frozen Column CSS */
        .cart-grid-wrapper {
            width: 100%;
            max-height: 500px;
            overflow-x: auto;
            overflow-y: auto;
            border: 1px solid #ccc;
        }

            .cart-grid-wrapper table {
                width: 100%;
                min-width: 1800px; /* Forces horizontal scrolling */
                border-collapse: separate;
                border-spacing: 0;
            }

            .cart-grid-wrapper th {
                position: sticky;
                top: 0;
                background-color: #006699;
                color: white;
                z-index: 20; /* Keep headers above everything */
                box-shadow: 0 2px 2px -1px rgba(0,0,0,0.4);
                white-space: nowrap;
                padding: 8px;
            }

            .cart-grid-wrapper td {
                white-space: nowrap;
                background-color: inherit;
            }

        /* Freeze Left Identifiers */
        .col-frozen-action {
            position: sticky;
            left: 0;
            z-index: 10;
            background-color: #fff;
            width: 80px;
            min-width: 80px;
            text-align: center;
        }

        .col-frozen-sl {
            position: sticky;
            left: 80px;
            z-index: 10;
            background-color: #fff;
            width: 40px;
            min-width: 40px;
            text-align: center;
        }

        .col-frozen-name {
            position: sticky;
            left: 120px;
            z-index: 10;
            background-color: #fff;
            border-right: 2px solid #444;
            width: 220px;
            min-width: 220px;
            max-width: 220px;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        /* Ensure Header/Footer Z-index overrides for frozen columns */
        .cart-grid-wrapper th.col-frozen-action,
        .cart-grid-wrapper th.col-frozen-sl,
        .cart-grid-wrapper th.col-frozen-name {
            z-index: 30;
        }

        .grid-footer td.col-frozen-action,
        .grid-footer td.col-frozen-sl,
        .grid-footer td.col-frozen-name {
            z-index: 30;
            background-color: #e9ecef;
        }

        /* Highlight editable zone */
        .col-editable {
            background-color: #fbfcff;
        }

        .action-btn {
            text-decoration: none;
            padding: 2px 5px;
            background: #eee;
            border: 1px solid #ccc;
            color: #333;
            font-size: 12px;
            border-radius: 3px;
            margin: 0 2px;
        }

            .action-btn:hover {
                background: #ddd;
                color: black;
            }

        .action-del {
            background: #ffcccc;
            border-color: #ff9999;
            color: red;
        }

            .action-del:hover {
                background: #ff9999;
                color: darkred;
            }

        .table1, .table2 {
            border-collapse: collapse;
            width: 100%;
        }

            .table1 td, .table2 td {
                text-align: left;
                border: 1px solid #666666;
                padding: 5px;
            }

            .table2 td {
                border-top: none;
            }
    </style>

    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/css/select2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>

    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
            $('#<%= cmbvendor.ClientID %>').select2({ placeholder: "Search Vendor..." });
            $('#<%= cmbClient.ClientID %>').select2({ placeholder: "Search Client..." });
            $('#<%= ddlPlaceOfSupply.ClientID %>').select2({ placeholder: "Search Place of Supply..." });
            $('#<%= cmbproduct_service.ClientID %>').select2({ placeholder: "Search Category..." });

            calculateCart();
            togglePanel();
        });

        // ----- CATALOG FILTER -----
        function filterCatalog() {
            var searchTxt = document.getElementById('txtCatalogSearch').value.toLowerCase();
            var filterMode = document.querySelector('input[name="catFilter"]:checked').value;
            var grid = document.getElementById('<%= gridProdWithCat.ClientID %>');
            if (!grid) return;
            var rows = grid.getElementsByTagName('tr');

            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];
                var cb = row.querySelector("input[type='checkbox']");
                if (!cb) continue;

                var textMatch = row.innerText.toLowerCase().indexOf(searchTxt) > -1;
                var stateMatch = (filterMode === 'all') || (filterMode === 'selected' && cb.checked) || (filterMode === 'unselected' && !cb.checked);
                row.style.display = (textMatch && stateMatch) ? '' : 'none';
            }
        }

        // ----- CART CALCULATIONS & VALIDATION -----
        function calculateCart() {
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            var rows = grid.getElementsByTagName('tr');
            var totQty = 0, totTaxable = 0, totTax = 0, totNet = 0;
            var sl = 1;

            for (var i = 1; i < rows.length - 1; i++) {
                var row = rows[i];
                if (row.className.indexOf('grid-footer') > -1) continue;

                var slInput = row.querySelector('.sl-input');
                if (slInput) slInput.value = sl++;

                var qtyInput = row.querySelector('.qty-input');
                var rateInput = row.querySelector('.rate-input');
                if (!qtyInput || !rateInput) continue;

                var qty = parseFloat(qtyInput.value) || 0;
                var rate = parseFloat(rateInput.value) || 0;
                var disc = parseFloat(row.querySelector('.disc-input').value) || 0;
                var taxRate = parseFloat(row.querySelector('.tax-lbl').innerText) || 0;

                var baseAmt = rate * qty;
                var discAmt = baseAmt * (disc / 100);
                var taxable = baseAmt - discAmt;
                var taxAmt = taxable * (taxRate / 100);
                var netAmt = taxable + taxAmt;

                row.querySelector('.lbl-taxable').innerText = taxable.toFixed(2);
                row.querySelector('.lbl-taxamt').innerText = taxAmt.toFixed(2);
                row.querySelector('.lbl-net').innerText = netAmt.toFixed(2);

                totQty += qty;
                totTaxable += taxable;
                totTax += taxAmt;
                totNet += netAmt;
            }

            var ftrQty = document.getElementById('ftr-qty');
            if (ftrQty) {
                ftrQty.innerText = totQty;
                document.getElementById('ftr-taxable').innerText = totTaxable.toFixed(2);
                document.getElementById('ftr-tax').innerText = totTax.toFixed(2);
                document.getElementById('ftr-net').innerText = totNet.toFixed(2);
            }
        }

        function validateCart() {
            calculateCart();
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid || grid.rows.length <= 2) { alert("Cart is empty!"); return false; }

            var rows = grid.getElementsByTagName('tr');
            for (var i = 1; i < rows.length - 1; i++) {
                var qtyInput = rows[i].querySelector('.qty-input');
                if (!qtyInput) continue; // Skip header/footer

                var qty = parseFloat(qtyInput.value) || 0;
                var rate = parseFloat(rows[i].querySelector('.rate-input').value) || 0;
                if (qty <= 0 || rate <= 0) {
                    alert("Error on Row " + i + ": Quantity and Base Rate must be greater than 0.");
                    rows[i].style.backgroundColor = "#ffcccc";
                    return false;
                } else {
                    rows[i].style.backgroundColor = "";
                }
            }
            return true;
        }

        // ----- WIZARD STEP 4 FINAL VALIDATION -----
        function validateButtonClick() {
            var grid = document.getElementById('<%= GridView3.ClientID %>');
            var rows = grid ? grid.getElementsByTagName('tr') : [];
            var totalPercent = 0;

            for (var i = 1; i < rows.length; i++) {
                var amountPerInput = rows[i].querySelector('input[type="text"]');
                if (amountPerInput) {
                    var amount = parseFloat(amountPerInput.value);
                    if (isNaN(amount) || amount < 0 || amount > 100) {
                        alert("Please enter a valid Payment Percentage (0-100) for each phase.");
                        amountPerInput.focus();
                        return false;
                    }
                    totalPercent += amount;
                }
            }

            if (rows.length > 1 && totalPercent !== 100) {
                if (!confirm("Warning: Your Payment Phases total " + totalPercent + "%, not 100%. Do you want to proceed anyway?")) {
                    return false;
                }
            }

            var ddlItemView = document.getElementById('<%= DDL_ItemViewType.ClientID %>');
            if (ddlItemView && ddlItemView.value === "0") {
                alert("Please select a Particulars View type.");
                ddlItemView.focus(); return false;
            }

            var ddlDeliveryTerms = document.getElementById('<%= DDL_DeliveryTerms.ClientID %>');
            if (ddlDeliveryTerms && ddlDeliveryTerms.value === "4") {
                var manualInput = document.getElementById('<%= txt_deltrms.ClientID %>');
                if (manualInput && manualInput.value.trim() === "") {
                    alert("Please enter manual delivery tenure.");
                    manualInput.focus(); return false;
                }
            }

            var ddlPkgFrwd = document.getElementById('<%= DDL_pkgfrwd.ClientID %>');
            if (ddlPkgFrwd && ddlPkgFrwd.value === "3") {
                var pkgInput = document.getElementById('<%= txt_pkgfrwd.ClientID %>');
                if (pkgInput && pkgInput.value.trim() === "") {
                    alert("Please enter manual package forwarding details.");
                    pkgInput.focus(); return false;
                }
            }

            var elOtherAmt = document.getElementById('<%= txt_othr_amnt.ClientID %>');
            var elOtherName = document.getElementById('<%= TextBox1.ClientID %>');
            if (elOtherAmt && parseFloat(elOtherAmt.value) > 0) {
                if (elOtherName && elOtherName.value.trim() === "") {
                    alert("You entered an Other Charges Amount. Please provide a Name for this charge.");
                    elOtherName.focus(); return false;
                }
            }

            return true;
        }

        // ----- UI HELPERS -----
        function toggleReferenceFields(value) {
            document.getElementById('<%= hdnRefOption.ClientID %>').value = value;
            var nameField = document.getElementById('<%= txt_clientrefname.ClientID %>');
            var idField = document.getElementById('<%= txt_clientrefid.ClientID %>');
            var dateField = document.getElementById('<%= txt_clientrefdate.ClientID %>');
            if (value === 'Yes') {
                nameField.readOnly = false; idField.readOnly = false; dateField.readOnly = false;
            } else {
                nameField.value = "N/A"; idField.value = "N/A"; dateField.value = "01-Jan-2000";
                nameField.readOnly = true; idField.readOnly = true; dateField.readOnly = true;
            }
        }

        function togglePanel() {
            var rbQt = document.getElementById('<%= rbQt.ClientID %>');
            var panel = document.getElementById('<%= PO_DataInputs.ClientID %>');

            if (rbQt && rbQt.checked) {
                if (panel) panel.style.display = 'none';
            } else {
                if (panel) panel.style.display = 'block';
            }
        }

        function handleDeliveryTermChange(dropdown) {
            var manualInputRow = document.getElementById('<%= manualInputRow.ClientID %>');
            if (manualInputRow) {
                if (dropdown.value == "4") { manualInputRow.style.display = "table-row"; }
                else { manualInputRow.style.display = "none"; }
            }
        }

        function handlePackageForwardingChange(dropdown) {
            var manualInputPkgRow = document.getElementById('<%= manualInputPkgRow.ClientID %>');
            if (manualInputPkgRow) {
                if (dropdown.value == "3") { manualInputPkgRow.style.display = "table-row"; }
                else { manualInputPkgRow.style.display = "none"; }
            }
        }

        function onlyNumberDecimal(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if (charCode === 8 || charCode === 9 || charCode === 37 || charCode === 39 || charCode === 46) return true;
            var input = evt.target || evt.srcElement;
            var ch = String.fromCharCode(charCode);
            if (/[0-9]/.test(ch)) return true;
            if (ch === '.' && input.value.indexOf('.') === -1) return true;
            return false;
        }

        function Check_Click(objRef) { filterCatalog(); }
        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) { inputList[i].checked = objRef.checked; }
            }
            filterCatalog();
        }

        function ValidateDelete1() {
            return confirm("Want to Delete this Quotation?");
        }
    </script>

    <asp:HiddenField ID="hdnRefOption" runat="server" Value="No" />

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <table cellpadding="1" cellspacing="1" class="auto-style1">
                <tr>
                    <td colspan="4" bgcolor="#19658A"><span class="style2">&nbsp;Edit Quotation / Client Purchase Order Wizard</span></td>
                </tr>
                <tr>
                    <td colspan="4">
                        <asp:Panel ID="PanelGlobalAlert" runat="server" Visible="False" Style="margin: 15px 0; padding: 10px; border-radius: 5px; border: 1px solid;">
                            <asp:Label ID="lblGlobalAlert" runat="server" Font-Bold="True"></asp:Label>
                        </asp:Panel>

                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
                        <asp:Label ID="lblqno" runat="server" Visible="False"></asp:Label>
                    </td>
                </tr>
            </table>

            <asp:MultiView ID="WizardMultiView" runat="server" ActiveViewIndex="0">

                <asp:View ID="View0_Search" runat="server">
                    <div class="wizard-steps">Step 0: Search & Select Record to Edit</div>
                    <table cellpadding="3" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="20%" align="right">Client Name:</td>
                            <td width="30%">
                                <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style" Width="100%"></asp:DropDownList></td>
                            <td width="20%" align="right">Search Type:</td>
                            <td width="30%">
                                <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem>Only Client</asp:ListItem>
                                    <asp:ListItem Selected="True">Only Date</asp:ListItem>
                                    <asp:ListItem>Client & Date</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">From Date:</td>
                            <td>
                                <asp:TextBox ID="txtfromDate" runat="server" CssClass="textbox_style datepicker" Width="110px"></asp:TextBox></td>
                            <td align="right">To Date:</td>
                            <td>
                                <asp:TextBox ID="txttodate" runat="server" CssClass="textbox_style datepicker" Width="110px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td colspan="4" style="text-align: center; padding-top: 15px;">
                                <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" Text="🔍 Search Records" OnClick="btnSertch_Click" Width="200px" />
                                &nbsp;&nbsp;&nbsp;
                                <asp:Button ID="btnreset" runat="server" CssClass="btn_style" Text="Reset Search" OnClick="btnreset_Click" Width="150px" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4" style="padding-top: 20px;">
                                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Width="100%" OnItemCommand="DataList1_ItemCommand">
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingItemStyle BackColor="#94B8FF" />
                                    <HeaderTemplate>
                                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                            <tr>
                                                <td style="text-align: center; width: 25%;">Client Name</td>
                                                <td style="text-align: center; width: 15%;">Date</td>
                                                <td style="text-align: center; width: 15%;">Record Number</td>
                                                <td style="text-align: center; width: 15%;">Amount (INR)</td>
                                                <td style="text-align: center; width: 10%;">Type</td>
                                                <td style="text-align: center; width: 10%;">View</td>
                                                <td style="text-align: center; width: 10%;">Edit</td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                            <tr>
                                                <td style="text-align: center; width: 25%;">
                                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label></td>
                                                <td style="text-align: center; width: 15%;">
                                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label></td>
                                                <td style="text-align: center; width: 15%; font-weight: bold;">
                                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label></td>
                                                <td style="text-align: center; width: 15%;">Rs.
                                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>/-</td>
                                                <td style="text-align: center; width: 10%;">
                                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("RecordType") %>'></asp:Label></td>
                                                <td style="text-align: center; width: 10%;">
                                                    <a href="#" title="Print Document..." onclick="window.open('/corporate/business/print/NewQuotation.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return false;">
                                                        <img alt="View" height="20px" src="../WebImages/viewicon.png" />
                                                    </a>
                                                </td>
                                                <td style="text-align: center; width: 10%;">
                                                    <asp:Button ID="btnEditRec" runat="server" CommandName="Select" CommandArgument='<%# Eval("Quotation_no") %>' Text="Load Data" CssClass="btn_style" Style="padding: 2px 10px;" />
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>
                                </asp:DataList>
                            </td>
                        </tr>
                    </table>
                </asp:View>

                <asp:View ID="View1_BasicDetails" runat="server">
                    <div class="wizard-steps">Step 1 of 4: Document & Client Details</div>
                    <div style="background: #eef; padding: 10px; border: 1px solid #ccc; text-align: center; font-size: 16px; margin-bottom: 15px;">
                        Editing Record: <b>
                            <asp:Label ID="lbl_recordno" runat="server" ForeColor="DarkRed"></asp:Label></b>
                    </div>

                    <table cellpadding="3" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="20%" align="right"><span style="color: red">*</span> Select Client:</td>
                            <td width="30%">
                                <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style" Width="100%"></asp:DropDownList></td>
                            <td width="20%" align="right"><span style="color: red">*</span> Place Of Supply:</td>
                            <td width="30%">
                                <asp:DropDownList ID="ddlPlaceOfSupply" runat="server" CssClass="dropdown_style" Width="100%"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td align="right">Enable Reference Details:</td>
                            <td>
                                <asp:RadioButton ID="rbYes" runat="server" GroupName="referenceOption" Text="Yes" Checked="true" onclick="toggleReferenceFields('Yes')" />
                                <asp:RadioButton ID="rbNo" runat="server" GroupName="referenceOption" Text="No" onclick="toggleReferenceFields('No')" />
                            </td>
                            <td align="right"><span style="color: red">*</span> Document Date:</td>
                            <td>
                                <asp:TextBox ID="txtquotationDate" runat="server" CssClass="textbox_style datepicker" Width="110px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">Reference Name:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefname" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                            <td align="right">Reference ID:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefid" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">Reference Date:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefdate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                            <td align="right">GST Type:</td>
                            <td>
                                <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonGst_SelectedIndexChanged">
                                    <asp:ListItem Value="1" Selected="True"> CGST/SGST </asp:ListItem>
                                    <asp:ListItem Value="0"> IGST </asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Record Type:</td>
                            <td colspan="3">
                                <asp:RadioButton ID="rbQt" runat="server" GroupName="recordOption" Text="Quotation" Checked="true" onclick="togglePanel()" />&nbsp;&nbsp;
                                <asp:RadioButton ID="rbPo" runat="server" GroupName="recordOption" Text="Purchase Order" onclick="togglePanel()" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <asp:Panel ID="PO_DataInputs" runat="server" Visible="true" Style="background: #f9f9f9; padding: 10px; border: 1px solid #ddd; margin-top: 10px;">
                                    <table width="100%">
                                        <tr>
                                            <td width="25%" align="right">Delivery Order No:</td>
                                            <td width="25%">
                                                <asp:TextBox ID="txb_donumber" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                                            <td width="25%" align="right">Ref. Contract No:</td>
                                            <td width="25%">
                                                <asp:TextBox ID="txb_ponumber" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                                        </tr>
                                        <tr>
                                            <td align="right">PO Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_podate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                                            <td align="right">Val. Start Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_strtdt" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                                        </tr>
                                        <tr>
                                            <td align="right">Val. End Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_enddt" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                                            <td colspan="2"></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                    <div style="text-align: center; margin-top: 20px;">
                        <asp:Button ID="btnCancelEdit" runat="server" Text="✖ Cancel Edit" CssClass="btn_style" Width="150px" OnClick="btnreset_Click" CausesValidation="false" />
                        &nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnNext1" runat="server" Text="Next: Review Catalog/Cart ➔" CssClass="btn_style" Width="250px" OnClientClick="if(document.getElementById('ContentPlaceHolder1_cmbClient').value == ''){alert('Select Client.'); return false;} return true;" OnClick="btnNext1_Click" />
                    </div>
                </asp:View>

                <asp:View ID="View2_Catalog" runat="server">
                    <div class="wizard-steps">Step 2 of 4: Browse & Add More Products</div>

                    <table width="100%" cellpadding="5">
                        <tr>
                            <td width="15%" align="right"><b>Category:</b></td>
                            <td width="35%">
                                <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style" Width="100%"></asp:DropDownList></td>
                            <td width="15%">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" Text="Load Items" OnClick="Button2_Click" /></td>
                            <td width="35%" align="right">
                                <b>Search:</b>
                                <input type="text" id="txtCatalogSearch" onkeyup="filterCatalog()" class="textbox_style" placeholder="Type to filter..." style="width: 150px;" />
                            </td>
                        </tr>
                    </table>

                    <asp:Panel ID="PanelCatalogGrid" runat="server" Visible="False">
                        <div style="background: #eef; padding: 5px; text-align: center; border: 1px solid #ccc; margin-bottom: 5px;">
                            <b>Filter View: </b>
                            <input type="radio" name="catFilter" value="all" checked="checked" onclick="filterCatalog()">
                            All Items &nbsp;&nbsp;
                            <input type="radio" name="catFilter" value="selected" onclick="filterCatalog()">
                            Selected Only &nbsp;&nbsp;
                            <input type="radio" name="catFilter" value="unselected" onclick="filterCatalog()">
                            Unselected Only
                        </div>

                        <div class="scrollable-grid">
                            <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" CellPadding="4" CssClass="Grid" Width="100%">
                                <RowStyle BackColor="#94B8FF" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Select" HeaderStyle-Width="5%">
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="ID">
                                        <ItemTemplate>
                                            <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="HSN">
                                        <ItemTemplate>
                                            <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Name">
                                        <ItemTemplate>
                                            <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Brand">
                                        <ItemTemplate>
                                            <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Unit">
                                        <ItemTemplate>
                                            <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Rate">
                                        <ItemTemplate>
                                            <asp:Label ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="GST %">
                                        <ItemTemplate>
                                            <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField Visible="false">
                                        <ItemTemplate>
                                            <asp:Label ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:Label>
                                            <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>

                    <div style="margin-top: 15px; text-align: center;">
                        <asp:Button ID="btnPrev2" runat="server" Text="🡄 Back to Details" CssClass="btn_style" Width="150px" OnClick="btnPrev2_Click" />
                        &nbsp;
                        <asp:Button ID="btnNext2" runat="server" Text="Add to Cart ➔" CssClass="btn_style" Width="200px" OnClick="btnNext2_Click" />
                        <br />
                        <br />
                        <asp:Button ID="btnSkipCatalog" runat="server" Text="Skip Catalog (Go to Cart)" CssClass="btn_style" BackColor="#6c757d" ForeColor="White" Width="250px" OnClick="btnNext2_Click" CausesValidation="false" />
                    </div>
                </asp:View>

                <asp:View ID="View3_Cart" runat="server">
                    <div class="wizard-steps">Step 3 of 4: Review Cart & Calculations</div>
                    <div style="text-align: right; margin-bottom: 5px;">
                        <asp:Button ID="btnAddMoreProducts" runat="server" Text="+ Add More Products" CssClass="btn_style" Width="180px" BackColor="#17a2b8" ForeColor="White" OnClick="btnAddMoreProducts_Click" />
                    </div>

                    <div class="cart-grid-wrapper">
                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" ShowFooter="true" OnRowCommand="gd_Service_Product_RowCommand">
                            <RowStyle BackColor="White" />
                            <FooterStyle CssClass="grid-footer" />
                            <Columns>
                                <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="col-frozen-action" ItemStyle-CssClass="col-frozen-action" FooterStyle-CssClass="col-frozen-action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn" ToolTip="Move Up">↑</asp:LinkButton>
                                        <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn" ToolTip="Move Down">↓</asp:LinkButton>
                                        <asp:LinkButton ID="btnDel" runat="server" CommandName="DeleteRow" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn action-del" ToolTip="Remove">X</asp:LinkButton>
                                    </ItemTemplate>
                                    <FooterTemplate><b>TOTAL:</b></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Sel" Visible="false">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chk" runat="server" Checked="true" />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="SL" HeaderStyle-CssClass="col-frozen-sl" ItemStyle-CssClass="col-frozen-sl" FooterStyle-CssClass="col-frozen-sl">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtOrder" runat="server" Width="100%" CssClass="center textbox_style sl-input" ReadOnly="true" Text='<%# Bind("Sl_no") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Product Name" HeaderStyle-CssClass="col-frozen-name" ItemStyle-CssClass="col-frozen-name" FooterStyle-CssClass="col-frozen-name">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Qty" ItemStyle-CssClass="col-editable" HeaderStyle-Width="6%">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>' CssClass="center textbox_style qty-input" Width="60px" onkeyup="calculateCart()" onchange="calculateCart()"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate><span id="ftr-qty">0</span></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Unit">
                                    <ItemTemplate>
                                        <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Rate" ItemStyle-CssClass="col-editable" HeaderStyle-Width="8%">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="center textbox_style rate-input" Width="80px" onkeyup="calculateCart()" onchange="calculateCart()"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Disc %" ItemStyle-CssClass="col-editable" HeaderStyle-Width="5%">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Discount_Rate" runat="server" Text='<%# Bind("Discount_Rate") %>' CssClass="center textbox_style disc-input" Width="50px" onkeyup="calculateCart()" onchange="calculateCart()"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="GST %">
                                    <ItemTemplate>
                                        <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>' CssClass="tax-lbl"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Taxable Amt">
                                    <ItemTemplate><span class="lbl-taxable">0.00</span></ItemTemplate>
                                    <FooterTemplate><span id="ftr-taxable">0.00</span></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Tax Amt">
                                    <ItemTemplate><span class="lbl-taxamt">0.00</span></ItemTemplate>
                                    <FooterTemplate><span id="ftr-tax">0.00</span></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Net Amt">
                                    <ItemTemplate><strong class="lbl-net">0.00</strong></ItemTemplate>
                                    <FooterTemplate><span id="ftr-net" style="font-size: 14px; color: darkgreen;">0.00</span></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Make/Brand" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Brand" runat="server" CssClass="textbox_style" Width="120px" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Specification" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Specification" runat="server" CssClass="textbox_style" Width="150px" Text='<%# Bind("Specification") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Pack" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="PackSize" runat="server" CssClass="textbox_style" Width="60px" Text='<%# Bind("PackSize") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Item No" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="ItemNo" runat="server" CssClass="textbox_style" Width="80px" Text='<%# Bind("ItemNo") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Material No" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="MaterialNo" runat="server" CssClass="textbox_style" Width="80px" Text='<%# Bind("MaterialNo") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Remarks" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="ItemRemarks" runat="server" CssClass="textbox_style" Width="150px" Text='<%# Bind("ItemRemarks") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="PRD ID">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="HSN Code">
                                    <ItemTemplate>
                                        <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Category">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Type">
                                    <ItemTemplate>
                                        <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Del. Date" Visible="false" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DeliveryDate" runat="server" CssClass="datepicker textbox_style" Width="90px" Text='<%# Bind("DeliveryDate") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Dept" Visible="false" ItemStyle-CssClass="col-editable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Department" runat="server" CssClass="textbox_style" Width="80px" Text='<%# Bind("Department") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div style="margin-top: 15px; text-align: center;">
                        <asp:Button ID="btnPrev3" runat="server" Text="🡄 Back" CssClass="btn_style" Width="120px" OnClick="btnPrev3_Click" CausesValidation="false" />
                        &nbsp;
                        <asp:Button ID="btnNext3" runat="server" Text="Proceed to Terms ➔" CssClass="btn_style" Width="200px" OnClientClick="return validateCart();" OnClick="btnNext3_Click" />
                    </div>
                </asp:View>

                <asp:View ID="View4_Terms" runat="server">
                    <div class="wizard-steps">Step 4 of 4: Commercial Terms & Finalization</div>
                    <table cellpadding="5" cellspacing="2" class="auto-style1">
                        <tr>
                            <td colspan="4">
                                <table width="100%">
                                    <tr>
                                        <td width="30%" style="font-weight: bold;">Select Payment Phases<br />
                                            <small>(Select items from list)</small></td>
                                        <td width="30%">
                                            <asp:ListBox ID="listPhaseType" runat="server" SelectionMode="Multiple" Rows="7" Width="100%" AutoPostBack="True" OnTextChanged="listPhaseType_TextChanged"></asp:ListBox>
                                        </td>
                                        <td width="40%">
                                            <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" BorderWidth="1px" CellPadding="3" Width="100%" OnRowDeleting="GridView3_RowDeleting">
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Payment %">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="AmountPer" runat="server" AutoPostBack="true" Text='<%# Bind("AmountPer") %>' Width="80%" CssClass="textbox_style" OnTextChanged="AmountPer_TextChanged"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Phase Term">
                                                        <ItemTemplate>
                                                            <asp:Label ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Description">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' Width="90%" TextMode="MultiLine" CssClass="textbox_style"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:CommandField ShowDeleteButton="True" />
                                                </Columns>
                                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                            </asp:GridView>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <hr />
                            </td>
                        </tr>
                        <tr>
                            <td width="20%" align="right">Validity Days:</td>
                            <td width="30%">
                                <asp:TextBox ID="txt_valdays" runat="server" Text="0" CssClass="textbox_style" TextMode="Number"></asp:TextBox></td>
                            <td width="20%" align="right">Particulars View:</td>
                            <td width="30%">
                                <asp:DropDownList ID="DDL_ItemViewType" runat="server" CssClass="dropdown_style">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Simple" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Detailed" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Delivery Tenure (Weeks):</td>
                            <td>
                                <asp:DropDownList ID="DDL_DeliveryTerms" runat="server" CssClass="dropdown_style" onchange="handleDeliveryTermChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="10-12" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="3-4" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="1-2" Value="3"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="4"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="right">Package Forwarding:</td>
                            <td>
                                <asp:DropDownList ID="DDL_pkgfrwd" runat="server" CssClass="dropdown_style" onchange="handlePackageForwardingChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="NILL" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="At Actuals" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="3"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="manualInputRow" runat="server" style="display: none;">
                            <td align="right">Manual Tenure:</td>
                            <td>
                                <asp:TextBox ID="txt_deltrms" runat="server" Text="" CssClass="textbox_style"></asp:TextBox></td>
                            <td colspan="2"></td>
                        </tr>
                        <tr id="manualInputPkgRow" runat="server" style="display: none;">
                            <td colspan="2"></td>
                            <td align="right">Manual Package Info:</td>
                            <td>
                                <asp:TextBox ID="txt_pkgfrwd" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">Discount Visibility:</td>
                            <td>
                                <asp:DropDownList ID="DDL_DiscountView" runat="server" CssClass="dropdown_style">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="No" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="right">TCS Amount:</td>
                            <td>
                                <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_style" Width="80px" Text="0" onkeypress="return onlyNumberDecimal(event)"></asp:TextBox>
                                @
                                <asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_style" Width="40px" Text="0" onkeypress="return onlyNumberDecimal(event)"></asp:TextBox>%
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Freight Charges:</td>
                            <td>
                                <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_style" Width="80px" Text="0" onkeypress="return onlyNumberDecimal(event)"></asp:TextBox>
                                @
                                <asp:TextBox ID="txt_freight_percent" runat="server" CssClass="textbox_style" Width="40px" Text="0" onkeypress="return onlyNumberDecimal(event)"></asp:TextBox>%
                            </td>
                            <td align="right">Other Charges:</td>
                            <td>Name:
                                <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_style" Width="100px"></asp:TextBox>
                                Amt:
                                <asp:TextBox ID="txt_othr_amnt" runat="server" CssClass="textbox_style" Width="60px" Text="0" onkeypress="return onlyNumberDecimal(event)"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="right" valign="top">Remarks / Comments:</td>
                            <td colspan="3">
                                <asp:TextBox ID="txt_remarks" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="3" Width="90%"></asp:TextBox></td>
                        </tr>
                    </table>

                    <div style="display: none;">
                        <asp:GridView ID="gridps" runat="server" AutoGenerateColumns="False">
                            <Columns>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Label ID="ProductCatagory" runat="server" Text='<%# Bind("ProductCatagory") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div style="margin-top: 15px; text-align: center;">
                        <asp:Button ID="btnPrev4" runat="server" Text="🡄 Back to Cart" CssClass="btn_style" Width="150px" OnClick="btnPrev4_Click" CausesValidation="false" />
                        &nbsp;
                        <br />
                        <br />
                        <asp:Button ID="btnSabe" runat="server" CssClass="btn_style" Text="💾 Update Existing Version" OnClientClick="return validateButtonClick();" OnClick="btnSabe_Click" Width="250px" BackColor="#28a745" ForeColor="White" Font-Bold="true" />
                        &nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnNew" runat="server" CssClass="btn_style" Text="📄 Save as New Version" OnClientClick="return validateButtonClick();" OnClick="btnNew_Click" Width="250px" BackColor="#17a2b8" ForeColor="White" Font-Bold="true" />
                    </div>
                </asp:View>

            </asp:MultiView>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
