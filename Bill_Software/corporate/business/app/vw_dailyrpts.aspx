<%@ Page Title="My Sales Visits" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="vw_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.vw_dailyrpts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; font-family: 'Segoe UI', Arial, sans-serif; }
        .style2 { color: #FFFFFF; font-weight: bold; font-size: 16px; padding: 10px; display: inline-block; }
        
        /* Summary Grid */
        .summary-table { width: 100%; border-collapse: collapse; margin-top: 15px; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
        .summary-table th { background-color: #19658A; color: white; padding: 12px; text-align: left; font-weight: bold; border: 1px solid #0f4b69; }
        .summary-table td { padding: 10px 12px; border: 1px solid #ddd; vertical-align: middle; }
        .summary-table tr:hover { background-color: #f1f8ff; }

        /* Mega Modal */
        .mega-modal-bg { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.7); z-index: 99999; overflow-y: auto; padding: 20px 0; }
        .mega-modal-content { background: #fff; width: 95%; max-width: 950px; margin: 0 auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); display: flex; flex-direction: column; overflow: hidden; }
        .mega-modal-header { background-color: #19658A; color: white; padding: 15px 20px; font-weight: bold; font-size: 18px; display: flex; justify-content: space-between; align-items: center; }
        
        /* Tabs */
        .mega-tabs { display: flex; background: #f1f8ff; border-bottom: 2px solid #19658A; }
        .mega-tab-btn { flex: 1; background: none; border: none; padding: 14px 20px; font-size: 15px; font-weight: bold; color: #666; cursor: pointer; transition: 0.3s; border-right: 1px solid #e1eef4; }
        .mega-tab-btn:hover { background: #e1eef4; color: #19658A; }
        .mega-tab-btn.active { background: #19658A; color: white; border-bottom: none; }
        .tab-content { display: none; padding: 25px; background: white; min-height: 400px; box-sizing: border-box; }
        .tab-content.active { display: block; }
        .section-title { font-size: 16px; color: #19658A; font-weight: bold; border-bottom: 2px solid #eee; padding-bottom: 8px; margin-bottom: 15px; margin-top: 0; }
        
        /* Edit Form Table in Modal */
        .edit-table { width: 100%; border-collapse: separate; border-spacing: 0 12px; }
        .edit-table td { padding: 5px; vertical-align: top; }
        .edit-table .lbl { font-weight: bold; color: #444; padding-top: 8px; width: 20%; }
        
        /* Chat */
        .chat-box { height: 350px; overflow-y: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #e5ddd5; display: flex; flex-direction: column; gap: 12px; }
        .chat-message { max-width: 75%; min-width: 130px; padding: 8px 12px 24px 12px; border-radius: 8px; position: relative; box-shadow: 0 1px 1px rgba(0,0,0,0.1); font-size: 14px; line-height: 1.5; display: inline-block; word-wrap: break-word; }
        .chat-left { align-self: flex-start; background-color: #ffffff; border-top-left-radius: 0; }
        .chat-left::before { content: ''; position: absolute; top: 0; left: -10px; width: 0; height: 0; border: 10px solid transparent; border-right-color: #ffffff; border-top: 0; }
        .chat-right { align-self: flex-end; background-color: #dcf8c6; border-top-right-radius: 0; }
        .chat-right::before { content: ''; position: absolute; top: 0; right: -10px; width: 0; height: 0; border: 10px solid transparent; border-left-color: #dcf8c6; border-top: 0; }
        .chat-sender { font-size: 12px; font-weight: 800; color: #128C7E; margin-bottom: 4px; display: block; }
        .chat-right .chat-sender { color: #075E54; } 
        .chat-text { color: #303030; display: block; margin-bottom: 2px; }
        .chat-time { font-size: 10px; color: #999; position: absolute; bottom: 4px; right: 8px; white-space: nowrap; }
        .chat-input-container { display: flex; gap: 10px; margin-top: 10px; background: #f0f0f0; padding: 10px; border-radius: 8px; flex: 0 0 auto; }
        .chat-input-box { flex-grow: 1; padding: 12px 15px; border-radius: 20px; border: 1px solid #ccc; outline: none; }

        /* Approval chips */
        .approval-chip { display: inline-block; padding: 4px 8px; border-radius: 4px; font-weight: 600; font-size: 0.95em; line-height:1; }
        .approval-approved { color: #0b6623; background: #e9f7ee; border: 1px solid #c6efcf; }
        .approval-pending { color: #8a5600; background: #fff6e6; border: 1px solid #f0d7a8; }
        .approval-rejected { color: #a10000; background: #fdecec; border: 1px solid #f5bcbc; }
        .exp-grid { width: 100%; border-collapse: collapse; font-size: 14px; }
        .exp-grid th { background: #6c757d; color: white; padding: 8px; text-align: left; }
        .exp-grid td { padding: 8px; border-bottom: 1px solid #eee; }
    </style>

    <script type="text/javascript">
        function openMegaTab(tabId, btnElement) {
            var contents = document.getElementsByClassName('tab-content');
            for (var i = 0; i < contents.length; i++) contents[i].style.display = 'none';
            var btns = document.getElementsByClassName('mega-tab-btn');
            for (var i = 0; i < btns.length; i++) btns[i].classList.remove('active');
            document.getElementById(tabId).style.display = 'block';
            if (btnElement) btnElement.classList.add('active');
        }

        function showMegaModal(defaultTabId) {
            document.getElementById('megaModal').style.display = 'block';
            document.body.style.overflow = 'hidden'; 
            if (!defaultTabId) defaultTabId = 'tabDetails'; 
            var btnId = defaultTabId.replace('tab', 'btnTab');
            openMegaTab(defaultTabId, document.getElementById(btnId));
            scrollToBottomChat();
        }

        function hideMegaModal() {
            document.getElementById('megaModal').style.display = 'none';
            document.body.style.overflow = 'auto'; 
        }

        function scrollToBottomChat() {
            var container = document.getElementById("chatContainer");
            if (container) container.scrollTop = container.scrollHeight;
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

    <asp:UpdatePanel ID="upMain" runat="server">
        <ContentTemplate>
            
            <table cellpadding="0" cellspacing="0" class="auto-style1" style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.05); margin-bottom: 20px;">
                <tr><td colspan="6" bgcolor="#19658A" style="border-radius: 6px 6px 0 0;"><span class="style2">🔍 Search My Past Visits</span></td></tr>
                <tr><td colspan="6">
                    <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" style="margin-top:10px;">
                        <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
                    </asp:Panel>
                    <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" BackColor="#FFDDDD" style="margin-top:10px;">
                        <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
                    </asp:Panel>
                </td></tr>
                
                <tr>
                    <td style="padding: 15px 10px;"><b>From Date:</b></td>
                    <td><asp:TextBox ID="txtSearchFrom" runat="server" CssClass="datepicker textbox_style" Width="80%"></asp:TextBox></td>
                    <td style="padding: 15px 10px;"><b>To Date:</b></td>
                    <td><asp:TextBox ID="txtSearchTo" runat="server" CssClass="datepicker textbox_style" Width="80%"></asp:TextBox></td>
                    <td style="padding: 15px 10px;"><b>Exec. Status:</b></td>
                    <td>
                        <asp:DropDownList ID="ddlSearchStatus" runat="server" CssClass="dropdown_style" Width="90%">
                            <asp:ListItem Text="-- All Statuses --" Value="" />
                            <asp:ListItem>Completed</asp:ListItem>
                            <asp:ListItem>Pending Execution</asp:ListItem>
                            <asp:ListItem>Escalated</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td colspan="6" style="text-align: center; padding: 15px;">
                        <asp:Button ID="btnSearch" runat="server" Text="🔍 Search" CssClass="btn_style" OnClick="btnSearch_Click" style="background:#19658A; color:white; padding:8px 30px; font-weight:bold; border-radius:4px; border:none; cursor:pointer;" />
                        <asp:Button ID="btnResetSearch" runat="server" Text="🔄 Reset Filters" CssClass="btn_style" OnClick="btnResetSearch_Click" style="background:#6c757d; color:white; padding:8px 30px; font-weight:bold; border-radius:4px; border:none; cursor:pointer; margin-left: 10px;" />
                    </td>
                </tr>
            </table>

            <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.05);">
                <asp:GridView ID="gvSalesVisits" runat="server" AutoGenerateColumns="False" CssClass="summary-table" OnRowCommand="gvSalesVisits_RowCommand" DataKeyNames="Id">
                    <Columns>
                        <asp:BoundField DataField="VisitDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="VisitType" HeaderText="Type" />
                        <asp:BoundField DataField="Status" HeaderText="Exec. Status" />
                        <asp:TemplateField HeaderText="Approval">
                            <ItemTemplate>
                                <span class='<%# GetApprovalClass(Eval("ApprovalStatus")) %>'><%# Eval("ApprovalStatus") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Button ID="btnOpenMega" runat="server" Text="👁️ View / Edit File" CommandName="OpenMegaModal" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" style="background-color:#19658A; color:white; padding: 6px 12px; font-size:12px; border:none; border-radius:4px; cursor:pointer;" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div style="text-align:center; padding: 20px; color: #666; font-style: italic;">No visits found matching your search criteria.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <asp:HiddenField ID="hfMegaVisitId" runat="server" />
            
            <div id="megaModal" class="mega-modal-bg" style="display: none;">
                <div class="mega-modal-content">
                    
                    <div class="mega-modal-header">
                        <span>📋 Visit File: <asp:Label ID="lblMegaHeaderTitle" runat="server"></asp:Label></span>
                        <button type="button" onclick="hideMegaModal();" style="background:none; border:none; color:white; font-size:24px; cursor:pointer; line-height:1;">×</button>
                    </div>
                    
                    <div class="mega-tabs">
                        <button type="button" id="btnTabDetails" class="mega-tab-btn active" onclick="openMegaTab('tabDetails', this)">✏️ Edit Details</button>
                        <button type="button" id="btnTabLocation" class="mega-tab-btn" onclick="openMegaTab('tabLocation', this)">📍 Location Map</button>
                        <button type="button" id="btnTabExpenses" class="mega-tab-btn" onclick="openMegaTab('tabExpenses', this)">💸 Claimed Expenses</button>
                        <button type="button" id="btnTabAction" class="mega-tab-btn" onclick="openMegaTab('tabAction', this)">💬 Manager Chat</button>
                    </div>

                    <div id="tabDetails" class="tab-content active">
                        <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #eee; padding-bottom: 8px; margin-bottom: 15px;">
                            <h4 class="section-title" style="border:none; margin:0; padding:0;">Visit Details & Notes</h4>
                            <asp:Label ID="lblEditWarning" runat="server" ForeColor="#dc3545" Font-Bold="true" Visible="false"></asp:Label>
                        </div>

                        <asp:Panel ID="pnlEditForm" runat="server">
                            <table class="edit-table">
                                <tr>
                                    <td class="lbl">Date of Visit <span style="color:red">*</span></td>
                                    <td><asp:TextBox ID="edit_txtVisitDate" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                                    <td class="lbl">Visit Type <span style="color:red">*</span></td>
                                    <td>
                                        <asp:DropDownList ID="edit_ddlVisitType" runat="server" CssClass="dropdown_style" Width="90%">
                                            <asp:ListItem Text="-- Select Type --" Value="" />
                                            <asp:ListItem>Office Visit</asp:ListItem>
                                            <asp:ListItem>Plant Visit</asp:ListItem>
                                            <asp:ListItem>Online Meeting</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="lbl">Customer Name <span style="color:red">*</span></td>
                                    <td><asp:TextBox ID="edit_txtCustomerName" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                                    <td class="lbl">Department <span style="color:red">*</span></td>
                                    <td><asp:TextBox ID="edit_txtDepartment" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                                </tr>
                                <tr>
                                    <td class="lbl">Contact Person <span style="color:red">*</span></td>
                                    <td><asp:TextBox ID="edit_txtContactPerson" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                                    <td class="lbl">Execution Status</td>
                                    <td>
                                        <asp:DropDownList ID="edit_ddlStatus" runat="server" CssClass="dropdown_style" Width="90%">
                                            <asp:ListItem Text="-- Select Status --" Value="" />
                                            <asp:ListItem>Completed</asp:ListItem>
                                            <asp:ListItem>Pending Execution</asp:ListItem>
                                            <asp:ListItem>Escalated</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="lbl">Discussion Points <span style="color:red">*</span></td>
                                    <td colspan="3"><asp:TextBox ID="edit_txtDiscussion" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="4" Width="96%"></asp:TextBox></td>
                                </tr>
                                <tr>
                                    <td class="lbl">Follow-Up Required</td>
                                    <td>
                                        <asp:DropDownList ID="edit_ddlFollowUp" runat="server" CssClass="dropdown_style" Width="90%">
                                            <asp:ListItem Text="-- Select --" Value="" />
                                            <asp:ListItem>Yes</asp:ListItem>
                                            <asp:ListItem>No</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td class="lbl">Next Follow-Up</td>
                                    <td><asp:TextBox ID="edit_txtNextFollowUp" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                                </tr>
                                <tr>
                                    <td class="lbl">Attachment</td>
                                    <td>
                                        <asp:FileUpload ID="edit_fileAttachment" runat="server" Width="90%" />
                                        <br /><asp:HyperLink ID="hlCurrentAttachment" runat="server" Target="_blank" ForeColor="#0066cc" Font-Bold="true"></asp:HyperLink>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <div style="text-align: right; margin-top: 20px; padding-top: 15px; border-top: 1px solid #eee;">
                            <asp:Button ID="btnUpdateVisit" runat="server" Text="💾 Save Changes" CssClass="btn_style" OnClick="btnUpdateVisit_Click" style="background:#19658A; color:white; padding:10px 25px; font-weight:bold; font-size:15px; border:none; border-radius:4px; cursor:pointer;" />
                        </div>
                    </div>

                    <div id="tabLocation" class="tab-content">
                        <h4 class="section-title">📍 Execution Location</h4>
                        <div id="megaMapContainer" runat="server" style="border: 2px solid #eaeaea; border-radius: 8px; height: 350px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; width:100%;">
                            <span style='color: #888; font-style: italic;'>Map loading...</span>
                        </div>
                        <p style="text-align:center; color:#666; margin-top:15px; font-size:13px;">
                            <i>Note: This pin was captured when you clicked 'Execute & Tag Location' on your calendar.</i>
                        </p>
                    </div>

                    <div id="tabExpenses" class="tab-content">
                        <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #eee; padding-bottom: 8px; margin-bottom: 15px;">
                            <h4 class="section-title" style="border:none; margin:0; padding:0;">💸 Claimed Expenses</h4>
                            <a href="expense_entry.aspx" style="background:#ff9900; color:white; padding:6px 15px; border-radius:4px; text-decoration:none; font-weight:bold; font-size:13px;">➕ Add Expense</a>
                        </div>
                        <asp:GridView ID="gvMegaExpenses" runat="server" AutoGenerateColumns="False" CssClass="exp-grid" EmptyDataText="No expenses submitted for this visit." GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="ExpenseDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                <asp:BoundField DataField="ExpenseCategory" HeaderText="Category" />
                                <asp:BoundField DataField="Description" HeaderText="Details" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount (₹)" DataFormatString="{0:N2}" ItemStyle-Font-Bold="true" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='<%# GetApprovalClass(Eval("ApprovalStatus")) %>'><%# Eval("ApprovalStatus") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div id="tabAction" class="tab-content" style="display: flex; flex-direction: column; height: auto;">
                        <h4 class="section-title" style="flex: 0 0 auto;">💬 Manager Conversation</h4>
                        <div id="chatContainer" class="chat-box">
                            <asp:Literal ID="litMegaComments" runat="server"></asp:Literal>
                        </div>
                        <div class="chat-input-container" style="flex: 0 0 auto;">
                            <asp:TextBox ID="txtMegaNewComment" runat="server" CssClass="chat-input-box" placeholder="Type your response to the manager..."></asp:TextBox>
                            <asp:Button ID="btnMegaSendChat" runat="server" Text="➤ Reply" CssClass="btn_style" OnClick="btnMegaSendChat_Click" style="background:#128C7E; color:white; border:none; border-radius:20px; padding:0 25px; font-weight:bold; font-size: 15px;" />
                        </div>
                    </div>

                </div>
            </div>

        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnUpdateVisit" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>