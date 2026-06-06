<%@ Page Title="Shift Assignment" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminShiftAssignment.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminShiftAssignment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style type="text/css">
        .dashboard-container {
            max-width: 1000px;
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        .section-card {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

        .section-title {
            color: #19658A;
            margin-top: 0;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 10px;
            margin-bottom: 20px;
            font-size: 22px;
        }

        .form-row {
            display: flex;
            gap: 20px;
            margin-bottom: 15px;
            flex-wrap: wrap;
        }

        .form-group {
            flex: 1;
            min-width: 200px;
            display: flex;
            flex-direction: column;
        }

            .form-group label {
                font-weight: bold;
                margin-bottom: 8px;
                color: #555;
                font-size: 13px;
            }

        .form-control {
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 14px;
        }

        .btn-action {
            padding: 12px 25px;
            font-size: 14px;
            font-weight: bold;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            color: white;
            background: #19658A;
            transition: 0.2s;
        }

            .btn-action:hover {
                background: #124B68;
                transform: translateY(-2px);
            }

        .beautiful-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
            margin-top: 20px;
        }

            .beautiful-grid th {
                background-color: #19658A;
                color: white;
                padding: 12px;
                text-align: left;
            }

            .beautiful-grid td {
                padding: 12px;
                border-bottom: 1px solid #eee;
            }

        /* --- FIX FOR SELECT2 WHITE-OUT EFFECT --- */
        .select2-container .select2-selection {
            border: 1px solid #ccc !important;
            border-radius: 6px !important;
            min-height: 40px !important;
        }

        .select2-results__option {
            color: #333 !important;
            background-color: #fff !important;
        }

            .select2-results__option[aria-selected="true"] {
                background-color: #f0f0f0 !important;
                color: #333 !important;
            }

        .select2-results__option--highlighted[aria-selected] {
            background-color: #19658A !important;
            color: white !important;
        }

        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: 38px !important;
            color: #333 !important;
            padding-left: 12px;
        }

        .select2-container--default .select2-selection--multiple .select2-selection__choice {
            background-color: #19658A !important;
            color: white !important;
            border: none !important;
            margin-top: 6px;
        }
    </style>

    <script type="text/javascript">
        // 1. Function to safely (re)build Select2
        function applySelect2() {
            var $ddlEmp = $('#<%= ddlEmployee.ClientID %>');
            var $ddlShift = $('#<%= ddlShift.ClientID %>');

            // Destroy existing instances if they survived partially to prevent clones
            if ($ddlEmp.hasClass("select2-hidden-accessible")) { $ddlEmp.select2('destroy'); }
            if ($ddlShift.hasClass("select2-hidden-accessible")) { $ddlShift.select2('destroy'); }

            // Initialize
            $ddlEmp.select2({ width: '100%', placeholder: "Select one or multiple employees..." });
            $ddlShift.select2({ width: '100%' });
        }

        // 2. Native ASP.NET AJAX Hook (Fires on Page Load AND after every UpdatePanel refresh)
        function pageLoad(sender, args) {
            applySelect2();
        }

        // 3. Event Delegation for Buttons (Anchors to the document so they survive UpdatePanels)
        $(document).ready(function () {

            // "Select All" Button Logic
            $(document).on('click', '#btnSelectAllEmp', function (e) {
                e.preventDefault();
                var $ddlEmp = $('#<%= ddlEmployee.ClientID %>');
                $ddlEmp.find('option').prop('selected', true);
                $ddlEmp.trigger('change');
            });

            // "Clear All" Button Logic
            $(document).on('click', '#btnClearAllEmp', function (e) {
                e.preventDefault();
                var $ddlEmp = $('#<%= ddlEmployee.ClientID %>');
                $ddlEmp.val(null).trigger('change');
            });

        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <asp:UpdatePanel ID="upRoster" runat="server">
        <ContentTemplate>
            <div class="dashboard-container">

                <div class="section-card">
                    <h2 class="section-title">📅 Monthly Shift Assignment</h2>

                    <div class="form-row">
                        <div class="form-group" style="flex: 2;">
                            <div style="display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 8px;">
                                <label style="margin-bottom: 0;">Select Employee(s)</label>
                                <div style="font-size: 11px;">
                                    <a href="#" id="btnSelectAllEmp" style="color: #19658A; text-decoration: none; font-weight: bold;">[ Select All ]</a> &nbsp;|&nbsp; 
                        <a href="#" id="btnClearAllEmp" style="color: #dc3545; text-decoration: none; font-weight: bold;">[ Clear ]</a>
                                </div>
                            </div>
                            <asp:ListBox ID="ddlEmployee" runat="server" SelectionMode="Multiple" CssClass="form-control"></asp:ListBox>
                        </div>
                        <div class="form-group">
                            <label>Target Month</label>
                            <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-control">
                                <asp:ListItem Value="1">January</asp:ListItem>
                                <asp:ListItem Value="2">February</asp:ListItem>
                                <asp:ListItem Value="3">March</asp:ListItem>
                                <asp:ListItem Value="4">April</asp:ListItem>
                                <asp:ListItem Value="5">May</asp:ListItem>
                                <asp:ListItem Value="6">June</asp:ListItem>
                                <asp:ListItem Value="7">July</asp:ListItem>
                                <asp:ListItem Value="8">August</asp:ListItem>
                                <asp:ListItem Value="9">September</asp:ListItem>
                                <asp:ListItem Value="10">October</asp:ListItem>
                                <asp:ListItem Value="11">November</asp:ListItem>
                                <asp:ListItem Value="12">December</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label>Target Year</label>
                            <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="form-row" style="align-items: flex-end;">
                        <div class="form-group" style="flex: 2;">
                            <label>Select Shift to Assign</label>
                            <asp:DropDownList ID="ddlShift" runat="server" CssClass="form-control"></asp:DropDownList>
                        </div>
                        <div class="form-group" style="flex: 1; flex-direction: row; gap: 10px; justify-content: flex-end;">
                            <asp:Button ID="btnViewAssignment" runat="server" Text="👁️ View Roster" CssClass="btn-action" Style="background: #6c757d; padding: 10px 15px;" OnClick="btnViewAssignment_Click" />
                            <asp:Button ID="btnAssignShift" runat="server" Text="⚡ Apply Shift Roster" CssClass="btn-action" OnClick="btnAssignShift_Click" />
                        </div>
                    </div>
                </div>

                <div class="section-card">
                    <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-bottom: 20px;">
                        <h3 class="section-title" style="border: none; margin: 0; padding: 0;">📋 Roster (<asp:Label ID="lblCurrentMonth" runat="server"></asp:Label>)</h3>

                        <div style="display: flex; gap: 10px;">
                            <asp:Button ID="btnForceSync" runat="server" Text="⚙️ Run Auto-Closure Sync" OnClick="btnForceSync_Click" CssClass="btn-action" Style="background: #dc3545; padding: 8px 15px; font-size: 12px;" OnClientClick="return confirm('This will forcefully auto-close any orphaned attendance punches for past dates based on their assigned shifts. Proceed?');" />
                            <asp:Button ID="btnRefreshGrid" runat="server" Text="🔄 Show All" OnClick="btnRefreshGrid_Click" CssClass="btn-action" Style="background: #28a745; padding: 8px 15px; font-size: 12px;" />
                        </div>
                    </div>

                    <asp:GridView ID="gvAssignments" runat="server" AutoGenerateColumns="False" CssClass="beautiful-grid" GridLines="None" EmptyDataText="No shift assignments found for this filter.">
                        <Columns>
                            <asp:BoundField DataField="EmployeeName" HeaderText="Employee" ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="UserCode" HeaderText="Emp Code" />
                            <asp:BoundField DataField="ShiftName" HeaderText="Assigned Shift" ItemStyle-ForeColor="#19658A" ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="EffectiveFromDate" HeaderText="Valid From" DataFormatString="{0:dd-MMM-yyyy}" />
                            <asp:BoundField DataField="EffectiveToDate" HeaderText="Valid Until" DataFormatString="{0:dd-MMM-yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>

            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
