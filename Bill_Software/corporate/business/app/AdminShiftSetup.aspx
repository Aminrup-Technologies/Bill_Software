<%@ Page Title="Shift Setup" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AdminShiftSetup.aspx.cs" Inherits="Bill_Software.corporate.business.app.AdminShiftSetup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* Main Container matching Attendance page */
        .dashboard-container { max-width: 950px; margin: 30px auto; font-family: 'Segoe UI', Arial, sans-serif; }
        
        /* Card styling matching the .history-section and .status-card */
        .section-card { 
            background: #ffffff; padding: 25px; border-radius: 10px; 
            box-shadow: 0 5px 15px rgba(0,0,0,0.08); border: 1px solid #eaeaea; margin-bottom: 30px; 
        }
        
        /* Consistent Header Styling */
        .section-title { 
            color: #19658A; margin-top: 0; border-bottom: 2px solid #f0f0f0; 
            padding-bottom: 10px; margin-bottom: 20px; font-size: 22px; 
        }
        
        /* Form Layout */
        .form-row { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 15px; }
        .form-group { flex: 1; min-width: 200px; display: flex; flex-direction: column; }
        .form-group label { font-weight: bold; margin-bottom: 8px; color: #555; font-size: 14px; }
        
        .form-control { 
            padding: 12px; border: 1px solid #ccc; border-radius: 6px; 
            font-size: 14px; transition: border-color 0.3s; 
        }
        .form-control:focus { border-color: #19658A; outline: none; }
        
        /* Button Styling perfectly matched to .btn-punch */
        .btn-action { 
            padding: 12px 30px; font-size: 16px; font-weight: bold; border: none; border-radius: 8px; cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s; color: white; 
            background: linear-gradient(135deg, #19658A, #124B68); 
        }
        .btn-action:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 6px 15px rgba(0,0,0,0.2); }
        
        /* GridView Styling exactly as Attendance Page */
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 14px; }
        .grid-style th { background-color: #19658A; color: white; padding: 12px; text-align: left; }
        .grid-style td { padding: 10px 12px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .grid-style tr:hover { background-color: #f9f9f9; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">
        
        <div class="section-card">
            <h2 class="section-title">⚙️ Create New Shift Rule</h2>
            
            <asp:Label ID="lblMessage" runat="server" Font-Bold="true" style="display:block; margin-bottom: 15px;"></asp:Label>

            <div class="form-row">
                <div class="form-group" style="flex: 2;">
                    <label>Shift Name (e.g., Night Shift, Morning Shift)</label>
                    <asp:TextBox ID="txtShiftName" runat="server" CssClass="form-control" placeholder="Enter Shift Name" Required="true"></asp:TextBox>
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
                    <label>Late Grace Period (Mins)</label>
                    <asp:TextBox ID="txtGraceLate" runat="server" CssClass="form-control" TextMode="Number" Text="15" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Early Out Grace Period (Mins)</label>
                    <asp:TextBox ID="txtGraceEarly" runat="server" CssClass="form-control" TextMode="Number" Text="15" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Half-Day Minimum Hours</label>
                    <asp:TextBox ID="txtHalfDayHours" runat="server" CssClass="form-control" TextMode="Number" step="0.5" Text="4.0" Required="true"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Full-Day Minimum Hours</label>
                    <asp:TextBox ID="txtFullDayHours" runat="server" CssClass="form-control" TextMode="Number" step="0.5" Text="8.0" Required="true"></asp:TextBox>
                </div>
            </div>

            <div style="text-align: right; margin-top: 15px;">
                <asp:Button ID="btnSaveShift" runat="server" Text="💾 Save Shift Rule" CssClass="btn-action" OnClick="btnSaveShift_Click" />
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-title">📋 Active Shift Roster</h3>
            <asp:GridView ID="gvShifts" runat="server" AutoGenerateColumns="False" CssClass="grid-style" GridLines="None" EmptyDataText="No shifts found.">
                <Columns>
                    <asp:BoundField DataField="ShiftID" HeaderText="ID" />
                    <asp:BoundField DataField="ShiftName" HeaderText="Shift Name" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                    <asp:BoundField DataField="EndTime" HeaderText="End Time" />
                    <asp:BoundField DataField="GracePeriodLateInMins" HeaderText="Late Grace (Mins)" />
                    <asp:BoundField DataField="HalfDayWorkingHours" HeaderText="Half Day (Hrs)" />
                    <asp:BoundField DataField="FullDayWorkingHours" HeaderText="Full Day (Hrs)" />
                </Columns>
            </asp:GridView>
        </div>

    </div>
</asp:Content>