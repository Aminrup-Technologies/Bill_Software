<%@ Page Title="Create Requisition" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="RequisitionNew.aspx.cs" Inherits="Bill_Software.corporate.business.app.RequisitionNew" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .style3 { color: #FF3300; }
        .Grid td { text-align: center; font-size: 11px; line-height: 200%; border: 1px solid #2D2D2D; padding: 4px; }
        .textbox_style21 { text-align: center; }
        .field-error { border: 2px solid #d9534f !important; background-color: #fff0f0; }
        .delete-link { color: #d9534f; font-weight: bold; cursor: pointer; text-decoration: none; }
        .delete-link:hover { color: #a94442; text-decoration: underline; }
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
        .wizard-container { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .wizard-steps { display: flex; border-bottom: 2px solid #ddd; margin-bottom: 20px; }
        .wizard-step { flex: 1; text-align: center; padding: 15px; font-weight: bold; color: #999; }
        .wizard-step.active { color: #19658A; border-bottom: 4px solid #19658A; }
        .wizard-footer { margin-top: 20px; text-align: right; padding-top: 15px; border-top: 1px solid #eee; }
        .form-grid-aligned {
            display: grid;
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 12px;
            margin-bottom: 12px;
            align-items: end;
        }
        .form-grid-aligned .form-group label {
            display: block;
            font-weight: 600;
            font-size: 11px;
            margin-bottom: 4px;
        }
        .form-grid-aligned .form-group .req { color: #c00; }
        .form-grid-aligned .dropdown_style,
        .form-grid-aligned .textbox_style {
            width: 100% !important;
            box-sizing: border-box;
        }
        @media (max-width: 900px) {
            .form-grid-aligned { grid-template-columns: repeat(2, minmax(0, 1fr)); }
        }
        .btn-viewmore {
            border: 1px solid #cbd5e1; background: #fff; color: #19658A;
            padding: 2px 8px; border-radius: 3px; font-size: 11px; cursor: pointer;
        }
        .btn-viewmore:hover { background: #e8f4fa; }
        a.pid-link { color: #19658A; font-weight: 700; text-decoration: underline; cursor: pointer; }
        .modal-backdrop {
            display: none; position: fixed; z-index: 9999; left: 0; top: 0; right: 0; bottom: 0;
            background: rgba(15, 23, 42, .45); align-items: center; justify-content: center; padding: 12px;
        }
        .modal-backdrop.is-open { display: flex; }
        .modal-box.product-detail-modal {
            background: #fff; border-radius: 8px; width: min(920px, 96vw); max-height: 92vh;
            overflow: hidden; padding: 0; border: 1px solid #d7e2ec; display: flex; flex-direction: column;
            box-shadow: 0 12px 40px rgba(0,0,0,.2);
        }
        .pd-header {
            display: flex; align-items: center; justify-content: space-between; gap: 20px;
            padding: 14px 16px; background: linear-gradient(135deg, #19658A 0%, #0f4a66 100%);
            border-radius: 8px 8px 0 0; color: #fff; flex-shrink: 0;
        }
        .pd-header-left { flex: 1; min-width: 0; }
        .pd-eyebrow { display: block; font-size: 10px; text-transform: uppercase; letter-spacing: .05em; opacity: .8; margin-bottom: 2px; }
        .pd-name { margin: 0; color: #fff; font-size: 18px; font-weight: 800; line-height: 1.2; word-break: break-word; }
        .pd-header-right {
            flex: 0 0 auto; background: rgba(255,255,255,.12); border: 1px solid rgba(255,255,255,.22);
            border-radius: 6px; padding: 6px 12px;
        }
        .pd-meta-k { font-size: 9px; text-transform: uppercase; letter-spacing: .04em; opacity: .8; }
        .pd-meta-v { font-size: 13px; font-weight: 700; }
        .pd-body { padding: 12px 14px 10px; overflow-y: auto; flex: 1 1 auto; min-height: 0; }
        .pd-top-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 12px; }
        .modal-section { margin: 0 0 12px; padding-bottom: 10px; border-bottom: 1px solid #e8edf3; }
        .modal-section:last-of-type { border-bottom: 0; margin-bottom: 0; }
        .modal-section-title { font-size: 11px; font-weight: 700; color: #19658A; margin-bottom: 8px; text-transform: uppercase; }
        .modal-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 8px; font-size: 12px; }
        .modal-grid .span2 { grid-column: span 2; }
        .pd-field { background: #f8fafc; border: 1px solid #e8edf3; border-radius: 6px; padding: 7px 9px; }
        .modal-grid .k { font-size: 9px; line-height: 1.1; margin-bottom: 3px; color: #64748b; font-weight: 700; text-transform: uppercase; }
        .modal-grid .v { font-size: 12px; line-height: 1.3; font-weight: 600; color: #0f172a; word-break: break-word; }
        .product-detail-modal .modal-close {
            margin: 0; padding: 10px 14px; border-top: 1px solid #e8edf3; text-align: right; flex-shrink: 0;
        }
        .view-gallery { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; }
        .view-gallery-item { border: 1px solid #e8edf3; border-radius: 6px; padding: 6px; text-align: center; background: #f8fafc; min-height: 90px; }
        .view-gallery-item img { max-width: 100%; max-height: 70px; cursor: pointer; }
        .view-label { display: block; font-size: 10px; color: #64748b; margin-bottom: 4px; }
        .view-empty { display: flex; align-items: center; justify-content: center; height: 70px; color: #94a3b8; font-size: 11px; }
        .img-lightbox {
            display: none; position: fixed; z-index: 10000; left: 0; top: 0; right: 0; bottom: 0;
            background: rgba(0,0,0,.8); align-items: center; justify-content: center; flex-direction: column;
        }
        .img-lightbox.is-open { display: flex; }
        .img-lightbox img { max-width: 90vw; max-height: 80vh; }
        .img-lightbox-hint { color: #fff; margin-top: 10px; font-size: 12px; }
        @media (max-width: 700px) {
            .pd-top-row, .modal-grid, .view-gallery { grid-template-columns: 1fr 1fr; }
        }
    </style>

    <script type="text/javascript">
        function pageLoad() {
            $('.select2-enable').select2({ width: '100%' });
        }

        function searchProductGrid() {
            var filter = document.getElementById('<%= txtProductSearch.ClientID %>').value.toLowerCase();
            var grid = document.getElementById('<%= gvProductsToSelect.ClientID %>');
            if (!grid) return;
            var rows = grid.getElementsByTagName("tr");
            for (var i = 1; i < rows.length; i++) {
                var text = rows[i].innerText.toLowerCase();
                rows[i].style.display = (filter === "" || text.indexOf(filter) > -1) ? "" : "none";
            }
        }

        function toggleAllProducts(source) {
            var checkboxes = document.querySelectorAll('.product-checkbox input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) {
                var tr = checkboxes[i].closest('tr');
                if (tr.style.display !== "none") checkboxes[i].checked = source.checked;
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
            if (!grid) return;
            var rows = grid.getElementsByTagName("tr");
            var matchCount = 0;
            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];
                if (filter === "" || getRowSearchText(row).indexOf(filter) > -1) {
                    row.style.display = "";
                    if (filter !== "") matchCount++;
                } else {
                    row.style.display = "none";
                }
            }
            var lblNoRec = document.getElementById("lblNoServiceRecords");
            if (lblNoRec) lblNoRec.style.display = (filter !== "" && matchCount === 0) ? "block" : "none";
        }

        function clearServiceGridSearch() {
            var input = document.getElementById('<%= txtServiceSearch.ClientID %>');
            input.value = "";
            searchServiceGrid();
        }

        let modifiedCount = 0;
        function markRowModified(ctrl) {
            const row = ctrl.closest("tr");
            if (!row) return;
            const hidden = row.querySelector("input[id*='hdnIsModified']");
            if (!hidden || hidden.value === "1") return;
            hidden.value = "1";
            modifiedCount++;
            const badge = row.querySelector(".modified-badge");
            if (badge) badge.style.display = "inline";
            row.style.backgroundColor = "#FFF3CD";
            const lbl = document.getElementById("lblModifiedCount");
            if (lbl) lbl.innerText = modifiedCount;
        }

        function showModifiedOnly() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            const rows = grid.getElementsByTagName("tr");
            let found = false;
            for (let i = 1; i < rows.length; i++) {
                const hdn = rows[i].querySelector("input[id*='hdnIsModified']");
                if (hdn && hdn.value === "1") { rows[i].style.display = ""; found = true; }
                else rows[i].style.display = "none";
            }
            const lbl = document.getElementById("lblNoServiceRecords");
            if (lbl) lbl.style.display = found ? "none" : "inline";
        }

        function showAllRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            const rows = grid.getElementsByTagName("tr");
            for (let i = 1; i < rows.length; i++) rows[i].style.display = "";
            const lbl = document.getElementById("lblNoServiceRecords");
            if (lbl) lbl.style.display = "none";
        }

        function calculateDiscount(changedInput) {
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            var rows = grid.getElementsByTagName("tr");
            for (var i = 1; i < rows.length; i++) {
                if (!rows[i].contains(changedInput)) continue;
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
                if (taxableAmountInput) taxableAmountInput.value = (total - amount).toFixed(2);
                markRowModified(changedInput);
                recalcSummary();
                break;
            }
        }

        function recalcSummary() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return;
            const rows = grid.getElementsByTagName("tr");
            let totalTaxable = 0, totalGST = 0, netTotal = 0;
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                if (row.style.display === "none") continue;
                const taxableBox = row.querySelector("[id*='TaxableAmount']");
                const chkTax = row.querySelector("input[id*='chkTaxApplicable']");
                const gstDDL = row.querySelector("[id*='vat_parsentage']");
                let taxable = taxableBox && taxableBox.value ? parseFloat(taxableBox.value) : 0;
                let gstPercent = 0;
                if (chkTax && chkTax.checked && gstDDL && gstDDL.value !== "NA")
                    gstPercent = parseFloat(gstDDL.value) || 0;
                let gstAmount = taxable * gstPercent / 100;
                totalTaxable += taxable;
                totalGST += gstAmount;
                netTotal += (taxable + gstAmount);
            }
            document.getElementById("lblTotalTaxable").innerText = totalTaxable.toFixed(2);
            document.getElementById("lblTotalGST").innerText = totalGST.toFixed(2);
            document.getElementById("lblNetTotal").innerText = netTotal.toFixed(2);
        }

        function validateModifiedRows() {
            const grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return true;
            const rows = grid.getElementsByTagName("tr");
            let hasError = false, firstErrorRow = null;
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const hdn = row.querySelector("input[id*='hdnIsModified']");
                if (!hdn || hdn.value !== "1") continue;
                const qty = row.querySelector("[id*='Quantity']");
                const rate = row.querySelector("[id*='Vendor_rate']");
                [qty, rate].forEach(c => { if (c) c.classList.remove("field-error"); });
                let rowHasError = false;
                if (!qty || qty.value.trim() === "" || Number(qty.value) <= 0) {
                    if (qty) qty.classList.add("field-error"); rowHasError = true;
                }
                if (rate && rate.value.trim() !== "" && Number(rate.value) < 0) {
                    rate.classList.add("field-error"); rowHasError = true;
                }
                if (rowHasError) { hasError = true; if (!firstErrorRow) firstErrorRow = row; }
            }
            if (hasError) {
                if (firstErrorRow) firstErrorRow.scrollIntoView({ behavior: "smooth", block: "center" });
                alert("Please correct highlighted fields in modified rows.");
                return false;
            }
            return true;
        }

        function validatePRGrid() {
            var grid = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!grid) return true;
            var rows = grid.rows;
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var rate = row.querySelector('input.rate-input') || row.querySelector("input[id*='Vendor_rate']");
                if (!rate) continue;
                var tax = row.querySelector("input[id*='chkTaxApplicable']")
                    || row.querySelector('input.tax-check')
                    || row.querySelector('.tax-check input[type="checkbox"]');
                var gst = row.querySelector('select.gst-select')
                    || row.querySelector("select[id*='vat_parsentage']");
                var v = parseFloat(rate.value);
                if (isNaN(v) || v <= 0) {
                    row.style.backgroundColor = 'red';
                    alert('Rate must be greater than zero.');
                    return false;
                }
                if (!tax || !tax.checked) {
                    row.style.backgroundColor = 'red';
                    alert('Tax Applicable must be checked.');
                    return false;
                }
                var g = gst && gst.value != null ? String(gst.value).replace(/^\s+|\s+$/g, '') : '';
                var gNum = parseFloat(g);
                if (!g || g === 'NA' || g === '0' || g === '0.00' || isNaN(gNum) || gNum <= 0) {
                    row.style.backgroundColor = 'red';
                    alert('Please select a GST percentage.');
                    return false;
                }
                row.style.backgroundColor = '';
            }
            return true;
        }

        function setPdTxt(id, val) {
            var el = document.getElementById(id);
            if (el) el.textContent = (val && String(val).replace(/^\s+|\s+$/g, '')) ? val : '—';
        }
        function setPdView(imgId, emptyId, url, label) {
            var img = document.getElementById(imgId);
            var empty = document.getElementById(emptyId);
            var has = !!(url && String(url).replace(/^\s+|\s+$/g, ''));
            if (img) {
                img.onclick = null;
                if (has) {
                    img.src = url;
                    img.style.display = 'inline-block';
                    img.onclick = function () { return openImageLightbox(url, label); };
                } else {
                    img.removeAttribute('src');
                    img.style.display = 'none';
                }
            }
            if (empty) empty.style.display = has ? 'none' : 'flex';
        }
        function fillProductModal(d) {
            setPdTxt('mdHdrName', d.name); setPdTxt('mdHdrPid', d.pid);
            setPdTxt('mdHsn', d.hsn); setPdTxt('mdCat', d.cat); setPdTxt('mdType', d.type);
            setPdTxt('mdBrand', d.brand); setPdTxt('mdUnit', d.unit);
            setPdTxt('mdSrate', d.srate); setPdTxt('mdPrate', d.prate); setPdTxt('mdTax', d.tax);
            setPdTxt('mdQty', d.qty); setPdTxt('mdMoq', d.moq); setPdTxt('mdExpiry', d.expiry);
            setPdTxt('mdSaleNote', d.salenote); setPdTxt('mdRemarks', d.remarks); setPdTxt('mdSpec', d.spec);
            var oemEl = document.getElementById('mdOem');
            if (oemEl) {
                oemEl.innerHTML = '';
                if (d.oem) {
                    var a = document.createElement('a');
                    a.href = d.oem; a.target = '_blank'; a.rel = 'noopener noreferrer'; a.textContent = d.oem;
                    oemEl.appendChild(a);
                } else oemEl.textContent = '—';
            }
            setPdView('mdImgTop', 'mdEmptyTop', d.imgtop, 'Top View');
            setPdView('mdImgBottom', 'mdEmptyBottom', d.imgbottom, 'Bottom View');
            setPdView('mdImgLeft', 'mdEmptyLeft', d.imgleft, 'Left View');
            setPdView('mdImgRight', 'mdEmptyRight', d.imgright, 'Right View');
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop is-open';
        }
        function showProductModal(btn) {
            if (!btn) return false;
            function g(k) { return (btn.dataset && btn.dataset[k]) || btn.getAttribute('data-' + k) || ''; }
            fillProductModal({
                name: g('name'), pid: g('pid'), hsn: g('hsn'), cat: g('cat'), type: g('type'),
                brand: g('brand'), unit: g('unit'), srate: g('srate'), prate: g('prate'), tax: g('tax'),
                qty: g('qty'), moq: g('moq'), expiry: g('expiry'), salenote: g('salenote'),
                remarks: g('remarks'), spec: g('spec'), oem: g('oem'),
                imgtop: g('imgtop'), imgbottom: g('imgbottom'), imgleft: g('imgleft'), imgright: g('imgright')
            });
            return false;
        }
        function openProductDetailById(productId) {
            if (!productId) return false;
            PageMethods.GetProductDetail(productId, function (r) {
                if (!r || !r.ok) { alert((r && r.message) || 'Product details not found.'); return; }
                fillProductModal(r);
            }, function () { alert('Unable to load product details.'); });
            return false;
        }
        function closeProductModal() {
            closeImageLightbox();
            var backdrop = document.getElementById('productModal');
            if (backdrop) backdrop.className = 'modal-backdrop';
            return false;
        }
        function openImageLightbox(url, label) {
            if (!url) return false;
            var box = document.getElementById('imgLightbox');
            var img = document.getElementById('imgLightboxSrc');
            var hint = document.getElementById('imgLightboxHint');
            if (img) { img.src = url; img.alt = label || 'Product view'; }
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
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" EnablePageMethods="true"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="style1">
                <tr><td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">Create Purchase Requisition</span>&nbsp;</td></tr>
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
                <div class="form-grid-aligned">
                    <div class="form-group">
                        <label><span class="req">*</span> Vendor</label>
                        <asp:DropDownList ID="cmbvendor" runat="server" AutoPostBack="True" CssClass="dropdown_style select2-enable" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Label ID="lblvendor_id" runat="server" Visible="false"></asp:Label>
                        <asp:Label ID="lbl_vendordbid" runat="server" Visible="false"></asp:Label>
                    </div>
                    <div class="form-group">
                        <label>Date</label>
                        <asp:TextBox ID="txtPRDate" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>External ERP PR No</label>
                        <asp:TextBox ID="txtExternalPRNo" runat="server" CssClass="textbox_style" placeholder="e.g. ERP-987654"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Remarks</label>
                        <asp:TextBox ID="txtRemarks" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="2" placeholder="Add custom comments here..."></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>City</label>
                        <asp:TextBox ID="cmbcity" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Address 1</label>
                        <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>State</label>
                        <asp:TextBox ID="cmbState" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Phone No</label>
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Email ID</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
                    </div>
                </div>

                <div class="wizard-steps">
                    <asp:Label ID="lblTabSelect" runat="server" CssClass="wizard-step active" Text="1. Select Items" />
                    <asp:Label ID="lblTabCart" runat="server" CssClass="wizard-step" Text="2. Review / Edit Cart" />
                </div>

                <asp:MultiView ID="mvRequisition" runat="server" ActiveViewIndex="0">
                    <asp:View ID="vwSelectItems" runat="server">
                        <asp:Panel ID="pnlSelectItems" runat="server">
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

                            <div style="margin-top: 15px;">
                                <asp:TextBox ID="txtProductSearch" runat="server" CssClass="textbox_U_style" Width="300px" placeholder="Search products in this category..." onkeyup="searchProductGrid()" />
                                <div style="max-height: 250px; overflow-y: auto; margin-top: 10px; border: 1px solid #ccc;">
                                    <asp:GridView ID="gvProductsToSelect" runat="server" AutoGenerateColumns="False" CssClass="Grid" Width="100%" DataKeyNames="ItemId" EmptyDataText="No products found in this category.">
                                        <HeaderStyle BackColor="#19658A" Font-Bold="True" ForeColor="White" />
                                        <Columns>
                                            <asp:TemplateField>
                                                <HeaderTemplate>
                                                    <input type="checkbox" id="chkAll" onclick="toggleAllProducts(this)" title="Select All Visible" />
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chkSelect" runat="server" CssClass="product-checkbox" />
                                                </ItemTemplate>
                                                <ItemStyle Width="40px" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Item Code" ItemStyle-Width="110px">
                                                <ItemTemplate>
                                                    <a href="javascript:void(0);" class="pid-link"
                                                       onclick='<%# Convert.ToString(Eval("IsProduct")) == "1"
                                                           ? "return openProductDetailById(\"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("ItemId"))) + "\");"
                                                           : "return false;" %>'><%# Eval("ItemId") %></a>
                                                    <asp:Label ID="lblItemId" runat="server" Text='<%# Eval("ItemId") %>' style="display:none;"></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="HSN / SAC" ItemStyle-Width="90px">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblHsn" runat="server" Text='<%# Eval("HSN") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Item Name" ItemStyle-HorizontalAlign="Left">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblItemName" runat="server" Text='<%# Eval("ItemName") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Details" ItemStyle-Width="90px">
                                                <ItemTemplate>
                                                    <%# Convert.ToString(Eval("IsProduct")) == "1"
                                                        ? "<button type=\"button\" class=\"btn-viewmore\" "
                                                          + "data-pid=\"" + Server.HtmlEncode(Convert.ToString(Eval("ItemId"))) + "\" "
                                                          + "data-name=\"" + Server.HtmlEncode(Convert.ToString(Eval("ItemName"))) + "\" "
                                                          + "data-hsn=\"" + Server.HtmlEncode(Convert.ToString(Eval("HSN"))) + "\" "
                                                          + "data-cat=\"" + Server.HtmlEncode(Convert.ToString(Eval("Category"))) + "\" "
                                                          + "data-type=\"" + Server.HtmlEncode(Convert.ToString(Eval("Type"))) + "\" "
                                                          + "data-brand=\"" + Server.HtmlEncode(Convert.ToString(Eval("Brand"))) + "\" "
                                                          + "data-unit=\"" + Server.HtmlEncode(Convert.ToString(Eval("Unit"))) + "\" "
                                                          + "data-srate=\"" + Server.HtmlEncode(Convert.ToString(Eval("Sail_Rate"))) + "\" "
                                                          + "data-prate=\"" + Server.HtmlEncode(Convert.ToString(Eval("Purches_Rate"))) + "\" "
                                                          + "data-tax=\"" + Server.HtmlEncode(Convert.ToString(Eval("Tax_Rate"))) + "\" "
                                                          + "data-qty=\"" + Server.HtmlEncode(Convert.ToString(Eval("Quantity"))) + "\" "
                                                          + "data-moq=\"" + Server.HtmlEncode(Convert.ToString(Eval("MOQ_Value"))) + "\" "
                                                          + "data-expiry=\"" + Server.HtmlEncode(Convert.ToString(Eval("ExpiryText"))) + "\" "
                                                          + "data-salenote=\"" + Server.HtmlEncode(Convert.ToString(Eval("SaleNote"))) + "\" "
                                                          + "data-remarks=\"" + Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) + "\" "
                                                          + "data-spec=\"" + Server.HtmlEncode(Convert.ToString(Eval("Specification"))) + "\" "
                                                          + "data-oem=\"" + Server.HtmlEncode(Convert.ToString(Eval("OemUrl"))) + "\" "
                                                          + "data-imgtop=\"" + Server.HtmlEncode(Convert.ToString(Eval("ImgTop"))) + "\" "
                                                          + "data-imgbottom=\"" + Server.HtmlEncode(Convert.ToString(Eval("ImgBottom"))) + "\" "
                                                          + "data-imgleft=\"" + Server.HtmlEncode(Convert.ToString(Eval("ImgLeft"))) + "\" "
                                                          + "data-imgright=\"" + Server.HtmlEncode(Convert.ToString(Eval("ImgRight"))) + "\" "
                                                          + "onclick=\"return showProductModal(this);\">View</button>"
                                                        : "—" %>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </div>

                            <div style="text-align: right; padding-top: 15px;">
                                <asp:Button ID="Button2" runat="server" Text="+ Add Selected Items to Grid" CssClass="btn_style" BackColor="#19658A" ForeColor="White" OnClick="Button2_Click" />
                            </div>

                            <div class="wizard-footer">
                                <asp:Button ID="btnGoCart" runat="server" Text="Next: Review & Edit Cart &raquo;" CssClass="btn_style" OnClick="btnGoCart_Click" />
                            </div>
                        </asp:Panel>
                    </asp:View>

                    <asp:View ID="vwReviewCart" runat="server">
                        <asp:Panel ID="pnlReviewCart" runat="server">
                            <div style="text-align: center; margin-bottom: 15px;">
                                <asp:TextBox ID="txtServiceSearch" runat="server" CssClass="textbox_U_style" Width="260px" placeholder="Search selected items..." onkeyup="debouncedSearchServiceGrid()" />
                                <asp:Button ID="btnClearServiceSearch" runat="server" Text="Clear" CssClass="btn_style" OnClientClick="clearServiceGridSearch(); return false;" />
                            </div>

                            <div style="text-align: right; margin-bottom: 10px;">
                                <span style="font-weight: bold; color: #d9534f;">Modified Items: <span id="lblModifiedCount">0</span></span>
                                <asp:Button ID="btnShowModified" runat="server" Text="Show Modified" CssClass="btn btn-warning btn-sm btn_style" OnClientClick="showModifiedOnly(); return false;" />
                                <asp:Button ID="btnShowAll" runat="server" Text="Show All" CssClass="btn btn-secondary btn-sm btn_style" OnClientClick="showAllRows(); return false;" />
                            </div>

                            <div style="overflow-x: auto;">
                                <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" Width="100%" OnRowDataBound="gd_Service_Product_RowDataBound" OnRowCommand="gd_Service_Product_RowCommand">
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Status">
                                            <ItemTemplate><span class="modified-badge" style="display: none; color:red;">✱</span></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Code">
                                            <ItemTemplate>
                                                <%# Convert.ToString(Eval("IsProduct")) == "1"
                                                    ? "<a href=\"javascript:void(0);\" class=\"pid-link\" onclick=\"return openProductDetailById('"
                                                      + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("Ser_pro_code")))
                                                      + "');\">" + Server.HtmlEncode(Convert.ToString(Eval("Ser_pro_code"))) + "</a>"
                                                    : Server.HtmlEncode(Convert.ToString(Eval("Ser_pro_code"))) %>
                                                <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Eval("Ser_pro_code") %>' style="display:none;"></asp:Label>
                                                <asp:HiddenField ID="hdnIsModified" runat="server" Value="0" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="HSN">
                                            <ItemTemplate>
                                                <asp:Label ID="lblCartHsn" runat="server" Text='<%# Eval("HSN") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Name">
                                            <ItemTemplate><asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Eval("Ser_pro_Name") %>'></asp:Label></ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Specification">
                                            <ItemTemplate>
                                                <asp:TextBox ID="sepecification" runat="server" Text='<%# Eval("Description") %>' onkeyup="markRowModified(this)" CssClass="textbox_style21" Width="150px"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Qty">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Quantity" runat="server" Text='<%# Eval("Qnty") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21" Width="60px"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Rate">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Vendor_rate" runat="server" Text='<%# Eval("Rate") %>' onkeyup="calculateDiscount(this)" CssClass="textbox_style21 rate-input" Width="80px"></asp:TextBox>
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
                                                <asp:TextBox ID="TaxableAmount" runat="server" ReadOnly="true" CssClass="textbox_style21" Width="80px"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Tax Applic.">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkTaxApplicable" runat="server" Text="Yes" Checked='<%# Convert.ToBoolean(Eval("IsTaxApplicable")) %>' onchange="markRowModified(this); recalcSummary();" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="GST %">
                                            <ItemTemplate>
                                                <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="dropdown_style gst-select" onchange="markRowModified(this); recalcSummary();"></asp:DropDownList>
                                                <asp:HiddenField ID="hdnSelectedGST" runat="server" Value='<%# Eval("GST") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Order">
                                            <ItemTemplate>
                                                <asp:TextBox ID="txtOrder" runat="server" Text='<%# Eval("ItemOrder") %>' onkeyup="markRowModified(this)" CssClass="textbox_style21" Width="50px" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Action">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lnkDelete" runat="server" Text="Remove" CommandName="DeleteItem" CommandArgument='<%# Eval("Ser_pro_code") %>' OnClientClick="return confirm('Remove this item from the cart?');" CssClass="delete-link" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <div style="text-align: right; margin-top: 15px; font-size: 14px; font-weight: bold;">
                                <span style="margin-right: 20px;">Total Taxable: <span id="lblTotalTaxable">0.00</span></span>
                                <span style="margin-right: 20px;">Total GST: <span id="lblTotalGST">0.00</span></span>
                                <span>Net Total: <span id="lblNetTotal">0.00</span></span>
                            </div>

                            <div class="wizard-footer">
                                <asp:Button ID="btnGoSelect" runat="server" Text="&laquo; Back to Select Items" CssClass="btn_style" style="float:left;" OnClick="btnGoSelect_Click" />
                                <asp:Button ID="btnSaveDraft" runat="server" Text="Save Draft" CssClass="btn_style" BackColor="#f0ad4e" ForeColor="White" OnClientClick="return validateModifiedRows();" OnClick="btnSaveDraft_Click" />
                                &nbsp;
                                <asp:Button ID="Button3" runat="server" Text="Submit PR" CssClass="btn_style" BackColor="#5cb85c" ForeColor="White" OnClientClick="return validateModifiedRows() && validatePRGrid();" OnClick="Button3_Click" />
                                &nbsp;
                                <asp:Button ID="btnCancelPR" runat="server" Text="Cancel PR" CssClass="btn_style" BackColor="#d9534f" ForeColor="White" OnClick="btnCancelPR_Click" />
                            </div>
                        </asp:Panel>
                    </asp:View>
                </asp:MultiView>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

            <div id="productModal" class="modal-backdrop" onclick="if(event.target===this)closeProductModal();">
                <div class="modal-box product-detail-modal" onclick="event.stopPropagation();">
                    <div class="pd-header">
                        <div class="pd-header-left">
                            <span class="pd-eyebrow">Product Detail</span>
                            <div class="pd-name" id="mdHdrName">—</div>
                        </div>
                        <div class="pd-header-right">
                            <div class="pd-meta-k">Product ID</div>
                            <div class="pd-meta-v" id="mdHdrPid">—</div>
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
                                <div class="pd-field span2"><div class="k">OEM Reference URL</div><div class="v" id="mdOem"></div></div>
                            </div>
                        </div>
                        <div class="modal-section">
                            <div class="modal-section-title">4. Product Views</div>
                            <div class="view-gallery">
                                <div class="view-gallery-item">
                                    <span class="view-label">Top View</span>
                                    <img id="mdImgTop" alt="Top View" style="display:none;" />
                                    <div class="view-empty" id="mdEmptyTop">No image</div>
                                </div>
                                <div class="view-gallery-item">
                                    <span class="view-label">Bottom View</span>
                                    <img id="mdImgBottom" alt="Bottom View" style="display:none;" />
                                    <div class="view-empty" id="mdEmptyBottom">No image</div>
                                </div>
                                <div class="view-gallery-item">
                                    <span class="view-label">Left View</span>
                                    <img id="mdImgLeft" alt="Left View" style="display:none;" />
                                    <div class="view-empty" id="mdEmptyLeft">No image</div>
                                </div>
                                <div class="view-gallery-item">
                                    <span class="view-label">Right View</span>
                                    <img id="mdImgRight" alt="Right View" style="display:none;" />
                                    <div class="view-empty" id="mdEmptyRight">No image</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-close">
                        <button type="button" class="btn_style" onclick="return closeProductModal();">Close</button>
                    </div>
                </div>
            </div>
            <div id="imgLightbox" class="img-lightbox" onclick="return closeImageLightbox();">
                <img id="imgLightboxSrc" alt="" />
                <div id="imgLightboxHint" class="img-lightbox-hint">Click anywhere to close</div>
            </div>
</asp:Content>
