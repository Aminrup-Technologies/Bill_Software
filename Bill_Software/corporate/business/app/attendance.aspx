<%@ Page Title="Daily Attendance" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="attendance.aspx.cs" Inherits="Bill_Software.corporate.business.app.attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .dashboard-container { max-width: 950px; margin: 30px auto; font-family: 'Segoe UI', Arial, sans-serif; }
        
        .status-card { 
            background: #ffffff; padding: 30px; border-radius: 10px; text-align: center;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; margin-bottom: 30px;
        }
        
        .status-badge { 
            display: inline-block; padding: 8px 16px; border-radius: 20px; font-weight: bold; font-size: 16px; margin: 15px 0;
        }
        .status-out { background-color: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }
        .status-in { background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb; }
        .status-completed { background-color: #e2e3e5; color: #383d41; border: 1px solid #d6d8db; }

        .btn-punch {
            padding: 15px 40px; font-size: 18px; font-weight: bold; border: none; border-radius: 8px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white; margin: 10px;
        }
        .btn-punch:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 6px 15px rgba(0,0,0,0.2); }
        .btn-punch:disabled { opacity: 0.5; cursor: not-allowed; }
        
        .btn-in { background: linear-gradient(135deg, #34ce57, #28a745); }
        .btn-out { background: linear-gradient(135deg, #ff6b6b, #dc3545); }

        .history-section { background: #ffffff; padding: 25px; border-radius: 10px; box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; }
        
        /* GridView Styling */
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }
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

                // Load Punch IN Map
                var mapIn = document.getElementById('mapContainerIn');
                if (details.InLat && details.InLon && details.InLat !== "") {
                    var urlIn = "https://maps.google.com/maps?q=" + details.InLat + "," + details.InLon + "&hl=en&z=15&output=embed";
                    mapIn.innerHTML = "<iframe width='100%' height='100%' frameborder='0' scrolling='no' marginheight='0' marginwidth='0' src='" + urlIn + "'></iframe>";
                } else {
                    mapIn.innerHTML = "<span style='color: #888; font-style: italic;'>Location not captured.</span>";
                }

                // Load Punch OUT Map
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
            <asp:Label ID="lblStatusBadge" runat="server" CssClass="status-badge status-out" Text="Status: Not Punched In"></asp:Label>
            
            <div style="margin-top: 20px;">
                <asp:Label ID="lblPunchInTime" runat="server" Font-Bold="true" ForeColor="#28a745" style="display:block; margin-bottom:5px;"></asp:Label>
                <asp:Label ID="lblPunchOutTime" runat="server" Font-Bold="true" ForeColor="#dc3545" style="display:block; margin-bottom:15px;"></asp:Label>
            </div>

            <div>
                <button type="button" id="btnHtmlPunchIn" runat="server" class="btn-punch btn-in" onclick="capturePunch('IN', this);">▶️ Punch IN</button>
                <button type="button" id="btnHtmlPunchOut" runat="server" class="btn-punch btn-out" onclick="capturePunch('OUT', this);">⏹️ Punch OUT</button>
            </div>
            
            <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Bold="true" style="display:block; margin-top: 15px;"></asp:Label>
        </div>

        <div class="history-section">
            <h3 style="color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px;">📅 My Recent Attendance</h3>
            <asp:GridView ID="gvAttendanceHistory" runat="server" AutoGenerateColumns="False" CssClass="grid-style" EmptyDataText="No attendance records found for the last 30 days." GridLines="None">
                <Columns>
                    <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="PunchInTime" HeaderText="Punch In Time" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" />
                    <asp:BoundField DataField="PunchOutTime" HeaderText="Punch Out Time" DataFormatString="{0:hh:mm tt}" NullDisplayText="-" />
                    <asp:BoundField DataField="AttendanceStatus" HeaderText="Status" />
                    
                    <%-- NEW MAP VIEW BUTTON COLUMN --%>
                    <asp:TemplateField HeaderText="Location Details">
                        <ItemTemplate>
                            <button type="button" onclick="viewAttendanceMap('<%# Eval("Id") %>');" style="background-color: #17a2b8; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-weight: bold;">
                                📍 View Map
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
            </asp:GridView>
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
</asp:Content>