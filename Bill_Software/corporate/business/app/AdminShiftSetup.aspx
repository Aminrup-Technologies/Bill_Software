<%@ Page Title="Shift Setup" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminShiftSetup.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminShiftSetup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* Main Container matching Attendance page */
        .dashboard-container {
            max-width: 950px;
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        /* Card styling matching the .history-section and .status-card */
        .section-card {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.08);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

        /* Consistent Header Styling */
        .section-title {
            color: #19658A;
            margin-top: 0;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 10px;
            margin-bottom: 20px;
            font-size: 22px;
        }

        /* Form Layout */
        .form-row {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-bottom: 15px;
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
                font-size: 14px;
            }

        .form-control {
            padding: 12px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s;
        }

            .form-control:focus {
                border-color: #19658A;
                outline: none;
            }

        /* Button Styling perfectly matched to .btn-punch */
        .btn-action {
            padding: 12px 30px;
            font-size: 16px;
            font-weight: bold;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
            color: white;
            background: linear-gradient(135deg, #19658A, #124B68);
        }

            .btn-action:hover:not(:disabled) {
                transform: translateY(-3px);
                box-shadow: 0 6px 15px rgba(0,0,0,0.2);
            }

        /* GridView Styling exactly as Attendance Page */
        .grid-style {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
            font-size: 14px;
        }

            .grid-style th {
                background-color: #19658A;
                color: white;
                padding: 12px;
                text-align: left;
            }

            .grid-style td {
                padding: 10px 12px;
                border-bottom: 1px solid #eee;
                vertical-align: middle;
            }

            .grid-style tr:hover {
                background-color: #f9f9f9;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">

        <div class="section-card">
            <h2 class="section-title">⚙️ Configure Shift & Tolerance Rules</h2>

            <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Style="display: block; margin-bottom: 15px;"></asp:Label>

            <div class="form-row">
                <div class="form-group" style="flex: 2;">
                    <label>Shift Name</label>
                    <asp:TextBox ID="txtShiftName" runat="server" CssClass="form-control" placeholder="e.g., General Shift" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Start Time</label>
                    <asp:TextBox ID="txtStartTime" runat="server" CssClass="form-control" TextMode="Time" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>End Time</label>
                    <asp:TextBox ID="txtEndTime" runat="server" CssClass="form-control" TextMode="Time" Required="true"></asp:TextBox>
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Late-In Grace (Mins)</label>
                    <asp:TextBox ID="txtGraceLate" runat="server" CssClass="form-control" TextMode="Number" Text="15" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Early-Out Grace (Mins)</label>
                    <asp:TextBox ID="txtGraceEarly" runat="server" CssClass="form-control" TextMode="Number" Text="10" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Half-Day Min. Hours</label>
                    <asp:TextBox ID="txtHalfDayHours" runat="server" CssClass="form-control" TextMode="Number" step="0.5" Text="4.0" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Full-Day Min. Hours</label>
                    <asp:TextBox ID="txtFullDayHours" runat="server" CssClass="form-control" TextMode="Number" step="0.5" Text="8.0" Required="true"></asp:TextBox>
                </div>
            </div>

            <h3 style="color: #d9534f; margin-top: 20px; font-size:16px; border-bottom:1px solid #eee; padding-bottom:5px;">Monthly Compliance & Penalties</h3>
            <div class="form-row" style="background: #fdfdfd; padding: 15px; border: 1px solid #eee; border-radius: 6px;">
                <div class="form-group">
                    <label>Max Lates Allowed / Month</label>
                    <asp:TextBox ID="txtMaxLateDays" runat="server" CssClass="form-control" TextMode="Number" Text="3"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Penalty (If Max Lates Exceeded)</label>
                    <asp:DropDownList ID="ddlLatePenalty" runat="server" CssClass="form-control">
                        <asp:ListItem Value="Half-Day">Deduct Half-Day</asp:ListItem>
                        <asp:ListItem Value="Absent">Mark Absent (Full Day)</asp:ListItem>
                        <asp:ListItem Value="None">No Auto Penalty (Manager Review)</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Max Early-Outs Allowed / Month</label>
                    <asp:TextBox ID="txtMaxEarlyDays" runat="server" CssClass="form-control" TextMode="Number" Text="3"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Penalty (If Max Early-Outs Exceeded)</label>
                    <asp:DropDownList ID="ddlEarlyPenalty" runat="server" CssClass="form-control">
                        <asp:ListItem Value="Half-Day">Deduct Half-Day</asp:ListItem>
                        <asp:ListItem Value="Absent">Mark Absent (Full Day)</asp:ListItem>
                        <asp:ListItem Value="None">No Auto Penalty</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="form-row" style="margin-top: 15px;">
                <div class="form-group" style="flex-direction: row; align-items: center; gap: 10px;">
                    <asp:CheckBox ID="chkAutoRegularize" runat="server" Checked="true" />
                    <label style="margin:0; color:#19658A; cursor:pointer;" for="<%= chkAutoRegularize.ClientID %>">
                        <b>Auto-Regularize</b> (Automatically mark 'Present' if late, BUT employee works their Total Full-Day Hours)
                    </label>
                </div>
            </div>
            
            <div class="form-row" style="margin-top: 5px;">
                <div class="form-group" style="flex-direction: row; align-items: center; gap: 10px;">
                    <asp:CheckBox ID="chkOutPunchMandatory" runat="server" Checked="true" />
                    <label style="margin:0; color:#d9534f; cursor:pointer;" for="<%= chkOutPunchMandatory.ClientID %>">
                        <b>Strict Out-Punch Required</b> (If missing an out-punch, block auto-regularization and deduct a Half-Day)
                    </label>
                </div>
            </div>

            <div style="text-align: right; margin-top: 25px;">
                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel Edit" CssClass="btn-action" Style="background: #6c757d; display:none;" OnClick="btnCancelEdit_Click" formnovalidate="true" />
                <asp:Button ID="btnSaveShift" runat="server" Text="💾 Save Shift Rule" CssClass="btn-action" OnClick="btnSaveShift_Click" />
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-title">📋 Active Shift Roster</h3>
            <asp:GridView ID="gvShifts" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No shifts found." DataKeyNames="ShiftID" OnRowCommand="gvShifts_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ShiftName" HeaderText="Shift Name" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="StartTime" HeaderText="IN Time" />
                    <asp:BoundField DataField="EndTime" HeaderText="OUT Time" />
                    <asp:BoundField DataField="GracePeriodLateInMins" HeaderText="Late Grace" />
                    <asp:BoundField DataField="MaxLateDaysAllowed" HeaderText="Permissible Lates" />
                    <asp:BoundField DataField="LatePenalty" HeaderText="Penalty" ItemStyle-ForeColor="#d9534f" ItemStyle-Font-Bold="true" />
                    
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" CommandName="EditShift" CommandArgument="<%# Container.DataItemIndex %>" Text="✏️ Edit" CssClass="btn-action" Style="padding: 5px 10px; font-size: 12px; background: #f39c12;" formnovalidate="true" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

    </div>
</asp:Content>
