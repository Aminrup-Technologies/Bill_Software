<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="New_vendor.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm5" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .style3 {
            color: #FF3300;
        }

        .style4 {
            text-align: center;
        }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtvendorName.ClientID%>').value == "") {
                alert("Provide Vendor Name.");
                document.getElementById('<%=txtvendorName.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=txtAddress1.ClientID%>').value == "") {
                alert("Provide Vendor Address ");
                document.getElementById('<%=txtAddress1.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=cmbcity.ClientID%>').selectedIndex == 0) {
                alert("Please Select City.");
                document.getElementById('<%=cmbcity.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=cmbState.ClientID%>').selectedIndex == 0) {
                alert("Please Select State.");
                document.getElementById('<%=cmbState.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtPin.ClientID%>').value == "") {
                alert("Provide Vendor Pin");
                document.getElementById('<%=txtPin.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=txtRepresentativeName.ClientID%>').value == "") {
                alert("Provide Representatives Name");
                document.getElementById('<%=txtRepresentativeName.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresantativeDesig.ClientID%>').value == "") {
                alert("Provide Representatives Designation.");
                document.getElementById('<%=txtRepresantativeDesig.ClientID%>').focus();
                return false;
            }




        }
    </script>

    <script type="text/javascript">
        //Function to allow only numbers to textbox
        function validate(key) {
            //getting key code of pressed key
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('txtfillrequar');
            //comparing pressed keycodes
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
                return false;
            }
            else {
                //Condition to check textbox contains ten numbers or not
                if (phn.value.length < 50) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="6">&nbsp;<span class="style2">Create Principle</span>&nbsp;</td>
        </tr>
        <tr>
            <td width="10%">&nbsp;</td>
            <td colspan="2" width="40%">&nbsp;</td>
            <td colspan="2" width="40%">&nbsp;</td>
            <td width="10%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD"
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server"
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>

            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">&nbsp;<span class="style3">*</span>Principle / Vendor Name&nbsp;</td>
            <td width="25%">
                <asp:TextBox ID="txtvendorName" runat="server" CssClass="textbox_style"
                    Width="250px"></asp:TextBox>
            </td>
            <td width="15%">&nbsp;<span class="style3">*</span>Address 1</td>
            <td width="25%">
                <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style"
                    Width="250px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td width="15%">Address 2</td>
            <td width="25%">
                <asp:TextBox ID="txtAddress2" runat="server" CssClass="textbox_style"
                    Width="250px"></asp:TextBox>
            </td>
            <td width="15%">
                <span class="style3">*</span>City</td>
            <td width="25%">
                <asp:TextBox ID="txtCity" runat="server" CssClass="textbox_style"></asp:TextBox>
                <asp:DropDownList ID="cmbcity" runat="server" Visible="false" CssClass="dropdown_style">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">
                <span class="style3">*</span>State&nbsp;</td>
            <td width="25%">
                <asp:DropDownList ID="cmbState" runat="server" CssClass="dropdown_style">
                </asp:DropDownList>
            </td>
            <td width="15%">
                <span class="style3">*</span>Pin&nbsp;&nbsp;</td>
            <td width="25%">
                <asp:TextBox ID="txtPin" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td width="15%">Company <span>Website</span></td>
            <td width="25%">
                <asp:TextBox ID="txtWebsite" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td width="15%">
                <span>Company Email ID</span></td>
            <td width="25%">
                <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">
                <span>Company Phone No</span></td>
            <td width="25%">
                <asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td width="15%">
                <span>Company Fax Number</span></td>
            <td width="25%">
                <asp:TextBox ID="txtFax" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">
                <span class="style3">*</span>Representatives Name</td>
            <td width="25%">
                <asp:TextBox ID="txtRepresentativeName" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td width="15%">
                <span class="style3">*</span>Designation</td>
            <td width="25%">
                <asp:TextBox ID="txtRepresantativeDesig" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">Phone No.</td>
            <td width="25%">
                <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td width="15%">Email</td>
            <td width="25%">
                <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">GSTIN No.</td>
            <td width="25%">
                <asp:TextBox ID="txtservicetaxNo" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td width="15%">Pan No</td>
            <td width="25%">
                <asp:TextBox ID="txtpanNo" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td width="15%"></td>
            <td width="25%">
            </td>
            <td width="15%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>

        <%--<tr>
            <td>&nbsp;</td>
            <td width="15%">Vat No</td>
            <td width="25%">
                <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
            <td width="15%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>--%>

        <tr>
            <td>&nbsp;</td>
            <td width="15%">Principle Vendor Code`</td>
            <td width="25%"><asp:TextBox ID="txt_pvc" runat="server" CssClass="textbox_style"></asp:TextBox></td>
            <td width="15%">Bank Acc No`</td>
            <td width="25%"><asp:TextBox ID="txt_vndr_bankacc" runat="server" CssClass="textbox_style"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">IFSC Code`</td>
            <td width="25%"><asp:TextBox ID="txt_ifsc" runat="server" CssClass="textbox_style"></asp:TextBox></td>
            <td width="15%">Account Name`</td>
            <td width="25%"><asp:TextBox ID="txt_accholdername" runat="server" CssClass="textbox_style"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td width="15%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
            <td width="25%">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4" class="style4">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style"
                    OnClick="btnSave_Click" Text="Save" OnClientClick="return ValidateField();" />
                &nbsp;
            <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn_style" OnClick="btnReset_Click" />
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
