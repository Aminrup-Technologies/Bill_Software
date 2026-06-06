<%@ Page Title="Enterprise Attendance Register" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminAttendanceDashboard.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminAttendanceDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

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

        /* Action Buttons */
        .btn-action-approve {
            background-color: #28a745;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 11px;
            font-weight: bold;
            margin-right: 5px;
        }

        .btn-action-reject {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 11px;
            font-weight: bold;
        }

        /* Modal Background */
        .modal-bg {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.5);
            z-index: 1000;
            align-items: center;
            justify-content: center;
        }

        .modal-content {
            background-color: #fff;
            padding: 25px;
            border-radius: 8px;
            width: 400px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.2);
        }

        .modal-header {
            font-size: 18px;
            font-weight: bold;
            color: #19658A;
            margin-bottom: 15px;
            border-bottom: 1px solid #eee;
            padding-bottom: 10px;
        }

        /* High-Contrast CSS Fix for Select2 */
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
            if ($ddl.hasClass("select2-hidden-accessible")) { $ddl.select2('destroy'); }
            $ddl.select2({ width: '100%' });
        }

        $(document).ready(function () {
            initSelect2();
        });

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function (sender, e) {
                initSelect2();
            });
        }

        window.openActionModal = function (userCode, activityDate, actionType) {
            document.getElementById('<%= hdnTargetUser.ClientID %>').value = userCode;
            document.getElementById('<%= hdnTargetDate.ClientID %>').value = activityDate;
            document.getElementById('<%= hdnActionType.ClientID %>').value = actionType;

            var modalTitle = document.getElementById('modalTitle');
            var timeInputDiv = document.getElementById('divTimeInput'); // Assuming you added this div for Force Checkout

            if (actionType === 'Present') {
                modalTitle.innerText = "Mark as Present";
                modalTitle.style.color = "#28a745";
                if (timeInputDiv) timeInputDiv.style.display = 'none';
            } else if (actionType === 'Absent') {
                modalTitle.innerText = "Mark as Absent";
                modalTitle.style.color = "#dc3545";
                if (timeInputDiv) timeInputDiv.style.display = 'none';
            } else if (actionType === 'ForceOut') {
                modalTitle.innerText = "Force Checkout (Orphaned Punch)";
                modalTitle.style.color = "#fd7e14";
                if (timeInputDiv) timeInputDiv.style.display = 'block';
            }

            document.getElementById('actionModal').style.display = 'flex';
            return false;
        }

        window.closeActionModal = function () {
            document.getElementById('actionModal').style.display = 'none';
            document.getElementById('<%= txtAdminRemarks.ClientID %>').value = '';
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <asp:UpdatePanel ID="upDashboard" runat="server">
        <ContentTemplate>

            <asp:HiddenField ID="hdnTargetUser" runat="server" />
            <asp:HiddenField ID="hdnTargetDate" runat="server" />
            <asp:HiddenField ID="hdnActionType" runat="server" />

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
                        <asp:Button ID="btnExport" runat="server" Text="📥 Export Excel" OnClick="btnExport_Click" Visible="false" Style="background: #28a745; color: white; border: none; padding: 8px 25px; border-radius: 4px; font-weight: bold; cursor: pointer; height: 38px;" />
                    </div>
                </div>

                <div id="SummaryPanel" runat="server" visible="false" style="margin-top: 20px; margin-bottom: 25px;">
                    <h3 style="color: #19658A; margin-bottom: 15px; font-size: 16px; border-bottom: 1px solid #eee; padding-bottom: 5px;">📊 Monthly Payroll Summary
                    </h3>
                    <div style="display: flex; gap: 15px; flex-wrap: wrap;">

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #f8f9fa; border-left: 4px solid #6c757d; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #666; text-transform: uppercase;">Month Days</h4>
                            <asp:Label ID="lblTotalDays" runat="server" Text="0" Font-Bold="true" Font-Size="22px" ForeColor="#333"></asp:Label>
                        </div>

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #e8f5e9; border-left: 4px solid #28a745; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #28a745; text-transform: uppercase;">Payable Days</h4>
                            <asp:Label ID="lblPayableDays" runat="server" Text="0.0" Font-Bold="true" Font-Size="22px" ForeColor="#28a745"></asp:Label>
                        </div>

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #e3f2fd; border-left: 4px solid #19658A; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #19658A; text-transform: uppercase;">Present (Full)</h4>
                            <asp:Label ID="lblPresent" runat="server" Text="0" Font-Bold="true" Font-Size="22px" ForeColor="#19658A"></asp:Label>
                        </div>

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #fff3e0; border-left: 4px solid #ff9800; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #ff9800; text-transform: uppercase;">Half Days</h4>
                            <asp:Label ID="lblHalfDays" runat="server" Text="0" Font-Bold="true" Font-Size="22px" ForeColor="#ff9800"></asp:Label>
                        </div>

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #ffebee; border-left: 4px solid #dc3545; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #dc3545; text-transform: uppercase;">Absent</h4>
                            <asp:Label ID="lblAbsent" runat="server" Text="0" Font-Bold="true" Font-Size="22px" ForeColor="#dc3545"></asp:Label>
                        </div>

                        <div style="flex: 1; min-width: 120px; padding: 15px; border-radius: 8px; background: #f3e5f5; border-left: 4px solid #9c27b0; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                            <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #9c27b0; text-transform: uppercase;">Offs / Holidays</h4>
                            <asp:Label ID="lblOffs" runat="server" Text="0" Font-Bold="true" Font-Size="22px" ForeColor="#9c27b0"></asp:Label>
                        </div>

                    </div>
                </div>

                <div style="overflow-x: auto; padding-bottom: 20px;">
                    <asp:GridView ID="gvOmniAttendance" runat="server" AutoGenerateColumns="False" CssClass="beautiful-grid" GridLines="None" EmptyDataText="No records found for this period." OnRowDataBound="gvOmniAttendance_RowDataBound">
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
                            <asp:BoundField DataField="AttendanceCode" HeaderText="Code" ItemStyle-Font-Bold="true" ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="PayableDay" HeaderText="Payable Day" DataFormatString="{0:0.0}" ItemStyle-HorizontalAlign="Center" />
                            <asp:TemplateField HeaderText="Visits Logged" ItemStyle-HorizontalAlign="Center" ItemStyle-Font-Bold="true">
                                <ItemTemplate>
                                    <%# Convert.ToInt32(Eval("FieldVisitsLogged")) > 0 
                                        ? string.Format("<a href='srch_dailyrpts.aspx?emp={0}&dt={1:yyyy-MM-dd}' target='_blank' style='color:#007bff; text-decoration:underline;' title='Click to view detailed visit logs'>{2}</a>", Eval("UserCode"), Eval("ActivityDate"), Eval("FieldVisitsLogged"))
                                        : "0" 
                                    %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="DailyRevenue" HeaderText="Revenue (₹)" DataFormatString="{0:N2}" NullDisplayText="0.00" ItemStyle-HorizontalAlign="Right" />

                            <asp:TemplateField HeaderText="HR Action">
                                <ItemTemplate>
                                    <asp:Literal ID="litActions" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <div id="actionModal" class="modal-bg">
                <div class="modal-content">
                    <div class="modal-header" id="modalTitle">Process Attendance</div>

                    <div id="divTimeInput" style="display: none; margin-bottom: 15px;">
                        <label style="font-weight: bold; font-size: 13px; color: #555;">Manual Checkout Time:</label>
                        <asp:TextBox ID="txtManualOutTime" runat="server" type="time" CssClass="form-control" Width="100%" Style="margin-top: 5px;"></asp:TextBox>
                    </div>

                    <label style="font-weight: bold; font-size: 13px; color: #555;">Remarks / Reason (Required):</label>
                    <asp:TextBox ID="txtAdminRemarks" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" Width="100%" Style="margin-top: 5px; margin-bottom: 15px; resize: none;"></asp:TextBox>

                    <div style="display: flex; justify-content: flex-end; gap: 10px;">
                        <button type="button" class="btn btn-action-reject btn-secondary" onclick="return closeActionModal();" style="padding: 8px 15px;">Cancel</button>
                        <asp:Button ID="btnConfirmAction" runat="server" Text="Confirm Action" CssClass="btn btn-primary" OnClick="btnConfirmAction_Click" OnClientClient="if(document.getElementById('<%=txtAdminRemarks.ClientID%>').value.trim() == '') { alert('Remarks are required!'); return false; }" Style="padding: 8px 15px; background: #19658A; border: none;" />
                    </div>
                </div>
            </div>

        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnExport" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
