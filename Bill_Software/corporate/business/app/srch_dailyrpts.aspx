<%@ Page Title="Search Daily Reports" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="srch_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.srch_dailyrpts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
            font-size: 16px;
            padding: 10px;
            display: inline-block;
        }

        /* Clean Summary Table */
        .summary-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }

            .summary-table th {
                background-color: #19658A;
                color: white;
                padding: 12px;
                text-align: left;
                font-weight: bold;
                border: 1px solid #0f4b69;
            }

            .summary-table td {
                padding: 10px 12px;
                border: 1px solid #ddd;
                vertical-align: middle;
            }

            .summary-table tr:hover {
                background-color: #f1f8ff;
            }

        /* Mega Modal Styling */
        .mega-modal-bg {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.7);
            z-index: 99999;
            overflow-y: auto;
            padding: 20px 0;
        }

        .mega-modal-content {
            background: #fff;
            width: 95%;
            max-width: 900px;
            margin: 0 auto;
            border-radius: 8px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.4);
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }

        .mega-modal-header {
            background-color: #19658A;
            color: white;
            padding: 15px 20px;
            font-weight: bold;
            font-size: 18px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        /* === NEW: TAB UI STYLING === */
        .mega-tabs {
            display: flex;
            background: #f1f8ff;
            border-bottom: 2px solid #19658A;
        }

        .mega-tab-btn {
            flex: 1;
            background: none;
            border: none;
            padding: 14px 20px;
            font-size: 15px;
            font-weight: bold;
            color: #666;
            cursor: pointer;
            transition: 0.3s;
            border-right: 1px solid #e1eef4;
        }

            .mega-tab-btn:hover {
                background: #e1eef4;
                color: #19658A;
            }

            .mega-tab-btn.active {
                background: #19658A;
                color: white;
                border-bottom: none;
            }

        .tab-content { 
            display: none; 
            padding: 25px; 
            background: white; 
            min-height: 400px; 
            box-sizing: border-box; /* FIX: Prevents padding from breaking the layout */
        }

        .tab-content.active {
            display: block;
        }

        /* Modal Sections & Forms */
        .section-title {
            font-size: 16px;
            color: #19658A;
            font-weight: bold;
            border-bottom: 2px solid #eee;
            padding-bottom: 8px;
            margin-bottom: 15px;
            margin-top: 0;
        }

        /* === WHATSAPP STYLE CHAT === */
        .chat-box { 
            height: 300px; /* FIX: Strict height forces the scrollbar inside this box */
            overflow-y: auto; 
            padding: 20px; 
            border: 1px solid #ddd; 
            border-radius: 8px; 
            background-color: #e5ddd5; 
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .chat-message {
            max-width: 75%;
            min-width: 130px; /* FIX: Forces bubble to be wide enough for the timestamp */
            padding: 8px 12px 24px 12px; /* FIX: Added a bit more bottom padding */
            border-radius: 8px;
            position: relative;
            box-shadow: 0 1px 1px rgba(0,0,0,0.1);
            font-size: 14px;
            line-height: 1.5;
            display: inline-block;
            word-wrap: break-word;
        }

        /* Salesperson (Left side - White Bubble) */
        .chat-left {
            align-self: flex-start;
            background-color: #ffffff;
            border-top-left-radius: 0;
        }
        .chat-left::before { 
            content: ''; position: absolute; top: 0; left: -10px; width: 0; height: 0; 
            border: 10px solid transparent; border-right-color: #ffffff; border-top: 0; 
        }

        /* Manager (Right side - Green Bubble) */
        .chat-right {
            align-self: flex-end;
            background-color: #dcf8c6;
            border-top-right-radius: 0;
        }
        .chat-right::before { 
            content: ''; position: absolute; top: 0; right: -10px; width: 0; height: 0; 
            border: 10px solid transparent; border-left-color: #dcf8c6; border-top: 0; 
        }

        .chat-sender {
            font-size: 12px;
            font-weight: 800;
            color: #128C7E;
            margin-bottom: 4px;
            display: block;
        }
        .chat-right .chat-sender { color: #075E54; } /* Darker green for self */

        .chat-time {
            font-size: 10px;
            color: #999;
            position: absolute;
            bottom: 4px;
            right: 8px;
            white-space: nowrap; /* FIX: Prevents the date from wrapping into two lines */
        }
        
        .chat-text {
            color: #303030;
            display: block;
            margin-bottom: 2px; /* Keeps text away from the timestamp */
        }
        
        /* Modernized Chat Input Box */
        .chat-input-container { display: flex; gap: 10px; margin-top: 10px; background: #f0f0f0; padding: 10px; border-radius: 8px; }
        .chat-input-box { flex-grow: 1; padding: 12px 15px; border-radius: 20px; border: 1px solid #ccc; outline: none; }

        /* Expense Grid inside Modal */
        .exp-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 14px;
        }

            .exp-grid th {
                background: #6c757d;
                color: white;
                padding: 8px;
                text-align: left;
            }

            .exp-grid td {
                padding: 8px;
                border-bottom: 1px solid #eee;
            }

        .mega-modal-footer {
            background-color: #f1f8ff;
            padding: 15px 25px;
            border-top: 2px solid #19658A;
            border-radius: 0 0 8px 8px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .exp-action-btn {
            background: none;
            border: none;
            font-size: 16px;
            cursor: pointer;
            padding: 0 5px;
        }
    </style>

    <script type="text/javascript">
        // NEW: Handles clicking between tabs
        function openMegaTab(tabId, btnElement) {
            // Hide all contents
            var contents = document.getElementsByClassName('tab-content');
            for (var i = 0; i < contents.length; i++) {
                contents[i].style.display = 'none';
            }
            // Remove active style from all buttons
            var btns = document.getElementsByClassName('mega-tab-btn');
            for (var i = 0; i < btns.length; i++) {
                btns[i].classList.remove('active');
            }
            // Show target tab and highlight button
            document.getElementById(tabId).style.display = 'block';
            if (btnElement) { btnElement.classList.add('active'); }
        }

        // UPDATED: Accepts a 'defaultTabId' so C# can tell it which tab to open
        function showMegaModal(defaultTabId) {
            document.getElementById('megaModal').style.display = 'block';
            document.body.style.overflow = 'hidden'; // Lock background scrolling

            if (!defaultTabId) { defaultTabId = 'tabDetails'; }

            var btnId = defaultTabId.replace('tab', 'btnTab');
            var tabBtn = document.getElementById(btnId);
            openMegaTab(defaultTabId, tabBtn);

            scrollToBottomChat();
        }

        function hideMegaModal() {
            document.getElementById('megaModal').style.display = 'none';
            document.body.style.overflow = 'auto'; // Unlock background scrolling
        }

        function scrollToBottomChat() {
            var container = document.getElementById("chatContainer");
            if (container) { container.scrollTop = container.scrollHeight; }
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
            <table cellpadding="0" cellspacing="0" class="auto-style1" style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.05);">
                <tr>
                    <td colspan="6" bgcolor="#19658A" style="border-radius: 6px 6px 0 0;"><span class="style2">🔍 Manager Dashboard: Search Daily Reports</span></td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" Style="margin-top: 10px;">
                            <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" BackColor="#FFDDDD" Style="margin-top: 10px;">
                            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="6" style="height: 15px;"></td>
                </tr>
                <tr>
                    <td style="width: 5%"></td>
                    <td style="width: 15%; padding: 8px;"><b>Salesperson:</b></td>
                    <td style="width: 30%;">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style" Width="90%"></asp:DropDownList></td>
                    <td style="width: 15%; padding: 8px;"><b>Search By:</b></td>
                    <td style="width: 30%;">
                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem>Person</asp:ListItem>
                            <asp:ListItem Selected="True">Date</asp:ListItem>
                            <asp:ListItem>Both</asp:ListItem>
                        </asp:RadioButtonList>
                    </td>
                    <td style="width: 5%"></td>
                </tr>
                <tr>
                    <td></td>
                    <td style="padding: 8px;"><b>From Date:</b></td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" CssClass="datepicker textbox_style" Width="50%"></asp:TextBox></td>
                    <td style="padding: 8px;"><b>To Date:</b></td>
                    <td>
                        <asp:TextBox ID="txtfromDate" runat="server" CssClass="datepicker textbox_style" Width="50%"></asp:TextBox></td>
                    <td></td>
                </tr>
                <tr>
                    <td colspan="6" style="text-align: center; padding: 20px;">
                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="🔍 Search" Style="background: #19658A; color: white; padding: 8px 25px;" />
                        <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="🔄 Reset" Style="background: #6c757d; color: white; padding: 8px 25px;" />
                    </td>
                </tr>
            </table>

            <asp:DataList ID="DataList2" runat="server" Width="100%" OnItemCommand="DataList2_ItemCommand">
                <HeaderTemplate>
                    <table class="summary-table">
                        <tr>
                            <th style="width: 10%;">Date</th>
                            <th style="width: 15%;">Salesperson</th>
                            <th style="width: 20%;">Customer</th>
                            <th style="width: 15%;">Type</th>
                            <th style="width: 15%;">Status</th>
                            <th style="width: 10%;">Approval</th>
                            <th style="width: 15%; text-align: center;">Action</th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr style="background-color: <%# Container.ItemIndex % 2 == 0 ? "#ffffff" : "#fbfcfd" %>;">
                        <td>
                            <asp:Label ID="lblVisitDate" runat="server" Text='<%# Eval("VisitDate", "{0:dd-MMM-yyyy}") %>' Font-Bold="true"></asp:Label></td>
                        <td><%# Eval("Salesperson") %></td>
                        <td><b><%# Eval("CustomerName") %></b><br />
                            <small style="color: #666;"><%# Eval("Department") %></small></td>
                        <td><%# Eval("VisitType") %></td>
                        <td>
                            <span style="padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; background: <%# Eval("Status").ToString() == "Completed" ? "#d4edda" : "#fff3cd" %>; color: <%# Eval("Status").ToString() == "Completed" ? "#155724" : "#856404" %>;">
                                <%# Eval("Status") %>
                            </span>
                        </td>
                        <td>
                            <span style="padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; background: <%# Eval("ApprovalStatus").ToString() == "Approved" ? "#d4edda" : (Eval("ApprovalStatus").ToString() == "Rejected" ? "#f8d7da" : "#e2e3e5") %>; color: <%# Eval("ApprovalStatus").ToString() == "Approved" ? "#155724" : (Eval("ApprovalStatus").ToString() == "Rejected" ? "#721c24" : "#383d41") %>;">
                                <%# Eval("ApprovalStatus") %>
                            </span>
                        </td>
                        <td style="text-align: center;">
                            <asp:Button ID="btnViewComplete" runat="server" Text="👁️ View Complete File" CommandName="OpenMegaModal" CommandArgument='<%# Eval("Id") %>' CssClass="btn_style" Style="background-color: #19658A; color: white; padding: 6px 12px; font-size: 12px; border: none; cursor: pointer;" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:DataList>

            <asp:HiddenField ID="hfMegaVisitId" runat="server" />

            <div id="megaModal" class="mega-modal-bg" style="display: none;">
                <div class="mega-modal-content">

                    <div class="mega-modal-header">
                        <span>📋 Complete Visit File:
                            <asp:Label ID="lblMegaHeaderTitle" runat="server"></asp:Label></span>
                        <button type="button" onclick="hideMegaModal();" style="background: none; border: none; color: white; font-size: 24px; cursor: pointer; line-height: 1;">×</button>
                    </div>

                    <div class="mega-tabs">
                        <button type="button" id="btnTabDetails" class="mega-tab-btn active" onclick="openMegaTab('tabDetails', this)">📋 Details</button>
                        <button type="button" id="btnTabLocation" class="mega-tab-btn" onclick="openMegaTab('tabLocation', this)">📍 Location Map</button>
                        <button type="button" id="btnTabExpenses" class="mega-tab-btn" onclick="openMegaTab('tabExpenses', this)">💸 Expenses</button>
                        <button type="button" id="btnTabAction" class="mega-tab-btn" onclick="openMegaTab('tabAction', this)">💬 Chat</button>
                    </div>

                    <div id="tabDetails" class="tab-content active">
                        <h4 class="section-title">Visit Details & Notes</h4>
                        <table style="width: 100%; line-height: 2; font-size: 15px;">
                            <tr>
                                <td style="width: 30%; color: #666;">Salesperson:</td>
                                <td><b>
                                    <asp:Label ID="lblMegaSalesperson" runat="server"></asp:Label></b></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Customer:</td>
                                <td><b>
                                    <asp:Label ID="lblMegaCustomer" runat="server"></asp:Label></b></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Contact / Dept:</td>
                                <td>
                                    <asp:Label ID="lblMegaContact" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Planned Date:</td>
                                <td>
                                    <asp:Label ID="lblMegaPlanDate" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Execution Date:</td>
                                <td>
                                    <asp:Label ID="lblMegaExecDate" runat="server" ForeColor="#28a745" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Follow-Up / Next:</td>
                                <td>
                                    <asp:Label ID="lblMegaFollow" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td style="color: #666;">Attachment:</td>
                                <td>
                                    <asp:HyperLink ID="hlMegaAttachment" runat="server" Target="_blank" Style="color: #0066cc; font-weight: bold;"></asp:HyperLink></td>
                            </tr>
                        </table>
                        <div style="margin-top: 20px; padding: 15px; background: #f4f8fb; border: 1px solid #e1eef4; border-radius: 6px;">
                            <b style="color: #19658A;">Discussion Points / Notes:</b><br />
                            <br />
                            <asp:Label ID="lblMegaNotes" runat="server" Style="white-space: pre-wrap; font-size: 14px; color: #333;"></asp:Label>
                        </div>
                    </div>

                    <div id="tabLocation" class="tab-content">
                        <h4 class="section-title">📍 Execution Location</h4>
                        <div id="megaMapContainer" runat="server" style="border: 2px solid #eaeaea; border-radius: 8px; height: 350px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; width: 100%;">
                            <span style='color: #888; font-style: italic;'>Map loading...</span>
                        </div>
                        <p style="text-align: center; color: #666; margin-top: 15px; font-size: 13px;">
                            <i>Note: The map pin represents the exact GPS location of the salesperson when they pressed the "Execute" button.</i>
                        </p>
                    </div>

                    <div id="tabExpenses" class="tab-content">
                        <h4 class="section-title">💸 Expenses Claimed for this Visit</h4>
                        <asp:GridView ID="gvMegaExpenses" runat="server" AutoGenerateColumns="False" DataKeyNames="Id" OnRowCommand="gvMegaExpenses_RowCommand" CssClass="exp-grid" EmptyDataText="No expenses submitted for this visit." GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="ExpenseDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                <asp:BoundField DataField="ExpenseCategory" HeaderText="Category" />
                                <asp:BoundField DataField="Description" HeaderText="Details" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount (₹)" DataFormatString="{0:N2}" ItemStyle-Font-Bold="true" />
                                <asp:TemplateField HeaderText="Receipt">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hlExpReceipt" runat="server" NavigateUrl='<%# Eval("AttachmentName", "~/Uploads/Expenses/{0}") %>' Text="View File" Target="_blank" Visible='<%# Eval("AttachmentName") != DBNull.Value %>' ForeColor="#0066cc" Font-Bold="true" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <asp:Label ID="lblExpStatus" runat="server" Text='<%# Eval("ApprovalStatus") %>' Font-Bold="true"
                                            ForeColor='<%# Eval("ApprovalStatus").ToString() == "Approved" ? System.Drawing.Color.Green : (Eval("ApprovalStatus").ToString() == "Rejected" ? System.Drawing.Color.Red : System.Drawing.Color.Orange) %>'>
                                        </asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Action">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkApproveExp" runat="server" CommandName="ApproveExp" CommandArgument='<%# Eval("Id") %>' Visible='<%# Eval("ApprovalStatus").ToString() == "Pending" %>' Text="✔" CssClass="exp-action-btn" Style="color: #28a745; font-weight: bold;" ToolTip="Approve Expense" />
                                        <asp:LinkButton ID="lnkRejectExp" runat="server" CommandName="RejectExp" CommandArgument='<%# Eval("Id") %>' Visible='<%# Eval("ApprovalStatus").ToString() == "Pending" %>' Text="✖" CssClass="exp-action-btn" Style="color: #dc3545; font-weight: bold;" ToolTip="Reject Expense" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div id="tabAction" class="tab-content" style="display: flex; flex-direction: column; height: auto;">
                        <h4 class="section-title" style="flex: 0 0 auto;">💬 Conversation Thread</h4>
                        
                        <div id="chatContainer" class="chat-box">
                            <asp:Literal ID="litMegaComments" runat="server"></asp:Literal>
                        </div>
                        
                        <div class="chat-input-container" style="flex: 0 0 auto;">
                            <asp:TextBox ID="txtMegaNewComment" runat="server" CssClass="chat-input-box" placeholder="Type a message..."></asp:TextBox>
                            <asp:Button ID="btnMegaSendChat" runat="server" Text="➤ Send" CssClass="btn_style" OnClick="btnMegaSendChat_Click" style="background:#128C7E; color:white; border:none; border-radius:20px; padding:0 20px; font-weight:bold; font-size: 15px;" />
                        </div>
                    </div>

                    <div class="mega-modal-footer">
                        <asp:Panel ID="pnlMegaAction" runat="server" Width="100%" Style="display: flex; gap: 15px; align-items: center;">
                            <div style="flex-grow: 1;">
                                <asp:TextBox ID="txtMegaRemarks" runat="server" CssClass="textbox_style" Width="100%" placeholder="Enter Official Manager Remarks here before approving/rejecting the overall visit..." Style="padding: 8px; border: 1px solid #ccc;"></asp:TextBox>
                            </div>
                            <div>
                                <asp:Button ID="btnMegaReject" runat="server" Text="✖ Reject Visit" OnClick="btnMegaReject_Click" CssClass="btn_style" Style="background: #dc3545; color: white; padding: 5px 5px; border: none; border-radius: 2px; font-weight: bold; cursor: pointer;" />
                                <asp:Button ID="btnMegaApprove" runat="server" Text="✔ Approve Visit" OnClick="btnMegaApprove_Click" CssClass="btn_style" Style="background: #28a745; color: white; padding: 5px 5px; border: none; border-radius: 2px; font-weight: bold; cursor: pointer;" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlMegaAlreadyActioned" runat="server" Visible="false" Width="100%">
                            <div style="text-align: center; width: 100%; font-size: 15px; color: #333;">
                                Visit Status:
                                <asp:Label ID="lblMegaFinalStatus" runat="server" Font-Bold="true" Font-Size="16px"></asp:Label>
                                | Actioned By:
                                <asp:Label ID="lblMegaFinalBy" runat="server" Font-Bold="true"></asp:Label>
                                on
                                <asp:Label ID="lblMegaFinalDate" runat="server" Font-Bold="true"></asp:Label>
                            </div>
                        </asp:Panel>
                    </div>

                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
