<%@ Page Title="HR Override Dashboard" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminOverride.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminOverride" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .dashboard-container {
            max-width: 1100px;
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        /* TAB NAVIGATION STYLES */
        .tab-wrapper {
            border-bottom: 2px solid #eaeaea;
            margin-bottom: 20px;
            display: flex;
            gap: 5px;
        }

        .tab-btn {
            padding: 12px 25px;
            cursor: pointer;
            background: #f8f9fa;
            border: 1px solid #eaeaea;
            border-bottom: none;
            border-radius: 8px 8px 0 0;
            font-weight: bold;
            color: #666;
            transition: all 0.3s;
            margin-bottom: -2px;
        }

            .tab-btn:hover {
                background: #eee;
                color: #19658A;
            }

            .tab-btn.active {
                background: #fff;
                color: #19658A;
                border-top: 3px solid #19658A;
                border-left: 1px solid #eaeaea;
                border-right: 1px solid #eaeaea;
                height: 100%;
                padding-bottom: 14px;
            }

        .tab-content {
            display: none;
            animation: fadeIn 0.4s;
        }

            .tab-content.active {
                display: block;
            }

        @keyframes fadeIn {
            from {
                opacity: 0;
            }

            to {
                opacity: 1;
            }
        }

        .section-card {
            background: #ffffff;
            padding: 25px;
            border-radius: 0 0 10px 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            border-top: none;
        }

        .grid-style {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }

            .grid-style th {
                background-color: #19658A;
                color: white;
                padding: 12px;
                text-align: left;
            }

            .grid-style td {
                padding: 8px 12px;
                border-bottom: 1px solid #eee;
            }

        .btn-action-small {
            padding: 6px 12px;
            font-size: 12px;
            font-weight: bold;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            color: white;
            margin-right: 4px;
        }

        .btn-approve {
            background: linear-gradient(135deg, #34ce57, #28a745);
        }

        .btn-reject {
            background: linear-gradient(135deg, #ff6b6b, #dc3545);
        }

        .btn-resend {
            background: linear-gradient(135deg, #17a2b8, #138496);
        }

        .cell-description {
            max-width: 150px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            cursor: help;
        }

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
    </style>

    <script type="text/javascript">
        function switchTab(tabId, btn) {
            document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            document.getElementById(tabId).classList.add('active');
            btn.classList.add('active');
            localStorage.setItem('activeHROverrideTab', tabId);
        }

        window.onload = function () {
            var activeTab = localStorage.getItem('activeHROverrideTab') || 'tab-all-leaves';
            var btn = document.querySelector('[onclick*="' + activeTab + '"]');
            if (btn) switchTab(activeTab, btn);
        };
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        <h2>HR Override Dashboard</h2>
        <p style="color: #666; margin-bottom: 20px;">Force-approve requests or resend notifications to managers.</p>

        <asp:Panel ID="PanelOK" runat="server" Visible="False" Style="padding: 15px; margin-bottom: 20px; border-radius: 6px; text-align: center; background: #EEFFDD; border: 1px solid #006600;">
            <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="PanelError" runat="server" Visible="False" Style="padding: 15px; margin-bottom: 20px; border-radius: 6px; text-align: center; background: #FFDDDD; border: 1px solid #FF3300;">
            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
        </asp:Panel>

        <div class="tab-wrapper">
            <div class="tab-btn active" onclick="switchTab('tab-all-leaves', this)">🏖️ All Pending Leaves</div>
            <div class="tab-btn" onclick="switchTab('tab-all-reg', this)">⏱️ All Pending Corrections</div>
        </div>

        <div id="tab-all-leaves" class="tab-content active">
            <div class="section-card">
                <asp:GridView ID="gvAllLeaves" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending leave applications system-wide." OnRowCommand="gvAllLeaves_RowCommand" DataKeyNames="RequestID, UserCode, TotalDays, StartDate, EndDate, ManagerID, EmpName">
                    <Columns>
                        <asp:BoundField DataField="EmpName" HeaderText="Employee" />
                        <asp:BoundField DataField="ManagerName" HeaderText="Assigned Manager" />
                        <asp:BoundField DataField="LeaveName" HeaderText="Type" />
                        <asp:BoundField DataField="StartDate" HeaderText="Start" DataFormatString="{0:dd-MMM}" />
                        <asp:BoundField DataField="TotalDays" HeaderText="Days" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button ID="btnApprove" runat="server" Text="✔ Override" CssClass="btn-action-small btn-approve" CommandName="ForceApprove" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Force approve this leave?');" />
                                <asp:Button ID="btnReject" runat="server" Text="✖" CssClass="btn-action-small btn-reject" CommandName="ForceReject" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Force reject this leave?');" />
                                <asp:Button ID="btnResend" runat="server" Text="📧 Resend Alert" CssClass="btn-action-small btn-resend" CommandName="ResendAlert" CommandArgument='<%# Eval("RequestID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <div id="tab-all-reg" class="tab-content">
            <div class="section-card">
                <asp:GridView ID="gvAllRegs" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No pending attendance corrections system-wide." OnRowCommand="gvAllRegs_RowCommand" DataKeyNames="RequestID, UserCode, AttendanceDate, RequestedInTime, RequestedOutTime, ManagerID, EmpName, Reason">
                    <Columns>
                        <asp:BoundField DataField="EmpName" HeaderText="Employee" />
                        <asp:BoundField DataField="ManagerName" HeaderText="Assigned Manager" />
                        <asp:BoundField DataField="AttendanceDate" HeaderText="Date" DataFormatString="{0:dd-MMM}" />
                        <asp:BoundField DataField="RequestedInTime" HeaderText="Req IN" />
                        <asp:BoundField DataField="RequestedOutTime" HeaderText="Req OUT" />
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:Button ID="btnApproveReg" runat="server" Text="✔ Override" CssClass="btn-action-small btn-approve" CommandName="ForceApprove" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Force approve this correction?');" />
                                <asp:Button ID="btnRejectReg" runat="server" Text="✖" CssClass="btn-action-small btn-reject" CommandName="ForceReject" CommandArgument='<%# Eval("RequestID") %>' OnClientClick="return confirm('Force reject this correction?');" />
                                <asp:Button ID="btnResendReg" runat="server" Text="📧 Resend Alert" CssClass="btn-action-small btn-resend" CommandName="ResendAlert" CommandArgument='<%# Eval("RequestID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
