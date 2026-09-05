<%@ Page Title="Flame-Ex | Dashboard" Language="C#" Async="true" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .dashboard-wrapper { padding: 20px; max-width: 1400px; margin: 0 auto; }
        .dashboard-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 20px; margin-bottom: 20px; }
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.05); border: 1px solid #eaeaea; display: flex; flex-direction: column; }
        .box-title { color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-bottom: 15px; font-size: 16px; font-weight: bold; text-transform: uppercase; }
        .stat-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; font-size: 14px; }
        .stat-value { font-weight: bold; color: #19658A; }
        .full-width-panel { grid-column: 1 / -1; }

        .biz-card { background: linear-gradient(145deg, #ffffff 0%, #f0f4f8 100%); border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,0.08); overflow: hidden; position: relative; border: 1px solid #e1e8ed; }
        .biz-card-header { background: linear-gradient(135deg, #153e75 0%, #19658A 100%); color: white; padding: 15px 20px; display: flex; justify-content: space-between; align-items: center; }
        .biz-card-header h3 { margin: 0; font-size: 16px; font-weight: 700; letter-spacing: 1px; }
        .biz-card-body { padding: 25px 20px; display: flex; flex-direction: column; align-items: center; text-align: center; }
        .biz-profile-pic { width: 110px; height: 110px; border-radius: 50%; object-fit: cover; border: 4px solid #fff; box-shadow: 0 4px 15px rgba(0,0,0,0.15); margin-bottom: 15px; }
        .biz-name { font-size: 22px; font-weight: 800; color: #333; margin: 0 0 5px 0; }
        .biz-role { font-size: 13px; font-weight: 600; color: #19658A; text-transform: uppercase; letter-spacing: 1px; background: #eaf2ff; padding: 5px 12px; border-radius: 20px; margin-bottom: 15px; }
        .biz-dept { font-size: 12px; color: #666; margin-bottom: 15px; font-weight: 500; }
        .biz-contact-grid { width: 100%; display: grid; grid-template-columns: 1fr; gap: 10px; text-align: left; margin-bottom: 20px; font-size: 13px; color: #555; }
        .biz-contact-item { display: flex; align-items: center; gap: 10px; background: #fff; padding: 8px 12px; border-radius: 8px; border: 1px solid #eee; }
        .biz-contact-item i { color: #19658A; font-size: 16px; width: 20px; text-align: center; }
        .verified-badge { color: #28a745; font-size: 14px; margin-left: auto; }
        .biz-products { width: 100%; text-align: left; margin-bottom: 15px; }
        .biz-products h4 { font-size: 12px; color: #888; text-transform: uppercase; margin: 0 0 8px 0; border-bottom: 1px solid #ddd; padding-bottom: 4px; }
        .biz-tags { display: flex; flex-wrap: wrap; gap: 6px; }
        .biz-tag { background: #333; color: #fff; font-size: 11px; padding: 4px 10px; border-radius: 4px; }
        .biz-footer { background: #eaf2ff; padding: 12px 20px; display: flex; justify-content: space-between; align-items: center; font-size: 11px; color: #555; border-top: 1px solid #dce6f2; }
        .biz-qr { width: 60px; height: 60px; background: #fff; padding: 4px; border-radius: 6px; border: 1px solid #ccc; }

        .widget-action { display: inline-block; padding: 8px 15px; background: #f4f7f6; color: #19658A; text-decoration: none; border-radius: 6px; font-size: 13px; font-weight: 600; text-align: center; border: 1px solid #dce6f2; width: 100%; box-sizing: border-box; margin-top: auto; }
        .widget-action:hover { background: #eaf2ff; }

        .noti-container { width: 100%; max-height: 260px; overflow-y: auto; }
        .noti-item { width: 100%; box-sizing: border-box; padding: 12px 15px; background: #f8fafc; border-left: 4px solid #1e90ff; border-radius: 4px; margin-bottom: 12px; }
        .noti-item.severity-high, .noti-item.severity-critical { border-left-color: #dc3545; background: #fff5f5; }
        .noti-item.severity-medium, .noti-item.severity-warning { border-left-color: #ffc107; background: #fffdf5; }
        .noti-title { font-weight: 700; font-size: 14px; color: #333; margin-bottom: 4px; display: block; }
        .noti-date { font-size: 11px; color: #888; margin-bottom: 6px; display: block; }
        .noti-msg { font-size: 13px; color: #555; line-height: 1.4; display: block; }
        .noti-empty { padding: 20px; text-align: center; color: #888; font-style: italic; font-size: 14px; }

        .popupFallbackBox { border: 2px solid #336699; padding: 18px; background: #fff; width: 520px; margin: 40px auto; text-align: center; font-family: Arial, sans-serif; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <script src="https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>

    <asp:Panel ID="PanelMain" runat="server">
        <div class="dashboard-wrapper">
            <div class="dashboard-grid">

                <div class="id-card-section">
                    <div id="idCardContainer" class="biz-card">
                        <div class="biz-card-header">
                            <h3>FLAME-EX DIGITAL ID</h3>
                            <span style="font-size: 11px; opacity: 0.8;">★ Verified</span>
                        </div>

                        <div class="biz-card-body">
                            <asp:Image ID="imgIdProfile" runat="server" CssClass="biz-profile-pic" ImageUrl="~/corporate/business/WebImages/default-avatar.png" />
                            <asp:Label ID="lblName" runat="server" CssClass="biz-name" Text="Employee Name"></asp:Label>
                            <asp:Label ID="lblDesignation" runat="server" CssClass="biz-role" Text="Designation"></asp:Label>
                            <asp:Label ID="lblDepartment" runat="server" CssClass="biz-dept" Text="Department"></asp:Label>

                            <div class="biz-contact-grid">
                                <div class="biz-contact-item">
                                    <i>📞</i> <asp:Label ID="lblContactNo" runat="server"></asp:Label>
                                    <span class="verified-badge" title="Verified via OTP">✔️</span>
                                </div>
                                <div class="biz-contact-item">
                                    <i>✉️</i> <asp:Label ID="lblEmailID" runat="server"></asp:Label>
                                    <asp:Literal ID="litEmailVerified" runat="server"></asp:Literal>
                                </div>
                                <div class="biz-contact-item"><i>🌐</i> www.aagroupindia.com</div>
                            </div>

                            <div class="biz-products">
                                <h4>Key Solutions</h4>
                                <div class="biz-tags">
                                    <span class="biz-tag">Bearings & Lubricants</span>
                                    <span class="biz-tag">Industrial Machines</span>
                                    <span class="biz-tag">Industrial Consulting</span>
                                </div>
                            </div>
                        </div>

                        <div class="biz-footer">
                            <div>
                                <div id="liveTimestamp" style="font-weight:bold; margin-bottom:3px;">Locating...</div>
                                <div id="liveGPS">GPS: Acquiring...</div>
                            </div>
                            <div id="qrcode" class="biz-qr"></div>
                        </div>
                    </div>
                    <div style="display: flex; justify-content: flex-end; margin: 10px 0 0 0;">
                        <button type="button" id="btnShareID" onclick="shareOrDownloadIDCard()" class="widget-action" style="background:#28a745; color:white; border:none; width: auto; margin-top: 0;">
                            📤 Share / Download ID
                        </button>
                    </div>
                </div>

                <asp:Panel ID="pnlWidgetLastLogin" runat="server" CssClass="box-panel">
                    <div class="box-title">Security & Access</div>
                    <div class="stat-row">
                        <span>Last Login</span>
                        <asp:Label ID="lblLastLogin" runat="server" CssClass="stat-value" Text="Checking..."></asp:Label>
                    </div>
                    <a href="/corporate/business/app/settings.aspx" class="widget-action">Manage Account Security</a>
                </asp:Panel>

                <asp:Panel ID="pnlWidgetAttendanceToday" runat="server" CssClass="box-panel">
                    <div class="box-title">Today's Status</div>
                    <div class="stat-row">
                        <span>Punch Status</span>
                        <asp:Label ID="lblAttStatus" runat="server" CssClass="stat-value" Text="Not Punched In"></asp:Label>
                    </div>
                    <div class="stat-row">
                        <span>Details</span>
                        <asp:Label ID="lblAttTime" runat="server" CssClass="stat-value" Text="Awaiting your first punch today."></asp:Label>
                    </div>
                    <a href="/corporate/business/app/attendance.aspx" class="widget-action">Go to Attendance Portal</a>
                </asp:Panel>

                <asp:Panel ID="pnlWidgetMonthlyAtt" runat="server" CssClass="box-panel">
                    <div class="box-title">Monthly Attendance</div>
                    <div class="stat-row">
                        <span>Present this month</span>
                        <asp:Label ID="lblDaysPresent" runat="server" CssClass="stat-value" Text="0 Days"></asp:Label>
                    </div>
                    <a href="/corporate/business/app/MyLeaves.aspx" class="widget-action">View Leave Balances</a>
                </asp:Panel>

                <asp:Panel ID="pnlWidgetSalesVisitsToday" runat="server" CssClass="box-panel">
                    <div class="box-title">Today's Field Ops</div>
                    <div class="stat-row">
                        <span>Visits logged today</span>
                        <asp:Label ID="lblVisitsToday" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                    </div>
                    <div class="stat-row">
                        <span>Quotes Generated</span>
                        <asp:Label ID="lblQuotesToday" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                    </div>
                    <div class="stat-row">
                        <span>Revenue Realized</span>
                        <asp:Label ID="lblRevenueToday" runat="server" CssClass="stat-value" Text="₹0.00"></asp:Label>
                    </div>
                    <asp:HyperLink ID="lnkSalesVisit" runat="server" CssClass="widget-action" NavigateUrl="/corporate/business/app/visit_planner.aspx" Text="Plan a New Visit"></asp:HyperLink>
                </asp:Panel>

                <asp:Panel ID="pnlWidgetSalesVisitsMonth" runat="server" CssClass="box-panel">
                    <div class="box-title">Monthly Field Ops</div>
                    <div class="stat-row">
                        <span>Total visits this month</span>
                        <asp:Label ID="lblVisitsMonth" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                    </div>
                    <div class="stat-row">
                        <span>Quotes Generated</span>
                        <asp:Label ID="lblQuotesMonth" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                    </div>
                    <div class="stat-row">
                        <span>Revenue Realized</span>
                        <asp:Label ID="lblRevenueMonth" runat="server" CssClass="stat-value" Text="₹0.00"></asp:Label>
                    </div>
                    <a href="/corporate/business/app/visit_planner.aspx" class="widget-action">View Visit Reports</a>
                </asp:Panel>

                <asp:Panel ID="pnlWidgetNotifications" runat="server" CssClass="box-panel full-width-panel">
                    <div class="box-title">System Notifications & Alerts</div>
                    <div class="noti-container">
                        <asp:Repeater ID="rptNotifications" runat="server">
                            <ItemTemplate>
                                <div class='noti-item severity-<%# Eval("Severity").ToString().ToLower() %>'>
                                    <span class="noti-title"><%# Eval("Title") %></span>
                                    <span class="noti-date"><%# Convert.ToDateTime(Eval("CreatedOn")).ToString("dd MMM yyyy, hh:mm tt") %></span>
                                    <span class="noti-msg"><%# Eval("Message") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoNotifications" runat="server" CssClass="noti-empty" Visible="false" Text="There are no active notifications at this time."></asp:Label>
                    </div>
                </asp:Panel>

            </div>
        </div>
        <asp:HiddenField ID="hfRequireGeo" runat="server" Value="true" />
    </asp:Panel>

    <asp:Panel ID="PanelFallback" runat="server" Visible="false">
        <div class="popupFallbackBox">
            <h3>Important — Complete account setup</h3>
            <p>We've opened an account-update window that requires you to verify your email and set a custom password. If your browser blocked the popup, please click the button below.</p>
            <asp:Button ID="btnOpenUpdatePopup" runat="server" CssClass="popupFallbackBtn" Text="Open Update Window" OnClientClick="window.location='/corporate/business/app/Update/UpdateRequired.aspx'; return false;" />
        </div>
    </asp:Panel>

    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            var userName = document.getElementById('<%= lblName.ClientID %>').innerText;
            var userEmail = document.getElementById('<%= lblEmailID.ClientID %>').innerText;
            var userPhone = document.getElementById('<%= lblContactNo.ClientID %>').innerText;
            var userRole = document.getElementById('<%= lblDesignation.ClientID %>').innerText;
            var requireGeo = document.getElementById('<%= hfRequireGeo.ClientID %>').value === 'true';

            var qrContainer = document.getElementById("qrcode");
            var timeLabel = document.getElementById("liveTimestamp");
            var gpsLabel = document.getElementById("liveGPS");

            var currentTime = new Date().toLocaleString();
            timeLabel.innerText = "Verified: " + currentTime;

            function generateQR(lat, lon) {
                qrContainer.innerHTML = "";
                var vCardData = "BEGIN:VCARD\r\nVERSION:3.0\r\nFN:" + userName + "\r\nORG:Flame-Ex\r\nTITLE:" + userRole + "\r\nTEL:" + userPhone + "\r\nEMAIL:" + userEmail + "\r\nURL:https://www.aagroupindia.com\r\nNOTE:GPS:" + lat + "," + lon + "\r\nEND:VCARD";
                new QRCode(qrContainer, { text: vCardData, width: 60, height: 60, colorDark: "#153e75", colorLight: "#ffffff", correctLevel: QRCode.CorrectLevel.L });
            }

            if (requireGeo && navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(
                    function (position) {
                        var lat = position.coords.latitude.toFixed(6);
                        var lon = position.coords.longitude.toFixed(6);
                        gpsLabel.innerText = "GPS: " + lat + ", " + lon;
                        generateQR(lat, lon);
                    },
                    function (error) {
                        gpsLabel.innerText = "GPS: Unavailable";
                        generateQR("N/A", "N/A");
                    },
                    { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
                );
            } else {
                gpsLabel.innerText = "GPS: Not Required";
                generateQR("Disabled", "Disabled");
            }
        });

        function shareOrDownloadIDCard() {
            var card = document.getElementById('idCardContainer');
            var userName = document.getElementById('<%= lblName.ClientID %>').innerText.trim().replace(/\s+/g, '_');
            var btn = document.getElementById('btnShareID');
            var originalText = btn.innerHTML;
            btn.innerHTML = "⏳ Generating...";
            btn.disabled = true;

            html2canvas(card, { scale: 2, useCORS: true, backgroundColor: null }).then(function (canvas) {
                canvas.toBlob(function (blob) {
                    var fileName = 'FLAME_EX_ID_' + userName + '.png';
                    var file = new File([blob], fileName, { type: 'image/png' });
                    if (navigator.canShare && navigator.canShare({ files: [file] })) {
                        navigator.share({ title: 'Official Digital ID', text: 'Please find my official FLAME-EX Digital ID attached.', files: [file] })
                        .finally(() => { btn.innerHTML = originalText; btn.disabled = false; });
                    } else {
                        var url = URL.createObjectURL(file);
                        var link = document.createElement('a'); link.download = fileName; link.href = url;
                        document.body.appendChild(link); link.click(); document.body.removeChild(link);
                        URL.revokeObjectURL(url);
                        btn.innerHTML = originalText; btn.disabled = false;
                    }
                }, 'image/png');
            });
        }
    </script>
</asp:Content>