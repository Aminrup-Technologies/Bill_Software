<%@ Page Title="Assign User Roles" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Update_Designation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm81" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .role-box { border: 1px solid #ccc; padding: 25px; background: #fafafa; border-radius: 5px; margin-top: 15px; width: 60%; }
        .chk-roles label { margin-left: 8px; font-weight: bold; color: #333; cursor: pointer; }
        .chk-roles td { padding: 10px; border-bottom: 1px solid #eee; }
        .btn_style { background: #19658A; color: white; border: none; padding: 8px 20px; cursor: pointer; border-radius: 4px; font-weight: bold; }
        .btn_style:hover { background: #134e6a; }
        .btn_back { background: #666; }
        .btn_back:hover { background: #444; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="3">&nbsp;<span class="style2">Assign User Roles</span></td>
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
            <td style="padding: 20px;" align="center">
                
                <div class="role-box" align="left">
                    <table style="width: 100%; font-size: 15px; margin-bottom: 15px;">
                        <tr>
                            <td style="width: 150px; font-weight: bold;">User Id:</td>
                            <td><asp:Label ID="lblEmpId" runat="server" ForeColor="#006699"></asp:Label></td>
                        </tr>
                        <tr>
                            <td style="font-weight: bold;">Employee Name:</td>
                            <td><asp:Label ID="lblEmpName" runat="server" ForeColor="#006699"></asp:Label></td>
                        </tr>
                    </table>
                    
                    <hr style="border: 0; border-top: 1px solid #ccc; margin: 15px 0;" />
                    
                    <h3 style="color: #333;">Select Access Roles</h3>
                    <p style="font-size: 12px; color: #666;">Check the boxes below to assign roles to this user. A user can have multiple roles.</p>
                    
                    <asp:CheckBoxList ID="chkRoles" runat="server" CssClass="chk-roles" RepeatColumns="2" RepeatDirection="Horizontal" Width="100%">
                    </asp:CheckBoxList>
                    
                    <br /><br />
                    <asp:Button ID="btnSave" runat="server" Text="Save Roles" CssClass="btn_style" OnClick="btnSave_Click" />
                    &nbsp;
                    <asp:Button ID="btnReset" runat="server" Text="Back to View Users" CssClass="btn_style btn_back" OnClick="btnReset_Click" />
                </div>

            </td>
        </tr>
    </table>
</asp:Content>