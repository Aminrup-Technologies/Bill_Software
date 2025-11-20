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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
                    <asp:Panel ID="Panel1" runat="server" BorderColor="#336699" BorderStyle="Solid"
                        BorderWidth="2px">
                        <table cellpadding="0" cellspacing="1" class="style1">
                            <tr>
                                <td class="style1" colspan="2" width="50%">&nbsp;Welcome
                            <asp:Label ID="lblName" runat="server" CssClass="style3"></asp:Label>
                                    &nbsp;to <b>Flam-Ex</b>. You are logged in from <b>IP</b> : &nbsp;<asp:Label ID="lblIP" runat="server" Font-Bold="True" ForeColor="DarkBlue"></asp:Label>
                                    &nbsp;& Computer Name :
                                <asp:Label ID="lblpcname" runat="server" Font-Bold="True" ForeColor="DarkBlue"></asp:Label>.</td>
                            </tr>
                            <tr>
                                <td class="style1" colspan="2" width="50%">&nbsp;</td>
                            </tr>
                            <tr>
                                <td width="50%">&nbsp;<span class="style4">Contact Information</span>&nbsp;</td>
                                <td width="50%">&nbsp;</td>
                            </tr>
                            <tr>
                                <td>&nbsp;Your Email ID is
                            <asp:Label ID="lblEmailID" runat="server" CssClass="style3"
                                Text="username@domain.com"></asp:Label>
                                    &nbsp;</td>
                                <td>&nbsp;&nbsp;Contact No.
                            <asp:Label ID="lblContactNo" runat="server" CssClass="style3"></asp:Label>
                                    &nbsp;</td>
                            </tr>
                            <tr>
                                <td colspan="2">&nbsp;</td>
                            </tr>
                        </table>
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
