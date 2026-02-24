<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Generate_PO_From_PR.aspx.cs" Inherits="Bill_Software.corporate.business.app.Generate_PO_From_PR" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .pr-header th { background: #0b5f8a; color: white; padding: 8px; font-size: 12px; text-align: center; }
        .pr-row td { padding: 8px; border-bottom: 1px solid #e0e0e0; font-size: 12px; }
        .pr-row:hover { background-color: #f4f9ff; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Purchase Requisitions</span></td>
        </tr>
        <tr>
            <td colspan="4">
                <br />
                <table class="pr-header" width="100%">
                    <tr>
                        <th style="width: 5%">Sl</th>
                        <th style="width: 15%">PR No</th>
                        <th style="width: 25%">Vendor</th>
                        <th style="width: 15%">Net Amount</th>
                        <th style="width: 20%">Approved By (Timestamp)</th>
                        <th style="width: 20%">Actions</th>
                    </tr>
                </table>

                <asp:DataList ID="DataList1" runat="server" Width="100%" RepeatLayout="Table"
                    OnItemCommand="DataList1_ItemCommand" OnItemDataBound="DataList1_ItemDataBound">
                    <ItemTemplate>
                        <tr class="pr-row">
                            <td style="width: 5%; text-align: center;"><asp:Label ID="lblSlNo" runat="server" /></td>
                            <td style="width: 15%;"><%# Eval("ReqNo") %></td>
                            <td style="width: 25%;"><%# Eval("clientName") %></td>
                            <td style="width: 15%; text-align: right;"><%# Eval("NetAmount", "{0:N2}") %></td>
                            <td style="width: 20%; text-align: center;">
                                <%# Eval("ApprovedBy") %> &nbsp; &nbsp; <%# Eval("ApprovedOn", "{0:dd-MMM-yyyy hh:mm tt}") %>
                            </td>
                            <td style="width: 20%; text-align: center;">
                                <asp:LinkButton ID="lnkPreview" runat="server" CommandName="Preview" CommandArgument='<%# Eval("ReqNo") %>' CssClass="btn btn-sm btn-info">
                                    Process to PO
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:DataList>
            </td>
        </tr>
    </table>
</asp:Content>