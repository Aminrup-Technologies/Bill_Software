<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="NewUpdate_product.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm70" %>
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
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtSalerate.ClientID%>').value == "") {
                alert("Provide Sale Rate.");
                document.getElementById('<%=txtSalerate.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=cmbtax.ClientID%>').selectedIndex == 0) {
                alert("Please Select Tax.");
                document.getElementById('<%=cmbtax.ClientID%>').focus();
                return false;
            }
        }

</script>

    <script type="text/javascript">
        //Function to allow only numbers to textbox
        function validate(key) {
            //getting key code of pressed key
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('txtfillrequar');
            //comparing pressed keycodes
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
                return false;
            }
            else {
                //Condition to check textbox contains ten numbers or not
                if (phn.value.length < 50) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">New Products Update</span></td>
        </tr>
        <tr>
            <td width="20%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="20%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
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
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>
                <asp:Label ID="lblid" runat="server" Visible="False"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; PRODUCT / SERVICE CATAGORY</td>
            <td>
                <asp:Label ID="lblproductname" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;PRODUCT / SERVICE</td>
            <td>
                <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style" Width="300px" Enabled="false">
                    <asp:ListItem>Product</asp:ListItem>
                    <asp:ListItem>Service</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; PRODUCT / SERVICE SPECIFICATIONS&nbsp;</td>
            <td>
                <asp:TextBox ID="txtSubProdName" runat="server" CssClass="textbox_U_style" Enabled="False"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;EXTRA SPECIFICATIONS</td>
            <td>
                <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;HSN / SAC CODE</td>
            <td>
                <asp:TextBox ID="txtProductCode" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                &nbsp;&nbsp;&nbsp;&nbsp;UNIT</td>
            <td>
                <asp:TextBox ID="txtUnit" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; BASE RATE (RS)</td>
            <td>
                <asp:TextBox ID="txtSalerate" runat="server" CssClass="textbox_U_style" Enabled="False"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; Tax Rate (%)</td>
            <td><asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style" Enabled="False">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2" style="text-align: center">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Update" 
                    onclientclick="return ValidateField();" onclick="btnSave_Click" Visible="False"/>
                <asp:Button ID="btnedit" runat="server" CssClass="btn_style" Text="Edit" OnClick="btnedit_Click" />
            &nbsp;<asp:Button ID="btnback" runat="server" CssClass="btn_style" Text="Back" OnClick="btnback_Click" />
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
       
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
