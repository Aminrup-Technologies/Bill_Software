<%@ Page Title="Daily Attendance" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="attendance.aspx.cs" Inherits="Bill_Software.corporate.business.app.attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src='https://cdn.jsdelivr.net/npm/fullcalendar@6.1.11/index.global.min.js'></script>

    <style type="text/css">
        /* Keep your existing styles here */

        /* New Calendar Specific Styles */
        .calendar-section {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

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
    </style>
    <style type="text/css">
        .dashboard-container {
            max-width: 950px;
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

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

        .status-completed {
            background-color: #e2e3e5;
            color: #383d41;
            border: 1px solid #d6d8db;
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

        .history-section {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
        }

        /* GridView Styling */
        .grid-style {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
            font-size: 13px;
        }

            .grid-style th {
                background-color: #19658A;
                color: white;
                padding: 12px;
                text-align: left;
            }

            .grid-style td {
                padding: 5px 6px;
                border-bottom: 1px solid #eee;
                vertical-align: middle;
            }

            .grid-style tr:hover {
                background-color: #f9f9f9;
            }
    </style>

    <script type="text/javascript">
        // 1. Capture GPS for Punch IN/OUT
        function capturePunch(actionType, btnElement) {
            if (navigator.geolocation) {
                var originalText = btnElement.innerText;
                btnElement.innerText = "📍 Acquiring GPS...";
                btnElement.disabled = true;

                navigator.geolocation.getCurrentPosition(
                    function (position) {
                        document.getElementById('<%= hfLatitude.ClientID %>').value = position.coords.latitude;
                        document.getElementById('<%= hfLongitude.ClientID %>').value = position.coords.longitude;
                        document.getElementById('<%= hfPunchAction.ClientID %>').value = actionType;
                        document.getElementById('<%= btnProcessServerPunch.ClientID %>').click();
                    },
                    function (error) {
                        alert("Geolocation failed: " + error.message + ".\n\nYou must allow location access to mark attendance.");
                        btnElement.innerText = originalText;
                        btnElement.disabled = false;
                    },
                    { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
                );
            } else {
                alert("Geolocation is not supported by your browser.");
            }
        }

        // 2. Fetch and Show Maps in Modal
        function viewAttendanceMap(id) {
            fetch('attendance.aspx/GetAttendanceDetails', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
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
                } else {
                    mapIn.innerHTML = "<span style='color: #888; font-style: italic;'>Location not captured.</span>";
                }

                var mapOut = document.getElementById('mapContainerOut');
                if (details.OutLat && details.OutLon && details.OutLat !== "") {
                    var urlOut = "https://maps.google.com/maps?q=" + details.OutLat + "," + details.OutLon + "&hl=en&z=15&output=embed";
                    mapOut.innerHTML = "<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='" + urlOut + "'></iframe>";
                } else {
                    mapOut.innerHTML = "<span style='color: #888; font-style: italic;'>Location not captured.</span>";
                }

                document.getElementById('attendanceMapModal').style.display = 'block';
            })
            .catch(error => console.error('Error fetching details:', error));
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">

        <asp:HiddenField ID="hfLatitude" runat="server" />
        <asp:HiddenField ID="hfLongitude" runat="server" />
        <asp:HiddenField ID="hfPunchAction" runat="server" />
        <asp:Button ID="btnProcessServerPunch" runat="server" OnClick="btnProcessServerPunch_Click" Style="display: none;" />

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

            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Bold="true" Style="display: block; margin-top: 15px;"></asp:Label>
        </div>

        <div class="calendar-section">
            <h3 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px;">🗓️ Monthly View</h3>
            <div id="attendanceCalendar"></div>
        </div>

        <div style="display: flex; gap: 15px; flex-wrap: wrap; margin-bottom: 30px;">
            
            <div class="history-section" style="flex: 2; min-width: 400px;">
                <h3 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; padding-bottom: 5px;">📅 My Recent Attendance</h3>
                <asp:GridView ID="gvAttendanceHistory" runat="server" AutoGenerateColumns="False" CssClass="grid-style" EmptyDataText="No attendance records found." GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="PunchInTime" HeaderText="IN Time" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" />
                        <asp:BoundField DataField="PunchOutTime" HeaderText="OUT Time" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" />
                        <asp:BoundField DataField="TotalHoursWorked" HeaderText="Total Hrs" DataFormatString="{0:F2}" NullDisplayText="-" />
                        <asp:BoundField DataField="LateByMins" HeaderText="Late (Mins)" NullDisplayText="0" />
                        <asp:BoundField DataField="SystemCalculatedStatus" HeaderText="Status" ItemStyle-Font-Bold="true" />
                        
                        <asp:TemplateField HeaderText="Location">
                            <ItemTemplate>
                                <button type="button" onclick="viewAttendanceMap('<%# Eval("Id") %>');" style="background-color: #17a2b8; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-weight: bold;">
                                    📍 Map
                                </button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <div class="history-section" style="flex: 1.5; min-width: 350px;">
                <h3 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; padding-bottom: 5px;">📝 My Correction Requests</h3>
                <asp:GridView ID="gvRegHistory" runat="server" AutoGenerateColumns="False" CssClass="grid-style" EmptyDataText="No regularization requests submitted." GridLines="None">
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

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {

            // --- NEW: Pre-load Leave Types on page load ---
            fetch('attendance.aspx/GetActiveLeaveTypes', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' }
            })
            .then(response => response.json())
            .then(data => {
                var leaves = JSON.parse(data.d);
                var ddl = document.getElementById('ddlLeaveTypes');
                leaves.forEach(function (leave) {
                    var option = document.createElement("option");
                    option.value = leave.ID;
                    option.text = leave.Name;
                    ddl.appendChild(option);
                });
            });
            // ----------------------------------------------


            var calendarEl = document.getElementById('attendanceCalendar');

            var calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'dayGridMonth,listMonth'
                },
                height: 'auto',
                firstDay: 1,

                events: function (fetchInfo, successCallback, failureCallback) {
                    var midDate = new Date((fetchInfo.start.getTime() + fetchInfo.end.getTime()) / 2);
                    var queryMonth = midDate.getMonth() + 1;
                    var queryYear = midDate.getFullYear();

                    fetch('attendance.aspx/GetMonthlyCalendarData', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json; charset=utf-8' },
                        body: JSON.stringify({ month: queryMonth, year: queryYear })
                    })
                    .then(response => response.json())
                    .then(data => {
                        var parsedEvents = JSON.parse(data.d);
                        successCallback(parsedEvents);
                    })
                    .catch(error => failureCallback(error));
                },

                // Modified Event Click
                //eventClick: function (info) {
                //    var recordId = info.event.id;
                //    var clickedDate = info.event.start;

                //    // Reset modal state
                //    hideForms();
                //    document.getElementById('hfClickedDate').value = info.event.startStr;
                //    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                //    document.getElementById('modalEventStatus').innerText = info.event.title;
                //    document.getElementById('modalEventStatus').style.color = info.event.backgroundColor;

                //    fetch('attendance.aspx/GetAttendanceDetails', {
                //        method: 'POST',
                //        headers: { 'Content-Type': 'application/json; charset=utf-8' },
                //        body: JSON.stringify({ id: parseInt(recordId) })
                //    })
                //    .then(response => response.json())
                //    .then(data => {
                //        var details = JSON.parse(data.d);
                //        document.getElementById('modalPunchIn').innerText = details.InTime;
                //        document.getElementById('modalPunchOut').innerText = details.OutTime;

                //        document.getElementById('btnViewMapFromModal').onclick = function () {
                //            document.getElementById('dateDetailsModal').style.display = 'none';
                //            viewAttendanceMap(recordId);
                //        };

                //        document.getElementById('modalExistingRecordInfo').style.display = 'block';
                //        document.getElementById('modalEmptyRecordInfo').style.display = 'none';
                //        document.getElementById('dateDetailsModal').style.display = 'block';
                //    });
                //},

                //// Modified Date Click
                //dateClick: function (info) {
                //    var clickedDate = new Date(info.dateStr);
                //    if (clickedDate > new Date()) return; // Prevent clicking future dates for now

                //    // Reset modal state
                //    hideForms();
                //    document.getElementById('hfClickedDate').value = info.dateStr;
                //    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                //    document.getElementById('modalExistingRecordInfo').style.display = 'none';
                //    document.getElementById('modalEmptyRecordInfo').style.display = 'block';
                //    document.getElementById('dateDetailsModal').style.display = 'block';
                //}

                eventClick: function (info) {
                    var recordId = info.event.id;
                    var clickedDate = info.event.start;
                    var dateStr = info.event.startStr; // Get the YYYY-MM-DD string

                    // Reset modal state
                    hideForms();
                    document.getElementById('hfClickedDate').value = dateStr;
                    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                    document.getElementById('modalEventStatus').innerText = info.event.title;
                    document.getElementById('modalEventStatus').style.color = info.event.backgroundColor;

                    // NEW: Prefill the form inputs with expected shift timings
                    prefillShiftTimings(dateStr);

                    fetch('attendance.aspx/GetAttendanceDetails', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json; charset=utf-8' },
                        body: JSON.stringify({ id: parseInt(recordId) })
                    })
                    .then(response => response.json())
                    .then(data => {
                        var details = JSON.parse(data.d);
                        document.getElementById('modalPunchIn').innerText = details.InTime;
                        document.getElementById('modalPunchOut').innerText = details.OutTime;

                        document.getElementById('btnViewMapFromModal').onclick = function () {
                            document.getElementById('dateDetailsModal').style.display = 'none';
                            viewAttendanceMap(recordId);
                        };

                        document.getElementById('modalExistingRecordInfo').style.display = 'block';
                        document.getElementById('modalEmptyRecordInfo').style.display = 'none';
                        document.getElementById('dateDetailsModal').style.display = 'block';
                    });
                },

                dateClick: function (info) {
                    var clickedDate = new Date(info.dateStr);
                    if (clickedDate > new Date()) return; // Prevent clicking future dates

                    // Reset modal state
                    hideForms();
                    document.getElementById('hfClickedDate').value = info.dateStr;
                    document.getElementById('modalDateHeader').innerText = clickedDate.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });

                    // NEW: Prefill the form inputs with expected shift timings
                    prefillShiftTimings(info.dateStr);

                    document.getElementById('modalExistingRecordInfo').style.display = 'none';
                    document.getElementById('modalEmptyRecordInfo').style.display = 'block';
                    document.getElementById('dateDetailsModal').style.display = 'block';
                }
            });

            calendar.render();
        });

        // --- NEW: Helper Functions for the UI ---

        function hideForms() {
            document.getElementById('regForm').style.display = 'none';
            document.getElementById('leaveForm').style.display = 'none';

            // Clear inputs EXCEPT for the prefilled IN/OUT times
            document.getElementById('txtRegReason').value = '';
            document.getElementById('ddlLeaveTypes').selectedIndex = 0;
            document.getElementById('txtLeaveReason').value = '';
        }

        function toggleForm(formId) {
            hideForms();
            document.getElementById(formId).style.display = 'block';
        }

        function prefillShiftTimings(dateStr) {
            // Show a loading state briefly
            document.getElementById('txtRegIn').value = "";
            document.getElementById('txtRegOut').value = "";

            fetch('attendance.aspx/GetShiftTimings', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: dateStr })
            })
            .then(response => response.json())
            .then(data => {
                var timings = JSON.parse(data.d);
                if (timings.InTime) document.getElementById('txtRegIn').value = timings.InTime;
                if (timings.OutTime) document.getElementById('txtRegOut').value = timings.OutTime;
            })
            .catch(error => console.error('Error fetching shift timings:', error));
        }

        function submitRegularization() {
            var date = document.getElementById('hfClickedDate').value;
            var inTime = document.getElementById('txtRegIn').value;
            var outTime = document.getElementById('txtRegOut').value;
            var reason = document.getElementById('txtRegReason').value;

            if (!reason) {
                alert("Please provide a reason for the regularization.");
                return;
            }

            fetch('attendance.aspx/SubmitRegularization', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: date, inTime: inTime, outTime: outTime, reason: reason })
            })
            .then(response => response.json())
            .then(data => {
                if (data.d === "Success") {
                    alert("Regularization request submitted successfully!");
                    document.getElementById('dateDetailsModal').style.display = 'none';
                } else {
                    alert(data.d);
                }
            });
        }

        function submitLeave() {
            var date = document.getElementById('hfClickedDate').value;
            var leaveId = document.getElementById('ddlLeaveTypes').value;
            var reason = document.getElementById('txtLeaveReason').value;

            if (!leaveId || !reason) {
                alert("Please select a Leave Type and provide a reason.");
                return;
            }

            fetch('attendance.aspx/SubmitLeave', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ reqDate: date, leaveId: parseInt(leaveId), reason: reason })
            })
            .then(response => response.json())
            .then(data => {
                if (data.d === "Success") {
                    alert("Leave application submitted successfully!");
                    document.getElementById('dateDetailsModal').style.display = 'none';
                } else {
                    alert(data.d);
                }
            });
        }
    </script>
</asp:Content>
