<%@ Page Title="Plan Sales Visit" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="daily_rpt.aspx.cs" Inherits="Bill_Software.corporate.business.app.daily_rpt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .style3 { color: #FF3300; }
        .style4 { text-align: center; }
    </style>

    <script type="text/javascript">
        window.onload = function () {
            limitVisitDate();
        };

        function limitVisitDate() {
            const dateInput = document.getElementById('<%= txtVisitDate.ClientID %>');
            const today = new Date();
            
            // For planning, restrict selection to today or future dates
            const minDate = today.toISOString().split('T')[0];
            dateInput.setAttribute('min', minDate);
        }

        function validateSalesVisitForm() {
            const visitDate = document.getElementById('<%= txtVisitDate.ClientID %>').value.trim();
            const salesperson = document.getElementById('<%= txtSalesperson.ClientID %>').value.trim();
            const customerName = document.getElementById('<%= txtCustomerName.ClientID %>').value.trim();
            const department = document.getElementById('<%= txtDepartment.ClientID %>').value.trim();
            const contactPerson = document.getElementById('<%= txtContactPerson.ClientID %>').value.trim();
            const visitType = document.getElementById('<%= ddlVisitType.ClientID %>').value;
            const discussion = document.getElementById('<%= txtDiscussion.ClientID %>').value.trim();

            let errorMsg = '';

            if (visitDate === '') errorMsg += '• Planned Visit Date is required.\n';
            if (salesperson === '') errorMsg += '• Salesperson Name is required.\n';
            if (customerName === '') errorMsg += '• Customer Name is required.\n';
            if (department === '') errorMsg += '• Please enter a Department.\n';
            if (contactPerson === '') errorMsg += '• Contact Person is required.\n';
            if (visitType === '') errorMsg += '• Please select a Visit Type.\n';
            if (discussion === '') errorMsg += '• Agenda / Purpose is required.\n';

            if (errorMsg !== '') {
                alert('Please fix the following errors:\n\n' + errorMsg);
                return false;
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript" language="javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript" language="javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({
                dateFormat: 'dd-M-yy',
                minDate: 0, // 0 means today. Disables past dates.
                changeMonth: true,
                changeYear: true
            });
        });
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <table class="style1">
                <tr>
                    <td bgcolor="#19658A" colspan="7">&nbsp;<span class="style2">Plan Sales Visit</span>&nbsp;</td>
                </tr>
                <tr><td colspan="7">&nbsp;</td></tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="5">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="5">
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr><td colspan="7">&nbsp;</td></tr>

                <tr>
                    <td>&nbsp;</td>
                    <td width="15%"><span class="style3">*</span>Planned Date</td>
                    <td width="25%">
                        <asp:TextBox ID="txtVisitDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                    <td width="15%"><span class="style3">*</span>Salesperson Name</td>
                    <td width="25%">
                        <asp:TextBox ID="txtSalesperson" runat="server" CssClass="textbox_style" ReadOnly="true"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td><span class="style3">*</span>Customer Name</td>
                    <td>
                        <asp:TextBox ID="txtCustomerName" runat="server" CssClass="textbox_style"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                    <td><span class="style3">*</span>Department</td>
                    <td>
                        <asp:TextBox ID="txtDepartment" runat="server" CssClass="textbox_style"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td><span class="style3">*</span>Contact Person</td>
                    <td>
                        <asp:TextBox ID="txtContactPerson" runat="server" CssClass="textbox_style"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                    <td><span class="style3">*</span>Visit Type</td>
                    <td>
                        <asp:DropDownList ID="ddlVisitType" runat="server" CssClass="dropdown_style">
                            <asp:ListItem Text="-- Select Visit Type --" Value="" />
                            <asp:ListItem>Office Visit</asp:ListItem>
                            <asp:ListItem>Plant Visit</asp:ListItem>
                            <asp:ListItem>Online Meeting</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td><span class="style3">*</span>Agenda / Purpose</td>
                    <td colspan="4">
                        <asp:TextBox ID="txtDiscussion" runat="server" CssClass="textbox_style" TextMode="MultiLine" Columns="2" Rows="4" Height="44px" Width="90%" Placeholder="What is the objective of this visit?"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr><td colspan="7">&nbsp;</td></tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" class="style4">
                        <asp:Button ID="btnSubmit" runat="server" Text="Save Visit Plan" CssClass="btn_style" OnClientClick="return validateSalesVisitForm();" OnClick="btnSubmit_Click" />
                        &nbsp;
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn_style" OnClick="btnReset_Click" />
                    </td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSubmit" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>