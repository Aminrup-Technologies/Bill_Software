<%@ Page Title="Manage Roles" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs" Inherits="Bill_Software.corporate.business.app.ManageRoles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .form-control { padding: 5px; width: 250px; border: 1px solid #ccc; border-radius: 4px; }
        .btn_style { background: #19658A; color: white; border: none; padding: 6px 15px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        .btn_style:hover { background: #134e6a; }
        .grid-style { width: 100%; border-collapse: collapse; margin-top: 15px; }
        .grid-style th { background-color: #f1f1f1; padding: 8px; border: 1px solid #ccc; text-align: left; }
        .grid-style td { padding: 8px; border: 1px solid #ccc; }
        .section-box { border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; background: #fafafa; border-radius: 5px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="3">&nbsp;<span class="style2">Manage Roles & Permissions</span></td>
        </tr>
        <tr>
            <td colspan="3">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding:10px; margin-top:10px;">
                    <asp:Label ID="lblOk" runat="server" ForeColor="Green" Font-Bold="true"></asp:Label>
                </asp:Panel>
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding:10px; margin-top:10px;">
                    <asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td style="padding: 20px;" colspan="3">
                
                <div class="section-box">
                    <h3>1. Create a New Role</h3>
                    Role Name: <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control" placeholder="e.g., Sales Manager"></asp:TextBox>
                    &nbsp; Description: <asp:TextBox ID="txtRoleDesc" runat="server" CssClass="form-control" placeholder="Brief description"></asp:TextBox>
                    &nbsp; <asp:Button ID="btnCreateRole" runat="server" Text="Create Role" CssClass="btn_style" OnClick="btnCreateRole_Click" />
                </div>

                <div class="section-box">
                    <h3>2. Assign Permissions</h3>
                    Select Role to Edit: 
                    <asp:DropDownList ID="ddlRoles" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlRoles_SelectedIndexChanged">
                    </asp:DropDownList>
                    
                    <asp:GridView ID="gvPermissions" runat="server" AutoGenerateColumns="False" CssClass="grid-style" DataKeyNames="PermissionId">
                        <Columns>
                            <asp:TemplateField HeaderText="Grant Access">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" Width="100px" />
                            </asp:TemplateField>
                            <asp:BoundField DataField="PermissionKey" HeaderText="Module / Page Name" />
                            <asp:BoundField DataField="Description" HeaderText="Description" />
                        </Columns>
                    </asp:GridView>
                    <br />
                    <asp:Button ID="btnSavePermissions" runat="server" Text="Save Permissions for Selected Role" CssClass="btn_style" OnClick="btnSavePermissions_Click" />
                </div>

            </td>
        </tr>
    </table>
</asp:Content>