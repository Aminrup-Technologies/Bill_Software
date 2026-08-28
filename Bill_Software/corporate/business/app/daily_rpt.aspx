<%@ Page Title="Sales Visit" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="daily_rpt.aspx.cs" Inherits="Bill_Software.corporate.business.app.daily_rpt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .style3 { color: #FF3300; }
        .style4 { text-align: center; }
    </style>

    <script type="text/javascript">
        function validateSalesVisitForm() {
            const mode = '<%= Request.QueryString["mode"] ?? "plan" %>';
            const visitStart = document.getElementById('<%= txtVisitStart.ClientID %>').value.trim();
            const visitEnd = document.getElementById('<%= txtVisitEnd.ClientID %>').value.trim();
            const customerName = document.getElementById('<%= txtCustomerName.ClientID %>').value.trim();
            const department = document.getElementById('<%= txtDepartment.ClientID %>').value.trim();
            const contactPerson = document.getElementById('<%= txtContactPerson.ClientID %>').value.trim();
            const visitType = document.getElementById('<%= ddlVisitType.ClientID %>').value;
            const discussion = document.getElementById('<%= txtDiscussion.ClientID %>').value.trim();
            
            let errorMsg = '';
            if (visitStart === '') errorMsg += '• Start Date & Time is required.\n';
            if (visitEnd === '') errorMsg += '• End Date & Time is required.\n';
            
            // Validate chronological order if both are provided
            if (visitStart !== '' && visitEnd !== '') {
                if (new Date(visitStart) >= new Date(visitEnd)) {
                    errorMsg += '• End Time must be strictly after the Start Time.\n';
                }
            }

            if (customerName === '') errorMsg += '• Customer Name is required.\n';
            if (department === '') errorMsg += '• Please enter a Department.\n';
            if (contactPerson === '') errorMsg += '• Contact Person is required.\n';
            if (visitType === '') errorMsg += '• Please select a Visit Type.\n';
            if (discussion === '') errorMsg += (mode === 'past') ? '• Visit Outcome is required.\n' : '• Agenda is required.\n';
            
            if (mode === 'past') {
                const followUp = document.getElementById('<%= ddlFollowUp.ClientID %>').value;
                const status = document.getElementById('<%= ddlStatus.ClientID %>').value;
                if (followUp === '') errorMsg += '• Please select Follow-Up requirement.\n';
                if (status === '') errorMsg += '• Please select Execution Status.\n';
            }

            if (errorMsg !== '') {
                alert('Please fix the following errors:\n\n' + errorMsg);
                return false;
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="style1" style="margin-top: 20px;">
                <tr>
                    <td bgcolor="#19658A" colspan="7" style="padding: 8px;">&nbsp;<asp:Label ID="lblPageTitle" runat="server" CssClass="style2"></asp:Label>&nbsp;</td>
                </tr>
                <tr><td colspan="7">&nbsp;</td></tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="5">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5">
                            &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server" Font-Bold="true" ForeColor="DarkGreen"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" Padding="5" BackColor="#FFDDDD">
                            &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="Red"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr><td colspan="7">&nbsp;</td></tr>

                <tr>
                    <td style="width: 2%">&nbsp;</td>
                    <td width="15%"><span class="style3">*</span>Start Time</td>
                    <td width="30%">
                        <asp:TextBox ID="txtVisitStart" runat="server" TextMode="DateTimeLocal" CssClass="textbox_style" Width="90%"></asp:TextBox>
                    </td>
                    <td style="width: 2%">&nbsp;</td>
                    <td width="15%"><span class="style3">*</span>End Time</td>
                    <td width="30%">
                        <asp:TextBox ID="txtVisitEnd" runat="server" TextMode="DateTimeLocal" CssClass="textbox_style" Width="90%"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px;"><span class="style3">*</span>Salesperson</td>
                    <td style="padding-top: 15px;">
                        <asp:TextBox ID="txtSalesperson" runat="server" CssClass="textbox_style" ReadOnly="true" Width="90%" BackColor="#f4f4f4"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px;"><span class="style3">*</span>Customer Name</td>
                    <td style="padding-top: 15px;"><asp:TextBox ID="txtCustomerName" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px;"><span class="style3">*</span>Department</td>
                    <td style="padding-top: 15px;"><asp:TextBox ID="txtDepartment" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px;"><span class="style3">*</span>Contact Person</td>
                    <td style="padding-top: 15px;"><asp:TextBox ID="txtContactPerson" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px;"><span class="style3">*</span>Visit Type</td>
                    <td style="padding-top: 15px;">
                        <asp:DropDownList ID="ddlVisitType" runat="server" CssClass="dropdown_style" Width="90%">
                            <asp:ListItem Text="-- Select Type --" Value="" />
                            <asp:ListItem>Office Visit</asp:ListItem>
                            <asp:ListItem>Plant Visit</asp:ListItem>
                            <asp:ListItem>Online Meeting</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                    <td style="padding-top: 15px; vertical-align: top;"><span class="style3">*</span><asp:Label ID="lblDiscussionLabel" runat="server"></asp:Label></td>
                    <td style="padding-top: 15px;">
                        <asp:TextBox ID="txtDiscussion" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="3" Width="90%"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>

            <asp:Panel ID="pnlExecution" runat="server">
                <table class="style1">
                    <tr>
                        <td style="width: 2%">&nbsp;</td>
                        <td width="15%" style="padding-top: 15px;"><span class="style3">*</span>Follow-Up</td>
                        <td width="30%" style="padding-top: 15px;">
                            <asp:DropDownList ID="ddlFollowUp" runat="server" CssClass="dropdown_style" Width="90%">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem>Yes</asp:ListItem>
                                <asp:ListItem>No</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                        <td style="width: 2%">&nbsp;</td>
                        <td width="15%" style="padding-top: 15px;">Next Follow-Up Time</td>
                        <td width="30%" style="padding-top: 15px;">
                            <asp:TextBox ID="txtNextFollowUp" runat="server" TextMode="DateTimeLocal" CssClass="textbox_style" Width="90%"></asp:TextBox>
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td style="padding-top: 15px;"><span class="style3">*</span>Status</td>
                        <td style="padding-top: 15px;">
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="dropdown_style" Width="90%">
                                <asp:ListItem Text="-- Select Status --" Value="" />
                                <asp:ListItem>Completed</asp:ListItem>
                                <asp:ListItem>Pending</asp:ListItem>
                                <asp:ListItem>Escalated</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                        <td>&nbsp;</td>
                        <td style="padding-top: 15px;">Attachment</td>
                        <td style="padding-top: 15px;">
                            <asp:FileUpload ID="fileAttachment" runat="server" Width="90%" />
                        </td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </asp:Panel>

            <table class="style1">
                <tr><td colspan="3">&nbsp;</td></tr>
                <tr>
                    <td class="style4" style="padding: 20px;">
                        <asp:Button ID="btnSubmit" runat="server" Text="Save Record" CssClass="btn_style" OnClientClick="return validateSalesVisitForm();" OnClick="btnSubmit_Click" style="background-color: #19658A; color: white; padding: 8px 25px; border: none; font-weight: bold; cursor: pointer;" />
                        &nbsp;&nbsp;
                        <asp:Button ID="btnReset" runat="server" Text="Cancel" CssClass="btn_style" OnClick="btnReset_Click" CausesValidation="false" style="background-color: #6c757d; color: white; padding: 8px 25px; border: none; font-weight: bold; cursor: pointer;" />
                    </td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSubmit" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>