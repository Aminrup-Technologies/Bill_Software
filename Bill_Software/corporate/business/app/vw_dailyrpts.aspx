<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="vw_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.vw_dailyrpts" %>

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

        .sales-grid {
            border-collapse: collapse;
            width: 100%;
            font-family: Arial, sans-serif;
            font-size: 14px;
        }

            .sales-grid th {
                background-color: #005f8f;
                color: white;
                padding: 8px;
                text-align: left;
            }

            .sales-grid td {
                padding: 6px;
                vertical-align: top;
                border: 1px solid #ddd;
                word-wrap: break-word;
            }

        .summary-row {
            background-color: #ddd;
            font-weight: bold;
        }

        .details-row td {
            background-color: #fcfcfc;
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

        .ui-datepicker {
            z-index: 99999 !important;
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
    </script>
    <script type="text/javascript">
        function showReplyPopup(visitId) {
            document.getElementById('<%= hfVisitId.ClientID %>').value = visitId;
            document.getElementById('<%= pnlReply.ClientID %>').style.display = 'block';
        }

        function hideReplyPopup() {
            document.getElementById('<%= pnlReply.ClientID %>').style.display = 'none';
        }

        function showCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'block';
        }
        function hideCommentsPopup() {
            document.getElementById('<%= pnlComments.ClientID %>').style.display = 'none';
        }

        function ShowEditPopup() {
            $('#editModal').modal('show');
        }


        function hideEditModal() {
            $('#editModal').modal('hide');
        }

        // Reopen modal after UpdatePanel partial postback if needed
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            if ($('#hfEditId').val() && $('#hfEditId').val() !== '') {
                showEditModal();
            }
        });

        function showEditModal() {
            document.getElementById('<%= pnlModify.ClientID %>').style.display = 'block';
        }

        function hideEditModal() {
            document.getElementById('<%= pnlModify.ClientID %>').style.display = 'none';
        }




    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>--%>
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Daily Reports</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
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
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td colspan="4">
                <asp:UpdatePanel ID="upGrid" runat="server">
                    <ContentTemplate>
                        <asp:GridView ID="gvSalesVisits" runat="server" AutoGenerateColumns="False" CssClass="sales-grid" OnRowDataBound="gvSalesVisits_RowDataBound" OnRowCommand="gvSalesVisits_RowCommand" DataKeyNames="Id">
                            <Columns>
                                <asp:TemplateField HeaderText="Summary">
                                    <ItemTemplate>
                                        <table style="width: 100%">
                                            <tr class="summary-row">
                                                <td style="width: 8%"><strong><%# Eval("Id") %></strong></td>
                                                <td style="width: 12%"><%# Eval("VisitDate", "{0:dd-MMM-yyyy}") %></td>
                                                <td style="width: 18%"><%# Eval("Salesperson") %></td>
                                                <td style="width: 20%">Customer: <%# Eval("CustomerName") %></td>
                                                <td style="width: 15%">Approval: <%# Eval("ApprovalStatus") %></td>
                                                <td style="width: 10%">
                                                    <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn_style" OnClick="btnEdit_Click" CommandArgument='<%# Eval("Id") %>' />
                                                </td>
                                            </tr>
                                            <tr class="details-row">
                                                <td colspan="3" style="padding: 10px; line-height: 1.6;">
                                                    <div>
                                                        <b>Department:</b> <span><%# Eval("Department") %></span> &nbsp;|&nbsp;
                                                <b>Contact:</b> <span><%# Eval("ContactPerson") %></span> &nbsp;|&nbsp;
                                                <b>Visit Type:</b> <span><%# Eval("VisitType") %></span>
                                                    </div>

                                                    <div>
                                                        <b>Follow-Up:</b> <span><%# Eval("FollowUpRequired") %></span> &nbsp;|&nbsp;
                                                <b>Next Follow-Up Date:</b>
                                                        <span><%# Eval("NextFollowUpDate", "{0:dd-MMM-yyyy}") %></span> &nbsp;|&nbsp;
                                                <b>Visit Status:</b> <span><%# Eval("Status") %></span>
                                                    </div>
                                                </td>
                                                <td colspan="3">
                                                    <div>
                                                        <b>Discussion:</b> <span><%# Eval("DiscussionPoints") %></span>
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr class="details-row">
                                                <td colspan="3">
                                                    <div>
                                                        <b>Manager Remarks:</b> <span><%# Eval("ManagerRemarks") %></span>
                                                    </div>
                                                </td>
                                                <%--<td colspan="2" style="width: 10%">
                                            <div>
                                                <b>Response:</b> <span><%# Eval("SalespersonReply") %></span>
                                            </div>
                                        </td>--%>
                                                <td colspan="3" style="width: 10%">
                                                    <div>
                                                        <%--<asp:Button ID="btnReply" runat="server" Visible="false" Text="Respond" OnClientClick='<%# "showReplyPopup(" + Eval("Id") + "); return false;" %>' CssClass="btn btn_style" />--%>
                                                        <asp:Button ID="btnViewComments" runat="server" Text="View Comments" CommandName="ViewComments" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn_style" />
                                                    </div>
                                                </td>
                                            </tr>

                                        </table>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <asp:Panel ID="pnlReply" runat="server" CssClass="modalPopup" Style="display: none;">
                    <div class="modal-content">
                        <h4>Respond to Manager Remarks</h4>
                        <br />
                        <asp:HiddenField ID="hfVisitId" runat="server" />
                        <asp:TextBox ID="txtSalespersonReply" runat="server" Width="100%" TextMode="MultiLine" Rows="4" CssClass="form-control" Placeholder="Enter your response..."></asp:TextBox>
                        <br />
                        <asp:Button ID="btnSaveReply" runat="server" Text="Submit Response" CssClass="btn btn_style" OnClick="btnSaveReply_Click" />
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

                <asp:UpdatePanel ID="upModify" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlModify" runat="server" Width="100%" CssClass="modalPopup" Style="display: none;">
                            <div id="editModal" class="modal fade" tabindex="-1" role="dialog">
                                <div class="modal-dialog modal-lg" role="document">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <h3 class="modal-title">Edit Sales Visit Report</h3>
                                        </div>
                                        <br />
                                        <div class="modal-body">
                                            <!-- Your existing form fields go here -->
                                            <asp:HiddenField ID="hfEditId" runat="server" />
                                            <table class="style1">
                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td width="15%"><span class="style3">*</span>Date of Visit</td>
                                                    <td width="25%">
                                                        <asp:TextBox ID="txtVisitDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td width="15%"><span class="style3">*</span>Salesperson Name</td>
                                                    <td width="25%">
                                                        <asp:TextBox ID="txtSalesperson" runat="server" CssClass="textbox_style" ReadOnly="true"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                </tr>

                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Customer Name</td>
                                                    <td>
                                                        <asp:TextBox ID="txtCustomerName" runat="server" CssClass="textbox_style"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Department</td>
                                                    <td>
                                                        <asp:TextBox ID="txtDepartment" runat="server" CssClass="textbox_style"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                </tr>

                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Contact Person</td>
                                                    <td>
                                                        <asp:TextBox ID="txtContactPerson" runat="server" CssClass="textbox_style"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Visit Type</td>
                                                    <td>
                                                        <asp:DropDownList ID="ddlVisitType" runat="server" CssClass="dropdown_style">
                                                            <asp:ListItem Text="-- Select Visit Type --" Value="" />
                                                            <asp:ListItem>Office Visit</asp:ListItem>
                                                            <asp:ListItem>Plant Visit</asp:ListItem>
                                                            <asp:ListItem>Online Meeting</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                </tr>

                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Discussion Points</td>
                                                    <td colspan="4">
                                                        <asp:TextBox ID="txtDiscussion" runat="server" CssClass="textbox_style" TextMode="MultiLine" Columns="2" Rows="4" Height="44px" Width="90%"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td>&nbsp;</td>
                                                </tr>

                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td><span class="style3">*</span>Follow-Up Required</td>
                                                    <td>
                                                        <asp:DropDownList ID="ddlFollowUp" runat="server" CssClass="dropdown_style">
                                                            <asp:ListItem Text="-- Select Follow-Up --" Value="" />
                                                            <asp:ListItem>Yes</asp:ListItem>
                                                            <asp:ListItem>No</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td>Next Follow-Up Date</td>
                                                    <td>
                                                        <asp:TextBox ID="txtNextFollowUp" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                </tr>

                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td>Status</td>
                                                    <td>
                                                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="dropdown_style">
                                                            <asp:ListItem Text="-- Select Status --" Value="" />
                                                            <asp:ListItem>Completed</asp:ListItem>
                                                            <asp:ListItem>Pending</asp:ListItem>
                                                            <asp:ListItem>Escalated</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </td>
                                                    <td>&nbsp;</td>
                                                    <td>Attachment</td>
                                                    <td>
                                                        <asp:FileUpload ID="fileAttachment" runat="server" />
                                                    </td>
                                                    <td>&nbsp;</td>
                                                </tr>
                                            </table>
                                            <!-- Include all the fields you posted in your first form -->
                                        </div>
                                        <div class="modal-footer">
                                            <asp:Button ID="btnUpdate" runat="server" CssClass="btn btn_style" Text="Save Changes" OnClick="btnUpdate_Click" />
                                            <asp:Button ID="Button1" runat="server" Text="Close" CssClass="btn btn_style" OnClientClick="hideEditModal(); return false;" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>




    <%--</ContentTemplate>
    </asp:UpdatePanel>--%>
</asp:Content>
