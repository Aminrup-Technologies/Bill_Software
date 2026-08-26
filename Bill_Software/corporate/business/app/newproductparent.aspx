<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="newproductparent.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm68" %>
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
            display: grid; grid-template-columns: minmax(280px, 1fr) minmax(320px, 1.4fr);
            gap: 20px; align-items: start;
        }
        .box-panel {
            background: var(--bg-card); border: 1px solid var(--border-color);
            border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,.04); margin-bottom: 0;
        }
        .box-title {
            margin: 0 0 16px; padding-bottom: 10px; font-size: 15px; font-weight: 700;
            color: var(--primary-color); border-bottom: 3px solid var(--primary-color);
        }
        .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 12px; }
        .form-group { margin-bottom: 14px; }
        .form-group label { display: block; font-weight: 600; font-size: 12px; color: #334155; margin-bottom: 6px; text-transform: uppercase; letter-spacing: .02em; }
        .form-group .textbox_U_style, .form-group input[type=text] {
            width: 100%; max-width: 100%; box-sizing: border-box; padding: 8px 10px;
            border: 1px solid var(--border-color); border-radius: 4px;
        }
        .action-toolbar { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; margin-top: 8px; }
        .action-toolbar .btn_style { min-width: 110px; }
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .grid-toolbar { display: flex; justify-content: space-between; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
        #txtGridSearch {
            min-width: 220px; padding: 8px 10px; border: 1px solid var(--border-color);
            border-radius: 4px; font-size: 13px; box-sizing: border-box;
        }
        .alert-ok, .alert-err { padding: 10px 12px; border-radius: 4px; margin-bottom: 14px; }
        .category-table { width: 100%; border-collapse: collapse; font-size: 13px; }
        .category-table th, .category-header td {
            background: var(--primary-color); color: #fff; font-weight: 700;
            padding: 10px 8px; text-align: center; border: 1px solid #0f4a66;
            position: sticky; top: 0; z-index: 2;
        }
        .category-table td { padding: 10px 8px; border: 1px solid var(--border-color); text-align: center; vertical-align: middle; }
        .category-item-row { background: #fff; }
        .category-item-row:nth-child(even), .category-item-row.alt { background: #f8fafc; }
        .category-item-row:hover { background: #eef6fb; }
        .badge-id {
            display: inline-block; padding: 2px 8px; border-radius: 4px;
            background: #e2e8f0; color: #0f172a; font-weight: 600; font-size: 12px;
        }
        .client-msg { display: none; padding: 10px 12px; border-radius: 4px; margin-bottom: 14px;
            border: 1px solid #f5c6cb; background: #f8d7da; color: #721c24; }
        @media (max-width: 900px) {
            .master-layout-grid { grid-template-columns: 1fr; }
            .form-grid-2 { grid-template-columns: 1fr; }
        }
        .table1 { border-collapse: collapse; width: 100%; }
        .table1 td { text-align: left; border: 1px solid var(--border-color); }
        .table2 { border-collapse: collapse; width: 100%; }
        .table2 td { text-align: left; border: 1px solid var(--border-color); border-top: none; }
    </style>
    <script type="text/javascript">
        function validateCategoryForm() {
            var el = document.getElementById('<%= txtParentProducts.ClientID %>');
            var msg = document.getElementById('clientValidationMsg');
            var errPanel = document.getElementById('<%= PanelError.ClientID %>');
            var errLbl = document.getElementById('<%= lblErrorMsg.ClientID %>');
            if (!el) return false;
            var v = (el.value || '').replace(/^\s+|\s+$/g, '');
            el.value = v;
            if (!v) {
                var text = 'Provide Products Name.';
                if (msg) { msg.style.display = 'block'; msg.innerHTML = text; }
                if (errPanel) { errPanel.style.display = 'block'; }
                if (errLbl) { errLbl.innerHTML = text; }
                el.focus();
                return false;
            }
            if (msg) { msg.style.display = 'none'; msg.innerHTML = ''; }
            return true;
        }
        function preventDoubleSubmit() {
            var btn = document.getElementById('<%= btnSave.ClientID %>');
            if (btn) {
                btn.style.pointerEvents = 'none';
                btn.value = 'Saving...';
                setTimeout(function () { btn.disabled = true; }, 0);
            }
            return true;
        }
        function onCategorySaveClick() {
            if (!validateCategoryForm()) return false;
            return preventDoubleSubmit();
        }
        function ValidateField() { return onCategorySaveClick(); }
        function ValidateDelete1() {
            return confirm('Want to Delete this Products?');
        }
        function filterCategoryGrid() {
            var input = document.getElementById('txtGridSearch');
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
    <div class="page-header">
        <div class="hdr-icon">C</div>
        <div class="hdr-text">
            <div class="breadcrumb">Masters / Catalog</div>
            <h1>Manage Products — Category Master</h1>
        </div>
    </div>

    <div id="clientValidationMsg" class="client-msg"></div>

    <div class="master-layout-grid">
        <div class="box-panel">
            <div class="box-title">Category Form</div>

            <asp:Panel ID="PanelOK" runat="server" CssClass="alert-ok" BackColor="#EEFFDD"
                BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" style="display:none;">
                <asp:Image ID="imageTick" runat="server"
                    ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="PanelError" runat="server" CssClass="alert-err" BorderColor="#FF3300"
                BorderStyle="Solid" BorderWidth="1px" style="display:none;">
                <asp:Image ID="Image1" runat="server" Height="16px"
                    ImageUrl="~/corporate/business/WebImages/Cross_icon.png"
                    Width="16px" />
                &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
            </asp:Panel>

            <div class="form-grid-2">
                <div class="form-group" style="grid-column: 1 / -1;">
                    <label for="<%= txtParentProducts.ClientID %>">Product / Service Category</label>
                    <asp:TextBox ID="txtParentProducts" runat="server" CssClass="textbox_U_style" Width="100%"></asp:TextBox>
                </div>
            </div>

            <div class="action-toolbar">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return onCategorySaveClick();" OnClick="btnSave_Click" />
            </div>
        </div>

        <div class="box-panel">
            <div class="box-title">Category Dashboard</div>
            <div class="grid-toolbar">
                <span style="font-size:13px;color:#64748b;">Browse categories for this company</span>
                <input type="text" id="txtGridSearch" placeholder="Search category / ID..."
                    onkeyup="filterCategoryGrid();" oninput="filterCategoryGrid();" autocomplete="off" />
            </div>
            <div class="table-responsive">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#e2e8f0"
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px"
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%"
                    OnItemCommand="DataList1_ItemCommand" CssClass="category-table">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle CssClass="alt" />
                    <SeparatorStyle BorderColor="#e2e8f0" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#19658A" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1 category-table category-header" width="100%">
                            <tr>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:70%;">
                                    <asp:Label ID="showrm" runat="server" Text="Product Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="edit" runat="server" Text="Delete"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2 category-table category-item-row" width="100%">
                            <tr>
                                <td style="text-align:center; width:15%;">
                                    <span class="badge-id"><asp:Label ID="ID" runat="server" Text='<%# Eval("id") %>'></asp:Label></span>
                                </td>
                                <td style="text-align:center; width:70%;">
                                    <asp:Label ID="Label2" runat="server" Text='<%# Eval("ProductOrServiceCat") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Delete" CommandArgument='<%# Eval("id") %>'
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
