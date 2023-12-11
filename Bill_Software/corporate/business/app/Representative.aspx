<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Representative.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm59" %>
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
            if (document.getElementById('<%=cmbvendor.ClientID%>').selectedIndex == 0) {
                alert("Please Select Vendor.");
                document.getElementById('<%=cmbvendor.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresentativeName.ClientID%>').value == "") {
                alert("Provide Representatives Name.");
                document.getElementById('<%=txtRepresentativeName.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresantativeDesig.ClientID%>').value == "") {
                alert("Provide Representatives Designation.");
                document.getElementById('<%=txtRepresantativeDesig.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresentativePhone.ClientID%>').value == "") {
                alert("Provide Representatives Phone NO.");
                document.getElementById('<%=txtRepresentativePhone.ClientID%>').focus();
                return false;
            }
        }

</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp;<span class="style2">Add Representative</span></td>
        </tr>
        <tr>
            <td width="20%">
                &nbsp;</td>
            <td width="30%">
                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="30%">
                &nbsp;</td>
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
                <span class="auto-style1">*</span>Select Client&nbsp;</td>
            <td>
                    <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style" Width="220px">
                    </asp:DropDownList>
                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                Representatives Title</td>
            <td>
                    <asp:DropDownList ID="ddlRepTitle" runat="server" CssClass="dropdown_style" Width="220px">
                        <asp:ListItem Selected="True">Mr</asp:ListItem>
                        <asp:ListItem>Mrs</asp:ListItem>
                        <asp:ListItem>Ms</asp:ListItem>
                        <asp:ListItem>Dr</asp:ListItem>
                        <asp:ListItem>Md</asp:ListItem>
                        <asp:ListItem>Col</asp:ListItem>
                    </asp:DropDownList>
                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
            <span class="style3">*</span>Representatives First Name</td>
            <td>
            <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="textbox_style" Width="220px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                Representatives Last Name</td>
            <td>
            <asp:TextBox ID="txtLastName" runat="server" CssClass="textbox_style" Width="220px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
            <span class="style3">*</span>Designation</td>
            <td>
            <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="textbox_style" Width="220px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                <span class="auto-style1">*</span>Phone No.</td>
            <td>
            <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="textbox_style" Width="220px" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
            Email</td>
            <td>
            <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="textbox_style" Width="220px"></asp:TextBox>
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
            <td colspan="2" style="text-align: center">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save" 
                    onclientclick="return ValidateField();" onclick="btnSave_Click"/>
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
            <td colspan="2">
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
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="showrm" runat="server" Text="Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label2" runat="server" Text="Email"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit" runat="server" Text="Edit"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label1" runat="server" Text="Delete"></asp:Label>
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
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Representative_name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:30%;">
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("Email") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                     <asp:ImageButton ID="ImageButton2" runat="server" CommandName="Edit" CommandArgument='<%# Eval("ID") %>' 
                                        ImageUrl="~/corporate/business/WebImages/edit1.png" ToolTip="Edit"/><img src="../WebImages/edit1.png" />
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
