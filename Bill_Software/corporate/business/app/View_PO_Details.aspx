<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PO_Details.aspx.cs" Inherits="Bill_Software.corporate.business.app.View_PO_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .style3 {
            color: #FF3300;
        }

        .style4 {
            text-align: center;
        }

        .auto-style1 {
            width: 100%;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
        }

        .redio {
            border: none;
        }

        .auto-style2 {
            height: 24px;
        }

        .textbox_style21 {
            text-align: center;
        }

        .auto-style3 {
            width: 10%;
            height: 24px;
        }

        .auto-style4 {
            width: 40%;
            height: 24px;
        }

        .field-error {
            border: 2px solid #d9534f !important;
            background-color: #fff0f0;
        }

        .pr-summary {
            margin-top: 10px;
            padding: 10px;
            background: #f4f9ff;
            border: 1px solid #cfe3ff;
            font-size: 12px;
        }

        .po-header-card {
            border: 1px solid #0b5a83;
            background: #f5fbff;
            border-radius: 4px;
            font-size: 13px;
        }

            .po-header-card td {
                padding: 6px 8px;
            }

        .po-label {
            font-weight: bold;
            color: #333;
        }

        .po-value {
            font-weight: bold;
            color: #003366;
        }

        .status-badge {
            padding: 3px 10px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
            display: inline-block;
        }

        .status-draft {
            background: #fff3cd;
            color: #856404;
        }

        .status-released {
            background: #d4edda;
            color: #155724;
        }

        .po-grid {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }

            .po-grid th {
                background: #0b5a83;
                color: #fff;
                padding: 8px;
                text-align: center;
            }

            .po-grid td {
                padding: 6px 8px;
                border-bottom: 1px solid #ddd;
            }

            .po-grid tr:hover {
                background: #f9f9f9;
            }

            .po-grid .num {
                text-align: right;
            }

        .po-summary {
            border: 1px solid #ccc;
            background: #fafafa;
            margin-top: 15px;
            padding: 10px;
            font-size: 13px;
        }

            .po-summary .total {
                font-size: 15px;
                font-weight: bold;
            }

        .po-action-bar {
            margin-top: 20px;
            padding: 12px;
            text-align: center;
            background: #f0f8ff;
            border-top: 2px solid #0b5a83;
        }

        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.4);
            z-index: 9999;
        }

        .modal-box {
            background: #fff;
            width: 420px;
            margin: 120px auto;
            padding: 20px;
            border-radius: 6px;
            text-align: center;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        }

        .modal-actions {
            margin-top: 20px;
        }

            .modal-actions .btn {
                margin: 0 8px;
            }

        .po-grid th {
            background: #0b5a83;
            color: white;
            font-size: 12px;
            padding: 6px;
        }

        .po-grid td {
            padding: 6px;
            border-bottom: 1px solid #ddd;
        }

        .po-grid .num {
            text-align: right;
            white-space: nowrap;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
    <script type="text/javascript">
        function disableButton(btn) {
            btn.disabled = true;
            btn.value = "Releasing...";
            btn.classList.add("disabled");
        }

        function confirmReleasePO(btn) {
            window._releaseBtn = btn;
            document.getElementById("confirmReleaseModal").style.display = "block";
            return false; // stop postback
        }

        function closeReleaseModal() {
            document.getElementById("confirmReleaseModal").style.display = "none";
            window._releaseBtn = null;
        }

        function confirmRelease() {
            if (!window._releaseBtn) return;

            disableButton(window._releaseBtn);
            document.getElementById("confirmReleaseModal").style.display = "none";

            // Trigger actual postback
            __doPostBack(window._releaseBtn.name, "");
        }
    </script>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <table class="style1" width="100%">

                <!-- ===== HEADER BAR ===== -->
                <tr>
                    <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">View Purchase Order</span>&nbsp;
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <!-- ===== HIDDEN IDS ===== -->
                <tr>
                    <td width="10%">&nbsp;</td>
                    <td colspan="2" width="40%">
                        <asp:Label ID="lblPO_Id" runat="server" Visible="False" />
                    </td>
                    <td colspan="2" width="40%">&nbsp;</td>
                    <td width="10%">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <!-- ===== PO HEADER INFO ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <table width="100%" cellpadding="6" cellspacing="0" class="po-header-card">

                            <tr>
                                <td class="po-label">PO No</td>
                                <td class="po-value">
                                    <asp:Label ID="lblPONo" runat="server" /></td>

                                <td style="width: 20%; font-weight: bold;">Status</td>
                                <td style="width: 30%;">
                                    <asp:Label ID="lblStatus" runat="server"
                                        Style="font-weight: bold; color: #CC6600;" />
                                </td>
                            </tr>

                            <tr>
                                <td style="font-weight: bold;">Req No</td>
                                <td>
                                    <asp:Label ID="lblReqNo" runat="server" />
                                </td>

                                <td style="font-weight: bold;">PO Date</td>
                                <td>
                                    <asp:Label ID="lblPODate" runat="server" />
                                </td>
                            </tr>

                            <tr>
                                <td style="font-weight: bold;">Vendor</td>
                                <td colspan="3">
                                    <asp:Label ID="lblVendor" runat="server"
                                        Style="font-weight: bold;" />
                                </td>
                            </tr>

                        </table>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>

                <!-- ===== SUCCESS MESSAGE ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server"
                            BackColor="#EEFFDD"
                            BorderColor="#006600"
                            BorderStyle="Solid"
                            BorderWidth="1px"
                            Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server"
                                ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server" />

                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <!-- ===== ERROR MESSAGE ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelError" runat="server"
                            BorderColor="#FF3300"
                            BorderStyle="Solid"
                            BorderWidth="1px"
                            Visible="False">
                            &nbsp;<asp:Image ID="imgError" runat="server"
                                ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png"
                                Width="16px" Height="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server" />

                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <!-- ===== PO ITEMS GRID ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">

                        <%--<asp:GridView ID="gdPOItems" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="po-grid"
                            GridLines="None">


                            <Columns>
                                <asp:BoundField DataField="ProductName" HeaderText="Item" />
                                <asp:BoundField DataField="Quantity" HeaderText="Qty" />
                                <asp:BoundField DataField="Rate" HeaderText="Rate" />
                                <asp:BoundField DataField="TaxableAmount" HeaderText="Taxable" />
                                <asp:BoundField DataField="TaxAmount" HeaderText="GST" />
                                <asp:BoundField DataField="NetAmount" HeaderText="Net Amount" />
                            </Columns>

                        </asp:GridView>--%>

                        <asp:GridView ID="gdPOItems" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="po-grid"
                            GridLines="None"
                            ShowFooter="True"
                            OnRowDataBound="gdPOItems_RowDataBound"
                            EmptyDataText="No items found for this Purchase Order">

                            <Columns>

                                <asp:TemplateField HeaderText="Sl">
                                    <ItemTemplate>
                                        <%# Container.DataItemIndex + 1 %>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <b>Total</b>
                                    </FooterTemplate>
                                    <ItemStyle HorizontalAlign="Center" Width="4%" />
                                </asp:TemplateField>

                                <asp:BoundField DataField="ProductName" HeaderText="Item" />

                                <asp:BoundField DataField="Quantity" HeaderText="Qty"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="Rate" HeaderText="Rate"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="DiscountPercent" HeaderText="Disc %"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="DiscountAmount" HeaderText="Disc Amt"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="TaxableAmount" HeaderText="Taxable"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="TaxRate" HeaderText="GST %"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="TaxAmount" HeaderText="GST Amt"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                                <asp:BoundField DataField="NetAmount" HeaderText="Net Amount"
                                    DataFormatString="{0:N2}"
                                    ItemStyle-CssClass="num" />

                            </Columns>

                        </asp:GridView>

                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <!-- ===== PO SUMMARY ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <table class="po-summary" width="100%">
                            <tr>
                                <td>Gross</td>
                                <td class="num">
                                    <asp:Label ID="lblGross" runat="server" /></td>
                                <td>GST</td>
                                <td class="num">
                                    <asp:Label ID="lblGST" runat="server" /></td>
                            </tr>
                            <tr>
                                <td></td>
                                <td></td>
                                <td>Net Amount</td>
                                <td class="num total">
                                    <asp:Label ID="lblNet" runat="server" />
                                </td>
                            </tr>
                        </table>

                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <!-- ===== ACTION BUTTONS ===== -->
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" style="text-align: center; padding: 10px;">

                        <asp:Button ID="btnReleasePO" runat="server"
                            Text="Release Purchase Order"
                            CssClass="btn btn-success btn-lg btn_style"
                            OnClientClick="return confirmReleasePO(this);"
                            OnClick="btnReleasePO_Click" />


                        &nbsp;&nbsp;

                    <asp:Button ID="btnPrintPO" runat="server"
                        Text="Print PO"
                        CssClass="btn btn-info btn_style"
                        Visible="false" />

                    </td>
                    <td>&nbsp;</td>
                </tr>

                <!-- CONFIRM RELEASE MODAL -->
                <div id="confirmReleaseModal" class="modal-overlay" style="display: none;">
                    <div class="modal-box">
                        <h4>Confirm PO Release</h4>

                        <p>
                            Once released, this Purchase Order will be
                        <b>locked and cannot be modified</b>.
                        </p>

                        <p>Do you want to proceed?</p>

                        <div class="modal-actions">
                            <button type="button" class="btn btn-secondary btn_style"
                                onclick="closeReleaseModal()">
                                Cancel</button>

                            <button type="button" class="btn btn-success btn_style"
                                onclick="confirmRelease()">
                                Yes, Release PO</button>
                        </div>
                    </div>
                </div>

            </table>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
