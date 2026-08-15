<%@ Page Title="Flame-Ex | Add New Products" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="newproduct_master.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm69" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        :root {
            --primary-color: #19658A;
            --border-color: #e2e8f0;
            --bg-card: #ffffff;
        }
        .page-header {
            display: flex; align-items: center; gap: 12px; margin-bottom: 18px;
            padding: 14px 18px; background: linear-gradient(135deg, #19658A 0%, #0f4a66 100%);
            border-radius: 8px; color: #fff;
        }
        .page-header .hdr-icon {
            width: 40px; height: 40px; border-radius: 8px; background: rgba(255,255,255,.15);
            display: flex; align-items: center; justify-content: center; font-size: 18px; font-weight: 700;
        }
        .page-header .hdr-text h1 { margin: 0; font-size: 18px; font-weight: 700; }
        .page-header .breadcrumb { margin: 2px 0 0; font-size: 12px; opacity: .85; }
        .master-layout-grid {
            display: grid; grid-template-columns: minmax(320px, 1.1fr) minmax(360px, 1.3fr);
            gap: 20px; align-items: start;
        }
        .box-panel {
            background: var(--bg-card); border: 1px solid var(--border-color);
            border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,.04);
        }
        .box-title {
            margin: 0 0 16px; padding-bottom: 10px; font-size: 15px; font-weight: 700;
            color: var(--primary-color); border-bottom: 3px solid var(--primary-color);
        }
        .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; margin-bottom: 8px; }
        .form-grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px; margin-bottom: 8px; }
        .form-group { margin-bottom: 4px; }
        .form-group label { display: block; font-weight: 600; font-size: 11px; color: #334155; margin-bottom: 6px; text-transform: uppercase; }
        .form-group .req { color: #dc2626; }
        .form-group .textbox_U_style, .form-group .dropdown_style, .form-group input[type=text], .form-group select {
            width: 100% !important; max-width: 100%; box-sizing: border-box; padding: 8px 10px;
            border: 1px solid var(--border-color); border-radius: 4px;
        }
        .action-toolbar { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; margin-top: 14px; }
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .grid-toolbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
        #txtCatalogSearch {
            min-width: 220px; padding: 8px 10px; border: 1px solid var(--border-color);
            border-radius: 4px; font-size: 13px; box-sizing: border-box;
        }
        .alert-ok, .alert-err { padding: 10px 12px; border-radius: 4px; margin-bottom: 14px; }
        .category-table { width: 100%; border-collapse: collapse; font-size: 11px; }
        .category-header td {
            background: var(--primary-color); color: #fff; font-weight: 700;
            padding: 8px 4px; text-align: center; border: 1px solid #0f4a66;
            position: sticky; top: 0; z-index: 2;
        }
        .category-item-row td { padding: 8px 4px; border: 1px solid var(--border-color); text-align: center; vertical-align: middle; }
        .category-item-row { background: #fff; }
        .category-item-row:nth-child(even) { background: #f8fafc; }
        .dup-panel { margin-top: 6px; }
        .span-2 { grid-column: span 2; }
        .span-3 { grid-column: span 3; }
        @media (max-width: 1100px) {
            .master-layout-grid { grid-template-columns: 1fr; }
            .form-grid-3 { grid-template-columns: 1fr 1fr; }
            .span-3 { grid-column: span 2; }
        }
        @media (max-width: 700px) {
            .form-grid-2, .form-grid-3 { grid-template-columns: 1fr; }
            .span-2, .span-3 { grid-column: span 1; }
        }
        .table1 { border-collapse: collapse; width: 100%; }
        .table1 td { text-align: left; border: 1px solid var(--border-color); }
        .table2 { border-collapse: collapse; width: 100%; }
        .table2 td { text-align: left; border: 1px solid var(--border-color); border-top: none; }
    </style>
    <script type="text/javascript">
        function showClientErr(text) {
            var errPanel = document.getElementById('<%= PanelError.ClientID %>');
            var errLbl = document.getElementById('<%= lblErrorMsg.ClientID %>');
            var okPanel = document.getElementById('<%= PanelOK.ClientID %>');
            if (okPanel) okPanel.style.display = 'none';
            if (errPanel) errPanel.style.display = 'block';
            if (errLbl) errLbl.innerHTML = text;
        }
        function validateProductForm() {
            var fields = [
                { id: '<%=cmdProduct.ClientID%>', message: "Select Product Category", isDropdown: true },
                { id: '<%=ddlProOrSer.ClientID%>', message: "Select Business Type", isDropdown: true },
                { id: '<%=txtSubProductsName.ClientID%>', message: "Provide Product Name" },
                { id: '<%=txtproducttype.ClientID%>', message: "Provide Product Specifications" },
                { id: '<%=txtBrand.ClientID%>', message: "Provide Brand Name" },
                { id: '<%=txtProductCode.ClientID%>', message: "Provide HSN Code" },
                { id: '<%=txtUnit.ClientID%>', message: "Provide UOM" },
                { id: '<%=TextBox2.ClientID%>', message: "Provide Opening Stock Value" },
                { id: '<%=txtSalerate.ClientID%>', message: "Provide Sale Rate." },
                { id: '<%=cmbtax.ClientID%>', message: "Please Select Tax Slab", isDropdown: true },
                { id: '<%=txtfromDate.ClientID%>', message: "Provide Expiry Date." },
                { id: '<%=TextBox4.ClientID%>', message: "Provide Sales Note" }
            ];
            for (var i = 0; i < fields.length; i++) {
                var field = document.getElementById(fields[i].id);
                if (!field) continue;
                if (fields[i].isDropdown) {
                    if (field.selectedIndex === 0) { showClientErr(fields[i].message); field.focus(); return false; }
                } else {
                    var v = (field.value || '').replace(/^\s+|\s+$/g, '');
                    field.value = v;
                    if (v === '') { showClientErr(fields[i].message); field.focus(); return false; }
                }
            }
            var rateEl = document.getElementById('<%=txtSalerate.ClientID%>');
            if (rateEl && isNaN(parseFloat(rateEl.value))) {
                showClientErr('Sale Rate must be numeric.');
                rateEl.focus();
                return false;
            }
            return true;
        }
        function ValidateField() { return validateProductForm(); }
        function preventDoubleSubmit() {
            var btn = document.getElementById('<%= btnSave.ClientID %>');
            if (btn) {
                btn.style.pointerEvents = 'none';
                btn.value = 'Saving...';
                setTimeout(function () { btn.disabled = true; }, 0);
            }
            return true;
        }
        function onProductSaveClick() {
            if (!validateProductForm()) return false;
            return preventDoubleSubmit();
        }
        function ResetFields() {
            var fields = [
                '<%=txtSubProductsName.ClientID%>', '<%=txtproducttype.ClientID%>', '<%=TextBox1.ClientID%>',
                '<%=txtBrand.ClientID%>', '<%=txtProductCode.ClientID%>', '<%=txtUnit.ClientID%>',
                '<%=TextBox2.ClientID%>', '<%=TextBox3.ClientID%>', '<%=txtSalerate.ClientID%>',
                '<%=txtfromDate.ClientID%>', '<%=TextBox4.ClientID%>'
            ];
            for (var i = 0; i < fields.length; i++) {
                var field = document.getElementById(fields[i]);
                if (field) field.value = '';
            }
            var dropdowns = ['<%=cmdProduct.ClientID%>', '<%=ddlProOrSer.ClientID%>', '<%=cmbtax.ClientID%>'];
            for (var j = 0; j < dropdowns.length; j++) {
                var dropdown = document.getElementById(dropdowns[j]);
                if (dropdown) dropdown.selectedIndex = 0;
            }
            return false;
        }
        function ValidateDelete1() {
            return confirm('Want to Delete this Products?');
        }
        function filterProductCatalog() {
            var input = document.getElementById('txtCatalogSearch');
            if (!input) return;
            var q = (input.value || '').toUpperCase();
            var rows = document.querySelectorAll('.category-item-row');
            for (var i = 0; i < rows.length; i++) {
                var t = (rows[i].innerText || rows[i].textContent || '').toUpperCase();
                rows[i].style.display = (!q || t.indexOf(q) > -1) ? '' : 'none';
            }
        }
        function validate(key) {
            var keycode = (key.which) ? key.which : key.keyCode;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) return false;
            return true;
        }
    </script>
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
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
        });
        function checkDuplicates() {
            var productName = $("#<%= txtSubProductsName.ClientID %>").val().trim();
            var category = $("#<%= cmdProduct.ClientID %>").val();
            $("#<%= lblDupMessage.ClientID %>").text("");
            $("#<%= lblSimilar.ClientID %>").text("");
            if (!productName) {
                $("#<%= lblDupMessage.ClientID %>").text("Please enter a product name to check.");
                return;
            }
            PageMethods.GetDuplicateInfo(productName, category,
                function (result) {
                    if (result.foundExact) {
                        var msg = "Exact product exists: Id=" + result.existingId;
                        if (result.productID) msg += " (ProductID: " + result.productID + ")";
                        $("#<%= lblDupMessage.ClientID %>").text(msg);
                    } else {
                        $("#<%= lblDupMessage.ClientID %>").text("No exact match found. You may proceed.");
                    }
                    if (result.similar && result.similar.length > 0) {
                        $("#<%= lblSimilar.ClientID %>").text("Similar products: " + result.similar.join(" | "));
                    }
                },
                function (err) {
                    $("#<%= lblDupMessage.ClientID %>").text("Unable to check duplicates. Please try again.");
                }
            );
        }
    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>

    <div class="page-header">
        <div class="hdr-icon">P</div>
        <div class="hdr-text">
            <div class="breadcrumb">Masters / Catalog</div>
            <h1>Product &amp; Service Master</h1>
        </div>
    </div>

    <div class="master-layout-grid">
        <div class="box-panel">
            <div class="box-title">Product Entry</div>

            <asp:Panel ID="PanelOK" runat="server" CssClass="alert-ok" BackColor="#EEFFDD"
                BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" style="display:none;">
                <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
            </asp:Panel>
            <asp:Panel ID="PanelError" runat="server" CssClass="alert-err" BorderColor="#FF3300"
                BorderStyle="Solid" BorderWidth="1px" style="display:none;">
                <asp:Image ID="Image1" runat="server" Height="16px"
                    ImageUrl="~/corporate/business/WebImages/Cross_icon.png" Width="16px" />
                &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-grid-2">
                <div class="form-group">
                    <label><asp:Label ID="Label16" runat="server" Text="*" CssClass="req"></asp:Label> Product / Service Category</label>
                    <asp:DropDownList ID="cmdProduct" runat="server" CssClass="dropdown_style" AutoPostBack="True" OnSelectedIndexChanged="cmdProduct_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label17" runat="server" Text="*" CssClass="req"></asp:Label> Business Type</label>
                    <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style">
                        <asp:ListItem>--Select--</asp:ListItem>
                        <asp:ListItem>Product</asp:ListItem>
                        <asp:ListItem>Service</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="form-grid-2">
                <div class="form-group span-2">
                    <label><asp:Label ID="Label18" runat="server" Text="*" CssClass="req"></asp:Label> Product / Service Name</label>
                    <asp:TextBox ID="txtSubProductsName" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                    <asp:Button ID="btnCheckDup" runat="server" Text="Check Duplicates" OnClientClick="checkDuplicates(); return false;" CssClass="btn btn_style" style="margin-top:6px;" />
                    <div id="dupResultPanel" class="dup-panel">
                        <asp:Label ID="lblDupMessage" runat="server" ForeColor="Crimson" />
                        <br />
                        <asp:Label ID="lblSimilar" runat="server" ForeColor="Gray" />
                    </div>
                </div>
            </div>

            <div class="form-grid-2">
                <div class="form-group">
                    <label>Product / Service Specifications</label>
                    <asp:TextBox ID="txtproducttype" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Extra Specifications</label>
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
            </div>

            <div class="form-grid-3">
                <div class="form-group">
                    <label><asp:Label ID="Label21" runat="server" Text="*" CssClass="req"></asp:Label> Make / Brand</label>
                    <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label22" runat="server" Text="*" CssClass="req"></asp:Label> HSN / SAC Code</label>
                    <asp:TextBox ID="txtProductCode" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label23" runat="server" Text="*" CssClass="req"></asp:Label> UOM</label>
                    <asp:TextBox ID="txtUnit" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
            </div>

            <div class="form-grid-3">
                <div class="form-group">
                    <label><asp:Label ID="Label20" runat="server" Text="*" CssClass="req"></asp:Label> Opening / Stock Qty</label>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>MOQ Value</label>
                    <asp:TextBox ID="TextBox3" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label24" runat="server" Text="*" CssClass="req"></asp:Label> Base Rate (Rs)</label>
                    <asp:TextBox ID="txtSalerate" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
            </div>

            <div class="form-grid-3">
                <div class="form-group">
                    <label><asp:Label ID="Label25" runat="server" Text="*" CssClass="req"></asp:Label> GST Rate (%)</label>
                    <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Expiry Date</label>
                    <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="100%"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Sale Note</label>
                    <asp:TextBox ID="TextBox4" runat="server" CssClass="textbox_U_style" Text="N/A"></asp:TextBox>
                </div>
            </div>

            <div class="action-toolbar">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return onProductSaveClick();" OnClick="btnSave_Click" />
                <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClientClick="return ResetFields();" Text="Reset" />
            </div>
        </div>

        <div class="box-panel">
            <div class="box-title">Product Catalog</div>
            <div class="grid-toolbar">
                <span style="font-size:13px;color:#64748b;">Active products for this company</span>
                <input type="text" id="txtCatalogSearch" placeholder="Search name / category / HSN..."
                    onkeyup="filterProductCatalog();" oninput="filterProductCatalog();" autocomplete="off" />
            </div>
            <div class="table-responsive">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#e2e8f0"
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="10px"
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%"
                    OnItemCommand="DataList1_ItemCommand" CssClass="category-table">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#f8fafc" />
                    <SeparatorStyle BorderColor="#e2e8f0" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#19658A" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1 category-table category-header" width="100%">
                            <tr>
                                <td style="text-align: center; width: 5%;"><asp:Label ID="showid" runat="server" Text="ID"></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label12" runat="server" Text="PRODUCT / SERVICE"></asp:Label></td>
                                <td style="text-align: center; width: 12%;"><asp:Label ID="showrm" runat="server" Text="PRODUCT / SERVICE CATEGORY"></asp:Label></td>
                                <td style="text-align: center; width: 15%;"><asp:Label ID="Label5" runat="server" Text="PRODUCT / SERVICE NAME"></asp:Label></td>
                                <td style="text-align: center; width: 18%;"><asp:Label ID="Label14" runat="server" Text="PRODUCT / SERVICE TYPE"></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label11" runat="server" Text="MAKE / BRAND"></asp:Label></td>
                                <td style="text-align: center; width: 7%;"><asp:Label ID="Label1" runat="server" Text="HSN / SAC CODE"></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label6" runat="server" Text="BASE RATE (RS)"></asp:Label></td>
                                <td style="text-align: center; width: 6%;"><asp:Label ID="Label8" runat="server" Text="GST RATE (%)"></asp:Label></td>
                                <td style="text-align: center; width: 4%;"><asp:Label ID="Label9" runat="server" Text="Edit"></asp:Label></td>
                                <td style="text-align: center; width: 3%;"><asp:Label ID="edit" runat="server" Text="Delete"></asp:Label></td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2 category-table category-item-row" width="100%">
                            <tr>
                                <td style="text-align: center; width: 5%;"><asp:Label ID="ID" runat="server" Text='<%# Eval("Id") %>'></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label2" runat="server" Text='<%# Eval("Type") %>'></asp:Label></td>
                                <td style="text-align: center; width: 12%;"><asp:Label ID="addshowname" runat="server" Text='<%# Eval("ProductOrServiceCat") %>'></asp:Label></td>
                                <td style="text-align: center; width: 15%;"><asp:Label ID="Label3" runat="server" Text='<%# Eval("ProductName") %>'></asp:Label></td>
                                <td style="text-align: center; width: 18%;"><asp:Label ID="Label15" runat="server" Text='<%# Eval("Product_catagory") %>'></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label10" runat="server" Text='<%# Eval("Brand") %>'></asp:Label></td>
                                <td style="text-align: center; width: 7%;"><asp:Label ID="Label13" runat="server" Text='<%# Eval("Product_code") %>'></asp:Label></td>
                                <td style="text-align: center; width: 10%;"><asp:Label ID="Label4" runat="server" Text='<%# Eval("Sail_Rate") %>'></asp:Label></td>
                                <td style="text-align: center; width: 6%;"><asp:Label ID="Label7" runat="server" Text='<%# Eval("Tax_Rate") %>'></asp:Label></td>
                                <td style="text-align: center; width: 4%;">
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandName="Edit" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/edit1.png" ToolTip="Edit" />
                                </td>
                                <td style="text-align: center; width: 3%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" OnClientClick="return ValidateDelete1();" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </div>
        </div>
    </div>
</asp:Content>
