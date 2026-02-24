<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Generate_PO_Preview.aspx.cs" Inherits="Bill_Software.corporate.business.app.Generate_PO_Preview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2 { color: #FFFFFF; font-weight: bold; }
        
        /* Grid Styles */
        .po-preview-grid { border-collapse: collapse; width: 100%; font-size: 13px; margin-bottom: 20px; }
        .po-preview-grid th { background-color: #0b5f8a; color: #ffffff; padding: 10px; text-align: center; border: 1px solid #0b5f8a; font-weight: bold; }
        .po-preview-grid td { padding: 8px 10px; border: 1px solid #d6d6d6; background-color: #ffffff; }
        .po-preview-grid tr:nth-child(even) td { background-color: #f6f9fc; }
        .po-preview-grid .num { text-align: right; white-space: nowrap; }
        .po-preview-grid .center { text-align: center; }
        .po-preview-grid .amount { font-weight: bold; color: #0b5f8a; }

        /* PO Details Styles */
        .page-container { padding: 20px; background: #fdfdfd; }
        .po-card { border: 1px solid #e2e6ea; border-radius: 8px; background: #fff; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
        .po-card-header { padding: 12px 15px; font-weight: bold; font-size: 15px; background: #f5f7fa; border-bottom: 1px solid #e2e6ea; color: #333; }
        .po-card-body { padding: 20px; }
        
        .po-section { margin-bottom: 15px; }
        .po-section-title { font-weight: 600; font-size: 14px; margin-bottom: 10px; color: #0b5f8a; }
        .po-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
        .po-grid-4 { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 20px; }
        
        .po-input { width: 100%; padding: 8px 10px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; height: 36px; }
        select { width: 100%; box-sizing: border-box; height: 36px; }
        label { display: block; margin-bottom: 6px; font-size: 13px; color: #444; font-weight: 500;}
        .po-radio-group { display: flex; gap: 20px; margin-bottom: 10px; }
        .po-radio-group label { display: flex; align-items: center; gap: 5px; cursor: pointer; font-size: 13px; margin-bottom: 0; font-weight: normal;}
        select:disabled, input:disabled { background-color: #f3f4f6; color: #999; border-style: dashed; }
        
        .po-subsection { padding: 15px; background: #fafbfc; border: 1px solid #eee; border-radius: 6px; }
        .op-title { margin-top: 20px; padding-top: 15px; border-top: 2px solid #0d6efd; }
        .req { color: #dc3545; font-weight: bold; }
        
        .action-bar { text-align: right; padding: 15px; background: #fff; border-top: 1px solid #ddd; margin-top: 20px; }
        .info-header { margin-bottom: 15px; font-size: 15px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table width="100%" cellpadding="0" cellspacing="0">
        <tr>
            <td bgcolor="#19658A" style="padding: 8px;">&nbsp;<span class="style2">Generate Purchase Order from PR</span></td>
        </tr>
    </table>

    <div class="page-container">
        
        <div class="info-header">
            <strong>PR No:</strong> <asp:Label ID="lblPrevReqNo" runat="server" ForeColor="#0b5f8a" Font-Bold="true" />
            &nbsp; | &nbsp;
            <strong>Vendor:</strong> <asp:Label ID="lblPrevVendor" runat="server" ForeColor="#0b5f8a" Font-Bold="true" />
        </div>

        <asp:GridView ID="gvPreviewItems" runat="server" AutoGenerateColumns="false" ShowFooter="true"
            CssClass="po-preview-grid" OnRowDataBound="gvPreviewItems_RowDataBound">
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

        <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
        
        <div class="po-card">
            <div class="po-card-header">Step 2: PO Delivery & Billing Details</div>
            <div class="po-card-body">
                <div class="po-grid-2">
                    <div class="po-subsection">
                        <div class="po-section">
                            <div class="po-section-title">Bill To <span class="req">*</span></div>
                            <asp:RadioButtonList ID="rblBillToType" runat="server" CssClass="po-radio-group"
                                RepeatDirection="Horizontal" AutoPostBack="true" RepeatLayout="Flow"
                                OnSelectedIndexChanged="rblBillToType_SelectedIndexChanged">
                                <asp:ListItem Text="Company" Value="Company" />
                                <asp:ListItem Text="Store" Value="Store" />
                            </asp:RadioButtonList>
                            <div style="margin-top: 10px;">
                                <asp:DropDownList ID="ddlBillToCompany" runat="server" CssClass="po-input" Enabled="false" />
                                <asp:DropDownList ID="ddlBillToStore" runat="server" CssClass="po-input" Enabled="false" Style="margin-top:8px;" />
                            </div>
                        </div>
                    </div>

                    <div class="po-subsection">
                        <div class="po-section">
                            <div class="po-section-title">Ship To (Consignee) <span class="req">*</span></div>
                            <asp:RadioButtonList ID="rblShipToType" runat="server" CssClass="po-radio-group"
                                RepeatDirection="Horizontal" AutoPostBack="true" RepeatLayout="Flow"
                                OnSelectedIndexChanged="rblShipToType_SelectedIndexChanged">
                                <asp:ListItem Text="Store" Value="Store" />
                                <asp:ListItem Text="Client (Direct Delivery)" Value="Client" />
                            </asp:RadioButtonList>
                            <div style="margin-top: 10px;">
                                <asp:DropDownList ID="ddlShipToStore" runat="server" CssClass="po-input" Enabled="false" />
                                <asp:DropDownList ID="ddlShipToClient" runat="server" CssClass="po-input" Enabled="false" Style="margin-top:8px;"/>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="po-section">
                    <div class="po-section-title op-title">Operational Details</div>
                    <div class="po-grid-4">
                        <div>
                            <label>Engineer Name <span class="req">*</span></label>
                            <asp:TextBox ID="txtEngineerName" runat="server" CssClass="po-input" />
                        </div>
                        <div>
                            <label>Dispatch Mode</label>
                            <asp:DropDownList ID="ddlDispatchMode" runat="server" CssClass="po-input">
                                <asp:ListItem Text="Transport" />
                                <asp:ListItem Text="Courier" />
                                <asp:ListItem Text="DTDC" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label>Dispatch Upto</label>
                            <asp:TextBox ID="txtDispatchUpto" runat="server" CssClass="po-input" />
                        </div>
                        <div>
                            <label>Delivery Basis</label>
                            <asp:DropDownList ID="ddlDeliveryBasis" runat="server" CssClass="po-input">
                                <asp:ListItem Text="Door Delivery" />
                                <asp:ListItem Text="Godown Delivery" />
                                <asp:ListItem Text="By Hand" />
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="po-grid-2" style="margin-top: 15px;">
                        <div>
                            <label>Freight Terms</label>
                            <asp:DropDownList ID="ddlFreightTerms" runat="server" CssClass="po-input">
                                <asp:ListItem Text="Paid" />
                                <asp:ListItem Text="To Pay" />
                                <asp:ListItem Text="Add in Invoice" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label>Remarks</label>
                            <asp:TextBox ID="txtRemarks" runat="server" CssClass="po-input" TextMode="MultiLine" Rows="2" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="action-bar">
            <asp:Button ID="btnCancel" runat="server" Text="Cancel / Back" CssClass="btn btn-secondary btn_style" OnClick="btnCancel_Click" CausesValidation="false" />
            &nbsp;&nbsp;
            <asp:Button ID="btnCreatePO" runat="server" Text="Confirm & Create PO" CssClass="btn btn-success btn_style" OnClick="btnCreatePO_Click" OnClientClick="return confirm('Are you sure you want to generate the Purchase Order?');" />
        </div>
    </div>
</asp:Content>