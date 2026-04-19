<%@ Page Title="Create Tax Invoice (From Source)" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Add_invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm26" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
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

        .form-grid-3 {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
            margin-bottom: 15px;
            align-items: end;
        }

        .form-grid-3 { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 15px; align-items: end; }
        .form-grid-2 { display: grid; grid-template-columns: repeat(2, 1fr); gap: 20px; margin-bottom: 15px; }
        
        /* NEW: Forces 5 items into a single row, perfectly proportioned */
        .form-grid-5 { display: grid; grid-template-columns: 2fr 2.5fr 1.5fr 1.5fr 1fr; gap: 15px; margin-bottom: 15px; align-items: end; }

        .form-grid-2 {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 15px;
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

        .total-box {
            margin-top: 20px;
            padding: 20px;
            background-color: #f9fbfd;
            border: 1px solid #d1d9e0;
            border-radius: 6px;
            float: right;
            width: 400px;
        }

        .lbl-grand {
            font-size: 20px;
            font-weight: bold;
            color: #28a745;
        }

        .clearfix::after {
            content: "";
            clear: both;
            display: table;
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

        /* Fix Invisible Select2 Dropdown Text */
        .select2-results__option {
            color: #333 !important;
            background-color: #fff !important;
        }

        .select2-results__option--highlighted {
            background-color: #006699 !important;
            color: #fff !important;
        }
    </style>

    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />

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
        }

        $(document).ready(function () { initScripts(); });
        function pageLoad() { initScripts(); }

        // --- INSTANT CLIENT-SIDE MATH ---
        // --- INSTANT CLIENT-SIDE MATH & VALIDATION ---
        function CalculateRow(input, trigger) {
            var row = input.parentNode.parentNode;
            var txtQty = row.querySelector("input[id*='txtqnty']");
            var txtRate = row.querySelector("input[id*='txtsailrate']");
            var txtDiscPer = row.querySelector("input[id*='txtDiscPer']");
            var txtDiscAmt = row.querySelector("input[id*='txtDiscAmt']");
            var lblGross = row.querySelector("span[id*='lblGross']");
            var lblTaxable = row.querySelector("span[id*='lblTaxable']");
            var lblGst = row.querySelector("span[id*='lblGstRate']");
            var lblTaxAmt = row.querySelector("span[id*='lblTaxAmt']");
            var lblNet = row.querySelector("span[id*='lblNet']");

            if (!txtQty || !txtRate || !lblGross) return;

            // 1. OVER-INVOICING VALIDATION
            var maxQty = parseFloat(txtQty.getAttribute('data-max')) || 0;
            var qty = parseFloat(txtQty.value);

            if (isNaN(qty)) qty = 0;

            if (qty > maxQty) {
                alert("Action Restricted: Bill Qty cannot exceed the Pending Qty of " + maxQty + ".");
                qty = maxQty; // Snap back to max allowed
                txtQty.value = maxQty;
            } else if (qty < 0) {
                qty = 0;
                txtQty.value = 0;
            }

            // 2. CALCULATE MATH
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

            // --- NEW: ZERO TOTAL VALIDATION ---
            var btnSubmit = document.getElementById("<%= Button1.ClientID %>");
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
        <div class="section-header">Generate Tax Invoice (From Quotation/ Client PO/ Challan/ PI)</div>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>

                <asp:Panel ID="PanelMsg" runat="server" Visible="false" Style="padding: 12px; margin-bottom: 15px; border-left: 5px solid #19658A; background-color: #f8f9fa;">
                    <asp:Label ID="lblMsg" runat="server" Font-Bold="true"></asp:Label>
                </asp:Panel>

                <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">

                    <asp:View ID="vSetup" runat="server">
                        <div class="box-panel">
                            <div class="box-title">1. Find Source Document</div>

                            <div class="form-grid-5">
                                <div>
                                    <label class="form-label">Filter by Client</label>
                                    <asp:DropDownList ID="cmbvendor" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged" ClientIDMode="Static"></asp:DropDownList>
                                    <asp:Label ID="lblclientId" runat="server" Visible="false"></asp:Label>
                                </div>
                                <div>
                                    <label class="form-label">Source Type <span style="color:red">*</span></label>
                                    <asp:DropDownList ID="ddlDocType" runat="server" CssClass="form-control">
                                        <asp:ListItem Value="Quotation">Quotation</asp:ListItem>
                                        <asp:ListItem Value="Purchase Order">Purchase Order (Customer PO)</asp:ListItem>
                                        <asp:ListItem Value="Delivery Challan">Delivery Challan</asp:ListItem>
                                        <asp:ListItem Value="Proforma">Pro-Forma Invoice</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label class="form-label">Date From</label>
                                    <asp:TextBox ID="txtfromDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">Date To</label>
                                    <asp:TextBox ID="txttodate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div>
                                    <asp:Button ID="btnSertch" runat="server" Text="Search" CssClass="btn-nav btn-secondary" OnClick="btnSertch_Click" Style="width:100%; height: 35px;" />
                                </div>
                            </div>

                            <asp:Panel ID="pnlAddress" runat="server" Visible="false" Style="margin-bottom: 15px;">
                                <label class="form-label">Select Client Address <span style="color:red">*</span></label>
                                <asp:ListBox ID="List_SiteAddress" runat="server" CssClass="form-control" Height="70px"></asp:ListBox>
                            </asp:Panel>

                            <div style="max-height: 250px; overflow-y: auto; border: 1px solid #e2e8f0;">
                                <asp:GridView ID="gvSearchDocs" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" OnRowCommand="gvSearchDocs_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="DocNo" HeaderText="Document No" ItemStyle-Font-Bold="true" />
                                        <asp:BoundField DataField="DocDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                        <asp:BoundField DataField="Client_Name" HeaderText="Client Name" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="Net_amount" HeaderText="Net Amount" DataFormatString="{0:N2}" ItemStyle-HorizontalAlign="Right" />
                                        <asp:TemplateField HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:Button ID="btnSelect" runat="server" CommandName="SelectDoc" CommandArgument='<%# Eval("DocNo") %>' Text="Select & Proceed &rarr;" CssClass="btn-nav" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div style="padding: 15px; text-align: center; color: #777;"><i>No records found. Select a document type and click Search.</i></div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                            </div>
                        </div>
                    </asp:View>

                    <asp:View ID="vProducts" runat="server">

                        <div class="box-panel">
                            <div class="box-title">2. Invoice Master Details</div>
                            <div class="form-grid-3">
                                <div>
                                    <label class="form-label">Invoice Date <span style="color: red">*</span></label>
                                    <asp:TextBox ID="txtinvoiceDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                                </div>
                                <div>
                                    <label class="form-label">Tax Type <span style="color: red">*</span></label>
                                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal" CellPadding="5">
                                        <asp:ListItem Selected="True" Value="1">Intra (CGST/SGST)</asp:ListItem>
                                        <asp:ListItem Value="0">Inter (IGST)</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                                <div>
                                    <label class="form-label">Sales Person <span style="color: red">*</span></label>
                                    <asp:DropDownList ID="cmbSalesPerson" runat="server" CssClass="form-control select-search" ClientIDMode="Static"></asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <div class="box-panel">
                            <div class="box-title">3. Review Items & Finalize Totals</div>
                            <div style="margin-bottom: 10px; color: #555; font-size: 13px;">
                                Source Document: <strong style="color: #006699;">
                                    <asp:Label ID="lblRefDoc" runat="server"></asp:Label></strong>
                            </div>

                            <div style="max-height: 400px; overflow-y: auto; overflow-x: auto; border: 1px solid #e2e8f0; margin-bottom: 20px; width: 100%;">
                                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CssClass="Grid" DataKeyNames="TrueID" OnRowCommand="GridView1_RowCommand" style="min-width: 1500px; white-space: nowrap;">
                                    <Columns>
                                        <asp:BoundField DataField="TrueID" HeaderText="ID" ReadOnly="true" />
                                        <asp:BoundField DataField="Product_name" HeaderText="Product Name" ReadOnly="true" />
                                        <asp:BoundField DataField="TrueHSN" HeaderText="HSN" ReadOnly="true" ItemStyle-Width="60px" />
                                        
                                        <asp:TemplateField HeaderText="Specification" ItemStyle-Width="130px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtdes" runat="server" Text='<%# Bind("specification") %>' CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="In Stock" ItemStyle-Width="60px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblStock" runat="server" Text='<%# Bind("AvailableStock") %>' Font-Bold="true" ForeColor="#19658A"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:BoundField DataField="QuotedQty" HeaderText="Quote Qty" ReadOnly="true" ItemStyle-Width="50px" ItemStyle-Font-Bold="true" />
                                        <asp:BoundField DataField="InvoicedQty" HeaderText="Inv. Qty" ReadOnly="true" ItemStyle-Width="50px" ItemStyle-ForeColor="#dc3545" ItemStyle-Font-Bold="true" />

                                        <asp:TemplateField HeaderText="Bill Qty" ItemStyle-Width="80px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtqnty" runat="server" Text='<%# Bind("PendingQty") %>' data-max='<%# Eval("PendingQty") %>' CssClass="form-control" Style="text-align: center; font-weight: bold; color: #006699;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Rate" ItemStyle-Width="70px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtsailrate" runat="server" Text='<%# Bind("sail_rate") %>' CssClass="form-control" Style="text-align: right;" onkeyup="CalculateRow(this, 'MAIN')"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Gross" ItemStyle-Width="70px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblGross" runat="server" Text="0.00"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Disc %" ItemStyle-Width="50px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtDiscPer" runat="server" Text='<%# Bind("discountRate") %>' CssClass="form-control" Style="text-align: center;" onkeyup="CalculateRow(this, 'PER')"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Disc Amt" ItemStyle-Width="60px">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtDiscAmt" runat="server" Text="0.00" CssClass="form-control" Style="text-align: right;" onkeyup="CalculateRow(this, 'AMT')"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Taxable" ItemStyle-Width="70px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblTaxable" runat="server" Text="0.00" Font-Bold="true"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="GST%" ItemStyle-Width="40px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblGstRate" runat="server" Text='<%# Bind("Service_tax_rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Tax Amt" ItemStyle-Width="60px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblTaxAmt" runat="server" Text="0.00"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Net" ItemStyle-Width="70px">
                                            <ItemTemplate>
                                                <asp:Label ID="lblNet" runat="server" Text="0.00" Font-Bold="true" ForeColor="Green"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:Button ID="btnRemove" runat="server" CommandName="RemoveItem" CommandArgument='<%# Container.DataItemIndex %>' Text="X" CssClass="btn-del" OnClientClick="return confirm('Remove this item?');" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <div class="total-box">
                                <table width="100%" cellpadding="6" cellspacing="0">
                                    <tr>
                                        <td width="50%" align="right" style="color: #555; font-weight: bold;">Freight Charges (+)</td>
                                        <td width="50%" align="right">
                                            <asp:TextBox ID="txt_delivery_amnt" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" placeholder="Other Charge Name"></asp:TextBox>
                                        </td>
                                        <td align="right">
                                            <asp:TextBox ID="txt_othr_amnt" runat="server" Text="0" CssClass="form-control" Style="text-align: right;" onkeyup="RecalculateFooter()"></asp:TextBox>
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

                            <div style="text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e2e8f0;">

                                <div id="zeroTotalWarning" style="display: none; color: #dc3545; font-weight: bold; font-size: 15px; margin-bottom: 15px; background: #fff3cd; padding: 10px; border: 1px solid #ffeeba; border-radius: 4px;">
                                    Cannot generate an invoice with a Grand Total of ₹0.00. Please allocate quantities or charges to proceed.
                                </div>

                                <asp:Button ID="btnBackSetup" runat="server" Text="&larr; Back to Search" CssClass="btn-nav btn-secondary" OnClick="btnBackSetup_Click" />
                                &nbsp;&nbsp;
                                <asp:Button ID="Button1" runat="server" Text="Generate Tax Invoice" CssClass="btn-nav" Style="background-color: #28a745;" OnClick="Button1_Click" OnClientClick="return confirm('Confirm generation? Physical stock will be deducted.');" />
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
                                                            <%# Eval("Validity_StartDate") %>
                                                            <br />
                                                            to
                                                            <br />
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

                        </div>
                    </asp:View>

                </asp:MultiView>
            </ContentTemplate>
            <Triggers>
                <asp:PostBackTrigger ControlID="Button1" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
