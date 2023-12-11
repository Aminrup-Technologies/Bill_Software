<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ViewUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm80" %>
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
  <%--  <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtEmployee.ClientID%>').value == "") {
                alert("Provide Employee Name.");
                document.getElementById('<%=txtEmployee.ClientID%>').focus();
                return false;
                if (document.getElementById('<%=txtPass.ClientID%>').value == "") {
                    alert("Provide Password.");
                    document.getElementById('<%=txtPass.ClientID%>').focus();
                    return false;
                }
                if (document.getElementById('<%=txtEmail.ClientID%>').value == "") {
                    alert("Provide Email Address.");
                    document.getElementById('<%=txtEmail.ClientID%>').focus();
                    return false;
                }
                if (document.getElementById('<%=txtPhno.ClientID%>').value == "") {
                    alert("Provide Phone Number.");
                    document.getElementById('<%=txtPhno.ClientID%>').focus();
                    return false;
                }
            }
        }
</script>--%>
    <script type="text/javascript">
    function ValidateDelete1() {
        var answer = confirm("Want to Delete this User?");
        if (!answer) {
            return false;
        }
    }
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">
                &nbsp;<span class="style2">View User</span></td>
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
                Employee Id</td>
            <td colspan="2">
                <asp:DropDownList ID="ddlEmpId" runat="server" Width="220px" Font-Size="12px" CssClass="textbox_U_style" AutoPostBack="True" OnTextChanged="ddlEmpId_TextChanged"></asp:DropDownList>
            </td>
            <td>
                &nbsp;</td>
        </tr>

        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2"></td>
            <td colspan="2"></td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2"></td>
            <td colspan="2"></td>
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
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label4" runat="server" Text="User Id"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showrm" runat="server" Text="Employee Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label1" runat="server" Text="Email"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label2" runat="server" Text="Phno"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label3" runat="server" Text="Password"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label5" runat="server" Text="Menu Edit"></asp:Label>
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
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="User_Id" runat="server" Text='<%# Eval("User_Id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Name" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Email" runat="server" Text='<%# Eval("Email") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Phone_no" runat="server" Text='<%# Eval("Phone_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Password" runat="server" Text='<%# Eval("Password") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:ImageButton ID="ImageButton2" runat="server" CommandArgument='<%# Eval("User_Id") %>' CommandName="Menu Edit" ImageUrl="../WebImages/edit_icon.png" Height="20px" Width="60px" ToolTip="Menu Edit" />
                                </td>
                                
                                <td style="text-align:center; width:10%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Delete" CommandArgument='<%# Eval("User_Id") %>' 
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
