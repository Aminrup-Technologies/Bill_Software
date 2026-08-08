<%@ Page Title="Create Tax Invoice (From Source)" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Add_invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm26" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f7f6;
        }

        .section-header {
            background-color: #19658A;
            color: white;
            padding: 12px 15px;
            font-weight: bold;
            font-size: 16px;
            margin-bottom: 20px;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .box-panel {
            border: 1px solid #d1d9e0;
            border-radius: 6px;
            padding: 20px;
            background: #ffffff;
            margin-bottom: 20px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.02);
        }

        .box-title {
            margin-top: 0;
            font-size: 15px;
            color: #006699;
            border-bottom: 2px solid #f0f4f8;
            padding-bottom: 8px;
            margin-bottom: 18px;
            font-weight: bold;
        }

        /* Grid Layouts */
        .form-grid-3 {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
            margin-bottom: 15px;
            align-items: end;
        }

        .form-grid-4 {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 20px;
            margin-bottom: 15px;
            align-items: end;
        }

        .form-grid-5 {
            display: grid;
            grid-template-columns: repeat(5, 1fr);
            gap: 20px;
            margin-bottom: 15px;
            align-items: end;
        }

        .form-label {
            display: block;
            font-weight: bold;
            margin-bottom: 6px;
            color: #444;
            font-size: 12px;
            text-transform: uppercase;
        }

        .form-control {
            width: 100%;
            padding: 8px 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            font-size: 13px;
            box-sizing: border-box;
            transition: all 0.3s;
        }

        .btn-nav {
            padding: 9px 20px;
            background-color: #006699;
            color: white;
            border: none;
            cursor: pointer;
            font-weight: bold;
            font-size: 13px;
            border-radius: 4px;
            transition: background 0.2s;
        }

            .btn-nav:hover {
                background-color: #004d73;
            }

        .btn-secondary {
            background-color: #6c757d;
        }

        .btn-del {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 5px 10px;
            border-radius: 3px;
            cursor: pointer;
            font-weight: bold;
        }

        .Grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 12px;
            background: white;
        }

            .Grid th {
                background-color: #006699;
                color: white;
                padding: 10px;
                border: 1px solid #004d73;
                text-align: center;
                position: sticky;
                top: 0;
                z-index: 10;
            }

            .Grid td {
                padding: 8px;
                border: 1px solid #ddd;
                text-align: center;
                vertical-align: middle;
            }

        /* Stacked Cell Styling for Compact View */
        .stack-cell {
            text-align: left !important;
            padding: 6px 10px !important;
        }

        .stack-title {
            font-weight: bold;
            color: #006699;
            font-size: 13px;
            display: block;
            margin-bottom: 2px;
        }

        .stack-sub {
            font-size: 11px;
            color: #666;
            display: block;
        }

        .lbl-grand {
            font-size: 20px;
            font-weight: bold;
            color: #28a745;
        }

        .select2-container--default .select2-selection--single {
            height: 34px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

            .select2-container--default .select2-selection--single .select2-selection__rendered {
                line-height: 32px;
                font-size: 13px;
                color: #333 !important;
            }

        .ui-datepicker {
            z-index: 9999 !important;
        }

        .select2-results__option {
            color: #333 !important;
            background-color: #fff !important;
        }

        .select2-results__option--highlighted {
            background-color: #006699 !important;
            color: #fff !important;
        }

        .rowChk input, #chkAll {
            cursor: pointer;
            width: 16px;
            height: 16px;
            margin: 0;
        }

        /* AJAX Spinner Overlay */
        .loader-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255,255,255,0.85);
            z-index: 99999;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .loader-box {
            background: white;
            padding: 30px 50px;
            border-radius: 8px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            text-align: center;
        }

        .spinner {
            border: 4px solid #f3f3f3;
            border-top: 4px solid #006699;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto;
        }

        @keyframes spin {
            0% {
                transform: rotate(0deg);
            }

            100% {
                transform: rotate(360deg);
            }
        }

        /* Breadcrumb Tracker */
        .breadcrumb-tracker {
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 25px;
            font-weight: bold;
            color: #aaa;
            font-size: 14px;
        }

            .breadcrumb-tracker .step {
                padding: 10px 25px;
                background: #e2e8f0;
                border-radius: 20px;
                transition: all 0.3s;
            }

                .breadcrumb-tracker .step.active {
                    background: #006699;
                    color: white;
                    box-shadow: 0 4px 10px rgba(0,102,153,0.3);
                }

            .breadcrumb-tracker .step-divider {
                letter-spacing: -2px;
                margin: 0 15px;
            }

        /* Sticky Action Bar */
        .sticky-action-bar {
            position: sticky;
            bottom: 0;
            background: #fff;
            padding: 15px 20px;
            border-top: 3px solid #19658A;
            box-shadow: 0 -5px 20px rgba(0,0,0,0.15);
            z-index: 100;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-radius: 6px 6px 0 0;
        }
    </style>

    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="calender/jquery.ui.core.js"></script>
    <script src="calender/jquery.ui.widget.js"></script>
    <script src="calender/jquery.ui.datepicker.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <script type="text/javascript">
        jQuery.browser = {};
        (function () {
            jQuery.browser.msie = false;
            jQuery.browser.version = 0;
            if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
                jQuery.browser.msie = true;
                jQuery.browser.version = RegExp.$1;
            }
        })();

        function initScripts() {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
            var $ddlClient = $('#<%= cmbvendor.ClientID %>');
            if ($ddlClient.hasClass("select2-hidden-accessible")) { $ddlClient.select2('destroy'); }
            $ddlClient.select2({ placeholder: "Search Client (Optional)", allowClear: true, width: '100%' });
            var $ddlSales = $('#cmbSalesPerson');
            if ($ddlSales.hasClass("select2-hidden-accessible")) { $ddlSales.select2('destroy'); }
            $ddlSales.select2({ placeholder: "Search Sales Person...", allowClear: true, width: '100%' });
            updateDocPlaceholder();
        }

        $(document).ready(function () { initScripts(); });
        function pageLoad() { initScripts(); }

        function updateDocPlaceholder() {
            var ddl = document.getElementById('<%= ddlDocType.ClientID %>');
            var txt = document.getElementById('<%= txtSourceDocNo.ClientID %>');
            var lbl = document.getElementById('lblDocNoPrompt');
        
            if (ddl && txt && lbl) {
                if (ddl.value === 'Purchase Order') {
                    lbl.innerText = "PO No. / DO No. / System Ref";
                    txt.placeholder = "Search by PO, DO, or System Ref...";
                } else if (ddl.value === 'Delivery Challan') {
                    lbl.innerText = "Challan No. / PO No.";
                    txt.placeholder = "Search Challan or PO...";
                } else {
                    lbl.innerText = "Specific Document No.";
                    txt.placeholder = "Search Document No...";
                }
            }
        }

        function filterGrid() {
            var input, filter, table, tr, td, i, j, txtValue;
            input = document.getElementById("txtQuickFilter");
            filter = input.value.toUpperCase();
            table = document.getElementById("<%= gvSearchDocs.ClientID %>");
            if (!table) return;
            tr = table.getElementsByTagName("tr");

            for (i = 1; i < tr.length; i++) {
                tr[i].style.display = "none";
                td = tr[i].getElementsByTagName("td");
                for (j = 0; j < td.length; j++) {
                    if (td[j]) {
                        txtValue = td[j].textContent || td[j].innerText;
                        if (txtValue.toUpperCase().indexOf(filter) > -1) { tr[i].style.display = ""; break; }
                    }
                }
            }
        }

        function toggleAll(source) {
            var checkboxes = document.querySelectorAll('.rowChk input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) checkboxes[i].checked = source.checked;
        }

        function confirmBulkRemove(btn) {
            if (btn.dataset.confirmed === 'true') { btn.dataset.confirmed = 'false'; return true; }
            Swal.fire({
                title: 'Remove Selected?', text: "Are you sure you want to remove the selected items?", icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', cancelButtonColor: '#6c757d', confirmButtonText: 'Yes, remove them!'
            }).then((result) => { if (result.isConfirmed) { btn.dataset.confirmed = 'true'; btn.click(); } });
            return false;
        }

        function confirmSingleRemove(btn) {
            if (btn.dataset.confirmed === 'true') { btn.dataset.confirmed = 'false'; return true; }
            Swal.fire({
                title: 'Remove Item?', text: "Are you sure you want to remove this item?", icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', cancelButtonColor: '#6c757d', confirmButtonText: 'Yes, remove it!'
            }).then((result) => { if (result.isConfirmed) { btn.dataset.confirmed = 'true'; btn.click(); } });
            return false;
        }

        function validateAndConfirmGenerate(btn) {
            if (btn.dataset.confirmed === 'true') { btn.dataset.confirmed = 'false'; return true; }
            
            var invDate = document.getElementById('<%= txtinvoiceDate.ClientID %>').value;
            if (invDate.trim() === "") { Swal.fire('Action Blocked', 'Please provide an Invoice Date.', 'warning'); return false; }

            var extNo = document.getElementById('<%= txtExtInvoiceNo.ClientID %>').value;
            if (extNo.trim() === "") { Swal.fire('Action Blocked', 'Please provide the External ERP No.', 'warning'); return false; }

            var salesPerson = document.getElementById('<%= cmbSalesPerson.ClientID %>').value;
            if (salesPerson === "") { Swal.fire('Action Blocked', 'Please select a Sales Person.', 'warning'); return false; }
            
            // NEW: Explicit Tax Type Validation
            var taxType = document.querySelector('input[name="<%= RadioButtonGst.UniqueID %>"]:checked');
            if (!taxType) { 
                Swal.fire('Action Blocked', 'Please explicitly select a Tax Type (Intra or Inter).', 'warning'); 
                return false; 
            }
            
            Swal.fire({
                title: 'Confirm Generation?', 
                text: "Physical stock will be deducted.", 
                icon: 'question', 
                showCancelButton: true, 
                confirmButtonColor: '#28a745', 
                cancelButtonColor: '#6c757d', 
                confirmButtonText: 'Yes, Generate Invoice!'
            }).then((result) => { 
                if (result.isConfirmed) { 
                    btn.dataset.confirmed = 'true'; 
                    btn.click(); 
                } 
            });
            
            return false;
        }

        function CalculateRow(input, trigger) {
            var row = input;
            while (row && row.tagName !== 'TR') {
                row = row.parentNode;
            }
            if (!row) return;

            var txtQty = row.querySelector("input[id*='txtqnty']");
            var txtRate = row.querySelector("input[id*='txtsailrate']");
            var txtDiscPer = row.querySelector("input[id*='txtDiscPer']");
            var txtUnitDiscAmt = row.querySelector("input[id*='txtUnitDiscAmt']");
            var txtDiscAmt = row.querySelector("input[id*='txtDiscAmt']"); // Total Row Discount
            var lblGross = row.querySelector("span[id*='lblGross']");
            var lblTaxable = row.querySelector("span[id*='lblTaxable']"); // Main Summary Column
            var lblAfterDisc = row.querySelector("span[id*='lblAfterDisc']"); // Pricing Summary Column
            var lblEffectiveRate = row.querySelector("span[id*='lblEffectiveRate']");
            var lblGst = row.querySelector("span[id*='lblGstRate']");
            var lblTaxAmt = row.querySelector("span[id*='lblTaxAmt']");
            var lblNet = row.querySelector("span[id*='lblNet']");

            if (!txtQty || !txtRate || !lblGross) return;

            var maxQty = parseFloat(txtQty.getAttribute('data-max')) || 0;
            var qty = parseFloat(txtQty.value);
            if (isNaN(qty)) qty = 0;

            if (qty > maxQty) {
                Swal.fire({ title: 'Quantity Exceeded', text: "Bill Qty cannot exceed Pending Qty of " + maxQty + ".", icon: 'warning', toast: true, position: 'top-end', showConfirmButton: false, timer: 3000 });
                qty = maxQty; txtQty.value = maxQty;
            } else if (qty < 0) { qty = 0; txtQty.value = 0; }

            var rate = Math.max(0, parseFloat(txtRate.value) || 0);
            var gst = Math.max(0, parseFloat(lblGst ? lblGst.innerText : 0) || 0);

            // 1. Base Gross (Qty * Original Rate)
            var gross = qty * rate;
            lblGross.innerText = gross.toFixed(2);

            var discPer = 0, unitDisc = 0, totalDisc = 0;

            // 2. Bidirectional syncing depending on what the user edited
            if (trigger === 'PER') {
                discPer = Math.max(0, parseFloat(txtDiscPer.value) || 0);
                if (discPer > 100) { discPer = 100; txtDiscPer.value = 100; }
                unitDisc = (rate * discPer) / 100;
                totalDisc = unitDisc * qty;
                if (txtUnitDiscAmt) txtUnitDiscAmt.value = unitDisc.toFixed(2);
                if (txtDiscAmt) txtDiscAmt.value = totalDisc.toFixed(2);
            }
            else if (trigger === 'UNIT_AMT') {
                unitDisc = Math.max(0, parseFloat(txtUnitDiscAmt.value) || 0);
                if (unitDisc > rate) { unitDisc = rate; txtUnitDiscAmt.value = unitDisc.toFixed(2); }
                discPer = rate > 0 ? (unitDisc / rate) * 100 : 0;
                totalDisc = unitDisc * qty;
                if (txtDiscPer) txtDiscPer.value = discPer.toFixed(2);
                if (txtDiscAmt) txtDiscAmt.value = totalDisc.toFixed(2);
            }
            else if (trigger === 'TOTAL_AMT') {
                totalDisc = Math.max(0, parseFloat(txtDiscAmt.value) || 0);
                if (totalDisc > gross) { totalDisc = gross; txtDiscAmt.value = totalDisc.toFixed(2); }
                unitDisc = qty > 0 ? totalDisc / qty : 0;
                discPer = rate > 0 ? (unitDisc / rate) * 100 : 0;
                if (txtDiscPer) txtDiscPer.value = discPer.toFixed(2);
                if (txtUnitDiscAmt) txtUnitDiscAmt.value = unitDisc.toFixed(2);
            }
            else {
                // Default sync from Disc% when Quantity or Rate changes
                discPer = Math.max(0, parseFloat(txtDiscPer ? txtDiscPer.value : 0) || 0);
                unitDisc = (rate * discPer) / 100;
                totalDisc = unitDisc * qty;
                if (txtUnitDiscAmt) txtUnitDiscAmt.value = unitDisc.toFixed(2);
                if (txtDiscAmt) txtDiscAmt.value = totalDisc.toFixed(2);
            }

            // 3. Effective Unit Rate = Rate - Per-Unit Discount
            var effRate = Math.max(0, rate - unitDisc);
            if (lblEffectiveRate) lblEffectiveRate.innerText = effRate.toFixed(2);

            // 4. Taxable / Net Amount After Discount
            var taxable = Math.max(0, gross - totalDisc);
            if (lblTaxable) lblTaxable.innerText = taxable.toFixed(2);
            if (lblAfterDisc) lblAfterDisc.innerText = taxable.toFixed(2);

            // 5. Tax & Net Calculations
            var taxVal = (taxable * gst) / 100;
            if (lblTaxAmt) lblTaxAmt.innerText = taxVal.toFixed(2);

            var net = taxable + taxVal;
            if (lblNet) lblNet.innerText = net.toFixed(2);

            RecalculateFooter();
        }

        function RecalculateFooter() {
            var grid = document.getElementById("<%= GridView1.ClientID %>");
            var totalTax = 0, totalGrand = 0;

            if (grid) {
                var rows = grid.getElementsByTagName("tr");
                for (var i = 1; i < rows.length; i++) {
                    var lTax = rows[i].querySelector("span[id*='lblTaxAmt']");
                    var lNet = rows[i].querySelector("span[id*='lblNet']");
                    if (lTax) totalTax += parseFloat(lTax.innerText) || 0;
                    if (lNet) totalGrand += parseFloat(lNet.innerText) || 0;
                }
            }

            var inputFrt = document.getElementById("<%= txt_delivery_amnt.ClientID %>");
            var inputOth = document.getElementById("<%= txt_othr_amnt.ClientID %>");
            var frt = inputFrt ? Math.max(0, parseFloat(inputFrt.value) || 0) : 0;
            var oth = inputOth ? Math.max(0, parseFloat(inputOth.value) || 0) : 0;

            var outTax = document.getElementById("<%= lblFooterTax.ClientID %>");
            var outGrand = document.getElementById("<%= lblFooterGrand.ClientID %>");

            var finalGrandTotal = totalGrand + frt + oth;

            if (outTax) outTax.innerText = totalTax.toFixed(2);
            if (outGrand) outGrand.innerText = finalGrandTotal.toFixed(2);

            var btnSubmit = document.getElementById("<%= Button1.ClientID %>");
            var warningMsg = document.getElementById("zeroTotalWarning");

            if (btnSubmit && warningMsg) {
                if (finalGrandTotal <= 0) {
                    btnSubmit.disabled = true; btnSubmit.style.backgroundColor = "#cccccc"; btnSubmit.style.cursor = "not-allowed";
                    warningMsg.style.display = "block";
                } else {
                    btnSubmit.disabled = false; btnSubmit.style.backgroundColor = "#28a745"; btnSubmit.style.cursor = "pointer";
                    warningMsg.style.display = "none";
                }
            }
        }

        function showReconcile(productId, productName) {
            var refNo = document.getElementById('hdnRefNo').value;
            if (!refNo) return;

            Swal.fire({
                title: 'Reconciling Item...',
                html: 'Fetching billing history...',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                    $.ajax({
                        type: "POST",
                        url: "Add_invoice.aspx/GetReconciliation",
                        data: JSON.stringify({ refNo: refNo, productId: productId }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            Swal.fire({
                                title: 'Reconciliation Details',
                                html: '<div style="font-size:13px; color:#444; margin-bottom:15px; text-align:left;">Item: <strong>' + productName + '</strong></div>' + response.d,
                                width: 500,
                                confirmButtonText: 'Close',
                                confirmButtonColor: '#6c757d'
                            });
                        },
                        error: function (err) {
                            Swal.fire('Error', 'Failed to fetch reconciliation data.', 'error');
                        }
                    });
                }
            });
        }

        function confirmRemoveZeroQty(btn) {
            if (btn.dataset.confirmed === 'true') { btn.dataset.confirmed = 'false'; return true; }
            Swal.fire({
                title: 'Clear Zero Quantities?',
                text: "Are you sure you want to remove all items with a Bill Qty of 0?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#ff9800',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, clean grid!'
            }).then((result) => {
                if (result.isConfirmed) { btn.dataset.confirmed = 'true'; btn.click(); }
            });
            return false;
        }

        function syncAddresses() {
            var chk = document.getElementById('chkSameAsBilling');
            var lstBill = document.getElementById('<%= List_BillingAddress.ClientID %>');
            var lstShip = document.getElementById('<%= List_ShippingAddress.ClientID %>');
            
            if (chk && lstBill && lstShip) {
                if (chk.checked) {
                    lstShip.disabled = true;
                    lstShip.selectedIndex = lstBill.selectedIndex;
                } else {
                    lstShip.disabled = false;
                }
            }
        }

        // Run once on page load to set the initial state
        window.onload = updateDocPlaceholder;
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <ProgressTemplate>
            <div class="loader-overlay">
                <div class="loader-box">
                    <div class="spinner"></div>
                    <h3 style="color: #006699; margin-top: 15px; font-size: 16px;">Processing...</h3>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <div style="width: 98%; margin: auto; padding-top: 10px;">
        <div class="section-header">Generate Tax Invoice (From Source)</div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">

                    <asp:View ID="vSetup" runat="server">
                        <div class="breadcrumb-tracker">
                            <div class="step active">1. Select Source Document</div>
                            <div class="step-divider">━━━━━</div>
                            <div class="step">2. Review & Finalize</div>
                        </div>

                        <div class="box-panel">
                            <div class="box-title">Search Parameters</div>

                            <div class="form-grid-3">
                                <div>
                                    <label class="form-label">Filter by Client</label>
                                    <asp:DropDownList ID="cmbvendor" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged" ClientIDMode="Static"></asp:DropDownList>
                                    <asp:Label ID="lblclientId" runat="server" Visible="false"></asp:Label>
                                </div>
                                <div>
                                    <label class="form-label">Source Type <span style="color: red">*</span></label>
                                    <asp:DropDownList ID="ddlDocType" runat="server" CssClass="form-control" onchange="updateDocPlaceholder()">
                                        <asp:ListItem Value="Quotation">Quotation</asp:ListItem>
                                        <asp:ListItem Value="Purchase Order">Purchase Order (Customer PO)</asp:ListItem>
                                        <asp:ListItem Value="Delivery Challan">Delivery Challan</asp:ListItem>
                                        <asp:ListItem Value="Proforma">Pro-Forma Invoice</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label class="form-label" id="lblDocNoPrompt">Specific Document No.</label>
                                    <asp:TextBox ID="txtSourceDocNo" runat="server" CssClass="form-control" placeholder="Search Quotation No..."></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-grid-3">
                                <div>
                                    <label class="form-label">Date From</label>
                                    <asp:TextBox ID="txtfromDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">Date To</label>
                                    <asp:TextBox ID="txttodate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div style="display: flex; gap: 10px; align-items: flex-end; height: 100%;">
                                    <asp:Button ID="btnSertch" runat="server" Text="🔍 Search Records" CssClass="btn-nav" OnClick="btnSertch_Click" Style="flex: 2; height: 38px; background-color: #006699; color: white; font-weight: bold; border: none; border-radius: 4px; cursor: pointer; transition: background-color 0.2s;" />
    
                                    <asp:Button ID="btnResetSearch" runat="server" Text="↺ Reset" CssClass="btn-nav" OnClick="btnResetSearch_Click" Style="flex: 1; height: 38px; background-color: #6c757d; color: white; font-weight: bold; border: none; border-radius: 4px; cursor: pointer; transition: background-color 0.2s;" ToolTip="Clear all search filters and reload" />
                                </div>
                            </div>

                            <%--<asp:Panel ID="pnlAddress" runat="server" Visible="false" Style="margin-bottom: 15px;">
                                <label class="form-label">Select Client Address <span style="color: red">*</span></label>
                                <asp:ListBox ID="List_SiteAddress" runat="server" CssClass="form-control" Height="70px"></asp:ListBox>
                            </asp:Panel>--%>

                            <asp:Panel ID="pnlAddress" runat="server" Visible="false" Style="margin-bottom: 15px;">
                                <div style="display: flex; gap: 20px; align-items: stretch;">
        
                                    <div style="flex: 1; background: #fdfdfd; padding: 15px; border: 1px solid #e2e8f0; border-radius: 6px;">
                                        <label class="form-label" style="color:#006699;">1. Select Billing Address <span style="color:red">*</span></label>
                                        <p style="font-size:11px; color:#666; margin-top:0;">This address will appear under "Billed To" on the Invoice.</p>
                                        <asp:ListBox ID="List_BillingAddress" runat="server" CssClass="form-control" Height="70px" onchange="syncAddresses()"></asp:ListBox>
                                    </div>

                                    <div style="flex: 1; background: #f8fafc; padding: 15px; border: 1px solid #cbd5e1; border-radius: 6px;">
                                        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px;">
                                            <label class="form-label" style="margin-bottom:0; color:#475569;">2. Select Shipping Address <span style="color:red">*</span></label>
                                            <div>
                                                <input type="checkbox" id="chkSameAsBilling" checked="checked" onclick="syncAddresses()" style="vertical-align: middle; cursor: pointer;" />
                                                <label for="chkSameAsBilling" style="font-size:11px; cursor: pointer;">Same as Billing</label>
                                            </div>
                                        </div>
                                        <p style="font-size:11px; color:#666; margin-top:0;">This address will appear under "Shipped To".</p>
                                        <asp:ListBox ID="List_ShippingAddress" runat="server" CssClass="form-control" Height="70px" Enabled="false"></asp:ListBox>
                                    </div>

                                </div>
                            </asp:Panel>

                            <div style="margin-bottom: 10px;">
                                <input type="text" id="txtQuickFilter" onkeyup="filterGrid()" class="form-control" placeholder="🔍 Quick Filter Results..." style="width: 100%; max-width: 400px; display: inline-block; border-color: #006699;" />
                            </div>

                            <div style="max-height: 250px; overflow-y: auto; border: 1px solid #e2e8f0;">
                                <asp:GridView ID="gvSearchDocs" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" OnRowCommand="gvSearchDocs_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="DocNo" HeaderText="Document No" ItemStyle-Font-Bold="true" />
                                        <asp:BoundField DataField="DocDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                        <asp:BoundField DataField="Client_Name" HeaderText="Client Name" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="ExtRef" HeaderText="Ext. Ref / DO / PO" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="CreatedBy" HeaderText="Created By" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-HorizontalAlign="Center" />
                                        <asp:BoundField DataField="Net_amount" HeaderText="Net Amount" DataFormatString="{0:N2}" ItemStyle-HorizontalAlign="Right" />
                                        <asp:TemplateField HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:Button ID="btnSelect" runat="server" CommandName="SelectDoc" CommandArgument='<%# Eval("DocNo") %>' Text="Select & Proceed &rarr;" CssClass="btn-nav" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="padding: 15px; text-align: center; color: #777;"><i>No records found. Select parameters and click Search.</i></div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                            </div>
                        </div>
                    </asp:View>

                    <asp:View ID="vProducts" runat="server">
                        <div class="breadcrumb-tracker">
                            <div class="step" style="cursor: pointer;" onclick="document.getElementById('<%= btnBackSetup.ClientID %>').click();">1. Select Source Document</div>
                            <div class="step-divider">━━━━━</div>
                            <div class="step active">2. Review & Finalize</div>
                        </div>

                        <%--<div class="box-panel" style="background-color: #f0f7fb; border-left: 4px solid #19658A; padding: 15px 20px;">
                            <div class="form-grid-3" style="margin-bottom: 0;">
                                <div><span style="font-size: 11px; color: #666; text-transform: uppercase; font-weight: bold;">Billed To Client</span><br />
                                    <strong style="font-size: 14px; color: #333;">
                                        <asp:Label ID="lblConfirmClient" runat="server"></asp:Label></strong></div>
                                <div><span style="font-size: 11px; color: #666; text-transform: uppercase; font-weight: bold;">Source Document</span><br />
                                    <strong style="font-size: 14px; color: #006699;">
                                        <asp:Label ID="lblConfirmDoc" runat="server"></asp:Label></strong></div>
                                <div><span style="font-size: 11px; color: #666; text-transform: uppercase; font-weight: bold;">Selected Address</span><br />
                                    <strong style="font-size: 13px; color: #444;">
                                        <asp:Label ID="lblConfirmAddress" runat="server"></asp:Label></strong></div>
                                <asp:HiddenField ID="hdnRefNo" ClientIDMode="Static" runat="server" />
                            </div>
                        </div>--%>

                        <div class="box-panel" style="background-color: #f8fafc; border: 1px solid #cbd5e1; border-top: 4px solid #19658A; padding: 0;">
    
                            <div style="background-color: #e2e8f0; padding: 10px 20px; border-bottom: 1px solid #cbd5e1; display: flex; justify-content: space-between; align-items: center;">
                                <span style="font-weight: bold; color: #1e293b; font-size: 14px;">📄 Invoice Generation Context</span>
                                <span style="font-size: 12px; color: #475569;">Document: <strong style="color: #006699;"><asp:Label ID="lblConfirmDoc" runat="server"></asp:Label></strong></span>
                            </div>

                            <div style="display: flex; gap: 20px; padding: 20px;">
        
                                <div style="flex: 1;">
                                    <div style="font-size: 11px; color: #64748b; text-transform: uppercase; font-weight: bold; margin-bottom: 8px; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px;">Client Details</div>
                                    <strong style="font-size: 15px; color: #0f172a; display: block; margin-bottom: 12px;">
                                        <asp:Label ID="lblConfirmClient" runat="server"></asp:Label>
                                    </strong>

                                    <div style="margin-bottom: 12px;">
                                        <span style="font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Billed To:</span>
                                        <div style="font-size: 12px; color: #333; line-height: 1.4; padding-left: 6px; border-left: 3px solid #006699;">
                                            <asp:Label ID="lblBillingAddress" runat="server"></asp:Label>
                                        </div>
                                    </div>

                                    <div>
                                        <span style="font-size: 10px; font-weight: bold; color: #94a3b8; text-transform: uppercase;">Shipped To:</span>
                                        <div style="font-size: 12px; color: #333; line-height: 1.4; padding-left: 6px; border-left: 3px solid #28a745;">
                                            <asp:Label ID="lblConfirmAddress" runat="server"></asp:Label>
                                        </div>
                                    </div>
                                </div>

                                <div style="width: 1px; background-color: #e2e8f0;"></div>

                                <div style="flex: 1;">
                                    <div style="font-size: 11px; color: #64748b; text-transform: uppercase; font-weight: bold; margin-bottom: 8px; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px;">Source Document Data</div>
            
                                    <table style="width: 100%; font-size: 12px; border-collapse: collapse;">
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0; width: 45%; border-bottom: 1px dashed #eee;">Document Type:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right; border-bottom: 1px dashed #eee;"><asp:Label ID="lblDocTypeView" runat="server"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0; border-bottom: 1px dashed #eee;">DO Number:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right; border-bottom: 1px dashed #eee;"><asp:Label ID="lblConfirmDO" runat="server" Text="N/A"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0; border-bottom: 1px dashed #eee;">PO Number:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right; border-bottom: 1px dashed #eee;"><asp:Label ID="lblConfirmPONum" runat="server" Text="N/A"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0; border-bottom: 1px dashed #eee;">PO Date:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right; border-bottom: 1px dashed #eee;"><asp:Label ID="lblConfirmPODate" runat="server" Text="N/A"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0; border-bottom: 1px dashed #eee;">Validity Start:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right; border-bottom: 1px dashed #eee;"><asp:Label ID="lblConfirmValStart" runat="server" Text="N/A"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td style="color:#64748b; padding: 5px 0;">Validity End:</td>
                                            <td style="font-weight:bold; color:#0f172a; text-align:right;"><asp:Label ID="lblConfirmValEnd" runat="server" Text="N/A"></asp:Label></td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                            <asp:HiddenField ID="hdnRefNo" ClientIDMode="Static" runat="server" />
                        </div>

                        <div class="box-panel">
                            <div class="box-title">Invoice Master Details</div>
                            <div class="form-grid-5">
                                <div>
                                    <label class="form-label">Invoice Date <span style="color: red">*</span></label>
                                    <asp:TextBox ID="txtinvoiceDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">External ERP No. <span style="color: red">*</span></label>
                                    <asp:TextBox ID="txtExtInvoiceNo" runat="server" CssClass="form-control" placeholder="Enter ERP No..."></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">Ext. ERP Date</label>
                                    <asp:TextBox ID="txtExtInvoiceDate" runat="server" CssClass="form-control datepicker" placeholder="Select Date..."></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">Tax Type <span style="color: red">*</span></label>
                                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal" CellPadding="5">
                                        <asp:ListItem Value="1">Intra (CGST/SGST)</asp:ListItem>
                                        <asp:ListItem Value="0">Inter (IGST)</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                                <div>
                                    <label class="form-label">Sales Person <span style="color: red">*</span></label>
                                    <asp:DropDownList ID="cmbSalesPerson" runat="server" CssClass="form-control select-search" ClientIDMode="Static"></asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <div class="box-panel" style="margin-bottom: 0;">
                            <div class="box-title" style="display: flex; justify-content: space-between; align-items: center;">
                                <div style="display: flex; align-items: center;">
                                    Review Items
                                    <span style="font-size: 12px; font-weight: normal; color: #666; margin-left: 15px; border-left: 2px solid #e2e8f0; padding-left: 15px;">Active Items: <strong style="color: #006699; font-size: 15px; margin-right: 10px;">
                                        <asp:Label ID="lblActiveCount" runat="server" Text="0"></asp:Label></strong>
                                        Removed: <strong style="color: #dc3545; font-size: 15px;">
                                            <asp:Label ID="lblRemovedCount" runat="server" Text="0"></asp:Label></strong>
                                    </span>
                                </div>
                                <div>
                                    <asp:Button ID="btnRestore" runat="server" Text="↺ Undo Removes" CssClass="btn-nav btn-secondary" Style="margin-right: 10px;" OnClick="btnRestore_Click" Visible="false" />
                                    <asp:Button ID="btnRemoveZeroQty" runat="server" Text="🧹 Clear Zero Qty" CssClass="btn-nav btn-danger" Style="background-color: #ff9800; color: #fff; margin-right: 10px; border: none;" OnClick="btnRemoveZeroQty_Click" OnClientClick="return confirmRemoveZeroQty(this);" ToolTip="Instantly remove all rows where Bill Qty is 0" />
                                    <asp:Button ID="btnRemoveBulk" runat="server" Text="🗑 Remove Selected" CssClass="btn-nav btn-del" OnClick="btnRemoveBulk_Click" OnClientClick="return confirmBulkRemove(this);" />
                                </div>
                            </div>

                            <div style="max-height: 450px; overflow-y: auto; overflow-x: auto; border: 0.5px solid #e2e8f0; width: 100%;">
                                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CssClass="Grid" DataKeyNames="TrueID" OnRowCommand="gvGrid1_RowCommand" Style="width: 100%;">
                                    <Columns>
                                        <asp:TemplateField ItemStyle-Width="30px" ItemStyle-HorizontalAlign="Center">
                                            <HeaderTemplate>
                                                <input type="checkbox" id="chkAll" onclick="toggleAll(this);" title="Select All" /></HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkSelect" runat="server" CssClass="rowChk" /></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product Details" ItemStyle-CssClass="stack-cell" ItemStyle-Width="220px">
                                            <ItemTemplate>
                                                <span class="stack-title"><%# Eval("Product_name") %></span>
                                                <span class="stack-sub">ID: <strong><%# Eval("TrueID") %></strong> &nbsp;|&nbsp; HSN: <strong><%# Eval("TrueHSN") %></strong></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Specification" ItemStyle-Width="150px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtdes" runat="server" Text='<%# Bind("specification") %>' CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox></ItemTemplate>
                                        </asp:TemplateField>

                                        <%--<asp:TemplateField HeaderText="Stock & Quantities" ItemStyle-CssClass="stack-cell" ItemStyle-Width="160px">
                                            <ItemTemplate>
                                                <span class="stack-sub" style="margin-bottom: 4px;">Stock:
                                                    <asp:Label ID="lblStock" runat="server" Text='<%# Bind("AvailableStock") %>' Font-Bold="true" ForeColor="#19658A"></asp:Label>
                                                    | Q: <strong><%# Eval("QuotedQty") %></strong> | I: <span style="color: #dc3545; font-weight: bold;"><%# Eval("InvoicedQty") %></span></span>
                                                <div style="display: flex; align-items: center; gap: 5px;">
                                                    <span style="font-size: 11px; font-weight: bold; color: #444;">Bill Qty:</span>
                                                    <asp:TextBox ID="txtqnty" runat="server" Text='<%# Bind("PendingQty") %>' data-max='<%# Eval("PendingQty") %>' CssClass="form-control" Style="text-align: center; font-weight: bold; color: #006699; padding: 4px;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>

                                        <asp:TemplateField HeaderText="Stock & Quantities" ItemStyle-CssClass="stack-cell" ItemStyle-Width="160px">
                                            <ItemTemplate>
                                                <span class="stack-sub" style="margin-bottom: 4px;">
                                                    Stock: <asp:Label ID="lblStock" runat="server" Text='<%# Bind("AvailableStock") %>' Font-Bold="true" ForeColor="#19658A"></asp:Label> | 
                                                    Q: <strong><%# Eval("QuotedQty") %></strong> | 
                                                    I: <span style="color:#dc3545; font-weight:bold;"><%# Eval("InvoicedQty") %></span>
            
                                                    <%# Convert.ToDecimal(Eval("InvoicedQty")) > 0 ? "<a href=\"javascript:void(0);\" onclick=\"showReconcile('" + Eval("TrueID") + "', '" + HttpUtility.JavaScriptStringEncode(Eval("Product_name").ToString()) + "')\" style=\"background:#e2e8f0; color:#006699; padding:2px 5px; border-radius:3px; font-size:10px; margin-left:4px; text-decoration:none; vertical-align:middle;\" title=\"Reconcile Previous Invoices\">🔍 History</a>" : "" %>
                                                </span>
                                                <div style="display: flex; align-items: center; gap: 5px;">
                                                    <span style="font-size:11px; font-weight:bold; color:#444;">Bill Qty:</span>
                                                    <asp:TextBox ID="txtqnty" runat="server" Text='<%# Bind("PendingQty") %>' data-max='<%# Eval("PendingQty") %>' CssClass="form-control" Style="text-align: center; font-weight: bold; color: #006699; padding: 4px;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <%--<asp:TemplateField HeaderText="Pricing & Discounts" ItemStyle-CssClass="stack-cell" ItemStyle-Width="180px">
                                            <ItemTemplate>
                                                <div style="display:flex; justify-content:space-between; margin-bottom: 4px; font-size:11px;">
                                                    <span>Rate: <asp:TextBox ID="txtsailrate" runat="server" Text='<%# Bind("sail_rate") %>' CssClass="form-control" Style="display:inline-block; width:80px; text-align:right; padding:2px;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox></span>
                                                    <span>Gross: <asp:Label ID="lblGross" runat="server" Text="0.00" Font-Bold="true"></asp:Label></span>
                                                </div>
                                                <div style="display:flex; gap: 5px; align-items:center;">
                                                    <span style="font-size:10px; color:#666;">Disc%:</span>
                                                    <asp:TextBox ID="txtDiscPer" runat="server" Text='<%# Bind("discountRate") %>' CssClass="form-control" Style="text-align: center; width:50px; padding:2px;" onkeyup="CalculateRow(this, 'PER')"></asp:TextBox>
                                                    <span style="font-size:10px; color:#666;">Amt:</span>
                                                    <asp:TextBox ID="txtDiscAmt" runat="server" Text="0.00" CssClass="form-control" Style="text-align: right; width:65px; padding:2px;" onkeyup="CalculateRow(this, 'AMT')"></asp:TextBox>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>

                                        <asp:TemplateField HeaderText="Pricing & Discounts" ItemStyle-CssClass="stack-cell" ItemStyle-Width="260px">
                                            <ItemTemplate>
                                                <div style="display:flex; justify-content:space-between; margin-bottom: 3px; font-size:11px;">
                                                    <span>Rate: <asp:TextBox ID="txtsailrate" runat="server" Text='<%# Bind("sail_rate") %>' CssClass="form-control" Style="display:inline-block; width:70px; text-align:right; padding:2px;" onkeyup="CalculateRow(this, 'RATE')"></asp:TextBox></span>
                                                    <span>Gross: <asp:Label ID="lblGross" runat="server" Text="0.00" Font-Bold="true"></asp:Label></span>
                                                </div>
        
                                                <div style="display:flex; gap: 4px; align-items:center; margin-bottom: 3px; font-size:10px;">
                                                    <span>Disc%:</span>
                                                    <asp:TextBox ID="txtDiscPer" runat="server" Text='<%# Bind("discountRate") %>' CssClass="form-control" Style="text-align: center; width:38px; padding:2px;" onkeyup="CalculateRow(this, 'PER')"></asp:TextBox>
            
                                                    <span>Unit ₹:</span>
                                                    <asp:TextBox ID="txtUnitDiscAmt" runat="server" Text="0.00" CssClass="form-control" Style="text-align: right; width:50px; padding:2px;" onkeyup="CalculateRow(this, 'UNIT_AMT')"></asp:TextBox>

                                                    <span>Total ₹:</span>
                                                    <asp:TextBox ID="txtDiscAmt" runat="server" Text="0.00" CssClass="form-control" Style="text-align: right; width:60px; padding:2px;" onkeyup="CalculateRow(this, 'TOTAL_AMT')"></asp:TextBox>
                                                </div>

                                                <div style="display:flex; justify-content:space-between; font-size:11px; background:#f8fafc; padding:3px 5px; border-radius:3px; border:1px dashed #cbd5e1;">
                                                    <span style="color:#006699;">Eff.Rate: ₹<asp:Label ID="lblEffectiveRate" runat="server" Text="0.00" Font-Bold="true"></asp:Label></span>
                                                    <span style="color:#28a745;">Net Amt: ₹<asp:Label ID="lblAfterDisc" runat="server" Text="0.00" Font-Bold="true"></asp:Label></span>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Tax & Net Amount" ItemStyle-CssClass="stack-cell" ItemStyle-Width="160px">
                                            <ItemTemplate>
                                                <span class="stack-sub">Taxable:
                                                    <asp:Label ID="lblTaxable" runat="server" Text="0.00" Font-Bold="true"></asp:Label>
                                                    | GST:
                                                    <asp:Label ID="lblGstRate" runat="server" Text='<%# Bind("Service_tax_rate") %>'></asp:Label>%</span>
                                                <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 3px;">
                                                    <span style="font-size: 11px; color: #666;">Tax: ₹<asp:Label ID="lblTaxAmt" runat="server" Text="0.00"></asp:Label></span>
                                                    <span style="font-size: 13px; color: Green; font-weight: bold;">Net: ₹<asp:Label ID="lblNet" runat="server" Text="0.00"></asp:Label></span>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="30px" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <asp:Button ID="btnRemove" runat="server" CommandName="RemoveItem" CommandArgument='<%# Container.DataItemIndex %>' Text="X" CssClass="btn-del" OnClientClick="return confirmSingleRemove(this);" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>

                        <div class="sticky-action-bar">
                            <div style="flex: 1;">
                                <asp:Button ID="btnBackSetup" runat="server" Text="&larr; Back to Search" CssClass="btn-nav btn-secondary" OnClick="btnBackSetup_Click" />
                                <div id="zeroTotalWarning" style="display: none; color: #dc3545; font-weight: bold; font-size: 13px; margin-top: 10px;">
                                    ⚠️ Cannot generate invoice with ₹0.00 Total.
                                </div>
                            </div>

                            <div style="flex: 2; padding: 0 20px;">
                                <table width="100%" cellpadding="2" cellspacing="0" style="text-align: right;">
                                    <tr>
                                        <td style="color: #555; font-weight: bold;">Freight Charges (+)</td>
                                        <td width="120px">
                                            <asp:TextBox ID="txt_delivery_amnt" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox></td>
                                        <td style="color: #555; font-weight: bold;">Total Tax</td>
                                        <td width="120px">
                                            <asp:Label ID="lblFooterTax" runat="server" Text="0.00" Font-Bold="true"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" placeholder="Other Charge Name" Style="max-width: 150px; float: right;"></asp:TextBox></td>
                                        <td>
                                            <asp:TextBox ID="txt_othr_amnt" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox></td>
                                        <td style="font-size: 16px;"><strong>Grand Total</strong></td>
                                        <td>
                                            <asp:Label ID="lblFooterGrand" runat="server" Text="0.00" CssClass="lbl-grand"></asp:Label></td>
                                    </tr>
                                </table>
                            </div>

                            <div style="flex: 1; text-align: right;">
                                <asp:Button ID="Button1" runat="server" Text="Generate Tax Invoice" CssClass="btn-nav" Style="background-color: #28a745; padding: 12px 25px; font-size: 16px;" OnClick="Button1_Click" OnClientClick="return validateAndConfirmGenerate(this);" />
                            </div>
                        </div>

                        <div class="box-panel" style="margin-top: 30px; background: #fafbfc;">
                            <div class="box-title" style="color: #444;">Previous Invoices Against This Source</div>
                            <div style="overflow-x: auto;">
                                <table class="styled-table Grid" style="width: 100%; min-width: 1000px;">
                                    <thead>
                                        <tr>
                                            <th style="width: 3%;">Sl</th>
                                            <th style="width: 14%;">Customer Name</th>
                                            <th style="width: 9%;">Inv Date</th>
                                            <th style="width: 16%;">Invoice / Quotation Info</th>
                                            <th style="width: 12%;">ARC / PO / DO</th>
                                            <th style="width: 16%;">Amount Summary</th>
                                            <th style="width: 11%;">Validity</th>
                                            <th style="width: 11%;">Created By</th>
                                            <th style="width: 4%;">Buyer</th>
                                            <th style="width: 4%;">Seller</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptInvoices" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# Container.ItemIndex + 1 %></td>
                                                    <td class="text-left"><strong><%# Eval("Client_Name") %></strong></td>
                                                    <td><%# Eval("Invoice_Date") %></td>
                                                    <td class="text-left">
                                                        <span style="background: #006699; color: white; padding: 2px 4px; border-radius: 3px; font-size: 10px;">Inv</span> <strong><%# Eval("Invoice_No") %></strong><br />
                                                        <span style="background: #6c757d; color: white; padding: 2px 4px; border-radius: 3px; font-size: 10px;">Src</span>
                                                        <span style='<%# Eval("Quotation_No").ToString().ToUpper() == "VERBAL" ? "color:#d39e00; font-weight:bold;": "" %>'>
                                                            <%# Eval("Quotation_No") %>
                                                        </span>
                                                    </td>
                                                    <td class="text-left">
                                                        <span style="background: #6c757d; color: white; padding: 2px 4px; border-radius: 3px; font-size: 10px;">ARC</span> <%# Eval("PO_Number") %><br />
                                                        <span style="background: #6c757d; color: white; padding: 2px 4px; border-radius: 3px; font-size: 10px;">PO/DO</span> <%# Eval("DO_Number") %>
                                                    </td>
                                                    <td class="text-right" style="line-height: 1.4;">
                                                        <span style="color: #666;">Gross:</span> ₹<%# Eval("Gross") %><br /><%# Convert.ToDecimal(Eval("discount") == DBNull.Value ? 0 : Eval("discount")) > 0 ? "<span style='color:red;'>Disc: -₹" + Eval("discount") + "</span><br />" : "" %><span style="color: #666;">Taxable:</span> ₹<%# Eval("sub_total") %><br /><span style="background: #e8f4fd; color: #006699; padding: 2px 4px; border-radius: 3px; font-size: 10px;"><%# Eval("cgstOrsgst").ToString() == "YES" ? "CGST/SGST" : (Eval("igst").ToString() == "YES" ? "IGST" : "TAX") %></span>₹<%# Eval("Gst") %><br /><%# Convert.ToDecimal(Eval("Delivery_Amount") == DBNull.Value ? 0 : Eval("Delivery_Amount")) + Convert.ToDecimal(Eval("otherAmount1") == DBNull.Value ? 0 : Eval("otherAmount1")) > 0 ? "<span style='color:#666;'>Frt/Oth:</span> ₹" + (Convert.ToDecimal(Eval("Delivery_Amount") == DBNull.Value ? 0 : Eval("Delivery_Amount")) + Convert.ToDecimal(Eval("otherAmount1") == DBNull.Value ? 0 : Eval("otherAmount1"))) + "<br />" : "" %><strong style="color: #28a745; font-size: 13px;">Total: ₹<%# Eval("Net_Amount") %></strong></td>
                                                    <td>
                                                        <%# Eval("Validity_StartDate") %><br />
                                                        to<br />
                                                        <%# Eval("Validity_EndDate") %>
                                                    </td>
                                                    <td>
                                                        <span style="color: #333; font-weight: bold;"><%# Eval("AddedByName") %></span><br />
                                                        <span style="font-size: 10px; color: #666;"><%# Convert.ToDateTime(Eval("TimeStamp")).ToString("dd-MMM-yyyy hh:mm tt") %></span>
                                                    </td>
                                                    <td>
                                                        <a href="#" onclick="window.open('/corporate/business/print/NewInvoice.aspx?ID=<%# Eval("ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                            <img alt="Buyer View" height="22px" src="../WebImages/viewicon.png" />
                                                        </a>
                                                    </td>
                                                    <td>
                                                        <a href="#" onclick="window.open('/corporate/business/print/NewInvoiceDuplicate.aspx?ID=<%# Eval("ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                            <img alt="Seller View" height="22px" src="../WebImages/viewicon.png" />
                                                        </a>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:PlaceHolder ID="phNoData" runat="server" Visible='<%# ((Repeater)Container.NamingContainer).Items.Count == 0 %>'>
                                                    <tr>
                                                        <td colspan="10" style="padding: 20px; color: #666; font-style: italic;">No previous invoices found for this source document.</td>
                                                    </tr>
                                                </asp:PlaceHolder>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </asp:View>

                </asp:MultiView>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
