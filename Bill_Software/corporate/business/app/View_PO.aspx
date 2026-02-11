<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PO.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PO" %>

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

        .pr-header {
            border-collapse: collapse;
            font-size: 13px;
        }

        .pr-column-header th {
            background-color: #0b5a83;
            color: white;
            font-weight: bold;
            text-align: center;
            padding: 6px;
        }

        .pr-row td {
            border-bottom: 1px solid #ddd;
            padding: 6px;
        }

        .pr-row:hover {
            background-color: #f5f5f5;
        }

        .pr-title {
            background-color: #0b5a83;
            color: white;
            font-weight: bold;
            padding: 8px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Purchase Order</span></td>
        </tr>
        <tr>
            <td width="10%">&nbsp;</td>
            <td width="40%">&nbsp;</td>
            <td width="40%">&nbsp;</td>
            <td width="10%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="lbl_recordtype" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>Select Record / Document Type</td>
            <td>&nbsp;
                <asp:RadioButton ID="rbQt" runat="server" GroupName="recordOption" Text="Quotation" Checked="true" AutoPostBack="true" OnCheckedChanged="RecordTypeChanged"/>
                <asp:RadioButton ID="rbPo" runat="server" GroupName="recordOption" Text="Purchase Order" AutoPostBack="true" OnCheckedChanged="RecordTypeChanged"/>
            </td>
            <td>&nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:DataList ID="DataListPO" runat="server"
                    Width="100%"
                    OnItemCommand="DataListPO_ItemCommand"
                    OnItemDataBound="DataListPO_ItemDataBound">

                    <HeaderTemplate>
                        <table class="pr-header" width="100%" cellspacing="0">
                            <tr class="pr-column-header">
                                <th style="width: 3%">Sl</th>
                                <th style="width: 22%">Vendor</th>
                                <th style="width: 15%">PO No</th>
                                <th style="width: 15%">Req No</th>
                                <th style="width: 15%">Created By</th>
                                <th style="width: 15%">Created On</th>
                                <th style="width: 10%">Status</th>
                                <th style="width: 5%">View</th>
                            </tr>
                    </HeaderTemplate>


                    <ItemTemplate>
                        <tr class="pr-row">
                            <td align="center">
                                <asp:Label ID="lblSlNo" runat="server" />
                            </td>

                            <!-- Vendor -->
                            <td><%# Eval("Vendor_Name") %></td>

                            <!-- PO No -->
                            <td><%# Eval("PO_No") %></td>

                            <!-- Req No -->
                            <td><%# Eval("ReqNo") %></td>

                            <!-- Created By -->
                            <td><%# Eval("CreatedByName") %></td>

                            <!-- Created On -->
                            <td>
                                <%# Eval("CreatedOn", "{0:dd-MMM-yyyy hh:mm tt}") %>
                            </td>

                            <!-- Status -->
                            <td>
                                <asp:Label ID="lblStatus" runat="server"
                                    Text='<%# Eval("PO_Status") %>' />
                            </td>

                            <!-- View -->
                            <td align="center">
                                <asp:ImageButton
                                    ID="btnView"
                                    runat="server"
                                    CommandName="View"
                                    CommandArgument='<%# Eval("PO_Id") %>'
                                    ImageUrl="~/corporate/business/WebImages/viewicon.png"
                                    ToolTip="View PO"
                                    Height="18" Width="18" />
                            </td>
                        </tr>
                    </ItemTemplate>


                    <FooterTemplate>
                        </table>
                    </FooterTemplate>

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
