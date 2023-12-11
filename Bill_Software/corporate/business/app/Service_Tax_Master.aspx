<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Service_Tax_Master.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm8" %>
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
            if (document.getElementById('<%=txtVatRate.ClientID%>').value == "") {
                alert("Provide Service Tax Rate");
                document.getElementById('<%=txtVatRate.ClientID%>').focus();
                return false;
            }
        }

</script>
<script type="text/javascript">
    function ValidateDelete1() {
        var answer = confirm("Want to Delete this Service Tax?");
        if (!answer) {
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
            <td bgcolor="#19658A" colspan="6">
                &nbsp;<span class="style2">Manage Service Tax</span></td>
        </tr>
        <tr>
            <td width="20%">
                &nbsp;</td>
            <td colspan="2" width="30%">
                &nbsp;</td>
            <td colspan="2" width="30%">
                &nbsp;</td>
            <td width="20%">
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="4">
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
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td width="20%">
                &nbsp;
                Provide new Service Tax Rate</td>
            <td colspan="2" width="20%">
                <asp:TextBox ID="txtVatRate" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td width="20%">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save" 
                    onclientclick="return ValidateField();" onclick="btnSave_Click"/>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%" 
                    onitemcommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:70%;">
                                    <asp:Label ID="showrm" runat="server" Text="Service Tax Rate"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit" runat="server" Text="Delete"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:70%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Service_tax") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Delete" CommandArgument='<%# Eval("ID") %>' 
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" onclientclick="return ValidateDelete1();"/>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td colspan="2">
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
</asp:Content>
