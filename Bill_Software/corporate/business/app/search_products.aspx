<%@ Page Title="Flame-Ex | Search Products" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="search_products.aspx.cs" Inherits="Bill_Software.corporate.business.app.search_products" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }

        .auto-style1 {
            height: 19px;
        }
    </style>

    <style type="text/css">
        .product-card-container {
            width: 100%;
        }

        .product-card {
            background: #ffffff;
            border-radius: 10px;
            padding: 14px;
            margin: 10px;
            width: 240px;
            border: 1px solid #e6e6e6;
            box-shadow: 0 6px 14px rgba(0,0,0,0.08);
            transition: transform .2s ease, box-shadow .2s ease;
        }

            .product-card:hover {
                transform: translateY(-4px);
                box-shadow: 0 10px 20px rgba(0,0,0,0.12);
            }

        .brand-name {
            font-size: 18px;
            font-weight: 700;
            color: #1f3c88;
            text-transform: uppercase;
            margin-bottom: 6px;
        }

        .product-name {
            font-size: 14px;
            font-weight: 600;
            color: #333;
            margin-bottom: 10px;
        }

        .product-meta {
            font-size: 12px;
            color: #666;
            display: flex;
            justify-content: space-between;
            margin-bottom: 12px;
        }

        .card-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .price {
            font-size: 15px;
            font-weight: 700;
            color: #2e7d32;
        }

        .btn-view {
            background: #1f3c88;
            color: #fff;
            border: none;
            border-radius: 6px;
            padding: 4px 10px;
            font-size: 12px;
            cursor: pointer;
        }

            .btn-view:hover {
                background: #162d66;
            }

        .image-wrapper {
            position: relative;
            height: 120px;
            background: #f5f5f5;
            border-radius: 8px;
            overflow: hidden;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .product-img {
            max-height: 100%;
            max-width: 100%;
            object-fit: contain;
        }

        /* Badges */
        .badge {
            position: absolute;
            top: 8px;
            left: 8px;
            font-size: 10px;
            padding: 3px 6px;
            border-radius: 4px;
            color: #fff;
            font-weight: 700;
        }

            .badge.gst {
                background: #ff9800;
            }

            .badge.new {
                background: #4caf50;
                top: 8px;
                right: 8px;
                left: auto;
            }

            .badge.fast {
                background: #e53935;
                top: 30px;
                right: 8px;
                left: auto;
            }
    </style>

    <style type="text/css">
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.55);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .modal-content {
            background: #fff;
            width: 750px;
            border-radius: 10px;
            overflow: hidden;
        }

        .modal-header {
            padding: 12px 16px;
            background: #1f3c88;
            color: #fff;
            display: flex;
            justify-content: space-between;
        }

        .modal-close {
            cursor: pointer;
            font-size: 22px;
        }

        .modal-body {
            display: flex;
            padding: 16px;
            gap: 20px;
        }

        .modal-left img {
            width: 220px;
            height: 220px;
            object-fit: contain;
            border: 1px solid #eee;
            border-radius: 6px;
        }

        .modal-right {
            flex: 1;
            font-size: 14px;
        }

            .modal-right p {
                margin: 6px 0;
            }

        .muted {
            color: #666;
        }

        .modal-footer {
            padding: 12px 16px;
            text-align: right;
            border-top: 1px solid #eee;
        }

        .btn-primary {
            background: #1f3c88;
            color: #fff;
            border: none;
            padding: 6px 14px;
            border-radius: 6px;
        }
    </style>


    <script type="text/javascript">

        /* ===============================
           DEBOUNCE SEARCH (CARDS)
        =============================== */
        var cardSearchTimer = null;

        function debouncedCardSearch() {
            clearTimeout(cardSearchTimer);
            cardSearchTimer = setTimeout(function () {
                searchCards();
            }, 300);
        }

        /* ===============================
           CARD SEARCH (PRIMARY LOGIC)
        =============================== */
        function searchCards() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            if (!input) return;

            var filter = input.value.trim().toLowerCase();
            var cards = document.querySelectorAll('.product-card');
            var matchCount = 0;

            cards.forEach(function (card) {
                var text =
                    (card.dataset.name || "") + " " +
                    (card.dataset.brand || "") + " " +
                    (card.dataset.category || "") + " " +
                    (card.dataset.type || "");

                if (filter === "" || text.indexOf(filter) > -1) {
                    card.style.display = "";
                    matchCount++;
                } else {
                    card.style.display = "none";
                }
            });

            var lbl = document.getElementById("lblNoRecords");
            if (lbl) {
                lbl.style.display = (filter !== "" && matchCount === 0) ? "block" : "none";
            }
        }

        /* ===============================
           CLEAR SEARCH
        =============================== */
        function clearCardSearch() {
            var input = document.getElementById('<%= txtSearch.ClientID %>');
            var cards = document.querySelectorAll('.product-card');

            if (input) input.value = "";

            cards.forEach(function (card) {
                card.style.display = "";
            });

            var lbl = document.getElementById("lblNoRecords");
            if (lbl) lbl.style.display = "none";

            if (input) input.focus();
        }

        /* ===============================
           LAZY LOAD IMAGES
        =============================== */
        function lazyLoadImages() {
            var images = document.querySelectorAll("img.lazy-img");

            images.forEach(function (img) {
                if (img.dataset.loaded) return;

                var rect = img.getBoundingClientRect();
                if (rect.top < window.innerHeight) {
                    img.src = img.dataset.src;
                    img.dataset.loaded = "1";
                }
            });
        }

        document.addEventListener("scroll", lazyLoadImages);
        document.addEventListener("DOMContentLoaded", function () {
            searchCards();       // initialize
            lazyLoadImages();    // load visible images
        });

        /* ===============================
           LOAD PRODUCT DETAILS (LAZY)
        =============================== */
        function loadProductDetails(productId) {
            // AJAX call will come here later

            var card = document.querySelector('.product-card[data-id="' + productId + '"]');
            if (!card) return;

            var img = card.querySelector('.product-img');
            if (img && img.dataset.real && !img.dataset.loaded) {
                img.src = img.dataset.real;
                img.dataset.loaded = "1";
            }
        }


        function loadProductDetails(productId) {

            fetch("search_products.aspx/GetProductDetails", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId: productId })
            })
            .then(res => res.json())
            .then(res => {
                var d = res.d;
                if (!d) return;

                document.getElementById("mdlProductName").innerText = d.ProductName;
                document.getElementById("mdlBrand").innerText = d.Brand;
                document.getElementById("mdlCategory").innerText = d.Category;
                document.getElementById("mdlUnit").innerText = d.Unit;
                document.getElementById("mdlRate").innerText = d.Rate;
                document.getElementById("mdlGST").innerText = d.GST;
                document.getElementById("mdlSpec").innerText = d.Spec || "-";

                var img = document.getElementById("mdlProductImg");
                img.src = d.Image || "/assets/img/product-default.png";

                var oem = document.getElementById("mdlOEMUrl");
                if (d.OEMUrl) {
                    oem.href = d.OEMUrl;
                    oem.style.display = "inline";
                } else {
                    oem.style.display = "none";
                }

                document.getElementById("productModal").style.display = "flex";
            });
        }

        function closeProductModal() {
            document.getElementById("productModal").style.display = "none";
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
            $(".datepicker").datepicker({
                dateFormat: 'dd-M-yy',

                changeMonth: true,
                changeYear: true
            });
        });


    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    </asp:ScriptManager>
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">New & View Products Details</span></td>
        </tr>
        <tr>
            <td width="20%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="20%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD"
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server"
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300"
                    BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px"
                        ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png"
                        Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                </asp:Panel>

            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE CATAGORY</td>
            <td>
                <asp:DropDownList ID="cmdProduct" runat="server" CssClass="dropdown_style" Width="300px" AutoPostBack="True" OnSelectedIndexChanged="cmdProduct_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<span>Search Box</span></td>
            <td>
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="textbox_U_style"
                    Width="300px"
                    placeholder="Search by name, brand, category..."
                    onkeyup="debouncedCardSearch()" />
            </td>
            <td>&nbsp;<asp:Button ID="btnClearServiceSearch" runat="server"
                Text="Clear"
                CssClass="btn btn-primary btn_style"
                OnClientClick="clearServiceGridSearch(); return false;" /></td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server"
                    RepeatColumns="4"
                    RepeatDirection="Horizontal"
                    CssClass="product-card-container">

                    <ItemTemplate>
                        <div class="product-card"
                            data-id="<%# Eval("Id") %>"
                            data-name="<%# Eval("NormalizedProductName") %>"
                            data-brand="<%# Eval("Brand") %>"
                            data-category="<%# Eval("ProductOrServiceCat") %>"
                            data-type="<%# Eval("Type") %>">

                            <!-- Image Slot -->
                            <div class="image-wrapper">
                                <img class="product-img lazy-img"
                                    src="../../../Images/no_image.jpg"
                                    data-src="../../../Images/no_image.jpg"
                                    alt="Product Image" />

                                <!-- Badges -->
                                <span class="badge gst">GST <%# Eval("Tax_Rate") %>%</span>

                                <%-- Future flags --%>
                                <%-- 
                                    <span class="badge new">NEW</span>
                                    <span class="badge fast">FAST</span>
                                --%>
                            </div>

                            <!-- Brand -->
                            <div class="brand-name">
                                <%# Eval("Brand") %>
                            </div>

                            <!-- Product Name -->
                            <div class="product-name">
                                <%# Eval("ProductName") %>
                            </div>

                            <!-- Footer -->
                            <div class="card-footer">
                                <span class="price">₹ <%# Eval("Sail_Rate") %></span>

                                <asp:Button runat="server"
                                    Text="Details"
                                    CssClass="btn-view"
                                    OnClientClick='<%# "loadProductDetails(" + Eval("Id") + "); return false;" %>' />
                            </div>

                        </div>
                    </ItemTemplate>
                </asp:DataList>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>

    <!-- Product Details Modal -->
    <div id="productModal" class="modal-overlay" style="display: none;">
        <div class="modal-content">

            <!-- Header -->
            <div class="modal-header">
                <h3 id="mdlProductName">Product Details</h3>
                <span class="modal-close" onclick="closeProductModal()">×</span>
            </div>

            <!-- Body -->
            <div class="modal-body">

                <div class="modal-left">
                    <img id="mdlProductImg"
                        src="../../../Images/no_image.jpg"
                        alt="Product Image" />
                </div>

                <div class="modal-right">
                    <p><b>Brand:</b> <span id="mdlBrand"></span></p>
                    <p><b>Category:</b> <span id="mdlCategory"></span></p>
                    <p><b>Unit:</b> <span id="mdlUnit"></span></p>
                    <p><b>GST:</b> <span id="mdlGST"></span>%</p>
                    <p><b>Rate:</b> ₹ <span id="mdlRate"></span></p>

                    <p><b>Specification:</b></p>
                    <p id="mdlSpec" class="muted"></p>

                    <p>
                        <a id="mdlOEMUrl" href="#" target="_blank" style="display: none;">🔗 View OEM Product Page
                        </a>
                    </p>
                </div>

            </div>

            <!-- Footer -->
            <div class="modal-footer">
                <button class="btn-primary" onclick="closeProductModal()">Close</button>
            </div>

        </div>
    </div>

</asp:Content>
