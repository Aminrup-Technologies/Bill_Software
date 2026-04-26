<%@ Page Title="Create Client" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="New_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm15" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>
    <style type="text/css">
        /* --- 1. Base Layout Components --- */
        .box-panel {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }

        .section-title {
            font-size: 16px;
            font-weight: bold;
            color: #19658A;
            border-bottom: 2px solid #19658A;
            padding-bottom: 5px;
            margin-bottom: 15px;
            margin-top: 10px;
        }

        /* --- 2. Form Controls --- */
        .form-group {
            margin-bottom: 15px;
        }

            .form-group label {
                font-weight: bold;
                display: block;
                margin-bottom: 5px;
            }

        .form-control {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .req {
            color: #FF3300;
        }

        /* --- 3. Buttons --- */
        .btn-primary {
            background: #19658A;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
        }

        .btn-secondary {
            background: #6c757d;
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
            margin-left: 10px;
        }

        /* --- 4. Select2 Modern High-Contrast Theme --- */
        /* Container & Selection Box */
        .select2-container {
            width: 100% !important;
        }

        .select2-container--default .select2-selection--single {
            height: 38px !important;
            border: 1px solid #ccc !important;
            border-radius: 4px !important;
            padding: 5px;
        }

            .select2-container--default .select2-selection--single .select2-selection__arrow {
                height: 36px !important;
            }

        /* Dropdown Results List */
        .select2-dropdown {
            background-color: #ffffff !important;
            border: 1px solid #19658A !important;
            z-index: 9999 !important;
        }

        .select2-results__option {
            color: #333333 !important;
            padding: 8px 12px !important;
            background-color: #ffffff !important;
        }

        /* Hover/Active States */
        .select2-container--default .select2-results__option--highlighted[aria-selected] {
            background-color: #19658A !important;
            color: #ffffff !important;
        }

        /* Inline City Addition / Tagging Visibility */
        .select2-container--default .select2-results__option[aria-selected=true] {
            background-color: #f4f8fb !important;
            color: #19658A !important;
        }

        /* Dropdown Search Field */
        .select2-search--dropdown .select2-search__field {
            border: 1px solid #ccc !important;
            color: #333 !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <script type="text/javascript">
        function ValidateClientData() {
            var name = document.getElementById('<%=txtvendorName.ClientID%>').value.trim();
            var phone = document.getElementById('<%=txtPhone.ClientID%>').value.trim();
            var email = document.getElementById('<%=txtEmail.ClientID%>').value.trim();
            var gst = document.getElementById('<%=txtservicetax_no.ClientID%>').value.trim();
            var city = document.getElementById('<%=ddlCity.ClientID%>').value;

            // 1. Name Check
            if (name === "") {
                alert("Client Name is required.");
                return false;
            }

            // 2. City Check
            if (city === "" || city === "--Select--") {
                alert("Please select or add a City.");
                return false;
            }

            // 3. Phone Check (Indian 10-digit mobile)
            var phoneRegex = /^[6-9]\d{9}$/;
            if (phone !== "" && !phoneRegex.test(phone)) {
                alert("Please enter a valid 10-digit Phone Number.");
                return false;
            }

            // 4. Email Check
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (email !== "" && !emailRegex.test(email)) {
                alert("Please enter a valid Email Address.");
                return false;
            }

            // 5. GSTIN Check (Indian Format: 22AAAAA0000A1Z5)
            var gstRegex = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$/;
            if (gst !== "" && !gstRegex.test(gst.toUpperCase())) {
                alert("Please enter a valid 15-character GST Number.");
                return false;
            }

            return true; // All good!
        }
    </script>

    <div class="box-panel">
        <h3 style="color: #19658A; margin-top: 0;">Create New Client</h3>

        <asp:Panel ID="PanelOK" runat="server" BackColor="#D4EDDA" BorderColor="#C3E6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #155724;">
            <strong>Success:</strong>
            <asp:Label ID="lblOk" runat="server"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="PanelError" runat="server" BackColor="#FFDDDD" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #FF3300;">
            <strong>Error:</strong>
            <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
        </asp:Panel>

        <div class="section-title">
            Primary Details (Next ID:
            <asp:Label ID="lbl_nxtclientid" runat="server" ForeColor="#FF6600"></asp:Label>)
        </div>
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
                <asp:DropDownList ID="ddlCity" runat="server" CssClass="form-control"></asp:DropDownList>
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

        <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 15px;">
            <asp:Button ID="btnSave" runat="server" CssClass="btn-primary" OnClick="btnSave_Click" Text="Save Client"
                UseSubmitBehavior="false"
                OnClientClick="if(ValidateClientData()){ this.value='Saving...'; this.style.pointerEvents='none'; this.style.opacity='0.7'; } else { return false; }" />

            <asp:Button ID="btnReset" runat="server" CssClass="btn-secondary" OnClick="btnReset_Click" Text="Reset" />
        </div>
    </div>

    <script type="text/javascript">
        $(document).ready(function () {

            // Initialize the City Dropdown as a Tagging Combobox
            $('#<%= ddlCity.ClientID %>').select2({
                placeholder: "🔍 Search or TYPE to Add New City...", // More explicit prompt
                tags: true,
                createTag: function (params) {
                    var term = $.trim(params.term);
                    if (term === '') {
                        return null;
                    }
                    // If it's a new word, show it with a label so the user knows it's new
                    return {
                        id: term,
                        text: term + " (Add New)",
                        isNew: true, // Custom flag to trigger AJAX
                        actualName: term
                    };
                }
            }).on('select2:select', function (e) {
                var data = e.params.data;

                // Check if they selected a BRAND NEW typed tag
                if (data.isNew) {
                    var stateDropdown = document.getElementById('<%= cmbState.ClientID %>');
                    var selectedState = stateDropdown.options[stateDropdown.selectedIndex].text;

                    // Gatekeeper: Ensure they picked a state first
                    if (selectedState === "" || selectedState === "--Select--") {
                        alert("Please select a State first before adding a new city.");
                        $('#<%= ddlCity.ClientID %>').val(null).trigger('change'); // Clear the bad tag
                        return;
                    }

                    // Fire the silent AJAX call to the C# Backend
                    PageMethods.AddNewCityInline(data.actualName, selectedState,
                        function (response) {
                            if (response.startsWith("ERROR:")) {
                                alert(response); // Show duplicate error
                                $('#<%= ddlCity.ClientID %>').val(null).trigger('change'); // Clear it
                            } else {
                                // Success! Swap the "(Add New)" tag with the clean saved name
                                var newOption = new Option(data.actualName, data.actualName, true, true);
                                $('#<%= ddlCity.ClientID %>').append(newOption).trigger('change');
                            }
                        },
                        function (error) {
                            alert("Error: " + error.get_message());
                            $('#<%= ddlCity.ClientID %>').val(null).trigger('change'); // Clear it
                        }
                    );
                    }
            });
        });
    </script>
</asp:Content>
