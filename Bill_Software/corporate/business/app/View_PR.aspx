<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PR.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PR" %>

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

        .pr-header th {
            background: #0b5f8a;
            color: white;
            padding: 8px;
            font-size: 12px;
            text-align: center;
        }

        .pr-row td {
            padding: 8px;
            border-bottom: 1px solid #e0e0e0;
            font-size: 12px;
        }

        .pr-row:hover {
            background-color: #f4f9ff;
            transition: 0.2s;
        }

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Purchase Requitions</span></td>
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
                <asp:DataList ID="DataList1" runat="server"
                    Width="100%"
                    GridLines="Both"
                    OnItemCommand="DataList1_ItemCommand"
                    OnItemDataBound="DataList1_ItemDataBound">

                    <HeaderTemplate>
                        <headertemplate>
                        <table class="pr-header" width="100%">
                            <tr>
                                <th style="width:5%">Sl</th>
                                <th style="width:25%">Client</th>
                                <th style="width:20%">PR No</th>
                                <th style="width:15%">Created By</th>
                                <th style="width:15%">Created On</th>
                                <th style="width:10%">Status</th>
                                <th style="width:5%">View</th>
                            </tr>
                        </table>
                    </headertemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table class="pr-row" width="100%">
                            <tr>
                                <td style="width: 5%; text-align: center">
                                    <asp:Label ID="lblSlNo" runat="server" />
                                </td>

                                <td style="width: 25%">
                                    <%# Eval("clientName") %>
                                </td>

                                <td style="width: 20%; font-weight: 600">
                                    <%# Eval("ReqNo") %>
                                </td>

                                <td style="width: 15%">
                                    <%# Eval("CreatedBy") %>
                                </td>

                                <td style="width: 15%">
                                    <%# Eval("CreatedOn","{0:dd-MMM-yyyy HH:mm tt}") %>
                                </td>

                                <td style="width: 10%; text-align: center">
                                    <asp:Label ID="lblStatus"
                                        runat="server"
                                        Text='<%# Eval("Status") %>' />
                                </td>

                                <td style="width: 5%; text-align: center">
                                    <asp:ImageButton
                                        ID="btnView"
                                        runat="server"
                                        CommandName="View"
                                        CommandArgument='<%# Eval("ReqNo") %>'
                                        ImageUrl="~/corporate/business/WebImages/viewicon.png"
                                        ToolTip="View / Modify PR"
                                        Height="18" Width="18" />
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
