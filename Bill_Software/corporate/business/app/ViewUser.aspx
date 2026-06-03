<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ViewUser.aspx.cs" MaintainScrollPositionOnPostback="true" Inherits="Bill_Software.corporate.business.app.WebForm80" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        /* Modern Card Layout */
        .list-container {
            width: 100%;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .user-card {
            display: flex;
            align-items: center;
            background: #fff;
            border: 1px solid #e1e8f0;
            border-radius: 8px;
            padding: 15px 20px;
            margin-bottom: 12px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.02);
            transition: box-shadow 0.2s;
        }

            .user-card:hover {
                box-shadow: 0 4px 10px rgba(0,0,0,0.08);
            }

        /* Flex Sections */
        .card-profile {
            display: flex;
            align-items: center;
            gap: 15px;
            width: 25%;
        }

        .card-contact {
            display: flex;
            flex-direction: column;
            width: 25%;
            font-size: 13px;
            color: #555;
        }

        .card-status {
            display: flex;
            flex-direction: column;
            gap: 5px;
            width: 15%;
            align-items: flex-start;
        }

        .card-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 6px;
            width: 35%;
            justify-content: flex-end;
        }

        /* Edit Mode Layout */
        .edit-card {
            background: #fdfdfe;
            border: 1px solid #b8daff;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 12px;
        }

        .edit-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 15px;
            margin-bottom: 15px;
        }

        .edit-group {
            display: flex;
            flex-direction: column;
            gap: 4px;
            font-size: 12px;
            font-weight: 600;
            color: #555;
        }

        .form-control {
            padding: 6px 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            font-size: 13px;
        }

        /* Typography & Badges */
        .user-name {
            font-size: 16px;
            font-weight: bold;
            color: #153e75;
            margin: 0;
        }

        .user-id {
            font-size: 12px;
            color: #888;
        }

        .role-badge {
            font-size: 11px;
            font-weight: bold;
            color: #19658A;
        }

        .badge {
            padding: 3px 8px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: bold;
        }

        .badge-success {
            background: #e8ffe8;
            color: #008000;
            border: 1px solid #b3e6b3;
        }

        .badge-danger {
            background: #ffe8e8;
            color: #b30000;
            border: 1px solid #e6b3b3;
        }

        .badge-warning {
            background: #fff3cd;
            color: #856404;
            border: 1px solid #ffeeba;
        }

        /* Refined Outline Buttons */
        .action-btn {
            padding: 5px 12px;
            border-radius: 20px;
            font-size: 11px;
            font-weight: 600;
            cursor: pointer;
            text-decoration: none;
            text-align: center;
            transition: all 0.2s;
            background: transparent;
        }

        .btn-primary {
            border: 1px solid #007bff;
            color: #007bff !important;
        }

            .btn-primary:hover {
                background: #007bff;
                color: #fff !important;
            }

        .btn-danger {
            border: 1px solid #dc3545;
            color: #dc3545 !important;
        }

            .btn-danger:hover {
                background: #dc3545;
                color: #fff !important;
            }

        .btn-warning {
            border: 1px solid #ffc107;
            color: #856404 !important;
        }

            .btn-warning:hover {
                background: #ffc107;
                color: #212529 !important;
            }

        .btn-success {
            border: 1px solid #28a745;
            background: #28a745;
            color: #fff !important;
        }

            .btn-success:hover {
                opacity: 0.85;
            }

        .btn-secondary {
            border: 1px solid #6c757d;
            background: #6c757d;
            color: #fff !important;
        }

            .btn-secondary:hover {
                opacity: 0.85;
            }

        /* Filter Pill Buttons */
        .filter-group {
            display: flex;
            gap: 10px;
            margin-bottom: 15px;
        }

        .filter-btn {
            padding: 6px 16px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 600;
            background: #fff;
            border: 1px solid #c3d4e6;
            color: #555;
            cursor: pointer;
            transition: all 0.2s;
        }

            .filter-btn:hover {
                background: #f4f7fa;
                border-color: #19658A;
                color: #19658A;
            }

            .filter-btn.active {
                background: #19658A;
                color: #fff;
                border-color: #19658A;
                box-shadow: 0 2px 4px rgba(25,101,138,0.3);
            }

        @keyframes fadeSlideIn {
            from {
                opacity: 0;
                transform: translateY(-20px);
            }

            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Beautiful Modal Table Styling */
        .modal-table-wrapper {
            width: 100%;
            overflow-x: auto;
            border-radius: 8px;
            border: 1px solid #e1e8f0;
            background: #fff;
        }

        .beautiful-grid {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            font-size: 13px;
            text-align: left;
            margin: 0;
        }

            .beautiful-grid thead th {
                background-color: #f4f7f9; /* Soft tech-gray background */
                color: #153e75; /* Deep navy text */
                padding: 14px 16px;
                font-weight: 700;
                text-transform: uppercase;
                font-size: 11px;
                letter-spacing: 0.5px;
                border-bottom: 2px solid #e1e8f0;
            }

            .beautiful-grid tbody td {
                padding: 14px 16px;
                border-bottom: 1px solid #f0f4f8;
                color: #444;
                vertical-align: middle;
                line-height: 1.5;
            }

            /* Remove border from the very last row so it doesn't double-up with the wrapper */
            .beautiful-grid tbody tr:last-child td {
                border-bottom: none;
            }

            /* Smooth hover effect */
            .beautiful-grid tbody tr {
                transition: background-color 0.2s ease;
            }

                .beautiful-grid tbody tr:hover {
                    background-color: #f8fbfd;
                }

        .geo-btn {
            background-color: #f8f9fa;
            color: #19658A;
            border: 1px solid #19658A;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 13px;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 5px;
        }

            .geo-btn:hover {
                background-color: #19658A;
                color: #ffffff;
                box-shadow: 0 4px 8px rgba(25, 101, 138, 0.2);
            }
    </style>

    <script type="text/javascript">
        function confirmSendCredentials() {
            return confirm("Are you sure you want to generate a new temporary password and email it to this user?");
        }

        function showSessionHistory(userId, userName) {
            document.getElementById('sessionModalName').innerText = userName;

            // FIX: Collapsed to a single line
            document.getElementById('sessionTableBody').innerHTML = "<tr><td colspan='4' style='text-align: center; padding: 20px; color: #666;'>⏳ Loading session data...</td></tr>";
            document.getElementById('sessionHistoryModal').style.display = 'block';

            fetch('ViewUser.aspx/GetSessionHistory', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ userId: parseInt(userId) })
            })
            .then(response => response.json())
            .then(data => {
                var sessions = JSON.parse(data.d);
                var tbody = document.getElementById('sessionTableBody');
                tbody.innerHTML = "";

                if (sessions.length === 0) {
                    // FIX: Collapsed to a single line
                    tbody.innerHTML = "<tr><td colspan='4' style='text-align: center; padding: 20px; color: #666;'>No login history found for this user.</td></tr>";
                    return;
                }

                sessions.forEach(s => {
                    // Create a nice active status indicator
                    var sessionStatus = s.IsActive
                        ? "<span style='color: #28a745; font-size: 11px; font-weight: bold;'>● Live Session</span>"
                        : "<span style='color: #6c757d; font-size: 11px;'>Ended</span>";

                    // Backticks (`) allow multi-line strings, so this part remains as is
                    var row = `<tr>
                <td style="font-weight: 500; color: #333;">${s.LoginTime}</td>
                <td>${s.LastHeartbeat}<br />${sessionStatus}</td>
                <td style="font-family: monospace; color: #19658A;">${s.IPAddress}</td>
                <td>
                    <div style="max-width: 250px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; font-size: 12px; color: #555;" title="${s.UserAgent}">${s.UserAgent}</div>
                </td>
            </tr>`;
                    tbody.innerHTML += row;
                });
            })
            .catch(error => {
                console.error('Error fetching sessions:', error);
                // FIX: Collapsed to a single line
                document.getElementById('sessionTableBody').innerHTML = "<tr><td colspan='4' style='text-align: center; color: red;'>Failed to load data.</td></tr>";
            });
        }
    </script>
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">View User</span></td>
        </tr>

        <tr>
            <td colspan="6">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="">
                    <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server" ForeColor="Green"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BackColor="#FFEEEE" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="">
                    <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red"></asp:Label>
                </asp:Panel>
            </td>
        </tr>

        <%--<tr>
            <td colspan="6" style="">Employee Id:
                <asp:DropDownList ID="ddlEmpId" runat="server" Width="220px" Font-Size="12px" AutoPostBack="True" OnSelectedIndexChanged="ddlEmpId_SelectedIndexChanged"></asp:DropDownList>
                &nbsp;<asp:Button ID="btnRefresh" runat="server" Text="Refresh" OnClick="btnRefresh_Click" CssClass="action-btn btn-unlock" />
            </td>
        </tr>--%>

        <tr>
            <td colspan="6" style="padding-bottom: 15px; padding-top: 10px; border-bottom: 1px solid #eee;">

                <div class="filter-group">
                    <asp:Button ID="btnFilterAll" runat="server" Text="All Users" CssClass="filter-btn active" OnClick="btnFilter_Click" CommandArgument="All" />
                    <asp:Button ID="btnFilterActive" runat="server" Text="Active" CssClass="filter-btn" OnClick="btnFilter_Click" CommandArgument="Active" />
                    <asp:Button ID="btnFilterInactive" runat="server" Text="Inactive" CssClass="filter-btn" OnClick="btnFilter_Click" CommandArgument="Inactive" />
                    <asp:Button ID="btnFilterLocked" runat="server" Text="Locked" CssClass="filter-btn" OnClick="btnFilter_Click" CommandArgument="Locked" />
                </div>

                <div style="display: flex; gap: 10px; align-items: center;">
                    <asp:TextBox ID="txtSearch" runat="server" Width="350px" CssClass="form-control"
                        Placeholder="Search by Name, User ID, Email, or Phone..."></asp:TextBox>

                    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click"
                        CssClass="action-btn btn-primary" Style="padding: 6px 18px; font-size: 13px;" />

                    <asp:Button ID="btnClear" runat="server" Text="Clear" OnClick="btnClear_Click"
                        CssClass="action-btn btn-secondary" Style="padding: 6px 18px; font-size: 13px;" />
                </div>
            </td>
        </tr>

        <tr>
            <td colspan="6">
                <div class="list-container">
                    <asp:ListView ID="lvUsers" runat="server" DataKeyNames="Id"
                        OnItemCommand="lvUsers_ItemCommand"
                        OnItemDataBound="lvUsers_ItemDataBound"
                        OnItemEditing="lvUsers_ItemEditing"
                        OnItemCanceling="lvUsers_ItemCanceling"
                        OnItemUpdating="lvUsers_ItemUpdating">

                        <LayoutTemplate>
                            <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
                        </LayoutTemplate>

                        <ItemTemplate>
                            <div class="user-card">
                                <div class="card-profile">
                                    <asp:Image ID="imgThumbnail" runat="server" Width="45px" Height="45px" Style="border-radius: 50%; object-fit: cover; border: 2px solid #e1e8f0;"
                                        ImageUrl='<%# string.IsNullOrEmpty(Convert.ToString(Eval("ProfilePictureUrl"))) ? ResolveUrl("~/corporate/business/WebImages/representative.png") : ResolveUrl(Convert.ToString(Eval("ProfilePictureUrl"))) %>' />
                                    <div>
                                        <p class="user-name"><%# Eval("Name") %></p>
                                        <span class="user-id">ID: <%# Eval("User_Id") %></span><br />
                                        <span class="role-badge"><%# string.IsNullOrEmpty(Convert.ToString(Eval("RoleName"))) ? "Unassigned" : Eval("RoleName") %></span>

                                        <div style="font-size: 11px; color: #666; margin-top: 6px; line-height: 1.4;">
                                            🏢 <%# Eval("DepartmentName") == DBNull.Value ? "No Department" : Eval("DepartmentName") %>
                                            <br />
                                            💼 <%# Eval("DesignationName") == DBNull.Value ? "No Designation" : Eval("DesignationName") %><br />
                                            👤 Manager: <strong><%# Eval("ManagerName") == DBNull.Value ? "None" : Eval("ManagerName") %></strong>
                                        </div>
                                    </div>
                                </div>

                                <div class="card-contact">
                                    <span style="margin-bottom: 4px;">📧 <a href='mailto:<%# Eval("Email") %>' style="color: #007bff; text-decoration: none;"><%# Eval("Email") %></a></span>
                                    <span>📞 <%# Eval("Phone_no") %></span>
                                </div>

                                <div class="card-status">
                                    <asp:Label ID="lblStatus" runat="server" Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active Account" : "Inactive Account" %>' CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-danger" %>'></asp:Label>

                                    <div style="margin-top: 8px;" onclick="showSessionHistory('<%# Eval("Id") %>', '<%# Eval("Name") %>')">
                                        <%# GetOnlineStatusHtml(Eval("LatestHeartbeat")) %>
                                    </div>

                                    <span style="font-size: 11px; color: #888; margin-top: 5px;" title="Geo Tagging">📍 <%# (Eval("RequireGeoTagging") != DBNull.Value && Convert.ToBoolean(Eval("RequireGeoTagging"))) ? "Geo: ON" : "Geo: OFF" %></span>
                                </div>

                                <div class="card-actions">
                                    <asp:LinkButton ID="lnkEdit" runat="server" CommandName="Edit" CssClass="action-btn btn-primary">Edit</asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggleActive" runat="server" CommandName="ToggleActive" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                    <asp:LinkButton ID="lnkReset" runat="server" CommandName="ResetPassword" CommandArgument='<%# Eval("Id") %>' OnClientClick="return confirmSendCredentials();" CssClass="action-btn btn-primary">Email Access</asp:LinkButton>
                                    <asp:LinkButton ID="lnkLock" runat="server" CommandName="ToggleLock" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                    <asp:LinkButton ID="lnkMenuEdit" runat="server" CommandName="MenuEdit" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn btn-primary">Menu</asp:LinkButton>

                                    <button type="button"
                                        class="action-btn"
                                        style="background-color: #f8f9fa; color: #19658A; border: 1px solid #19658A;"
                                        title="Set Geo-Fence Boundaries"
                                        onclick="openGeoFenceModal('<%# Eval("Id") %>', '<%# Eval("GeoFenceLat") %>', '<%# Eval("GeoFenceLng") %>', '<%# Eval("GeoFenceRadius") %>')">
                                        📍 Geo-Fence
   
                                    </button>
                                </div>
                            </div>
                        </ItemTemplate>

                        <EditItemTemplate>
                            <div class="edit-card">
                                <div style="border-bottom: 1px solid #ccc; padding-bottom: 10px; margin-bottom: 15px;">
                                    <strong style="color: #153e75; font-size: 15px;">Editing User: <%# Eval("User_Id") %></strong>
                                </div>

                                <div class="edit-grid">
                                    <div class="edit-group">
                                        <label>Employee Name</label>
                                        <asp:TextBox ID="txtName" runat="server" Text='<%# Bind("Name") %>' CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="edit-group">
                                        <label>Email Address</label>
                                        <asp:TextBox ID="txtEmail" runat="server" Text='<%# Bind("Email") %>' CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="edit-group">
                                        <label>Phone Number</label>
                                        <asp:TextBox ID="txtPhone" runat="server" Text='<%# Bind("Phone_no") %>' CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="edit-group">
                                        <label>System Role</label>
                                        <asp:DropDownList ID="ddlGridRole" runat="server" CssClass="form-control"></asp:DropDownList>
                                        <asp:HiddenField ID="hfCurrentRoleId" runat="server" Value='<%# Eval("RoleId") %>' />
                                    </div>
                                    <div class="edit-group">
                                        <label>Department</label>
                                        <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-control"></asp:DropDownList>
                                        <asp:HiddenField ID="hfDeptId" runat="server" Value='<%# Eval("DepartmentID") %>' />
                                    </div>
                                    <div class="edit-group">
                                        <label>Designation</label>
                                        <asp:DropDownList ID="ddlDesignation" runat="server" CssClass="form-control"></asp:DropDownList>
                                        <asp:HiddenField ID="hfDesigId" runat="server" Value='<%# Eval("DesignationID") %>' />
                                    </div>
                                    <div class="edit-group">
                                        <label>Reporting Manager</label>
                                        <asp:DropDownList ID="ddlManager" runat="server" CssClass="form-control"></asp:DropDownList>
                                        <asp:HiddenField ID="hfManagerId" runat="server" Value='<%# Eval("ReportingManagerId") %>' />
                                    </div>
                                </div>

                                <div class="edit-grid" style="grid-template-columns: auto auto 1fr;">
                                    <div class="edit-group" style="flex-direction: row; align-items: center; gap: 8px;">
                                        <asp:CheckBox ID="chkEmailVerified" runat="server" Checked='<%# Eval("EmailVerified") != DBNull.Value && Convert.ToBoolean(Eval("EmailVerified")) %>' />
                                        <label style="margin: 0;">Email Verified</label>
                                    </div>
                                    <div class="edit-group" style="flex-direction: row; align-items: center; gap: 8px;">
                                        <asp:CheckBox ID="chkMustChangePwd" runat="server" Checked='<%# Eval("MustChangePassword") != DBNull.Value && Convert.ToBoolean(Eval("MustChangePassword")) %>' />
                                        <label style="margin: 0;">Force Password Change</label>
                                    </div>

                                    <div class="edit-group" style="flex-direction: row; align-items: center; gap: 8px;">
                                        <asp:CheckBox ID="chkRequireGeo" runat="server" Checked='<%# Eval("RequireGeoTagging") != DBNull.Value && Convert.ToBoolean(Eval("RequireGeoTagging")) %>' />
                                        <label style="margin: 0;">Require Geo-Tagging</label>
                                    </div>

                                    <div class="edit-group" style="flex-direction: row; align-items: center; gap: 8px;">
                                        <asp:CheckBox ID="chkEmails" runat="server" Checked='<%# Eval("EnableEmailAlerts") != DBNull.Value && Convert.ToBoolean(Eval("EnableEmailAlerts")) %>' />
                                        <label style="margin: 0;">Email Alerts</label>
                                    </div>

                                    <div class="edit-group" style="flex-direction: row; align-items: center; gap: 8px;">
                                        <asp:CheckBox ID="chkWhatsApp" runat="server" Checked='<%# Eval("EnableWhatsAppAlerts") != DBNull.Value && Convert.ToBoolean(Eval("EnableWhatsAppAlerts")) %>' />
                                        <label style="margin: 0;">WhatsApp Alerts</label>
                                    </div>

                                    <div style="display: flex; justify-content: flex-end; gap: 10px; width: 100%;">
                                        <asp:LinkButton ID="lnkCancel" runat="server" CommandName="Cancel" CssClass="action-btn btn-secondary" Style="border-radius: 4px;">Cancel</asp:LinkButton>
                                        <asp:LinkButton ID="lnkUpdate" runat="server" CommandName="Update" CssClass="action-btn btn-success" Style="border-radius: 4px;">Save Changes</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </EditItemTemplate>

                    </asp:ListView>
                </div>
                <%--<asp:GridView ID="gvUsers" runat="server"
                    AutoGenerateColumns="False"
                    DataKeyNames="Id"
                    CssClass="user-grid"
                    GridLines="None"
                    Width="100%"
                    OnRowCommand="gvUsers_RowCommand"
                    OnRowDataBound="gvUsers_RowDataBound"
                    OnRowEditing="gvUsers_RowEditing"
                    OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                    OnRowUpdating="gvUsers_RowUpdating">

                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="true" ItemStyle-Width="30px" />

                        <asp:TemplateField HeaderText="Pic" ItemStyle-Width="45px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Image ID="imgThumbnail" runat="server" Width="36px" Height="36px" Style="border-radius: 50%; object-fit: cover; border: 2px solid #e1e8f0;"
                                    ImageUrl='<%# string.IsNullOrEmpty(Convert.ToString(Eval("ProfilePictureUrl"))) ? ResolveUrl("~/corporate/business/WebImages/representative.png") : ResolveUrl(Convert.ToString(Eval("ProfilePictureUrl"))) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="User_Id" HeaderText="User Id" ReadOnly="true" ItemStyle-Width="90px" />

                        <asp:TemplateField HeaderText="Employee Name" ItemStyle-Width="140px">
                            <ItemTemplate><strong><%# Eval("Name") %></strong></ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtName" runat="server" Text='<%# Bind("Name") %>' Width="120px" CssClass="form-control"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="System Role" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="lblRole" runat="server" Font-Bold="true" ForeColor="#19658A"
                                    Text='<%# string.IsNullOrEmpty(Convert.ToString(Eval("RoleName"))) ? "Unassigned" : Eval("RoleName") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlGridRole" runat="server" Width="110px"></asp:DropDownList>
                                <asp:HiddenField ID="hfCurrentRoleId" runat="server" Value='<%# Eval("RoleId") %>' />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email" ItemStyle-Width="180px">
                            <ItemTemplate><a href='mailto:<%# Eval("Email") %>' style="color: #007bff; text-decoration: none;"><%# Eval("Email") %></a></ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEmail" runat="server" Text='<%# Bind("Email") %>' Width="160px"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Phone" ItemStyle-Width="100px">
                            <ItemTemplate><%# Eval("Phone_no") %></ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtPhone" runat="server" Text='<%# Bind("Phone_no") %>' Width="90px"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Verified" ItemStyle-Width="70px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Label ID="lblEmailVer" runat="server" Font-Bold="true"
                                    Text='<%# Eval("EmailVerified") != DBNull.Value && Convert.ToBoolean(Eval("EmailVerified")) ? "Yes" : "No" %>'
                                    ForeColor='<%# Eval("EmailVerified") != DBNull.Value && Convert.ToBoolean(Eval("EmailVerified")) ? System.Drawing.Color.Green : System.Drawing.Color.Red %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:CheckBox ID="chkEmailVerified" runat="server"
                                    Checked='<%# Eval("EmailVerified") != DBNull.Value && Convert.ToBoolean(Eval("EmailVerified")) %>' />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="LastLogin" HeaderText="Last Login" ReadOnly="true" DataFormatString="{0:MMM dd, HH:mm}" ItemStyle-Width="100px" ItemStyle-ForeColor="#888888" />

                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Label ID="lblStatus" runat="server"
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "status-active" : "status-inactive" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="260px">
                            <ItemTemplate>
                                <div class="action-group">
                                    <asp:LinkButton ID="lnkEdit" runat="server" CommandName="Edit" CssClass="action-btn btn-edit">Edit</asp:LinkButton>
                                    <asp:LinkButton ID="lnkToggleActive" runat="server" CommandName="ToggleActive" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                    <asp:LinkButton ID="lnkReset" runat="server" CommandName="ResetPassword" CommandArgument='<%# Eval("Id") %>' OnClientClick="return confirmSendCredentials();" CssClass="action-btn btn-reset">Email Access</asp:LinkButton>
                                    <asp:LinkButton ID="lnkLock" runat="server" CommandName="ToggleLock" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                    <asp:LinkButton ID="lnkMenuEdit" runat="server" CommandName="MenuEdit" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn btn-menu-edit">Menu Auth</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div class="action-group">
                                    <asp:LinkButton ID="lnkUpdate" runat="server" CommandName="Update" CssClass="action-btn btn-save">Save Changes</asp:LinkButton>
                                    <asp:LinkButton ID="lnkCancel" runat="server" CommandName="Cancel" CssClass="action-btn btn-cancel">Cancel</asp:LinkButton>
                                </div>
                            </EditItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>--%>
            </td>
        </tr>
    </table>
    <div id="sessionHistoryModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.6); z-index: 99998;">
        <div style="background: #fff; width: 95%; max-width: 800px; margin: 5% auto; border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.4); overflow: hidden; font-family: 'Segoe UI', Arial, sans-serif; animation: fadeSlideIn 0.3s ease-out;">

            <div style="background-color: #19658A; color: white; padding: 15px 20px; font-weight: bold; font-size: 16px; display: flex; justify-content: space-between; align-items: center;">
                <span>📡 Login Activity: <span id="sessionModalName" style="color: #e1effe;"></span></span>
                <span style="cursor: pointer; font-size: 20px;" onclick="document.getElementById('sessionHistoryModal').style.display='none';">✖</span>
            </div>

            <div style="padding: 10px; max-height: 70vh; overflow-y: auto;">
                <div class="modal-table-wrapper">
                    <table class="beautiful-grid">
                        <thead>
                            <tr>
                                <th>Login Time</th>
                                <th>Last Active (Heartbeat)</th>
                                <th>IP Address</th>
                                <th>Device / Browser</th>
                            </tr>
                        </thead>
                        <tbody id="sessionTableBody">
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div id="geoFenceModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.6); z-index: 99999;">
        <div style="background: #fff; width: 90%; max-width: 600px; margin: 5% auto; border-radius: 8px; overflow: hidden; box-shadow: 0 5px 15px rgba(0,0,0,0.3);">
            <div style="background: #19658A; color: #fff; padding: 15px; font-weight: bold; display: flex; justify-content: space-between;">
                <span>📍 Set Geo-Fence for Employee</span>
                <span style="cursor: pointer;" onclick="closeGeoFenceModal()">✖</span>
            </div>

            <div style="padding: 15px;">
                <input type="hidden" id="hfGeoUserId" />

                <div style="margin-bottom: 10px; display: flex; gap: 8px;">
                    <input type="text" id="txtSearchLocation" placeholder="Search city, area, or landmark..." style="flex: 1; padding: 8px; border: 1px solid #ccc; border-radius: 4px;" onkeypress="if(event.keyCode==13) { searchLocation(); return false; }" />
                    <button type="button" onclick="searchLocation()" style="background: #19658A; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer;" title="Search">🔍 Search</button>
                    <button type="button" onclick="getCurrentLocation()" style="background: #6c757d; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer;" title="My Current Location">🎯</button>
                </div>

                <div style="margin-bottom: 10px;">
                    <label style="font-weight: bold; font-size: 13px; color: #333;">Allowed Radius: <span id="lblRadius" style="color: #19658A; font-size: 16px;">100</span> meters</label>
                    <input type="range" id="rngRadius" min="10" max="1000" value="100" style="width: 100%; margin-top: 5px;" oninput="updateMapCircle()" />
                </div>

                <div id="map" style="height: 300px; width: 100%; border: 1px solid #ccc; border-radius: 4px;"></div>

                <div style="margin-top: 15px; display: flex; gap: 10px;">
                    <input type="text" id="txtLat" placeholder="Latitude" readonly style="flex: 1; padding: 8px; background: #f0f0f0; border: 1px solid #ddd; border-radius: 4px;" />
                    <input type="text" id="txtLng" placeholder="Longitude" readonly style="flex: 1; padding: 8px; background: #f0f0f0; border: 1px solid #ddd; border-radius: 4px;" />
                </div>

                <div style="margin-top: 15px; text-align: right;">
                    <button type="button" onclick="saveGeoFence()" style="background: #28a745; color: white; border: none; padding: 10px 20px; border-radius: 4px; font-weight: bold; cursor: pointer;">Save Geo-Fence</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        var map, marker, circle;

        function openGeoFenceModal(userId, currentLat, currentLng, currentRadius) {
            document.getElementById('geoFenceModal').style.display = 'block';
            document.getElementById('hfGeoUserId').value = userId;

            // Clear search box on open
            document.getElementById('txtSearchLocation').value = '';

            // Default to Jamshedpur/Kolkata area if no data exists
            var lat = currentLat || 22.8046;
            var lng = currentLng || 86.2029;
            var radius = currentRadius || 100;

            document.getElementById('txtLat').value = lat;
            document.getElementById('txtLng').value = lng;
            document.getElementById('rngRadius').value = radius;
            document.getElementById('lblRadius').innerText = radius;

            if (!map) {
                map = L.map('map').setView([lat, lng], 16);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '© OpenStreetMap'
                }).addTo(map);

                map.on('click', function (e) {
                    updatePin(e.latlng.lat, e.latlng.lng);
                });
            } else {
                map.setView([lat, lng], 16);
            }

            updatePin(lat, lng);
            setTimeout(function () { map.invalidateSize(); }, 200);
        }

        function updatePin(lat, lng) {
            document.getElementById('txtLat').value = lat;
            document.getElementById('txtLng').value = lng;
            var radius = parseInt(document.getElementById('rngRadius').value);

            if (marker) map.removeLayer(marker);
            if (circle) map.removeLayer(circle);

            marker = L.marker([lat, lng]).addTo(map);
            circle = L.circle([lat, lng], { radius: radius, color: '#19658A', fillOpacity: 0.3 }).addTo(map);
        }

        function updateMapCircle() {
            var r = document.getElementById('rngRadius').value;
            document.getElementById('lblRadius').innerText = r;
            var lat = document.getElementById('txtLat').value;
            var lng = document.getElementById('txtLng').value;
            if (lat && lng) {
                updatePin(lat, lng);
            }
        }

        // --- NEW: HTML5 Geolocation ---
        function getCurrentLocation() {
            if (navigator.geolocation) {
                document.getElementById('txtSearchLocation').value = "Locating...";
                navigator.geolocation.getCurrentPosition(function (position) {
                    var lat = position.coords.latitude;
                    var lng = position.coords.longitude;
                    map.setView([lat, lng], 17);
                    updatePin(lat, lng);
                    document.getElementById('txtSearchLocation').value = "Current Location";
                }, function (error) {
                    alert("Error getting location: Please ensure location services are allowed in your browser.");
                    document.getElementById('txtSearchLocation').value = "";
                }, { enableHighAccuracy: true });
            } else {
                alert("Geolocation is not supported by this browser.");
            }
        }

        // --- NEW: OpenStreetMap Nominatim Search ---
        function searchLocation() {
            var query = document.getElementById('txtSearchLocation').value.trim();
            if (!query || query === "Locating..." || query === "Current Location") return;

            // Using free OSM geocoding
            var url = 'https://nominatim.openstreetmap.org/search?format=json&limit=1&q=' + encodeURIComponent(query);

            fetch(url)
                .then(res => res.json())
                .then(data => {
                    if (data && data.length > 0) {
                        var lat = parseFloat(data[0].lat);
                        var lng = parseFloat(data[0].lon);
                        map.setView([lat, lng], 15);
                        updatePin(lat, lng);
                    } else {
                        alert("Location not found. Try adding a city name.");
                    }
                })
                .catch(err => {
                    alert("Error searching location. Please check your network.");
                    console.error(err);
                });
        }

        function closeGeoFenceModal() {
            document.getElementById('geoFenceModal').style.display = 'none';
        }

        function saveGeoFence() {
            var userId = document.getElementById('hfGeoUserId').value;
            var lat = document.getElementById('txtLat').value;
            var lng = document.getElementById('txtLng').value;
            var radius = document.getElementById('rngRadius').value;

            fetch('ViewUser.aspx/SaveGeoFence', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: parseInt(userId), lat: parseFloat(lat), lng: parseFloat(lng), radius: parseInt(radius) })
            })
            .then(res => res.json())
            .then(data => {
                if (data.d === "Success") {
                    alert("Geo-Fence saved successfully!");
                    closeGeoFenceModal();
                    // Optional: trigger a postback here if you want the grid to refresh immediately
                    // __doPostBack('UpdatePanel1', ''); 
                } else {
                    alert("Error: " + data.d);
                }
            });
        }
    </script>
</asp:Content>
