<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_purches.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm20" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
    <script src="https://code.jquery.com/jquery-1.12.4.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>

    <style type="text/css">
        .search-container {
            background: #fff;
            border: 1px solid #e1e4e8;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }

        .ui-autocomplete-loading {
            background: white url('https://jqueryui.com/resources/demos/autocomplete/images/ui-anim_basic_16x16.gif') right 10px center no-repeat;
        }
        /* Dashboard Header Styling */
        .grid-header-container {
            background: #19658A;
            color: white;
            padding: 12px 20px;
            border-radius: 8px 8px 0 0;
            font-size: 14px;
            font-weight: 600;
        }

        /* Modern Table with Vertical Lines */
        .modern-table {
            width: 100%;
            border-collapse: collapse;
            background: white;
            font-size: 13px;
            border-left: 1px solid #dee2e6;
            border-right: 1px solid #dee2e6;
        }

            .modern-table thead th {
                background: #f8f9fa;
                color: #333;
                padding: 12px;
                text-align: left;
                border-bottom: 2px solid #dee2e6;
                border-right: 1px solid #dee2e6; /* Vertical Grid Lines */
            }

            .modern-table tbody td {
                padding: 12px;
                border-bottom: 1px solid #edf2f7;
                border-right: 1px solid #edf2f7; /* Vertical Grid Lines */
                vertical-align: middle;
            }

            .modern-table tbody tr:nth-child(even) {
                background-color: #f9fbff;
            }

            .modern-table tbody tr:hover {
                background-color: #f1f5f9;
            }

        /* Status & ID Badges */
        .badge-id {
            background: #e2e8f0;
            color: #4a5568;
            padding: 4px 8px;
            border-radius: 4px;
            font-weight: bold;
            font-family: monospace;
            border: 1px solid #cbd5e0;
        }

        /* Input Grouping */
        .form-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: flex-end;
        }

        .input-group {
            display: flex;
            flex-direction: column;
        }

            .input-group label {
                font-size: 11px;
                font-weight: bold;
                color: #666;
                margin-bottom: 4px;
            }

        .form-control {
            padding: 8px;
            border: 1px solid #cbd5e0;
            border-radius: 4px;
            width: 180px;
        }

        .btn-search {
            background: #19658A;
            color: white;
            border: none;
            padding: 9px 20px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-clear {
            background: #edf2f7;
            color: #4a5568;
            border: 1px solid #cbd5e0;
            padding: 9px 15px;
            border-radius: 4px;
            cursor: pointer;
        }

        /* Container of the suggestions */
        .ui-autocomplete {
            background-color: #ffffff !important;
            border: 1px solid #cbd5e0 !important;
            border-radius: 4px !important;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1) !important;
            z-index: 9999 !important; /* Ensures it appears above other elements */
            padding: 0 !important;
            max-height: 300px;
            overflow-y: auto;
        }

        /* Individual list items */
        .ui-menu-item {
            padding: 8px 12px !important;
            color: #333333 !important; /* Dark text for readability */
            font-size: 13px !important;
            cursor: pointer !important;
            list-style: none !important;
            border-bottom: 1px solid #f1f5f9 !important;
        }

        /* Hover/Focus state */
        .ui-state-active,
        .ui-widget-content .ui-state-active {
            background-color: #19658A !important; /* Matches your dashboard blue */
            color: #ffffff !important; /* White text on blue background */
            border: none !important;
            margin: 0 !important;
        }

        /* Remove default jQuery UI item borders */
        .ui-menu-item-wrapper {
            display: block !important;
            padding: 3px 1em 3px .4em !important;
        }
    </style>

    <script type="text/javascript">
        $(function () {
            // Bind Autocomplete to the Keyword search box
            $("#<%= txtSearch.ClientID %>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: '<%= ResolveUrl("View_purches.aspx/GetSearchSuggestions") %>',
                        data: JSON.stringify({ prefix: request.term }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            response(data.d);
                        },
                        error: function (response) {
                            console.log(response.responseText);
                        }
                    });
                },
                minLength: 2 // Start suggesting after 2 characters
            });

            $(".datepicker").datepicker({ dateFormat: 'yy-mm-dd' });
        });

        $(function () {
            // Autocomplete for Vendor Name
            $("#<%= txtVendorSearch.ClientID %>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: '<%= ResolveUrl("View_purches.aspx/GetVendorSuggestions") %>',
                        data: JSON.stringify({ prefix: request.term }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            response(data.d);
                        }
                    });
                },
                minLength: 2
            });
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="grid-header-container">Purchase Management Dashboard</div>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="search-container" Style="border-left: 5px solid #e53e3e;">
        <asp:Label ID="lblUserMessage" runat="server" ForeColor="#c53030" Font-Bold="true"></asp:Label>
    </asp:Panel>

    <div class="search-container">
        <div class="form-row">
            <div class="input-group">
                <label>Keyword (Invoice/Order)</label>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="rj11"></asp:TextBox>
            </div>
            <div class="input-group">
                <label>Vendor Name</label>
                <asp:TextBox ID="txtVendorSearch" runat="server" CssClass="form-control" placeholder="Search vendor..."></asp:TextBox>
            </div>
            <div class="input-group">
                <label>From Date</label>
                <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
            </div>
            <div class="input-group">
                <label>To Date</label>
                <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control datepicker"></asp:TextBox>
            </div>
            <div class="input-group">
                <asp:Button ID="btnFilter" runat="server" Text="Search" CssClass="btn-search" OnClick="btnFilter_Click" />
            </div>
            <div class="input-group">
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-clear" OnClick="btnClear_Click" />
            </div>
        </div>
    </div>

    <div style="overflow-x: auto; border-bottom: 1px solid #dee2e6;">
        <table class="modern-table">
            <thead>
                <tr>
                    <th style="width: 30px;">#</th>
                    <th>Purchase ID</th>
                    <th>Date</th>
                    <th>Vendor</th>
                    <th>Invoice No</th>
                    <th>Order No</th>
                    <th style="text-align: right;">Tax</th>
                    <th style="text-align: right;">Net Amount</th>
                    <th style="text-align: center; border-right: none;">Action</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptPurchase" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%# Container.ItemIndex + 1 %></td>
                            <td><span class="badge-id"><%# Eval("Purches_Id") %></span></td>
                            <td><%# Eval("DisplayDate") %></td>
                            <td><strong><%# Eval("Vendor_Name") %></strong></td>
                            <td><%# Eval("Invoice_No") %></td>
                            <td><%# string.IsNullOrEmpty(Convert.ToString(Eval("BuyerOrderNo"))) ? "--" : Eval("BuyerOrderNo") %></td>
                            <td style="text-align: right;">₹ <%# Eval("Total_Tax_rate", "{0:N2}") %></td>
                            <td style="text-align: right; font-weight: bold; color: #19658A;">₹ <%# Eval("Total_purches_rate", "{0:N2}") %></td>
                            <td style="text-align: center; border-right: none;">
                                <a href="javascript:void(0);" onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# Eval("Purches_Id") %>', 'pop', 'width=900,height=800');">
                                    <img src="../WebImages/viewicon.png" height="18" alt="View" />
                                </a>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</asp:Content>
