<%@ Page Title="Edit Purchase Order" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="edit_purchaseorder.aspx.cs" Inherits="Bill_Software.corporate.business.app.edit_purchaseorder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
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

        .center {
            text-align: center;
        }

        /* Wizard Header */
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

        /* --- Select2 Custom Styling to Match FLMX Theme --- */
        .select2-container .select2-selection--single {
            height: 30px !important;
            border: 1px solid #ccc !important;
            border-radius: 4px !important;
        }

        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 28px !important;
            font-size: 12px;
            color: #333 !important;
        }

        .select2-container--default .select2-selection--single .select2-selection__arrow {
            height: 28px !important;
        }

        /* --- Premium Cart Grid Styling --- */
        .cart-grid-wrapper {
            width: 100%;
            overflow-x: auto;
            border: 1px solid #ccc;
            margin-top: 15px;
        }

            .cart-grid-wrapper table {
                min-width: 1500px; /* Forces horizontal scrolling */
                border-collapse: collapse;
            }

            .cart-grid-wrapper th, .cart-grid-wrapper td {
                padding: 8px 5px;
                border: 1px solid #ddd;
                vertical-align: middle;
                font-size: 11px;
                white-space: nowrap; /* Prevents text from wrapping */
            }

        .grid-footer {
            background-color: #f1f1f1;
            font-weight: bold;
        }

        /* Sticky Columns */
        .col-frozen-action {
            position: sticky;
            left: 0;
            background-color: #f9f9f9;
            z-index: 2;
            border-right: 2px solid #aaa !important;
        }

        .col-frozen-sl {
            position: sticky;
            left: 60px;
            background-color: #f9f9f9;
            z-index: 2;
        }

        .col-frozen-name {
            position: sticky;
            left: 100px;
            background-color: #f9f9f9;
            z-index: 2;
            border-right: 2px solid #aaa !important;
        }

        /* Action Buttons */
        .action-btn {
            text-decoration: none;
            font-weight: bold;
            padding: 2px 6px;
            border: 1px solid #ccc;
            border-radius: 3px;
            background: white;
            color: #333;
        }

            .action-btn:hover {
                background: #e0e0e0;
            }

        .action-del {
            color: red;
            border-color: red;
        }

            .action-del:hover {
                background: #ffe6e6;
            }
    </style>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/css/select2.min.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>

    <script type="text/javascript">
        function InitializeSelect2() {
            $('.select2-search').select2({
                placeholder: "-- Select --",
                allowClear: true,
                width: '100%'
            });
        }

        // Make the function globally available and safe
        window.checkAllOnLoad = function () {
            var GridView = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!GridView) return; // If we are not on Step 3, do nothing!

            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                if (inputList[i].type == "checkbox" && inputList[i].checked) {
                    var row = inputList[i].parentNode.parentNode;
                    row.style.backgroundColor = "#84e26e"; // Paint checked rows green
                }
            }
        };

            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(function () {
                $(".datepicker").datepicker({
                    dateFormat: 'dd-M-yy',
                    changeMonth: true,
                    changeYear: true
                });
                InitializeSelect2();

                // Automatically run the grid painter whenever the page updates
                if (typeof window.checkAllOnLoad === "function") {
                    window.checkAllOnLoad();
                }
            });

            prm.add_endRequest(function (sender, e) {
                InitializeSelect2();
            });

            function ValidateDelete1() {
                var answer = confirm("Want to Delete this Quotation?");
                if (!answer) {
                    return false;
                }
            }

            function Check_Click(objRef) {
                var row = objRef.parentNode.parentNode;
                if (objRef.checked) { row.style.backgroundColor = "#84e26e"; }
                else {
                    if (row.rowIndex % 2 == 0) { row.style.backgroundColor = "#C2D69B"; }
                    else { row.style.backgroundColor = "white"; }
                }
                var GridView = row.parentNode;
                var inputList = GridView.getElementsByTagName("input");
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
                var inputList = GridView.getElementsByTagName("input");
                for (var i = 0; i < inputList.length; i++) {
                    var row = inputList[i].parentNode.parentNode;
                    if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                        if (objRef.checked) {
                            row.style.backgroundColor = "#84e26e";
                            inputList[i].checked = true;
                        } else {
                            if (row.rowIndex % 2 == 0) { row.style.backgroundColor = "#C2D69B"; }
                            else { row.style.backgroundColor = "white"; }
                            inputList[i].checked = false;
                        }
                    }
                }
            }

            function toggleReferenceFields(value) {
                document.getElementById('<%= hdnRefOption.ClientID %>').value = value;
                var nameField = document.getElementById('<%= txt_clientrefname.ClientID %>');
                var idField = document.getElementById('<%= txt_clientrefid.ClientID %>');
                var dateField = document.getElementById('<%= txt_clientrefdate.ClientID %>');

                if (value === 'Yes') {
                    nameField.readOnly = false; idField.readOnly = false; dateField.readOnly = false;
                    nameField.value = ""; idField.value = ""; dateField.value = "";
                } else {
                    nameField.value = "N/A"; idField.value = "N/A"; dateField.value = "01-Jan-2000";
                    nameField.readOnly = true; idField.readOnly = true; dateField.readOnly = true;
                }
            }

            function togglePanel() {
                var rbQt = document.getElementById('<%= rbQt.ClientID %>');
                var panel = document.getElementById('<%= PO_DataInputs.ClientID %>');
                var poFields = document.querySelectorAll('.po-mandatory');

                if (rbQt.checked) {
                    panel.style.display = 'none';
                    poFields.forEach(function (field) { field.removeAttribute('required'); });
                } else {
                    panel.style.display = 'block';
                    poFields.forEach(function (field) { field.setAttribute('required', 'required'); });
                }
            }

            function handlePackageForwardingChange(dropdown) {
                var selectedValue = dropdown.value;
                var manualInputPkgRow = document.getElementById("ContentPlaceHolder1_manualInputPkgRow");
                if (manualInputPkgRow) {
                    manualInputPkgRow.style.display = (selectedValue == "3") ? "table-row" : "none";
                }
            }

            function handleDeliveryTermChange(dropdown) {
                var selectedValue = dropdown.value;
                var manualInputRow = document.getElementById("ContentPlaceHolder1_manualInputRow");
                if (manualInputRow) {
                    manualInputRow.style.display = (selectedValue == "4") ? "table-row" : "none";
                }
            }

            // --- UPGRADED: Client-Side Validation ---
            function ValidateDocument() {
                var client = document.getElementById('<%= cmbClient.ClientID %>').value;
                var salesPerson = document.getElementById('<%= cmbSalesPerson.ClientID %>').value;

                if (client === "0" || client === "") {
                    alert("⚠️ Please select a Client.");
                    return false;
                }
                if (salesPerson === "0" || salesPerson === "") {
                    alert("⚠️ Please assign a Sales Person.");
                    return false;
                }

                // Validate PO fields manually if PO is checked
                var rbPo = document.getElementById('<%= rbPo.ClientID %>');
                if (rbPo && rbPo.checked) {
                    var doNum = document.getElementById('<%= txb_donumber.ClientID %>').value;
                    var poNum = document.getElementById('<%= txb_ponumber.ClientID %>').value;
                    var poDate = document.getElementById('<%= txb_podate.ClientID %>').value;

                    if (doNum.trim() === "" || poNum.trim() === "" || poDate.trim() === "") {
                        alert("⚠️ Please fill in all mandatory Purchase Order details (DO No, PO No, PO Date).");
                        return false;
                    }
                }
                return true;
            }

            function calculateCart() {
                var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
                if (!grid) return;
                var sumTaxable = 0, sumTaxAmt = 0, sumNet = 0, sumQty = 0;

                for (var i = 1; i < grid.rows.length - 1; i++) {
                    var row = grid.rows[i];
                    var qtyBox = row.querySelector('.qty-input');
                    var rateBox = row.querySelector('.rate-input');
                    var discBox = row.querySelector('.disc-input');
                    var taxLbl = row.querySelector('.tax-lbl');

                    if (qtyBox && rateBox) {
                        var qty = parseFloat(qtyBox.value) || 0;
                        var rate = parseFloat(rateBox.value) || 0;
                        var discPct = parseFloat(discBox ? discBox.value : 0) || 0;
                        var taxPct = parseFloat(taxLbl ? taxLbl.innerText : 0) || 0;

                        var taxable = qty * (rate - (rate * discPct / 100));
                        var taxAmt = (taxable * taxPct) / 100;
                        var net = taxable + taxAmt;

                        if (row.querySelector('.lbl-taxable')) row.querySelector('.lbl-taxable').innerText = taxable.toFixed(2);
                        if (row.querySelector('.lbl-taxamt')) row.querySelector('.lbl-taxamt').innerText = taxAmt.toFixed(2);
                        if (row.querySelector('.lbl-net')) row.querySelector('.lbl-net').innerText = net.toFixed(2);

                        sumQty += qty; sumTaxable += taxable; sumTaxAmt += taxAmt; sumNet += net;
                    }
                }
                // Update Footer
                if (document.getElementById('ftr-qty')) document.getElementById('ftr-qty').innerText = sumQty.toFixed(2);
                if (document.getElementById('ftr-taxable')) document.getElementById('ftr-taxable').innerText = sumTaxable.toFixed(2);
                if (document.getElementById('ftr-tax')) document.getElementById('ftr-tax').innerText = sumTaxAmt.toFixed(2);
                if (document.getElementById('ftr-net')) document.getElementById('ftr-net').innerText = sumNet.toFixed(2);
            }

            // We run this when the page loads so the totals are correct immediately
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(function () {
                calculateCart();
            });

            function validateCart() {
                var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
                // A grid with data has Header (1) + Data (N) + Footer (1). So > 2 means it has items.
                if (!grid || grid.rows.length <= 2) {
                    alert("⚠️ Your cart is empty. Please add products from the catalog first.");
                    return false;
                }
                return true;
            }
    </script>

    <asp:HiddenField ID="hdnRefOption" runat="server" Value="No" />
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Edit Purchase Order (PO) Wizard</span></td>
                </tr>
                <tr>
                    <td colspan="4">
                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
                        <asp:Label ID="lblqno" runat="server" Visible="False"></asp:Label>
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; margin: 10px 0;">
                            <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            <asp:Label ID="lblOk" runat="server"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; margin: 10px 0; background: #ffeeee;">
                            <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                </tr>
            </table>

            <asp:MultiView ID="WizardMultiView" runat="server" ActiveViewIndex="0">

                <asp:View ID="View0_Search" runat="server">
                    <div class="wizard-steps">Step 0: Search & Select Record to Edit</div>
                    <table cellpadding="5" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="20%" align="right">Client Name :</td>
                            <td width="30%">
                                <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style select2-search" Width="100%"></asp:DropDownList></td>
                            <td width="20%" align="right">Search Type :</td>
                            <td width="30%">
                                <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem>Only Client</asp:ListItem>
                                    <asp:ListItem Selected="True">Only Date</asp:ListItem>
                                    <asp:ListItem>Client & Date</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">From Date :</td>
                            <td>
                                <asp:TextBox ID="txtfromDate" runat="server" CssClass="textbox_style datepicker" Width="120px"></asp:TextBox></td>
                            <td align="right">To Date :</td>
                            <td>
                                <asp:TextBox ID="txttodate" runat="server" CssClass="textbox_style datepicker" Width="120px"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td colspan="4" align="center" style="padding-top: 15px; padding-bottom: 15px;">
                                <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="🔍 Search" Width="150px" />
                                &nbsp;&nbsp;
                                <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" CausesValidation="false" Text="Reset" Width="150px" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Width="100%" OnItemCommand="DataList1_ItemCommand">
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingItemStyle BackColor="#94B8FF" />
                                    <HeaderTemplate>
                                        <table class="table1" width="100%">
                                            <tr>
                                                <td align="center" width="25%">Client Name</td>
                                                <td align="center" width="15%">Date</td>
                                                <td align="center" width="15%">Record No</td>
                                                <td align="center" width="15%">Net Amount</td>
                                                <td align="center" width="10%">Type</td>
                                                <td align="center" width="10%">View</td>
                                                <td align="center" width="10%">Edit</td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <table class="table2" width="100%">
                                            <tr>
                                                <td align="center" width="25%">
                                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label></td>
                                                <td align="center" width="15%">
                                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label></td>
                                                <td align="center" width="15%">
                                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>' Font-Bold="true"></asp:Label></td>
                                                <td align="center" width="15%">Rs.
                                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>/-</td>
                                                <td align="center" width="10%">
                                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("RecordType") %>'></asp:Label></td>
                                                <td align="center" width="10%">
                                                    <a href="#" onclick="window.open('/corporate/business/print/NewPurchaseOrder.aspx?ID=<%# Eval("ID")%>', 'popup','width=900,height=800,scrollbars=yes');return false;">
                                                        <img src="../WebImages/viewicon.png" height="20px" /></a>
                                                </td>
                                                <td align="center" width="10%">
                                                    <asp:Button ID="btnLoad" runat="server" CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select" Text="Load Data" CssClass="btn_style" Style="padding: 2px 5px;" />
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

                    <table cellpadding="5" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="20%" align="right" valign="top"><span style="color: red">*</span> Select Client:</td>
                            <td width="30%" valign="top">
                                <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style select2-search" Width="100%" AutoPostBack="True" OnSelectedIndexChanged="cmbClient_SelectedIndexChanged"></asp:DropDownList>
                                <asp:Panel ID="pnlClientPreview" runat="server" Visible="false" Style="margin-top: 10px; padding: 10px; background: #eef7f9; border: 1px solid #19658A; border-radius: 4px; font-size: 11px; text-align: left; line-height: 1.6;">
                                    <div style="margin-bottom: 5px;">
                                        <span style="font-size: 13px; color: #19658A;"><strong>
                                            <asp:Label ID="lblPreviewName" runat="server"></asp:Label></strong></span>
                                        <span style="color: #666;">[ERP Code: <strong>
                                            <asp:Label ID="lblPreviewERPCode" runat="server"></asp:Label></strong>]</span>
                                    </div>

                                    <asp:Label ID="lblPreviewAddress" runat="server"></asp:Label><br />

                                    <div style="margin-top: 5px; padding-top: 5px; border-top: 1px dashed #b5c7d3;">
                                        <strong>State:</strong>
                                        <asp:Label ID="lblPreviewState" runat="server"></asp:Label>
                                        &nbsp;|&nbsp; 
        <strong>POS:</strong>
                                        <asp:Label ID="lblPreviewPOS" runat="server"></asp:Label><br />
                                        <strong>GSTIN:</strong>
                                        <asp:Label ID="lblPreviewGST" runat="server"></asp:Label>
                                        &nbsp;|&nbsp; 
        <strong>PAN:</strong>
                                        <asp:Label ID="lblPreviewPAN" runat="server"></asp:Label>
                                    </div>

                                    <div style="margin-top: 8px; text-align: right;">
                                        <a href="Add_client.aspx" target="_blank" style="color: #d9534f; font-weight: bold; text-decoration: underline;">✎ Modify Client Details</a>
                                    </div>
                                </asp:Panel>
                            </td>
                            <td width="20%" align="right" valign="top"><span style="color: red">*</span> Assigned Sales Person:</td>
                            <td width="30%" valign="top">
                                <asp:DropDownList ID="cmbSalesPerson" runat="server" CssClass="dropdown_style select2-search" Width="100%"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Enable Reference Details:</td>
                            <td>
                                <asp:RadioButton ID="rbYes" runat="server" GroupName="referenceOption" Text="Yes" onclick="toggleReferenceFields('Yes')" />
                                <asp:RadioButton ID="rbNo" runat="server" GroupName="referenceOption" Text="No" onclick="toggleReferenceFields('No')" />
                            </td>
                            <td align="right">Reference Person Name:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefname" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">Reference ID:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefid" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                            <td align="right">Reference Date:</td>
                            <td>
                                <asp:TextBox ID="txt_clientrefdate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right"><span style="color: red">*</span> Document Date:</td>
                            <td>
                                <asp:TextBox ID="txtquotationDate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox></td>
                            <td align="right"><span style="color: red">*</span> Place Of Supply:</td>
                            <td>
                                <asp:DropDownList ID="ddlPlaceOfSupply" runat="server" CssClass="dropdown_style select2-search" Width="100%"></asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td align="right">GST Type:</td>
                            <td>
                                <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Value="1"> CGST/SGST </asp:ListItem>
                                    <asp:ListItem Value="0"> IGST </asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                            <td align="right"><span style="color: red">*</span> Record Type:</td>
                            <td>
                                <asp:RadioButton ID="rbQt" runat="server" GroupName="recordOption" Text="Quotation" onclick="togglePanel()" />
                                <asp:RadioButton ID="rbPo" runat="server" GroupName="recordOption" Text="Purchase Order" onclick="togglePanel()" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <asp:Panel ID="PO_DataInputs" runat="server" Style="background: #f9f9f9; padding: 10px; border: 1px solid #ddd; margin-top: 10px;">
                                    <table width="100%" cellpadding="3">
                                        <tr>
                                            <td width="25%" align="right">Delivery Order No:</td>
                                            <td width="25%">
                                                <asp:TextBox ID="txb_donumber" runat="server" CssClass="textbox_style po-mandatory"></asp:TextBox></td>
                                            <td width="25%" align="right">Ref. Contract No:</td>
                                            <td width="25%">
                                                <asp:TextBox ID="txb_ponumber" runat="server" CssClass="textbox_style po-mandatory"></asp:TextBox></td>
                                        </tr>
                                        <tr>
                                            <td align="right">Purchase Order Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_podate" runat="server" CssClass="textbox_style datepicker po-mandatory"></asp:TextBox></td>
                                            <td align="right">Validity Start Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_strtdt" runat="server" CssClass="textbox_style datepicker po-mandatory"></asp:TextBox></td>
                                        </tr>
                                        <tr>
                                            <td align="right">Validity End Date:</td>
                                            <td>
                                                <asp:TextBox ID="txb_enddt" runat="server" CssClass="textbox_style datepicker po-mandatory"></asp:TextBox></td>
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
                        <asp:Button ID="btnNext1" runat="server" Text="Next: Review Catalog/Cart ➔" CssClass="btn_style" Width="250px" OnClientClick="if (!ValidateDocument()) return false;" OnClick="btnNext1_Click" CausesValidation="false" UseSubmitBehavior="false" />
                    </div>
                </asp:View>

                <asp:View ID="View2_Catalog" runat="server">
                    <div class="wizard-steps">Step 2 of 4: Browse & Add More Products</div>
                    <table width="100%" cellpadding="5">
                        <tr>
                            <td width="20%" align="right">Select Category:</td>
                            <td width="40%">
                                <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style select2-search" Width="100%"></asp:DropDownList></td>
                            <td width="40%">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" OnClick="Button2_Click" Text="Load Products" /></td>
                        </tr>
                    </table>

                    <div style="margin-top: 10px; max-height: 400px; overflow-y: auto; border: 1px solid #ccc;">
                        <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%">
                            <RowStyle BackColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <Columns>
                                <asp:TemplateField HeaderText="Select" ItemStyle-Width="5%">
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Product ID" Visible="false">
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
                                <asp:TemplateField HeaderText="Brand">
                                    <ItemTemplate>
                                        <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Specification">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Specification" runat="server" Text='<%# Bind("Specification") %>' CssClass="textbox_style" Width="90%"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Unit">
                                    <ItemTemplate>
                                        <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Base Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="textbox_style center" Width="80px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="GST %">
                                    <ItemTemplate>
                                        <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Quantity">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" Text="1" CssClass="textbox_style center" Width="60px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div style="margin-top: 15px; text-align: center;">
                        <asp:Button ID="btnPrev2" runat="server" Text="🡄 Back to Details" CssClass="btn_style" Width="150px" OnClick="btnPrev2_Click" />
                        &nbsp;
                        <asp:Button ID="btnNext2" runat="server" Text="Add to Cart ➔" CssClass="btn_style" Width="200px" OnClick="btnAddProduct_Click" CausesValidation="false" UseSubmitBehavior="false" />
                        <br />
                        <br />
                        <asp:Button ID="btnSkipCatalog" runat="server" Text="Skip Catalog (Go to Cart)" CssClass="btn_style" BackColor="#6c757d" ForeColor="White" Width="250px" OnClick="btnSkipCatalog_Click" CausesValidation="false" />
                    </div>
                </asp:View>

                <asp:View ID="View3_Cart" runat="server">
                    <div class="wizard-steps">Step 3 of 4: Review Cart & Calculations</div>
                    <div style="text-align: right; margin-bottom: 5px;">
                        <asp:Button ID="btnAddMoreProducts" runat="server" Text="+ Add More Products" CssClass="btn_style" Width="180px" BackColor="#17a2b8" ForeColor="White" OnClick="btnAddMoreProducts_Click" CausesValidation="false" UseSubmitBehavior="false" />
                    </div>

                    <div class="cart-grid-wrapper">
                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" ShowFooter="true" OnRowCommand="gd_Service_Product_RowCommand">
                            <RowStyle BackColor="White" />
                            <FooterStyle CssClass="grid-footer" />
                            <Columns>
                                <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="col-frozen-action" ItemStyle-CssClass="col-frozen-action" FooterStyle-CssClass="col-frozen-action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn">↑</asp:LinkButton>
                                        <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn">↓</asp:LinkButton>
                                        <asp:LinkButton ID="btnDel" runat="server" CommandName="DeleteRow" CommandArgument="<%# Container.DataItemIndex %>" CssClass="action-btn action-del">X</asp:LinkButton>
                                    </ItemTemplate>
                                    <FooterTemplate><b>TOTAL:</b></FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="SL" HeaderStyle-CssClass="col-frozen-sl" ItemStyle-CssClass="col-frozen-sl">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtOrder" runat="server" Width="30px" CssClass="center textbox_style" Text='<%# Bind("Sl_no") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Product Name" HeaderStyle-CssClass="col-frozen-name" ItemStyle-CssClass="col-frozen-name">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>' CssClass="center textbox_style qty-input" Width="60px" onkeyup="calculateCart()"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate><span id="ftr-qty">0</span></FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="center textbox_style rate-input" Width="80px" onkeyup="calculateCart()"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Disc %">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Discount_Rate" runat="server" Text='<%# Bind("discount_rate") %>' CssClass="center textbox_style disc-input" Width="50px" onkeyup="calculateCart()"></asp:TextBox>
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
                                    <FooterTemplate><span id="ftr-net" style="color: darkgreen;">0.00</span></FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Brand">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Brand" runat="server" CssClass="textbox_style" Width="100px" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Specification">
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
                                <asp:TemplateField HeaderText="Delivery Date" Visible="true">
                                    <ItemTemplate>
                                        <asp:TextBox ID="DeliveryDate" runat="server" CssClass="datepicker textbox_style center" Width="90px" Text='<%# Bind("DeliveryDate") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Department" Visible="true">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Department" runat="server" CssClass="textbox_style center" Width="90px" Text='<%# Bind("Department") %>'></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="PRD ID">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductId") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div style="margin-top: 15px; text-align: center;">
                        <asp:Button ID="btnPrev3" runat="server" Text="🡄 Back" CssClass="btn_style" Width="120px" OnClick="btnPrev3_Click" CausesValidation="false" />
                        &nbsp;
       
                        <asp:Button ID="btnNext3" runat="server" Text="Proceed to Terms ➔" CssClass="btn_style" Width="200px" OnClientClick="if (!validateCart()) return false;" OnClick="btnNext3_Click" CausesValidation="false" UseSubmitBehavior="false" />
                    </div>
                </asp:View>

                <asp:View ID="View4_Terms" runat="server">
                    <div class="wizard-steps">Step 4 of 4: Commercial Terms & Finalization</div>

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

                    <table cellpadding="4" cellspacing="2" class="auto-style1">
                        <tr>
                            <td width="15%" align="right">Validay Days: </td>
                            <td width="35%">
                                <asp:TextBox ID="txt_valdays" runat="server" Text="0" CssClass="textbox_style" TextMode="Number" Width="100px"></asp:TextBox></td>
                            <td width="15%" align="right">View Type: </td>
                            <td width="35%">
                                <asp:DropDownList ID="DDL_ItemViewType" runat="server" CssClass="dropdown_style">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Simple" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Detailed" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">Discount Visibility: </td>
                            <td>
                                <asp:DropDownList ID="DDL_DiscountView" runat="server" CssClass="dropdown_style">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="No" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="right">Delivery Tenure: </td>
                            <td>
                                <asp:DropDownList ID="DDL_DeliveryTerms" runat="server" CssClass="dropdown_style" onchange="handleDeliveryTermChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="10-12" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="3-4" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="1-2" Value="3"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="4"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="manualInputRow" runat="server" style="display: none;">
                            <td colspan="2"></td>
                            <td align="right">Manual Tenure: </td>
                            <td>
                                <asp:TextBox ID="txt_deltrms" runat="server" Text="0" CssClass="textbox_style" MaxLength="5" placeholder="e.g., 1-2"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td align="right">Package Forwarding: </td>
                            <td>
                                <asp:DropDownList ID="DDL_pkgfrwd" runat="server" CssClass="dropdown_style" onchange="handlePackageForwardingChange(this)">
                                    <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="NILL" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="At Actuals" Value="2"></asp:ListItem>
                                    <asp:ListItem Text="Manual Input" Value="3"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="right">TCS Amount: </td>
                            <td>
                                <asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_style center" Width="80px" Text="0"></asp:TextBox>
                                @
                                <asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_style center" Width="40px" Text="0"></asp:TextBox>%
                            </td>
                        </tr>
                        <tr id="manualInputPkgRow" runat="server" style="display: none;">
                            <td align="right">Manual Package: </td>
                            <td>
                                <asp:TextBox ID="txt_pkgfrwd" runat="server" Text="" CssClass="textbox_style"></asp:TextBox></td>
                            <td colspan="2"></td>
                        </tr>
                        <tr>
                            <td align="right">Freight Charges: </td>
                            <td>
                                <asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_style center" Width="80px" Text="0"></asp:TextBox>
                                @
                                <asp:TextBox ID="txt_freight_percent" runat="server" CssClass="textbox_style center" Width="40px" Text="0"></asp:TextBox>%
                            </td>
                            <td align="right">Other Charges: </td>
                            <td>Name:
                                <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_style" Width="100px"></asp:TextBox>
                                Amt:
                                <asp:TextBox ID="txt_othr_amnt" runat="server" CssClass="textbox_style center" Width="60px" Text="0"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="right" valign="top">Remarks: </td>
                            <td colspan="3">
                                <asp:TextBox ID="txt_remarks" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="3" Width="90%"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <hr />
                                <b>Add Payment Phase & Payment %age</b>
                                <table width="100%">
                                    <tr>
                                        <td width="30%" valign="top">
                                            <asp:ListBox ID="listPhaseType" runat="server" SelectionMode="Multiple" Rows="7" Width="100%" CssClass="dropdown_style" OnTextChanged="listPhaseType_TextChanged" AutoPostBack="True"></asp:ListBox>
                                        </td>
                                        <td width="70%" valign="top">
                                            <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" CssClass="Grid" OnRowDeleting="GridView3_RowDeleting" Width="100%">
                                                <RowStyle BackColor="White" />
                                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Payment %">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="AmountPer" runat="server" AutoPostBack="true" Text='<%# Bind("AmountPer") %>' CssClass="textbox_style center" Width="60px" OnTextChanged="AmountPer_TextChanged"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Phase Term">
                                                        <ItemTemplate>
                                                            <asp:Label ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Instruction">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' CssClass="textbox_style" Width="90%" TextMode="MultiLine"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:CommandField ButtonType="Button" HeaderText="Delete" ShowDeleteButton="True" ControlStyle-CssClass="btn_style" />
                                                </Columns>
                                            </asp:GridView>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>

                    <div style="margin-top: 20px; text-align: center;">
                        <asp:Button ID="btnPrev4" runat="server" Text="🡄 Back to Cart" CssClass="btn_style" Width="150px" OnClick="btnPrev4_Click" CausesValidation="false" />
                        <br />
                        <br />
                        <asp:Button ID="btnSabe" runat="server" CssClass="btn_style" OnClientClick="if (!ValidateDocument()) return false;" OnClick="btnSabe_Click" Text="💾 Update Existing Version" Width="250px" BackColor="#28a745" ForeColor="White" Font-Bold="true" />
                        &nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnNew" runat="server" CssClass="btn_style" OnClientClick="if (!ValidateDocument()) return false;" OnClick="btnNew_Click" Text="📄 Save as New Version" Width="250px" BackColor="#17a2b8" ForeColor="White" Font-Bold="true" />
                    </div>
                </asp:View>

            </asp:MultiView>

        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSabe" />
            <asp:PostBackTrigger ControlID="btnNew" />
            <asp:PostBackTrigger ControlID="Button2" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
