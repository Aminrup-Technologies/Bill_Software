<%@ Page Title="Update Vendor" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Update_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm12" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .section-title { font-size: 16px; font-weight: bold; color: #19658A; border-bottom: 2px solid #19658A; padding-bottom: 5px; margin-bottom: 15px; margin-top: 10px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { font-weight: bold; display: block; margin-bottom: 5px; color: #333; }
        .form-control { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
        
        .btn-primary { background: #19658A; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn-secondary { background: #6c757d; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; text-decoration: none; }
        .btn-primary:hover { background: #124d6b; }
        .req { color: red; }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtvendorName.ClientID%>').value == "") { alert("Provide Vendor Name."); return false; }
            if (document.getElementById('<%=txtAddress1.ClientID%>').value == "") { alert("Provide Vendor Address 1."); return false; }
            if (document.getElementById('<%=cmbcity.ClientID%>').selectedIndex == 0) { alert("Please Select City."); return false; }
            if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) { alert("Please Select State."); return false; }
            if (document.getElementById('<%=txtPin.ClientID%>').value == "") { alert("Provide Vendor PIN."); return false; }
            return true;
        }
        function ValidateVendorData() {
            // 1. Basic Empty Checks
            if (document.getElementById('<%=txtvendorName.ClientID%>').value.trim() == "") { alert("Provide Vendor Name."); return false; }
            if (document.getElementById('<%=txtAddress1.ClientID%>').value.trim() == "") { alert("Provide Vendor Address 1."); return false; }
            if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) { alert("Please Select State."); return false; }
            if (document.getElementById('<%=txtPin.ClientID%>').value.trim() == "") { alert("Provide Vendor PIN."); return false; }

            // 2. Email Format Validation (If provided)
            var email = document.getElementById('<%=txtEmail.ClientID%>').value.trim();
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (email !== "" && !emailRegex.test(email)) {
                alert("Please enter a valid Company Email ID (e.g., info@company.com).");
                return false;
            }

            // 3. Phone/Mobile Validation (10 to 15 digits)
            var phone = document.getElementById('<%=txtPhone.ClientID%>').value.trim();
            var phoneRegex = /^\d{10,15}$/;
            if (phone !== "" && !phoneRegex.test(phone)) {
                alert("Please enter a valid Phone Number (10 to 15 digits, no spaces or dashes).");
                return false;
            }

            // 4. PAN Number Validation (Indian Standard: 5 Letters, 4 Numbers, 1 Letter)
            var pan = document.getElementById('<%=txtpanNo.ClientID%>').value.trim().toUpperCase();
            var panRegex = /^[A-Z]{5}[0-9]{4}[A-Z]{1}$/;
            if (pan !== "" && !panRegex.test(pan)) {
                alert("Invalid PAN Number format. Example: ABCDE1234F");
                return false;
            }

                // 5. GSTIN Validation (Indian Standard: 15 alphanumeric characters)
            var gst = document.getElementById('<%=txtservicetaxNo.ClientID%>').value.trim().toUpperCase();
            var gstRegex = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/;
            if (gst !== "" && !gstRegex.test(gst)) {
                alert("Invalid GSTIN format. Please check the 15-character code.");
                return false;
            }

            return true; // All validations passed!
        }

        // Ensures only numbers can be typed in numeric fields
        function validateNumber(event) {
            var keycode = (event.which) ? event.which : event.keyCode;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) return false;
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="box-panel">
        <h3 style="color: #19658A; margin-top:0; display:flex; justify-content:space-between; align-items:center;">
            <span>Update Vendor Details</span>
            <span style="font-size: 14px; color: #FF6600; background: #fff3e0; padding: 4px 10px; border-radius: 4px;">Vendor ID: <asp:Label ID="lblvendor_id" runat="server"></asp:Label></span>
        </h3>
        
        <asp:Panel ID="PanelOK" runat="server" BackColor="#D4EDDA" BorderColor="#C3E6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #155724;">
            <strong>Success:</strong> <asp:Label ID="lblOk" runat="server"></asp:Label>
        </asp:Panel>

        <div class="section-title">Primary Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Vendor Name</label>
                <asp:TextBox ID="txtvendorName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Principle Vendor Code</label>
                <asp:TextBox ID="txt_pvc" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div class="section-title">Location & Contact Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label><span class="req">*</span> Address 1</label>
                <asp:TextBox ID="txtAddress1" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Address 2</label>
                <asp:TextBox ID="txtAddress2" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox>
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
                <label>Company Phone</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Company Email ID</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Company Website</label>
                <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Fax Number</label>
                <asp:TextBox ID="txtFax" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div class="section-title">Primary Representative</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>Representative Name</label>
                <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Designation</label>
                <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Rep Phone No.</label>
                <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="form-control" onkeypress="return validateNumber(event)"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Rep Email ID</label>
                <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div class="section-title">Compliance & Banking Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>GSTIN No.</label>
                <asp:TextBox ID="txtservicetaxNo" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>PAN No.</label>
                <asp:TextBox ID="txtpanNo" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>VAT No. (Legacy)</label>
                <asp:TextBox ID="txtvat" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Bank Account No.</label>
                <asp:TextBox ID="txt_vndr_bankacc" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>IFSC Code</label>
                <asp:TextBox ID="txt_ifsc" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Account Name</label>
                <asp:TextBox ID="txt_accholdername" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 15px;">
            <asp:Button ID="btnBack" runat="server" Text="← Back to List" CssClass="btn-secondary" OnClick="btnBack_Click" CausesValidation="false" />
            <asp:Button ID="btnUpdate" runat="server" CssClass="btn-primary" OnClick="btnUpdate_Click" Text="Update Vendor Details" OnClientClick="return ValidateField();" />
        </div>
    </div>
</asp:Content>