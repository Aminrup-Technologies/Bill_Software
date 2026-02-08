<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master"
    AutoEventWireup="true" CodeBehind="Product_stock.aspx.cs"
    Inherits="Bill_Software.corporate.business.app.WebForm50" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #fff;
            font-weight: bold;
        }

        .search-container {
            display: flex;
            gap: 8px;
            background: #F4F8FB;
            padding: 10px;
            border: 1px solid #B5C7D3;
            border-radius: 4px;
        }

        .search-input {
            flex: 1;
            padding: 8px 10px;
            border: 1px solid #9FB3C8;
            border-radius: 3px;
        }

        .search-btn, .clear-btn {
            padding: 8px 14px;
            font-size: 12px;
            font-weight: bold;
            border: none;
            color: #fff;
            border-radius: 3px;
            cursor: pointer;
        }

        .search-btn {
            background: #19658A;
        }

        .clear-btn {
            background: #777;
        }

        .section-title {
            font-size: 12px;
            font-weight: bold;
            margin-bottom: 6px;
        }

        .search-result {
            padding: 8px;
            border-bottom: 1px solid #ddd;
            cursor: pointer;
        }

            .search-result:hover {
                background: #E8F1FF;
            }

            .search-result.active {
                background: #d9ecff;
                border-left: 4px solid #19658A;
            }

        .category {
            background: #006699;
            color: #fff;
            font-weight: bold;
            padding: 6px;
        }

        .table2 {
            width: 100%;
            border-collapse: collapse;
        }

            .table2 td {
                border: 1px solid #999;
                padding: 6px;
            }

        .summary-box {
            background: #FFFCE8;
            padding: 6px;
            font-size: 12px;
            font-weight: bold;
            margin-bottom: 6px;
            border: 1px solid #e0d7a6;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">
                <span class="style2">Product Stock</span>
            </td>
        </tr>

        <tr>
            <td colspan="4">&nbsp;</td>
        </tr>

        <!-- SEARCH -->
        <tr>
            <td></td>
            <td colspan="2">
                <div style="font-size: 11px; color: #666; margin-bottom: 4px;">
                    Search by Product Code or Product Name
                </div>
                <div class="search-container">
                    <input type="text" id="txtSearch" class="search-input"
                        placeholder="🔍 Search product name..." />
                    <button type="button" class="search-btn" onclick="triggerSearch()">Search</button>
                    <button type="button" class="clear-btn" onclick="clearSearch()">Clear</button>
                </div>
            </td>
            <td></td>
        </tr>

        <tr>
            <td colspan="4">&nbsp;</td>
        </tr>

        <!-- SPLIT VIEW -->
        <tr>
            <td></td>
            <td colspan="2">

                <div style="display: flex; gap: 16px; align-items: flex-start;">

                    <!-- LEFT: PRODUCTS -->
                    <div style="width: 45%; max-height: 420px; overflow-y: auto;">
                        <div class="section-title">Matching Products</div>
                        <div id="searchResults"></div>
                    </div>

                    <!-- RIGHT: STOCK -->
                    <div style="width: 55%; position: sticky; top: 10px;">
                        <div id="stockDetails">
                            <div style="color: #777; font-size: 12px;">
                                Select a product to view stock
                            </div>
                        </div>
                    </div>

                </div>

            </td>
            <td></td>
        </tr>
    </table>

    <script>
        let searchTimer = null;
        let selectedIndex = -1;
        let products = [];
        let currentStock = [];

        const SEARCH_DELAY = 600;

        // ================= SEARCH =================
        document.getElementById("txtSearch").addEventListener("keyup", function (e) {
            clearTimeout(searchTimer);

            if (["ArrowUp", "ArrowDown", "Enter"].includes(e.key)) {
                handleKeyboard(e.key);
                return;
            }

            const val = this.value.trim();
            searchTimer = setTimeout(() => {
                if (val.length === 0) clearUI();
                else fetchSearch(val);
            }, SEARCH_DELAY);
        });

        function triggerSearch() {
            const val = txtSearch.value.trim();
            if (val) fetchSearch(val);
        }

        function clearSearch() {
            txtSearch.value = "";
            clearUI();
        }

        function clearUI() {
            searchResults.innerHTML = "";
            stockDetails.innerHTML = "<div style='color:#777;'>Select a product to view stock</div>";
            products = [];
            selectedIndex = -1;
        }

        // ================= SEARCH RESULTS =================
        function fetchSearch(search) {
            fetch("Product_stock.aspx/SearchProducts", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ search })
            })
            .then(r=>r.json())
            .then(d=> {
                products = d.d || [];
                selectedIndex = -1;

                let html = products.map((p, i) =>`
            <div class="search-result" data-index="${i}"
                 onclick="selectProduct(${i})">
                <b>${p.ProductName}</b><br/>
                <span style="font-size:11px;color:#666">${p.CategoryName}</span>
            </div>`).join("");

                searchResults.innerHTML = html || "<div>No products found</div>";
            });
        }

        // ================= KEYBOARD NAV =================
        function handleKeyboard(key) {
            if (products.length === 0) return;

            if (key === "ArrowDown") selectedIndex = Math.min(selectedIndex + 1, products.length - 1);
            if (key === "ArrowUp") selectedIndex = Math.max(selectedIndex - 1, 0);
            if (key === "Enter" && selectedIndex >= 0) {
                selectProduct(selectedIndex);
                return;
            }

            highlightSelection();
        }

        function highlightSelection() {
            document.querySelectorAll(".search-result").forEach(e=>e.classList.remove("active"));
            const el = document.querySelector(`.search-result[data-index="${selectedIndex}"]`);
            if (el) {
                el.classList.add("active");
                el.scrollIntoView({ block: "nearest" });
            }
        }

        // ================= STOCK =================
        function selectProduct(index) {
            selectedIndex = index;
            highlightSelection();

            const p = products[index];

            fetch("Product_stock.aspx/GetStock", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId: p.ProductID })
            })
            .then(r=>r.json())
            .then(d=> {
                currentStock = d.d || [];
                renderStock(p.ProductName);
            });
        }

        // ================= RENDER STOCK =================
        function renderStock(productName) {
            let total = currentStock.reduce((s, x) =>s + parseFloat(x.StockQty || 0), 0);

            let html = `
    <div class="category">Stock Availability – ${productName}</div>

    <div class="summary-box">
        Total Stock (All Stores): ${total.toFixed(2)}
    </div>

    <div style="margin-bottom:6px;">
        <select id="storeFilter" onchange="applyStoreFilter()">
            <option value="">All Stores</option>
            ${[...new Set(currentStock.map(x=>x.StoreName))]
                        .map(s=>`<option value="${s}">${s}</option>`).join("")}
        </select>
    </div>

    <table class="table2">
        <tr style="background:#f0f0f0;font-weight:bold;">
            <td>Store</td>
            <td style="text-align:right;">Available Qty</td>
        </tr>`;

            currentStock.forEach(s=> {
                html += `
        <tr class="store">
            <td>${s.StoreName}</td>
            <td style="text-align:right">${s.StockQty}</td>
        </tr>`;
            });

            html += "</table>";
            stockDetails.innerHTML = html;
        }

        function applyStoreFilter() {
            const val = document.getElementById("storeFilter").value;
            const rows = document.querySelectorAll(".store");
            rows.forEach(r=> {
                r.style.display = (!val || r.cells[0].innerText === val) ? "" : "none";
            });
        }
    </script>

</asp:Content>
