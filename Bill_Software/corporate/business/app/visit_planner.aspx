<%@ Page Title="My Visit Calendar" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="visit_planner.aspx.cs" Inherits="Bill_Software.corporate.business.app.visit_planner" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src='https://cdn.jsdelivr.net/npm/fullcalendar@6.1.10/index.global.min.js'></script>

    <style>
        /* =========================================
           1. MODERN DASHBOARD LAYOUT
           ========================================= */
        .dashboard-container {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin: 20px auto;
            max-width: 100%;
            padding: 0 15px;
        }

        /* LHS Calendar Section */
        .calendar-section {
            flex: 2;
            min-width: 600px;
            background-color: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.05);
            border: 1px solid #eaeaea;
        }

        /* RHS Visit List Section */
        .list-section {
            flex: 1;
            min-width: 300px;
            background-color: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.05);
            display: flex;
            flex-direction: column;
            max-height: 750px; 
            border: 1px solid #eaeaea;
        }

        .visit-list-container {
            overflow-y: auto;
            flex-grow: 1;
            padding-right: 5px;
        }

        /* =========================================
           2. BEAUTIFIED CALENDAR OVERRIDES
           ========================================= */
        #calendar {
            font-family: 'Segoe UI', Arial, Helvetica, sans-serif;
            font-size: 14px;
            min-height: 600px;
        }

        /* Calendar Toolbar & Title */
        .fc-toolbar-title {
            color: #19658A !important;
            font-weight: 800 !important;
            font-size: 1.6em !important;
            letter-spacing: 0.5px;
        }

        /* Calendar Buttons (Match Corporate Theme) */
        .fc-button-primary {
            background-color: #19658A !important;
            border-color: #19658A !important;
            text-transform: capitalize !important;
            border-radius: 6px !important;
            font-weight: 600 !important;
            padding: 8px 16px !important;
            box-shadow: 0 2px 4px rgba(25, 101, 138, 0.3) !important;
            transition: all 0.2s ease;
        }
        .fc-button-primary:hover {
            background-color: #0f4b69 !important;
            box-shadow: 0 4px 8px rgba(25, 101, 138, 0.4) !important;
            transform: translateY(-1px);
        }
        .fc-button-active {
            background-color: #0b364c !important;
            border-color: #0b364c !important;
        }

        /* Grid & Days Styling */
        .fc-theme-standard .fc-scrollgrid { border: 1px solid #f0f0f0; border-radius: 8px; overflow: hidden; }
        .fc-theme-standard th { border: none; border-bottom: 2px solid #f4f4f4; padding: 12px 0; background: #fafafa; }
        .fc-col-header-cell-cushion { color: #666; font-weight: 700; text-transform: uppercase; font-size: 12px; text-decoration: none !important; }
        .fc-daygrid-day { border: 1px solid #f4f4f4 !important; transition: background-color 0.2s; }
        .fc-daygrid-day:hover { background-color: #fdfdfd; cursor: pointer; }
        .fc-daygrid-day-number { color: #444; font-weight: 600; text-decoration: none !important; padding: 8px !important; }
        
        /* Today Highlight */
        .fc-day-today { background-color: #f0f8ff !important; }
        .fc-day-today .fc-daygrid-day-number { background: #19658A; color: white; border-radius: 50%; width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; margin: 4px; padding: 0 !important; }

        /* =========================================
           3. EVENT CHIPS (PILLS) & RHS CARDS
           ========================================= */
        /* Calendar Events */
        .fc-event {
            border-radius: 12px !important;
            border: none !important;
            padding: 4px 8px !important;
            margin: 2px 4px !important;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            transition: transform 0.2s, box-shadow 0.2s;
            font-weight: 600;
            font-size: 12px;
        }
        .fc-event:hover {
            transform: scale(1.03);
            box-shadow: 0 4px 8px rgba(0,0,0,0.15);
            z-index: 5;
        }

        .event-planned, .badge-planned { background: linear-gradient(135deg, #007bb5, #005f8f) !important; color: white !important; cursor: pointer; }
        .event-executed, .badge-executed { background: linear-gradient(135deg, #34ce57, #28a745) !important; color: white !important; cursor: pointer; }

        /* RHS Visit Cards */
        .visit-card {
            padding: 15px;
            margin-bottom: 15px;
            background: #ffffff;
            border-radius: 8px;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0,0,0,0.06);
            transition: all 0.2s ease;
            border: 1px solid #f0f0f0;
        }
        .visit-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 15px rgba(0,0,0,0.1);
            border-color: #e0e0e0;
        }
    </style>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var calendarEl = document.getElementById('calendar');

            var calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: {
                    left: 'prev,next today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek'
                },
                // Add padding inside the cells
                dayMaxEvents: true, // Allow "more" link when too many events
                events: function (fetchInfo, successCallback, failureCallback) {
                    fetch('visit_planner.aspx/GetCalendarEvents', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json; charset=utf-8' }
                    })
                    .then(response => response.json())
                    .then(data => {
                        var events = JSON.parse(data.d);
                        successCallback(events);
                        renderVisitList(events);
                    })
                    .catch(error => {
                        console.error("Error fetching events:", error);
                        failureCallback(error);
                    });
                },
                dateClick: function (info) {
                    var clickedDate = info.dateStr; 
                    var today = new Date().toISOString().split('T')[0];
                    if (clickedDate < today) {
                        alert("You cannot plan a visit in the past!");
                        return;
                    }
                    window.location.href = "daily_rpt.aspx?date=" + clickedDate; 
                },
                eventClick: function (info) {
                    handleVisitClick(info.event.id, info.event.extendedProps.visitPhase);
                }
            });

            calendar.render();
        });

        // Generate the RHS List
        function renderVisitList(events) {
            var listContainer = document.getElementById('visitList');
            listContainer.innerHTML = ''; 

            if (events.length === 0) {
                listContainer.innerHTML = '<div style="color: #888; text-align: center; margin-top: 40px; font-style: italic;">No visits found.<br/><br/>Click a date on the calendar to plan one!</div>';
                return;
            }

            events.sort(function(a, b) {
                return new Date(b.start) - new Date(a.start);
            });

            events.forEach(function(ev) {
                var isPlanned = ev.visitPhase === 'Planned';
                var badgeClass = isPlanned ? 'badge-planned' : 'badge-executed';
                var borderColor = isPlanned ? '#005f8f' : '#28a745';

                var html = `
                    <div class="visit-card" style="border-left: 6px solid ${borderColor};" onclick="handleVisitClick('${ev.id}', '${ev.visitPhase}')">
                        <div style="font-weight: bold; font-size: 16px; color: #222;">${ev.title}</div>
                        <div style="font-size: 13px; color: #777; margin: 8px 0; display: flex; align-items: center; gap: 5px;">
                            <span style="font-size:16px;">📅</span> <span>${formatDate(ev.start)}</span>
                        </div>
                        <div style="margin-top: 10px;">
                            <span class="${badgeClass}" style="padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: bold; letter-spacing: 0.5px;">
                                ${ev.visitPhase.toUpperCase()}
                            </span>
                        </div>
                    </div>
                `;
                listContainer.insertAdjacentHTML('beforeend', html);
            });
        }

        function formatDate(dateString) {
            var options = { day: 'numeric', month: 'short', year: 'numeric' };
            return new Date(dateString).toLocaleDateString('en-GB', options);
        }

        function handleVisitClick(visitId, phase) {
            if(phase === 'Planned') {
                document.getElementById('<%= hfExecuteVisitId.ClientID %>').value = visitId;
                document.getElementById('executeModal').style.display = 'block';
            } else {
                fetch('visit_planner.aspx/GetVisitDetails', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify({ visitId: parseInt(visitId) })
                })
                .then(response => response.json())
                .then(data => {
                    var details = JSON.parse(data.d);
                    
                    document.getElementById('lblViewCustomer').innerText = details.CustomerName;
                    document.getElementById('lblViewDepartment').innerText = details.Department;
                    document.getElementById('lblViewContact').innerText = details.ContactPerson;
                    document.getElementById('lblViewSalesperson').innerText = details.Salesperson;
                    document.getElementById('lblViewType').innerText = details.VisitType;
                    document.getElementById('lblViewPlanDate').innerText = details.VisitDate;
                    document.getElementById('lblViewExecDate').innerText = details.ExecutionDate;
                    document.getElementById('lblViewStatus').innerText = details.Status;
                    document.getElementById('lblViewFollowUp').innerText = details.FollowUpRequired;
                    document.getElementById('lblViewNextFollowUp').innerText = details.NextFollowUpDate;
                    document.getElementById('lblViewNotes').innerText = details.DiscussionPoints;

                    var attachmentHtml = (details.AttachmentName && details.AttachmentName !== "") 
                        ? "<a href='Uploads/" + details.AttachmentName + "' target='_blank' style='color:#0066cc; font-weight:bold; text-decoration:none;'>📎 View Document</a>" 
                        : "N/A";
                    document.getElementById('lblViewAttachment').innerHTML = attachmentHtml;

                    var mapContainer = document.getElementById('mapContainer');
                    if(details.Latitude && details.Longitude && details.Latitude !== "" && details.Longitude !== "") {
                        var mapUrl = "https://maps.google.com/maps?q=" + details.Latitude + "," + details.Longitude + "&hl=en&z=15&output=embed";
                        mapContainer.innerHTML = "<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='" + mapUrl + "'></iframe>";
                    } else {
                        mapContainer.innerHTML = "<span style='color: #6c757d; font-style: italic;'>Location not captured during execution.</span>";
                    }
                    
                    document.getElementById('btnAddExpense').onclick = function() {
                        window.location.href = "expense_entry.aspx?visitId=" + visitId;
                    };

                    document.getElementById('viewModal').style.display = 'block';
                })
                .catch(error => console.error('Error fetching details:', error));
            }
        }

        function captureLocationAndSubmit(btnElement) {
            var discussion = document.getElementById('<%= txtExecDiscussion.ClientID %>').value.trim();
            if(discussion === '') {
                alert("Please enter the Visit Outcome / Discussion points.");
                return false;
            }

            if (navigator.geolocation) {
                btnElement.innerText = "📍 Acquiring GPS...";
                btnElement.disabled = true;

                navigator.geolocation.getCurrentPosition(
                    function (position) {
                        document.getElementById('<%= hfLatitude.ClientID %>').value = position.coords.latitude;
                        document.getElementById('<%= hfLongitude.ClientID %>').value = position.coords.longitude;
                        document.getElementById('<%= btnSubmitExecution.ClientID %>').click();
                    }, 
                    function (error) {
                        alert("Geolocation failed: " + error.message + ".\n\nYou must allow location access to execute a visit.");
                        btnElement.innerText = "📍 Execute & Tag Location";
                        btnElement.disabled = false;
                    },
                    { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
                );
            } else {
                alert("Geolocation is not supported by your browser.");
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="dashboard-container">
        
        <div class="calendar-section">
            <div style="border-bottom: 2px solid #f0f0f0; padding-bottom: 12px; margin-bottom: 20px;">
                <h3 style="color: #19658A; margin: 0; font-size: 20px; font-weight: 800; letter-spacing: 0.5px;">🗓️ Visit Planner</h3>
            </div>
            <div id="calendar"></div>
        </div>

        <div class="list-section">
            <div style="border-bottom: 2px solid #f0f0f0; padding-bottom: 12px; margin-bottom: 20px;">
                <h3 style="color: #19658A; margin: 0; font-size: 20px; font-weight: 800; letter-spacing: 0.5px;">📝 My Itinerary</h3>
            </div>
            <div id="visitList" class="visit-list-container">
                </div>
        </div>

    </div>

    <asp:HiddenField ID="hfExecuteVisitId" runat="server" />
    <asp:HiddenField ID="hfLatitude" runat="server" />
    <asp:HiddenField ID="hfLongitude" runat="server" />

    <div id="executeModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99999;">
        <div style="background: #fff; width: 90%; max-width: 700px; margin: 5% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: Arial, sans-serif;">
            
            <div style="background-color: #19658A; color: white; padding: 18px; font-weight: bold; font-size: 18px;">
                📍 Execute Sales Visit
            </div>
            
            <div style="padding: 25px;">
                <table style="width: 100%; border-spacing: 0; border-collapse: separate; row-gap: 15px;">
                    <tr style="height: 60px;">
                        <td style="width: 35%; vertical-align: top; padding-top: 5px;"><b>Visit Outcome / Discussion:</b><span style="color: red">*</span></td>
                        <td><asp:TextBox ID="txtExecDiscussion" runat="server" TextMode="MultiLine" Rows="4" Width="100%" style="border: 1px solid #ccc; padding: 8px; border-radius: 6px; font-family: inherit;"></asp:TextBox></td>
                    </tr>
                    <tr style="height: 40px;">
                        <td><b>Status:</b><span style="color: red">*</span></td>
                        <td>
                            <asp:DropDownList ID="ddlExecStatus" runat="server" Width="100%" style="border: 1px solid #ccc; padding: 8px; border-radius: 6px;">
                                <asp:ListItem>Completed</asp:ListItem>
                                <asp:ListItem>Pending</asp:ListItem>
                                <asp:ListItem>Escalated</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr style="height: 40px;">
                        <td><b>Follow-Up Required:</b></td>
                        <td>
                            <asp:DropDownList ID="ddlExecFollowUp" runat="server" Width="100%" style="border: 1px solid #ccc; padding: 8px; border-radius: 6px;">
                                <asp:ListItem Value="No">No</asp:ListItem>
                                <asp:ListItem Value="Yes">Yes</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr style="height: 40px;">
                        <td><b>Next Follow-Up Date:</b></td>
                        <td><asp:TextBox ID="txtExecNextDate" runat="server" TextMode="Date" Width="100%" style="border: 1px solid #ccc; padding: 8px; border-radius: 6px;"></asp:TextBox></td>
                    </tr>
                    <tr style="height: 40px;">
                        <td><b>Attachment (Photo/Doc):</b></td>
                        <td><asp:FileUpload ID="fileExecAttachment" runat="server" Width="100%" style="padding: 5px 0;" /></td>
                    </tr>
                </table>
            </div>
            
            <div style="text-align: right; padding: 18px 25px; border-top: 1px solid #eee; background-color: #fcfcfc;">
                <button type="button" onclick="captureLocationAndSubmit(this);" style="background: linear-gradient(135deg, #34ce57, #28a745); color: white; padding: 10px 20px; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; box-shadow: 0 2px 5px rgba(40,167,69,0.3); transition: transform 0.2s;">
                    📍 Execute & Tag Location
                </button>
                <asp:Button ID="btnSubmitExecution" runat="server" OnClick="btnSubmitExecution_Click" Style="display: none;" />
                <button type="button" onclick="document.getElementById('executeModal').style.display = 'none';" style="background-color: #6c757d; color: white; padding: 10px 20px; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; margin-left: 12px; transition: transform 0.2s;">
                    Close
                </button>
            </div>
        </div>
    </div>

    <div id="viewModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99999;">
        <div style="background: #fff; width: 90%; max-width: 850px; margin: 5% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: Arial, sans-serif; max-height: 90vh; display: flex; flex-direction: column;">
            
            <div style="background-color: #28a745; color: white; padding: 18px; font-weight: bold; font-size: 18px;">
                ✅ Executed Visit Details
            </div>
            
            <div style="padding: 25px; line-height: 1.6; overflow-y: auto; flex-grow: 1;">
                
                <div style="display: flex; flex-wrap: wrap; gap: 25px;">
                    <div style="flex: 1; min-width: 300px;">
                        <p style="margin: 0 0 8px 0;"><b>Customer:</b> <span id="lblViewCustomer" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Department:</b> <span id="lblViewDepartment" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Contact Person:</b> <span id="lblViewContact" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Salesperson:</b> <span id="lblViewSalesperson" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Visit Type:</b> <span id="lblViewType" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Planned Date:</b> <span id="lblViewPlanDate" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Executed On:</b> <span id="lblViewExecDate" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Status:</b> <span id="lblViewStatus" style="font-weight:bold; color:#19658A;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Follow-Up Required:</b> <span id="lblViewFollowUp" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Next Follow-Up Date:</b> <span id="lblViewNextFollowUp" style="color:#333;"></span></p>
                        <p style="margin: 0 0 8px 0;"><b>Attachment:</b> <span id="lblViewAttachment" style="color:#333;"></span></p>
                    </div>
                    
                    <div style="flex: 1; min-width: 300px;">
                        <b style="display:block; margin-bottom: 8px;">Execution Location:</b>
                        <div id="mapContainer" style="border: 2px solid #eaeaea; border-radius: 8px; overflow: hidden; height: 260px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; box-shadow: inset 0 2px 5px rgba(0,0,0,0.05);">
                            <span style='color: #888; font-style: italic;'>Loading map...</span>
                        </div>
                    </div>
                </div>

                <hr style="border: 0; border-top: 2px dashed #eee; margin: 25px 0;" />
                <p style="margin:0;"><b>Outcome / Discussion Points:</b><br />
                    <span id="lblViewNotes" style="display:block; background:#f9fcfd; border: 1px solid #e1eef4; padding:15px; border-radius:6px; margin-top:10px; white-space: pre-wrap; color: #444;"></span>
                </p>
            </div>
            
            <div style="text-align: right; padding: 18px 25px; border-top: 1px solid #eee; background-color: #fcfcfc;">
                <button type="button" id="btnAddExpense" style="background: linear-gradient(135deg, #ffb13d, #ff9900); color: white; padding: 10px 20px; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; float:left; box-shadow: 0 2px 5px rgba(255,153,0,0.3); transition: transform 0.2s;">
                    ➕ Attach Expense
                </button>
                <button type="button" onclick="document.getElementById('viewModal').style.display = 'none';" style="background-color: #6c757d; color: white; padding: 10px 20px; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; transition: transform 0.2s;">
                    Close
                </button>
            </div>
        </div>
    </div>
</asp:Content>