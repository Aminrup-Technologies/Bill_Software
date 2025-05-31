<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="vw_dailyrpts.aspx.cs" Inherits="Bill_Software.corporate.business.app.vw_dailyrpts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Daily Reports</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
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
        <tr>
            <td colspan="4">
                <asp:DataList ID="DataList2" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />

                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align: center; width: 8%;">Visit Date</td>
                                <td style="text-align: center; width: 10%;">Salesperson</td>
                                <td style="text-align: center; width: 12%;">Customer Name</td>
                                <td style="text-align: center; width: 8%;">Department</td>
                                <td style="text-align: center; width: 10%;">Contact Person</td>
                                <td style="text-align: center; width: 8%;">Visit Type</td>
                                <td style="text-align: center; width: 12%;">Discussion Points</td>
                                <td style="text-align: center; width: 6%;">Follow-Up</td>
                                <td style="text-align: center; width: 8%;">Next Follow-Up</td>
                                <td style="text-align: center; width: 6%;">Status</td>
                                <td style="text-align: center; width: 6%;">Attachment</td>
                                <td style="text-align: center; width: 6%;">Created Date</td>
                            </tr>
                        </table>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align: center; width: 8%;">
                                    <asp:Label ID="lblVisitDate" runat="server" Text='<%# Eval("VisitDate", "{0:yyyy-MM-dd}") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="lblSalesperson" runat="server" Text='<%# Eval("Salesperson") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">
                                    <asp:Label ID="lblCustomerName" runat="server" Text='<%# Eval("CustomerName") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 8%;">
                                    <asp:Label ID="lblDepartment" runat="server" Text='<%# Eval("Department") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="lblContactPerson" runat="server" Text='<%# Eval("ContactPerson") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 8%;">
                                    <asp:Label ID="lblVisitType" runat="server" Text='<%# Eval("VisitType") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">
                                    <asp:Label ID="lblDiscussionPoints" runat="server" Text='<%# Eval("DiscussionPoints") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:Label ID="lblFollowUpRequired" runat="server" Text='<%# Eval("FollowUpRequired") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 8%;">
                                    <asp:Label ID="lblNextFollowUpDate" runat="server" Text='<%# Eval("NextFollowUpDate", "{0:yyyy-MM-dd}") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:HyperLink ID="hlAttachment" runat="server"
                                        NavigateUrl='<%# Eval("AttachmentName", "~/Uploads/{0}") %>'
                                        Text="View" Target="_blank" />
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:Label ID="lblCreatedDate" runat="server" Text='<%# Eval("TimeStamp", "{0:yyyy-MM-dd HH:mm}") %>'></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
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
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
