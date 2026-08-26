<%@ Page Title="View Tax Invoices" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm27" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <style type="text/css">
        /* --- Modern Layout & Search Panel --- */
        .page-header {
            background-color: #19658A;
            color: #FFFFFF;
            padding: 12px 15px;
            font-weight: bold;
            font-size: 16px;
            border-radius: 4px;
            margin-bottom: 15px;
        }

        .search-panel {
            background-color: #f8f9fa;
            border: 1px solid #ddd;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: center;
        }

            .search-panel label {
                font-size: 12px;
                font-weight: bold;
                color: #333;
                display: block;
                margin-bottom: 4px;
            }

            .search-panel input[type="text"] {
                padding: 6px 10px;
                border: 1px solid #ccc;
                border-radius: 4px;
                font-size: 13px;
                width: 160px;
            }

        .btn-action {
            padding: 7px 15px;
            background-color: #006699;
            color: white;
            border: none;
            cursor: pointer;
            font-weight: bold;
            font-size: 13px;
            border-radius: 4px;
            transition: background 0.2s;
            margin-top: 18px;
        }

            .btn-action:hover {
                background-color: #004d73;
            }

        .btn-clear {
            background-color: #6c757d;
            margin-left: 5px;
        }

            .btn-clear:hover {
                background-color: #5a6268;
            }

        /* --- Modern Table Design --- */
        .styled-table {
            width: 100%;
            border-collapse: collapse;
            font-family: Arial, sans-serif;
            font-size: 12px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
        }

            .styled-table thead tr {
                background-color: #006699;
                color: #ffffff;
                text-align: center;
            }

            .styled-table th {
                padding: 10px;
                border: 1px solid #004d73;
                position: sticky;
                top: 0;
                z-index: 5;
            }

            .styled-table td {
                padding: 8px 10px;
                border: 1px solid #ddd;
                text-align: center;
                vertical-align: middle;
            }

            .styled-table tbody tr {
                background-color: #ffffff;
                transition: background-color 0.15s ease;
            }

                .styled-table tbody tr:nth-of-type(even) {
                    background-color: #f3f8fb;
                }

                .styled-table tbody tr:hover {
                    background-color: #e2eef4;
                }

        .text-left {
            text-align: left !important;
        }

        .text-right {
            text-align: right !important;
        }

        .badge {
            background: #eee;
            padding: 2px 5px;
            border-radius: 3px;
            font-size: 10px;
            color: #555;
            display: inline-block;
            min-width: 30px;
            text-align: center;
        }

        .badge-blue {
            background-color: #19658A;
            color: white;
        }
        /* --- Fix Datepicker Overlap --- */
        .ui-datepicker {
            z-index: 9999 !important;
        }
    </style>

    <script type="text/javascript">
        $(document).ready(function () {
            // Initialize Datepickers
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="width: 98%; margin: auto; padding-top: 10px;">

        <div class="page-header">View Tax Invoices</div>

        <asp:Panel ID="PanelMsg" runat="server" Visible="false" Style="padding: 10px; margin-bottom: 15px; border-radius: 4px;">
            <asp:Label ID="lblMsg" runat="server" Font-Bold="true"></asp:Label>
        </asp:Panel>

        <div class="search-panel">
            <div>
                <label>Invoice No:</label>
                <asp:TextBox ID="txtSearchInv" runat="server" placeholder="e.g. INV/C/..."></asp:TextBox>
            </div>
            <div>
                <label>Ext Ref (ERP):</label>
                <asp:TextBox ID="txtSearchExt" runat="server" placeholder="e.g. FE/25-26..."></asp:TextBox>
            </div>
            <div>
                <label>Client Name:</label>
                <asp:TextBox ID="txtSearchClient" runat="server" placeholder="Search Client"></asp:TextBox>
            </div>
            <div>
                <label>From Date:</label>
                <asp:TextBox ID="txtFromDate" runat="server" CssClass="datepicker"></asp:TextBox>
            </div>
            <div>
                <label>To Date:</label>
                <asp:TextBox ID="txtToDate" runat="server" CssClass="datepicker"></asp:TextBox>
            </div>
            <div>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-action" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-action btn-clear" OnClick="btnClear_Click" />
                <asp:Button ID="btnExport" runat="server" Text="Export Excel" CssClass="btn-action"
                    Style="background-color: #28a745; margin-left: 5px;" OnClick="btnExport_Click" />
            </div>
        </div>

        <div style="max-height: 600px; overflow-y: auto; border: 1px solid #ccc;">
            <table class="styled-table">
                <thead>
                    <tr>
                        <th style="width: 3%;">Sl</th>
                        <th style="width: 14%;">Customer Name</th>
                        <th style="width: 9%;">Inv Date</th>
                        <th style="width: 16%;">Invoice / Quotation Info</th>
                        <th style="width: 12%;">ARC / PO / DO</th>
                        <th style="width: 16%;">Amount Summary</th>
                        <th style="width: 11%;">Validity</th>
                        <th style="width: 11%;">Created By</th>
                        <th style="width: 4%;">Buyer</th>
                        <th style="width: 4%;">Seller</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptInvoices" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><%# Container.ItemIndex + 1 %></td>
                                <td class="text-left"><strong><%# Eval("Client_Name") %></strong></td>
                                <td><%# Eval("Invoice_Date") %></td>
                                <td class="text-left">
                                    <span class="badge badge-blue">Inv</span> <strong><%# Eval("Invoice_No") %></strong><br />

                                    <%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ExtInvoiceNo"))) ? "" : "<span class='badge'>Ext</span> " + Eval("ExtInvoiceNo") + "<br />" %>

                                    <span class="badge">Quo</span>
                                    <span style='<%# Eval("Quotation_No").ToString().ToUpper() == "VERBAL" ? "color:#d39e00; font-weight:bold;": "" %>'>
                                        <%# Eval("Quotation_No") %>
                                    </span>
                                    <br />

                                    <span style="font-size: 10px; color: #666;"><%# Eval("PServiceName") %></span>
                                </td>
                                <td class="text-left">
                                    <span class="badge">ARC</span> <%# Eval("PO_Number") %><br />
                                    <span class="badge">PO/DO</span> <%# Eval("DO_Number") %>
                                </td>
                                <td class="text-right" style="line-height: 1.4;">
                                    <span style="color: #666;">Gross:</span> ₹<%# Eval("Gross") %><br />
                                    <%# Convert.ToDecimal(Eval("discount") == DBNull.Value ? 0 : Eval("discount")) > 0 ? "<span style='color:red;'>Disc: -₹" + Eval("discount") + "</span><br />" : "" %><span style="color: #666;">Taxable:</span> ₹<%# Eval("sub_total") %><br />
                                    <span class="badge" style="background: #e8f4fd; color: #006699;"><%# Eval("cgstOrsgst").ToString() == "YES" ? "CGST/SGST" : (Eval("igst").ToString() == "YES" ? "IGST" : "TAX") %></span>₹<%# Eval("Gst") %><br />
                                    <%# Convert.ToDecimal(Eval("Delivery_Amount") == DBNull.Value ? 0 : Eval("Delivery_Amount")) + Convert.ToDecimal(Eval("otherAmount1") == DBNull.Value ? 0 : Eval("otherAmount1")) > 0 ? "<span style='color:#666;'>Frt/Oth:</span> ₹" + (Convert.ToDecimal(Eval("Delivery_Amount") == DBNull.Value ? 0 : Eval("Delivery_Amount")) + Convert.ToDecimal(Eval("otherAmount1") == DBNull.Value ? 0 : Eval("otherAmount1"))) + "<br />" : "" %><strong style="color: #28a745; font-size: 13px;">Total: ₹<%# Eval("Net_Amount") %></strong></td>
                                <td>
                                    <%# Eval("Validity_StartDate") %>
                                    <br />
                                    to<br />
                                    <%# Eval("Validity_EndDate") %>
                                </td>
                                <td>
                                    <span style="color: #333; font-weight: bold;">
                                        <%# Convert.ToString(Eval("AddedByName")) %>
                                    </span>
                                    <br />
                                    <span style="font-size: 10px; color: #666;">
                                        <%# Convert.ToDateTime(Eval("TimeStamp")).ToString("dd-MMM-yyyy hh:mm tt") %>
                                    </span>
                                </td>
                                <td>
                                    <a href="#" onclick="window.open('/corporate/business/print/NewInvoice.aspx?ID=<%# Eval("ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                        <img alt="Buyer View" height="22px" src="../WebImages/viewicon.png" />
                                    </a>
                                </td>
                                <td>
                                    <a href="#" onclick="window.open('/corporate/business/print/NewInvoiceDuplicate.aspx?ID=<%# Eval("ID") %>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                        <img alt="Seller View" height="22px" src="../WebImages/viewicon.png" />
                                    </a>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:PlaceHolder ID="phNoData" runat="server" Visible='<%# ((Repeater)Container.NamingContainer).Items.Count == 0 %>'>
                                <tr>
                                    <td colspan="10" style="padding: 20px; color: red; font-weight: bold;">No Invoices Found for the selected filters.</td>
                                </tr>
                            </asp:PlaceHolder>
                        </FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

    </div>
</asp:Content>
