<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Update_factory.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm63" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style type="text/css">
        .style1
        {
            width: 100%;
        }
        .style2
        {
            color: #FFFFFF;
            font-weight: bold;
        }
        .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
         .auto-style1 {
             color: #FF6600;
         }
    </style>
     <script type="text/javascript">
         function ValidateField() {
             
            if (document.getElementById('<%=txtAddress1.ClientID%>').value == "") {
                 alert("Provide Address 1.");
                 document.getElementById('<%=txtAddress1.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=cmbcity.ClientID%>').selectedIndex == 0) {
                 alert("Please Select City.");
                 document.getElementById('<%=cmbcity.ClientID%>').focus();
                 return false;
             }
             if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) {
                 alert("Please Select State.");
                 document.getElementById('<%=cmbState.ClientID%>').focus();
                 return false;
             }
             if (document.getElementById('<%=txtpin.ClientID%>').value == "") {
                 alert("Provide PIN");
                 document.getElementById('<%=txtpin.ClientID%>').focus();
                return false;
            }
        }

</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp;<span class="style2">Update Factory</span></td>
        </tr>
        <tr>
            <td width="20%">
                &nbsp;</td>
            <td width="30%">
                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="30%">
                <asp:Label ID="lblID" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="20%">
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
        
            <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" 
                BorderStyle="Solid" BorderWidth="1px" Visible="False">
                &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" 
                    ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" 
                    Width="16px" />
                &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
            </asp:Panel>
        
                                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Client&nbsp;Name</td>
            <td>
                <asp:Label ID="lblclientName" runat="server"></asp:Label>
                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                Factory Name</td>
            <td>
                <asp:Label ID="lblFactoryName" runat="server"></asp:Label>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
            <span class="style3">*</span>Address 1</td>
            <td>
            <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" Enabled="False" Width="250px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                Address 2</td>
            <td>
            <asp:TextBox ID="txtaddress2" runat="server" CssClass="textbox_style" onkeypress="return validate(event)" Enabled="False" Width="250px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                <span class="auto-style1">*</span>City</td>
            <td>
            <asp:DropDownList ID="cmbcity" runat="server" CssClass="dropdown_style" Enabled="False">
            </asp:DropDownList>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                <span class="auto-style1">*</span>State</td>
            <td>
            <asp:DropDownList ID="cmbState" runat="server" CssClass="dropdown_style" Enabled="False">
            </asp:DropDownList>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                <span class="auto-style1">*</span>Pin</td>
            <td>
            <asp:TextBox ID="txtpin" runat="server" CssClass="textbox_style" Enabled="False"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2" style="text-align: center; font-weight: 700;">
                <asp:Button ID="btnEdit" runat="server" CssClass="btn_style" Text="Edit" onclick="btnEdit_Click"/>
                <asp:Button ID="btnUpdate" runat="server" CssClass="btn_style" Text="Update" onclientclick="return ValidateField();" OnClick="btnUpdate_Click" Visible="False"/>
            &nbsp;<asp:Button ID="btnBack" runat="server" CssClass="btn_style" Text="Back" onclick="btnBack_Click"/>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
</asp:Content>
