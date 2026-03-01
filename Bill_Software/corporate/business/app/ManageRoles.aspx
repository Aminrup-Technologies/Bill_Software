<%@ Page Title="Manage Roles" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs" Inherits="Bill_Software.corporate.business.app.ManageRoles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .form-control { padding: 6px; width: 250px; border: 1px solid #ccc; border-radius: 4px; }
        .btn_style { background: #19658A; color: white; border: none; padding: 8px 15px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        .btn_style:hover { background: #134e6a; }
        .section-box { border: 1px solid #ccc; padding: 15px; margin-bottom: 20px; background: #fafafa; border-radius: 5px; }
        
        /* Hierarchy Styles */
        .module-card { border: 1px solid #006699; margin-bottom: 15px; background: #fff; border-radius: 5px; overflow: hidden; }
        .module-header { background: #006699; color: #fff; padding: 10px 15px; margin: 0; font-size: 16px; }
        .sub-module-section { padding: 15px; border-bottom: 1px dashed #eee; }
        .sub-module-section:last-child { border-bottom: none; }
        .sub-module-title { color: #d9534f; margin-top: 0; margin-bottom: 10px; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; }
        .feature-chklist label { margin-left: 5px; margin-right: 20px; font-weight: normal; color: #333; cursor: pointer; }
        .feature-chklist td { padding: 5px 0; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="3" style="padding:8px;">&nbsp;<span class="style2">Manage Roles & Permissions</span></td>
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
                    <h3 style="margin-top:0; color:#333;">1. Create a New Role</h3>
                    Role Name: <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control" placeholder="e.g., Sales Manager"></asp:TextBox>
                    &nbsp; Description: <asp:TextBox ID="txtRoleDesc" runat="server" CssClass="form-control" placeholder="Brief description"></asp:TextBox>
                    &nbsp; <asp:Button ID="btnCreateRole" runat="server" Text="Create Role" CssClass="btn_style" OnClick="btnCreateRole_Click" />
                </div>

                <div class="section-box" style="background: #f4f9ff;">
                    <h3 style="margin-top:0; color:#333;">2. Assign Permissions to Role</h3>
                    <div style="margin-bottom: 20px;">
                        <strong>Select Role to Edit:</strong> 
                        <asp:DropDownList ID="ddlRoles" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlRoles_SelectedIndexChanged">
                        </asp:DropDownList>
                        &nbsp;
                        <asp:Button ID="btnSavePermissions" runat="server" Text="Save Permissions" CssClass="btn_style" OnClick="btnSavePermissions_Click" style="background:#28a745;" />
                    </div>

                    <asp:Repeater ID="rptModules" runat="server" OnItemDataBound="rptModules_ItemDataBound">
                        <ItemTemplate>
                            <div class="module-card">
                                <h4 class="module-header"><%# Container.DataItem %></h4>
                                
                                <asp:Repeater ID="rptSubModules" runat="server" OnItemDataBound="rptSubModules_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="sub-module-section">
                                            <h5 class="sub-module-title"><%# Container.DataItem %></h5>
                                            
                                            <asp:CheckBoxList ID="chkFeatures" runat="server" CssClass="feature-chklist" RepeatDirection="Horizontal" RepeatColumns="5" DataTextField="FeatureName" DataValueField="PermissionId">
                                            </asp:CheckBoxList>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                </div>
            </td>
        </tr>
    </table>
</asp:Content>