<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PO_Details.aspx.cs" MaintainScrollPositionOnPostback="true" Inherits="Bill_Software.corporate.business.app.View_PO_Details" %>

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

        .po-card {
            border: 1px solid #e2e6ea;
            border-radius: 8px;
            background: #fff;
            margin: 15px 0;
        }

        .po-card-header {
            padding: 10px 14px;
            font-weight: 600;
            background: #f5f7fa;
            border-bottom: 1px solid #e2e6ea;
        }

        .po-card-body {
            padding: 14px;
        }

        .po-section {
            margin-bottom: 14px;
        }

        .po-section-title {
            font-weight: 600;
            font-size: 13px;
            margin-bottom: 6px;
        }

        .po-grid-2 {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }

        .po-grid-4 {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
            gap: 14px;
        }

        .po-grid-4 > div,
        .po-grid-2 > div {
            min-width: 0;
        }


        .po-input {
            width: 100%;
            padding: 6px 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        .po-input,
        select,
        textarea {
            width: 100%;
            box-sizing: border-box;
        }

        label {
            display: block;
            margin-bottom: 4px;
            font-size: 12px;
            color: #555;
        }


        .po-radio-inline {
            margin-bottom: 6px;
        }

        label {
            font-size: 12px;
            color: #555;
            margin-bottom: 2px;
            display: block;
        }

        .req {
            color: #d9534f;
        }

        .mt-10 {
            margin-top: 10px;
        }

        .po-radio-group {
            display: flex;
            gap: 18px;
            margin-bottom: 6px;
        }

            .po-radio-group label {
                display: flex;
                align-items: center;
                gap: 6px;
                cursor: pointer;
                font-size: 13px;
            }

        select:disabled {
            background-color: #f3f4f6;
            color: #999;
            border-style: dashed;
        }


        .po-subsection {
            padding-bottom: 10px;
            margin-bottom: 12px;
            border-bottom: 1px dashed #e0e0e0;
        }

        .po-subsection:last-child {
            border-bottom: none;
        }
        .op-title {
            margin-top: 10px;
            padding-top: 6px;
            border-top: 2px solid #0d6efd;
        }
        .req {
            color: #dc3545;
            font-weight: bold;
        }
        .po-input.required {
            border-left: 3px solid #dc3545;
        }

        .po-input, select {
            height: 34px;
        }
        .po-lock-hint {
    font-size: 12px;
    color: #856404;
    background: #fff3cd;
    padding: 6px 10px;
    border-radius: 4px;
    margin-bottom: 8px;
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
                    <td colspan="4">
                        <asp:Panel ID="pnlPODetails" runat="server">

                            <div class="po-card">

                                <!-- HEADER -->
                                <div class="po-card-header">
                                    PO Delivery & Billing Details
                                </div>

                                <div class="po-card-body">

                                    <!-- ================= BILL TO / SHIP TO ================= -->
                                    <div class="po-grid-2">
                                        <div class="po-subsection">
                                            <!-- BILL TO -->
                                            <div class="po-section">

                                                <div class="po-section-title">Bill To</div>

                                                <asp:RadioButtonList ID="rblBillToType" runat="server"
                                                    CssClass="po-radio-group"
                                                    RepeatDirection="Horizontal"
                                                    AutoPostBack="true" RepeatColumns="2" CellPadding="5" CellSpacing="10" RepeatLayout="Table"
                                                    OnSelectedIndexChanged="rblBillToType_SelectedIndexChanged">
                                                    <asp:ListItem Text="Company" Value="Company" />
                                                    <asp:ListItem Text="Store" Value="Store" />
                                                </asp:RadioButtonList>


                                                <asp:DropDownList ID="ddlBillToCompany" runat="server"
                                                    CssClass="po-input" />

                                                <asp:DropDownList ID="ddlBillToStore" runat="server"
                                                    CssClass="po-input" />
                                            </div>
                                        </div>
                                        <div class="po-subsection">
                                            <!-- SHIP TO -->
                                            <div class="po-section">
                                                <div class="po-section-title">Ship To (Consignee)</div>

                                                <asp:RadioButtonList ID="rblShipToType" runat="server"
                                                    CssClass="po-radio-inline"
                                                    RepeatDirection="Horizontal"
                                                    AutoPostBack="true" RepeatColumns="2" CellPadding="5" CellSpacing="10" RepeatLayout="Table"
                                                    OnSelectedIndexChanged="rblShipToType_SelectedIndexChanged">
                                                    <asp:ListItem Text="Store" Value="Store" />
                                                    <asp:ListItem Text="Client (Direct Delivery)" Value="Client" />
                                                </asp:RadioButtonList>

                                                <asp:DropDownList ID="ddlShipToStore" runat="server"
                                                    CssClass="po-input" />

                                                <asp:DropDownList ID="ddlShipToClient" runat="server"
                                                    CssClass="po-input" />
                                            </div>
                                        </div>
                                    </div>

                                    <!-- ================= OPERATIONAL DETAILS ================= -->
                                    <div class="po-section">
                                        <div class="po-section-title op-title">Operational Details</div>

                                        <div class="po-grid-4">

                                            <div>
                                                <label>Engineer Name <span class="req">*</span></label>
                                                <asp:TextBox ID="txtEngineerName" runat="server"
                                                    CssClass="po-input" />
                                            </div>

                                            <div>
                                                <label>Dispatch Mode</label>
                                                <asp:DropDownList ID="ddlDispatchMode" runat="server"
                                                    CssClass="po-input">
                                                    <asp:ListItem Text="Transport" />
                                                    <asp:ListItem Text="Courier" />
                                                    <asp:ListItem Text="DTDC" />
                                                </asp:DropDownList>
                                            </div>

                                            <div>
                                                <label>Dispatch Upto</label>
                                                <asp:TextBox ID="txtDispatchUpto" runat="server"
                                                    CssClass="po-input" />
                                            </div>

                                            <div>
                                                <label>Delivery Basis</label>
                                                <asp:DropDownList ID="ddlDeliveryBasis" runat="server"
                                                    CssClass="po-input">
                                                    <asp:ListItem Text="Door Delivery" />
                                                    <asp:ListItem Text="Godown Delivery" />
                                                    <asp:ListItem Text="By Hand" />
                                                </asp:DropDownList>
                                            </div>

                                        </div>

                                        <div class="po-grid-2 mt-10">

                                            <div>
                                                <label>Freight Terms</label>
                                                <asp:DropDownList ID="ddlFreightTerms" runat="server"
                                                    CssClass="po-input">
                                                    <asp:ListItem Text="Paid" />
                                                    <asp:ListItem Text="To Pay" />
                                                    <asp:ListItem Text="Add in Invoice" />
                                                </asp:DropDownList>
                                            </div>

                                            <div>
                                                <label>Remarks</label>
                                                <asp:TextBox ID="txtRemarks" runat="server"
                                                    CssClass="po-input"
                                                    TextMode="MultiLine"
                                                    Rows="2" />
                                            </div>

                                        </div>
                                    </div>

                                </div>
                            </div>

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
                        <div class="po-lock-hint">
    <i class="fa fa-lock"></i>
    Once released, this PO cannot be edited.
</div>

                        <asp:Button ID="btnReleasePO" runat="server"
                            Text="Release Purchase Order" Width="150px"
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
