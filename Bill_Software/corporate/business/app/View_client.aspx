<%@ Page Title="View Clients" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm16" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
    <style>
        .box-panel {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }

        .form-group {
            margin-bottom: 15px;
        }

            .form-group label {
                font-weight: bold;
                display: block;
                margin-bottom: 5px;
            }

        .form-control {
            width: 100%;
            max-width: 400px;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        /* Modern DataList Table Styles */
        .modern-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }

            .modern-table th {
                background-color: #19658A;
                color: white;
                padding: 10px;
                text-align: center;
                border: 1px solid #ddd;
            }

            .modern-table td {
                padding: 10px;
                text-align: center;
                border: 1px solid #ddd;
                vertical-align: middle;
            }

            .modern-table tr:nth-child(even) {
                background-color: #f9f9f9;
            }

            .modern-table tr:hover {
                background-color: #f1f1f1;
            }

        .icon-btn {
            transition: transform 0.2s;
        }

            .icon-btn:hover {
                transform: scale(1.1);
            }
        /* Styling to match FLMX theme and keep dropdown above modern tables */
        .ui-autocomplete {
            z-index: 1050 !important;
            max-height: 250px;
            overflow-y: auto;
            overflow-x: hidden;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }

        .ui-menu-item .ui-menu-item-wrapper.ui-state-active {
            background: #19658A !important;
            color: #fff !important;
            border: 1px solid #19658A !important;
        }
    </style>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#<%=txtClientSearch.ClientID%>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "View_client.aspx/GetClientNames",
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
                minLength: 2, // Triggers after 2 characters
                select: function (event, ui) {
                    // Auto-Select Magic: Populate the textbox and instantly trigger the Search button click
                    $("#<%=txtClientSearch.ClientID%>").val(ui.item.value);
                    $("#<%=btnSearch.ClientID%>").click();
                    return false;
                }
            });
        });
</script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="box-panel">
        <h3 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #19658A; padding-bottom: 5px;">View & Manage Clients</h3>

        <div class="form-group" style="margin-top: 20px; display: flex; gap: 10px; align-items: flex-end;">
            <div style="flex-grow: 1; max-width: 400px;">
                <label>Smart Search: Type Client Name</label>
                <asp:TextBox ID="txtClientSearch" runat="server" CssClass="form-control" placeholder="Type at least 2 letters..."></asp:TextBox>
            </div>
            <div>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary btn_style" OnClick="btnSearch_Click" />&nbsp;&nbsp;
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn-secondary btn_style" OnClick="btnReset_Click" />&nbsp;&nbsp;
                <button type="button" class="btn-secondary btn_style" onclick="document.getElementById('exportModal').style.display='block';" style="background: #28a745; border-color: #28a745;">📥 Export Data</button>
            </div>
        </div>

        <div id="exportModal" style="display: none; position: fixed; z-index: 9999; left: 0; top: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5);">
            <div style="background-color: #fff; margin: 10% auto; padding: 20px; border-radius: 8px; width: 400px; box-shadow: 0 4px 15px rgba(0,0,0,0.2);">
                <h4 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #19658A; padding-bottom: 10px;">Export Client Data</h4>

                <div class="form-group" style="margin-top: 15px;">
                    <label>Select Export Report Type:</label>
                    <asp:DropDownList ID="ddlExportType" runat="server" CssClass="form-control">
                        <asp:ListItem Value="Master">🏢 Client Master Data (Tax, Contact, Address)</asp:ListItem>
                        <asp:ListItem Value="Reps">👥 Clients + Representatives List</asp:ListItem>
                        <asp:ListItem Value="Factories">🏭 Clients + Factory/Unit Locations</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <p style="font-size: 12px; color: #666;"><em>Note: The export will respect your current search filter. If the search box is empty, it will export all clients.</em></p>

                <div style="text-align: right; margin-top: 20px;">
                    <button type="button" class="btn_style btn-secondary" onclick="document.getElementById('exportModal').style.display='none';">Cancel</button>
                    <asp:Button ID="btnDownloadExcel" runat="server" Text="Download Excel" CssClass="btn_style btn-primary" OnClick="btnDownloadExcel_Click" />
                </div>
            </div>
        </div>

        <div style="margin-top: 15px; margin-bottom: 10px;">
            <asp:Label ID="lblRecordCount" runat="server" Font-Bold="true" Font-Size="13px"></asp:Label>
        </div>

        <asp:DataList ID="DataList1" runat="server" Width="100%" OnItemCommand="DataList1_ItemCommand">
            <HeaderTemplate>
                <table class="modern-table">
                    <tr>
                        <th style="width: 18%; text-align: left; padding-left: 15px;">Client Identity</th>
                        <th style="width: 20%; text-align: left;">Location & Supply</th>
                        <th style="width: 18%; text-align: left;">Contact Info</th>
                        <th style="width: 14%; text-align: left;">Compliance</th>
                        <th style="width: 15%; text-align: left;">Audit Info</th>
                        <th style="width: 15%; text-align: center;">Manage</th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td style="text-align: left; padding-left: 15px; vertical-align: top;">
                        <span style="color: #FF6600; font-weight: bold; font-size: 11px;"><%# Eval("Client_Id") %></span><br />
                        <strong style="font-size: 14px; color: #19658A;"><%# Eval("Client_Name") %></strong><br />
                        <span style="background: #e9ecef; padding: 2px 6px; border-radius: 4px; font-size: 11px; margin-top: 4px; display: inline-block;">Industry: <%# string.IsNullOrEmpty(Convert.ToString(Eval("Industry"))) ? "N/A" : Eval("Industry") %>
                        </span>
                    </td>

                    <td style="text-align: left; font-size: 12px; color: #444; vertical-align: top;">
                        <strong>📍 <%# Eval("City") %>, <%# Eval("State") %> - <%# Eval("pin") %></strong><br />
                        <div style="margin-top: 3px; line-height: 1.3; font-size: 11px; color: #777;">
                            <%# Eval("Address1") %>
                        </div>
                        <div style="margin-top: 5px; font-weight: 500; color: #2D2D2D;">
                            <span style="color: #666;">Place of Supply:</span> <%# Eval("PlaceofSupply") %>
                        </div>
                    </td>

                    <td style="text-align: left; font-size: 12px; color: #444; vertical-align: top;">
                        <div style="margin-bottom: 4px;">📞 <%# string.IsNullOrEmpty(Convert.ToString(Eval("Com_phone"))) ? "N/A" : Eval("Com_phone") %></div>
                        <div style="margin-bottom: 4px;">✉️ <%# string.IsNullOrEmpty(Convert.ToString(Eval("Com_email"))) ? "N/A" : Eval("Com_email") %></div>
                        <div>🌐 <%# string.IsNullOrEmpty(Convert.ToString(Eval("Com_web_site"))) ? "N/A" : Eval("Com_web_site") %></div>
                    </td>

                    <td style="text-align: left; font-size: 12px; vertical-align: top;">
                        <div style="margin-bottom: 4px;">
                            <span style="color: #666; font-size: 10px; display: block;">GST NO.</span>
                            <strong><%# string.IsNullOrEmpty(Convert.ToString(Eval("Service_tax_no"))) ? "N/A" : Eval("Service_tax_no") %></strong>
                        </div>
                        <div>
                            <span style="color: #666; font-size: 10px; display: block;">PAN NO.</span>
                            <strong><%# string.IsNullOrEmpty(Convert.ToString(Eval("Pan_no"))) ? "N/A" : Eval("Pan_no") %></strong>
                        </div>
                    </td>

                    <td style="text-align: left; font-size: 11px; color: #555; vertical-align: top;">
                        <div style="margin-bottom: 6px;">
                            <span style="color: #888; font-size: 9px; text-transform: uppercase; display: block;">Created By</span>
                            <%# string.IsNullOrEmpty(Convert.ToString(Eval("CreatedBy"))) ? "System" : Eval("CreatedBy") %><br />
                            <span style="font-size: 10px; color: #999;"><%# Eval("CreatedOn", "{0:dd-MMM-yyyy}") %></span>
                        </div>
                        <div>
                            <span style="color: #888; font-size: 9px; text-transform: uppercase; display: block;">Last Updated</span>
                            <%# string.IsNullOrEmpty(Convert.ToString(Eval("UpdatedBy"))) ? "--" : Eval("UpdatedBy") %><br />
                            <span style="font-size: 10px; color: #999;"><%# string.IsNullOrEmpty(Convert.ToString(Eval("UpdatedOn"))) ? "" : Eval("UpdatedOn", "{0:dd-MMM-yyyy HH:mm}") %></span>
                        </div>
                    </td>

                    <td style="text-align: center; vertical-align: middle;">
                        <div style="display: flex; flex-direction: column; gap: 8px; align-items: center;">
                            <div style="display: flex; gap: 5px;">
                                <asp:LinkButton ID="btnFactory" runat="server" CommandName="Factory" CommandArgument='<%# Eval("Client_Id") %>' CssClass="btn-secondary" Style="padding: 4px 8px; font-size: 11px; text-decoration: none; background: #17a2b8; color: white; border-radius: 3px;">🏭 Units</asp:LinkButton>
                                <asp:LinkButton ID="btnReps" runat="server" CommandName="Representative" CommandArgument='<%# Eval("Client_Id") %>' CssClass="btn-secondary" Style="padding: 4px 8px; font-size: 11px; text-decoration: none; background: #6c757d; color: white; border-radius: 3px;">👥 Reps</asp:LinkButton>
                            </div>
                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CommandArgument='<%# Eval("Client_Id") %>' Style="padding: 4px 15px; font-size: 11px; text-decoration: none; background: #ffc107; color: #212529; border-radius: 3px; font-weight: bold; width: 100%; box-sizing: border-box; text-align: center;">✏️ Edit</asp:LinkButton>
                        </div>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:DataList>
    </div>
</asp:Content>
