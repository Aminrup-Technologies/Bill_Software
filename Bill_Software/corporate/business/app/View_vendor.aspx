<%@ Page Title="View Vendors" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm13" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <style>
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { font-weight: bold; display: block; margin-bottom: 5px; }
        .form-control { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
        .btn-primary { background: #19658A; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn-secondary { background: #6c757d; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; }
        .btn-success { background: #28a745; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; }
        
        /* Modern DataList Table Styles */
        .modern-table { width: 100%; border-collapse: collapse; margin-top: 15px; }
        .modern-table th { background-color: #19658A; color: white; padding: 10px; text-align: left; border: 1px solid #ddd; }
        .modern-table td { padding: 10px; text-align: left; border: 1px solid #ddd; vertical-align: top; }
        .modern-table tr:nth-child(even) { background-color: #f9f9f9; }
        .modern-table tr:hover { background-color: #f1f1f1; }
        
        /* jQuery UI Autocomplete Overrides */
        .ui-autocomplete { z-index: 1050 !important; max-height: 250px; overflow-y: auto; overflow-x: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        .ui-menu-item .ui-menu-item-wrapper.ui-state-active { background: #19658A !important; color: #fff !important; border: 1px solid #19658A !important; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="box-panel">
        <h3 style="color: #19658A; margin-top:0; border-bottom: 2px solid #19658A; padding-bottom: 5px;">View & Manage Vendors</h3>
        
        <div class="form-group" style="margin-top: 20px; display: flex; gap: 10px; align-items: flex-end;">
            <div style="flex-grow: 1; max-width: 400px;">
                <label>Smart Search: Type Vendor Name</label>
                <asp:TextBox ID="txtVendorSearch" runat="server" CssClass="form-control" placeholder="Type at least 2 letters..."></asp:TextBox>
            </div>
            <div>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn-secondary" OnClick="btnReset_Click" />
                <button type="button" class="btn-success" onclick="document.getElementById('exportModal').style.display='block';">📥 Export</button>
            </div>
        </div>

        <div style="margin-top: 15px; margin-bottom: 10px;">
            <asp:Label ID="lblRecordCount" runat="server" Font-Bold="true" Font-Size="13px"></asp:Label>
        </div>

        <asp:DataList ID="DataList1" runat="server" Width="100%" OnItemCommand="DataList1_ItemCommand">
            <HeaderTemplate>
                <table class="modern-table">
                    <tr>
                        <th style="width: 20%;">Vendor Identity</th>
                        <th style="width: 20%;">Location & Contact</th>
                        <th style="width: 25%;">Compliance & Banking</th>
                        <th style="width: 20%;">Audit Info</th>
                        <th style="width: 15%; text-align: center;">Manage</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                    <tr>
                        <td>
                            <span style="color: #FF6600; font-weight: bold; font-size: 11px;"><%# Eval("Vendor_Id") %></span><br />
                            <strong style="font-size: 14px; color: #19658A;"><%# Eval("Vendor_Name") %></strong><br />
                            <span style="background:#e9ecef; padding:2px 6px; border-radius:4px; font-size:11px; margin-top:4px; display:inline-block;">
                                Code: <%# string.IsNullOrEmpty(Convert.ToString(Eval("PrincipleVndrCode"))) ? "N/A" : Eval("PrincipleVndrCode") %>
                            </span>
                        </td>
                        
                        <td style="font-size: 12px; color: #444;">
                            <strong>📍 <%# Eval("City") %>, <%# Eval("State") %> - <%# Eval("pin") %></strong><br />
                            <div style="margin-top: 3px; line-height: 1.3; font-size: 11px; color: #777;">
                                <%# Eval("Address1") %>
                            </div>
                            <div style="margin-top: 8px;">📞 <%# string.IsNullOrEmpty(Convert.ToString(Eval("Com_phone"))) ? "N/A" : Eval("Com_phone") %></div>
                            <div>✉️ <%# string.IsNullOrEmpty(Convert.ToString(Eval("Com_email"))) ? "N/A" : Eval("Com_email") %></div>
                        </td>
                        
                        <td style="font-size: 12px;">
                            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 5px;">
                                <div><span style="color: #666; font-size: 10px; display:block;">GST NO.</span><strong><%# string.IsNullOrEmpty(Convert.ToString(Eval("Service_tax_No"))) ? "N/A" : Eval("Service_tax_No") %></strong></div>
                                <div><span style="color: #666; font-size: 10px; display:block;">PAN NO.</span><strong><%# string.IsNullOrEmpty(Convert.ToString(Eval("Pan_No"))) ? "N/A" : Eval("Pan_No") %></strong></div>
                                <div style="margin-top: 5px; grid-column: span 2;">
                                    <span style="color: #666; font-size: 10px; display:block;">BANK ACC & IFSC</span>
                                    <span style="color: #19658A; font-weight: bold;"><%# string.IsNullOrEmpty(Convert.ToString(Eval("BankAccNo"))) ? "N/A" : Eval("BankAccNo") %></span> 
                                    | <%# string.IsNullOrEmpty(Convert.ToString(Eval("BankIfscCode"))) ? "N/A" : Eval("BankIfscCode") %>
                                </div>
                            </div>
                        </td>

                        <td style="font-size: 11px; color: #555;">
                            <div style="margin-bottom: 6px;">
                                <span style="color: #888; font-size: 9px; text-transform: uppercase; display:block;">Created By</span>
                                <%# string.IsNullOrEmpty(Convert.ToString(Eval("CreatedBy"))) ? "System" : Eval("CreatedBy") %><br />
                                <span style="font-size: 10px; color: #999;"><%# Eval("CreatedOn", "{0:dd-MMM-yyyy}") %></span>
                            </div>
                            <div>
                                <span style="color: #888; font-size: 9px; text-transform: uppercase; display:block;">Last Updated</span>
                                <%# string.IsNullOrEmpty(Convert.ToString(Eval("UpdatedBy"))) ? "--" : Eval("UpdatedBy") %><br />
                                <span style="font-size: 10px; color: #999;"><%# string.IsNullOrEmpty(Convert.ToString(Eval("UpdatedOn"))) ? "" : Eval("UpdatedOn", "{0:dd-MMM-yyyy HH:mm}") %></span>
                            </div>
                        </td>

                        <td style="text-align: center; vertical-align: middle;">
                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("Vendor_Id") %>' style="padding: 6px 15px; font-size: 12px; text-decoration: none; background: #ffc107; color: #212529; border-radius: 4px; font-weight: bold; display: inline-block;">✏️ Edit Profile</asp:LinkButton>
                        </td>
                    </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:DataList>
    </div>

    <div id="exportModal" style="display:none; position:fixed; z-index:9999; left:0; top:0; width:100%; height:100%; background-color:rgba(0,0,0,0.5);">
        <div style="background-color:#fff; margin: 10% auto; padding: 20px; border-radius: 8px; width: 400px; box-shadow: 0 4px 15px rgba(0,0,0,0.2);">
            <h4 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #19658A; padding-bottom: 10px;">Export Vendor Data</h4>
            
            <div class="form-group" style="margin-top: 15px;">
                <label>Select Export Type:</label>
                <asp:DropDownList ID="ddlExportType" runat="server" CssClass="form-control">
                    <asp:ListItem Value="Master">🏢 Basic Info & Location</asp:ListItem>
                    <asp:ListItem Value="Banking">🏦 Tax, Compliance & Banking Details</asp:ListItem>
                    <asp:ListItem Value="Full">📋 Full Vendor Dump (All Columns)</asp:ListItem>
                </asp:DropDownList>
            </div>

            <p style="font-size: 12px; color: #666;"><em>Note: The export will match your current search box keyword.</em></p>

            <div style="text-align: right; margin-top: 20px;">
                <button type="button" class="btn-secondary" onclick="document.getElementById('exportModal').style.display='none';">Cancel</button>
                <asp:Button ID="btnDownloadExcel" runat="server" Text="Download CSV" CssClass="btn-success" OnClick="btnDownloadExcel_Click" />
            </div>
        </div>
    </div>

    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#<%=txtVendorSearch.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "View_vendor.aspx/GetVendorNames",
                        data: "{ 'prefix': '" + request.term + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            response($.map(data.d, function (item) {
                                return { label: item, value: item };
                            }));
                        }
                    });
                },
                minLength: 2,
                select: function(event, ui) {
                    $("#<%=txtVendorSearch.ClientID%>").val(ui.item.value);
                    $("#<%=btnSearch.ClientID%>").click();
                    return false;
                }
            });
        });
    </script>
</asp:Content>