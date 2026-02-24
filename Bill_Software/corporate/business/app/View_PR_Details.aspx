<%@ Page Title="Modify/View PR" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PR_Details.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PR_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .style3 { color: #FF3300; }
        .Grid td { text-align: center; font-size: 11px; border: 1px solid #2D2D2D; padding: 4px; }
        .textbox_style21 { text-align: center; }
        .field-error { border: 2px solid #d9534f !important; background-color: #fff0f0; }
        .pr-summary { margin-top: 10px; padding: 10px; background: #f4f9ff; border: 1px solid #cfe3ff; font-size: 12px; font-weight:bold; }
        .delete-link { color: #d9534f; font-weight: bold; cursor: pointer; }
        .delete-link:hover { color: #a94442; text-decoration: underline; }
        
        /* Fix Select2 white text issue */
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            color: #333333 !important; 
            line-height: 28px;
            text-align: left;
        }
        .select2-container--default .select2-results__option {
            color: #333333 !important; 
            text-align: left;
        }
        .select2-container--default .select2-results__option--highlighted[aria-selected] {
            background-color: #19658A !important; 
            color: #ffffff !important;
        }
        
        /* Wizard Styles */
        .wizard-container { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .wizard-steps { display: flex; border-bottom: 2px solid #ddd; margin-bottom: 20px; }
        .wizard-step { flex: 1; text-align: center; padding: 15px; font-weight: bold; color: #999; cursor: pointer; transition: 0.3s; }
        .wizard-step.active { color: #19658A; border-bottom: 4px solid #19658A; }
        .wizard-step:hover { background: #f9f9f9; }
        .step-content { display: none; }
        .step-content.active { display: block; animation: fadeIn 0.5s; }
        .wizard-footer { margin-top: 20px; text-align: right; padding-top: 15px; border-top: 1px solid #eee; }
        .approval-box { background-color: #fdf5e6; border: 1px solid #faebcc; padding: 15px; margin-top: 20px; border-radius: 5px; }
        @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    </style>

    <script type="text/javascript">
        function pageLoad() {
            $('.select2-enable').select2({ width: '100%' });
            var hdnStep = document.getElementById('<%= hdnActiveStep.ClientID %>');
            if (hdnStep && hdnStep.value) { restoreStep(hdnStep.value); }
            applyInactiveRowStyle();
        }

        function restoreStep(stepIndex) {
            $('.wizard-step').removeClass('active');
            $('.step-content').removeClass('active');
            $('#tab' + stepIndex).addClass('active');
            $('#step' + stepIndex).addClass('active');
        }

        function showStep(stepIndex) {
            restoreStep(stepIndex);
            var hdnStep = document.getElementById('<%= hdnActiveStep.ClientID %>');
            if (hdnStep) hdnStep.value = stepIndex;
        }

        function searchProductGrid() {
            var filter = document.getElementById('<%= txtProductSearch.ClientID %>').value.toLowerCase();
            var grid = document.getElementById('<%= gvProductsToSelect.ClientID %>');
            if (!grid) return;
            var rows = grid.getElementsByTagName("tr");
            for (var i = 1; i < rows.length; i++) {
                var text = rows[i].innerText.toLowerCase();
                if (filter === "" || text.indexOf(filter) > -1) {
                    rows[i].style.display = "";
                } else {
                    rows[i].style.display = "none";
                }
            }
        }

        function toggleAllProducts(source) {
            var checkboxes = document.querySelectorAll('.product-checkbox input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) {
                var tr = checkboxes[i].closest('tr');
                if (tr.style.display !== "none") { checkboxes[i].checked = source.checked; }
            }
        }

        var serviceSearchTimer = null;
        function debouncedSearchServiceGrid() {
            clearTimeout(serviceSearchTimer);
            serviceSearchTimer = setTimeout(function () { searchServiceGrid(); }, 300);
        }

        function getRowSearchText(row) {
            var text = row.innerText || "";
            var inputs = row.querySelectorAll("input[type='text']");
            inputs.forEach(function (inp) { text += " " + inp.value; });
            return text.toLowerCase();
        }

        function searchServiceGrid() {
            var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
            var filter = input.value.trim().toLowerCase();
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if(!grid) return;
            var rows = grid.getElementsByTagName("tr");
            var matchCount = 0;

            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];
                var rowText = getRowSearchText(row);
                if (filter === "" || rowText.indexOf(filter) > -1) {
                    row.style.display = "";
                    if (filter !== "") matchCount++;
                } else {
                    row.style.display = "none";
                }
            }
            var lblNoRec = document.getElementById("lblNoServiceRecords");
            if(lblNoRec) lblNoRec.style.display = (filter !== "" && matchCount === 0) ? "block" : "none";
        }

        function clearServiceGridSearch() {
            var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
            input.value = "";
            searchServiceGrid();
        }

        let modifiedCount = 0;
        function markRowModified(ctrl) {
            if (ctrl && ctrl.id && ctrl.id.indexOf("sepecification") !== -1) return; 
            
            const row = ctrl.closest("tr");
            if (!row) return;
            const hidden = row.querySelector("input[type='hidden'][id*='hdnIsModified']");
            if (!hidden || hidden.value === "1") return;

            hidden.value = "1";
            modifiedCount++;
            const badge = row.querySelector("span[data-modified='1']");
            if (badge) badge.style.display = "inline";
            
            row.style.backgroundColor = "#fff7cc";
            row.style.opacity = "1";

            const lbl = document.getElementById("lblModifiedCount");
            if (lbl) lbl.innerHTML = modifiedCount;
        }

        function showModifiedOnly() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            const rows = grid.getElementsByTagName("tr");
            let found = false;
            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[type='hidden'][id*='hdnIsModified']");
                if (hdn && hdn.value === "1") {
                    rows[i].style.display = "";
                    found = true;
                } else {
                    rows[i].style.display = "none";
                }
            }
            const lbl = document.getElementById("lblNoServiceRecords");
            if (lbl) lbl.style.display = found ? "none" : "inline";
        }

        function showAllRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            const rows = grid.getElementsByTagName("tr");
            for (let i = 1; i < rows.length; i++) { rows[i].style.display = ""; }
            const lbl = document.getElementById("lblNoServiceRecords");
            if (lbl) lbl.style.display = "none";
        }

        function calculateDiscount(changedInput) {
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
                    } else if (changedInput === amountInput) {
                        percent = total !== 0 ? ((amount / total) * 100) : 0;
                        percentInput.value = percent.toFixed(2);
                    }

                    var taxable = total - amount;
                    if (taxableAmountInput) taxableAmountInput.value = taxable.toFixed(2);

                    markRowModified(changedInput);
                    recalcSummary();
                    break;
                }
            }
        }

        function recalcSummary() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            let gross = 0, discount = 0, taxable = 0, gst = 0;
            const rows = grid.getElementsByTagName("tr");

            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                if (row.style.display === "none") continue; 

                const qty = row.querySelector("[id*='Quantity']");
                const rate = row.querySelector("[id*='Vendor_rate']");
                const disc = row.querySelector("[id*='DiscountAmount']");
                const tax = row.querySelector("[id*='TaxableAmount']");
                const gstDDL = row.querySelector("[id*='vat_parsentage']");
                const chkTax = row.querySelector("input[id*='chkTaxApplicable']");

                if (!qty || !rate) continue;

                let q = parseFloat(qty.value) || 0;
                let r1 = parseFloat(rate.value) || 0;
                let d = parseFloat(disc ? disc.value : 0) || 0;
                let t = parseFloat(tax ? tax.value : 0) || 0;
                let g = parseFloat(gstDDL ? gstDDL.value : 0) || 0;

                gross += (q * r1);
                discount += d;
                taxable += t;

                if (chkTax && chkTax.checked) {
                    gst += (t * g / 100);
                }
            }

            document.getElementById('<%= lblGross.ClientID %>').innerText = gross.toFixed(2);
            document.getElementById('<%= lblDiscount.ClientID %>').innerText = discount.toFixed(2);
            document.getElementById('<%= lblTaxable.ClientID %>').innerText = taxable.toFixed(2);
            document.getElementById('<%= lblGST.ClientID %>').innerText = gst.toFixed(2);
            document.getElementById('<%= lblNet.ClientID %>').innerText = (taxable + gst).toFixed(2);
        }

        function validateModifiedRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if(!grid) return true;
            const rows = grid.getElementsByTagName("tr");
            let hasError = false;
            let firstErrorRow = null;

            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const hdn = row.querySelector("input[type='hidden'][id*='hdnIsModified']");
                if (!hdn || hdn.value !== "1") continue;

                const qty = row.querySelector("[id*='Quantity']");
                const rate = row.querySelector("[id*='Vendor_rate']");
                const gst = row.querySelector("[id*='vat_parsentage']");
                const chkTax = row.querySelector("input[id*='chkTaxApplicable']");

                [qty, rate, gst].forEach(c => { if (c) c.classList.remove("field-error"); });
                let rowHasError = false;

                if (chkTax && chkTax.checked && (!gst || gst.value === "" || gst.value === "NA")) {
                    gst.classList.add("field-error"); rowHasError = true;
                }
                if (!qty || qty.value.trim() === "" || Number(qty.value) <= 0) {
                    qty.classList.add("field-error"); rowHasError = true;
                }
                if (!rate || rate.value.trim() === "" || Number(rate.value) < 0) {
                    rate.classList.add("field-error"); rowHasError = true;
                }

                if (rowHasError) {
                    hasError = true;
                    if (!firstErrorRow) firstErrorRow = row;
                }
            }

            if (hasError) {
                if (firstErrorRow) firstErrorRow.scrollIntoView({ behavior: "smooth", block: "center" });
                alert("Please correct highlighted fields in modified rows.");
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
                    rows[i].style.opacity = "0.85"; 
                }
            }
        }
        
        function scrollToMessage() {
            const ok = document.getElementById('<%= PanelOK.ClientID %>');
            const err = document.getElementById('<%= PanelError.ClientID %>');
            if (ok && ok.style.display !== "none") ok.scrollIntoView({ behavior: "smooth", block: "center" });
            if (err && err.style.display !== "none") err.scrollIntoView({ behavior: "smooth", block: "center" });
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:HiddenField ID="hdnActiveStep" runat="server" Value="1" />

            <table class="style1">
                <tr><td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">View/Modify Purchase Requisition</span>&nbsp;</td></tr>
            </table>
            
            <div style="margin:15px 0;">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding:10px;">
                    <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    <asp:Label ID="lblOk" runat="server" ForeColor="Green" Font-Bold="true"></asp:Label>
                </asp:Panel>
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding:10px; background-color:#fff0f0;">
                    <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    <asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
                </asp:Panel>
            </div>

            <table width="100%" cellpadding="10" cellspacing="0" style="border: 1px solid #006699; background-color: #F4FAFF; margin-bottom: 20px;">
                <tr>
                    <td style="width: 20%; font-weight: bold;">PR No:</td>
                    <td style="width: 30%;"><asp:Label ID="lblReqNo" runat="server" Text="(Not Generated)" Style="font-weight: bold; color: #003366;" /></td>
                    <td style="width: 20%; font-weight: bold;">Status:</td>
                    <td style="width: 30%;"><asp:Label ID="lblStatus" runat="server" Text="Draft" Style="font-weight: bold; color: #CC6600;" /></td>
                </tr>
            </table>

            <div class="wizard-container">
                <div class="wizard-steps">
                    <div id="tab1" class="wizard-step active" onclick="showStep(1)" runat="server" ClientIDMode="Static">1. Vendor Info</div>
                    <div id="tab2" class="wizard-step" onclick="showStep(2)" runat="server" ClientIDMode="Static">2. Add Items</div>
                    <div id="tab3" class="wizard-step" onclick="showStep(3)" runat="server" ClientIDMode="Static">3. Review & Submit</div>
                </div>

                <div id="step1" class="step-content active" runat="server" ClientIDMode="Static">
                    <table class="style1" cellpadding="6">
                        <tr>
                            <td width="20%"><span class="style3">*</span>Vendor Name</td>
                            <td width="30%">
                                <asp:DropDownList ID="cmbvendor" runat="server" AutoPostBack="True" CssClass="dropdown_style select2-enable" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged"></asp:DropDownList>
                                <asp:Label ID="lbl_vendordbid" runat="server" Visible="false"></asp:Label>
                                <asp:Label ID="lblvendor_id" runat="server" Visible="False"></asp:Label>
                            </td>
                            <td width="20%">City</td>
                            <td width="30%"><asp:TextBox ID="cmbcity" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Address 1</td>
                            <td><asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox></td>
                            <td>State</td>
                            <td><asp:TextBox ID="cmbState" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>Phone No</td>
                            <td><asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox></td>
                            <td>Email ID</td>
                            <td><asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox></td>
                        </tr>
                    </table>
                    <div class="wizard-footer">
                        <asp:Button ID="btnNextToStep2" runat="server" Text="Next: Add Items &raquo;" CssClass="btn_style" OnClientClick="showStep(2); return false;" />
                        <asp:Button ID="btnNextToStep3From1" runat="server" Text="Next: Review PR &raquo;" CssClass="btn_style" OnClientClick="showStep(3); return false;" Visible="false" />
                    </div>
                </div>

                <div id="step2" class="step-content" runat="server" ClientIDMode="Static">
                    <table class="style1" cellpadding="8">
                        <tr>
                            <td width="15%"><strong>Purchase Type:</strong></td>
                            <td width="35%">
                                <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonList1_SelectedIndexChanged">
                                    <asp:ListItem Selected="True" Value="Product">Product</asp:ListItem>
                                    <asp:ListItem Value="Service">Service</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                            <td width="15%"><strong>Category:</strong></td>
                            <td width="35%">
                                <asp:DropDownList ID="cmbproduct_service" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cmbproduct_service_SelectedIndexChanged" CssClass="dropdown_style select2-enable"></asp:DropDownList>
                            </td>
                        </tr>
                    </table>

                    <div style="margin-top: 15px;" id="SearchBox_Row" runat="server">
                        <asp:TextBox ID="txtProductSearch" runat="server" CssClass="textbox_U_style" Width="300px" placeholder="Search products in this category..." onkeyup="searchProductGrid()" />
                        
                        <div style="max-height: 250px; overflow-y: auto; margin-top: 10px; border: 1px solid #ccc;">
                            <asp:GridView ID="gvProductsToSelect" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" DataKeyNames="ItemId" EmptyDataText="No products found in this category or all are already added.">
                                <HeaderStyle BackColor="#19658A" Font-Bold="True" ForeColor="White" />
                                <Columns>
                                    <asp:TemplateField>
                                        <HeaderTemplate><input type="checkbox" id="chkAll" onclick="toggleAllProducts(this)" title="Select All Visible" /></HeaderTemplate>
                                        <ItemTemplate><asp:CheckBox ID="chkSelect" runat="server" CssClass="product-checkbox" /></ItemTemplate>
                                        <ItemStyle Width="40px" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ItemId" HeaderText="Item Code" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="ItemName" HeaderText="Item Name" ItemStyle-HorizontalAlign="Left" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <div style="text-align: right; padding-top: 15px;">
                        <asp:Button ID="Button2" runat="server" Text="+ Add Selected Items to Grid" CssClass="btn_style" BackColor="#19658A" ForeColor="White" Padding="8px" OnClick="Button2_Click" />
                    </div>

                    <div class="wizard-footer">
                        <button type="button" class="btn_style" style="float:left; padding:8px 20px;" onclick="showStep(1)">&laquo; Back</button>
                        <button type="button" class="btn_style" style="padding:8px 20px;" onclick="showStep(3)">Next: Review & Submit &raquo;</button>
                    </div>
                </div>

                <div id="step3" class="step-content" runat="server" ClientIDMode="Static">
                    
                    <div style="text-align: center; margin-bottom: 15px;" id="Step3SearchDiv" runat="server">
                        <asp:TextBox ID="txtServiceSearch" runat="server" CssClass="textbox_U_style" Width="260px" placeholder="Search selected items..." onkeyup="debouncedSearchServiceGrid()" />
                        <asp:Button ID="btnClearServiceSearch" runat="server" Text="Clear" CssClass="btn_style" OnClientClick="clearServiceGridSearch(); return false;" />
                    </div>

                    <div style="text-align: right; margin-bottom: 10px;" id="Modifier_Msg_Row" runat="server">
                        <span style="font-weight: bold; color: #d9534f;">Modified Items: <span id="lblModifiedCount">0</span></span>
                        <asp:Button ID="btnShowModified" runat="server" Text="Show Modified" CssClass="btn btn-warning btn-sm btn_style" OnClientClick="showModifiedOnly(); return false;" />
                        <asp:Button ID="btnShowAll" runat="server" Text="Show All" CssClass="btn btn-secondary btn-sm btn_style" OnClientClick="showAllRows(); return false;" />
                    </div>

                    <div style="overflow-x: auto;">
                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" Width="100%" OnRowDataBound="gd_Service_Product_RowDataBound" OnRowCommand="gd_Service_Product_RowCommand">
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <Columns>
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate><span data-modified="1" style="display: none; color:red; font-weight:bold;">✱</span></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Code">
                                    <ItemTemplate>
                                        <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Eval("Ser_pro_code") %>'></asp:Label>
                                        <asp:HiddenField ID="hdnIsModified" runat="server" Value='<%# Convert.ToBoolean(Eval("IsModified")) ? "1" : "0" %>' />
                                        <asp:HiddenField ID="hdnParentCategoryId" runat="server" Value='<%# Eval("ParentCategoryId") %>' />
                                        <asp:HiddenField ID="hdnRowId" runat="server" Value='<%# Eval("id") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name">
                                    <ItemTemplate><asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Eval("Ser_pro_Name") %>'></asp:Label></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Specification">
                                    <ItemTemplate>
                                        <asp:TextBox ID="sepecification" runat="server" Text='<%# Eval("Description") %>' CssClass="textbox_style21" Width="150px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" Text='<%# Eval("Qnty") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21" Width="60px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Vendor_rate" runat="server" Text='<%# Eval("Rate") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21" Width="80px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Dis. %">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DiscountPercent" runat="server" Text='<%# Eval("DiscountPercent") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21" Width="50px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Disc. Amt">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DiscountAmount" runat="server" Text='<%# Eval("DiscountAmount") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21" Width="60px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Taxable Amt">
                                    <ItemTemplate>
                                        <asp:TextBox ID="TaxableAmount" runat="server" Text='<%# Eval("TaxableAmount") %>' ReadOnly="true" CssClass="textbox_style21" Width="80px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Tax Applic.">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkTaxApplicable" runat="server" Text="Yes" Checked='<%# Eval("IsTaxApplicable") != DBNull.Value && Convert.ToBoolean(Eval("IsTaxApplicable")) %>' onchange="markRowModified(this); recalcSummary();" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="GST %">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="dropdown_style" onchange="markRowModified(this); recalcSummary();"></asp:DropDownList>
                                        <asp:HiddenField ID="hdnSelectedGST" runat="server" Value='<%# Eval("gstrate") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Order">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtOrder" runat="server" Text='<%# Eval("ItemOrder") %>' onkeyup="markRowModified(this)" CssClass="textbox_style21" Width="50px" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkDelete" runat="server" Text="Delete" CommandName="DeleteItem" CommandArgument='<%# Eval("id") %>' OnClientClick="return confirm('Delete this item?');" CssClass="delete-link" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <asp:Panel ID="pnlSummary" runat="server" CssClass="pr-summary">
                        <table width="100%">
                            <tr>
                                <td align="right" width="20%">Gross Amount :</td>
                                <td width="30%"><asp:Label ID="lblGross" runat="server" /></td>
                                <td align="right" width="20%">Discount :</td>
                                <td width="30%"><asp:Label ID="lblDiscount" runat="server" /></td>
                            </tr>
                            <tr>
                                <td align="right">Taxable Amount :</td>
                                <td><asp:Label ID="lblTaxable" runat="server" /></td>
                                <td align="right">GST Amount :</td>
                                <td><asp:Label ID="lblGST" runat="server" /></td>
                            </tr>
                            <tr style="font-weight: bold; color:#19658A; font-size:14px;">
                                <td align="right">Net Amount :</td>
                                <td><asp:Label ID="lblNet" runat="server" /></td>
                                <td></td><td></td>
                            </tr>
                        </table>
                    </asp:Panel>

                    <asp:Panel ID="pnlApproval" runat="server" Visible="false" CssClass="approval-box">
                        <h4 style="color:#8a6d3b; margin-top:0;">Approval Action</h4>
                        <asp:TextBox ID="txtApprovalRemarks" runat="server" TextMode="MultiLine" Width="100%" Height="60px" placeholder="Remarks (optional)" style="margin-bottom:10px; border:1px solid #ccc; padding:5px;" />
                        <asp:Button ID="btnApprove" runat="server" Text="Approve PR" CssClass="btn_style" BackColor="#5cb85c" ForeColor="White" OnClick="btnApprove_Click" />
                        &nbsp;
                        <asp:Button ID="btnReject" runat="server" Text="Reject PR" CssClass="btn_style" BackColor="#d9534f" ForeColor="White" OnClick="btnReject_Click" />
                    </asp:Panel>

                    <div class="wizard-footer" id="divActionButtons" runat="server">
                        <asp:Button ID="btnBackToStep2" runat="server" Text="&laquo; Back" CssClass="btn_style" OnClientClick="showStep(2); return false;" style="float:left;" />
                        <asp:Button ID="btnBackToStep1" runat="server" Text="&laquo; Back to Vendor" CssClass="btn_style" OnClientClick="showStep(1); return false;" style="float:left;" Visible="false" />
                        
                        <asp:Button ID="btnSaveDraft" runat="server" Text="Save Edits" CssClass="btn_style" BackColor="#f0ad4e" ForeColor="White" OnClientClick="return validateModifiedRows();" OnClick="btnSaveEdit_Click" />
                        &nbsp;
                        <asp:Button ID="Button3" runat="server" Text="Submit PR" CssClass="btn_style" BackColor="#5cb85c" ForeColor="White" OnClientClick="return validateModifiedRows();" OnClick="Button3_Click" />
                        &nbsp;
                        <asp:Button ID="btnCancelPR" runat="server" Text="Cancel PR" CssClass="btn_style" BackColor="#d9534f" ForeColor="White" OnClick="btnCancelPR_Click" />
                    </div>
                </div>

            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>