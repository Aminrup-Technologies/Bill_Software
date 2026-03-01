<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AddUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm79" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* container */
        .dl-wrapper {
            width: 100%;
            border: 1px solid #666;
            font-family: Arial, Helvetica, sans-serif;
            color: #2d2d2d;
            font-size: 13px;
            border-radius: 6px;
            overflow: hidden;
        }

        /* header */
        .dl-header, .dl-item {
            display: grid;
            grid-template-columns: 8% 28% 22% 18% 18% 6%;
            align-items: center;
            gap: 8px;
            padding: 10px 12px;
        }

        .dl-header {
            background: #006699;
            color: #fff;
            font-weight: 700;
        }

        /* alternating item style */
        .dl-item:nth-child(2n) {
            background: #f0f6ff;
        }

        .dl-item:hover {
            background: #eef6ff;
        }

        /* cells */
        .dl-cell {
            text-align: center;
            word-wrap: break-word;
        }

        /* labels smaller on mobile */
        @media (max-width: 768px) {
            .dl-header, .dl-item {
                grid-template-columns: 20% 1fr;
                grid-auto-rows: auto;
            }
            /* stack the rest below first two columns */
            .dl-cell {
                text-align: left;
            }

                .dl-cell.password, .dl-cell.phno, .dl-cell.email {
                    grid-column: 1 / -1;
                    padding-left: 6px;
                }

                .dl-cell.actions {
                    text-align: center;
                }
        }
    </style>
    <style>
        /* Header row */
        .table1 {
            width: 100%;
            border-collapse: collapse;
            font-size: 12px;
            font-family: Arial;
        }

            .table1 td {
                padding: 6px 4px;
                font-weight: bold;
                background: #006699;
                color: #fff;
                border: 1px solid #666;
                text-align: center;
            }

        /* Data rows */
        .table2 {
            width: 100%;
            border-collapse: collapse;
            font-size: 12px;
            font-family: Arial;
        }

            .table2 td {
                padding: 6px 4px;
                border: 1px solid #ccc;
                text-align: center;
            }

            /* Alternate row color */
            .table2 tr:nth-child(even) {
                background: #eaf2ff;
            }

            /* Hover effect */
            .table2 tr:hover {
                background: #d6e8ff;
            }

        /* Fixed column widths */
        .col-id {
            width: 8%;
        }

        .col-name {
            width: 20%;
        }

        .col-email {
            width: 22%;
        }

        .col-ph {
            width: 18%;
        }

        .col-pass {
            width: 18%;
        }

        .col-action {
            width: 10%;
        }
    </style>


    <script type="text/javascript">
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

        function ValidateDelete1() {
            var answer = confirm("Want to Delete this User?");
            if (!answer) {
                return false;
            }
        }

        function ValidateDelete1() {
            return confirm('Are you sure you want to delete this record?');
        }

        function togglePassword(btn, actualPassword) {
            var label = btn.previousElementSibling; // the <span> before the button

            if (label.getAttribute("data-visible") === "false") {
                // Show password
                label.innerText = actualPassword;
                label.setAttribute("data-visible", "true");
                btn.innerText = "Hide";
            } else {
                // Mask password again
                label.innerText = "••••••";
                label.setAttribute("data-visible", "false");
                btn.innerText = "Show";
            }

            return false; // prevent PostBack
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">&nbsp;<span style="color: white;" class="style2">Add User</span></td>
        </tr>
        <tr>
            <td width="10%">&nbsp;</td>
            <td colspan="2" width="40%">&nbsp;</td>
            <td colspan="2" width="40%">&nbsp;</td>
            <td width="10%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
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
            <td>&nbsp;</td>
        </tr>


        <tr>
            <td>&nbsp;</td>
            <td colspan="2">Employee Name</td>
            <td colspan="2">
                <asp:TextBox ID="txtEmployee" runat="server" CssClass="textbox_U_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td colspan="2">Phone Number</td>
            <td colspan="2">
                <asp:TextBox ID="txtPhno" runat="server" CssClass="textbox_U_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>


        <tr>
            <td>&nbsp;</td>
            <td colspan="2">Email</td>
            <td colspan="2">
                <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_U_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">Password</td>
            <td colspan="2">
                <asp:TextBox ID="txtPass" runat="server" CssClass="textbox_U_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td colspan="2"></td>
            <td colspan="2"></td>
            <td>&nbsp;</td>
        </tr>


        <tr>
            <td>&nbsp;</td>
            <td colspan="4" style="text-align: center">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return ValidateField();" OnClick="btnSave_Click" />
            </td>
            <td>&nbsp;</td>
        </tr>


        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666"
                    BorderStyle="Solid" BorderWidth="1px" Font-Size="11px"
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%"
                    OnItemCommand="DataList1_ItemCommand">

                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td class="col-id">ID</td>
                                <td class="col-name">Employee Name</td>
                                <td class="col-email">Email</td>
                                <td class="col-ph">Phno</td>
                                <td class="col-action">Delete</td>
                            </tr>
                        </table>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td class="col-id"><%# Eval("id") %></td>
                                <td class="col-name"><%# Eval("Name") %></td>
                                <td class="col-email"><%# Eval("Email") %></td>
                                <td class="col-ph"><%# Eval("Phone_no") %></td>
                                <td class="col-action">
                                    <asp:ImageButton ID="ImageButton1" runat="server"
                                        CommandName="Inactivate" CommandArgument='<%# Eval("id") %>'
                                        ImageUrl="~/corporate/business/WebImages/delete.png"
                                        ToolTip="Delete" OnClientClick="return ValidateDelete1();" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
