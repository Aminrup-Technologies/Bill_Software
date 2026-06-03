<%@ Page Title="Enterprise Attendance Register" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminAttendanceDashboard.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminAttendanceDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <style type="text/css">
        .dashboard-container {
            max-width: 1200px;
            margin: 20px auto;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        /* Modern Cards */
        .filter-card {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.05);
            border-left: 4px solid #19658A;
            margin-bottom: 20px;
            display: flex;
            gap: 15px;
            align-items: flex-end;
            flex-wrap: wrap;
        }

        .summary-card {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            flex: 1;
            min-width: 200px;
            border: 1px solid #e1e8f0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02);
        }

        .summary-value {
            font-size: 28px;
            font-weight: bold;
            color: #19658A;
            margin-top: 10px;
            display: block;
        }

        /* Grid Styling */
        .beautiful-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
            background: #fff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 10px rgba(0,0,0,0.05);
        }

            .beautiful-grid th {
                background: #19658A;
                color: white;
                padding: 14px 12px;
                text-align: left;
                font-weight: 600;
                text-transform: uppercase;
                font-size: 12px;
                letter-spacing: 0.5px;
            }

            .beautiful-grid td {
                padding: 12px;
                border-bottom: 1px solid #f0f0f0;
                vertical-align: middle;
            }

            .beautiful-grid tr:hover {
                background-color: #f8fafc;
            }

        /* Dynamic Status Badges */
        .badge {
            padding: 5px 10px;
            border-radius: 4px;
            font-weight: bold;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            display: inline-block;
        }

        .badge-office {
            background: #d1fae5;
            color: #065f46;
            border: 1px solid #34d399;
        }

        .badge-field {
            background: #dbeafe;
            color: #1e40af;
            border: 1px solid #93c5fd;
        }

        .badge-leave {
            background: #fef3c7;
            color: #92400e;
            border: 1px solid #fcd34d;
        }

        .badge-absent {
            background: #fee2e2;
            color: #991b1b;
            border: 1px solid #f87171;
        }

        .badge-off {
            background: #f1f5f9;
            color: #475569;
            border: 1px solid #cbd5e1;
        }

        /* High-Contrast CSS Fix: Ensures the dropdown doesn't inherit invisible text from the Master Page */
        .select2-container--default .select2-results__option {
            color: #333 !important;
            background-color: #fff !important;
        }

            .select2-container--default .select2-results__option[aria-selected="true"] {
                background-color: #f0f0f0 !important;
                color: #333 !important;
            }

        .select2-container--default .select2-results__option--highlighted[aria-selected] {
            background-color: #19658A !important;
            color: white !important;
        }

        /* Input sizing mapped to match the 38px height of your 'Fetch Register' button */
        .select2-container .select2-selection--single {
            height: 38px !important;
            border: 1px solid #ccc !important;
            border-radius: 4px !important;
        }

        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 36px !important;
            padding-left: 12px !important;
            color: #333 !important;
        }

        .select2-container--default .select2-selection--single .select2-selection__arrow {
            height: 36px !important;
        }
    </style>

    <script type="text/javascript">
        function initSelect2() {
            var $ddl = $('#<%= ddlEmployee.ClientID %>');

            // Prevent duplicate initializations
            if ($ddl.hasClass("select2-hidden-accessible")) {
                $ddl.select2('destroy');
            }

            $ddl.select2({
                width: '100%'
            });
        }

        // Run on initial page load
        $(document).ready(function () {
            initSelect2();
        });

        // Run on UpdatePanel Postback (if page uses Partial Rendering)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function (sender, e) {
                initSelect2();
            });
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">

        <div class="filter-card">
            <div style="flex: 2; min-width: 250px;">
                <label style="font-weight: bold; font-size: 13px; color: #333;">Select Employee:</label>
                <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="select2-search"></asp:DropDownList>
            </div>
            <div style="flex: 1; min-width: 150px;">
                <label style="font-weight: bold; font-size: 13px; color: #333;">Month:</label>
                <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">January</asp:ListItem>
                    <asp:ListItem Value="2">February</asp:ListItem>
                    <asp:ListItem Value="3">March</asp:ListItem>
                    <asp:ListItem Value="4">April</asp:ListItem>
                    <asp:ListItem Value="5">May</asp:ListItem>
                    <asp:ListItem Value="6">June</asp:ListItem>
                    <asp:ListItem Value="7">July</asp:ListItem>
                    <asp:ListItem Value="8">August</asp:ListItem>
                    <asp:ListItem Value="9">September</asp:ListItem>
                    <asp:ListItem Value="10">October</asp:ListItem>
                    <asp:ListItem Value="11">November</asp:ListItem>
                    <asp:ListItem Value="12">December</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div style="flex: 1; min-width: 120px;">
                <label style="font-weight: bold; font-size: 13px; color: #333;">Year:</label>
                <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div style="display: flex; gap: 10px;">
                <asp:Button ID="btnGenerate" runat="server" Text="🔍 Fetch Register" OnClick="btnGenerate_Click" Style="background: #19658A; color: white; border: none; padding: 8px 25px; border-radius: 4px; font-weight: bold; cursor: pointer; height: 38px;" />

                <asp:Button ID="btnExport" runat="server" Text="📥 Export Excel" OnClick="btnExport_Click" Style="background: #28a745; color: white; border: none; padding: 8px 25px; border-radius: 4px; font-weight: bold; cursor: pointer; height: 38px;" />
            </div>
        </div>

        <div style="display: flex; gap: 15px; margin-bottom: 20px; flex-wrap: wrap;">
            <div class="summary-card">
                <div style="font-size: 12px; color: #64748b; font-weight: 600;">OFFICE PRESENCE</div>
                <asp:Label ID="lblTotalOffice" runat="server" CssClass="summary-value">0 Days</asp:Label>
            </div>
            <div class="summary-card">
                <div style="font-size: 12px; color: #64748b; font-weight: 600;">FIELD OPERATIONS</div>
                <asp:Label ID="lblTotalField" runat="server" CssClass="summary-value">0 Days</asp:Label>
            </div>
            <div class="summary-card">
                <div style="font-size: 12px; color: #64748b; font-weight: 600;">SALES VISITS LOGGED</div>
                <asp:Label ID="lblTotalVisits" runat="server" CssClass="summary-value" Style="color: #0f172a;">0</asp:Label>
            </div>
            <div class="summary-card">
                <div style="font-size: 12px; color: #64748b; font-weight: 600;">TOTAL ABSENTS</div>
                <asp:Label ID="lblTotalAbsents" runat="server" CssClass="summary-value" Style="color: #dc2626;">0 Days</asp:Label>
            </div>
        </div>

        <div style="overflow-x: auto; padding-bottom: 20px;">
            <asp:GridView ID="gvOmniAttendance" runat="server" AutoGenerateColumns="False" CssClass="beautiful-grid" GridLines="None" EmptyDataText="No records found for this period.">
                <Columns>
                    <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" ItemStyle-Font-Bold="true" ItemStyle-Width="120px" />
                    <asp:BoundField DataField="DayOfWeek" HeaderText="Day" ItemStyle-ForeColor="#64748b" ItemStyle-Width="100px" />
                    <asp:BoundField DataField="EmployeeName" HeaderText="Employee Name" />

                    <asp:TemplateField HeaderText="Daily Status">
                        <ItemTemplate>
                            <span class='<%# GetStatusBadgeClass(Eval("CalculatedStatus").ToString()) %>'>
                                <%# Eval("CalculatedStatus") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="PunchInTime" HeaderText="Office IN" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="PunchOutTime" HeaderText="Office OUT" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="TotalHoursWorked" HeaderText="Total Hrs" DataFormatString="{0:F2}" NullDisplayText="-" ItemStyle-HorizontalAlign="Center" />

                    <asp:TemplateField HeaderText="Visits Logged" ItemStyle-HorizontalAlign="Center" ItemStyle-Font-Bold="true">
                        <ItemTemplate>
                            <%# Convert.ToInt32(Eval("FieldVisitsLogged")) > 0 
                            ? string.Format("<a href='srch_dailyrpts.aspx?emp={0}&dt={1:yyyy-MM-dd}' target='_blank' style='color:#007bff; text-decoration:underline;' title='Click to view detailed visit logs'>{2}</a>", Eval("UserCode"), Eval("ActivityDate"), Eval("FieldVisitsLogged"))
                            : "0" 
                            %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="DailyRevenue" HeaderText="Revenue (₹)" DataFormatString="{0:N2}" NullDisplayText="0.00" ItemStyle-HorizontalAlign="Right" />
                </Columns>
            </asp:GridView>
        </div>

    </div>
</asp:Content>
