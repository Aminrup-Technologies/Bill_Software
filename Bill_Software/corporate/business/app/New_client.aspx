<%@ Page Title="Create Client" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="New_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm15" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .section-title { font-size: 16px; font-weight: bold; color: #19658A; border-bottom: 2px solid #19658A; padding-bottom: 5px; margin-bottom: 15px; margin-top: 10px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { font-weight: bold; display: block; margin-bottom: 5px; }
        .form-control { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
        .btn-primary { background: #19658A; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn-secondary { background: #6c757d; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; }
        .req { color: red; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="box-panel">
        <h3 style="color: #19658A; margin-top:0;">Create New Client</h3>
        
        <asp:Panel ID="PanelOK" runat="server" BackColor="#D4EDDA" BorderColor="#C3E6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #155724;">
            <strong>Success:</strong> <asp:Label ID="lblOk" runat="server"></asp:Label>
        </asp:Panel>

        <div class="section-title">Primary Details (Next ID: <asp:Label ID="lbl_nxtclientid" runat="server" ForeColor="#FF6600"></asp:Label>)</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Client Name</label>
                <asp:TextBox ID="txtvendorName" runat="server" CssClass="form-control" placeholder="Enter Client Name"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Industry Type</label>
                <asp:DropDownList ID="cmbIndustry" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
        </div>

        <div class="section-title">Corporate Office Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Address</label>
                <asp:TextBox ID="txtAddress1" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> State</label>
                <asp:DropDownList ID="cmbState" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> City</label>
                <asp:TextBox ID="txtCity" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> PIN Code</label>
                <asp:TextBox ID="txtPin" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> Phone Number</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Fax Number</label>
                <asp:TextBox ID="txtFax" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div class="section-title">Business & Tax Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>Company Website</label>
                <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Company Email ID</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>GST No</label>
                <asp:TextBox ID="txtservicetax_no" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>PAN No</label>
                <asp:TextBox ID="txtpanno" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Place of Supply</label>
                <asp:TextBox ID="txtplaceofSupply" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div style="text-align: right; margin-top: 20px;">
            <asp:Button ID="btnSave" runat="server" CssClass="btn-primary" OnClick="btnSave_Click" Text="Save Client" />
            <asp:Button ID="btnReset" runat="server" CssClass="btn-secondary" OnClick="btnReset_Click" Text="Reset" />
        </div>
    </div>
</asp:Content>