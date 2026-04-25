<%@ Page Title="Manage Representatives" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Representative.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm59" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .box-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .section-title { font-size: 16px; font-weight: bold; color: #19658A; border-bottom: 2px solid #19658A; padding-bottom: 5px; margin-bottom: 15px; margin-top: 10px; }
        .form-group { margin-bottom: 15px; }
        .form-group label { font-weight: bold; display: block; margin-bottom: 5px; }
        .form-control { width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
        
        .btn-primary { background: #19658A; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn-secondary { background: #6c757d; color: #fff; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-left: 10px; text-decoration: none; }
        .btn-danger { background: #dc3545; color: white; padding: 5px 10px; border-radius: 3px; border: none; cursor: pointer; }
        .req { color: red; }

        /* Client Summary Card */
        .summary-card { background: #f4f8fb; border-left: 4px solid #19658A; padding: 15px; border-radius: 4px; margin-bottom: 20px; display: flex; gap: 20px; flex-wrap: wrap; }
        .summary-item { flex: 1; min-width: 200px; }
        .summary-item label { font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 2px; display: block; }
        .summary-item div { font-size: 14px; color: #222; font-weight: 500; }

        /* Modern DataList Table Styles */
        .modern-table { width: 100%; border-collapse: collapse; margin-top: 15px; }
        .modern-table th { background-color: #19658A; color: white; padding: 10px; text-align: left; border: 1px solid #ddd; }
        .modern-table td { padding: 10px; text-align: left; border: 1px solid #ddd; vertical-align: middle; }
        .modern-table tr:nth-child(even) { background-color: #f9f9f9; }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=cmbvendor.ClientID%>').selectedIndex == 0) { alert("Please Select a Client."); return false; }
            if (document.getElementById('<%=txtRepresentativeName.ClientID%>').value == "") { alert("Provide Representative's First Name."); return false; }
            if (document.getElementById('<%=txtRepresantativeDesig.ClientID%>').value == "") { alert("Provide Designation."); return false; }
            if (document.getElementById('<%=txtRepresentativePhone.ClientID%>').value == "") { alert("Provide Phone No."); return false; }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
        <h3 style="color: #19658A; margin: 0;">Manage Client Representatives</h3>
        <asp:Button ID="btnBack" runat="server" Text="← Back to Client List" CssClass="btn-secondary" OnClick="btnBack_Click" CausesValidation="false" />
    </div>

    <div class="box-panel">
        
        <asp:Panel ID="PanelOK" runat="server" BackColor="#D4EDDA" BorderColor="#C3E6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #155724;">
            <strong>Success:</strong> <asp:Label ID="lblOk" runat="server"></asp:Label>
        </asp:Panel>
        
        <asp:Panel ID="PanelError" runat="server" BackColor="#F8D7DA" BorderColor="#F5C6CB" BorderStyle="Solid" BorderWidth="1px" Visible="False" style="padding: 10px; border-radius: 4px; margin-bottom: 15px; color: #721C24;">
            <strong>Error:</strong> <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
        </asp:Panel>

        <div class="form-group" style="max-width: 500px;">
            <label><span class="req">*</span> Select Client</label>
            <asp:DropDownList ID="cmbvendor" runat="server" CssClass="form-control" AutoPostBack="True" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged"></asp:DropDownList>
        </div>

        <asp:Panel ID="pnlClientSummary" runat="server" Visible="false" CssClass="summary-card">
            <div class="summary-item">
                <label>Client Identity</label>
                <div><asp:Literal ID="litClientName" runat="server"></asp:Literal> (<asp:Literal ID="litClientId" runat="server"></asp:Literal>)</div>
            </div>
            <div class="summary-item">
                <label>Contact Info</label>
                <div>📞 <asp:Literal ID="litPhone" runat="server"></asp:Literal> | ✉️ <asp:Literal ID="litEmail" runat="server"></asp:Literal></div>
            </div>
            <div class="summary-item">
                <label>Corporate Address</label>
                <div>📍 <asp:Literal ID="litAddress" runat="server"></asp:Literal></div>
            </div>
        </asp:Panel>

        <div class="section-title">New Representative Details</div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>Title</label>
                <asp:DropDownList ID="ddlRepTitle" runat="server" CssClass="form-control">
                    <asp:ListItem Selected="True">Mr</asp:ListItem>
                    <asp:ListItem>Mrs</asp:ListItem>
                    <asp:ListItem>Ms</asp:ListItem>
                    <asp:ListItem>Dr</asp:ListItem>
                    <asp:ListItem>Md</asp:ListItem>
                    <asp:ListItem>Col</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> Designation</label>
                <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> First Name</label>
                <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Last Name</label>
                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label><span class="req">*</span> Phone No.</label>
                <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Email ID</label>
                <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
        </div>

        <div style="text-align: right; margin-top: 15px;">
            <asp:Button ID="btnSave" runat="server" CssClass="btn-primary" Text="Save Representative" OnClientClick="return ValidateField();" OnClick="btnSave_Click"/>
        </div>
    </div>

    <div class="box-panel">
        <h4 style="color: #333; margin-top: 0; border-bottom: 2px solid #ccc; padding-bottom: 5px;">Registered Representatives</h4>
        <asp:GridView ID="gvReps" runat="server" AutoGenerateColumns="False" CssClass="modern-table" Width="100%" OnRowCommand="gvReps_RowCommand" EmptyDataText="No representatives registered for this client.">
            <Columns>
                <asp:TemplateField HeaderText="Name">
                    <ItemTemplate>
                        <strong><%# Eval("RepTitle") %> <%# Eval("Representative_name") %> <%# Eval("RepLastName") %></strong>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Designation" HeaderText="Designation" />
                <asp:BoundField DataField="Phone_no" HeaderText="Phone No." />
                <asp:BoundField DataField="Email" HeaderText="Email ID" />
                <asp:BoundField DataField="CreatedBy" HeaderText="Added By" />
                <asp:BoundField DataField="CreatedOn" HeaderText="Added Date" DataFormatString="{0:dd-MMM-yyyy}" />
                <asp:TemplateField HeaderText="Manage">
                    <ItemStyle HorizontalAlign="Center" />
                    <ItemTemplate>
                        <asp:Button ID="btnDelete" runat="server" CommandName="DeleteRep" CommandArgument='<%# Eval("ID") %>' Text="Delete" CssClass="btn-danger" OnClientClick="return confirm('Delete this representative?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>