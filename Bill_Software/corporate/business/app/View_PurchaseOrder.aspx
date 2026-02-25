<%@ Page Title="View Purchase Orders" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PurchaseOrder.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PurchaseOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>

    <script type="text/javascript">
        $(function () {
            // 1. Initialize Datepickers to match image format (e.g., 26-Jan-2026)
            $(".date-picker").datepicker({
                dateFormat: 'dd-M-yy',
                changeMonth: true,
                changeYear: true
            });

            // 2. Setup AJAX Autocomplete for Client Name
            $("#<%=txtCustomerName.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "View_PurchaseOrder.aspx/GetClientNames",
                        data: "{ 'prefix': '" + request.term + "'}",
                        dataType: "json", type: "POST", contentType: "application/json; charset=utf-8",
                        success: function (data) { response($.map(data.d, function (item) { return { label: item, value: item } })); }
                    });
                }, minLength: 2
            });

            // 3. Setup AJAX Autocomplete for Quotation No
            $("#<%=txtQuotationNo.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "View_PurchaseOrder.aspx/GetQuotationNos",
                        data: "{ 'prefix': '" + request.term + "'}",
                        dataType: "json", type: "POST", contentType: "application/json; charset=utf-8",
                        success: function (data) { response($.map(data.d, function (item) { return { label: item, value: item } })); }
                    });
                }, minLength: 2
            });

            // 4. Setup AJAX Autocomplete for ARC / PO / DO Number
            $("#<%=txtArcPoDo.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "View_PurchaseOrder.aspx/GetArcPoDoNos",
                        data: "{ 'prefix': '" + request.term + "'}",
                        dataType: "json", 
                        type: "POST", 
                        contentType: "application/json; charset=utf-8",
                        success: function (data) { 
                            response($.map(data.d, function (item) { 
                                return { label: item, value: item } 
                            })); 
                        }
                    });
                }, 
                minLength: 2
            });
        });

        // Global Soft Error Handling for WebForms
        window.onerror = function (msg) { console.warn("JS Error softly caught: " + msg); return true; };
    </script>

    <style type="text/css">
        /* Layout matching the provided image */
        .wrapper-card { border: 1px solid #ddd; border-radius: 6px; background: #fff; margin: 15px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        .card-header { background-color: #1a6083; color: white; padding: 12px 20px; font-size: 16px; font-weight: bold; border-radius: 5px 5px 0 0; }
        
        /* Search Panel Styling */
        .search-area { background-color: #f9f9f9; padding: 15px 20px; border-bottom: 1px solid #eaeaea; display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; }
        .form-group { display: flex; flex-direction: column; flex: 1; min-width: 140px; }
        .form-group label { font-size: 11px; font-weight: 600; margin-bottom: 5px; color: #333; }
        .form-control { border: 1px solid #ccc; padding: 8px 10px; border-radius: 4px; font-size: 12px; }
        .form-control:focus { outline: none; border-color: #1a6083; box-shadow: 0 0 3px rgba(26,96,131,0.3); }
        
        .btn-group { display: flex; gap: 8px; }
        .btn { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; color: white; font-weight: bold; font-size: 12px; }
        .btn-search { background-color: #005b8f; }
        .btn-clear { background-color: #6c757d; }
        .btn:hover { opacity: 0.9; }

        /* Table Styling */
        .data-table { width: 100%; border-collapse: collapse; font-size: 12px; }
        .data-table thead th { background-color: #006699; color: white; padding: 10px; text-align: center; font-weight: normal; border: 1px solid #005b8f; }
        .data-table tbody td { padding: 10px; border: 1px solid #ddd; text-align: center; color: #444; vertical-align: middle; }
        .data-table tbody tr:hover { background-color: #f4f8fa; }

        /* Badge Styling inside Table */
        .info-block { display: flex; align-items: center; justify-content: flex-start; margin-bottom: 4px; gap: 6px; text-align: left; }
        .badge-label { background-color: #e2e3e5; color: #383d41; padding: 3px 6px; border-radius: 3px; font-size: 10px; font-weight: bold; min-width: 40px; text-align: center; }
        .badge-dark { background-color: #1a6083; color: white; }
        
        /* Summary Styling */
        .summary-block { text-align: right; line-height: 1.5; font-size: 11px; padding-right: 15px; }
        .summary-total { color: #28a745; font-weight: bold; font-size: 12px; }
        .summary-tax { color: #1a6083; }

        .error-panel { background-color: #f8d7da; color: #721c24; padding: 10px; border-bottom: 1px solid #f5c6cb; font-size: 12px; }
        /* --- jQuery UI Autocomplete Fixes --- */
        .ui-autocomplete {
            background-color: #ffffff !important; /* Force white background */
            border: 1px solid #cccccc !important;
            box-shadow: 0 4px 8px rgba(0,0,0,0.15); /* Add a subtle drop shadow */
            max-height: 250px; /* Prevent it from getting too tall */
            overflow-y: auto;
            overflow-x: hidden;
            z-index: 9999 !important; /* Ensure it pops up over everything else */
            padding: 0;
            margin: 0;
        }

        .ui-menu-item {
            list-style-type: none;
            margin: 0;
            padding: 0;
        }

        .ui-menu-item .ui-menu-item-wrapper {
            padding: 8px 12px !important;
            color: #333333 !important; /* Force dark gray text so it is readable */
            font-size: 12px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            border-bottom: 1px solid #f0f0f0;
            cursor: pointer;
        }

        /* Hover & Selection State */
        .ui-menu-item .ui-state-active,
        .ui-menu-item .ui-menu-item-wrapper:hover {
            background-color: #1a6083 !important; /* Your theme's blue color */
            color: #ffffff !important; /* White text on hover */
            border: none !important;
            border-radius: 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="wrapper-card">
        
        <div class="card-header">
            View Purchase Orders
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-panel">
            <asp:Label ID="lblErrorMessage" runat="server"></asp:Label>
        </asp:Panel>

        <div class="search-area">
            <div class="form-group">
                <label>Quotation No:</label>
                <asp:TextBox ID="txtQuotationNo" runat="server" CssClass="form-control" placeholder="e.g. QUO/25..."></asp:TextBox>
            </div>
            <div class="form-group">
                <label>ARC / PO / DO:</label>
                <asp:TextBox ID="txtArcPoDo" runat="server" CssClass="form-control" placeholder="e.g. PO-98..."></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Client Name:</label>
                <asp:TextBox ID="txtCustomerName" runat="server" CssClass="form-control" placeholder="Search Client"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>From Date:</label>
                <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control date-picker" placeholder="DD-Mon-YYYY"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>To Date:</label>
                <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control date-picker" placeholder="DD-Mon-YYYY"></asp:TextBox>
            </div>
            <div class="btn-group">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-search" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-clear" OnClick="btnClear_Click" />
            </div>
        </div>

        <table class="data-table">
            <thead>
                <tr>
                    <th style="width: 3%;">Sl</th>
                    <th style="width: 15%;">Customer Name</th>
                    <th style="width: 10%;">Quo Date</th>
                    <th style="width: 18%;">Quotation Info</th>
                    <th style="width: 12%;">ARC / PO / DO</th>
                    <th style="width: 15%;">Amount Summary</th>
                    <th style="width: 10%;">Validity</th>
                    <th style="width: 12%;">Created By</th>
                    <th style="width: 5%;">View</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptPurchaseOrders" runat="server" OnItemCommand="rptPurchaseOrders_ItemCommand">
                    <ItemTemplate>
                        <tr>
                            <td><%# Container.ItemIndex + 1 %></td>
                            <td style="font-weight: bold; color: #555;"><%# Convert.ToString(Eval("Client_Name")) %></td>
                            <td><%# Convert.ToDateTime(Eval("Quotation_date")).ToString("dd-MMM-yyyy") %></td>
                            <td>
                                <div class="info-block"><span class="badge-label badge-dark">Quo</span> <%# Convert.ToString(Eval("Quotation_no")) %></div>
                                <div class="info-block" style="color:#777; font-size:11px;"> <%# Convert.ToString(Eval("PServiceName")) %></div>
                            </td>
                            <td>
                                <div class="info-block"><span class="badge-label">ARC</span> <%# Convert.ToString(Eval("PO_Number")) %></div>
                                <div class="info-block"><span class="badge-label">PO/DO</span> <%# Convert.ToString(Eval("DO_Number")) %></div>
                            </td>
                            <td class="summary-block">
                                Gross: ₹<%# Convert.ToString(Eval("Gross")) %><br />Taxable: ₹<%# Convert.ToString(Eval("sub_total")) %><br /><span class="summary-tax">CGST/SGST: ₹<%# Convert.ToString(Eval("service_tax1")) %></span><br /><span class="summary-total">Total: ₹<%# Convert.ToString(Eval("Net_amount")) %></span></td>
                            <td style="color:#777; font-size:11px;">
                                <%# Convert.ToString(Eval("Validity_StartDate")) %><br />to<br /><%# Convert.ToString(Eval("Validity_EndDate")) %></td>
                            <td style="font-size:11px;">
                                <strong><%# Convert.ToString(Eval("AddedByName")) %></strong><br />
                                <span style="color:#888;"><%# Eval("TimsStamp") != DBNull.Value ? Convert.ToDateTime(Eval("TimsStamp")).ToString("dd-MMM-yyyy hh:mm tt") : "N/A" %></span>
                            </td>
                            <td>
                                <asp:ImageButton ID="btnView" runat="server" CommandName="View" CommandArgument='<%# Eval("ID") %>'
                                    ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View" Width="18px" Height="18px" />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</asp:Content>