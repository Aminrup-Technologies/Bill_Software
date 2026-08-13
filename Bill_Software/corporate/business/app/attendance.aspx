<%@ Page Title="Daily Attendance" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="attendance.aspx.cs" Inherits="Bill_Software.corporate.business.app.attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script src='https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.js'></script>
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.3/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11.7.3/dist/sweetalert2.all.min.js"></script>

    <style type="text/css">
        .dashboard-container {
            max-width: 1100px; /* Widened slightly to accommodate new columns */
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        .section-card {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

        .section-title {
            color: #19658A;
            margin-top: 0;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 10px;
            margin-bottom: 20px;
            font-size: 20px;
        }

        /* Status & Action Card */
        .status-card {
            background: #ffffff;
            padding: 30px;
            border-radius: 10px;
            text-align: center;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

        .status-badge {
            display: inline-block;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: bold;
            font-size: 16px;
            margin: 15px 0;
        }

        .status-out {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }

        .status-in {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }

        .btn-punch {
            padding: 15px 40px;
            font-size: 18px;
            font-weight: bold;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
            color: white;
            margin: 10px;
        }

            .btn-punch:hover:not(:disabled) {
                transform: translateY(-3px);
                box-shadow: 0 6px 15px rgba(0,0,0,0.2);
            }

            .btn-punch:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }

        .btn-in {
            background: linear-gradient(135deg, #34ce57, #28a745);
        }

        .btn-out {
            background: linear-gradient(135deg, #ff6b6b, #dc3545);
        }

        /* Calendar Styles */
        #attendanceCalendar {
            max-width: 100%;
            margin: 0 auto;
        }

        .fc-event {
            cursor: pointer;
            border: none;
            padding: 2px 4px;
            border-radius: 4px;
            font-weight: bold;
        }

        .fc-toolbar-title {
            color: #19658A;
            font-weight: bold;
        }

        .fc-button-primary {
            background-color: #19658A !important;
            border-color: #19658A !important;
        }

        /* GridView Styling */
        .grid-style {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            font-size: 12px;
        }

            .grid-style th {
                background-color: #19658A;
                color: white;
                padding: 10px;
                text-align: left;
                font-size: 12px;
            }

            .grid-style td {
                padding: 8px;
                border-bottom: 1px solid #eee;
                vertical-align: middle;
            }

            .grid-style tr:hover {
                background-color: #f9f9f9;
            }

        /* Map Fixes */
        .leaflet-container img {
            max-width: none !important;
            max-height: none !important;
            box-shadow: none !important;
        }

        #employeeMap {
            height: 350px !important;
            width: 100% !important;
            position: relative !important;
            display: block !important;
        }

        #employeeMap, #map {
            box-sizing: border-box !important;
            max-width: 100% !important;
            overflow: hidden !important;
            position: relative !important;
            z-index: 1;
        }

        .leaflet-container {
            width: 100% !important;
            max-width: 100% !important;
        }

        .leaflet-map-pane, .leaflet-tile-pane {
            max-width: 100% !important;
        }

        @keyframes popupFade {
            from {
                opacity: 0;
                transform: scale(0.95);
            }

            to {
                opacity: 1;
                transform: scale(1);
            }
        }

        #myBoundaryModal > div {
            animation: popupFade 0.2s ease-out;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">

        <div class="status-card">
            <h2 style="color: #19658A; margin-top: 0;">⏱️ Daily Attendance</h2>
            <asp:Label ID="lblCurrentDate" runat="server" Font-Size="18px" ForeColor="#666666"></asp:Label><br />
            <asp:Label ID="lblAssignedShift" runat="server" Font-Size="14px" Font-Italic="true" ForeColor="#888888"></asp:Label><br />
            <asp:Label ID="lblStatusBadge" runat="server" CssClass="status-badge status-out" Text="Status: Not Punched In"></asp:Label>

            <div style="margin-top: 20px;">
                <asp:Label ID="lblPunchInTime" runat="server" Font-Bold="true" ForeColor="#28a745" Style="display: block; margin-bottom: 5px;"></asp:Label>
                <asp:Label ID="lblPunchOutTime" runat="server" Font-Bold="true" ForeColor="#dc3545" Style="display: block; margin-bottom: 15px;"></asp:Label>
            </div>

            <div>
                <button type="button" id="btnHtmlPunchIn" runat="server" class="btn-punch btn-in" onclick="capturePunch('IN', this);">▶️ Punch IN</button>
                <button type="button" id="btnHtmlPunchOut" runat="server" class="btn-punch btn-out" onclick="capturePunch('OUT', this);">⏹️ Punch OUT</button>
            </div>

            <div style="margin-top: 20px; padding: 12px 15px; background-color: #f8f9fa; border-radius: 6px; border-left: 4px solid #19658A; font-size: 13px; color: #555; text-align: left; display: inline-block; max-width: 450px;">
                <span style="display: block; font-weight: bold; color: #19658A; margin-bottom: 4px;">📍 Location Security Enabled</span>
                Your GPS coordinates are verified automatically. Based on your profile, you may be required to Punch IN/OUT from within the Authorized Office Boundary.
                <br />
                <a href="javascript:void(0);" onclick="showMyBoundary();" style="color: #007bff; font-weight: bold; text-decoration: underline; display: inline-block; margin-top: 5px;">🗺️ View My Authorized Boundary</a>
            </div>

            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Bold="true" Style="display: block; margin-top: 15px;"></asp:Label>
        </div>

        <h3 class="section-title">📊 Your Monthly Payroll Summary (<span id="summaryMonthText"></span>)</h3>
        <div id="SummaryPanel" style="display: flex; gap: 15px; flex-wrap: wrap; margin-bottom: 25px;">
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #f8f9fa; border-left: 4px solid #6c757d; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #666; text-transform: uppercase;">Month Days</h4>
                <span id="lblTotalDays" style="font-weight: bold; font-size: 22px; color: #333;">0</span>
            </div>
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #e8f5e9; border-left: 4px solid #28a745; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #28a745; text-transform: uppercase;">Payable Days</h4>
                <span id="lblPayableDays" style="font-weight: bold; font-size: 22px; color: #28a745;">0.0</span>
            </div>
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #e3f2fd; border-left: 4px solid #19658A; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #19658A; text-transform: uppercase;">Present</h4>
                <span id="lblPresent" style="font-weight: bold; font-size: 22px; color: #19658A;">0</span>
            </div>
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #fff3e0; border-left: 4px solid #ff9800; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #ff9800; text-transform: uppercase;">Half Days</h4>
                <span id="lblHalfDays" style="font-weight: bold; font-size: 22px; color: #ff9800;">0</span>
            </div>
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #ffebee; border-left: 4px solid #dc3545; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #dc3545; text-transform: uppercase;">Absent</h4>
                <span id="lblAbsent" style="font-weight: bold; font-size: 22px; color: #dc3545;">0</span>
            </div>
            <div style="flex: 1; min-width: 100px; padding: 15px; border-radius: 8px; background: #f3e5f5; border-left: 4px solid #9c27b0; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
                <h4 style="margin: 0 0 5px 0; font-size: 11px; color: #9c27b0; text-transform: uppercase;">Offs / Holidays</h4>
                <span id="lblOffs" style="font-weight: bold; font-size: 22px; color: #9c27b0;">0</span>
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-title">🗓️ Monthly Visual Calendar</h3>
            <div id="attendanceCalendar"></div>
        </div>

        <div style="display: flex; gap: 20px; flex-wrap: wrap; margin-bottom: 30px;">

            <div class="section-card" style="flex: 2; min-width: 600px; overflow-x: auto;">
                <h3 class="section-title">📅 Detailed Attendance Log</h3>
                <%--<asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="False" CssClass="grid-style" EmptyDataText="No attendance records found." GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="DayOfWeek" HeaderText="Day" />
                        
                        <asp:BoundField DataField="CalculatedStatus" HeaderText="Daily Status" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="AttendanceCode" HeaderText="Code" ItemStyle-Font-Bold="true" ItemStyle-HorizontalAlign="Center" />
                        <asp:BoundField DataField="PayableDay" HeaderText="Payable" DataFormatString="{0:0.0}" ItemStyle-HorizontalAlign="Center" ItemStyle-ForeColor="#28a745" ItemStyle-Font-Bold="true" />
                        
                        <asp:TemplateField HeaderText="IN Time">
                            <ItemTemplate><%# Eval("PunchInTime") != DBNull.Value ? Convert.ToDateTime(Eval("PunchInTime")).ToString("hh:mm tt") : "-" %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="OUT Time">
                            <ItemTemplate><%# Eval("PunchOutTime") != DBNull.Value ? Convert.ToDateTime(Eval("PunchOutTime")).ToString("hh:mm tt") : "-" %></ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="TotalHoursWorked" HeaderText="Total Hrs" DataFormatString="{0:F2}" NullDisplayText="-" />
                        <asp:BoundField DataField="LateByMins" HeaderText="Late" ItemStyle-ForeColor="#dc3545" />
                        <asp:BoundField DataField="EarlyOutByMins" HeaderText="Early" ItemStyle-ForeColor="#ff9800" />
                        <asp:BoundField DataField="OvertimeMins" HeaderText="OT" ItemStyle-ForeColor="#28a745" />

                        <asp:TemplateField HeaderText="Map">
                            <ItemTemplate>
                                <button type="button" onclick="viewAttendanceMap('<%# Eval("AttendanceID") %>');" style="background-color: #17a2b8; color: white; border: none; padding: 4px 8px; border-radius: 4px; cursor: pointer; font-size:11px;">
                                    📍 View
                                </button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>--%>
                <table class="grid-style" id="tblHistory">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Day</th>
                            <th>Daily Status</th>
                            <th style="text-align: center;">Code</th>
                            <th style="text-align: center;">Payable</th>
                            <th>IN Time</th>
                            <th>OUT Time</th>
                            <th>Total Hrs</th>
                            <th>Late</th>
                            <th>Early</th>
                            <th>OT</th>
                            <th>Map</th>
                        </tr>
                    </thead>
                    <tbody id="historyBody">
                    </tbody>
                </table>
            </div>

            <div class="section-card" style="flex: 1; min-width: 350px;">
                <h3 class="section-title">📝 My Correction Requests</h3>
                <asp:GridView ID="gvRegularizations" runat="server" AutoGenerateColumns="False" CssClass="grid-style" EmptyDataText="No regularization requests submitted." GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="AppliedOn" HeaderText="Applied" DataFormatString="{0:dd-MMM}" />
                        <asp:BoundField DataField="AttendanceDate" HeaderText="For Date" DataFormatString="{0:dd-MMM}" ItemStyle-Font-Bold="true" />
                        <asp:TemplateField HeaderText="Req. Times">
                            <ItemTemplate>
                                IN: <%# Eval("RequestedInTime") %><br />
                                OUT: <%# Eval("RequestedOutTime") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span style='<%# GetStatusColor(Eval("RequestStatus").ToString()) %>'>
                                    <%# Eval("RequestStatus") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

        </div>
    </div>

    <div id="dateDetailsModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99998;">
        <div style="background: #fff; width: 90%; max-width: 500px; margin: 8% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: Arial, sans-serif;">

            <div style="background-color: #19658A; color: white; padding: 15px; font-weight: bold; font-size: 16px; display: flex; justify-content: space-between;">
                <span>📅 Action for: <span id="modalDateHeader"></span></span>
                <span style="cursor: pointer;" onclick="document.getElementById('dateDetailsModal').style.display='none';">✖</span>
            </div>

            <div style="padding: 25px; line-height: 1.6; max-height: 70vh; overflow-y: auto;">

                <input type="hidden" id="hfClickedDate" />

                <div id="modalExistingRecordInfo" style="display: none;">
                    <p><b>Status:</b> <span id="modalEventStatus" style="font-weight: bold;"></span></p>
                    <p><b>Punch IN:</b> <span id="modalPunchIn" style="color: #28a745; font-weight: bold;"></span></p>
                    <p><b>Punch OUT:</b> <span id="modalPunchOut" style="color: #dc3545; font-weight: bold;"></span></p>

                    <button type="button" id="btnViewMapFromModal" style="margin-top: 15px; background-color: #17a2b8; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; width: 100%; font-weight: bold;">
                        📍 View Map Details
                    </button>

                    <hr style="border: 0; border-top: 1px solid #eee; margin: 20px 0;" />
                    <p style="font-size: 13px; color: #666; text-align: center;">Forgot to punch out? Need to correct this?</p>
                    <button type="button" onclick="toggleForm('regForm');" style="background-color: #ffc107; color: #333; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; width: 100%; font-weight: bold;">
                        📝 Regularize Attendance
                    </button>
                </div>

                <div id="modalEmptyRecordInfo" style="display: none; text-align: center;">
                    <p style="color: #666; font-style: italic; margin-bottom: 20px;">No attendance data found for this date.</p>

                    <div style="display: flex; gap: 10px;">
                        <button type="button" onclick="toggleForm('leaveForm');" style="flex: 1; background-color: #28a745; color: white; border: none; padding: 10px; border-radius: 4px; cursor: pointer; font-weight: bold;">
                            🏖️ Apply Leave
                        </button>
                        <button type="button" onclick="toggleForm('regForm');" style="flex: 1; background-color: #ffc107; color: #333; border: none; padding: 10px; border-radius: 4px; cursor: pointer; font-weight: bold;">
                            📝 Regularize
                        </button>
                    </div>
                </div>

                <div id="regForm" style="display: none; margin-top: 20px; background: #f9f9f9; padding: 15px; border-radius: 6px; border: 1px solid #eee;">
                    <h4 style="margin-top: 0; color: #19658A;">Submit Regularization</h4>

                    <label style="display: block; font-weight: bold; margin-top: 10px; font-size: 13px;">Correct IN Time:</label>
                    <input type="time" id="txtRegIn" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;" />

                    <label style="display: block; font-weight: bold; margin-top: 10px; font-size: 13px;">Correct OUT Time:</label>
                    <input type="time" id="txtRegOut" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;" />

                    <label style="display: block; font-weight: bold; margin-top: 10px; font-size: 13px;">Reason for Correction:</label>
                    <textarea id="txtRegReason" rows="2" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;" placeholder="e.g., Machine not working, Client visit..."></textarea>

                    <button type="button" onclick="submitRegularization();" style="margin-top: 15px; background-color: #19658A; color: white; border: none; padding: 10px; border-radius: 4px; cursor: pointer; width: 100%; font-weight: bold;">
                        Submit for Approval
                    </button>
                </div>

                <div id="leaveForm" style="display: none; margin-top: 20px; background: #f9f9f9; padding: 15px; border-radius: 6px; border: 1px solid #eee;">
                    <h4 style="margin-top: 0; color: #19658A;">Apply for Leave</h4>

                    <label style="display: block; font-weight: bold; margin-top: 10px; font-size: 13px;">Leave Type:</label>
                    <select id="ddlLeaveTypes" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;">
                        <option value="">-- Select Leave Type --</option>
                    </select>

                    <label style="display: block; font-weight: bold; margin-top: 10px; font-size: 13px;">Reason:</label>
                    <textarea id="txtLeaveReason" rows="2" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box;" placeholder="e.g., Medical appointment, Personal work..."></textarea>

                    <button type="button" onclick="submitLeave();" style="margin-top: 15px; background-color: #28a745; color: white; border: none; padding: 10px; border-radius: 4px; cursor: pointer; width: 100%; font-weight: bold;">
                        Submit Leave Application
                    </button>
                </div>

            </div>
        </div>
    </div>

    <div id="attendanceMapModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99999;">
        <div style="background: #fff; width: 90%; max-width: 800px; margin: 5% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: Arial, sans-serif; display: flex; flex-direction: column;">
            <div style="background-color: #19658A; color: white; padding: 15px; font-weight: bold; font-size: 16px;">
                📍 Attendance Location Details
            </div>
            <div style="padding: 20px; line-height: 1.6; overflow-y: auto;">
                <p style="font-size: 16px; border-bottom: 1px solid #eee; padding-bottom: 10px;"><b>Date:</b> <span id="lblMapDate" style="color: #19658A;"></span></p>
                <div style="display: flex; flex-wrap: wrap; gap: 20px; margin-top: 15px;">
                    <div style="flex: 1; min-width: 300px;">
                        <b style="color: #28a745; font-size: 15px;">▶️ Punch IN Location (<span id="lblMapInTime"></span>)</b>
                        <div id="mapContainerIn" style="border: 2px solid #eaeaea; border-radius: 8px; height: 260px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; margin-top: 8px; overflow: hidden;">
                            <span style='color: #888; font-style: italic;'>Loading map...</span>
                        </div>
                    </div>
                    <div style="flex: 1; min-width: 300px;">
                        <b style="color: #dc3545; font-size: 15px;">⏹️ Punch OUT Location (<span id="lblMapOutTime"></span>)</b>
                        <div id="mapContainerOut" style="border: 2px solid #eaeaea; border-radius: 8px; height: 260px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; margin-top: 8px; overflow: hidden;">
                            <span style='color: #888; font-style: italic;'>Loading map...</span>
                        </div>
                    </div>
                </div>
            </div>
            <div style="text-align: right; padding: 15px 20px; border-top: 1px solid #eee; background-color: #fcfcfc;">
                <button type="button" onclick="document.getElementById('attendanceMapModal').style.display='none';" style="background-color: #6c757d; color: white; padding: 8px 20px; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;">
                    Close
                </button>
            </div>
        </div>
    </div>

    <div id="myBoundaryModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99999;">
        <div style="background: #fff; width: 90%; max-width: 600px; margin: 5% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: Arial, sans-serif;">
            <div style="background-color: #19658A; color: white; padding: 15px; font-weight: bold; font-size: 16px; display: flex; justify-content: space-between;">
                <span>🗺️ My Authorized Location Boundary</span>
                <span style="cursor: pointer;" onclick="document.getElementById('myBoundaryModal').style.display='none';">✖</span>
            </div>
            <div style="padding: 15px;">
                <p style="font-size: 13px; color: #555; margin-top: 0;">You must be inside the blue circle to punch your attendance successfully.</p>
                <div id="employeeMap" style="height: 350px; width: 100%; border: 1px solid #ccc; border-radius: 4px;"></div>
            </div>
        </div>
    </div>

    <script type="text/javascript">

        const Toast = Swal.mixin({
            toast: true, position: 'top-end', showConfirmButton: false, timer: 4000, timerProgressBar: true,
            didOpen: (toast) => { toast.addEventListener('mouseenter', Swal.stopTimer); toast.addEventListener('mouseleave', Swal.resumeTimer); }
        });

        function showNotification(title, text, type) {
            if (type === 'notice') type = 'warning';
            Toast.fire({ icon: type, title: title, text: text });
        }

        // 1. Capture GPS for Punch IN/OUT
        function capturePunch(actionType, btnElement) {
            var originalText = btnElement.innerText;
            var errorLabel = document.getElementById('<%= lblError.ClientID %>');

            btnElement.innerText = "📍 Verifying Location...";
            btnElement.disabled = true;
            if (errorLabel) errorLabel.innerText = "";

            if (navigator.geolocation) {
                // Force hardware GPS over cellular triangulation; never reuse a cached fix
                var geoOptions = { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 };

                navigator.geolocation.getCurrentPosition(
                    function (position) {
                        fetch('attendance.aspx/ProcessPunch', {
                            method: 'POST', headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ action: actionType, lat: position.coords.latitude, lng: position.coords.longitude, address: "" })
                        })
                        .then(res => res.json())
                        .then(data => {
                            var response = JSON.parse(data.d);
                            if (response.status === "success") {
                                showNotification("Attendance Marked", response.message, "success");
                                setTimeout(function () { location.reload(); }, 1500);
                            } else {
                                showNotification("Punch Failed", response.message, "error");
                                if (errorLabel) errorLabel.innerText = "❌ " + response.message;
                                btnElement.innerText = originalText;
                                btnElement.disabled = false;
                            }
                        })
                        .catch(err => {
                            showNotification("Network Error", "Unable to connect to server.", "error");
                            btnElement.innerText = originalText;
                            btnElement.disabled = false;
                        });
                    },
                    function (error) {
                        var errorMsg = "Geolocation failed. Please allow location access in your browser.";
                        showNotification("Location Required", errorMsg, "warning");
                        if (errorLabel) errorLabel.innerText = "⚠️ " + errorMsg;
                        btnElement.innerText = originalText;
                        btnElement.disabled = false;
                    },
                    geoOptions
                );
            } else {
                showNotification("Unsupported Browser", "Geolocation is not supported by your browser.", "error");
                btnElement.innerText = originalText;
                btnElement.disabled = false;
            }
        }

        // 2. Fetch and Show Maps in Modal
        function viewAttendanceMap(id) {
            fetch('attendance.aspx/GetAttendanceDetails', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ id: parseInt(id) })
            })
            .then(response => response.json())
            .then(data => {
                var details = JSON.parse(data.d);

                document.getElementById('lblMapDate').innerText = details.Date;
                document.getElementById('lblMapInTime').innerText = details.InTime;
                document.getElementById('lblMapOutTime').innerText = details.OutTime;

                var mapIn = document.getElementById('mapContainerIn');
                if (details.InLat && details.InLon && details.InLat !== "") {
                    var urlIn = "https://maps.google.com/maps?q=" + details.InLat + "," + details.InLon + "&hl=en&z=15&output=embed";
                    mapIn.innerHTML = "<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='" + urlIn + "'></iframe>";
                } else { mapIn.innerHTML = "<span style='color: #888; font-style: italic;'>Location not captured.</span>"; }

                var mapOut = document.getElementById('mapContainerOut');
                if (details.OutLat && details.OutLon && details.OutLat !== "") {
                    var urlOut = "https://maps.google.com/maps?q=" + details.OutLat + "," + details.OutLon + "&hl=en&z=15&output=embed";
                    mapOut.innerHTML = "<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='" + urlOut + "'></iframe>";
                } else { mapOut.innerHTML = "<span style='color: #888; font-style: italic;'>Location not captured.</span>"; }

                document.getElementById('attendanceMapModal').style.display = 'block';
            })
            .catch(error => console.error('Error fetching details:', error));
        }

        document.addEventListener('DOMContentLoaded', function () {

            // Pre-load Leave Types
            fetch('attendance.aspx/GetActiveLeaveTypes', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' }
            })
            .then(response => response.json())
            .then(data => {
                var leaves = JSON.parse(data.d);
                var ddl = document.getElementById('ddlLeaveTypes');
                leaves.forEach(function (leave) {
                    var option = document.createElement("option");
                    option.value = leave.id;
                    option.text = leave.name;
                    ddl.appendChild(option);
                });
            })
            .catch(err => console.error("Error loading leaves:", err));

            // Calendar
            var calendarEl = document.getElementById('attendanceCalendar');
            var calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,listMonth' },
                height: 'auto',
                firstDay: 1,
                //events: function (fetchInfo, successCallback, failureCallback) {
                //    fetch('attendance.aspx/GetCalendarData', {
                //        method: 'POST',
                //        headers: { 'Content-Type': 'application/json; charset=utf-8' },
                //        body: "{}"
                //    })
                //    .then(response => response.json())
                //    .then(data => {
                //        // FIX: Safely check if ASP.NET returned data OR an Error Message
                //        if (data.d !== undefined) {
                //            successCallback(JSON.parse(data.d));
                //        } else if (data.Message) {
                //            console.error("ASP.NET Backend Error:", data.Message);
                //            showNotification("Calendar Sync Failed", data.Message, "error");
                //            failureCallback();
                //        } else {
                //            failureCallback();
                //        }
                //    })
                //    .catch(error => {
                //        console.error("Fetch crash:", error);
                //        failureCallback(error);
                //    });
                //},
                events: function (fetchInfo, successCallback, failureCallback) {
                    // FIX: Use fetchInfo instead of the 'calendar' variable to prevent initial load crashes
                    // We find the middle date of the rendered calendar grid to guarantee we get the correct month
                    var midDate = new Date((fetchInfo.start.getTime() + fetchInfo.end.getTime()) / 2);
                    var m = midDate.getMonth() + 1;
                    var y = midDate.getFullYear();

                    // Update Title Text dynamically
                    document.getElementById('summaryMonthText').innerText = midDate.toLocaleString('default', { month: 'long', year: 'numeric' });

                    fetch('attendance.aspx/GetMonthlyData', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json; charset=utf-8' },
                        body: JSON.stringify({ month: m, year: y })
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.d) {
                            var payload = JSON.parse(data.d);

                            // 1. Update the Summary Cards Instantly!
                            document.getElementById('lblTotalDays').innerText = payload.Summary.TotalDays;
                            document.getElementById('lblPayableDays').innerText = payload.Summary.PayableDays.toFixed(1);
                            document.getElementById('lblPresent').innerText = payload.Summary.Present;
                            document.getElementById('lblHalfDays').innerText = payload.Summary.HalfDays;
                            document.getElementById('lblAbsent').innerText = payload.Summary.Absent;
                            document.getElementById('lblOffs').innerText = payload.Summary.Offs;

                            // 2. Build the HTML Grid Instantly!
                            var tbody = document.getElementById('historyBody');
                            tbody.innerHTML = '';
                            if (payload.Grid.length === 0) {
                                tbody.innerHTML = "<tr><td colspan='12' style='text-align:center;'>No records found.</td></tr>";
                            } else {
                                payload.Grid.forEach(row => {
                                    var bgColor = ''; var statusColor = '#333';
                                    if (row.Status.includes('Absent')) { bgColor = '#ffebee'; statusColor = 'darkred'; }
                                    else if (row.Status.includes('Leave')) { bgColor = '#f3e5f5'; statusColor = 'purple'; }
                                    else if (row.Status.includes('Working')) { bgColor = '#e8f5e9'; statusColor = '#155724'; }
                                    else if (row.Status.includes('Upcoming') || row.Status.includes('Off')) { statusColor = 'gray'; }

                                    var mapBtn = row.Id ? `<button type='button' onclick="viewAttendanceMap('${row.Id}');" style='background-color:#17a2b8; color:white; border:none; padding:4px 8px; border-radius:4px; cursor:pointer; font-size:11px;'>📍 View</button>` : '';

                                    tbody.innerHTML += `
                                        <tr style='background-color:${bgColor}'>
                                            <td><b>${row.Date}</b></td>
                                            <td>${row.Day}</td>
                                            <td style='color:${statusColor}; font-weight:bold;'>${row.Status}</td>
                                            <td style='text-align:center; font-weight:bold;'>${row.Code}</td>
                                            <td style='text-align:center; color:#28a745; font-weight:bold;'>${row.Payable}</td>
                                            <td>${row.In}</td>
                                            <td>${row.Out}</td>
                                            <td>${row.Hrs}</td>
                                            <td style='color:#dc3545'>${row.Late}</td>
                                            <td style='color:#ff9800'>${row.Early}</td>
                                            <td style='color:#28a745'>${row.OT}</td>
                                            <td>${mapBtn}</td>
                                        </tr>`;
                                });
                            }

                            // 3. Render Calendar
                            successCallback(payload.Events);
                        } else {
                            failureCallback();
                        }
                    })
                    .catch(error => failureCallback(error));
                },
                eventContent: function (arg) {
                    let arrayOfDomNodes = [];
                    let titleEl = document.createElement('div');
                    titleEl.innerHTML = '<span class="calendar-tag" style="background:' + arg.event.backgroundColor + '; padding: 3px 6px; border-radius: 4px; color: white; font-size: 11px; display: block; text-align: center; white-space: normal; line-height: 1.2;">' + arg.event.title + '</span>';
                    arrayOfDomNodes.push(titleEl);
                    return { domNodes: arrayOfDomNodes };
                },
                eventClick: function (info) {
                    var clickedDate = info.event.start;
                    var dateStr = info.event.startStr;

                    hideForms();
                    document.getElementById('hfClickedDate').value = dateStr;
                    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                    // Load Rich Tooltip Data into Modal
                    document.getElementById('modalEventStatus').innerHTML = info.event.extendedProps.description || info.event.title;

                    document.getElementById('modalPunchIn').innerText = "-";
                    document.getElementById('modalPunchOut').innerText = "-";

                    prefillShiftTimings(dateStr);

                    document.getElementById('btnViewMapFromModal').onclick = function () {
                        document.getElementById('dateDetailsModal').style.display = 'none';
                        if (info.event.id) viewAttendanceMap(info.event.id);
                    };

                    document.getElementById('modalExistingRecordInfo').style.display = 'block';
                    document.getElementById('modalEmptyRecordInfo').style.display = 'none';
                    document.getElementById('dateDetailsModal').style.display = 'block';
                },
                dateClick: function (info) {
                    var clickedDate = new Date(info.dateStr);
                    if (clickedDate > new Date()) return;

                    hideForms();
                    document.getElementById('hfClickedDate').value = info.dateStr;
                    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                    prefillShiftTimings(info.dateStr);

                    document.getElementById('modalExistingRecordInfo').style.display = 'none';
                    document.getElementById('modalEmptyRecordInfo').style.display = 'block';
                    document.getElementById('dateDetailsModal').style.display = 'block';
                }
            });

            calendar.render();
        });

        function hideForms() {
            document.getElementById('regForm').style.display = 'none';
            document.getElementById('leaveForm').style.display = 'none';
            document.getElementById('txtRegReason').value = '';
            document.getElementById('ddlLeaveTypes').selectedIndex = 0;
            document.getElementById('txtLeaveReason').value = '';
        }

        function toggleForm(formId) {
            hideForms();
            document.getElementById(formId).style.display = 'block';
        }

        function prefillShiftTimings(dateStr) {
            document.getElementById('txtRegIn').value = "";
            document.getElementById('txtRegOut').value = "";
            fetch('attendance.aspx/GetShiftTimings', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: dateStr })
            })
            .then(response => response.json())
            .then(data => {
                var timings = JSON.parse(data.d);
                if (timings.InTime) document.getElementById('txtRegIn').value = timings.InTime;
                if (timings.OutTime) document.getElementById('txtRegOut').value = timings.OutTime;
            }).catch(error => console.error(error));
        }

        function submitRegularization() {
            var date = document.getElementById('hfClickedDate').value;
            var inTime = document.getElementById('txtRegIn').value;
            var outTime = document.getElementById('txtRegOut').value;
            var reason = document.getElementById('txtRegReason').value;

            if (!reason) { showNotification("Missing Information", "Please provide a reason.", "warning"); return; }

            fetch('attendance.aspx/SubmitRegularization', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: date, inTime: inTime, outTime: outTime, reason: reason })
            })
            .then(response => response.json())
            .then(data => {
                if (data.d === "Success") {
                    showNotification("Request Sent", "Regularization request submitted!", "success");
                    document.getElementById('dateDetailsModal').style.display = 'none';
                    setTimeout(function () { location.reload(); }, 1500);
                } else { showNotification("Submission Failed", data.d, "error"); }
            }).catch(err => showNotification("Network Error", "Communication error.", "error"));
        }

        function submitLeave() {
            var date = document.getElementById('hfClickedDate').value;
            var leaveId = document.getElementById('ddlLeaveTypes').value;
            var reason = document.getElementById('txtLeaveReason').value;

            if (!leaveId || !reason) { showNotification("Missing Info", "Select Leave Type and reason.", "warning"); return; }

            fetch('attendance.aspx/SubmitLeave', {
                method: 'POST', headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: date, leaveId: parseInt(leaveId), reason: reason })
            })
            .then(response => response.json())
            .then(data => {
                if (data.d === "Success") {
                    showNotification("Leave Applied", "Leave application submitted!", "success");
                    document.getElementById('dateDetailsModal').style.display = 'none';
                    setTimeout(function () { location.reload(); }, 1500);
                } else { showNotification("Submission Failed", data.d, "error"); }
            }).catch(err => showNotification("Network Error", "Communication error.", "error"));
        }

        var empMap, empMarker, empCircle;
        function showMyBoundary() {
            fetch('attendance.aspx/GetMyGeoFence', { method: 'POST', headers: { 'Content-Type': 'application/json' } })
            .then(res => res.json())
            .then(data => {
                var geoData = JSON.parse(data.d);
                if (!geoData.Required) { showNotification("Not Required", "Geo-Fencing is disabled for your account.", "info"); return; }
                if (!geoData.Lat || !geoData.Lng) { showNotification("Not Configured", "Your office location is not set.", "warning"); return; }

                var lat = parseFloat(geoData.Lat); var lng = parseFloat(geoData.Lng); var radius = parseInt(geoData.Radius);

                document.getElementById('myBoundaryModal').style.display = 'block';
                setTimeout(function () {
                    if (!empMap) {
                        empMap = L.map('employeeMap');
                        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '© OSMap' }).addTo(empMap);
                    }
                    if (empMarker) empMap.removeLayer(empMarker);
                    if (empCircle) empMap.removeLayer(empCircle);

                    empMarker = L.marker([lat, lng]).addTo(empMap);
                    empCircle = L.circle([lat, lng], { radius: radius, color: '#19658A', fillColor: '#19658A', fillOpacity: 0.25, weight: 2 }).addTo(empMap);
                    empMap.setView([lat, lng], 17);

                    empMap.invalidateSize(true);
                    window.dispatchEvent(new Event('resize'));
                }, 350);
            }).catch(err => showNotification("Data Error", "Could not load location data.", "error"));
        }
    </script>
</asp:Content>
