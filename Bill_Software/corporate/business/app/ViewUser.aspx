<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ViewUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm80" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .grid-active {
            background: #E9FFF0;
        }

        .grid-inactive {
            background: #FFF0F0;
            color: #666;
        }

        .action-link {
            cursor: pointer;
            text-decoration: underline;
            color: #006699;
        }

        .small {
            font-size: 11px;
        }

        .btn {
            padding: 4px 6px;
            border-radius: 4px;
            border: 1px solid #ccc;
            background: #f5f5f5;
        }

        /* Generic button */
        .action-btn {
            padding: 4px 10px;
            border-radius: 5px;
            font-size: 12px;
            font-weight: 600;
            color: #fff;
            cursor: pointer;
            border: none;
            display: inline-block;
            text-decoration: none;
            min-width: 80px;
            text-align: center;
        }

        /* Status-based colors */
        .btn-activate {
            background-color: #28a745;
        }
        /* green */
        .btn-deactivate {
            background-color: #dc3545;
        }
        /* red   */
        .btn-lock {
            background-color: #ffc107;
            color: #000;
        }
        /* yellow */
        .btn-unlock {
            background-color: #007bff;
        }
        /* blue */
        .btn-reset {
            background-color: #17a2b8;
        }
        /* cyan */
        .btn-menu-edit {
            background-color: #6f42c1;
        }
        /* purple */
        .btn-delete {
            background-color: #343a40;
        }
        /* dark */

        /* Hover effect */
        .action-btn:hover {
            opacity: 0.85;
            text-decoration: none;
        }

        /* Disabled look (optional) */
        .btn-disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        /* Clean Grid Layout */
        .user-grid {
            border-collapse: collapse;
            width: 100%;
            font-size: 13px;
        }

        /* Header */
        .user-grid th {
            background: #f1f1f1;
            border: 1px solid #ccc;
            padding: 8px;
            font-weight: 600;
        }

        /* Rows */
        .user-grid td {
            border: 1px solid #ddd;
            padding: 6px;
            vertical-align: middle;
        }

        /* Status Tags */
        .status-active {
            background: #e8ffe8;
            color: #008000;
            padding: 3px 8px;
            border-radius: 3px;
            font-weight: bold;
        }

        .status-inactive {
            background: #ffe8e8;
            color: #b30000;
            padding: 3px 8px;
            border-radius: 3px;
            font-weight: bold;
        }

        /* Buttons */
        .action-btn {
            padding: 4px 8px;
            border-radius: 4px;
            margin-right: 5px;
            font-size: 12px;
            text-decoration: none !important;
            border: 1px solid #ccc;
        }

        /* Color-coded buttons */
        .btn-toggle  { background:#d9edf7; color:#31708f; }
        .btn-reset   { background:#fcf8e3; color:#8a6d3b; }
        .btn-lock    { background:#f5e8ff; color:#6000b3; }
        .btn-menu    { background:#e7f3ff; color:#005b96; }
        .btn-delete  { background:#f2dede; color:#a94442; }

        /* Hover */
        .action-btn:hover {
            opacity: 0.85;
            cursor: pointer;
        }

    </style>

    <script type="text/javascript">
        function confirmDelete() {
            return confirm("Do you really want to delete this user?");
        }
        function confirmReset() {
            return confirm("Reset user's password and force change on next login?");
        }
        function confirmToggle(active) {
            if (active) return confirm("Deactivate this user?");
            return confirm("Activate this user?");
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">View User</span></td>
        </tr>

        <tr>
            <td colspan="6">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                </asp:Panel>
            </td>
        </tr>

        <tr>
            <td colspan="6">
                <!-- Employee Id filter (kept similar to your original) -->
                <table width="100%">
                    <tr>
                        <td style="width: 20%">&nbsp;</td>
                        <td>Employee Id:
                        <asp:DropDownList ID="ddlEmpId" runat="server" Width="220px" Font-Size="12px" CssClass="textbox_U_style" AutoPostBack="True" OnSelectedIndexChanged="ddlEmpId_SelectedIndexChanged"></asp:DropDownList>
                            &nbsp;<asp:Button ID="btnRefresh" runat="server" Text="Refresh" OnClick="btnRefresh_Click" CssClass="btn" />
                        </td>
                        <td style="width: 20%">&nbsp;</td>
                    </tr>
                </table>
            </td>
        </tr>

        <tr>
            <td colspan="6">
                <asp:GridView ID="gvUsers" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="user-grid"
                    GridLines="Both"
                    CellPadding="3"
                    CellSpacing="0"
                    Width="100%"
                    OnRowCommand="gvUsers_RowCommand"
                    OnRowDataBound="gvUsers_RowDataBound">

                    <Columns>

                        <asp:BoundField DataField="Id" HeaderText="ID"
                            ItemStyle-Width="20px" HeaderStyle-Width="20px" />

                        <asp:BoundField DataField="User_Id" HeaderText="User Id"
                            ItemStyle-Width="100px" HeaderStyle-Width="100px" />

                        <asp:BoundField DataField="Name" HeaderText="Employee Name"
                            ItemStyle-Width="150px" HeaderStyle-Width="150px" />

                        <asp:BoundField DataField="Email" HeaderText="Email"
                            ItemStyle-Width="180px" HeaderStyle-Width="180px" />

                        <asp:BoundField DataField="Phone_no" HeaderText="Phone"
                            ItemStyle-Width="90px" HeaderStyle-Width="90px" />

                        <asp:BoundField DataField="LastLogin"
                            HeaderText="Last Login"
                            DataFormatString="{0:yyyy-MM-dd HH:mm}"
                            ItemStyle-Width="140px" HeaderStyle-Width="140px" />

                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="80px" HeaderStyle-Width="80px">
                            <ItemTemplate>
                                <asp:Label ID="lblStatus" runat="server"
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "status-active" : "status-inactive" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="420px" HeaderStyle-Width="420px">
                            <ItemTemplate>
                                <asp:HiddenField ID="hfUserId" runat="server" Value='<%# Eval("User_Id") %>' />

                                <asp:LinkButton ID="lnkToggleActive"
                                    runat="server"
                                    CommandName="ToggleActive"
                                    CommandArgument='<%# Eval("Id") %>'
                                    CssClass="action-btn btn-toggle">
                                </asp:LinkButton>

                                <asp:LinkButton ID="lnkReset"
                                    runat="server"
                                    CommandName="ResetPassword"
                                    CommandArgument='<%# Eval("Id") %>'
                                    CssClass="action-btn btn-reset">
                    Reset
                                </asp:LinkButton>

                                <asp:LinkButton ID="lnkLock"
                                    runat="server"
                                    CommandName="ToggleLock"
                                    CommandArgument='<%# Eval("Id") %>'
                                    CssClass="action-btn btn-lock">
                                </asp:LinkButton>

                                <asp:LinkButton ID="lnkMenuEdit"
                                    runat="server"
                                    CommandName="MenuEdit"
                                    CommandArgument='<%# Eval("Id") %>'
                                    CssClass="action-btn btn-menu">
                    Menu
                                </asp:LinkButton>

                                <asp:LinkButton ID="lnkDelete" Enabled="false"
                                    runat="server" Visible="false"
                                    CommandName="DeleteUser"
                                    CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Delete this user?');"
                                    CssClass="action-btn btn-delete">
                    Delete
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>

            </td>
        </tr>
    </table>
</asp:Content>
