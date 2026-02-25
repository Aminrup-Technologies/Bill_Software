<%@ Page Title="Search Daily Reports" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="srch_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.srch_dailyrpts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .table1 { border-collapse: collapse; }
        .table1 td { text-align: left; border: 1px solid #666666; width: 100%; padding: 5px; }
        .table2 { border-collapse: collapse; }
        .table2 td { text-align: left; border: 1px solid #666666; width: 100%; border-top: none; padding: 5px; }

        .modalPopup { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99999; }
        .modal-content { background: #fff; padding: 20px; margin: 5% auto; width: 90%; max-width: 850px; border-radius: 8px; box-shadow: 0 5px 15px rgba(0,0,0,0.3); }

        .comment { width: 100%; box-sizing: border-box; margin-bottom: 10px; }
        .comment-left { text-align: left; }
        .comment-right { text-align: right; }
        .comment-right b { display: inline-block; background: #e1f5fe; padding: 8px; border-radius: 8px; }
        .comment-left b { display: inline-block; background: #fce4ec; padding: 8px; border-radius: 8px; }
    </style>

    <script type="text/javascript">
        function showCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'block';
        }
        function hideCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'none';
        }
        function scrollToBottom() {
            var container = document.getElementById("commentsContainer");
            if (container) { container.scrollTop = container.scrollHeight; }
        }
        function hideDetailsModal() {
            document.getElementById('viewDetailsModal').style.display = 'none';
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
        });
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr><td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Search Daily Reports</span></td></tr>
                <tr>
                    <td width="15%">&nbsp;</td>
                    <td width="35%" colspan="2"><asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label></td>
                    <td width="35%" colspan="2">&nbsp;</td>
                    <td width="15%">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5">
                            &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5">
                            &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr><td>&nbsp;</td><td colspan="4">&nbsp;</td><td>&nbsp;</td></tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Sales Person Name</td>
                    <td colspan="2"><asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style"></asp:DropDownList></td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date</td>
                    <td><asp:TextBox ID="txttodate" runat="server" CssClass="datepicker textbox_style" Width="110px"></asp:TextBox></td>
                    <td>To Date</td>
                    <td><asp:TextBox ID="txtfromDate" runat="server" CssClass="datepicker textbox_style" Width="110px"></asp:TextBox></td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Search Type</td>
                    <td colspan="2">
                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem>Only Person</asp:ListItem>
                            <asp:ListItem Selected="True">Only Date</asp:ListItem>
                            <asp:ListItem>Person & Date</asp:ListItem>
                        </asp:RadioButtonList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr><td>&nbsp;</td><td colspan="4" style="text-align: center; padding: 15px;">
                    <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" />&nbsp;
                    <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />
                </td><td>&nbsp;</td></tr>
                
                <tr>
                    <td colspan="6">
                        <asp:DataList ID="DataList2" runat="server" Width="100%" OnItemCommand="DataList2_ItemCommand" OnItemDataBound="DataList2_ItemDataBound">
                            <HeaderTemplate>
                                <table class="table1">
                                    <tr style="background-color:#006699; color:white; font-weight:bold;">
                                        <td style="text-align:center; width:8%;">Visit Date</td>
                                        <td style="text-align:center; width:10%;">Salesperson</td>
                                        <td style="text-align:center; width:12%;">Customer</td>
                                        <td style="text-align:center; width:8%;">Department</td>
                                        <td style="text-align:center; width:10%;">Contact Person</td>
                                        <td style="text-align:center; width:8%;">Visit Type</td>
                                        <td style="text-align:center; width:6%;">Follow-Up</td>
                                        <td style="text-align:center; width:8%;">Next Date</td>
                                        <td style="text-align:center; width:6%;">Status</td>
                                        <td style="text-align:center; width:6%;">Details/Map</td>
                                        <td style="text-align:center; width:6%;">Attachment</td>
                                        <td style="text-align:center; width:12%;">Created Date</td>
                                    </tr>
                                </table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <table class="table2">
                                    <tr style="background-color: <%# Container.ItemIndex % 2 == 0 ? "#ffffff" : "#f4f8fb" %>;">
                                        <td style="text-align:center; width:8%;"><asp:Label ID="lblVisitDate" runat="server" Text='<%# Eval("VisitDate", "{0:dd-MM-yyyy}") %>' Font-Bold="true" ForeColor="DarkBlue"></asp:Label></td>
                                        <td style="text-align:center; width:10%;"><asp:Label ID="lblSalesperson" runat="server" Text='<%# Eval("Salesperson") %>'></asp:Label></td>
                                        <td style="text-align:center; width:12%;"><asp:Label ID="lblCustomerName" runat="server" Text='<%# Eval("CustomerName") %>'></asp:Label></td>
                                        <td style="text-align:center; width:8%;"><asp:Label ID="lblDepartment" runat="server" Text='<%# Eval("Department") %>'></asp:Label></td>
                                        <td style="text-align:center; width:10%;"><asp:Label ID="lblContactPerson" runat="server" Text='<%# Eval("ContactPerson") %>'></asp:Label></td>
                                        <td style="text-align:center; width:8%;"><asp:Label ID="lblVisitType" runat="server" Text='<%# Eval("VisitType") %>'></asp:Label></td>
                                        <td style="text-align:center; width:6%;"><asp:Label ID="lblFollowUpRequired" runat="server" Text='<%# Eval("FollowUpRequired") %>'></asp:Label></td>
                                        <td style="text-align:center; width:8%;"><asp:Label ID="lblNextFollowUpDate" runat="server" Text='<%# Eval("NextFollowUpDate", "{0:dd-MM-yyyy}") %>'></asp:Label></td>
                                        <td style="text-align:center; width:6%; font-weight:bold;"><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></td>
                                        
                                        <td style="text-align:center; width:6%;">
                                            <asp:Button ID="btnViewDet" runat="server" Text="📍 Map" CommandName="ViewDetails" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" style="background-color:#17a2b8; color:white; padding: 4px 8px; font-size:11px;" />
                                        </td>
                                        
                                        <td style="text-align:center; width:6%;"><asp:HyperLink ID="hlAttachment" runat="server" NavigateUrl='<%# Eval("AttachmentName", "~/Uploads/{0}") %>' Text="View" Target="_blank" /></td>
                                        <td style="text-align:center; width:12%;"><asp:Label ID="lblCreatedDate" runat="server" Text='<%# Eval("TimeStamp", "{0:dd-MM-yyyy HH:mm tt}") %>'></asp:Label></td>
                                    </tr>
                                    <tr style="background-color: <%# Container.ItemIndex % 2 == 0 ? "#ffffff" : "#f4f8fb" %>;">
                                        <td colspan="12" style="padding: 10px;">
                                            <b>Discussion Points:</b>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("DiscussionPoints").ToString().Replace(Environment.NewLine, "<br/>") %>' EnableViewState="false"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr style="background-color: <%# Container.ItemIndex % 2 == 0 ? "#ffffff" : "#f4f8fb" %>;">
                                        <td colspan="12" style="padding: 10px; border-top: 1px dashed #ccc;">
                                            <table style="width: 100%;">
                                                <tr>
                                                    <td style="vertical-align: top; text-align: left; width: 60%;">
                                                        <asp:Panel ID="pnlApproval" runat="server" Visible='<%# Eval("ApprovalStatus").ToString() == "Pending" %>'>
                                                            <asp:TextBox ID="txtManagerRemarks" runat="server" CssClass="textbox_style" Width="60%" TextMode="MultiLine" Rows="2" placeholder="Manager Remarks..."></asp:TextBox>&nbsp;
                                                            <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="Approve" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" style="background-color:#28a745; color:white;" />&nbsp;
                                                            <asp:Button ID="btnReject" runat="server" Text="Reject" CommandName="Reject" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" style="background-color:#dc3545; color:white;" />
                                                        </asp:Panel>
                                                        <asp:Panel ID="pnlApprovedInfo" runat="server" Visible='<%# Eval("ApprovalStatus").ToString() != "Pending" %>'>
                                                            <b>Approval Status:</b> <asp:Label ID="lblApprovalStatus" runat="server" Text='<%# Eval("ApprovalStatus") %>' Font-Bold="true"></asp:Label> &nbsp;|&nbsp;
                                                            <b>Approved By:</b> <asp:Label ID="lblApprovedBy" runat="server" Text='<%# Eval("ApprovedBy") %>'></asp:Label> &nbsp;|&nbsp;
                                                            <b>Timestamp:</b> <asp:Label ID="lblApprovedTime" runat="server" Text='<%# Eval("ApprovedDate", "{0:dd-MMM-yyyy HH:mm}") %>'></asp:Label>
                                                        </asp:Panel>
                                                    </td>
                                                    <td style="vertical-align: top; text-align: right; width: 40%;">
                                                        <asp:Button ID="btnViewComments" runat="server" Text="💬 View Comments" CommandName="ViewComments" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>

                        <asp:UpdatePanel ID="upComments" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnlComments" runat="server" Width="100%" CssClass="modalPopup" Style="display: none;">
                                    <div class="modal-content">
                                        <h3 style="font-weight: bold; color: #19658A;">Conversations:</h3><hr />
                                        <asp:HiddenField ID="hfVisitId" runat="server" />
                                        <div id="commentsContainer" style="max-height:300px; overflow-y:auto; border:1px solid #ccc; padding:10px; background:#f9f9f9;">
                                            <asp:Literal ID="litComments" runat="server"></asp:Literal>
                                        </div><hr />
                                        <h5 style="font-weight: bold; color: #19658A;">Type New Comment:</h5>
                                        <asp:TextBox ID="txtNewComment" runat="server" TextMode="MultiLine" Width="100%" CssClass="textbox_style" Rows="3" /><br /><br />
                                        <div style="text-align:right;">
                                            <asp:Button ID="btnSendComment" runat="server" Text="Send Message" CssClass="btn_style" OnClick="btnSendComment_Click" style="background-color:#19658A; color:white;" />
                                            <asp:Button ID="btnCloseComments" runat="server" Text="Close" CssClass="btn_style" OnClientClick="hideCommentsPopup(); return false;" />
                                        </div>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btnSendComment" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>

                        <div id="viewDetailsModal" class="modalPopup" style="display: none;">
                            <div class="modal-content" style="max-height: 90vh; overflow-y: auto; display: flex; flex-direction: column;">
                                <div style="background-color: #19658A; color: white; padding: 15px; font-weight: bold; font-size: 16px; border-radius: 4px 4px 0 0;">
                                    📍 Executed Visit Details & Location
                                </div>
                                <div style="padding: 20px; line-height: 1.6;">
                                    <div style="display: flex; flex-wrap: wrap; gap: 20px;">
                                        <div style="flex: 1; min-width: 250px;">
                                            <p style="margin: 0 0 5px 0;"><b>Customer:</b> <asp:Label ID="lblDetCustomer" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Department:</b> <asp:Label ID="lblDetDept" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Contact:</b> <asp:Label ID="lblDetContact" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Salesperson:</b> <asp:Label ID="lblDetSalesperson" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Visit Type:</b> <asp:Label ID="lblDetVisitType" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Planned Date:</b> <asp:Label ID="lblDetPlanDate" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Executed Date:</b> <asp:Label ID="lblDetExecDate" runat="server" ForeColor="#333"></asp:Label></p>
                                            <p style="margin: 0 0 5px 0;"><b>Status:</b> <asp:Label ID="lblDetStatus" runat="server" Font-Bold="true" ForeColor="#19658A"></asp:Label></p>
                                        </div>
                                        <div style="flex: 1; min-width: 300px;">
                                            <b style="display:block; margin-bottom: 5px;">Execution Location Map:</b>
                                            <div id="mapContainer" runat="server" style="border: 2px solid #eaeaea; border-radius: 8px; height: 250px; background: #f8f9fa; display: flex; align-items: center; justify-content: center;">
                                                <span style='color: #888; font-style: italic;'>Map will load here...</span>
                                            </div>
                                        </div>
                                    </div>
                                    <hr style="border: 0; border-top: 1px dashed #ccc; margin: 15px 0;" />
                                    <p style="margin:0;"><b>Discussion Points:</b><br />
                                        <asp:Label ID="lblDetNotes" runat="server" style="display:block; background:#f9fcfd; border: 1px solid #e1eef4; padding:10px; border-radius:6px; margin-top:5px; white-space: pre-wrap; color: #444;"></asp:Label>
                                    </p>
                                </div>
                                <div style="text-align: right; padding: 15px; border-top: 1px solid #eee; background-color: #fcfcfc;">
                                    <button type="button" class="btn_style" onclick="hideDetailsModal();" style="background-color: #6c757d; color: white; padding: 8px 20px; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;">Close View</button>
                                </div>
                            </div>
                        </div>

                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>