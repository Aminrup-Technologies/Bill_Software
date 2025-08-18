<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="srch_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.srch_dailyrpts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }

        .modalPopup {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.5);
            z-index: 9999;
        }

        .modal-content {
            background: #fff;
            padding: 15px;
            margin: 10% auto;
            width: 800px;
            border-radius: 5px;
        }

        .comment {
            width: 100%;
            box-sizing: border-box; /* To include padding/border in width */
        }

        .comment-left {
            text-align: left;
        }

        .comment-right {
            text-align: right;
        }

            .comment-right b {
                display: inline-block;
                background: #f0f8ff; /* light background for right */
                padding: 5px;
                border-radius: 5px;
            }

        .comment-left b {
            display: inline-block;
            background: #f8f8f8; /* light background for left */
            padding: 5px;
            border-radius: 5px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript" language="javascript"></script>

    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript" language="javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({
                dateFormat: 'dd-M-yy',

                changeMonth: true,
                changeYear: true
            });
        });

        <%--function showReplyPopup(visitId) {
            document.getElementById('<%= hfVisitId.ClientID %>').value = visitId;
            document.getElementById('<%= pnlReply.ClientID %>').style.display = 'block';
        }--%>

        <%--function hideReplyPopup() {
            document.getElementById('<%= pnlReply.ClientID %>').style.display = 'none';
        }--%>

        function showCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'block';
        }
        function hideCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'none';
        }
    </script>


    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Search Daily Reports</span></td>
                </tr>
                <tr>
                    <td width="15%">&nbsp;</td>
                    <td width="35%" colspan="2">
                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
                    </td>
                    <td width="35%" colspan="2">&nbsp;</td>
                    <td width="15%">&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD"
                            BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server"
                                ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                        </asp:Panel>

                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Sales Person Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date</td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>To Date</td>
                    <td>
                        <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Search Type</td>
                    <td colspan="2">
                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem>Only Person</asp:ListItem>
                            <asp:ListItem Selected="True">Only Date</asp:ListItem>
                            <asp:ListItem>Person &amp; Date</asp:ListItem>
                        </asp:RadioButtonList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" style="text-align: center">
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" />
                        &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:DataList ID="DataList2" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%"
                            OnItemCommand="DataList2_ItemCommand" OnItemDataBound="DataList2_ItemDataBound">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#94B8FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />

                            <HeaderTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 8%;">Visit Date</td>
                                        <td style="text-align: center; width: 10%;">Salesperson</td>
                                        <td style="text-align: center; width: 12%;">Customer Name</td>
                                        <td style="text-align: center; width: 8%;">Department</td>
                                        <td style="text-align: center; width: 10%;">Contact Person</td>
                                        <td style="text-align: center; width: 8%;">Visit Type</td>
                                        <td style="text-align: center; width: 6%;">Follow-Up</td>
                                        <td style="text-align: center; width: 8%;">Next Follow-Up</td>
                                        <td style="text-align: center; width: 6%;">Status</td>
                                        <td style="text-align: center; width: 6%;">Attachment</td>
                                        <td style="text-align: center; width: 6%;">Created Date</td>
                                    </tr>
                                </table>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblVisitDate" runat="server" Text='<%# Eval("VisitDate", "{0:dd-MM-yyyy}") %>' Font-Bold="true" ForeColor="DarkBlue"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="lblSalesperson" runat="server" Text='<%# Eval("Salesperson") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 12%;">
                                            <asp:Label ID="lblCustomerName" runat="server" Text='<%# Eval("CustomerName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblDepartment" runat="server" Text='<%# Eval("Department") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="lblContactPerson" runat="server" Text='<%# Eval("ContactPerson") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblVisitType" runat="server" Text='<%# Eval("VisitType") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="lblFollowUpRequired" runat="server" Text='<%# Eval("FollowUpRequired") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblNextFollowUpDate" runat="server" Text='<%# Eval("NextFollowUpDate", "{0:dd-MM-yyyy}") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:HyperLink ID="hlAttachment" runat="server"
                                                NavigateUrl='<%# Eval("AttachmentName", "~/Uploads/{0}") %>'
                                                Text="View" Target="_blank" />
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <asp:Label ID="lblCreatedDate" runat="server" Text='<%# Eval("TimeStamp", "{0:dd-MM-yyyy HH:mm tt}") %>'></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="12" style="padding: 5px;">
                                            <b>Discussion Points:</b>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("DiscussionPoints").ToString().Replace(Environment.NewLine, "<br/>") %>' EnableViewState="false"></asp:Label>
                                        </td>
                                    </tr>
                                    <%--<tr>
                                        <td colspan="12" style="padding: 5px;">
                                            <b>Viewer Remarks:</b><asp:TextBox ID="txtManagerRemarks" runat="server" CssClass="textbox_style" Width="400px" placeholder="Enter remarks..."></asp:TextBox>
                                            <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="Approve" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" />
                                            <asp:Button ID="btnReject" runat="server" Text="Reject" CommandName="Reject" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" />
                                        </td>
                                    </tr>--%>

                                    <td colspan="12" style="padding: 5px;">
                                        <table style="width: 100%;">
                                            <tr>
                                                <td style="vertical-align: top; text-align: left; width: 60%;">
                                                    <asp:Panel ID="pnlApproval" runat="server" Visible='<%# Eval("ApprovalStatus").ToString() == "Pending" %>'>
                                                        <asp:TextBox ID="txtManagerRemarks" runat="server"
                                                            Visible="false"
                                                            CssClass="textbox_style" Width="60%"
                                                            TextMode="MultiLine" Rows="2"></asp:TextBox>&nbsp;

                                                        <asp:Button ID="btnApprove" runat="server"
                                                            Text="Approve"
                                                            CommandName="Approve"
                                                            CommandArgument='<%# Eval("Id") %>'
                                                            CssClass="btn_style" />&nbsp;

                                                        <asp:Button ID="btnReject" runat="server"
                                                            Text="Reject"
                                                            CommandName="Reject"
                                                            CommandArgument='<%# Eval("Id") %>'
                                                            CssClass="btn_style" />
                                                    </asp:Panel>

                                                    <asp:Panel ID="pnlApprovedInfo" runat="server" Visible='<%# Eval("ApprovalStatus").ToString() != "Pending" %>'>
                                                        <b>Status:</b>
                                                        <asp:Label ID="lblApprovalStatus" runat="server" Text='<%# Eval("ApprovalStatus") %>'></asp:Label>&nbsp;|&nbsp;
                    
                                                        <%--<b>Remarks:</b>--%>
                                                        <asp:Label ID="lblApprovalRemarks" runat="server" Visible="false" Text='<%# Eval("ManagerRemarks") %>'></asp:Label>&nbsp;|&nbsp;
                    
                                                        <b>Approved By:</b>
                                                        <asp:Label ID="lblApprovedBy" runat="server" Text='<%# Eval("ApprovedBy") %>'></asp:Label>&nbsp;|&nbsp;
                    
                                                        <b>Timestamp:</b>
                                                        <asp:Label ID="lblApprovedTime" runat="server" Text='<%# Eval("ApprovedDate", "{0:yyyy-MM-dd HH:mm}") %>'></asp:Label>
                                                    </asp:Panel>
                                                </td>

                                                <td style="vertical-align: top; text-align: right; width: 40%;">
                                                    <asp:Button ID="btnViewComments" runat="server"
                                                        Text="View Comments"
                                                        CommandName="ViewComments"
                                                        CommandArgument='<%# Eval("Id") %>'
                                                        CssClass="btn btn_style" />&nbsp;
                                                </td>
                                            </tr>
                                        </table>
                                    </td>


                                </table>
                            </ItemTemplate>

                        </asp:DataList>

                        <asp:Panel ID="pnlReply" runat="server" CssClass="modalPopup" Style="display: none;">
                            <div class="modal-content">
                                <h4>Respond to Manager Remarks</h4>
                                <br />
                                <asp:HiddenField ID="hfVisitId" runat="server" />
                                <asp:TextBox ID="txtSalespersonReply" runat="server" Width="100%" TextMode="MultiLine" Rows="4" CssClass="form-control" Placeholder="Enter your response..."></asp:TextBox>
                                <br />
                                <asp:Button ID="btnSaveReply" runat="server" Text="Submit Response" CssClass="btn btn_style" />
                                <asp:Button ID="btnCancelReply" runat="server" Text="Cancel" CssClass="btn btn_style" OnClientClick="hideReplyPopup();return false;" />
                            </div>
                        </asp:Panel>

                        <asp:UpdatePanel ID="upComments" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnlComments" runat="server" Width="100%" CssClass="modalPopup" Style="display: none;">
                                    <div class="modal-content">
                                        <h3 style="font-weight: bold; font-size: medium; color: darkblue;">Conversations :</h3>
                                        <hr />
                                        <br />
                                        <asp:HiddenField ID="HiddenField1" runat="server" />
                                        <asp:Literal ID="litComments" runat="server"></asp:Literal>
                                        <hr />
                                        <br />
                                        <h5 style="font-weight: bold; font-size: small; color: darkblue;">Type New Comments :</h5>
                                        <div id="NewComment" runat="server" style="width: 100%">
                                            <asp:TextBox ID="txtNewComment" runat="server" TextMode="MultiLine" Width="100%"
                                                CssClass="form-control" Rows="3" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" />
                                        </div>
                                        <div id="Actions" runat="server" style="width: 100%">
                                            <asp:Button ID="btnSendComment" runat="server" Text="Send" CssClass="btn btn_style" OnClick="btnSendComment_Click" />
                                            <asp:Button ID="btnCloseComments" runat="server" Text="Close" CssClass="btn btn_style" OnClientClick="hideCommentsPopup(); return false;" />
                                        </div>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btnSendComment" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
