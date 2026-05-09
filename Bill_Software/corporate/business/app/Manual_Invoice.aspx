<%@ Page Title="Manual Tax Invoice" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Manual_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.Manual_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }

        /* --- Layout Styles (Synced with Add_Invoice) --- */
        .section-header { background-color: #19658A; color: white; padding: 12px 15px; font-weight: bold; font-size: 16px; margin-bottom: 20px; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .box-panel { border: 1px solid #d1d9e0; border-radius: 6px; padding: 20px; background: #ffffff; margin-bottom: 20px; box-shadow: 0 2px 5px rgba(0,0,0,0.02); }
        .box-title { margin-top: 0; font-size: 15px; color: #006699; border-bottom: 2px solid #f0f4f8; padding-bottom: 8px; margin-bottom: 18px; font-weight: bold; }
        
        .form-grid-5 { display: grid; grid-template-columns: 2fr 2.5fr 1.5fr 1.5fr 1fr; gap: 15px; margin-bottom: 15px; align-items: end; }
        .form-grid-4 { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin-bottom: 15px; align-items: end; }
        .form-grid-3 { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 15px; align-items: end; }
        .form-grid-2 { display: grid; grid-template-columns: repeat(2, 1fr); gap: 20px; margin-bottom: 15px; }
        
        .form-label { display: block; font-weight: bold; margin-bottom: 6px; color: #444; font-size: 12px; text-transform: uppercase; }
        .form-control { width: 100%; padding: 8px 10px; border: 1px solid #ccc; border-radius: 4px; font-size: 13px; box-sizing: border-box; }
        
        .btn-nav { padding: 9px 20px; background-color: #006699; color: white; border: none; cursor: pointer; font-weight: bold; font-size: 13px; border-radius: 4px; }
        .btn-nav:hover { background-color: #004d73; }
        .btn-secondary { background-color: #6c757d; }
        .btn-secondary:hover { background-color: #5a6268; }
        .btn-del { background-color: #dc3545; color: white; border: none; padding: 5px 10px; border-radius: 3px; cursor: pointer; font-weight: bold; }

        .btn-filter { padding: 6px 12px; border: 1px solid #ccc; background-color: #e9ecef; cursor: pointer; font-size: 12px; margin-right: 4px; border-radius: 4px; transition: all 0.2s ease; }
        .btn-filter:hover { background-color: #dde2e6; }
        .btn-filter.active { background-color: #19658A; color: white; border-color: #19658A; box-shadow: inset 0 3px 5px rgba(0,0,0,0.12); }

        /* --- Grid & Totals --- */
        .Grid { width: 100%; border-collapse: collapse; font-size: 12px; }
        .Grid th { background-color: #006699; color: white; padding: 10px; border: 1px solid #004d73; text-align: center; position: sticky; top: 0; z-index: 10; }
        .Grid td { padding: 8px; border: 1px solid #ddd; text-align: center; vertical-align: middle; }

        .cmd-btn { text-decoration: none; font-size: 14px; margin: 0 4px; font-weight: bold; display: inline-block; width: 15px; }
        .cmd-up { color: #006699; }
        .cmd-del { color: red; }

        .total-box { margin-top: 20px; padding: 20px; background-color: #f9fbfd; border: 1px solid #d1d9e0; border-radius: 6px; float: right; width: 400px; }
        .lbl-grand { font-size: 20px; font-weight: bold; color: #28a745; }

        /* --- Select2 Overrides --- */
        .select2-container--default .select2-selection--single { height: 34px; border: 1px solid #ccc; border-radius: 4px; }
        .select2-container--default .select2-selection--single .select2-selection__rendered { line-height: 32px; font-size: 13px; color: #333 !important; }
        .ui-datepicker { z-index: 9999 !important; }
        .select2-results__option { color: #333 !important; background-color: #fff !important; }
        .select2-results__option--highlighted { background-color: #006699 !important; color: #fff !important; }

        .clearfix::after { content: ""; clear: both; display: table; }
    </style>

    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="calender/jquery.ui.core.js"></script>
    <script src="calender/jquery.ui.widget.js"></script>
    <script src="calender/jquery.ui.datepicker.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    
    <script type="text/javascript">
        // jQuery UI Compat
        jQuery.browser = {};
        (function () {
            jQuery.browser.msie = false;
            jQuery.browser.version = 0;
            if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
                jQuery.browser.msie = true;
                jQuery.browser.version = RegExp.$1;
            }
        })();

        // Robust Initialization (Using specific ClientIDs)
        function initScripts() {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });

            var $ddlClient = $('#<%= cmbClient.ClientID %>');
            if ($ddlClient.hasClass("select2-hidden-accessible")) { $ddlClient.select2('destroy'); }
            $ddlClient.select2({ placeholder: "Search Client (Optional)", allowClear: true, width: '100%' });

            var $ddlSales = $('#<%= cmbSalesPerson.ClientID %>');
            if ($ddlSales.hasClass("select2-hidden-accessible")) { $ddlSales.select2('destroy'); }
            $ddlSales.select2({ placeholder: "Search Sales Person...", allowClear: true, width: '100%' });
        }

        // Run on load and after UpdatePanel PostBack
        $(document).ready(function () { initScripts(); });
        function pageLoad() { initScripts(); }

        // --- STEP 1: VALIDATION ---
        function validateSetupStep() {
            var taxRadios = document.getElementsByName('<%= rbTaxType.UniqueID %>');
            var isTaxSelected = false;
            for (var i = 0; i < taxRadios.length; i++) {
                if (taxRadios[i].checked) { isTaxSelected = true; break; }
            }
            if (!isTaxSelected) { alert("Action Blocked: Please select a Tax Type (Intra or Inter)."); return false; }

            var salesDropdown = document.getElementById('<%= cmbSalesPerson.ClientID %>');
            if (salesDropdown && salesDropdown.value === "") { alert("Action Blocked: Please select a Sales Person."); return false; }

            return true; 
        }

        // --- STEP 2: SEARCH LOGIC ---
        function FilterGrid() {
            var input = document.getElementById("txtSearchProduct");
            var filter = input.value.toUpperCase();
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            if (!table) return;
            var tr = table.getElementsByTagName("tr");
            for (var i = 1; i < tr.length; i++) {
                var td = tr[i].getElementsByTagName("td")[3];
                if (td) {
                    var txtValue = td.textContent || td.innerText;
                    tr[i].style.display = txtValue.toUpperCase().indexOf(filter) > -1 ? "" : "none";
                }
            }
        }

        function FilterSelection(mode) {
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            if (!table) return;
            var tr = table.getElementsByTagName("tr");
            var btns = document.getElementsByClassName('btn-filter');
            for (var b = 0; b < btns.length; b++) { btns[b].classList.remove('active'); }
            if (event && event.target) { event.target.classList.add('active'); }

            for (var i = 1; i < tr.length; i++) {
                var chk = tr[i].querySelector("input[type='checkbox']");
                if (chk) {
                    if (mode === 'ALL') tr[i].style.display = "";
                    else if (mode === 'SEL') tr[i].style.display = chk.checked ? "" : "none";
                    else if (mode === 'UNSEL') tr[i].style.display = !chk.checked ? "" : "none";
                }
            }
        }

        // --- STEP 3: LIVE MATH ---
        function CalculateRow(input, trigger) {
            var row = input.parentNode.parentNode;
            var txtQty = row.querySelector("input[id*='txtQty']");
            var txtRate = row.querySelector("input[id*='txtRate']");
            var txtDiscPer = row.querySelector("input[id*='txtDiscPer']");
            var txtDiscAmt = row.querySelector("input[id*='txtDiscAmt']");
            var lblGross = row.querySelector("span[id*='lblGross']");
            var lblTaxable = row.querySelector("span[id*='lblTaxable']");
            var lblGst = row.querySelector("span[id*='lblGstRate']");
            var lblTaxAmt = row.querySelector("span[id*='lblTaxAmt']");
            var lblNet = row.querySelector("span[id*='lblNet']");

            if (!txtQty || !txtRate || !lblGross) return;

            var qty = Math.max(0, parseFloat(txtQty.value) || 0);
            var rate = Math.max(0, parseFloat(txtRate.value) || 0);
            var gst = Math.max(0, parseFloat(lblGst ? lblGst.innerText : 0) || 0);

            var gross = qty * rate;
            lblGross.innerText = gross.toFixed(2);

            var discAmt = 0, discPer = 0;
            if (trigger === 'AMT') {
                discAmt = Math.max(0, parseFloat(txtDiscAmt.value) || 0);
                if (discAmt > gross) discAmt = gross;
                if (gross > 0) discPer = (discAmt / gross) * 100;
                if (txtDiscPer) txtDiscPer.value = discPer.toFixed(2);
            } else {
                if (txtDiscPer) discPer = Math.max(0, parseFloat(txtDiscPer.value) || 0);
                if (discPer > 100) discPer = 100;
                discAmt = (gross * discPer) / 100;
                if (txtDiscAmt) txtDiscAmt.value = discAmt.toFixed(2);
            }

            var taxable = Math.max(0, gross - discAmt);
            if (lblTaxable) lblTaxable.innerText = taxable.toFixed(2);
            var taxVal = (taxable * gst) / 100;
            if (lblTaxAmt) lblTaxAmt.innerText = taxVal.toFixed(2);
            var net = taxable + taxVal;
            if (lblNet) lblNet.innerText = net.toFixed(2);

            RecalculateFooter();
        }

        function RecalculateFooter() {
            var grid = document.getElementById("<%= gd_Cart.ClientID %>");
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

            var inputFrt = document.getElementById("<%= txtFreight.ClientID %>");
            var inputOth = document.getElementById("<%= txtOtherCharge.ClientID %>");
            var frt = inputFrt ? Math.max(0, parseFloat(inputFrt.value) || 0) : 0;
            var oth = inputOth ? Math.max(0, parseFloat(inputOth.value) || 0) : 0;

            var outTax = document.getElementById("<%= lblFooterTax.ClientID %>");
            var outGrand = document.getElementById("<%= lblFooterGrand.ClientID %>");
            
            var finalGrandTotal = totalGrand + frt + oth;

            if (outTax) outTax.innerText = totalTax.toFixed(2);
            if (outGrand) outGrand.innerText = finalGrandTotal.toFixed(2);

            var btnSubmit = document.getElementById("<%= btnSave.ClientID %>");
            var warningMsg = document.getElementById("zeroTotalWarning");

            if (btnSubmit && warningMsg) {
                if (finalGrandTotal <= 0) {
                    btnSubmit.disabled = true;
                    btnSubmit.style.backgroundColor = "#cccccc";
                    btnSubmit.style.cursor = "not-allowed";
                    warningMsg.style.display = "block";
                } else {
                    btnSubmit.disabled = false;
                    btnSubmit.style.backgroundColor = "#28a745";
                    btnSubmit.style.cursor = "pointer";
                    warningMsg.style.display = "none";
                }
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div style="width: 98%; margin: auto; padding-top: 10px;">
        <div class="section-header">Create Manual Tax Invoice (Direct)</div>

        <asp:Panel ID="PanelMsg" runat="server" Visible="false" Style="padding: 10px; margin-bottom: 10px; border: 1px solid #ccc; background-color: #fff;">
            <asp:Label ID="lblMsg" runat="server" Font-Bold="true"></asp:Label>
        </asp:Panel>

        <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">

            <asp:View ID="vSetup" runat="server">
                <div class="box-panel">
                    <div class="box-title">1. Invoice Master Details</div>
                    <div class="form-grid-4">
                        <div>
                            <label class="form-label">Client Name <span style="color: red">*</span></label>
                            <asp:DropDownList ID="cmbClient" runat="server" CssClass="form-control select-search" AutoPostBack="true" OnSelectedIndexChanged="cmbClient_SelectedIndexChanged" ClientIDMode="Static"></asp:DropDownList>
                            <asp:Label ID="lblClientID" runat="server" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label class="form-label">Invoice Date <span style="color: red">*</span></label>
                            <asp:TextBox ID="txtInvoiceDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                        </div>
                        <div>
                            <label class="form-label">Tax Type <span style="color: red">*</span></label>
                            <asp:RadioButtonList ID="rbTaxType" runat="server" RepeatDirection="Horizontal" CellPadding="5">
                                <asp:ListItem Value="1" Selected="True">Intra (CGST/SGST)</asp:ListItem>
                                <asp:ListItem Value="0">Inter (IGST)</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                        <div>
                            <label class="form-label">Sales Person <span style="color: red">*</span></label>
                            <asp:DropDownList ID="cmbSalesPerson" runat="server" CssClass="form-control select-search" ClientIDMode="Static"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="form-grid-2">
                        <div>
                            <label class="form-label">Billing & Delivery Addresses <span style="color: red">*</span></label>
                            <asp:ListBox ID="lstAddresses" runat="server" SelectionMode="Multiple" Height="90px" CssClass="form-control"></asp:ListBox>
                            <div style="font-size: 10px; color: #888; margin-top: 4px;">Hold CTRL to select multiple addresses.</div>
                        </div>
                        <div>
                            <div style="margin-bottom: 15px;">
                                <label class="form-label">Client PO No (Optional)</label>
                                <asp:TextBox ID="txtPONo" runat="server" CssClass="form-control" placeholder="e.g., PO-44812"></asp:TextBox>
                            </div>
                            <div>
                                <label class="form-label">ERP / Reference No (Optional)</label>
                                <asp:TextBox ID="txtERPRef" runat="server" CssClass="form-control" placeholder="e.g., EXT-001"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div style="text-align: right; margin-top: 10px; border-top: 1px solid #e2e8f0; padding-top: 15px;">
                        <asp:Button ID="btnNext" runat="server" Text="Next: Add Products &rarr;" CssClass="btn-nav" OnClientClick="return validateSetupStep();" OnClick="btnNext_Click" />
                    </div>
                </div>
            </asp:View>

            <asp:View ID="vProducts" runat="server">
                <div class="box-panel">
                    <div class="box-title">2. Select Products</div>
                    <div class="form-grid-3">
                        <div>
                            <label class="form-label">Filter Category:</label>
                            <asp:DropDownList ID="cmbCategory" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="cmbCategory_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                        <div style="text-align: center;">
                            <label class="form-label">View Options:</label>
                            <button type="button" class="btn-filter active" onclick="FilterSelection('ALL')">View All</button>
                            <button type="button" class="btn-filter" onclick="FilterSelection('SEL')">Selected</button>
                            <button type="button" class="btn-filter" onclick="FilterSelection('UNSEL')">Un-Selected</button>
                        </div>
                        <div>
                            <label class="form-label">Search Name/ID:</label>
                            <input type="text" id="txtSearchProduct" onkeyup="FilterGrid()" placeholder="Type to filter..." class="form-control" />
                        </div>
                    </div>

                    <div style="height: 380px; overflow-y: auto; border: 1px solid #e2e8f0; margin-bottom: 15px;">
                        <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" DataKeyNames="ProductID">
                            <Columns>
                                <asp:TemplateField HeaderText="Select" ItemStyle-Width="40px">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkSelect" runat="server" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ProductID" HeaderText="True ID" Visible="true" ItemStyle-Width="80px" ItemStyle-Font-Bold="true" />
                                <asp:BoundField DataField="Product_code" HeaderText="HSN Code" ItemStyle-Width="90px" />
                                <asp:BoundField DataField="ProductName" HeaderText="Product Name" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField DataField="Brand" HeaderText="Master Spec" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField DataField="Unit" HeaderText="Unit" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="Quantity" HeaderText="In Stock" ItemStyle-Width="70px" ItemStyle-ForeColor="#19658A" ItemStyle-Font-Bold="true" />
                                <asp:TemplateField HeaderText="Base Rate" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblBaseRate" runat="server" Text='<%# Bind("Sail_Rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="GST %" ItemStyle-Width="50px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblGstRate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div style="padding: 15px; color: #777; text-align: center;"><i>No products found for this category.</i></div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <div style="text-align: center; border-top: 1px solid #e2e8f0; padding-top: 15px;">
                        <asp:Button ID="btnBackSetup" runat="server" Text="&larr; Back to Master Details" CssClass="btn-nav btn-secondary" OnClick="btnBackSetup_Click" />
                        &nbsp;&nbsp;
                        <asp:Button ID="btnAddToCart" runat="server" Text="Add Selected & Review Cart &rarr;" CssClass="btn-nav" OnClick="btnAddToCart_Click" />
                    </div>
                </div>
            </asp:View>

            <asp:View ID="vReview" runat="server">
                <div class="box-panel">
                    <div class="box-title">3. Review & Finalize Manual Invoice</div>

                    <div style="margin-bottom: 15px; color: #555; font-size: 13px;">
                        Billing to: <strong style="color: #006699;">
                            <asp:Label ID="lblClientDisplay" runat="server"></asp:Label></strong> | 
                        Tax Mode: <strong>
                            <asp:Label ID="lblTaxModeDisplay" runat="server"></asp:Label></strong>
                    </div>

                    <div style="overflow-x: auto; border: 1px solid #e2e8f0;">
                        <asp:GridView ID="gd_Cart" runat="server" AutoGenerateColumns="False" CssClass="Grid" Style="min-width: 1300px; white-space: nowrap;" OnRowCommand="gd_Cart_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="Act" ItemStyle-Width="70px">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-up" ToolTip="Move Up">▲</asp:LinkButton>
                                        <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-up" ToolTip="Move Down">▼</asp:LinkButton>
                                        <asp:LinkButton ID="btnDel" runat="server" CommandName="Remove" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-del" ToolTip="Remove" OnClientClick="return confirm('Remove row?');">✖</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ProductID" HeaderText="ID" ReadOnly="true" />
                                <asp:BoundField DataField="ProductName" HeaderText="Product" ReadOnly="true" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField DataField="Product_code" HeaderText="HSN" ReadOnly="true" ItemStyle-Width="60px" />

                                <asp:TemplateField HeaderText="Specification" ItemStyle-Width="130px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtSpec" runat="server" Text='<%# Bind("Brand") %>' CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Bill Qty" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQty" runat="server" Text='<%# Bind("IQuantity") %>' CssClass="form-control" Style="text-align: center; font-weight: bold; color: #006699;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Rate" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtRate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="form-control" Style="text-align: right;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Gross" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblGross" runat="server" Text="0.00"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Disc %" ItemStyle-Width="50px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtDiscPer" runat="server" Text='<%# Bind("Discount_Rate") %>' CssClass="form-control" Style="text-align: center;" onkeyup="CalculateRow(this, 'PER')"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Disc Amt" ItemStyle-Width="70px">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtDiscAmt" runat="server" Text="0.00" CssClass="form-control" Style="text-align: right;" onkeyup="CalculateRow(this, 'AMT')"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Taxable" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblTaxable" runat="server" Text="0.00" Font-Bold="true"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="GST%" ItemStyle-Width="40px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblGstRate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Tax Amt" ItemStyle-Width="70px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblTaxAmt" runat="server" Text="0.00"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Net Total" ItemStyle-Width="90px">
                                    <ItemTemplate>
                                        <asp:Label ID="lblNet" runat="server" Text="0.00" Font-Bold="true" ForeColor="Green"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div style="padding: 15px; color: red; font-weight: bold; text-align: center;">Cart is Empty. Please go back and add products.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <div class="total-box">
                        <table width="100%" cellpadding="6" cellspacing="0">
                            <tr>
                                <td width="50%" align="right" style="color: #555; font-weight: bold;">Freight Charges (+)</td>
                                <td width="50%" align="right">
                                    <asp:TextBox ID="txtFreight" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td align="right" style="color: #555; font-weight: bold;">Other Charges (+)</td>
                                <td align="right">
                                    <asp:TextBox ID="txtOtherCharge" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <hr style="border-top: 1px dashed #ccc;" />
                                </td>
                            </tr>
                            <tr>
                                <td align="right" style="color: #555;">Total Tax:</td>
                                <td align="right">
                                    <asp:Label ID="lblFooterTax" runat="server" Text="0.00" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td align="right" style="font-size: 16px;"><strong>Grand Total:</strong></td>
                                <td align="right">
                                    <asp:Label ID="lblFooterGrand" runat="server" Text="0.00" CssClass="lbl-grand"></asp:Label></td>
                            </tr>
                        </table>
                    </div>
                    <div class="clearfix"></div>

                    <div style="text-align: center; margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">

                        <div id="zeroTotalWarning" style="display: none; color: #dc3545; font-weight: bold; font-size: 15px; margin-bottom: 15px; background: #fff3cd; padding: 10px; border: 1px solid #ffeeba; border-radius: 4px;">
                            Cannot generate an invoice with a Grand Total of ₹0.00. Please allocate quantities or charges to proceed.
                        </div>

                        <asp:Button ID="btnBackProd" runat="server" Text="&larr; Back to Products" CssClass="btn-nav btn-secondary" OnClick="btnBackProd_Click" />
                        &nbsp;&nbsp;
                        <asp:Button ID="btnSave" runat="server" Text="Generate Tax Invoice" CssClass="btn-nav" Style="background-color: #28a745;" OnClick="btnSave_Click" OnClientClick="return confirm('Confirm Invoice Generation? This will deduct stock directly from the warehouse.');" />
                    </div>
                </div>
            </asp:View>

        </asp:MultiView>
    </div>
</asp:Content>
