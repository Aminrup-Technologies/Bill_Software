<%@ Page Title="Add Employee" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="AddUser.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm79" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* Base Container */
        .dashboard-container {
            max-width: 1000px;
            margin: 30px auto;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        /* Card Elements */
        .section-card {
            background: #ffffff;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.06);
            border: 1px solid #eaeaea;
            margin-bottom: 30px;
        }

        .section-header {
            color: #19658A;
            margin-top: 0;
            border-bottom: 2px solid #f8f9fa;
            padding-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        /* Form Layout */
        .form-row {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-bottom: 18px;
        }

        .form-group {
            flex: 1;
            min-width: 220px;
            display: flex;
            flex-direction: column;
        }

            .form-group label {
                font-weight: 600;
                margin-bottom: 6px;
                color: #444;
                font-size: 13px;
            }

        .form-control {
            padding: 10px 12px;
            border: 1px solid #ccc;
            border-radius: 6px;
            box-sizing: border-box;
            font-size: 14px;
            transition: border-color 0.3s;
            font-family: inherit;
            background-color: #fff;
        }

            .form-control:focus {
                border-color: #19658A;
                outline: none;
                box-shadow: 0 0 0 3px rgba(25,101,138,0.15);
            }

        /* Buttons */
        .btn-action {
            padding: 12px 30px;
            font-size: 15px;
            font-weight: bold;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
            color: white;
            background: linear-gradient(135deg, #19658A, #124B68);
        }

            .btn-action:hover {
                transform: translateY(-2px);
                box-shadow: 0 4px 10px rgba(0,0,0,0.15);
            }

        .btn-delete {
            background: #dc3545;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: bold;
            font-size: 12px;
            transition: 0.2s;
        }

            .btn-delete:hover {
                background: #c82333;
            }

        /* Modern Data Table Styling */
        .table-responsive {
            overflow-x: auto;
            width: 100%;
            border-radius: 8px;
        }

        .modern-grid {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            margin-top: 10px;
            font-size: 13px;
            text-align: left;
        }

            .modern-grid th {
                background-color: #19658A;
                color: white;
                padding: 12px 15px;
                font-weight: 600;
                text-transform: uppercase;
                font-size: 11px;
                letter-spacing: 0.5px;
            }

                .modern-grid th:first-child {
                    border-top-left-radius: 8px;
                }

                .modern-grid th:last-child {
                    border-top-right-radius: 8px;
                }

            .modern-grid td {
                padding: 12px 15px;
                border-bottom: 1px solid #f0f0f0;
                vertical-align: middle;
                color: #444;
            }

            .modern-grid tr:hover td {
                background-color: #f8fbfd;
            }
    </style>

    <%--<script type="text/javascript">
        function ValidateField() {
            var name = document.getElementById('<%=txtEmployee.ClientID%>');
            var email = document.getElementById('<%=txtEmail.ClientID%>');
            var phone = document.getElementById('<%=txtPhno.ClientID%>');
            var pass = document.getElementById('<%=txtPass.ClientID%>');
            var role = document.getElementById('<%=ddlRole.ClientID%>');

            if (name.value.trim() === "") { alert("Please provide the Employee Name."); name.focus(); return false; }
            if (phone.value.trim() === "") { alert("Please provide the Phone Number."); phone.focus(); return false; }
            if (email.value.trim() === "") { alert("Please provide the Email Address."); email.focus(); return false; }
            if (pass.value.trim() === "") { alert("Please provide a Temporary Password."); pass.focus(); return false; }
            if (role.value === "" || role.value === "0") { alert("Please assign a System Role."); role.focus(); return false; }

            return true;
        }

        function confirmDelete() {
            return confirm('Are you sure you want to deactivate and remove this user?');
        }
    </script>--%>

    <script type="text/javascript">
        function checkDuplicatesAndSubmit(btnElement) {
            var name = document.getElementById('<%=txtEmployee.ClientID%>');
            var email = document.getElementById('<%=txtEmail.ClientID%>');
            var phone = document.getElementById('<%=txtPhno.ClientID%>');
            var pass = document.getElementById('<%=txtPass.ClientID%>');
            var role = document.getElementById('<%=ddlRole.ClientID%>');

            // 1. Basic empty field validation
            if (name.value.trim() === "") { alert("Please provide the Employee Name."); name.focus(); return false; }
            if (email.value.trim() === "") { alert("Please provide the Email Address."); email.focus(); return false; }
            if (pass.value.trim() === "") { alert("Please provide a Temporary Password."); pass.focus(); return false; }
            if (role.value === "" || role.value === "0") { alert("Please assign a System Role."); role.focus(); return false; }

            // 2. WhatsApp Format Validation (Requires '+' and Country Code)
            var phoneVal = phone.value.trim();
            var phoneRegex = /^\+\d{1,4}\s?\d{6,14}$/; 
            
            if (phoneVal === "") { 
                alert("Please provide the Phone Number."); phone.focus(); return false; 
            }
            if (!phoneRegex.test(phoneVal)) {
                alert("Invalid format! Mobile number must include the country code starting with '+' (e.g., +91 for India).");
                phone.focus(); 
                return false;
            }

            // 3. Change button UI to show it's working
            var originalText = btnElement.value;
            btnElement.value = "⏳ Checking...";
            btnElement.disabled = true;

            // 4. Check the database asynchronously
            fetch('AddUser.aspx/CheckDuplicates', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ email: email.value.trim(), phone: phoneVal })
            })
            .then(response => response.json())
            .then(data => {
                if (data.d === "Valid") {
                    // Safe to submit! Trigger the ASP.NET postback
                    __doPostBack(btnElement.name, '');
                } else {
                    // Duplicate found! Show the error and unlock the button
                    alert(data.d); 
                    btnElement.value = originalText;
                    btnElement.disabled = false;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                btnElement.value = originalText;
                btnElement.disabled = false;
            });

            return false; 
        }

        function confirmDelete() {
            return confirm('Are you sure you want to deactivate and remove this user?');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-container">

        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" Style="margin-bottom: 20px; border-radius: 6px; text-align: center;">
            <asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="10" BackColor="#FFDDDD" Style="margin-bottom: 20px; border-radius: 6px; text-align: center;">
            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
        </asp:Panel>

        <div class="section-card">
            <h2 class="section-header"><span style="font-size: 22px;">👤</span> Onboard New Employee</h2>

            <div class="form-row">
                <div class="form-group">
                    <label>Employee Name <span style="color: red">*</span></label>
                    <asp:TextBox ID="txtEmployee" runat="server" CssClass="form-control" placeholder="e.g. John Doe"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Email Address <span style="color: red">*</span></label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="john.doe@company.com"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Mobile No. (with Country Code) <span style="color:red">*</span></label>
                    <asp:TextBox ID="txtPhno" runat="server" CssClass="form-control" placeholder="e.g. +91 9876543210"></asp:TextBox>
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Temporary Password <span style="color: red">*</span></label>
                    <asp:TextBox ID="txtPass" runat="server" CssClass="form-control" placeholder="Will be required to change on first login"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>System Role <span style="color: red">*</span></label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
            </div>

            <h4 style="color: #666; border-bottom: 1px dashed #eee; padding-bottom: 5px; margin-top: 10px;">Corporate Hierarchy (Optional)</h4>

            <div class="form-row">
                <div class="form-group">
                    <label>Department</label>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Designation</label>
                    <asp:DropDownList ID="ddlDesignation" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Reporting Manager</label>
                    <asp:DropDownList ID="ddlManager" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
            </div>

            <div style="text-align: right; margin-top: 20px;">
                <asp:Button ID="btnSave" runat="server" CssClass="btn-action" Text="➕ Create User Account"
                    UseSubmitBehavior="false"
                    OnClientClick="return checkDuplicatesAndSubmit(this);"
                    OnClick="btnSave_Click" />
            </div>
        </div>

        <div class="section-card">
            <h3 class="section-header"><span style="font-size: 20px;">📋</span> Recently Onboarded Employees</h3>
            <div class="table-responsive">
                <asp:GridView ID="gvRecentUsers" runat="server" AutoGenerateColumns="False" CssClass="modern-grid" EmptyDataText="No recent active users found." GridLines="None" OnRowCommand="gvRecentUsers_RowCommand" DataKeyNames="Id">
                    <Columns>
                        <asp:BoundField DataField="User_Id" HeaderText="Emp ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:TemplateField HeaderText="Contact Info">
                            <ItemTemplate>
                                <span style="color: #007bff;"><%# Eval("Email") %></span><br />
                                <span style="color: #666; font-size: 11px;">📞 <%# Eval("Phone_no") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="RoleName" HeaderText="System Role" NullDisplayText="Unassigned" />
                        <asp:BoundField DataField="DepartmentName" HeaderText="Department" NullDisplayText="-" />
                        <asp:BoundField DataField="DesignationName" HeaderText="Designation" NullDisplayText="-" />
                        <asp:BoundField DataField="ManagerName" HeaderText="Reporting Manager" NullDisplayText="None" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnDelete" runat="server" Text="✖ Remove" Enabled="false" CssClass="btn-delete" CommandName="Inactivate" CommandArgument='<%# Eval("Id") %>' OnClientClick="return confirmDelete();" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </div>
</asp:Content>
