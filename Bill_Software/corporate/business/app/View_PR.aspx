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
                    OnItemCommand="DataList1_ItemCommand"
                    OnItemDataBound="DataList1_ItemDataBound">

                    <HeaderTemplate>
                        <table class="pr-header" width="100%">
                            <tr>
                                <th style="width: 4%">Sl</th>
                                <th style="width: 14%">Client</th>
                                <th style="width: 10%">PR No</th>

                                <th style="width: 18%">Created</th>
                                <th style="width: 18%">Submitted</th>
                                <th style="width: 18%">Approved</th>

                                <th style="width: 8%">Status</th>
                                <th style="width: 4%">View</th>
                            </tr>
                        </table>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <table class="pr-row" width="100%">
                            <tr>
                                <!-- SL -->
                                <td style="width: 4%; text-align: center">
                                    <asp:Label ID="lblSlNo" runat="server" />
                                </td>

                                <!-- Client -->
                                <td style="width: 14%">
                                    <%# Eval("clientName") %>
                                </td>

                                <!-- PR No -->
                                <td style="width: 10%; font-weight: 600">
                                    <%# Eval("ReqNo") %>
                                </td>

                                <!-- Created -->
                                <td style="width: 18%">
                                    <b><%# Eval("CreatedByName") %></b>
                                    <br />
                                    <small>ID: <%# Eval("CreatedById") %><br />
                                        <%# Eval("CreatedOn","{0:dd-MMM-yyyy HH:mm}") %>
                                    </small>
                                </td>

                                <!-- Submitted -->
                                <td style="width: 18%">
                                    <%# Eval("SubmittedByName") == DBNull.Value ? "-" : Eval("SubmittedByName") %>
                                    <br />
                                    <small>
                                        <%# Eval("SubmittedById") %>
                                        <%# Eval("SubmittedOn","{0:dd-MMM-yyyy HH:mm}") %>
                                    </small>
                                </td>

                                <!-- Approved -->
                                <td style="width: 18%">
                                    <%# Eval("ApprovedByName") == DBNull.Value ? "-" : Eval("ApprovedByName") %>
                                    <br />
                                    <small>
                                        <%# Eval("ApprovedById") %>
                                        <%# Eval("ApprovedOn","{0:dd-MMM-yyyy HH:mm}") %>
                                    </small>
                                </td>

                                <!-- Status -->
                                <td style="width: 8%; text-align: center">
                                    <asp:Label ID="lblStatus"
                                        runat="server"
                                        Text='<%# Eval("Status") %>' />
                                </td>

                                <!-- View -->
                                <td style="width: 4%; text-align: center">
                                    <asp:ImageButton
                                        ID="btnView"
                                        runat="server"
                                        CommandName="View"
                                        CommandArgument='<%# Eval("ReqNo") %>'
                                        ImageUrl="~/corporate/business/WebImages/viewicon.png"
                                        ToolTip="View PR"
                                        Height="18"
                                        Width="18" />
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
