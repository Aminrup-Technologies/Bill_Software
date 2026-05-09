<%@ Page Title="Approvals Dashboard" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminApprovalDashboard.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminApprovalDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .dashboard-container { max-width: 950px; margin: 30px auto; font-family: 'Segoe UI', Arial, sans-serif; }
        
        /* TAB NAVIGATION STYLES */
        .tab-wrapper { border-bottom: 2px solid #eaeaea; margin-bottom: 20px; display: flex; gap: 5px; }
        .tab-btn {
            padding: 12px 25px; cursor: pointer; background: #f8f9fa; border: 1px solid #eaeaea;
            border-bottom: none; border-radius: 8px 8px 0 0; font-weight: bold; color: #666;
            transition: all 0.3s; margin-bottom: -2px;
        }
        .tab-btn:hover { background: #eee; color: #19658A; }
        .tab-btn.active { background: #fff; color: #19658A; border-top: 3px solid #19658A; border-left: 1px solid #eaeaea; border-right: 1px solid #eaeaea; height: 100%; padding-bottom: 14px; }
        
        /* CONTENT STYLES */
        .tab-content { display: none; animation: fadeIn 0.4s; }
        .tab-content.active { display: block; }
        @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

        .section-card { background: #ffffff; padding: 25px; border-radius: 0 0 10px 10px; box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; border-top: none; }
        .grid-style { width: 100%; border-collapse: collapse; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; }
        .btn-action-small { padding: 8px 15px; font-size: 13px; font-weight: bold; border: none; border-radius: 6px; cursor: pointer; color: white; }
        .btn-approve { background: linear-gradient(135deg, #34ce57, #28a745); }
        .btn-reject { background: linear-gradient(135deg, #ff6b6b, #dc3545); }
        .cell-description {
            max-width: 200px;
            font-size: 12px;
            color: #555;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            cursor: help;
        }
        /* Shows full text on hover */
        .cell-description:hover {
            white-space: normal;
            overflow: visible;
            background: #fff8e1;
            position: relative;
            z-index: 10;
            border: 1px solid #ddd;
            padding: 5px;
            border-radius: 4px;
        }

        /* Styling for the Action buttons to make them look uniform */
        .btn-action-small { 
            padding: 5px 10px; 
            font-size: 12px; 
            border-radius: 4px; 
            border: none; 
            color: white; 
            cursor: pointer;
            margin-right: 2px;
        }
    </style>

    <script type="text/javascript">
        function switchTab(tabId, btn) {
            // Hide all content
            document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
            // Deactivate all buttons
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            
            // Show selected content and activate button
            document.getElementById(tabId).classList.add('active');
            btn.classList.add('active');

            // Store active tab in local storage to persist through postbacks
            localStorage.setItem('activeAdminTab', tabId);
        }

        // Restore tab on page load
        window.onload = function () {
            var activeTab = localStorage.getItem('activeAdminTab') || 'tab-regularization';
            var btn = document.querySelector('[onclick*="' + activeTab + '"]');
            if (btn) switchTab(activeTab, btn);
        };
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        
        <asp:Panel ID="PanelOK" runat="server" Visible="False" style="padding:15px; margin-bottom: 20px; border-radius: 6px; text-align: center; background:#EEFFDD; border:1px solid #006600;">
            <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
        </asp:Panel>
        
        <asp:Panel ID="PanelError" runat="server" Visible="False" style="padding:15px; margin-bottom: 20px; border-radius: 6px; text-align: center; background:#FFDDDD; border:1px solid #FF3300;">
            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
        </asp:Panel>

        <div class="tab-wrapper">
            <div class="tab-btn active" onclick="switchTab('tab-regularization', this)">⏱️ Attendance Corrections</div>
            <div class="tab-btn" onclick="switchTab('tab-leaves', this)">🏖️ Leave Applications</div>
        </div>

        <div id="tab-regularization" class="tab-content active">
            <div class="section-card">
                <asp:GridView ID="gvRegularizations" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending attendance corrections." OnRowCommand="gvRegularizations_RowCommand" DataKeyNames="RequestID">
                    <Columns>
                        <asp:BoundField DataField="UserCode" HeaderText="Emp ID"/>
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="AttendanceDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="RequestedInTime" HeaderText="Req. IN" />
                        <asp:BoundField DataField="RequestedOutTime" HeaderText="Req. OUT" />
                        <asp:TemplateField HeaderText="Reason/Description">
                            <ItemTemplate>
                                <div class="cell-description" title='<%# Eval("Reason") %>'>
                                    <%# Eval("Reason") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnApproveReg" runat="server" Text="✔" CssClass="btn-action-small btn-approve" CommandName="ApproveReq" CommandArgument='<%# Eval("RequestID") %>' />
                                <asp:Button ID="btnRejectReg" runat="server" Text="✖" CssClass="btn-action-small btn-reject" CommandName="RejectReq" CommandArgument='<%# Eval("RequestID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <div id="tab-leaves" class="tab-content">
            <div class="section-card">
                <asp:GridView ID="gvLeaves" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending leave applications." OnRowCommand="gvLeaves_RowCommand" DataKeyNames="RequestID">
                    <Columns>
                        <asp:BoundField DataField="UserCode" HeaderText="Emp ID"/>
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="LeaveName" HeaderText="Type" />
                        <asp:BoundField DataField="StartDate" HeaderText="Start" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="EndDate" HeaderText="End" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="TotalDays" HeaderText="Days" />
                        <asp:TemplateField HeaderText="Reason/Description">
                            <ItemTemplate>
                                <div class="cell-description" title='<%# Eval("Reason") %>'>
                                    <%# Eval("Reason") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnApproveLeave" runat="server" Text="✔" CssClass="btn-action-small btn-approve" CommandName="ApproveLeave" CommandArgument='<%# Eval("RequestID") %>' />
                                <asp:Button ID="btnRejectLeave" runat="server" Text="✖" CssClass="btn-action-small btn-reject" CommandName="RejectLeave" CommandArgument='<%# Eval("RequestID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </div>
</asp:Content>
