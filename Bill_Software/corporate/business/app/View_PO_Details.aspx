<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_PO_Details.aspx.cs" MaintainScrollPositionOnPostback="true" Inherits="Bill_Software.corporate.business.app.View_PO_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2 { color: #FFFFFF; font-weight: bold; }
        .page-title-bar { background: #19658A; padding: 8px; }
        .po-page { padding: 12px 8px 20px; }
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .po-card { border: 1px solid #e2e6ea; border-radius: 8px; background: #fff; margin: 12px 0; }
        .po-card-header { padding: 10px 14px; font-weight: 600; background: #f5f7fa; border-bottom: 1px solid #e2e6ea; color: #333; }
        .po-card-body { padding: 14px; }
        .po-section { margin-bottom: 14px; }
        .po-section-title { font-weight: 600; font-size: 13px; margin-bottom: 6px; color: #0b5a83; border-bottom: 1px solid #eee; padding-bottom: 4px;}
        .po-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        .po-grid-4 { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 14px; }
        .po-meta-grid { display: grid; grid-template-columns: auto 1fr auto 1fr; gap: 8px 16px; font-size: 13px; align-items: center; }
        .po-label { font-weight: bold; color: #333; }
        .po-value { font-weight: bold; color: #003366; }
        .po-grid { width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 0; }
        .po-grid th { background: #0b5a83; color: #fff; padding: 6px 8px; text-align: center; }
        .po-grid td { padding: 4px 8px; border-bottom: 1px solid #ddd; }
        .po-grid tr:hover { background: #f9f9f9; }
        .po-grid .num { text-align: right; }
        .po-summary { display: flex; flex-wrap: wrap; gap: 16px; justify-content: space-between; align-items: center; border: 1px solid #ccc; background: #fafafa; padding: 10px; font-size: 13px; margin-top: 10px; }
        .po-summary .total { font-size: 15px; font-weight: bold; }
        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.4); z-index: 9999; }
        .modal-box { background: #fff; width: 420px; max-width: 92vw; margin: 120px auto; padding: 20px; border-radius: 6px; text-align: center; box-shadow: 0 4px 12px rgba(0,0,0,0.3); }
        .modal-actions { margin-top: 20px; }
        label { font-size: 12px; color: #777; margin-bottom: 2px; display: block; }
        .review-text { font-size: 14px; font-weight: 500; color: #333; }
        .review-box { background: #fdfdfd; padding: 10px; border: 1px solid #eee; border-radius: 4px; }
        .po-lock-hint { font-size: 12px; color: #856404; background: #fff3cd; padding: 6px 10px; border-radius: 4px; margin-bottom: 8px; text-align: center;}
        .po-actions { text-align: center; padding: 16px 0; }
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
            return false;
        }

        function closeReleaseModal() {
            document.getElementById("confirmReleaseModal").style.display = "none";
            window._releaseBtn = null;
        }

        function confirmRelease() {
            if (!window._releaseBtn) return;
            disableButton(window._releaseBtn);
            document.getElementById("confirmReleaseModal").style.display = "none";
            __doPostBack(window._releaseBtn.name, "");
        }
    </script>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="page-title-bar">&nbsp;<span class="style2">Review & Release Purchase Order</span>&nbsp;</div>

            <div class="po-page">
                <asp:Label ID="lblPO_Id" runat="server" Visible="False" />

                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; margin: 15px 0;">
                    &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="Green" />
                </asp:Panel>

                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; margin: 15px 0; background: #fff0f0;">
                    &nbsp;<asp:Image ID="imgError" runat="server" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" Height="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red" />
                </asp:Panel>

                <div class="po-card">
                    <div class="po-card-header">Header</div>
                    <div class="po-card-body">
                        <div class="po-meta-grid">
                            <span class="po-label">PO No</span>
                            <asp:Label ID="lblPONo" runat="server" CssClass="po-value" />
                            <span class="po-label">Status</span>
                            <asp:Label ID="lblStatus" runat="server" Style="font-weight: bold; color: #CC6600;" />
                            <span class="po-label">Req No</span>
                            <asp:Label ID="lblReqNo" runat="server" CssClass="po-value" />
                            <span class="po-label">PO Date</span>
                            <asp:Label ID="lblPODate" runat="server" />
                            <span class="po-label">Vendor</span>
                            <asp:Label ID="lblVendor" runat="server" CssClass="po-value" style="grid-column: 2 / span 3;" />
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlPODetails" runat="server">
                    <div class="po-card">
                        <div class="po-card-header">Checker Review: Delivery & Billing Details</div>
                        <div class="po-card-body">
                            <div class="po-grid-2">
                                <div class="po-section review-box">
                                    <div class="po-section-title">Bill To (<asp:Label ID="lblBillToType" runat="server" />)</div>
                                    <div class="review-text">
                                        <asp:Label ID="lblBillToName" runat="server" Font-Bold="true" /><br />
                                        <asp:Label ID="lblBillToAddress" runat="server" Font-Size="12px" ForeColor="#555" />
                                    </div>
                                </div>

                                <div class="po-section review-box">
                                    <div class="po-section-title">Ship To / Consignee (<asp:Label ID="lblShipToType" runat="server" />)</div>
                                    <div class="review-text">
                                        <asp:Label ID="lblShipToName" runat="server" Font-Bold="true" /><br />
                                        <asp:Label ID="lblShipToAddress" runat="server" Font-Size="12px" ForeColor="#555" />
                                    </div>
                                </div>
                            </div>

                            <div class="po-section" style="margin-top: 15px;">
                                <div class="po-section-title">Operational Details</div>
                                <div class="po-grid-4">
                                    <div>
                                        <label>Engineer Name</label>
                                        <asp:Label ID="lblEngineerName" runat="server" CssClass="review-text" />
                                    </div>
                                    <div>
                                        <label>Dispatch Mode</label>
                                        <asp:Label ID="lblDispatchMode" runat="server" CssClass="review-text" />
                                    </div>
                                    <div>
                                        <label>Dispatch Upto</label>
                                        <asp:Label ID="lblDispatchUpto" runat="server" CssClass="review-text" />
                                    </div>
                                    <div>
                                        <label>Delivery Basis</label>
                                        <asp:Label ID="lblDeliveryBasis" runat="server" CssClass="review-text" />
                                    </div>
                                </div>
                                <div class="po-grid-2" style="margin-top: 15px;">
                                    <div>
                                        <label>Freight Terms</label>
                                        <asp:Label ID="lblFreightTerms" runat="server" CssClass="review-text" />
                                    </div>
                                    <div>
                                        <label>Remarks</label>
                                        <asp:Label ID="lblRemarks" runat="server" CssClass="review-text" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <div class="po-card">
                    <div class="po-card-header">Line Items</div>
                    <div class="po-card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gdPOItems" runat="server" AutoGenerateColumns="False" CssClass="po-grid"
                                GridLines="None" ShowFooter="True" OnRowDataBound="gdPOItems_RowDataBound" EmptyDataText="No items found">
                                <Columns>
                                    <asp:TemplateField HeaderText="Sl">
                                        <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                        <FooterTemplate><b>Total</b></FooterTemplate>
                                        <ItemStyle HorizontalAlign="Center" Width="4%" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ProductName" HeaderText="Item" />
                                    <asp:BoundField DataField="Quantity" HeaderText="Qty" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="Rate" HeaderText="Rate" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="DiscountPercent" HeaderText="Disc %" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="DiscountAmount" HeaderText="Disc Amt" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="TaxableAmount" HeaderText="Taxable" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="TaxRate" HeaderText="GST %" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="TaxAmount" HeaderText="GST Amt" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                    <asp:BoundField DataField="NetAmount" HeaderText="Net Amount" DataFormatString="{0:N2}" ItemStyle-CssClass="num" />
                                </Columns>
                            </asp:GridView>
                        </div>

                        <div class="po-summary">
                            <span>Gross: <asp:Label ID="lblGross" runat="server" Font-Bold="true" /></span>
                            <span>GST: <asp:Label ID="lblGST" runat="server" Font-Bold="true" /></span>
                            <span style="text-align: right;">Net Amount: <asp:Label ID="lblNet" runat="server" CssClass="total" ForeColor="#0b5a83" /></span>
                        </div>
                    </div>
                </div>

                <div class="po-actions">
                    <asp:Button ID="btnBack" runat="server" Text="Back to PO List" CssClass="btn btn-secondary btn_style"
                        PostBackUrl="View_PO.aspx" CausesValidation="false" />
                    &nbsp;&nbsp;
                    <asp:Panel ID="pnlReleaseActions" runat="server">
                        <div class="po-lock-hint">
                            <i class="fa fa-lock"></i> Please review carefully. Once released, this PO will be locked and sent to the vendor.
                        </div>
                        <asp:Button ID="btnReleasePO" runat="server" Text="Approve & Release PO" Width="200px"
                            CssClass="btn btn-success btn-lg btn_style" OnClientClick="return confirmReleasePO(this);" OnClick="btnReleasePO_Click" />
                    </asp:Panel>

                    <asp:Button ID="btnPrintPO" runat="server" Text="Print PO" CssClass="btn btn-info btn_style" Visible="false" />
                </div>
            </div>

            <div id="confirmReleaseModal" class="modal-overlay" style="display: none;">
                <div class="modal-box">
                    <h4>Confirm PO Release</h4>
                    <p>Once released, this Purchase Order will be <b>locked and cannot be modified</b>.</p>
                    <p>Do you want to proceed?</p>
                    <div class="modal-actions">
                        <button type="button" class="btn btn-secondary btn_style" onclick="closeReleaseModal()">Cancel</button>
                        <button type="button" class="btn btn-success btn_style" onclick="confirmRelease()">Yes, Release PO</button>
                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
