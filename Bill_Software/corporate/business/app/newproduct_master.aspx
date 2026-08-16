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
        .stacked-container { display: flex; flex-direction: column; gap: 18px; }
        .box-panel {
            background: var(--bg-card); border: 1px solid var(--border-color);
            border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,.04);
        }
        .box-title {
            margin: 0 0 16px; padding-bottom: 10px; font-size: 15px; font-weight: 700;
            color: var(--primary-color); border-bottom: 3px solid var(--primary-color);
        }
        .form-grid-aligned {
            display: grid; grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 14px; margin-bottom: 8px; align-items: end;
        }
        .form-group { margin-bottom: 4px; }
        .form-group label { display: block; font-weight: 600; font-size: 11px; color: #334155; margin-bottom: 6px; text-transform: uppercase; }
        .form-group .req { color: #dc2626; }
        .form-group .textbox_U_style, .form-group .dropdown_style, .form-group input[type=text], .form-group select {
            width: 100% !important; max-width: 100%; box-sizing: border-box; padding: 8px 10px;
            border: 1px solid var(--border-color); border-radius: 4px;
        }
        .form-grid-aligned select,
        .form-grid-aligned .dropdown_style,
        .form-grid-aligned .form-control,
        .form-grid-aligned .form-control option,
        .form-grid-aligned select option {
            color: #0f172a !important; background-color: #ffffff !important;
        }
        .input-group { display: flex; gap: 8px; align-items: flex-start; }
        .input-group .textbox_U_style { flex: 1; }
        .input-group button { white-space: nowrap; margin-top: 0; }
        .avail-ok { color: #15803d; font-size: 12px; font-weight: 600; }
        .avail-bad { color: #b91c1c; font-size: 12px; font-weight: 600; }
        .img-preview { width: 64px; height: 64px; object-fit: cover; border: 1px solid var(--border-color); border-radius: 4px; display: none; }
        .img-preview.is-on { display: inline-block; }
        .thumb-40 { width: 40px; height: 40px; object-fit: cover; border-radius: 3px; border: 1px solid var(--border-color); }
        .span-2 { grid-column: span 2; }
        .span-4 { grid-column: span 4; }
        .action-toolbar { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; margin-top: 14px; }
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .grid-toolbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
        .grid-toolbar .search-wrap { display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }
        .alert-ok, .alert-err { padding: 10px 12px; border-radius: 4px; margin-bottom: 14px; }
        .prod-primary { font-weight: 700; color: #0f172a; margin-bottom: 4px; }
        .badge {
            display: inline-block; padding: 1px 6px; border-radius: 3px; font-size: 10px;
            background: #e2e8f0; color: #334155; margin-right: 4px;
        }
        .rate-main { font-weight: 700; font-size: 13px; color: #0f4a66; }
        .rate-sub { font-size: 11px; color: #64748b; }
        .action-links { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; }
        .btn-viewmore {
            border: 1px solid var(--border-color); background: #fff; color: var(--primary-color);
            padding: 3px 8px; border-radius: 3px; font-size: 11px; cursor: pointer;
        }
        .dup-panel { margin-top: 6px; }
        .modal-backdrop {
            display: none; position: fixed; z-index: 9999; left: 0; top: 0; right: 0; bottom: 0;
            background: rgba(15, 23, 42, .45); align-items: center; justify-content: center;
        }
        .modal-backdrop.is-open { display: flex; }
        .modal-box {
            background: #fff; border-radius: 8px; padding: 18px 20px; width: min(560px, 92vw);
            box-shadow: 0 12px 40px rgba(0,0,0,.2);
        }
        .modal-box h3 { margin: 0 0 12px; color: var(--primary-color); font-size: 16px; }
        .modal-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px 16px; font-size: 13px; }
        .modal-grid .full { grid-column: 1 / -1; }
        .modal-grid .k { color: #64748b; font-size: 11px; text-transform: uppercase; }
        .modal-grid .v { color: #0f172a; font-weight: 600; }
        .modal-close { margin-top: 14px; }
        @media (max-width: 1100px) {
            .form-grid-aligned { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .span-4 { grid-column: span 2; }
        }
        @media (max-width: 700px) {
            .form-grid-aligned { grid-template-columns: 1fr; }
            .span-2, .span-4 { grid-column: span 1; }
            .modal-grid { grid-template-columns: 1fr; }
        }
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
        function trimEl(el) {
            if (!el) return '';
            var v = (el.value || '').replace(/^\s+|\s+$/g, '');
            el.value = v;
            return v;
        }
        function isValidNumber(v) {
            if (v === '') return false;
            return !isNaN(v) && isFinite(Number(String(v).replace(/,/g, '')));
        }
        function validateProductForm() {
            var cat = document.getElementById('<%= cmdProduct.ClientID %>');
            if (!cat || cat.selectedIndex === 0) { showClientErr('Select Product Category'); if (cat) cat.focus(); return false; }
            var name = document.getElementById('<%= txtSubProductsName.ClientID %>');
            if (!trimEl(name)) { showClientErr('Provide Product Name'); if (name) name.focus(); return false; }
            var pid = document.getElementById('<%= txtProductID.ClientID %>');
            var hf = document.getElementById('<%= hfEditProductID.ClientID %>');
            if (hf && hf.value && !trimEl(pid)) { showClientErr('Product ID is required'); if (pid) pid.focus(); return false; }
            var hsn = document.getElementById('<%= txtProductCode.ClientID %>');
            if (!trimEl(hsn)) { showClientErr('Provide HSN / SAC Code'); if (hsn) hsn.focus(); return false; }
            var srate = document.getElementById('<%= txtSalerate.ClientID %>');
            var sv = trimEl(srate);
            if (!sv || !isValidNumber(sv)) { showClientErr('Provide a valid Selling Rate'); if (srate) srate.focus(); return false; }
            var tax = document.getElementById('<%= cmbtax.ClientID %>');
            if (!tax || tax.selectedIndex === 0) { showClientErr('Please Select Tax Slab'); if (tax) tax.focus(); return false; }
            return true;
        }
        function ValidateField() { return validateProductForm(); }
        function preventDoubleSubmit() {
            var btn = document.getElementById('<%= btnSave.ClientID %>');
            if (btn) {
                setTimeout(function () {
                    btn.disabled = true;
                    btn.value = 'Saving...';
                }, 0);
            }
            return true;
        }
        function onProductSaveClick() {
            if (!validateProductForm()) return false;
            return preventDoubleSubmit();
        }
        function ResetFields() {
            var fields = [
                '<%= txtSubProductsName.ClientID %>', '<%= txtproducttype.ClientID %>', '<%= TextBox1.ClientID %>',
                '<%= txtBrand.ClientID %>', '<%= txtProductCode.ClientID %>', '<%= txtUnit.ClientID %>',
                '<%= TextBox2.ClientID %>', '<%= TextBox3.ClientID %>', '<%= txtSalerate.ClientID %>',
                '<%= txtfromDate.ClientID %>', '<%= TextBox4.ClientID %>', '<%= txtProductID.ClientID %>',
                '<%= txtOemUrl.ClientID %>'
            ];
            for (var i = 0; i < fields.length; i++) {
                var field = document.getElementById(fields[i]);
                if (field) field.value = '';
            }
            var dropdowns = ['<%= cmdProduct.ClientID %>', '<%= ddlProOrSer.ClientID %>', '<%= cmbtax.ClientID %>'];
            for (var j = 0; j < dropdowns.length; j++) {
                var dropdown = document.getElementById(dropdowns[j]);
                if (dropdown) dropdown.selectedIndex = 0;
            }
            var hf = document.getElementById('<%= hfEditProductID.ClientID %>');
            if (hf) hf.value = '';
            var hfImg = document.getElementById('<%= hfProductImage.ClientID %>');
            if (hfImg) hfImg.value = '';
            var prev = document.getElementById('imgProductPreview');
            if (prev) { prev.src = ''; prev.className = 'img-preview'; }
            var av = document.getElementById('lblAvailability');
            if (av) { av.className = ''; av.textContent = ''; }
            var btn = document.getElementById('<%= btnSave.ClientID %>');
            if (btn) btn.value = 'Save';
            return false;
        }
        function ValidateDelete1() {
            return confirm('Want to Delete this Products?');
        }
        function checkDuplicateProduct() {
            var nameEl = document.getElementById('<%= txtSubProductsName.ClientID %>');
            var catEl = document.getElementById('<%= cmdProduct.ClientID %>');
            var hf = document.getElementById('<%= hfEditProductID.ClientID %>');
            var status = document.getElementById('lblAvailability');
            if (!nameEl || !status) return false;
            var name = (nameEl.value || '').replace(/^\s+|\s+$/g, '');
            nameEl.value = name;
            if (!name) {
                status.className = 'avail-bad';
                status.textContent = 'Enter a product name.';
                return false;
            }
            var cat = '';
            if (catEl && catEl.selectedIndex > 0)
                cat = (catEl.options[catEl.selectedIndex].text || '').replace(/^\s+|\s+$/g, '');
            if (!cat || cat === '--Select--') {
                status.className = 'avail-bad';
                status.textContent = 'Select a category first.';
                return false;
            }
            var excludeId = 0;
            if (hf && hf.value) excludeId = parseInt(hf.value, 10) || 0;
            status.className = '';
            status.textContent = 'Checking...';
            var url = window.location.pathname.replace(/\\/g, '/') + '/CheckDuplicateName';
            fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ productName: name, category: cat, excludeId: excludeId }),
                credentials: 'same-origin'
            }).then(function (res) {
                if (!res.ok) throw new Error('http');
                return res.json();
            }).then(function (data) {
                var r = (data && typeof data.d !== 'undefined') ? data.d : data;
                if (!r || r.checkedOk !== true) {
                    status.className = 'avail-bad';
                    status.textContent = 'Unable to check availability.';
                    return;
                }
                if (r.isDuplicate === true) {
                    status.className = 'avail-bad';
                    status.textContent = 'Name already exists.';
                } else {
                    status.className = 'avail-ok';
                    status.textContent = 'Name available.';
                }
            }).catch(function () {
                status.className = 'avail-bad';
                status.textContent = 'Unable to check availability.';
            });
            return false;
        }
        function previewImage(input) {
            var prev = document.getElementById('imgProductPreview');
            if (!input || !input.files || !input.files[0]) return;
            var f = input.files[0];
            var n = (f.name || '').toLowerCase();
            if (!(n.lastIndexOf('.jpg') === n.length - 4 || n.lastIndexOf('.jpeg') === n.length - 5 || n.lastIndexOf('.png') === n.length - 4 || n.lastIndexOf('.webp') === n.length - 5)) {
                input.value = '';
                if (prev) { prev.src = ''; prev.className = 'img-preview'; }
                showClientErr('Only .jpg, .png, .webp images are allowed.');
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) {
                if (prev) { prev.src = e.target.result; prev.className = 'img-preview is-on'; }
            };
            reader.readAsDataURL(f);
        }
        function showProductModal(btn) {
            if (!btn) return false;
            var d = btn.dataset || {};
            function g(k) { return d[k] || btn.getAttribute('data-' + k) || ''; }
            function setTxt(id, val) { var el = document.getElementById(id); if (el) el.textContent = val; }
            setTxt('mdPid', g('pid')); setTxt('mdName', g('name')); setTxt('mdCat', g('cat'));
            setTxt('mdType', g('type')); setTxt('mdHsn', g('hsn')); setTxt('mdBrand', g('brand'));
            setTxt('mdUnit', g('unit')); setTxt('mdSrate', g('srate')); setTxt('mdPrate', g('prate'));
            setTxt('mdTax', g('tax')); setTxt('mdSpec', g('spec'));
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop is-open';
            return false;
        }
        function closeProductModal() {
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop';
            return false;
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
    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <asp:HiddenField ID="hfEditProductID" runat="server" Value="" />
    <asp:HiddenField ID="hfProductImage" runat="server" Value="" />

    <div class="page-header">
        <div class="hdr-icon">P</div>
        <div class="hdr-text">
            <div class="breadcrumb">Masters / Catalog</div>
            <h1>Product &amp; Service Master</h1>
        </div>
    </div>

    <div class="stacked-container">
        <div class="box-panel">
            <div class="box-title">Product / Service Master</div>

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

            <div class="form-grid-aligned">
                <div class="form-group span-2">
                    <label><asp:Label ID="Label16" runat="server" Text="*" CssClass="req"></asp:Label> Category</label>
                    <asp:DropDownList ID="cmdProduct" runat="server" CssClass="dropdown_style form-control" AutoPostBack="True" OnSelectedIndexChanged="cmdProduct_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div class="form-group span-2">
                    <label><asp:Label ID="Label18" runat="server" Text="*" CssClass="req"></asp:Label> Product Name</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtSubProductsName" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                        <button type="button" class="btn_style" onclick="return checkDuplicateProduct();">Check Availability</button>
                    </div>
                    <span id="lblAvailability"></span>
                    <asp:Button ID="btnCheckDup" runat="server" Text="Check Duplicates" OnClientClick="return checkDuplicateProduct();" CssClass="btn btn_style" style="display:none;" />
                    <div id="dupResultPanel" class="dup-panel" style="display:none;">
                        <asp:Label ID="lblDupMessage" runat="server" ForeColor="Crimson" />
                        <br />
                        <asp:Label ID="lblSimilar" runat="server" ForeColor="Gray" />
                    </div>
                </div>

                <div class="form-group">
                    <label><asp:Label ID="Label17" runat="server" Text="*" CssClass="req"></asp:Label> Type</label>
                    <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style form-control">
                        <asp:ListItem>--Select--</asp:ListItem>
                        <asp:ListItem>Product</asp:ListItem>
                        <asp:ListItem>Service</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Product ID</label>
                    <asp:TextBox ID="txtProductID" runat="server" CssClass="textbox_U_style" ReadOnly="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label22" runat="server" Text="*" CssClass="req"></asp:Label> HSN / SAC</label>
                    <asp:TextBox ID="txtProductCode" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label23" runat="server" Text="*" CssClass="req"></asp:Label> Unit</label>
                    <asp:TextBox ID="txtUnit" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label><asp:Label ID="Label24" runat="server" Text="*" CssClass="req"></asp:Label> Selling Rate</label>
                    <asp:TextBox ID="txtSalerate" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label20" runat="server" Text="*" CssClass="req"></asp:Label> Opening / Stock Qty</label>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label25" runat="server" Text="*" CssClass="req"></asp:Label> GST / Tax</label>
                    <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style form-control"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label21" runat="server" Text="*" CssClass="req"></asp:Label> Brand</label>
                    <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>

                <div class="form-group span-2">
                    <label>OEM URL</label>
                    <asp:TextBox ID="txtOemUrl" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="form-group span-2">
                    <label>Product Image</label>
                    <asp:FileUpload ID="fuProductImage" runat="server" onchange="previewImage(this);" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                    <img id="imgProductPreview" class="img-preview" alt="" />
                </div>

                <div class="form-group">
                    <label>MOQ Value</label>
                    <asp:TextBox ID="TextBox3" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Expiry Date</label>
                    <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="100%"></asp:TextBox>
                </div>
                <div class="form-group span-2">
                    <label>Sale Note</label>
                    <asp:TextBox ID="TextBox4" runat="server" CssClass="textbox_U_style" Text="N/A"></asp:TextBox>
                </div>

                <div class="form-group span-4">
                    <label>Specification / Remarks</label>
                    <asp:TextBox ID="txtproducttype" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
                <div class="form-group span-4">
                    <label>Extra Specifications</label>
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>
            </div>

            <div class="action-toolbar">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return onProductSaveClick();" OnClick="btnSave_Click" />
                <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClientClick="return ResetFields();" Text="Reset" />
            </div>
        </div>

        <div class="box-panel">
            <div class="box-title">Product Catalog / Data Directory</div>
            <div class="grid-toolbar">
                <div class="search-wrap">
                    <asp:TextBox ID="txtGlobalSearch" runat="server" CssClass="textbox_U_style" Width="240px" placeholder="Search name / HSN / ID / brand / category"></asp:TextBox>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn_style" OnClick="btnSearch_Click" />
                </div>
                <div class="search-wrap">
                    <span style="font-size:12px;color:#64748b;">Page size</span>
                    <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" CssClass="dropdown_style form-control"
                        OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" Width="80px">
                        <asp:ListItem Text="10" Value="10" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="25" Value="25"></asp:ListItem>
                        <asp:ListItem Text="50" Value="50"></asp:ListItem>
                        <asp:ListItem Text="100" Value="100"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="gridProducts" runat="server" AutoGenerateColumns="False" CssClass="catalog-table"
                    Width="100%" AllowPaging="True" PageSize="10" GridLines="Both"
                    OnPageIndexChanging="gridProducts_PageIndexChanging"
                    OnRowCommand="gridProducts_RowCommand"
                    BorderColor="#e2e8f0" BorderStyle="Solid" BorderWidth="1px" Font-Size="10px"
                    HeaderStyle-BackColor="#19658A" HeaderStyle-ForeColor="White" HeaderStyle-Font-Bold="True"
                    AlternatingRowStyle-BackColor="#f8fafc" EmptyDataText="No products found.">
                    <Columns>
                        <asp:TemplateField HeaderText="Img" ItemStyle-Width="50px">
                            <ItemTemplate>
                                <asp:Image ID="imgThumb" runat="server" CssClass="thumb-40"
                                    ImageUrl='<%# Eval("ProductImage") %>'
                                    Visible='<%# Eval("ProductImage") != null && !string.IsNullOrEmpty(Convert.ToString(Eval("ProductImage"))) %>'
                                    onerror="this.style.display='none';" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="160px">
                            <ItemTemplate>
                                <div class="action-links">
                                    <button type="button" class="btn-viewmore"
                                        data-pid='<%# Server.HtmlEncode(Convert.ToString(Eval("ProductID"))) %>'
                                        data-name='<%# Server.HtmlEncode(Convert.ToString(Eval("ProductName"))) %>'
                                        data-cat='<%# Server.HtmlEncode(Convert.ToString(Eval("ProductOrServiceCat"))) %>'
                                        data-type='<%# Server.HtmlEncode(Convert.ToString(Eval("Type"))) %>'
                                        data-hsn='<%# Server.HtmlEncode(Convert.ToString(Eval("Product_code"))) %>'
                                        data-brand='<%# Server.HtmlEncode(Convert.ToString(Eval("Brand"))) %>'
                                        data-unit='<%# Server.HtmlEncode(Convert.ToString(Eval("Unit"))) %>'
                                        data-srate='<%# Server.HtmlEncode(Convert.ToString(Eval("Sail_Rate"))) %>'
                                        data-prate='<%# Server.HtmlEncode(Convert.ToString(Eval("Purches_Rate"))) %>'
                                        data-tax='<%# Server.HtmlEncode(Convert.ToString(Eval("Tax_Rate"))) %>'
                                        data-spec='<%# Server.HtmlEncode(Convert.ToString(Eval("Specification"))) %>'
                                        onclick="return showProductModal(this);">View More</button>
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandName="EditProduct" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/edit1.png" ToolTip="Edit" />
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="DeleteProduct" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Deactivate" OnClientClick="return ValidateDelete1();" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Product / Category">
                            <ItemTemplate>
                                <div class="prod-primary"><asp:Label ID="Label3" runat="server" Text='<%# Eval("ProductName") %>'></asp:Label></div>
                                <span class="badge"><asp:Label ID="addshowname" runat="server" Text='<%# Eval("ProductOrServiceCat") %>'></asp:Label></span>
                                <span class="badge"><asp:Label ID="Label2" runat="server" Text='<%# Eval("Type") %>'></asp:Label></span>
                                <asp:Label ID="ID" runat="server" Text='<%# Eval("Id") %>' style="display:none;"></asp:Label>
                                <asp:Label ID="Label15" runat="server" Text='<%# Eval("Product_catagory") %>' style="display:none;"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Identifiers" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <div><asp:Label ID="lblGridPid" runat="server" Text='<%# Eval("ProductID") %>'></asp:Label></div>
                                <div class="rate-sub">HSN: <asp:Label ID="Label13" runat="server" Text='<%# Eval("Product_code") %>'></asp:Label></div>
                                <%# FormatOemLink(Eval("OEMUrl")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Commercial" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <div class="rate-main"><asp:Label ID="Label4" runat="server" Text='<%# Eval("Sail_Rate") %>'></asp:Label></div>
                                <div class="rate-sub">Pur: <asp:Label ID="Label10b" runat="server" Text='<%# Eval("Purches_Rate") %>'></asp:Label>
                                    · GST: <asp:Label ID="Label7" runat="server" Text='<%# Eval("Tax_Rate") %>'></asp:Label>%</div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Unit / Brand" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <div><asp:Label ID="Label6u" runat="server" Text='<%# Eval("Unit") %>'></asp:Label></div>
                                <div class="rate-sub"><asp:Label ID="Label10" runat="server" Text='<%# Eval("Brand") %>'></asp:Label></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:DataList ID="DataList1" runat="server" Visible="false" OnItemCommand="DataList1_ItemCommand"></asp:DataList>
            </div>
        </div>
    </div>

    <div id="productModal" class="modal-backdrop" onclick="if(event.target===this)closeProductModal();">
        <div class="modal-box" onclick="event.stopPropagation();">
            <h3>Product Detail</h3>
            <div class="modal-grid">
                <div><div class="k">Product ID</div><div class="v" id="mdPid"></div></div>
                <div><div class="k">HSN / SAC</div><div class="v" id="mdHsn"></div></div>
                <div class="full"><div class="k">Name</div><div class="v" id="mdName"></div></div>
                <div><div class="k">Category</div><div class="v" id="mdCat"></div></div>
                <div><div class="k">Type</div><div class="v" id="mdType"></div></div>
                <div><div class="k">Brand</div><div class="v" id="mdBrand"></div></div>
                <div><div class="k">Unit</div><div class="v" id="mdUnit"></div></div>
                <div><div class="k">Selling Rate</div><div class="v" id="mdSrate"></div></div>
                <div><div class="k">Purchase Rate</div><div class="v" id="mdPrate"></div></div>
                <div><div class="k">GST / Tax</div><div class="v" id="mdTax"></div></div>
                <div class="full"><div class="k">Specification</div><div class="v" id="mdSpec"></div></div>
            </div>
            <div class="modal-close">
                <button type="button" class="btn_style" onclick="return closeProductModal();">Close</button>
            </div>
        </div>
    </div>
</asp:Content>
