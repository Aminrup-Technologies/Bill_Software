<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .style3 {
            font-weight: bold;
        }

        .style4 {
            text-decoration: underline;
            font-weight: bold;
        }

        /* Simple fallback styling for blocked-popup message */
        .popupFallbackBox {
            border: 2px solid #336699;
            padding: 18px;
            background: #fff;
            width: 520px;
            margin: 40px auto;
            text-align: center;
            font-family: Arial, Helvetica, sans-serif;
        }

        .popupFallbackBtn {
            padding: 8px 14px;
            background: #19658A;
            color: #fff;
            border: none;
            cursor: pointer;
            font-weight: bold;
        }

        .erp-alert {
            background: #f0f8ff;
            border-left: 5px solid #1e90ff;
            padding: 12px 16px;
            margin-bottom: 15px;
            position: relative;
            font-size: 13px;
        }

        .erp-alert-sub {
            font-size: 12px;
            color: #444;
        }

        .erp-alert-close {
            position: absolute;
            top: 8px;
            right: 12px;
            font-weight: bold;
            text-decoration: none;
            color: #555;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            var userName = document.getElementById('<%= lblName.ClientID %>').innerText;
            var userEmail = document.getElementById('<%= lblEmailID.ClientID %>').innerText;
            var userRole = document.getElementById('<%= lblDashboardRole.ClientID %>').innerText;

            // NEW: Read the dynamic flag from the server
            var requireGeo = document.getElementById('<%= hfRequireGeo.ClientID %>').value === 'true';

            var qrContainer = document.getElementById("qrcode");
            var timeLabel = document.getElementById("liveTimestamp");
            var gpsLabel = document.getElementById("liveGPS");

            var currentTime = new Date().toLocaleString();
            timeLabel.innerText = "Logged: " + currentTime;

            function generateQR(lat, lon) {
                qrContainer.innerHTML = "";
                var qrData = "FLAME-EX SECURE ID\n" +
                             "Name: " + userName + "\n" +
                             "Role: " + userRole + "\n" +
                             "Email: " + userEmail + "\n" +
                             "Time: " + currentTime + "\n" +
                             "GPS: " + lat + ", " + lon;

                new QRCode(qrContainer, {
                    text: qrData, width: 90, height: 90,
                    colorDark: "#153e75", colorLight: "#ffffff",
                    correctLevel: QRCode.CorrectLevel.L
                });
            }

            // NEW LOGIC: Only prompt for GPS if the Admin enabled it for this user
            if (requireGeo) {
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition(
                        function (position) {
                            var lat = position.coords.latitude.toFixed(6);
                            var lon = position.coords.longitude.toFixed(6);
                            gpsLabel.innerText = "GPS: " + lat + ", " + lon;
                            generateQR(lat, lon);
                        },
                        function (error) {
                            var reason = "Access Denied/Unavailable";
                            if (error.code === error.TIMEOUT) reason = "Request Timed Out";
                            gpsLabel.innerText = "GPS: " + reason;
                            generateQR("N/A", "N/A");
                        },
                        { enableHighAccuracy: true, timeout: 30000, maximumAge: 0 }
                    );
                } else {
                    gpsLabel.innerText = "GPS: Not Supported";
                    generateQR("N/A", "N/A");
                }
            } else {
                // User does not require Geo Tracking
                gpsLabel.innerText = "GPS: Not Required";
                generateQR("Disabled", "Disabled");
            }
        });
    </script>
    <asp:Panel ID="pnlNotification" runat="server" Visible="false" CssClass="erp-alert">
        <strong>📢 New Module Live</strong><br />
        The <b>PR–PO (Purchase Requisition → Purchase Order)</b> module has been incorporated into the ERP.
    <br />
        <span class="erp-alert-sub">Access via <b>Procurement → PR–PO</b></span>

        <asp:LinkButton ID="btnDismiss"
            runat="server"
            CssClass="erp-alert-close"
            OnClick="btnDismiss_Click">
        ✕
        </asp:LinkButton>
    </asp:Panel>

    <asp:Panel ID="PanelMain" runat="server">
        <table cellpadding="0" cellspacing="1" class="style1">
            <tr>
                <td bgcolor="#19658A" colspan="4">&nbsp; <span class="style2">Home</span>&nbsp;</td>
            </tr>
            <tr>
                <td width="20%">&nbsp;</td>
                <td width="30%">&nbsp;</td>
                <td width="30%">&nbsp;</td>
                <td width="20%">&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <iframe
                    width="200"
                    height="200"
                    seamless
                    frameborder="0"
                    scrolling="no"
                    src="https://reports.aminruptechnologies.co.in/superset/explore/p/Qw1qzZKqJgn/?standalone=1&height=200"></iframe>

                </td>
                <td colspan="2">
                    <script src="https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.min.js"></script>

                    <asp:Panel ID="Panel1" runat="server" Style="max-width: 600px; margin: 0 auto 20px auto; font-family: 'Segoe UI', Arial, sans-serif;">

                        <div style="background: linear-gradient(135deg, #ffffff 0%, #f4f7f6 100%); border: 1px solid #c3d4e6; border-radius: 12px; box-shadow: 0 8px 20px rgba(0,0,0,0.1); overflow: hidden; position: relative;">

                            <div style="background-color: #153e75; color: white; padding: 10px 20px; font-weight: bold; font-size: 16px; display: flex; justify-content: space-between; align-items: center;">
                                <span>FLAME-EX DIGITAL ID</span>
                                <span style="font-size: 12px; font-weight: normal; opacity: 0.8;">Authorized Personnel</span>
                            </div>

                            <div style="display: flex; flex-wrap: wrap; padding: 20px; gap: 20px;">

                                <div style="display: flex; flex-direction: column; align-items: center; width: 120px;">
                                    <asp:Image ID="imgIdProfile" runat="server" ImageUrl="~/corporate/business/WebImages/representative.png"
                                        Style="width: 100px; height: 100px; border-radius: 8px; object-fit: cover; border: 3px solid #19658A; box-shadow: 0 4px 8px rgba(0,0,0,0.1); margin-bottom: 10px;" />
                                    <asp:HiddenField ID="hfRequireGeo" runat="server" Value="true" />
                                    <asp:Label ID="lblDashboardRole" runat="server" Font-Bold="True" ForeColor="#ffffff"
                                        Style="background-color: #19658A; padding: 4px 8px; border-radius: 4px; font-size: 11px; text-transform: uppercase; text-align: center; width: 100%; box-sizing: border-box;"></asp:Label>
                                </div>

                                <div style="flex: 1; min-width: 200px; display: flex; flex-direction: column; justify-content: center;">
                                    <asp:Label ID="lblName" runat="server" Font-Bold="True" ForeColor="#333333" Style="font-size: 22px; margin-bottom: 5px;"></asp:Label>

                                    <div style="font-size: 13px; color: #555; line-height: 1.6;">
                                        <strong style="color: #888;">Email:</strong>
                                        <asp:Label ID="lblEmailID" runat="server"></asp:Label><br />
                                        <strong style="color: #888;">Phone:</strong>
                                        <asp:Label ID="lblContactNo" runat="server"></asp:Label><br />
                                        <strong style="color: #888;">IP:</strong>
                                        <asp:Label ID="lblIP" runat="server"></asp:Label><br />
                                        <strong style="color: #888;">PC:</strong>
                                        <asp:Label ID="lblpcname" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; width: 100px;">
                                    <div id="qrcode" style="padding: 5px; background: white; border: 1px solid #ccc; border-radius: 4px;"></div>
                                    <div style="font-size: 9px; color: #888; margin-top: 5px; text-align: center;">LIVE SECURE TAG</div>
                                </div>
                            </div>

                            <div style="background-color: #eaf2ff; border-top: 1px solid #c3d4e6; padding: 8px 20px; font-size: 11px; color: #555; display: flex; justify-content: space-between;">
                                <span id="liveTimestamp">Locating...</span>
                                <span id="liveGPS">GPS: Acquiring...</span>
                            </div>
                        </div>
                    </asp:Panel>
                </td>
                <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <iframe
                    width="200"
                    height="200"
                    seamless
                    frameborder="0"
                    scrolling="no"
                    src="https://reports.aminruptechnologies.co.in/superset/explore/p/EKPqeVXWO9k/?standalone=1&height=200"></iframe>
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="2">&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </asp:Panel>

    <asp:Panel ID="PanelFallback" runat="server" Visible="false">
        <div class="popupFallbackBox">
            <h3>Important — Complete account setup</h3>
            <p>
                We've opened an account-update window that requires you to verify your email
                and set a custom password. If your browser blocked the popup, please click the button below.
            </p>

            <!-- Open in popup (attempt). If popup blocked, this will open in same tab. -->
            <asp:Button ID="btnOpenUpdatePopup" runat="server" CssClass="popupFallbackBtn" Text="Open Update Window"
                OnClientClick="var w = window.open('/corporate/business/app/Update/UpdateRequired.aspx','updatePopup','width=520,height=450,top=100,left=200,scrollbars=yes'); if(!w){ /* popup blocked - open same tab */ window.location='/corporate/business/app/Update/UpdateRequired.aspx'; } return false;" />

            <br />
            <br />
            <a href="/corporate/business/app/Update/UpdateRequired.aspx" target="_self">Open update page in this tab</a>
        </div>
    </asp:Panel>
</asp:Content>
