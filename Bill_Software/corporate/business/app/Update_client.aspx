<%@ Page Title="Update Client" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Update_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm17" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .section-title { font-size: 16px; font-weight: bold; color: #19658A; border-bottom: 2px solid #19658A; padding-bottom: 5px; margin-bottom: 15px; margin-top: 10px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { font-weight: bold; display: block; margin-bottom: 5px; }
        .form-control { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
        .btn-primary { background: #19658A; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn-secondary { background: #6c757d; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; text-decoration: none; }
        .req { color: red; }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtvendorName.ClientID%>').value == "") { alert("Provide Client Name."); return false; }
            if (document.getElementById('<%=cmbcity.ClientID%>').selectedIndex == 0) { alert("Please Select City."); return false; }
            if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) { alert("Please Select State."); return false; }
            if (document.getElementById('<%=txtPin.ClientID%>').value == "") { alert("Provide Client Pin"); return false; }
            return true;
        }
        function validateNumber(key) {
            var keycode = (key.which) ? key.which : key.keyCode;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) return false;
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="box-panel">
        <h3 style="color: #19658A; margin-top:0; display:flex; justify-content:space-between; align-items:center;">
            <span>Update Client Details</span>
            <span style="font-size: 14px; color: #FF6600; background: #fff3e0; padding: 4px 10px; border-radius: 4px;">Client ID: <asp:Label ID="lblvendor_id" runat="server"></asp:Label></span>
        </h3>
        
        <asp:Panel ID="PanelOK" runat="server" BackColor="#D4EDDA" BorderColor="#C3E6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #155724;">
            <strong>Success:</strong> <asp:Label ID="lblOk" runat="server"></asp:Label>
        </asp:Panel>

        <div class="section-title">Primary Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Client Name</label>
                <asp:TextBox ID="txtvendorName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Industry Type</label>
                <asp:DropDownList ID="cmbIndustry" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
        </div>

        <div class="section-title">Corporate Office Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Corporate Address</label>
                <asp:TextBox ID="txtAddress1" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> State</label>
                <asp:DropDownList ID="cmbState" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> City</label>
                <asp:DropDownList ID="cmbcity" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> PIN Code</label>
                <asp:TextBox ID="txtPin" runat="server" CssClass="form-control" onkeypress="return validateNumber(event)"></asp:TextBox>
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

        <div class="section-title">Registered Office Details (If Different)</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>Registered Address</label>
                <asp:TextBox ID="txtRegAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>State</label>
                <asp:DropDownList ID="ddlRegState" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>City</label>
                <asp:DropDownList ID="ddlRegCity" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>PIN Code</label>
                <asp:TextBox ID="txtRegPin" runat="server" CssClass="form-control" onkeypress="return validateNumber(event)"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Phone Number</label>
                <asp:TextBox ID="txtRegPhno" runat="server" CssClass="form-control"></asp:TextBox>
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
                <label>GST No.</label>
                <asp:TextBox ID="txtservicetax_no" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>PAN No.</label>
                <asp:TextBox ID="txtpanno" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 15px;">
            <asp:Button ID="btnBack" runat="server" Text="← Back to List" CssClass="btn-secondary" OnClick="btnBack_Click" />
            <asp:Button ID="btnUpdate" runat="server" CssClass="btn-primary" OnClick="btnUpdate_Click" Text="Update Client" OnClientClick="return ValidateField();" />
        </div>
    </div>
</asp:Content>