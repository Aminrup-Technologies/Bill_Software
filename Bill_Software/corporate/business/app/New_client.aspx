<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="New_client.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm15" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style type="text/css">
    .style1
    {
        width: 100%;
    }
    .style2
    {
        color: #FFFFFF;
        font-weight: bold;
    }
    .style3
    {
        color: #FF3300;
    }
        .style4
        {
            text-align: center;
        }
         .auto-style1 {
             width: 40%;
         }
         .auto-style2 {
             height: 26px;
         }
    </style>
    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=txtvendorName.ClientID%>').value == "") {
                alert("Provide Client Name.");
                document.getElementById('<%=txtvendorName.ClientID%>').focus();
                return false;
            }

          <%--  if (document.getElementById('<%=txtAddress1.ClientID%>').value == "") {
                alert("Provide Client Address ");
                document.getElementById('<%=txtAddress1.ClientID%>').focus();
                return false;
            }--%>

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
                alert("Provide Client Pin");
                document.getElementById('<%=txtPin.ClientID%>').focus();
                return false;
            }

           <%-- if (document.getElementById('<%=txtRepresentativeName.ClientID%>').value == "") {
                alert("Provide Representatives Name");
                document.getElementById('<%=txtRepresentativeName.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=txtRepresantativeDesig.ClientID%>').value == "") {
                alert("Provide Representatives Designation.");
                document.getElementById('<%=txtRepresantativeDesig.ClientID%>').focus();
                return false;
            }--%>




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
        <td bgcolor="#19658A" colspan="6">
            &nbsp;<span class="style2">Create Client</span>&nbsp;</td>
    </tr>
    <tr>
        <td width="10%">
            &nbsp;</td>
        <td colspan="2" width="40%">
            &nbsp;</td>
        <td colspan="2" width="40%">
            &nbsp;</td>
        <td width="10%">
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="4">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
        
            </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>


         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;<span class="style3">*</span>Client Name&nbsp;</td>
        <td width="25%">
            <asp:TextBox ID="txtvendorName" runat="server" CssClass="textbox_style" 
                Width="250px"></asp:TextBox>
        </td>
        <td width="15%" class="auto-style1" colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td colspan="4" style=" padding:3px 3px 3px 3px; text-align:center; font-weight:bold; background-color:gray; color:white;">Corporate Office Details</td>
        <td>
            &nbsp;</td>
    </tr>
          <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;<span class="style3">*</span>Corporate Office Address&nbsp;</td>
        <td width="25%">
            <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" TextMode="MultiLine" Width="250px"></asp:TextBox>
        </td>
        <td width="15%">
            <span class="style3">*</span>State&nbsp;&nbsp;</td>
        <td width="25%">
            <asp:DropDownList ID="cmbState" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
              </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td class="auto-style2">
             </td>
        <td width="15%" class="auto-style2">
            &nbsp;<span class="style3">*</span>City&nbsp;</td>
        <td width="25%" class="auto-style2">
            <asp:DropDownList ID="cmbcity" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
        </td>
        <td width="15%" class="auto-style2">
            <span class="style3">*</span>Pin</td>
        <td width="25%" class="auto-style2">
            <asp:TextBox ID="txtPin" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td class="auto-style2">
             </td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;<span class="style3">*</span>Phone Number</td>
        <td width="25%">
            <asp:TextBox ID="txtPhone" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>



          <tr>
        <td>
            &nbsp;</td>
        <td colspan="4" style=" padding:3px 3px 3px 3px; text-align:center; font-weight:bold; background-color:gray; color:white;">Registered Office Details (If Different from Corporate Office)</td>
        <td>
            &nbsp;</td>
    </tr>
          <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;Registered Office Address&nbsp;</td>
        <td width="25%">
            <asp:TextBox ID="txtRegAddress" runat="server" CssClass="textbox_style" TextMode="MultiLine" Width="250px"></asp:TextBox>
        </td>
        <td width="15%">
            State&nbsp;&nbsp;</td>
        <td width="25%">
            <asp:DropDownList ID="ddlRegState" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
              </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;City&nbsp;</td>
        <td width="25%">
            <asp:DropDownList ID="ddlRegCity" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
        </td>
        <td width="15%">
            Pin</td>
        <td width="25%">
            <asp:TextBox ID="txtRegPin" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;Phone Number</td>
        <td width="25%">
            <asp:TextBox ID="txtRegPhno" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>





         <tr>
        <td>
            &nbsp;</td>
        <td colspan="4" style=" padding:3px 3px 3px 3px; text-align:center; font-weight:bold; background-color:gray; color:white;">Client Details</td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Company <span>Website</span></td>
        <td width="25%">
            <asp:TextBox ID="txtWebsite" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td width="15%">
            <span>Company Email ID</span></td>
        <td width="25%">
            <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            GST No</td>
        <td width="25%">
            <asp:TextBox ID="txtservicetax_no" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td width="15%">
            Pan No</td>
        <td width="25%">
            <asp:TextBox ID="txtpanno" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Industry Type</td>
        <td width="25%">
            <asp:DropDownList ID="cmbIndustry" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
        </td>
        <td width="15%">
            <span>Fax Number</span></td>
        <td width="25%">
            <asp:TextBox ID="txtFax" runat="server" CssClass="textbox_style" Width="250px"></asp:TextBox>
             </td>
        <td>
            &nbsp;</td>
    </tr>

         <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Place Of Supply</td>
        <td width="25%">
            <asp:DropDownList ID="ddlplaceofSupply" runat="server" CssClass="dropdown_style" Width="250px">
            </asp:DropDownList>
        </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
         <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
         <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>

   <%-- <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;<span class="style3">*</span>Address 1</td>
        <td width="25%">
            <asp:TextBox ID="txtAddress1" runat="server" CssClass="textbox_style" 
                Width="250px"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>--%>
   
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            </td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            </td>
        <td width="25%">
            
        </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    
 <%--   <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Client GST No</td>
        <td width="25%">
            <asp:TextBox ID="txtGstNo" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>--%>
    
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
   <%-- <tr>
        <td>
            &nbsp;</td>
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
        <td>
            &nbsp;</td>
    </tr>--%>
    <%--<tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Phone No.</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresentativePhone" runat="server" CssClass="textbox_style" onkeypress="return validate(event)"></asp:TextBox>
        </td>
        <td width="15%">
            Email</td>
        <td width="25%">
            <asp:TextBox ID="txtRepresentativeEmail" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td>
            &nbsp;</td>
    </tr>--%>
   <%-- <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>--%>
    <%--<tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            Vat No</td>
        <td width="25%">
            <asp:TextBox ID="txtvatno" runat="server" CssClass="textbox_style"></asp:TextBox>
        </td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>--%>
    <tr>
        <td>
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
        <td width="25%">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="4" class="style4">
            <asp:Button ID="btnSave" runat="server" CssClass="btn_style" 
                onclick="btnSave_Click" Text="Save" onclientclick="return ValidateField();"/>
&nbsp;
            <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn_style" OnClick="btnReset_Click" />
        </td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td colspan="2">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
</table>
</asp:Content>
