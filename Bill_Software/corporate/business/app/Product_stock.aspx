<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master"
    AutoEventWireup="true" CodeBehind="Product_stock.aspx.cs"
    Inherits="Bill_Software.corporate.business.app.WebForm50" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .auto-style1 {
            width: 100%;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .style2 {
            color: #fff;
            font-weight: bold;
            padding: 10px;
            display: block;
        }

        .search-container {
            display: flex;
            gap: 10px;
            background: #F4F8FB;
            padding: 15px;
            border: 1px solid #B5C7D3;
            border-radius: 4px;
            align-items: center;
        }

        .search-input {
            flex: 1;
            padding: 8px 10px;
            border: 1px solid #9FB3C8;
            border-radius: 3px;
            height: 35px;
        }

        .search-btn {
            background: #19658A;
            color: white;
            border: none;
            padding: 0 20px;
            height: 35px;
            border-radius: 3px;
            cursor: pointer;
            font-weight: bold;
        }

            .search-btn:hover {
                background: #145270;
            }

        /* Spinner */
        .spinner {
            width: 20px;
            height: 20px;
            border: 3px solid #f3f3f3;
            border-top: 3px solid #19658A;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
            display: inline-block;
        }

        @keyframes spin {
            0% {
                transform: rotate(0deg);
            }

            100% {
                transform: rotate(360deg);
            }
        }

        .loading-box {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 40px;
            gap: 10px;
            color: #19658A;
        }

        .search-result {
            padding: 10px;
            border-bottom: 1px solid #eee;
            cursor: pointer;
            transition: 0.2s;
        }

            .search-result:hover {
                background: #f0f7ff;
            }

            .search-result.active {
                background: #d9ecff;
                border-left: 4px solid #19658A;
            }

        .copy-sku {
            font-size: 11px;
            color: #19658A;
            cursor: pointer;
            background: #eef4f7;
            padding: 2px 6px;
            border-radius: 3px;
        }

        .category-header {
            background: #006699;
            color: white;
            padding: 8px;
            font-weight: bold;
            border-radius: 4px 4px 0 0;
        }

        .table2 {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }

            .table2 td {
                border: 1px solid #ccc;
                padding: 8px;
                font-size: 13px;
            }

        .summary-box {
            background: #FFFCE8;
            padding: 10px;
            border: 1px solid #e0d7a6;
            font-weight: bold;
            margin: 10px 0;
            border-radius: 4px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="auto-style1">
        <tr>
            <td bgcolor="#19658A"><span class="style2">Product Inventory Manager</span></td>
        </tr>
        <tr>
            <td>
                <div class="search-container">
                    <select id="ddlCategory" class="search-input" style="flex: 0 0 200px;" onchange="triggerSearch()">
                        <option value="">All Categories (Show All)</option>
                    </select>

                    <input type="text" id="txtSearch" class="search-input"
                        placeholder="Search by name or Product ID..." onkeyup="handleKeyUp(event)" />

                    <button type="button" class="search-btn" onclick="triggerSearch()">Search</button>
                    <button type="button" class="clear-btn" onclick="clearSearch()"
                        style="background: #777; color: white; border: none; padding: 0 15px; height: 35px; border-radius: 3px; cursor: pointer;">
                        Clear
                    </button>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div style="display: flex; gap: 20px; margin-top: 20px; align-items: flex-start;">
                    <div style="width: 40%; max-height: 500px; overflow-y: auto; border: 1px solid #ddd; border-radius: 4px;">
                        <div id="searchResults"></div>
                    </div>
                    <div style="width: 60%; position: sticky; top: 10px;">
                        <div id="stockDetails">
                            <div style="text-align: center; padding: 50px; color: #999;">Select a product to view store-wise stock</div>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    </table>

    <script type="text/javascript">
        let products = [];
        let timer = null;

        document.addEventListener("DOMContentLoaded", () => {
            loadCategories();
            triggerSearch();
        });

        function loadCategories() {
            fetch("Product_stock.aspx/GetCategories", { method: "POST", headers: { "Content-Type": "application/json" } })
            .then(r => r.json()).then(d => {
                const ddl = document.getElementById("ddlCategory");
                d.d.forEach(c => { ddl.options.add(new Option(c, c)); });
            });
        }

        function handleKeyUp(e) {
            clearTimeout(timer);
            timer = setTimeout(triggerSearch, 500);
        }

        function renderResults() {
            if (products.length === 0) {
                searchResults.innerHTML = '<div style="padding:20px; color:#999;">No products found.</div>';
                return;
            }
            searchResults.innerHTML = products.map((p, i) => `
                <div class="search-result" onclick="selectProduct(${i})">
                    <div style="display:flex; justify-content:space-between;">
                        <b>${p.ProductName}</b>
                        <span class="copy-sku" onclick="copySKU(event, '${p.ProductID}')">${p.ProductID}</span>
                    </div>
                    <div style="font-size:11px; color:#666;">${p.CategoryName}</div>
                </div>
            `).join('');
        }

        function selectProduct(index) {
            const p = products[index];
            document.querySelectorAll('.search-result').forEach((r, i) => r.classList.toggle('active', i === index));

            stockDetails.innerHTML = `
                <div class="category-header">${p.ProductName}</div>
                <div class="loading-box"><div class="spinner"></div>Loading stock for ${p.ProductID}...</div>`;

            fetch("Product_stock.aspx/GetStock", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId: p.ProductID })
            })
            .then(r => r.json()).then(d => {
                const stock = d.d || [];
                let total = stock.reduce((acc, curr) => acc + parseFloat(curr.StockQty), 0);

                let html = `
                    <div class="category-header">${p.ProductName}</div>
                    <div class="summary-box">Total Available: ${total.toFixed(2)}</div>
                    <table class="table2">
                        <tr style="background:#f8f8f8; font-weight:bold;"><td>Store Location</td><td align="right">Quantity</td></tr>
                        ${stock.map(s => `<tr><td>${s.StoreName}</td><td align="right" style="color:${parseFloat(s.StockQty) <= 0 ? 'red' : 'green'}">${s.StockQty}</td></tr>`).join('')}
                    </table>`;
                stockDetails.innerHTML = html;
            });
        }

        function clearSearch() {
            // 1. Reset inputs
            document.getElementById("txtSearch").value = "";
            document.getElementById("ddlCategory").selectedIndex = 0;

            // 2. Reset the Stock View (Right Side)
            document.getElementById("stockDetails").innerHTML = `
            <div style="text-align:center; padding:50px; color:#999;">
                Select a product to view store-wise stock
            </div>`;

            // 3. Trigger a fresh search to show "All" again
            triggerSearch();
        }

        function triggerSearch() {
            const search = document.getElementById("txtSearch").value;
            const cat = document.getElementById("ddlCategory").value;

            searchResults.innerHTML = `
            <div class="loading-box">
                <div class="spinner"></div>
                Searching...
            </div>`;

            fetch("Product_stock.aspx/SearchProducts", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ search: search, category: cat })
            })
            .then(r => r.json())
            .then(d => {
                products = d.d || [];
                renderResults();
            })
            .catch(() => {
                searchResults.innerHTML =
                    '<div style="padding:20px; color:red;">Connection error.</div>';
            });
        }


        function copySKU(e, sku) {
            e.stopPropagation();
            navigator.clipboard.writeText(sku);
            const el = e.target;
            const old = el.innerText;
            el.innerText = "Copied!";
            setTimeout(() => el.innerText = old, 1000);
        }
    </script>
</asp:Content>
