<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ViewUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm80" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .grid-active { background: #E9FFF0; }
        .grid-inactive { background: #FFF0F0; color: #666; }
        
        /* Buttons */
        .action-btn {
            padding: 4px 10px; border-radius: 5px; font-size: 12px; font-weight: 600;
            color: #fff; cursor: pointer; border: none; display: inline-block;
            text-decoration: none; min-width: 70px; text-align: center; margin-right: 5px;
        }
        .btn-activate { background-color: #28a745; }
        .btn-deactivate { background-color: #dc3545; }
        .btn-lock { background-color: #ffc107; color: #000; }
        .btn-unlock { background-color: #007bff; }
        .btn-reset { background-color: #17a2b8; }
        .btn-menu-edit { background-color: #6f42c1; }
        .btn-delete { background-color: #343a40; }
        .btn-save { background-color: #28a745; }
        .btn-cancel { background-color: #6c757d; }
        .action-btn:hover { opacity: 0.85; text-decoration: none; }

        /* Grid Layout */
        .user-grid { border-collapse: collapse; width: 100%; font-size: 13px; }
        .user-grid th { background: #f1f1f1; border: 1px solid #ccc; padding: 8px; font-weight: 600; }
        .user-grid td { border: 1px solid #ddd; padding: 6px; vertical-align: middle; }

        /* Status Tags */
        .status-active { background: #e8ffe8; color: #008000; padding: 3px 8px; border-radius: 3px; font-weight: bold; }
        .status-inactive { background: #ffe8e8; color: #b30000; padding: 3px 8px; border-radius: 3px; font-weight: bold; }
    </style>

    <script type="text/javascript">
        function confirmSendCredentials() {
            return confirm("Are you sure you want to generate a new temporary password and email it to this user?");
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
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 5px; margin-bottom: 10px;">
                    <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server" ForeColor="Green"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BackColor="#FFEEEE" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 5px; margin-bottom: 10px;">
                    <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red"></asp:Label>
                </asp:Panel>
            </td>
        </tr>

        <tr>
            <td colspan="6" style="padding-bottom: 10px;">
                Employee Id:
                <asp:DropDownList ID="ddlEmpId" runat="server" Width="220px" Font-Size="12px" AutoPostBack="True" OnSelectedIndexChanged="ddlEmpId_SelectedIndexChanged"></asp:DropDownList>
                &nbsp;<asp:Button ID="btnRefresh" runat="server" Text="Refresh" OnClick="btnRefresh_Click" CssClass="action-btn btn-unlock" />
            </td>
        </tr>

        <tr>
            <td colspan="6">
                <asp:GridView ID="gvUsers" runat="server"
                    AutoGenerateColumns="False"
                    DataKeyNames="Id"
                    CssClass="user-grid"
                    GridLines="Both"
                    Width="100%"
                    OnRowCommand="gvUsers_RowCommand"
                    OnRowDataBound="gvUsers_RowDataBound"
                    OnRowEditing="gvUsers_RowEditing"
                    OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                    OnRowUpdating="gvUsers_RowUpdating">

                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="true" ItemStyle-Width="30px" />
                        <asp:BoundField DataField="User_Id" HeaderText="User Id" ReadOnly="true" ItemStyle-Width="100px" />

                        <asp:TemplateField HeaderText="Employee Name" ItemStyle-Width="150px">
                            <ItemTemplate><%# Eval("Name") %></ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtName" runat="server" Text='<%# Bind("Name") %>' Width="130px"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email" ItemStyle-Width="180px">
                            <ItemTemplate><%# Eval("Email") %></ItemTemplate>
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

                        <asp:TemplateField HeaderText="Email Verified" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
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

                        <asp:TemplateField HeaderText="Force Pwd Change" ItemStyle-Width="110px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:Label ID="lblForcePwd" runat="server" Font-Bold="true"
                                    Text='<%# Eval("MustChangePassword") != DBNull.Value && Convert.ToBoolean(Eval("MustChangePassword")) ? "Yes" : "No" %>'
                                    ForeColor='<%# Eval("MustChangePassword") != DBNull.Value && Convert.ToBoolean(Eval("MustChangePassword")) ? System.Drawing.Color.DarkOrange : System.Drawing.Color.Gray %>'>
                                </asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:CheckBox ID="chkMustChangePwd" runat="server" 
                                    Checked='<%# Eval("MustChangePassword") != DBNull.Value && Convert.ToBoolean(Eval("MustChangePassword")) %>' />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="LastLogin" HeaderText="Last Login" ReadOnly="true" DataFormatString="{0:yyyy-MM-dd HH:mm}" ItemStyle-Width="120px" />

                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="80px">
                            <ItemTemplate>
                                <asp:Label ID="lblStatus" runat="server"
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "status-active" : "status-inactive" %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="450px">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="Edit" CssClass="action-btn btn-menu-edit">Edit</asp:LinkButton>
                                
                                <asp:LinkButton ID="lnkToggleActive" runat="server" CommandName="ToggleActive" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                
                                <asp:LinkButton ID="lnkReset" runat="server" CommandName="ResetPassword" CommandArgument='<%# Eval("Id") %>' OnClientClick="return confirmSendCredentials();" CssClass="action-btn btn-reset">Email Access</asp:LinkButton>
                                
                                <asp:LinkButton ID="lnkLock" runat="server" CommandName="ToggleLock" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn"></asp:LinkButton>
                                
                                <asp:LinkButton ID="lnkMenuEdit" runat="server" CommandName="MenuEdit" CommandArgument='<%# Eval("Id") %>' CssClass="action-btn btn-menu-edit">Menu</asp:LinkButton>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:LinkButton ID="lnkUpdate" runat="server" CommandName="Update" CssClass="action-btn btn-save">Save</asp:LinkButton>
                                <asp:LinkButton ID="lnkCancel" runat="server" CommandName="Cancel" CssClass="action-btn btn-cancel">Cancel</asp:LinkButton>
                            </EditItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
</asp:Content>