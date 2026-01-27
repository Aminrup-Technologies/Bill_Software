<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Generate_PO_From_PR.aspx.cs" Inherits="Bill_Software.corporate.business.app.Generate_PO_From_PR" %>

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
        }

        .preview-box {
            margin-top: 20px;
            padding: 15px;
            border: 1px solid #0b5f8a;
            background: #f9fbfd;
        }

            .preview-box h3 {
                margin-top: 0;
                color: #0b5f8a;
            }

        .po-preview-grid {
            border-collapse: collapse;
            width: 100%;
            font-size: 12px;
        }

            .po-preview-grid th {
                background-color: #0b5f8a;
                color: #ffffff;
                padding: 8px;
                text-align: center;
                border: 1px solid #0b5f8a;
                font-weight: bold;
            }

            .po-preview-grid td {
                padding: 7px 8px;
                border: 1px solid #d6d6d6;
                background-color: #ffffff;
            }

            .po-preview-grid tr:nth-child(even) td {
                background-color: #f6f9fc;
            }

            .po-preview-grid tr:hover td {
                background-color: #eef6ff;
            }

            .po-preview-grid .num {
                text-align: right;
                white-space: nowrap;
            }

            .po-preview-grid .center {
                text-align: center;
            }

            .po-preview-grid .amount {
                font-weight: bold;
                color: #0b5f8a;
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

                <asp:DataList ID="DataList1" runat="server"
                    Width="100%"
                    RepeatLayout="Table"
                    OnItemCommand="DataList1_ItemCommand" OnItemDataBound="DataList1_ItemDataBound">

                    <ItemTemplate>
                        <tr class="pr-row">
                            <!-- Sl No -->
                            <td style="width: 5%; text-align: center;">
                                <asp:Label ID="lblSlNo" runat="server" />
                            </td>

                            <!-- PR No -->
                            <td style="width: 15%;">
                                <%# Eval("ReqNo") %>
                            </td>

                            <!-- Vendor -->
                            <td style="width: 25%;">
                                <%# Eval("clientName") %>
                            </td>

                            <!-- Net Amount -->
                            <td style="width: 15%; text-align: right;">
                                <%# Eval("NetAmount", "{0:N2}") %>
                            </td>

                            <!-- Approved On -->
                            <td style="width: 20%; text-align: center;">
                                <%# Eval("ApprovedBy") %> &nbsp; &nbsp;
                                <%# Eval("ApprovedOn", "{0:dd-MMM-yyyy hh:mm tt}") %>
                            </td>

                            <!-- Actions -->
                            <td style="width: 20%; text-align: center;">
                                <asp:LinkButton ID="lnkPreview" runat="server"
                                    CommandName="Preview"
                                    CommandArgument='<%# Eval("ReqNo") %>'>
                    Preview
                                </asp:LinkButton>

                                <%--&nbsp;|&nbsp;--%>

                                <asp:LinkButton ID="lnkConvert" runat="server"
                                    CommandName="Convert" Visible="false"
                                    CommandArgument='<%# Eval("ReqNo") %>'
                                    OnClientClick="return confirm('Generate PO for this PR?');">
                    Convert
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>

                </asp:DataList>

                <br />
                <asp:Panel ID="pnlPreview" runat="server" Visible="false" CssClass="preview-box">

                    <div style="font-size: 14px; font-weight: bold; color: #0b5f8a; margin-bottom: 10px;">
                        PO Preview
                    </div>


                    <div style="margin-bottom: 12px; font-size: 12px;">
                        <strong>PR No:</strong>
                        <asp:Label ID="lblPrevReqNo" runat="server" />
                        &nbsp;&nbsp;&nbsp;
    <strong>Vendor:</strong>
                        <asp:Label ID="lblPrevVendor" runat="server" />
                    </div>



                    <asp:GridView ID="gvPreviewItems" runat="server"
                        AutoGenerateColumns="false"
                        ShowFooter="true"
                        CssClass="po-preview-grid"
                        OnRowDataBound="gvPreviewItems_RowDataBound">

                        <Columns>

                            <asp:BoundField DataField="SlNo" HeaderText="Sl">
                                <HeaderStyle CssClass="center" Width="5%" />
                                <ItemStyle CssClass="center" />
                            </asp:BoundField>

                            <asp:BoundField DataField="ProductName" HeaderText="Item">
                                <HeaderStyle Width="35%" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Qnty" HeaderText="Qty" DataFormatString="{0:N2}">
                                <HeaderStyle CssClass="center" Width="8%" />
                                <ItemStyle CssClass="num" />
                            </asp:BoundField>

                            <asp:BoundField DataField="Rate" HeaderText="Rate" DataFormatString="{0:N2}">
                                <HeaderStyle CssClass="center" Width="8%" />
                                <ItemStyle CssClass="num" />
                            </asp:BoundField>

                            <asp:BoundField DataField="TaxableAmount" HeaderText="Before GST" DataFormatString="{0:N2}">
                                <HeaderStyle CssClass="center" Width="12%" />
                                <ItemStyle CssClass="num" />
                            </asp:BoundField>

                            <asp:BoundField DataField="TaxAmount" HeaderText="GST Amt" DataFormatString="{0:N2}">
                                <HeaderStyle CssClass="center" Width="10%" />
                                <ItemStyle CssClass="num" />
                            </asp:BoundField>

                            <asp:BoundField DataField="NetAmount" HeaderText="After GST" DataFormatString="{0:N2}">
                                <HeaderStyle CssClass="center" Width="12%" />
                                <ItemStyle CssClass="num amount" />
                            </asp:BoundField>

                        </Columns>
                    </asp:GridView>



                    <br />

                    <div style="margin-top: 15px; text-align: right;">
                        <asp:Button ID="btnCancelPreview" runat="server"
                            Text="Cancel"
                            CssClass="btn btn-secondary btn_style"
                            OnClick="btnCancelPreview_Click" />

                        &nbsp;&nbsp;

    <asp:Button ID="btnCreatePO" runat="server"
        Text="Create PO"
        CssClass="btn btn-success btn_style"
        OnClick="btnCreatePO_Click" />
                    </div>



                </asp:Panel>

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
    </table>
</asp:Content>
