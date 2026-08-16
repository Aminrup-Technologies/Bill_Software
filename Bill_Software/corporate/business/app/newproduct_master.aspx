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
            color: #0f172a; background-color: #ffffff;
            height: auto !important; min-height: 36px; line-height: 1.4;
        }
        .form-grid-aligned select,
        .form-grid-aligned .dropdown_style,
        .grid-toolbar select,
        .grid-toolbar .dropdown_style {
            color: #0f172a !important;
            background-color: #ffffff !important;
            -webkit-text-fill-color: #0f172a !important;
            opacity: 1 !important;
            height: auto !important;
            min-height: 36px;
            line-height: 1.4;
        }
        .form-grid-aligned select option,
        .form-grid-aligned .dropdown_style option,
        .grid-toolbar select option,
        .grid-toolbar .dropdown_style option {
            color: #0f172a;
            background-color: #ffffff;
        }
        .input-group { display: flex; gap: 8px; align-items: flex-start; }
        .input-group .textbox_U_style { flex: 1; }
        .input-group button { white-space: nowrap; margin-top: 0; }
        .avail-ok { color: #15803d; font-size: 12px; font-weight: 600; }
        .avail-bad { color: #b91c1c; font-size: 12px; font-weight: 600; }
        .img-preview { width: 64px; height: 64px; object-fit: cover; border: 1px solid var(--border-color); border-radius: 4px; display: none; }
        .img-preview.is-on { display: inline-block; }
        .view-upload-grid {
            display: grid; grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 12px; margin-top: 4px;
        }
        .view-slot {
            border: 1px solid var(--border-color); border-radius: 6px; padding: 10px;
            background: #f8fafc; text-align: center;
        }
        .view-slot .view-label { display: block; font-size: 11px; font-weight: 700; color: #334155; margin-bottom: 8px; text-transform: uppercase; }
        .view-slot input[type=file] { width: 100%; font-size: 11px; }
        .view-slot .img-preview { width: 72px; height: 72px; margin: 8px auto 0; }
        @media (max-width: 900px) {
            .view-upload-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
        }
        @media (max-width: 500px) {
            .view-upload-grid { grid-template-columns: 1fr; }
        }
        .thumb-40 { width: 40px; height: 40px; object-fit: cover; border-radius: 3px; border: 1px solid var(--border-color); }
        .span-2 { grid-column: span 2; }
        .span-3 { grid-column: span 3; }
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
        .catalog-table {
            border-collapse: collapse;
            border: 1px solid #dbe3ec;
        }
        .catalog-table th, .catalog-table td {
            padding: 7px 9px; vertical-align: middle;
            border: 1px solid #e8edf3;
        }
        .catalog-table th {
            white-space: nowrap;
            border-color: #c5d4e0;
        }
        .action-links { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; justify-content: center; }
        .btn-viewmore {
            border: 1px solid var(--border-color); background: #fff; color: var(--primary-color);
            padding: 3px 8px; border-radius: 3px; font-size: 11px; cursor: pointer;
        }
        .dup-panel { margin-top: 6px; }
        .modal-backdrop {
            display: none; position: fixed; z-index: 9999; left: 0; top: 0; right: 0; bottom: 0;
            background: rgba(15, 23, 42, .45); align-items: center; justify-content: center;
            padding: 12px;
        }
        .modal-backdrop.is-open { display: flex; }
        .modal-box {
            background: #fff; border-radius: 8px; padding: 18px 20px; width: min(560px, 92vw);
            max-height: 90vh; overflow-y: auto;
            box-shadow: 0 12px 40px rgba(0,0,0,.2);
        }
        .modal-box.product-detail-modal {
            width: min(920px, 96vw);
            max-height: 92vh;
            overflow: hidden;
            padding: 0;
            border: 1px solid #d7e2ec;
            display: flex;
            flex-direction: column;
        }
        .pd-header {
            display: flex; align-items: center; justify-content: space-between; gap: 20px;
            padding: 14px 16px;
            background: linear-gradient(135deg, #19658A 0%, #0f4a66 100%);
            border-radius: 8px 8px 0 0; color: #fff;
            flex-shrink: 0;
        }
        .pd-header-left {
            flex: 1; min-width: 0; display: flex; align-items: center; gap: 10px;
        }
        .pd-header-left .pd-eyebrow {
            display: block; font-size: 10px; text-transform: uppercase; letter-spacing: .05em;
            opacity: .8; margin-bottom: 2px;
        }
        .pd-header-left .pd-name {
            margin: 0; color: #fff; font-size: 20px; font-weight: 800; line-height: 1.2;
            word-break: break-word;
        }
        .pd-header-right {
            flex: 0 0 auto; display: flex; align-items: center; gap: 8px;
            background: rgba(255,255,255,.12); border: 1px solid rgba(255,255,255,.22);
            border-radius: 6px; padding: 6px 8px 6px 12px;
        }
        .pd-header-right .pd-meta-k {
            font-size: 9px; text-transform: uppercase; letter-spacing: .04em; opacity: .8; white-space: nowrap;
        }
        .pd-header-right .pd-meta-v {
            font-size: 13px; font-weight: 700; white-space: nowrap;
        }
        .pd-copy-btn {
            border: 0; background: #fff; color: #0f4a66; font-size: 10px; font-weight: 700;
            padding: 3px 8px; border-radius: 4px; cursor: pointer; white-space: nowrap; flex-shrink: 0;
        }
        .pd-copy-btn:hover { background: #e8f4fa; }
        .pd-copy-btn.is-copied { background: #bbf7d0; color: #166534; }
        .pd-body {
            padding: 12px 14px 10px;
            overflow-y: auto;
            overflow-x: hidden;
            flex: 1 1 auto;
            min-height: 0;
            -webkit-overflow-scrolling: touch;
        }
        .modal-box h3 { margin: 0 0 12px; color: var(--primary-color); font-size: 16px; }
        .modal-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px 16px; font-size: 13px; }
        .product-detail-modal .modal-grid {
            grid-template-columns: repeat(3, minmax(0, 1fr));
            gap: 8px; font-size: 12px;
        }
        .modal-grid .full { grid-column: 1 / -1; }
        .product-detail-modal .modal-grid .span2 { grid-column: span 2; }
        .modal-grid .k { color: #64748b; font-size: 11px; text-transform: uppercase; }
        .product-detail-modal .pd-field {
            background: #f8fafc; border: 1px solid #e8edf3; border-radius: 6px; padding: 7px 9px;
        }
        .product-detail-modal .modal-grid .k {
            font-size: 9px; line-height: 1.1; margin-bottom: 3px; color: #64748b; font-weight: 700;
        }
        .modal-grid .v { color: #0f172a; font-weight: 600; word-break: break-word; }
        .product-detail-modal .modal-grid .v {
            font-size: 12px; line-height: 1.3; font-weight: 600; color: #0f172a;
            display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
            overflow: hidden;
        }
        .modal-close { margin-top: 14px; }
        .product-detail-modal .modal-close {
            margin: 10px 0 0; padding-top: 10px; border-top: 1px solid #e8edf3;
            display: flex; justify-content: space-between; align-items: center; gap: 10px;
        }
        .product-detail-modal .btn-modal-edit {
            background: #19658A; color: #fff; border-color: #19658A;
        }
        .product-detail-modal .btn-modal-edit:hover { background: #0f4a66; }
        .flag-row { display: flex; flex-wrap: wrap; gap: 16px; align-items: center; padding-top: 4px; }
        .flag-row label { display: inline-flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600; color: #334155; margin: 0; }
        .audit-panel {
            margin: 10px 0 0; padding: 10px 12px; border: 1px solid #d7eaf4; border-radius: 8px;
            background: #f4fafd; display: grid; grid-template-columns: 1fr 1fr; gap: 8px 16px;
        }
        .audit-panel .audit-k { font-size: 10px; text-transform: uppercase; letter-spacing: .04em; color: #64748b; font-weight: 700; }
        .audit-panel .audit-v { font-size: 12px; font-weight: 600; color: #0f172a; word-break: break-word; }
        @media (max-width: 700px) { .audit-panel { grid-template-columns: 1fr; } }
        .modal-section { margin: 0 0 16px; padding-bottom: 14px; border-bottom: 1px solid var(--border-color); }
        .product-detail-modal .modal-section {
            margin: 0 0 10px; padding: 10px 10px 10px;
            background: #fff; border: 1px solid #e8edf3; border-radius: 8px;
        }
        .modal-section:last-of-type { border-bottom: 0; margin-bottom: 0; padding-bottom: 0; }
        .product-detail-modal .modal-section:last-of-type { margin-bottom: 0; }
        .modal-section-title {
            margin: 0 0 10px; font-size: 12px; font-weight: 700; letter-spacing: .04em;
            text-transform: uppercase; color: var(--primary-color);
        }
        .product-detail-modal .modal-section-title {
            margin: 0 0 8px; font-size: 11px; padding-bottom: 6px;
            border-bottom: 2px solid #d7eaf4; color: #19658A;
        }
        .pd-top-row {
            display: grid; grid-template-columns: 1fr 1fr; gap: 10px;
        }
        .pd-top-row .modal-section { margin-bottom: 0; }
        .view-gallery {
            display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px;
        }
        .view-gallery-item {
            position: relative; z-index: 1;
            border: 1px solid #e2e8f0; border-radius: 8px; padding: 6px;
            background: linear-gradient(180deg, #f8fafc 0%, #fff 100%);
            text-align: center; min-height: 0;
        }
        .view-gallery-item .view-label {
            display: block; font-size: 9px; font-weight: 700; color: #64748b;
            text-transform: uppercase; margin-bottom: 4px;
        }
        .view-gallery-item img {
            width: 100%; max-width: 120px; height: 64px; object-fit: cover;
            border-radius: 5px; border: 1px solid #e2e8f0; background: #fff;
            cursor: zoom-in; transition: box-shadow .15s ease, border-color .15s ease;
        }
        .view-gallery-item img:hover {
            border-color: #19658A;
            box-shadow: 0 4px 12px rgba(25, 101, 138, .25);
        }
        .view-gallery-item .view-empty {
            display: flex; align-items: center; justify-content: center;
            height: 64px; color: #94a3b8; font-size: 10px;
            border: 1px dashed #dbe3ec; border-radius: 5px; background: #f8fafc;
        }
        .view-gallery-item.is-empty { opacity: .75; }
        .oem-link { font-size: 12px; word-break: break-all; }
        .oem-link a { color: var(--primary-color); }
        .product-detail-modal .oem-link { font-size: 11px; }
        .img-lightbox {
            display: none; position: fixed; z-index: 10050; inset: 0;
            background: rgba(15, 23, 42, .92); align-items: center; justify-content: center;
            cursor: zoom-out; padding: 16px;
        }
        .img-lightbox.is-open { display: flex; }
        .img-lightbox img {
            max-width: 96vw; max-height: 90vh; object-fit: contain;
            border-radius: 6px; box-shadow: 0 16px 48px rgba(0,0,0,.45); background: #fff;
            cursor: zoom-out;
        }
        .img-lightbox-hint {
            position: absolute; bottom: 18px; left: 50%; transform: translateX(-50%);
            color: rgba(255,255,255,.85); font-size: 12px; letter-spacing: .02em;
        }
        @media (max-width: 820px) {
            .pd-top-row { grid-template-columns: 1fr; }
            .product-detail-modal .modal-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .pd-header { flex-direction: column; align-items: stretch; gap: 10px; }
            .pd-header-left .pd-name { font-size: 17px; }
            .pd-header-right { align-self: flex-start; }
        }
        @media (max-width: 700px) {
            .view-gallery { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .modal-box.product-detail-modal { max-height: 94vh; }
        }
        .similar-list { margin: 10px 0 0; padding-left: 18px; max-height: 220px; overflow-y: auto; }
        .similar-list li { margin: 4px 0; color: #0f172a; font-size: 13px; }
        .similar-note { margin: 0; color: #64748b; font-size: 13px; line-height: 1.4; }
        .expiry-presets { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 6px; }
        .expiry-presets button {
            border: 1px solid var(--border-color); background: #fff; color: #334155;
            padding: 4px 10px; border-radius: 4px; font-size: 11px; cursor: pointer;
        }
        .expiry-presets button.is-on { border-color: var(--primary-color); color: var(--primary-color); font-weight: 700; }
        .preview-file-note { font-size: 12px; color: #64748b; margin-top: 4px; }
        @media (max-width: 1100px) {
            .form-grid-aligned { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .span-3, .span-4 { grid-column: span 2; }
        }
        @media (max-width: 700px) {
            .form-grid-aligned { grid-template-columns: 1fr; }
            .span-2, .span-3, .span-4 { grid-column: span 1; }
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
            if (window.__productSaveConfirmed === true) {
                window.__productSaveConfirmed = false;
                if (!validateProductForm()) return false;
                return preventDoubleSubmit();
            }
            return openSavePreview();
        }
        function fmtExpiryDate(d) {
            var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
            var day = d.getDate();
            var dd = (day < 10 ? '0' : '') + day;
            return dd + '-' + months[d.getMonth()] + '-' + d.getFullYear();
        }
        function setExpiryMonths(months) {
            var d = new Date();
            d.setMonth(d.getMonth() + months);
            var el = document.getElementById('<%= txtfromDate.ClientID %>');
            if (el) el.value = fmtExpiryDate(d);
            var btns = document.querySelectorAll('.expiry-presets button');
            for (var i = 0; i < btns.length; i++) {
                var m = parseInt(btns[i].getAttribute('data-months'), 10);
                btns[i].className = (m === months) ? 'is-on' : '';
            }
            return false;
        }
        function setExpiryToday() {
            var el = document.getElementById('<%= txtfromDate.ClientID %>');
            if (el) el.value = fmtExpiryDate(new Date());
            var btns = document.querySelectorAll('.expiry-presets button');
            for (var i = 0; i < btns.length; i++) btns[i].className = '';
            return false;
        }
        function ddText(id) {
            var el = document.getElementById(id);
            if (!el || el.selectedIndex < 0) return '';
            return (el.options[el.selectedIndex].text || '').replace(/^\s+|\s+$/g, '');
        }
        function openSavePreview() {
            if (!validateProductForm()) return false;
            function val(id) {
                var el = document.getElementById(id);
                return el ? ((el.value || '').replace(/^\s+|\s+$/g, '')) : '';
            }
            function setTxt(id, v) { var el = document.getElementById(id); if (el) el.textContent = v || '—'; }
            setTxt('pvCat', ddText('<%= cmdProduct.ClientID %>'));
            setTxt('pvName', val('<%= txtSubProductsName.ClientID %>'));
            setTxt('pvType', ddText('<%= ddlProOrSer.ClientID %>'));
            setTxt('pvPid', val('<%= txtProductID.ClientID %>') || '(auto on save)');
            setTxt('pvHsn', val('<%= txtProductCode.ClientID %>'));
            setTxt('pvUnit', val('<%= txtUnit.ClientID %>'));
            setTxt('pvSrate', val('<%= txtSalerate.ClientID %>'));
            setTxt('pvPrate', val('<%= txtPurchaseRate.ClientID %>'));
            setTxt('pvQty', val('<%= TextBox2.ClientID %>'));
            setTxt('pvTax', ddText('<%= cmbtax.ClientID %>'));
            setTxt('pvBrand', val('<%= txtBrand.ClientID %>'));
            setTxt('pvOem', val('<%= txtOemUrl.ClientID %>'));
            setTxt('pvMoq', val('<%= TextBox3.ClientID %>'));
            setTxt('pvExpiry', val('<%= txtfromDate.ClientID %>'));
            setTxt('pvSaleNote', val('<%= TextBox4.ClientID %>'));
            setTxt('pvSpec', val('<%= txtproducttype.ClientID %>'));
            setTxt('pvExtra', val('<%= TextBox1.ClientID %>'));
            var isNew = document.getElementById('<%= chkIsNew.ClientID %>');
            var isFast = document.getElementById('<%= chkIsFastMoving.ClientID %>');
            var flags = [];
            if (isNew && isNew.checked) flags.push('New');
            if (isFast && isFast.checked) flags.push('Fast Moving');
            setTxt('pvFlags', flags.length ? flags.join(' · ') : '—');
            function fileLabel(id, label) {
                var fu = document.getElementById(id);
                if (fu && fu.files && fu.files.length > 0) return label + ': ' + fu.files[0].name;
                return '';
            }
            var parts = [];
            var t = fileLabel('<%= fuImgTop.ClientID %>', 'Top');
            var b = fileLabel('<%= fuImgBottom.ClientID %>', 'Bottom');
            var l = fileLabel('<%= fuImgLeft.ClientID %>', 'Left');
            var r = fileLabel('<%= fuImgRight.ClientID %>', 'Right');
            if (t) parts.push(t); if (b) parts.push(b); if (l) parts.push(l); if (r) parts.push(r);
            var fileNote = document.getElementById('pvFileNote');
            if (fileNote) {
                if (parts.length)
                    fileNote.textContent = 'New uploads — ' + parts.join(' | ');
                else if (val('<%= hfProductImage.ClientID %>'))
                    fileNote.textContent = 'Existing view images will be kept unless replaced.';
                else
                    fileNote.textContent = 'No product view images selected.';
            }
            var backdrop = document.getElementById('savePreviewModal');
            if (backdrop) backdrop.className = 'modal-backdrop is-open';
            return false;
        }
        function closeSavePreview() {
            var backdrop = document.getElementById('savePreviewModal');
            if (backdrop) backdrop.className = 'modal-backdrop';
            return false;
        }
        function confirmSaveFromPreview() {
            closeSavePreview();
            window.__productSaveConfirmed = true;
            var btn = document.getElementById('<%= btnSave.ClientID %>');
            if (btn) btn.click();
            return false;
        }
        function prepareReset() {
            var ids = [
                '<%= fuImgTop.ClientID %>', '<%= fuImgBottom.ClientID %>',
                '<%= fuImgLeft.ClientID %>', '<%= fuImgRight.ClientID %>'
            ];
            for (var i = 0; i < ids.length; i++) {
                var fu = document.getElementById(ids[i]);
                if (fu) fu.value = '';
            }
            var prevIds = ['imgPrevTop', 'imgPrevBottom', 'imgPrevLeft', 'imgPrevRight'];
            for (var j = 0; j < prevIds.length; j++) {
                var prev = document.getElementById(prevIds[j]);
                if (prev) { prev.src = ''; prev.className = 'img-preview'; }
            }
            var av = document.getElementById('lblAvailability');
            if (av) { av.className = ''; av.textContent = ''; }
            return true;
        }
        function ResetFields() {
            return prepareReset();
        }
        function ValidateDelete1() {
            return confirm('Soft-delete this product?\n\nIt will be deactivated and hidden from the catalog (not permanently removed). Continue?');
        }
        function showSimilarModal(title, msg, items) {
            var backdrop = document.getElementById('similarModal');
            var titleEl = document.getElementById('similarModalTitle');
            var msgEl = document.getElementById('similarModalMsg');
            var listEl = document.getElementById('similarModalList');
            if (titleEl) titleEl.textContent = title || 'Similar products found';
            if (msgEl) msgEl.textContent = msg || '';
            if (listEl) {
                listEl.innerHTML = '';
                var arr = items || [];
                for (var i = 0; i < arr.length; i++) {
                    var li = document.createElement('li');
                    li.textContent = arr[i];
                    listEl.appendChild(li);
                }
            }
            if (backdrop) backdrop.className = 'modal-backdrop is-open';
        }
        function closeSimilarModal() {
            var backdrop = document.getElementById('similarModal');
            if (backdrop) backdrop.className = 'modal-backdrop';
            return false;
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
                if (r.isDuplicate === true || r.hasSimilar === true) {
                    status.className = 'avail-bad';
                    status.textContent = r.isDuplicate === true
                        ? 'Name already exists.'
                        : 'Similar product already exists.';
                    showSimilarModal(
                        r.isDuplicate === true ? 'Duplicate product name' : 'Similar products found',
                        r.isDuplicate === true
                            ? 'An exact match already exists in this category. Use a different name.'
                            : 'Your text matches existing product name(s). Change the name before saving.',
                        r.similar || []
                    );
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
        function previewImage(input, previewId) {
            var prev = document.getElementById(previewId || 'imgPrevTop');
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
        function copyPdText(sourceId, btn) {
            var el = document.getElementById(sourceId);
            var text = el ? (el.textContent || '').replace(/^\s+|\s+$/g, '') : '';
            if (!text || text === '—') return false;
            function done() {
                if (!btn) return;
                var old = btn.textContent;
                btn.textContent = 'Copied';
                btn.className = 'pd-copy-btn is-copied';
                setTimeout(function () {
                    btn.textContent = old || 'Copy';
                    btn.className = 'pd-copy-btn';
                }, 1200);
            }
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text).then(done).catch(function () {
                    window.prompt('Copy:', text);
                });
            } else {
                window.prompt('Copy:', text);
                done();
            }
            return false;
        }
        function openImageLightbox(url, label) {
            if (!url) return false;
            var box = document.getElementById('imgLightbox');
            var img = document.getElementById('imgLightboxSrc');
            var hint = document.getElementById('imgLightboxHint');
            if (img) {
                img.src = url;
                img.alt = label || 'Product view';
            }
            if (hint) hint.textContent = (label ? label + ' — ' : '') + 'Click anywhere to close';
            if (box) box.className = 'img-lightbox is-open';
            return false;
        }
        function closeImageLightbox() {
            var box = document.getElementById('imgLightbox');
            var img = document.getElementById('imgLightboxSrc');
            if (box) box.className = 'img-lightbox';
            if (img) img.removeAttribute('src');
            return false;
        }
        document.addEventListener('keydown', function (e) {
            if ((e.key === 'Escape' || e.keyCode === 27) && document.getElementById('imgLightbox') &&
                document.getElementById('imgLightbox').className.indexOf('is-open') >= 0) {
                closeImageLightbox();
            }
        });
        function showProductModal(btn) {
            if (!btn) return false;
            var d = btn.dataset || {};
            function g(k) { return d[k] || btn.getAttribute('data-' + k) || ''; }
            function setTxt(id, val) {
                var el = document.getElementById(id);
                if (el) el.textContent = (val && String(val).replace(/^\s+|\s+$/g, '')) ? val : '—';
            }
            function setView(imgId, emptyId, itemId, url, label) {
                var img = document.getElementById(imgId);
                var empty = document.getElementById(emptyId);
                var item = document.getElementById(itemId);
                var has = !!(url && String(url).replace(/^\s+|\s+$/g, ''));
                if (img) {
                    img.onclick = null;
                    if (has) {
                        img.src = url;
                        img.style.display = 'inline-block';
                        img.title = 'Click to enlarge';
                        img.onclick = function () { return openImageLightbox(url, label); };
                        img.onerror = function () {
                            img.style.display = 'none';
                            img.onclick = null;
                            if (empty) empty.style.display = 'flex';
                            if (item) item.className = 'view-gallery-item is-empty';
                        };
                    } else {
                        img.removeAttribute('src');
                        img.style.display = 'none';
                    }
                }
                if (empty) empty.style.display = has ? 'none' : 'flex';
                if (item) item.className = has ? 'view-gallery-item' : 'view-gallery-item is-empty';
            }
            var nameVal = g('name');
            var pidVal = g('pid');
            setTxt('mdHdrName', nameVal); setTxt('mdHdrPid', pidVal);
            window._pdRowId = g('id') || '';
            setTxt('mdCat', g('cat')); setTxt('mdType', g('type')); setTxt('mdHsn', g('hsn'));
            setTxt('mdBrand', g('brand')); setTxt('mdUnit', g('unit'));
            setTxt('mdSrate', g('srate')); setTxt('mdPrate', g('prate'));
            setTxt('mdTax', g('tax')); setTxt('mdQty', g('qty')); setTxt('mdMoq', g('moq'));
            setTxt('mdExpiry', g('expiry')); setTxt('mdSaleNote', g('salenote'));
            setTxt('mdRemarks', g('remarks')); setTxt('mdSpec', g('spec'));
            setTxt('mdIsNew', g('isnew') === '1' || g('isnew').toLowerCase() === 'true' ? 'Yes' : 'No');
            setTxt('mdIsFast', g('isfast') === '1' || g('isfast').toLowerCase() === 'true' ? 'Yes' : 'No');
            setTxt('mdCreated', g('created') || '—');
            setTxt('mdModified', g('modified') || '—');
            var oem = g('oem');
            var oemEl = document.getElementById('mdOem');
            if (oemEl) {
                oemEl.innerHTML = '';
                if (oem) {
                    var a = document.createElement('a');
                    a.href = oem;
                    a.target = '_blank';
                    a.rel = 'noopener noreferrer';
                    a.textContent = oem;
                    oemEl.appendChild(a);
                } else {
                    oemEl.textContent = '—';
                }
            }
            setView('mdImgTop', 'mdEmptyTop', 'mdItemTop', g('imgtop'), 'Top View');
            setView('mdImgBottom', 'mdEmptyBottom', 'mdItemBottom', g('imgbottom'), 'Bottom View');
            setView('mdImgLeft', 'mdEmptyLeft', 'mdItemLeft', g('imgleft'), 'Left View');
            setView('mdImgRight', 'mdEmptyRight', 'mdItemRight', g('imgright'), 'Right View');
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop is-open';
            return false;
        }
        function closeProductModal() {
            closeImageLightbox();
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop';
            return false;
        }
        function editFromProductModal() {
            var id = window._pdRowId || '';
            if (!id) {
                alert('Unable to edit: product id missing.');
                return false;
            }
            var hf = document.getElementById('<%= hfModalEditId.ClientID %>');
            if (hf) hf.value = id;
            closeProductModal();
            __doPostBack('<%= btnModalEdit.UniqueID %>', '');
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
    <asp:HiddenField ID="hfModalEditId" runat="server" Value="" />
    <asp:Button ID="btnModalEdit" runat="server" style="display:none;" CausesValidation="false"
        OnClick="btnModalEdit_Click" UseSubmitBehavior="false" />
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
                    <asp:DropDownList ID="cmdProduct" runat="server" CssClass="dropdown_style" AutoPostBack="True" OnSelectedIndexChanged="cmdProduct_SelectedIndexChanged"></asp:DropDownList>
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
                    <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style">
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
                    <label>Purchase Rate</label>
                    <asp:TextBox ID="txtPurchaseRate" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label20" runat="server" Text="*" CssClass="req"></asp:Label> Opening / Stock Qty</label>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label25" runat="server" Text="*" CssClass="req"></asp:Label> GST / Tax</label>
                    <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label><asp:Label ID="Label21" runat="server" Text="*" CssClass="req"></asp:Label> Brand</label>
                    <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style"></asp:TextBox>
                </div>

                <div class="form-group span-2">
                    <label>OEM Reference URL (optional)</label>
                    <asp:TextBox ID="txtOemUrl" runat="server" CssClass="textbox_U_style" placeholder="https://oem-reference-or-catalog-link"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>MOQ Value</label>
                    <asp:TextBox ID="TextBox3" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Catalog Flags</label>
                    <div class="flag-row">
                        <label><asp:CheckBox ID="chkIsNew" runat="server" /> Is New</label>
                        <label><asp:CheckBox ID="chkIsFastMoving" runat="server" /> Fast Moving</label>
                    </div>
                </div>

                <div class="form-group span-4">
                    <label>Product Images (Top / Bottom / Left / Right)</label>
                    <div class="view-upload-grid">
                        <div class="view-slot">
                            <span class="view-label">Top View</span>
                            <asp:FileUpload ID="fuImgTop" runat="server" onchange="previewImage(this,'imgPrevTop');" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                            <img id="imgPrevTop" class="img-preview" alt="Top" />
                        </div>
                        <div class="view-slot">
                            <span class="view-label">Bottom View</span>
                            <asp:FileUpload ID="fuImgBottom" runat="server" onchange="previewImage(this,'imgPrevBottom');" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                            <img id="imgPrevBottom" class="img-preview" alt="Bottom" />
                        </div>
                        <div class="view-slot">
                            <span class="view-label">Left View</span>
                            <asp:FileUpload ID="fuImgLeft" runat="server" onchange="previewImage(this,'imgPrevLeft');" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                            <img id="imgPrevLeft" class="img-preview" alt="Left" />
                        </div>
                        <div class="view-slot">
                            <span class="view-label">Right View</span>
                            <asp:FileUpload ID="fuImgRight" runat="server" onchange="previewImage(this,'imgPrevRight');" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" />
                            <img id="imgPrevRight" class="img-preview" alt="Right" />
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label>Expiry Date</label>
                    <div class="expiry-presets">
                        <button type="button" data-months="0" onclick="return setExpiryToday();">Today</button>
                        <button type="button" data-months="6" onclick="return setExpiryMonths(6);">6 Months</button>
                        <button type="button" data-months="12" onclick="return setExpiryMonths(12);">12 Months</button>
                    </div>
                    <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="100%"></asp:TextBox>
                </div>
                <div class="form-group span-3">
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

            <asp:Panel ID="pnlAuditTrail" runat="server" CssClass="audit-panel" Visible="false">
                <div>
                    <div class="audit-k">Created By</div>
                    <div class="audit-v"><asp:Label ID="lblAuditCreated" runat="server" Text="—"></asp:Label></div>
                </div>
                <div>
                    <div class="audit-k">Last Modified By</div>
                    <div class="audit-v"><asp:Label ID="lblAuditModified" runat="server" Text="—"></asp:Label></div>
                </div>
            </asp:Panel>

            <div class="action-toolbar">
                <button type="button" class="btn_style" onclick="return openSavePreview();">Preview</button>
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return onProductSaveClick();" OnClick="btnSave_Click" />
                <asp:Button ID="btnreset" runat="server" CssClass="btn_style" Text="Reset"
                    OnClientClick="return prepareReset();" OnClick="btnreset_Click" />
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
                    <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" CssClass="dropdown_style"
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
                    BorderColor="#dbe3ec" BorderStyle="Solid" BorderWidth="1px" Font-Size="11px"
                    HeaderStyle-BackColor="#19658A" HeaderStyle-ForeColor="White" HeaderStyle-Font-Bold="True"
                    AlternatingRowStyle-BackColor="#f8fafc" EmptyDataText="No products found.">
                    <Columns>
                        <asp:TemplateField HeaderText="Product ID" ItemStyle-Width="90px">
                            <ItemTemplate>
                                <asp:Label ID="lblGridPid" runat="server" CssClass="prod-primary" Text='<%# Eval("ProductID") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Product Name">
                            <ItemTemplate>
                                <div class="prod-primary"><asp:Label ID="Label3" runat="server" Text='<%# Eval("ProductName") %>'></asp:Label></div>
                                <asp:Label ID="ID" runat="server" Text='<%# Eval("Id") %>' style="display:none;"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Category" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="addshowname" runat="server" Text='<%# Eval("ProductOrServiceCat") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Type" ItemStyle-Width="70px">
                            <ItemTemplate>
                                <span class="badge"><asp:Label ID="Label2" runat="server" Text='<%# Eval("Type") %>'></asp:Label></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="HSN / SAC" ItemStyle-Width="90px">
                            <ItemTemplate>
                                <asp:Label ID="Label13" runat="server" Text='<%# Eval("Product_code") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Brand" ItemStyle-Width="90px">
                            <ItemTemplate>
                                <asp:Label ID="Label10" runat="server" Text='<%# Eval("Brand") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Unit" ItemStyle-Width="60px">
                            <ItemTemplate>
                                <asp:Label ID="Label6u" runat="server" Text='<%# Eval("Unit") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Rate" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span class="rate-main"><asp:Label ID="Label4" runat="server" Text='<%# Eval("Sail_Rate") %>'></asp:Label></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="GST %" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <asp:Label ID="Label7" runat="server" Text='<%# Eval("Tax_Rate") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Stock" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <asp:Label ID="LabelQty" runat="server" Text='<%# Eval("Quantity") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div class="action-links">
                                    <button type="button" class="btn-viewmore"
                                        data-id='<%# Eval("Id") %>'
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
                                        data-qty='<%# Server.HtmlEncode(Convert.ToString(Eval("Quantity"))) %>'
                                        data-moq='<%# Server.HtmlEncode(Convert.ToString(Eval("MOQ_Value"))) %>'
                                        data-expiry='<%# Server.HtmlEncode(FormatExpiry(Eval("ExpiryDate"))) %>'
                                        data-salenote='<%# Server.HtmlEncode(Convert.ToString(Eval("SaleNote"))) %>'
                                        data-remarks='<%# Server.HtmlEncode(Convert.ToString(Eval("Product_catagory"))) %>'
                                        data-spec='<%# Server.HtmlEncode(Convert.ToString(Eval("Specification"))) %>'
                                        data-oem='<%# ViewImageUrl(Eval("ImageUrl"), "O") %>'
                                        data-imgtop='<%# ViewImageUrl(Eval("ImageUrl"), "T") %>'
                                        data-imgbottom='<%# ViewImageUrl(Eval("ImageUrl"), "B") %>'
                                        data-imgleft='<%# ViewImageUrl(Eval("ImageUrl"), "L") %>'
                                        data-imgright='<%# ViewImageUrl(Eval("ImageUrl"), "R") %>'
                                        data-isnew='<%# Eval("IsNew") %>'
                                        data-isfast='<%# Eval("IsFastMoving") %>'
                                        data-created='<%# Server.HtmlEncode(FormatAuditTrail(Eval("AddedbyUserId"), Eval("AddedOn"))) %>'
                                        data-modified='<%# Server.HtmlEncode(FormatAuditTrail(Eval("ModifiedByUserId"), Eval("ModifiedOn"))) %>'
                                        onclick="return showProductModal(this);">View More</button>
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandName="EditProduct" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/edit1.png" ToolTip="Edit" />
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="DeleteProduct" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Soft Delete / Deactivate" OnClientClick="return ValidateDelete1();" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:DataList ID="DataList1" runat="server" Visible="false" OnItemCommand="DataList1_ItemCommand"></asp:DataList>
            </div>
        </div>
    </div>

    <div id="productModal" class="modal-backdrop" onclick="if(event.target===this)closeProductModal();">
        <div class="modal-box product-detail-modal" onclick="event.stopPropagation();">
            <div class="pd-header">
                <div class="pd-header-left">
                    <div style="min-width:0;flex:1;">
                        <span class="pd-eyebrow">Product Detail</span>
                        <div class="pd-name" id="mdHdrName">—</div>
                    </div>
                    <button type="button" class="pd-copy-btn" onclick="return copyPdText('mdHdrName', this);">Copy</button>
                </div>
                <div class="pd-header-right">
                    <div>
                        <div class="pd-meta-k">Product ID</div>
                        <div class="pd-meta-v" id="mdHdrPid">—</div>
                    </div>
                    <button type="button" class="pd-copy-btn" onclick="return copyPdText('mdHdrPid', this);">Copy</button>
                </div>
            </div>

            <div class="pd-body">
                <div class="pd-top-row">
                    <div class="modal-section">
                        <div class="modal-section-title">1. Identity</div>
                        <div class="modal-grid">
                            <div class="pd-field"><div class="k">HSN / SAC</div><div class="v" id="mdHsn"></div></div>
                            <div class="pd-field"><div class="k">Category</div><div class="v" id="mdCat"></div></div>
                            <div class="pd-field"><div class="k">Type</div><div class="v" id="mdType"></div></div>
                            <div class="pd-field"><div class="k">Brand</div><div class="v" id="mdBrand"></div></div>
                            <div class="pd-field"><div class="k">Unit</div><div class="v" id="mdUnit"></div></div>
                        </div>
                    </div>
                    <div class="modal-section">
                        <div class="modal-section-title">2. Commercial</div>
                        <div class="modal-grid">
                            <div class="pd-field"><div class="k">Selling Rate</div><div class="v" id="mdSrate"></div></div>
                            <div class="pd-field"><div class="k">Purchase Rate</div><div class="v" id="mdPrate"></div></div>
                            <div class="pd-field"><div class="k">GST / Tax</div><div class="v" id="mdTax"></div></div>
                            <div class="pd-field"><div class="k">Stock Qty</div><div class="v" id="mdQty"></div></div>
                            <div class="pd-field"><div class="k">MOQ</div><div class="v" id="mdMoq"></div></div>
                            <div class="pd-field"><div class="k">Expiry Date</div><div class="v" id="mdExpiry"></div></div>
                        </div>
                    </div>
                </div>

                <div class="modal-section">
                    <div class="modal-section-title">3. Specifications &amp; Notes</div>
                    <div class="modal-grid">
                        <div class="pd-field span2"><div class="k">Specification / Remarks</div><div class="v" id="mdRemarks"></div></div>
                        <div class="pd-field"><div class="k">Sale Note</div><div class="v" id="mdSaleNote"></div></div>
                        <div class="pd-field span2"><div class="k">Extra Specifications</div><div class="v" id="mdSpec"></div></div>
                        <div class="pd-field"><div class="k">OEM Reference URL</div><div class="v oem-link" id="mdOem"></div></div>
                        <div class="pd-field"><div class="k">Is New</div><div class="v" id="mdIsNew"></div></div>
                        <div class="pd-field"><div class="k">Fast Moving</div><div class="v" id="mdIsFast"></div></div>
                    </div>
                </div>

                <div class="modal-section">
                    <div class="modal-section-title">Audit Trail</div>
                    <div class="modal-grid">
                        <div class="pd-field span2"><div class="k">Created By</div><div class="v" id="mdCreated"></div></div>
                        <div class="pd-field span2"><div class="k">Last Modified By</div><div class="v" id="mdModified"></div></div>
                    </div>
                </div>

                <div class="modal-section">
                    <div class="modal-section-title">4. Product Views</div>
                    <div class="view-gallery">
                        <div class="view-gallery-item" id="mdItemTop">
                            <span class="view-label">Top View</span>
                            <img id="mdImgTop" alt="Top View" style="display:none;" />
                            <div class="view-empty" id="mdEmptyTop">No image</div>
                        </div>
                        <div class="view-gallery-item" id="mdItemBottom">
                            <span class="view-label">Bottom View</span>
                            <img id="mdImgBottom" alt="Bottom View" style="display:none;" />
                            <div class="view-empty" id="mdEmptyBottom">No image</div>
                        </div>
                        <div class="view-gallery-item" id="mdItemLeft">
                            <span class="view-label">Left View</span>
                            <img id="mdImgLeft" alt="Left View" style="display:none;" />
                            <div class="view-empty" id="mdEmptyLeft">No image</div>
                        </div>
                        <div class="view-gallery-item" id="mdItemRight">
                            <span class="view-label">Right View</span>
                            <img id="mdImgRight" alt="Right View" style="display:none;" />
                            <div class="view-empty" id="mdEmptyRight">No image</div>
                        </div>
                    </div>
                </div>

                <div class="modal-close">
                    <button type="button" class="btn_style btn-modal-edit" onclick="return editFromProductModal();">Edit Product</button>
                    <button type="button" class="btn_style" onclick="return closeProductModal();">Close</button>
                </div>
            </div>
        </div>
    </div>

    <div id="imgLightbox" class="img-lightbox" onclick="return closeImageLightbox();">
        <img id="imgLightboxSrc" alt="" />
        <div id="imgLightboxHint" class="img-lightbox-hint">Click anywhere to close</div>
    </div>

    <div id="similarModal" class="modal-backdrop" onclick="if(event.target===this)closeSimilarModal();">
        <div class="modal-box" onclick="event.stopPropagation();">
            <h3 id="similarModalTitle">Similar products found</h3>
            <p id="similarModalMsg" class="similar-note"></p>
            <ul id="similarModalList" class="similar-list"></ul>
            <div class="modal-close">
                <button type="button" class="btn_style" onclick="return closeSimilarModal();">Close</button>
            </div>
        </div>
    </div>

    <div id="savePreviewModal" class="modal-backdrop" onclick="if(event.target===this)closeSavePreview();">
        <div class="modal-box" onclick="event.stopPropagation();">
            <h3>Preview before save</h3>
            <p class="similar-note">Review the product details below, then confirm to save.</p>
            <div class="modal-grid">
                <div><div class="k">Category</div><div class="v" id="pvCat"></div></div>
                <div><div class="k">Type</div><div class="v" id="pvType"></div></div>
                <div class="full"><div class="k">Product Name</div><div class="v" id="pvName"></div></div>
                <div><div class="k">Product ID</div><div class="v" id="pvPid"></div></div>
                <div><div class="k">HSN / SAC</div><div class="v" id="pvHsn"></div></div>
                <div><div class="k">Unit</div><div class="v" id="pvUnit"></div></div>
                <div><div class="k">Brand</div><div class="v" id="pvBrand"></div></div>
                <div><div class="k">Selling Rate</div><div class="v" id="pvSrate"></div></div>
                <div><div class="k">Purchase Rate</div><div class="v" id="pvPrate"></div></div>
                <div><div class="k">Stock Qty</div><div class="v" id="pvQty"></div></div>
                <div><div class="k">GST / Tax</div><div class="v" id="pvTax"></div></div>
                <div><div class="k">MOQ</div><div class="v" id="pvMoq"></div></div>
                <div><div class="k">Expiry</div><div class="v" id="pvExpiry"></div></div>
                <div class="full"><div class="k">Catalog Flags</div><div class="v" id="pvFlags"></div></div>
                <div class="full"><div class="k">OEM Reference URL</div><div class="v" id="pvOem"></div></div>
                <div class="full"><div class="k">Sale Note</div><div class="v" id="pvSaleNote"></div></div>
                <div class="full"><div class="k">Specification</div><div class="v" id="pvSpec"></div></div>
                <div class="full"><div class="k">Extra Specs</div><div class="v" id="pvExtra"></div></div>
            </div>
            <div id="pvFileNote" class="preview-file-note"></div>
            <div class="modal-close" style="display:flex;gap:8px;flex-wrap:wrap;">
                <button type="button" class="btn_style" onclick="return confirmSaveFromPreview();">Confirm &amp; Save</button>
                <button type="button" class="btn_style" onclick="return closeSavePreview();">Back to Edit</button>
            </div>
        </div>
    </div>
</asp:Content>
