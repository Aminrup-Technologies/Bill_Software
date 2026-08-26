<%@ Page Title="Manage City Master" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="master_city.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm3" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
            background-color: #006699;
            color: white;
            font-weight: bold;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                padding: 5px;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                padding: 5px;
                border-top: none;
            }

        .btn_style {
            cursor: pointer;
            padding: 5px 15px;
        }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            var city = document.getElementById('<%=txtCityName.ClientID%>').value.trim();
            var ddlState = document.getElementById('<%=ddlStateName.ClientID%>');

            if (city == "") {
                alert("Please provide a City Name.");
                document.getElementById('<%=txtCityName.ClientID%>').focus();
               return false;
           }
           if (ddlState.value == "") {
               alert("Please select a State.");
               ddlState.focus();
               return false;
           }
           return true;
       }

       function ValidateDelete1() {
           return confirm("Are you sure you want to delete this location?");
       }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hfCityID" runat="server" Value="0" />

    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">Manage City & State Master</span></td>
        </tr>
        <tr>
            <td colspan="6">&nbsp;</td>
        </tr>
        <tr>
            <td width="5%">&nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5">
                    <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server" ForeColor="#006600" Font-Bold="true"></asp:Label>
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BackColor="#FFDDDD" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5">
                    <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server" ForeColor="#FF3300" Font-Bold="true"></asp:Label>
                </asp:Panel>
            </td>
            <td width="5%">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%"><b>City Name:</b><br />
                <asp:TextBox ID="txtCityName" runat="server" CssClass="textbox_U_style" Width="90%"></asp:TextBox>
            </td>
            <td width="15%"><b>State Name:</b><br />
                <asp:DropDownList ID="ddlStateName" runat="server" CssClass="textbox_U_style" Width="95%">
                </asp:DropDownList>
            </td>
            <td width="20%" valign="bottom">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save" OnClientClick="if(!ValidateField()) return false;" OnClick="btnSave_Click" />
                &nbsp;
                <asp:Button ID="btnCancel" runat="server" CssClass="btn_style" Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </td>
            <td width="40%">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="12px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                    <AlternatingItemStyle BackColor="#F9F9F9" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align: center; width: 10%;">ID</td>
                                <td style="text-align: left; width: 40%;">City Name</td>
                                <td style="text-align: left; width: 35%;">State Name</td>
                                <td style="text-align: center; width: 7%;">Edit</td>
                                <td style="text-align: center; width: 8%;">Delete</td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="lblID" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                </td>
                                <td style="text-align: left; width: 40%;">
                                    <asp:Label ID="lblCityName" runat="server" Text='<%# Eval("City_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align: left; width: 35%;">
                                    <asp:Label ID="lblStateName" runat="server" Text='<%# Eval("State_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 7%;">
                                    <asp:ImageButton ID="btnEdit" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("ID") %>' ImageUrl="~/corporate/business/WebImages/edit.png" ToolTip="Edit" Height="16px" Width="16px" />
                                </td>
                                <td style="text-align: center; width: 8%;">
                                    <asp:ImageButton ID="btnDelete" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("ID") %>' ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" OnClientClick="if(!ValidateDelete1()) return false;" Height="16px" Width="16px" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
