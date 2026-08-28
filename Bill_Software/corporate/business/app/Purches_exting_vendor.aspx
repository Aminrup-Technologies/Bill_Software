<%@ Page Title="Flame-Ex | Purchase Wizard" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Purches_exting_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm11" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="calender/jquery.ui.all.css" rel="stylesheet" />
    <script src="calender/jquery-1.7.1.js"></script>
    <script src="calender/jquery.ui.core.js"></script>
    <script src="calender/jquery.ui.widget.js"></script>
    <script src="calender/jquery.ui.datepicker.js"></script>

    <style type="text/css">
        /* --- GLOBAL READABILITY FIX --- */
        input[type="text"], select, option, textarea {
            color: #333 !important;
            background-color: #fff !important;
        }

        /* --- DROPDOWN & SEARCH --- */
        .search-container {
            position: relative;
            width: 100%;
            margin-bottom: 5px;
        }

        .search-input {
            width: 100%;
            padding: 8px;
            border: 1px solid #19658A;
            border-radius: 4px;
            box-sizing: border-box;
            font-size: 13px;
            background-color: #f0f8ff !important; /* Light blue tint for search box */
        }

        .dropdown_style {
            width: 100%;
            padding: 5px;
            height: 30px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        /* FORCE VISIBLE TEXT IN INPUTS AND DROPDOWNS */
        .select2-container--default .select2-selection--single .select2-selection__rendered,
        select.dropdown_style,
        input.textbox_style,
        input.textbox_U_style,
        textarea {
            color: #000 !important; /* Black Text */
            background-color: #fff !important; /* White Background */
            opacity: 1 !important;
        }

        /* Fix for standard dropdown options */
        option {
            color: #000 !important;
            background-color: #fff !important;
        }

        /* --- GRID STYLES --- */
        .Grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }

            .Grid th {
                background-color: #19658A;
                color: white;
                padding: 10px;
                font-size: 13px;
            }

            .Grid td {
                padding: 5px;
                border: 1px solid #ddd;
                text-align: center;
            }

        .textbox_style {
            width: 95%;
            padding: 4px;
            border: 1px solid #ccc;
            border-radius: 3px;
            text-align: center;
        }

        .textbox_U_style {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        /* --- BUTTONS --- */
        .btn-nav {
            padding: 8px 20px;
            font-weight: bold;
            cursor: pointer;
            border: none;
            border-radius: 4px;
            color: white;
            transition: 0.3s;
        }

        .btn-next {
            background-color: #19658A;
        }

        .btn-prev {
            background-color: #6c757d;
            margin-right: 10px;
        }

        .btn-success {
            background-color: #28a745;
        }

        .btn-action {
            display: inline-block;
            padding: 3px 8px;
            color: white !important;
            text-decoration: none;
            border-radius: 3px;
            font-size: 11px;
            min-width: 18px;
            margin: 0 1px;
            cursor: pointer;
        }

        .btn-up, .btn-down {
            background-color: #17a2b8 !important;
            border: 1px solid #117a8b;
        }

        .btn-remove {
            background-color: #dc3545 !important;
            border: 1px solid #bd2130;
        }

        /* --- SUMMARY & LAYOUT --- */
        .form-section {
            background: #fff;
            padding: 20px;
            border: 1px solid #e0e0e0;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }

        .section-title {
            border-bottom: 2px solid #19658A;
            color: #19658A;
            font-size: 16px;
            font-weight: 700;
            margin-bottom: 15px;
            padding-bottom: 5px;
        }

        .row-flex {
            display: flex;
            flex-wrap: wrap;
            margin: 0 -10px;
        }

        .col-flex {
            padding: 10px;
            flex: 1;
            min-width: 200px;
        }

        .summary-container {
            background-color: #f1f9fc;
            border: 1px solid #b3d7e5;
            border-radius: 6px;
            padding: 15px;
            margin-top: 10px;
        }

        .summary-row {
            display: flex;
            justify-content: space-between;
            padding: 4px 0;
            font-size: 13px;
            color: #555;
        }

            .summary-row.total {
                border-top: 2px solid #19658A;
                margin-top: 10px;
                padding-top: 8px;
                font-size: 18px;
                font-weight: 800;
                color: #19658A;
            }

        .charge-input-group {
            display: flex;
            align-items: center;
            gap: 5px;
        }

        /* --- WIZARD STEPS --- */
        .step-wizard {
            display: flex;
            justify-content: space-between;
            margin-bottom: 25px;
            padding: 0 40px;
        }

        .step-wizard-item {
            text-align: center;
            width: 25%;
            position: relative;
        }

            .step-wizard-item .progress-count {
                height: 32px;
                width: 32px;
                display: inline-block;
                background: #e0e0e0;
                color: #555;
                border-radius: 50%;
                line-height: 32px;
                font-weight: bold;
                margin-bottom: 5px;
            }

            .step-wizard-item.active .progress-count {
                background: #19658A;
                color: #fff;
                box-shadow: 0 0 8px rgba(25,101,138,0.4);
            }

            .step-wizard-item.completed .progress-count {
                background: #28a745;
                color: #fff;
            }

        .wizard-step {
            display: none;
        }

            .wizard-step.active {
                display: block;
                animation: fadeEffect 0.4s;
            }

        @keyframes fadeEffect {
            from {
                opacity: 0;
            }

            to {
                opacity: 1;
            }
        }
    </style>

    <script type="text/javascript">
        // --- FILTER FUNCTION ---
        function filterDropdown(inputId, dropdownId) {
            var input = document.getElementById(inputId);
            var filter = input.value.toUpperCase();
            var select = document.getElementById(dropdownId);

            if (!select.dataset.originalOptions) {
                var ops = [];
                for (var i = 0; i < select.options.length; i++) {
                    ops.push({ val: select.options[i].value, text: select.options[i].text });
                }
                select.dataset.originalOptions = JSON.stringify(ops);
            }

            var originalOps = JSON.parse(select.dataset.originalOptions);
            select.innerHTML = "";

            var count = 0;
            for (var i = 0; i < originalOps.length; i++) {
                if (originalOps[i].text.toUpperCase().indexOf(filter) > -1 || originalOps[i].val === "0" || originalOps[i].val === "") {
                    var opt = document.createElement("option");
                    opt.value = originalOps[i].val;
                    opt.text = originalOps[i].text;
                    opt.title = originalOps[i].text;
                    select.add(opt);
                    count++;
                }
            }
            if (count === 0) {
                var opt = document.createElement("option");
                opt.text = "No match found"; select.add(opt);
            }
        }

        // --- CALCULATIONS ---
        function getBaseTotal() {
            var basic = 0, tax = 0;
            $("#<%= gd_Service_Product.ClientID %> tr").each(function () {
                var taxable = parseFloat($(this).find("input[id*='TaxableAmount']").val()) || 0;
                if (taxable > 0) {
                    basic += taxable;
                    var isTax = $(this).find("input[type='radio']:checked").val();
                    if (isTax === "Yes") {
                        var rate = parseFloat($(this).find("select[id*='vat_parsentage']").val()) || 0;
                        tax += (taxable * rate) / 100;
                    }
                }
            });
            return basic + tax;
        }

        function calcDelivery(source) {
            var base = getBaseTotal();
            if (base === 0) return;
            if (source === 'amt') {
                var amt = parseFloat($("#<%= txt_delivery_amnt.ClientID %>").val()) || 0;
                $("#<%= txt_delivery_percent.ClientID %>").val(((amt / base) * 100).toFixed(2));
            } else {
                var pct = parseFloat($("#<%= txt_delivery_percent.ClientID %>").val()) || 0;
                $("#<%= txt_delivery_amnt.ClientID %>").val(((base * pct) / 100).toFixed(2));
            }
            calculateGrandTotal();
        }

        function calcTCS(source) {
            var base = getBaseTotal();
            if (base === 0) return;
            if (source === 'amt') {
                var amt = parseFloat($("#<%= txt_tcs_amnt.ClientID %>").val()) || 0;
                $("#<%= txt_tcs_percent.ClientID %>").val(((amt / base) * 100).toFixed(2));
            } else {
                var pct = parseFloat($("#<%= txt_tcs_percent.ClientID %>").val()) || 0;
                $("#<%= txt_tcs_amnt.ClientID %>").val(((base * pct) / 100).toFixed(2));
            }
            calculateGrandTotal();
        }

        function calculateRow(txtObj) {
            var row = $(txtObj).closest("tr");
            var qty = parseFloat(row.find("input[id*='Quantity']").val()) || 0;
            var rate = parseFloat(row.find("input[id*='Vendor_rate']").val()) || 0;
            var discPct = parseFloat(row.find("input[id*='DiscountPercent']").val()) || 0;

            var total = qty * rate;
            var discAmt = (total * discPct) / 100;
            var taxable = total - discAmt;

            row.find("input[id*='DiscountAmount']").val(discAmt.toFixed(2));
            row.find("input[id*='TaxableAmount']").val(taxable.toFixed(2));
            calculateGrandTotal();
        }

        function calculateGrandTotal() {
            var basicTotal = 0, totalTax = 0;

            $("#<%= gd_Service_Product.ClientID %> tr").each(function () {
                var taxable = parseFloat($(this).find("input[id*='TaxableAmount']").val()) || 0;
                if (taxable > 0) {
                    basicTotal += taxable;
                    var isTax = $(this).find("input[type='radio']:checked").val();
                    if (isTax === "Yes") {
                        var rate = parseFloat($(this).find("select[id*='vat_parsentage']").val()) || 0;
                        totalTax += (taxable * rate) / 100;
                    }
                }
            });

            var delivery = parseFloat($("#<%= txt_delivery_amnt.ClientID %>").val()) || 0;
            var tcs = parseFloat($("#<%= txt_tcs_amnt.ClientID %>").val()) || 0;
            var other1 = parseFloat($("#<%= txt_othr_amnt1.ClientID %>").val()) || 0;
            var other2 = parseFloat($("#<%= txt_othr_amnt2.ClientID %>").val()) || 0;

            var desc1 = $("#<%= TextBox1.ClientID %>").val();
            var desc2 = $("#<%= TextBox2.ClientID %>").val();
            $("#lblOther1Name").text(desc1 ? "+ " + desc1 : "+ Other Charge 1");
            $("#lblOther2Name").text(desc2 ? "+ " + desc2 : "+ Other Charge 2");
            $("#rowOther1").toggle(other1 > 0 || desc1 !== "");
            $("#rowOther2").toggle(other2 > 0 || desc2 !== "");

            var totalExtras = delivery + tcs + other1 + other2;
            var grandTotal = basicTotal + totalTax + totalExtras;

            $("#lblSummaryBasic").text(basicTotal.toFixed(2));
            $("#lblSummaryTax").text(totalTax.toFixed(2));
            $("#lblExtrasDelivery").text(delivery.toFixed(2));
            $("#lblExtrasTCS").text(tcs.toFixed(2));
            $("#lblOther1Amt").text(other1.toFixed(2));
            $("#lblOther2Amt").text(other2.toFixed(2));
            $("#lblSummaryExtras").text(totalExtras.toFixed(2));
            $("#lblSummaryGrand").text(grandTotal.toFixed(2));

            $("#<%= lblpaayment_amount.ClientID %>").text(grandTotal.toFixed(2));
            $("#<%= txt_inv_amount.ClientID %>").val(grandTotal.toFixed(2));
        }

        // --- WIZARD NAV ---
        function showStep(step) {
            $(".wizard-step").removeClass("active");
            $("#step-" + step).addClass("active");
            $(".step-wizard-item").removeClass("active completed");
            for (var i = 1; i < step; i++) $("#progress-" + i).addClass("completed");
            $("#progress-" + step).addClass("active");
            $("#hdnCurrentStep").val(step);
            if (step >= 3) calculateGrandTotal();
        }
        function nextStep(target) { if (validateStep(parseInt($("#hdnCurrentStep").val()))) showStep(target); }
        function prevStep(target) { showStep(target); }

        function validateStep(step) {
            if (step === 1 && $("#<%= cmbvendor.ClientID %>").val() === "") { alert("Select Vendor"); return false; }
            if (step === 2 && $("#<%= txt_invno.ClientID %>").val() === "") { alert("Enter Invoice No"); return false; }
            return true;
        }

        function pageLoadHandler() {
            var step = parseInt($("#hdnCurrentStep").val()) || 1;
            showStep(step);
            calculateGrandTotal();
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
        }

        $(document).ready(pageLoadHandler);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoadHandler);
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />
    <asp:HiddenField ID="hdnCurrentStep" runat="server" Value="1" ClientIDMode="Static" />

    <div style="background-color: #19658A; color: white; padding: 12px; margin-bottom: 20px; border-radius: 4px;">
        <h3 style="margin: 0; font-size: 18px;">Create Purchase Request</h3>
    </div>

    <div class="step-wizard">
        <div class="step-wizard-item active" id="progress-1">
            <div class="progress-count">1</div>
            <div>Vendor</div>
        </div>
        <div class="step-wizard-item" id="progress-2">
            <div class="progress-count">2</div>
            <div>Invoice</div>
        </div>
        <div class="step-wizard-item" id="progress-3">
            <div class="progress-count">3</div>
            <div>Items</div>
        </div>
        <div class="step-wizard-item" id="progress-4">
            <div class="progress-count">4</div>
            <div>Payment</div>
        </div>
    </div>

    <asp:UpdatePanel ID="UpdMsg" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="PanelOK" runat="server" Visible="false" CssClass="alert alert-success" Style="background: #d4edda; padding: 10px; color: #155724; border: 1px solid #c3e6cb; margin-bottom: 10px;">
                <asp:Label ID="lblOk" runat="server"></asp:Label>
            </asp:Panel>
            <asp:Panel ID="PanelError" runat="server" Visible="false" CssClass="alert alert-danger" Style="background: #f8d7da; padding: 10px; color: #721c24; border: 1px solid #f5c6cb; margin-bottom: 10px;">
                <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div id="step-1" class="wizard-step active">
        <div class="form-section">
            <div class="section-title">Select Vendor</div>
            <asp:UpdatePanel ID="UpdVendor" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="row-flex">
                        <div class="col-flex" style="flex: 2;">
                            <label>Search & Select Vendor *</label>
                            <div class="search-container">
                                <input type="text" id="txtVendorFilter" placeholder="Type vendor name..." class="search-input"
                                    onkeyup="filterDropdown('txtVendorFilter', '<%= cmbvendor.ClientID %>')" />
                            </div>
                            <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style" AutoPostBack="True" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                        <div class="col-flex">
                            <label>Vendor ID</label><br />
                            <asp:Label ID="lblvendor_id" runat="server" Text="--" Font-Bold="true" ForeColor="#666"></asp:Label>
                        </div>
                    </div>
                    <div class="row-flex" style="margin-top: 10px;">
                        <div class="col-flex">
                            <label>Address</label><asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" Enabled="false"></asp:TextBox>
                        </div>
                        <div class="col-flex">
                            <label>City / State</label><asp:TextBox ID="cmbcity" runat="server" CssClass="textbox_style" Enabled="false" Width="48%"></asp:TextBox>
                            <asp:TextBox ID="cmbState" runat="server" CssClass="textbox_style" Enabled="false" Width="48%"></asp:TextBox>
                        </div>
                    </div>
                    <asp:TextBox ID="txtAddress2" runat="server" Visible="false"></asp:TextBox>
                    <asp:TextBox ID="txtPin" runat="server" Visible="false"></asp:TextBox>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div style="text-align: right; margin-top: 15px;">
                <button type="button" class="btn-nav btn-next" onclick="nextStep(2)">Next &raquo;</button>
            </div>
        </div>
    </div>

    <div id="step-2" class="wizard-step">
        <div class="form-section">
            <div class="section-title">Invoice & Shipping Details</div>
            <div class="row-flex">
                <div class="col-flex">
                    <label>Invoice No *</label><asp:TextBox ID="txt_invno" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="col-flex">
                    <label>Purchase Date *</label><asp:TextBox ID="txtPurchesDate" runat="server" CssClass="datepicker textbox_U_style"></asp:TextBox>
                </div>
                <div class="col-flex">
                    <label>Stock Rec. Date</label><asp:TextBox ID="txt_stockadddate" runat="server" CssClass="datepicker textbox_U_style"></asp:TextBox>
                </div>
            </div>
            <div class="row-flex">
                <div class="col-flex">
                    <label>Shipped To Store *</label><asp:DropDownList ID="DDL_ShippedTo" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                </div>
                <div class="col-flex">
                    <label>Ref Order No</label><asp:TextBox ID="txt_reforder" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="col-flex">
                    <label>Ref Date</label><asp:TextBox ID="txt_refordrdate" runat="server" CssClass="datepicker textbox_U_style"></asp:TextBox>
                </div>
            </div>
            <div class="row-flex">
                <div class="col-flex">
                    <label>Narration</label><asp:TextBox ID="txt_narration" runat="server" CssClass="textbox_U_style" TextMode="MultiLine" Rows="2"></asp:TextBox>
                </div>
            </div>
            <div style="text-align: right; margin-top: 15px;">
                <button type="button" class="btn-nav btn-prev" onclick="prevStep(1)">&laquo; Back</button>
                <button type="button" class="btn-nav btn-next" onclick="nextStep(3)">Next &raquo;</button>
            </div>
        </div>
    </div>

    <div id="step-3" class="wizard-step">
        <asp:UpdatePanel ID="UpdItems" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <div class="form-section">
                    <div class="section-title">Add Product / Service</div>
                    <div class="row-flex" style="align-items: center; margin-bottom: 10px;">
                        <div class="col-flex" style="flex: 0.5;">
                            <label>Type:</label><br />
                            <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonList1_SelectedIndexChanged">
                                <asp:ListItem Selected="True" Value="Product">Product</asp:ListItem>
                                <asp:ListItem Value="Service">Service</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                        <div class="col-flex" id="divCategory" runat="server">
                            <label>Category Filter:</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="dropdown_style" AutoPostBack="true" OnSelectedIndexChanged="ddlCategory_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>
                    <div class="row-flex" style="align-items: flex-end;">
                        <div class="col-flex" style="flex: 3;">
                            <label>Search & Select Item:</label>
                            <div class="search-container">
                                <input type="text" id="txtItemFilter" placeholder="Type to filter..." class="search-input"
                                    onkeyup="filterDropdown('txtItemFilter', '<%= cmbproduct_service.ClientID %>')" />
                            </div>
                            <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                        </div>
                        <div class="col-flex" style="flex: 0.5; min-width: 100px;">
                            <asp:Button ID="Button2" runat="server" Text="+ Add" CssClass="btn-nav btn-next" OnClick="Button2_Click" Width="100%" CausesValidation="false" />
                        </div>
                    </div>
                </div>

                <div class="form-section">
                    <div class="section-title">Item List</div>
                    <div style="overflow-x: auto;">
                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" CssClass="Grid"
                            OnRowDataBound="gd_Service_Product_RowDataBound" OnRowCommand="gd_Service_Product_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="Code">
                                    <ItemTemplate>
                                        <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Eval("Ser_pro_code") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name" ItemStyle-HorizontalAlign="Left">
                                    <ItemTemplate>
                                        <asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Eval("Ser_pro_Name") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Spec">
                                    <ItemTemplate>
                                        <asp:TextBox ID="sepecification" runat="server" CssClass="textbox_style" Width="70px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" CssClass="textbox_style" Width="40px" onkeyup="calculateRow(this)"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Vendor_rate" runat="server" CssClass="textbox_style" Width="60px" onkeyup="calculateRow(this)"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Disc %">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DiscountPercent" runat="server" CssClass="textbox_style" Width="35px" onkeyup="calculateRow(this)"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Disc Amt">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DiscountAmount" runat="server" CssClass="textbox_style" Width="50px" ReadOnly="true"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Taxable">
                                    <ItemTemplate>
                                        <asp:TextBox ID="TaxableAmount" runat="server" CssClass="textbox_style" Width="60px" ReadOnly="true"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Tax App.">
                                    <ItemTemplate>
                                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" onchange="calculateGrandTotal()">
                                            <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                            <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Tax %">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="textbox_style" onchange="calculateGrandTotal()"></asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Order">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtOrder" runat="server" Width="30px" Text='<%# Eval("Order") %>' CssClass="textbox_style"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="100px">
                                    <ItemTemplate>
                                        <div style="display: flex; gap: 3px; justify-content: center;">
                                            <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn-action btn-up">&#9650;</asp:LinkButton>
                                            <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn-action btn-down">&#9660;</asp:LinkButton>
                                            <asp:LinkButton ID="btnRemove" runat="server" CommandName="RemoveItem" CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn-action btn-remove" OnClientClick="return confirm('Remove?');">&#10006;</asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <div class="row-flex">
                    <div class="col-flex" style="flex: 1.5;">
                        <div class="form-section">
                            <div class="section-title">Additional Charges</div>
                            <div class="row-flex">
                                <div class="col-flex">
                                    <label>Delivery (Bi-directional)</label>
                                    <div class="charge-input-group">
                                        <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_U_style" onkeyup="calcDelivery('amt')" placeholder="Amount"></asp:TextBox>
                                        <asp:TextBox ID="txt_delivery_percent" runat="server" CssClass="textbox_U_style" Width="60px" onkeyup="calcDelivery('pct')" placeholder="%"></asp:TextBox><span>%</span>
                                    </div>
                                    <asp:DropDownList ID="DDL_vat_parsentage" runat="server" Style="display: none;"></asp:DropDownList>
                                </div>
                                <div class="col-flex">
                                    <label>TCS (Bi-directional)</label>
                                    <div class="charge-input-group">
                                        <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_U_style" onkeyup="calcTCS('amt')" placeholder="Amount"></asp:TextBox>
                                        <asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_U_style" Width="60px" onkeyup="calcTCS('pct')" placeholder="%"></asp:TextBox><span>%</span>
                                    </div>
                                </div>
                            </div>
                            <div class="row-flex">
                                <div class="col-flex">
                                    <label>Other Charge 1 (Taxable)</label>
                                    <asp:TextBox ID="TextBox1" runat="server" Placeholder="Desc. e.g. Insurance" CssClass="textbox_U_style" onkeyup="calculateGrandTotal()" Style="margin-bottom: 5px;"></asp:TextBox>
                                    <asp:TextBox ID="txt_othr_amnt1" runat="server" Placeholder="Amount" CssClass="textbox_U_style" onkeyup="calculateGrandTotal()"></asp:TextBox>
                                </div>
                                <div class="col-flex">
                                    <label>Other Charge 2 (Non-Taxable)</label>
                                    <asp:TextBox ID="TextBox2" runat="server" Placeholder="Desc. e.g. Loading" CssClass="textbox_U_style" onkeyup="calculateGrandTotal()" Style="margin-bottom: 5px;"></asp:TextBox>
                                    <asp:TextBox ID="txt_othr_amnt2" runat="server" Placeholder="Amount" CssClass="textbox_U_style" onkeyup="calculateGrandTotal()"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-flex" style="flex: 1;">
                        <div class="summary-container">
                            <div style="text-align: right; margin-top: 10px; margin-bottom: 10px;">
                                <asp:LinkButton ID="btnRecalculate" runat="server" CssClass="btn-action"
                                    Style="background-color: #ffc107!important; color: #000!important; padding: 5px 10px; font-weight: bold; text-decoration: none; border: 1px solid #e0a800;"
                                    OnClick="btnRecalculate_Click" ToolTip="Save Grid Inputs & Recalculate Totals">
        &#x21bb; Update & Recalculate
                                </asp:LinkButton>
                            </div>
                            <div class="section-title">Net Amount Breakdown</div>
                            <div class="summary-row"><span>Total Basic:</span> <span id="lblSummaryBasic">0.00</span></div>
                            <div class="summary-row"><span>Total Tax:</span> <span id="lblSummaryTax">0.00</span></div>
                            <div style="border-top: 1px dashed #ccc; margin: 10px 0;"></div>
                            <div class="summary-row"><span>+ Delivery:</span> <span id="lblExtrasDelivery">0.00</span></div>
                            <div class="summary-row"><span>+ TCS:</span> <span id="lblExtrasTCS">0.00</span></div>
                            <div class="summary-row sub-item" id="rowOther1" style="display: none;"><span id="lblOther1Name">+ Other 1:</span> <span id="lblOther1Amt">0.00</span></div>
                            <div class="summary-row sub-item" id="rowOther2" style="display: none;"><span id="lblOther2Name">+ Other 2:</span> <span id="lblOther2Amt">0.00</span></div>
                            <div class="summary-row" style="font-weight: bold; color: #19658A;"><span>Total Extras:</span> <span id="lblSummaryExtras">0.00</span></div>
                            <div class="summary-row total"><span>NET PAYABLE:</span> <span id="lblSummaryGrand">0.00</span></div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div style="text-align: right; margin-top: 20px;">
            <button type="button" class="btn-nav btn-prev" onclick="prevStep(2)">&laquo; Back</button>
            <button type="button" class="btn-nav btn-next" onclick="nextStep(4)">Next &raquo;</button>
        </div>
    </div>

    <div id="step-4" class="wizard-step">
        <asp:UpdatePanel ID="UpdPayment" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="form-section">
                    <div class="section-title">Final Payment</div>
                    <div style="text-align: center; margin-bottom: 20px;">
                        <span style="font-size: 14px; color: #666;">Total Payable Amount:</span><br />
                        <asp:Label ID="lblpaayment_amount" runat="server" Text="0.00" Font-Size="24px" Font-Bold="true" ForeColor="#19658A"></asp:Label>
                        <asp:TextBox ID="txt_inv_amount" runat="server" Style="display: none;"></asp:TextBox>
                    </div>
                    <div class="row-flex" style="justify-content: center; margin-bottom: 20px;">
                        <div class="col-flex" style="flex: 0; min-width: 300px;">
                            <label>Make Payment Now?</label><br />
                            <asp:RadioButtonList ID="RadioButtonList3" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonList2_SelectedIndexChanged">
                                <asp:ListItem Value="Yes">Yes</asp:ListItem>
                                <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                    </div>
                    <asp:Panel ID="PaymentDetailsPanel" runat="server" Visible="false" Style="background: #f9f9f9; padding: 15px; border-radius: 5px;">
                        <div class="row-flex">
                            <div class="col-flex">
                                <label>Amount Paid</label><asp:TextBox ID="txtpaymentamount" runat="server" CssClass="textbox_style"></asp:TextBox>
                            </div>
                            <div class="col-flex">
                                <label>Payment Date</label><asp:TextBox ID="txtpaymentdate" runat="server" CssClass="datepicker textbox_style"></asp:TextBox>
                            </div>
                        </div>
                        <div class="row-flex">
                            <div class="col-flex">
                                <label>Mode</label>
                                <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonList2_SelectedIndexChanged">
                                    <asp:ListItem Selected="True">Cash</asp:ListItem>
                                    <asp:ListItem>Cheque</asp:ListItem>
                                    <asp:ListItem>DD</asp:ListItem>
                                    <asp:ListItem>Online</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>
                        </div>
                        <div id="First" runat="server">
                            <label>Cash Date:</label>
                            <asp:TextBox ID="txtcashDate" runat="server" CssClass="datepicker textbox_style"></asp:TextBox>
                        </div>
                        <div id="Second" runat="server" visible="false">
                            <label>Cheque/DD No:</label>
                            <asp:TextBox ID="txtDDno" runat="server" CssClass="textbox_style"></asp:TextBox>
                            <label>Bank:</label>
                            <asp:TextBox ID="txtBankName" runat="server" CssClass="textbox_style"></asp:TextBox>
                            <label>Date:</label>
                            <asp:TextBox ID="txtdddate" runat="server" CssClass="datepicker textbox_style"></asp:TextBox>
                        </div>
                        <div id="Third" runat="server" visible="false">
                            <label>NEFT Ref:</label>
                            <asp:TextBox ID="txtneftnumber" runat="server" CssClass="textbox_style"></asp:TextBox>
                            <label>Bank:</label>
                            <asp:TextBox ID="txtbankname1" runat="server" CssClass="textbox_style"></asp:TextBox>
                            <label>Date:</label>
                            <asp:TextBox ID="txtneftdate" runat="server" CssClass="datepicker textbox_style"></asp:TextBox>
                        </div>
                    </asp:Panel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div style="text-align: right; margin-top: 20px;">
            <button type="button" class="btn-nav btn-prev" onclick="prevStep(3)">&laquo; Back</button>
            <asp:Button ID="Button3" runat="server" Text="Submit Purchase" CssClass="btn-nav btn-success" OnClick="Button3_Click" OnClientClick="return confirm('Confirm Purchase Submission?');" />
        </div>
    </div>
</asp:Content>
