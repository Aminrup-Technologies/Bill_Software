<%@ Page Title="Manage Pages & Permissions" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ManagePermissions.aspx.cs" Inherits="Bill_Software.corporate.business.app.ManagePermissions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        
        .form-container { border: 1px solid #ccc; padding: 20px; background: #fafafa; border-radius: 5px; margin-bottom: 20px; }
        .form-row { margin-bottom: 12px; display: flex; align-items: center; }
        .form-label { width: 180px; font-weight: bold; color: #333; }
        .form-control { padding: 6px; width: 300px; border: 1px solid #ccc; border-radius: 4px; }
        
        .btn-primary { background: #19658A; color: white; border: none; padding: 8px 20px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        .btn-primary:hover { background: #134e6a; }
        .btn-secondary { background: #666; color: white; border: none; padding: 8px 20px; cursor: pointer; border-radius: 4px; font-weight: bold; margin-left: 10px; }
        
        .perm-grid { width: 100%; border-collapse: collapse; font-size: 12px; margin-top: 15px; }
        .perm-grid th { background: #006699; color: white; padding: 8px; border: 1px solid #ccc; text-align: left; }
        .perm-grid td { padding: 6px; border: 1px solid #ccc; vertical-align: middle; }
        .perm-grid tr:nth-child(even) { background: #f9f9f9; }
        .perm-grid tr:hover { background: #eaf2ff; }
        
        .action-link { color: #006699; font-weight: bold; text-decoration: none; margin-right: 10px; }
        .action-link:hover { text-decoration: underline; }
        .action-delete { color: #d9534f; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="3" style="padding:8px;">&nbsp;<span class="style2">Manage System Pages & Permissions</span></td>
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
                
                <div class="form-container">
                    <h3 style="margin-top:0; border-bottom:1px solid #ccc; padding-bottom:10px; color:#006699;">Register New Page / Feature</h3>
                    
                    <asp:HiddenField ID="hfEditPermissionId" runat="server" Value="0" />

                    <div class="form-row">
                        <span class="form-label">HTML ID (PermissionKey):</span>
                        <asp:TextBox ID="txtPermissionKey" runat="server" CssClass="form-control" placeholder="e.g. Add_invoice (Must match ID in Bill.Master)"></asp:TextBox>
                    </div>
                    <div class="form-row">
                        <span class="form-label">Module Name:</span>
                        <asp:TextBox ID="txtModuleName" runat="server" CssClass="form-control" placeholder="e.g. Finance & Accounts"></asp:TextBox>
                    </div>
                    <div class="form-row">
                        <span class="form-label">Sub-Module Name:</span>
                        <asp:TextBox ID="txtSubModuleName" runat="server" CssClass="form-control" placeholder="e.g. Tax Invoice"></asp:TextBox>
                    </div>
                    <div class="form-row">
                        <span class="form-label">Feature Name:</span>
                        <asp:TextBox ID="txtFeatureName" runat="server" CssClass="form-control" placeholder="e.g. Create Invoice"></asp:TextBox>
                    </div>
                    <div class="form-row">
                        <span class="form-label">Description (Optional):</span>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" placeholder="Brief details about this feature"></asp:TextBox>
                    </div>
                    
                    <div style="margin-top: 15px; padding-left: 180px;">
                        <asp:Button ID="btnSave" runat="server" Text="Save Permission" CssClass="btn-primary" OnClick="btnSave_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-secondary" OnClick="btnClear_Click" />
                    </div>
                </div>

                <h3 style="color:#333;">Registered Permissions Map</h3>
                <asp:GridView ID="gvPermissions" runat="server" AutoGenerateColumns="False" CssClass="perm-grid" DataKeyNames="PermissionId" OnRowCommand="gvPermissions_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="PermissionId" HeaderText="ID" ItemStyle-Width="50px" />
                        <asp:BoundField DataField="ModuleName" HeaderText="Module" />
                        <asp:BoundField DataField="SubModuleName" HeaderText="Sub-Module" />
                        <asp:BoundField DataField="FeatureName" HeaderText="Feature / Display Name" />
                        <asp:BoundField DataField="PermissionKey" HeaderText="HTML Element ID (Key)" />
                        
                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditPerm" CommandArgument='<%# Eval("PermissionId") %>' CssClass="action-link">Edit</asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeletePerm" CommandArgument='<%# Eval("PermissionId") %>' CssClass="action-link action-delete" OnClientClick="return confirm('Are you sure you want to delete this permission? This will remove it from all roles!');">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

            </td>
        </tr>
    </table>
</asp:Content>