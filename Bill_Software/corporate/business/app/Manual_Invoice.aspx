<%@ Page Title="Manual Tax Invoice" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Manual_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.Manual_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* --- Layout Styles --- */
        .section-header {
            background-color: #19658A;
            color: white;
            padding: 10px;
            font-weight: bold;
            font-size: 14px;
            margin-bottom: 15px;
            border-radius: 4px;
        }

        /* Update your existing .form-control */
        .form-control {
            width: 95%;
            padding: 6px 8px; /* Increased padding for better click area */
            border: 1px solid #ccc;
            border-radius: 4px; /* Slightly rounder */
            font-family: Arial, sans-serif;
            font-size: 13px; /* Bumped up from 11px */
            box-sizing: border-box; /* Ensures padding doesn't break widths */
        }

        .btn-nav {
            padding: 6px 15px;
            background-color: #006699;
            color: white;
            border: none;
            cursor: pointer;
            font-weight: bold;
            font-size: 12px;
            border-radius: 3px;
        }

            .btn-nav:hover {
                background-color: #004d73;
            }

       /* Update your existing .btn-filter styles */
        .btn-filter {
            padding: 6px 12px;
            border: 1px solid #ccc;
            background-color: #e9ecef;
            cursor: pointer;
            font-size: 12px;
            margin-right: 4px;
            border-radius: 4px;
            transition: all 0.2s ease; /* Smooth color fading on hover */
        }

            .btn-filter:hover {
                background-color: #dde2e6;
            }

            .btn-filter.active {
                background-color: #19658A; /* Match the header color exactly */
                color: white;
                border-color: #19658A;
                box-shadow: inset 0 3px 5px rgba(0,0,0,0.12); /* Gives a "pressed in" look */
            }

        /* --- Grid Styles --- */
        /* Update your existing .Grid */
        .Grid {
            width: 100%;
            border-collapse: collapse;
            font-family: Arial, sans-serif;
            font-size: 12px; /* Bumped up from 11px for better data readability */
        }

            /* Add this to your existing .Grid th rules */
            .Grid th {
                background-color: #006699;
                color: white;
                padding: 8px; /* Slightly taller headers */
                border: 1px solid #333;
                text-align: center;
                /* NEW: Sticky Header Magic */
                position: sticky;
                top: 0;
                z-index: 10;
                box-shadow: 0 2px 2px rgba(0,0,0,0.1); /* Adds a nice shadow when scrolling */
            }

            .Grid td {
                padding: 5px;
                border: 1px solid #ccc;
                text-align: center;
                vertical-align: middle;
            }

        /* --- Text Icons for Buttons --- */
        .cmd-btn {
            text-decoration: none;
            font-size: 14px;
            margin: 0 4px;
            font-weight: bold;
            display: inline-block;
            width: 15px;
        }

        .cmd-up {
            color: #006699;
        }

        .cmd-del {
            color: red;
        }

        /* Update your existing .total-box */
        .total-box {
            margin-top: 20px;
            padding: 20px;
            background-color: #fcfcfc; /* Lighter, cleaner background */
            border: 1px solid #ddd;
            border-radius: 6px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.05); /* Soft depth */
            float: right; /* Aligns the box to the right side of the screen */
            width: 400px; /* Gives it a neat, compact receipt-like feel */
        }

        /* Update your existing .lbl-grand */
        .lbl-grand {
            font-size: 18px; /* Slightly larger */
            font-weight: bold;
            color: #28a745; /* Modern, standard success green */
        }

        /* 1. Force text color to black in the dropdown list */
        .select2-results__option {
            color: #333 !important;
            background-color: #fff !important;
        }

        /* 2. Highlight color on hover (matches your blue theme) */
        .select2-results__option--highlighted {
            background-color: #007bff !important; /* Blue background */
            color: #fff !important; /* White text on hover */
        }

        /* 3. Text color for the box that shows the selected item */
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            color: #333 !important;
            line-height: 28px; /* Vertical centering adjustment */
        }

        /* 4. Fix the search input box text color */
        .select2-search__field {
            color: #333 !important;
        }

        /* Clear the float so buttons below don't overlap */
        .clearfix::after {
            content: "";
            clear: both;
            display: table;
        }
    </style>

    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
        });

        function initSelect2() {
            var $ddl = $('#cmbClient');

            // Check if Select2 is already applied to prevent duplicates
            if ($ddl.hasClass("select2-hidden-accessible")) {
                $ddl.select2('destroy');
            }

            $ddl.select2({
                placeholder: "Select a Client",
                allowClear: true,
                width: '100%'
            });
        }

        // Run on initial load
        $(document).ready(function () {
            initSelect2();
        });

        // Run on UpdatePanel Postback (Partial Page Updates)
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            initSelect2();
        });

        // This handles both initial load and UpdatePanel partial postbacks
        function pageLoad() {
            initSelect2();
        }

        // --- STEP 2: SEARCH & FILTER LOGIC ---
        function FilterGrid() {
            var input = document.getElementById("txtSearchProduct");
            var filter = input.value.toUpperCase();
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            var tr = table.getElementsByTagName("tr");

            for (var i = 1; i < tr.length; i++) {
                // Column 2 is Product Name (0=Select, 1=HSN, 2=Name)
                var td = tr[i].getElementsByTagName("td")[3];
                if (td) {
                    var txtValue = td.textContent || td.innerText;
                    if (txtValue.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = "";
                    } else {
                        tr[i].style.display = "none";
                    }
                }
            }
        }

        function FilterSelection(mode) {
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            var tr = table.getElementsByTagName("tr");

            // Visual Active State for Buttons
            var btns = document.getElementsByClassName('btn-filter');
            for (var b = 0; b < btns.length; b++) { btns[b].classList.remove('active'); }
            event.target.classList.add('active');

            for (var i = 1; i < tr.length; i++) {
                var chk = tr[i].querySelector("input[type='checkbox']");
                if (chk) {
                    if (mode === 'ALL') {
                        tr[i].style.display = "";
                    } else if (mode === 'SEL') {
                        tr[i].style.display = chk.checked ? "" : "none";
                    } else if (mode === 'UNSEL') {
                        tr[i].style.display = !chk.checked ? "" : "none";
                    }
                }
            }
        }

        // --- STEP 3: INSTANT CALCULATION (DUAL DISCOUNT) ---
        // --- STEP 3: INSTANT CALCULATION (DUAL DISCOUNT & SAFE MATH) ---
        function CalculateRow(input, trigger) {
            var row = input.parentNode.parentNode;

            // 1. Safely grab all elements
            var txtQty = row.querySelector("input[id*='txtQty']");
            var txtRate = row.querySelector("input[id*='txtRate']");
            var txtDiscPer = row.querySelector("input[id*='txtDiscPer']");
            var txtDiscAmt = row.querySelector("input[id*='txtDiscAmt']");

            var lblGross = row.querySelector("span[id*='lblGross']");
            var lblTaxable = row.querySelector("span[id*='lblTaxable']");
            var lblGst = row.querySelector("span[id*='lblGstRate']");
            var lblTaxAmt = row.querySelector("span[id*='lblTaxAmt']");
            var lblNet = row.querySelector("span[id*='lblNet']");

            // SAFEGUARD: If the main elements are missing, stop to prevent crashes
            if (!txtQty || !txtRate || !lblGross) return;

            // 2. Parse values, prevent NaN, and prevent Negative numbers using Math.max()
            var qty = Math.max(0, parseFloat(txtQty.value) || 0);
            var rate = Math.max(0, parseFloat(txtRate.value) || 0);
            var gst = Math.max(0, parseFloat(lblGst ? lblGst.innerText : 0) || 0);

            // Gross
            var gross = qty * rate;
            lblGross.innerText = gross.toFixed(2);

            // 3. Discount Logic
            var discAmt = 0;
            var discPer = 0;

            if (trigger === 'AMT') {
                // User typed Amount
                discAmt = Math.max(0, parseFloat(txtDiscAmt.value) || 0);

                // SAFEGUARD: Do not allow discount to be higher than the gross amount
                if (discAmt > gross) discAmt = gross;

                if (gross > 0) discPer = (discAmt / gross) * 100;
                if (txtDiscPer) txtDiscPer.value = discPer.toFixed(2);
            }
            else {
                // User typed % (or triggered by Qty/Rate change)
                if (txtDiscPer) discPer = Math.max(0, parseFloat(txtDiscPer.value) || 0);

                // SAFEGUARD: Do not allow discount percentage to exceed 100%
                if (discPer > 100) discPer = 100;

                discAmt = (gross * discPer) / 100;
                if (txtDiscAmt) txtDiscAmt.value = discAmt.toFixed(2);
            }

            // Taxable
            var taxable = gross - discAmt;
            if (taxable < 0) taxable = 0; // Final safety net
            if (lblTaxable) lblTaxable.innerText = taxable.toFixed(2);

            // Tax Amount
            var taxVal = (taxable * gst) / 100;
            if (lblTaxAmt) lblTaxAmt.innerText = taxVal.toFixed(2);

            // Net Total
            var net = taxable + taxVal;
            if (lblNet) lblNet.innerText = net.toFixed(2);

            RecalculateFooter();
        }

        function RecalculateFooter() {
            var grid = document.getElementById("<%= gd_Cart.ClientID %>");
            var totalTax = 0;
            var totalGrand = 0;

            if (grid) {
                var rows = grid.getElementsByTagName("tr");
                for (var i = 1; i < rows.length; i++) {
                    var lTax = rows[i].querySelector("span[id*='lblTaxAmt']");
                    var lNet = rows[i].querySelector("span[id*='lblNet']");

                    // Safely add to totals
                    if (lTax) totalTax += parseFloat(lTax.innerText) || 0;
                    if (lNet) totalGrand += parseFloat(lNet.innerText) || 0;
                }
            }

            // Safely get footer inputs
            var inputFrt = document.getElementById("<%= txtFreight.ClientID %>");
            var inputOth = document.getElementById("<%= txtOtherCharge.ClientID %>");

            var frt = inputFrt ? Math.max(0, parseFloat(inputFrt.value) || 0) : 0;
            var oth = inputOth ? Math.max(0, parseFloat(inputOth.value) || 0) : 0;

            // Output to labels securely
            var outTax = document.getElementById("<%= lblFooterTax.ClientID %>");
            var outGrand = document.getElementById("<%= lblFooterGrand.ClientID %>");

            if (outTax) outTax.innerText = totalTax.toFixed(2);
            if (outGrand) outGrand.innerText = (totalGrand + frt + oth).toFixed(2);
        }

        // 1. Catch standard JavaScript runtime errors (e.g., calculation glitches)
        window.onerror = function (msg, url, lineNo, columnNo, error) {
            var softMsg = "A minor display glitch occurred. The page will still work, but some calculations might be delayed.";
            console.error("JS Error: " + msg + " at line " + lineNo);

            // Show a soft alert to the user
            alert(softMsg);

            // Return true to prevent the browser's default error handling
            return true;
        };

        // 2. Catch ASP.NET AJAX UpdatePanel Errors (e.g., server timeouts or C# crashes during partial postbacks)
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function (sender, args) {
            if (args.get_error() != undefined) {
                // Extract the error message for the console
                var errorMsg = args.get_error().message;

                // Suppress the default ugly ASP.NET popup
                args.set_errorHandled(true);

                // Show a friendly, soft alert
                alert("We had trouble communicating with the server. Please check your connection or try clicking again.");
                console.error("AJAX Error: ", errorMsg);
            } else {
                // If there's no error, make sure our dropdowns re-initialize properly
                initSelect2();
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div style="width: 98%; margin: auto; padding-top: 10px;">
        <div class="section-header">Create Manual Tax Invoice</div>

        <asp:Panel ID="PanelMsg" runat="server" Visible="false" Style="padding: 10px; margin-bottom: 10px; border: 1px solid #ccc; background-color: #fff;">
            <asp:Label ID="lblMsg" runat="server" Font-Bold="true"></asp:Label>
        </asp:Panel>

        <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">

            <asp:View ID="vSetup" runat="server">
                <table style="width: 100%; border-spacing: 8px;">
                    <tr>
                        <td width="15%"><strong>Client Name:</strong><span style="color: red">*</span></td>
                        <td width="35%">
                            <asp:DropDownList ID="cmbClient" runat="server" CssClass="form-control select-search"
                                AutoPostBack="true" OnSelectedIndexChanged="cmbClient_SelectedIndexChanged"
                                ClientIDMode="Static">
                            </asp:DropDownList>
                            <asp:Label ID="lblClientID" runat="server" Visible="false"></asp:Label>
                        </td>
                        <td width="15%"><strong>Invoice Date:</strong><span style="color: red">*</span></td>
                        <td width="35%">
                            <asp:TextBox ID="txtInvoiceDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td><strong>Tax Type:</strong><span style="color: red">*</span></td>
                        <td>
                            <asp:RadioButtonList ID="rbTaxType" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Value="1" Selected="True">Intra (CGST/SGST)</asp:ListItem>
                                <asp:ListItem Value="0">Inter (IGST)</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                        <td rowspan="3" valign="top"><strong>Addresses:</strong><span style="color: red">*</span></td>
                        <td rowspan="3">
                            <asp:ListBox ID="lstAddresses" runat="server" SelectionMode="Multiple" Height="80px" CssClass="form-control"></asp:ListBox>
                        </td>
                    </tr>
                    <tr>
                        <td><strong>PO No:</strong></td>
                        <td>
                            <asp:TextBox ID="txtPONo" runat="server" CssClass="form-control" placeholder="Optional PO Number"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td><strong>ERP Ref No:</strong></td>
                        <td>
                            <asp:TextBox ID="txtERPRef" runat="server" CssClass="form-control" placeholder="External Invoice Ref"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td colspan="4" align="center" style="padding-top: 15px;">
                            <asp:Button ID="btnNext" runat="server" Text="Next: Select Products >>" CssClass="btn-nav" OnClick="btnNext_Click" />
                        </td>
                    </tr>
                </table>
            </asp:View>

            <asp:View ID="vProducts" runat="server">
                <table style="width: 100%; margin-bottom: 5px;">
                    <tr>
                        <td width="15%"><strong>Category:</strong></td>
                        <td width="25%">
                            <asp:DropDownList ID="cmbCategory" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="cmbCategory_SelectedIndexChanged"></asp:DropDownList>
                        </td>
                        <td width="35%" align="center">
                            <button type="button" class="btn-filter active" onclick="FilterSelection('ALL')">View All</button>
                            <button type="button" class="btn-filter" onclick="FilterSelection('SEL')">Selected</button>
                            <button type="button" class="btn-filter" onclick="FilterSelection('UNSEL')">Un-Selected</button>
                        </td>
                        <td width="25%">
                            <input type="text" id="txtSearchProduct" onkeyup="FilterGrid()" placeholder="Search Product..." class="form-control" />
                        </td>
                    </tr>
                </table>

                <div style="height: 380px; overflow-y: auto; border: 1px solid #ccc;">
                    <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" DataKeyNames="ProductID">
                        <Columns>
                            <asp:TemplateField HeaderText="Select" ItemStyle-Width="40px">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ProductID" HeaderText="ID" Visible="true" />
                            <asp:BoundField DataField="Product_code" HeaderText="HSN Code" ItemStyle-Width="90px" />
                            <asp:BoundField DataField="ProductName" HeaderText="Product Name" />
                            <asp:BoundField DataField="Brand" HeaderText="Spec" ItemStyle-Width="100px" />
                            <asp:BoundField DataField="Unit" HeaderText="Unit" ItemStyle-Width="50px" />
                            <asp:BoundField DataField="Quantity" HeaderText="Stock" ItemStyle-Width="70px" ItemStyle-ForeColor="Green" ItemStyle-Font-Bold="true" />
                            <asp:TemplateField HeaderText="Rate">
                                <ItemTemplate>
                                    <asp:Label ID="lblBaseRate" runat="server" Text='<%# Bind("Sail_Rate") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="GST %">
                                <ItemTemplate>
                                    <asp:Label ID="lblGstRate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div style="padding: 10px;">No Products Found.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <div style="text-align: center; margin-top: 15px;">
                    <asp:Button ID="btnBackSetup" runat="server" Text="<< Back" CssClass="btn-nav" OnClick="btnBackSetup_Click" BackColor="#666" />
                    &nbsp;&nbsp;
                    <asp:Button ID="btnAddToCart" runat="server" Text="Add Selected & Review >>" CssClass="btn-nav" OnClick="btnAddToCart_Click" Width="200px" />
                </div>
            </asp:View>

            <asp:View ID="vReview" runat="server">
                <div style="margin-bottom: 10px; color: #555; font-size: 12px;">
                    Client: <strong>
                        <asp:Label ID="lblClientDisplay" runat="server"></asp:Label></strong> | 
                    Tax: <strong>
                        <asp:Label ID="lblTaxModeDisplay" runat="server"></asp:Label></strong>
                </div>

                <asp:GridView ID="gd_Cart" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" OnRowCommand="gd_Cart_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Act" ItemStyle-Width="70px">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-up" ToolTip="Move Up">▲</asp:LinkButton>
                                <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-up" ToolTip="Move Down">▼</asp:LinkButton>
                                <asp:LinkButton ID="btnDel" runat="server" CommandName="Remove" CommandArgument='<%# Container.DataItemIndex %>' CssClass="cmd-btn cmd-del" ToolTip="Remove" OnClientClick="return confirm('Remove row?');">✖</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ProductID" HeaderText="ID" Visible="true" />
                        <asp:BoundField DataField="ProductName" HeaderText="Product" ReadOnly="true" />
                        <asp:BoundField DataField="Product_code" HeaderText="HSN" ReadOnly="true" ItemStyle-Width="60px" />

                        <asp:TemplateField HeaderText="Qty" ItemStyle-Width="60px">
                            <ItemTemplate>
                                <asp:TextBox ID="txtQty" runat="server" Text='<%# Bind("IQuantity") %>' CssClass="form-control" Style="text-align: center;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
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

                        <asp:TemplateField Visible="false">
                            <ItemTemplate>
                                <asp:Label ID="lblPID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                <asp:Label ID="lblBrand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div style="padding: 15px; color: red; font-weight: bold;">Cart is Empty. Please go back and add products.</div>
                    </EmptyDataTemplate>
                </asp:GridView>

                <div class="total-box">
                    <table width="100%">
                        <tr>
                            <td width="70%" align="right">Freight Charges:</td>
                            <td width="15%" align="right">
                                <asp:TextBox ID="txtFreight" runat="server" Text="0" CssClass="form-control" Width="80px" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
                            </td>
                            <td width="15%"></td>
                        </tr>
                        <tr>
                            <td align="right">Other Charges:</td>
                            <td align="right">
                                <asp:TextBox ID="txtOtherCharge" runat="server" Text="0" CssClass="form-control" Width="80px" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td colspan="3">
                                <hr />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Total Tax:</td>
                            <td align="right">
                                <asp:Label ID="lblFooterTax" runat="server" Text="0.00" Font-Bold="true"></asp:Label>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td align="right" style="font-size: 14px;"><strong>Grand Total:</strong></td>
                            <td align="right">
                                <asp:Label ID="lblFooterGrand" runat="server" Text="0.00" CssClass="lbl-grand"></asp:Label>
                            </td>
                            <td></td>
                        </tr>
                    </table>
                </div>

                <div style="text-align: center; margin-top: 20px;">
                    <asp:Button ID="btnBackProd" runat="server" Text="<< Add More Products" CssClass="btn-nav" OnClick="btnBackProd_Click" BackColor="#777" />
                    &nbsp;&nbsp;
                    <asp:Button ID="btnSave" runat="server" Text="Generate Tax Invoice" CssClass="btn-nav" Width="200px" OnClick="btnSave_Click" OnClientClick="return confirm('Confirm Invoice Generation? This will deduct stock.');" />
                </div>
            </asp:View>

        </asp:MultiView>
    </div>
</asp:Content>
