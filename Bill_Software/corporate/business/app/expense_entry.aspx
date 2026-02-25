<%@ Page Title="Add Expense" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="expense_entry.aspx.cs" Inherits="Bill_Software.corporate.business.app.expense_entry" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .style3 { color: #FF3300; }
        .info-box { background-color: #e9ecef; padding: 15px; border-radius: 6px; border-left: 5px solid #19658A; margin-bottom: 20px; font-family: Arial, sans-serif; }
        
        /* GridView Styling */
        .expense-grid { width: 100%; border-collapse: collapse; margin-top: 20px; font-family: Arial, sans-serif; font-size: 13px; }
        .expense-grid th { background-color: #19658A; color: white; padding: 10px; text-align: left; border: 1px solid #ddd; }
        .expense-grid td { padding: 8px; border: 1px solid #ddd; }
        .expense-grid tr:nth-child(even) { background-color: #f9f9f9; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="style1" style="margin-top: 20px;">
                <tr>
                    <td bgcolor="#19658A" colspan="5">&nbsp;<span class="style2">Add New Expense Claim</span>&nbsp;</td>
                </tr>
                <tr><td colspan="5">&nbsp;</td></tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="3">
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
                
                <tr id="trVisitInfo" runat="server" visible="false">
                    <td>&nbsp;</td>
                    <td colspan="3">
                        <div class="info-box">
                            <h4 style="margin: 0 0 10px 0; color: #19658A;">🔗 Linked to Sales Visit</h4>
                            <b>Customer:</b> <asp:Label ID="lblLinkedCustomer" runat="server"></asp:Label><br />
                            <b>Visit Date:</b> <asp:Label ID="lblLinkedDate" runat="server"></asp:Label><br />
                            <b>Outcome:</b> <asp:Label ID="lblLinkedOutcome" runat="server"></asp:Label>
                        </div>
                        <asp:HiddenField ID="hfVisitId" runat="server" />
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td style="width: 5%;">&nbsp;</td>
                    <td style="width: 20%; padding: 10px 0;"><b><span class="style3">*</span>Expense Date:</b></td>
                    <td style="width: 30%;">
                        <asp:TextBox ID="txtExpenseDate" runat="server" TextMode="Date" CssClass="textbox_style" Width="90%" required="true"></asp:TextBox>
                    </td>
                    <td style="width: 40%;">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding: 10px 0;"><b><span class="style3">*</span>Category:</b></td>
                    <td>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="dropdown_style" Width="90%" required="true">
                            <asp:ListItem Value="">-- Select Category --</asp:ListItem>
                            <asp:ListItem>Travel - Fuel/Mileage</asp:ListItem>
                            <asp:ListItem>Travel - Train/Flight/Cab</asp:ListItem>
                            <asp:ListItem>Food & Meals</asp:ListItem>
                            <asp:ListItem>Accommodation</asp:ListItem>
                            <asp:ListItem>Client Entertainment</asp:ListItem>
                            <asp:ListItem>Miscellaneous</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding: 10px 0;"><b><span class="style3">*</span>Amount (₹):</b></td>
                    <td>
                        <asp:TextBox ID="txtAmount" runat="server" CssClass="textbox_style" Width="90%" TextMode="Number" step="0.01" required="true" placeholder="0.00"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding: 10px 0; vertical-align: top;"><b><span class="style3">*</span>Description:</b></td>
                    <td colspan="2">
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="textbox_style" TextMode="MultiLine" Rows="3" Width="95%" required="true" placeholder="Provide details of this expense..."></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td style="padding: 10px 0;"><b>Receipt Attachment:</b></td>
                    <td colspan="2"><asp:FileUpload ID="fileReceipt" runat="server" /></td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="3" style="text-align: center; border-top: 1px solid #ccc; padding-top: 20px; padding-bottom: 20px;">
                        <asp:Button ID="btnSubmit" runat="server" Text="➕ Add Expense" CssClass="btn_style" OnClick="btnSubmit_Click" style="background-color:#28a745; color:white; padding:8px 20px;" />
                        &nbsp;
                        <asp:Button ID="btnBack" runat="server" Text="Back to Calendar" CssClass="btn_style" OnClick="btnBack_Click" CausesValidation="false" formnovalidate="true" style="background-color:#6c757d; color:white; padding:8px 20px;" />
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr id="trExpenseGrid" runat="server" visible="false">
                    <td>&nbsp;</td>
                    <td colspan="3">
                        <h4 style="color: #19658A; border-bottom: 2px solid #ccc; padding-bottom: 5px;">Expenses claimed for this visit:</h4>
                        <asp:GridView ID="gvExpenses" runat="server" AutoGenerateColumns="False" CssClass="expense-grid" EmptyDataText="No expenses added yet.">
                            <Columns>
                                <asp:BoundField DataField="ExpenseDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                <asp:BoundField DataField="ExpenseCategory" HeaderText="Category" />
                                <asp:BoundField DataField="Description" HeaderText="Description" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount (₹)" DataFormatString="{0:N2}" />
                                <asp:BoundField DataField="ApprovalStatus" HeaderText="Status" />
                            </Columns>
                        </asp:GridView>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSubmit" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>