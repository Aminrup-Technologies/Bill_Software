<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Create_quotation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm19" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
        .style2
       {
          color: #FFFFFF;
          font-weight: bold;
        }
        .Grid td
        {
            
            text-align:center;
            font-size: 10px;
            line-height:200%;
			border-color:#2D2D2D;
            border-width:1px;
            border-style: solid;
        }

        .center {
          text-align:center;
        }
    </style>
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
            changeMonth: true,
            changeYear: true
        });
    });
	</script>

    <script type="text/javascript">
        function ValidateField() {
            if (document.getElementById('<%=cmbClient.ClientID%>').selectedIndex == 0) {
                alert("Please Select Client.");
                document.getElementById('<%=cmbClient.ClientID%>').focus();
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

    function validate1(key) {
        //getting key code of pressed key
        var keycode = (key.which) ? key.which : key.keyCode;
        var phn = document.getElementById('txtfillrequar');
        //comparing pressed keycodes
        if ((keycode == 39)) {
            return false;
        }
        else {
            return true;
            
        }

    }
</script>
<script type="text/javascript">
    function ValidateDataField10() {

        if (document.getElementById('<%=cmbproduct_service.ClientID%>').selectedIndex == 0) {
            alert("Please Select Product OR Service.");
            document.getElementById('<%=cmbproduct_service.ClientID%>').focus();
                return false;
        }
        if (document.getElementById('<%=ddlPlaceOfSupply.ClientID%>').selectedIndex == 0) {
            alert("Please Select Place Of Supply.");
            document.getElementById('<%=cmbproduct_service.ClientID%>').focus();
                return false;
        }

        
    }

</script>


<script type = "text/javascript">

        function Check_Click(objRef) {

            //Get the Row based on checkbox

            var row = objRef.parentNode.parentNode;

            if (objRef.checked) {

                //If checked change color to Aqua

                row.style.backgroundColor = "#84e26e";

            }

            else {

                //If not checked change back to original color

                if (row.rowIndex % 2 == 0) {

                    //Alternating Row Color

                    row.style.backgroundColor = "#C2D69B";

                }

                else {

                    row.style.backgroundColor = "white";

                }

            }



            //Get the reference of GridView

            var GridView = row.parentNode;



            //Get all input elements in Gridview

            var inputList = GridView.getElementsByTagName("input");



            for (var i = 0; i < inputList.length; i++) {

                //The First element is the Header Checkbox

                var headerCheckBox = inputList[0];



                //Based on all or none checkboxes

                //are checked check/uncheck Header Checkbox

                var checked = true;

                if (inputList[i].type == "checkbox" && inputList[i] != headerCheckBox) {

                    if (!inputList[i].checked) {

                        checked = false;

                        break;

                    }

                }

            }

            headerCheckBox.checked = checked;



        }

</script>
<script type = "text/javascript">

    function checkAll(objRef) {

        var GridView = objRef.parentNode.parentNode.parentNode;

        var inputList = GridView.getElementsByTagName("input");

        for (var i = 0; i < inputList.length; i++) {

            //Get the Cell To find out ColumnIndex

            var row = inputList[i].parentNode.parentNode;

            if (inputList[i].type == "checkbox" && objRef != inputList[i]) {

                if (objRef.checked) {

                    //If the header checkbox is checked

                    //check all checkboxes

                    //and highlight all rows

                    row.style.backgroundColor = "#84e26e";

                    inputList[i].checked = true;

                }

                else {

                    //If the header checkbox is checked

                    //uncheck all checkboxes

                    //and change rowcolor back to original

                    if (row.rowIndex % 2 == 0) {

                        //Alternating Row Color

                        row.style.backgroundColor = "#C2D69B";

                    }

                    else {

                        row.style.backgroundColor = "white";

                    }

                    inputList[i].checked = false;

                }

            }

        }

    }

</script> 



    <%--<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>--%>
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A"><span class="style2">&nbsp;Create Quotation</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">
                <asp:Label ID="lblclientID" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                <asp:Label ID="lblqno" runat="server" Visible="False"></asp:Label>
            </td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">
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
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Client </td>
            <td>
                <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style">
                </asp:DropDownList>
            </td>
            <td>
                <asp:Label ID="Label1" runat="server" Text="1" Visible="False"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Quotation Date&nbsp;</td>
            <td>
                            <asp:TextBox ID="txtquotationDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
            <td>Quotation Type</td>
            <td>
                <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">New</asp:ListItem>
                    <asp:ListItem>Old</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Sale Type&nbsp;</td>
            <td>
                <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">Product / Service</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>&nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Place Of Supply</td>
            <td>
                <asp:DropDownList ID="ddlPlaceOfSupply" runat="server" CssClass="dropdown_style">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Select CGST/SGST for Intra-State OR IGST for Inter-State</td>
            <td><asp:Panel ID="panelGst" runat="server">
                   <%-- <asp:CheckBox ID="CHKCGSTSGST" runat="server" Enabled="true" Text="CGST/SGST" />
                    &nbsp;<asp:CheckBox ID="CHKIGST" runat="server" Text="IGST" />--%>

                <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal">
                     <asp:ListItem Value="1" Selected="True"> CGST/SGST </asp:ListItem>
                     <asp:ListItem Value="0"> IGST </asp:ListItem> 
                </asp:RadioButtonList>

                </asp:Panel></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2" style="text-align: center">
                <asp:Button ID="Button1" runat="server" Text="Click to Retrieve Product/Service Category" CssClass="btn_style" onclientclick="return ValidateField();" OnClick="Button1_Click" Width="300px"/>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">
                <asp:Panel ID="Panel1" runat="server" Visible="False">
                    <table cellpadding="0" cellspacing="0" class="auto-style1">
                        <tr>
                            <td width="50%">Select Product &/or Service Category One by One</td>
                            <td width="50%">
                                <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                       <%-- <tr>
                            <td width="50%">Factory Address</td>
                            <td width="50%">
                                <asp:ListBox ID="listOffactory" runat="server"></asp:ListBox>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>--%>

                        <tr>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" Text="Click to Retrieve Product &/or Service from the selected Category" onclientclick="return ValidateDataField10();" OnClick="Button2_Click" Width="400px"/>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        
        <tr>
            <td colspan="4">
                <asp:Panel ID="Panel2" runat="server" Visible="False">
                    <table cellpadding="0" cellspacing="0" class="auto-style1">
                        <%--<asp:Panel ID="Panel3" runat="server">--%>
                        <tr><td style="color:red; text-align:center;font-weight:bold;" colspan="4"><span style="font-weight:900; font-size:14px;">*</span>Click the Checkbox to Select the Desired Product/Service</td></tr>
                        <tr>
                            
                            <td colspan="4" >
                                <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Product/Service Code">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                         <asp:TemplateField HeaderText="Product/Service">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Name">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                         <asp:TemplateField HeaderText="Extra Specifications">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Unit">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                       

                                     <%--   <asp:TemplateField HeaderText="EXTRA SPECIFICATIONS">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="specification" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="250px" onkeypress="return validate1(event)"></asp:TextBox>
                                               </ItemTemplate>
                                        </asp:TemplateField>--%>
                                        
                                        <asp:TemplateField HeaderText="Base Rate (RS)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' onkeypress="return validate(event)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="GST Rate">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Tax_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Quantity">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Quantity" runat="server" onkeypress="return validate(event)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Select">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                                </EditItemTemplate>

                                                <HeaderTemplate>
                                                     <asp:CheckBox ID="checkAll" runat="server" onclick = "checkAll(this);" />
                                                </HeaderTemplate>

                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chkdtp" runat="server" onclick = "Check_Click(this)" />
                                                </ItemTemplate>
                                                <HeaderStyle Width="3%" />
                                                <ItemStyle Width="3%" />
                                        </asp:TemplateField>

                                    </Columns>
                                    <FooterStyle BackColor="#CCCC99" />
                                    <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                    <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                </asp:GridView>
                            </td>
                        </tr>
                         <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                            
                        <tr>
                            <td colspan="2" style="text-align:center">
                                <asp:Button ID="btnAddProduct" runat="server" CssClass="btn_style" Text="Add Required Product &/or Service  against the Selected Category from the above Table" OnClick="btnAddProduct_Click" Width="500px" />
                            </td>
                            <td colspan="2" style="text-align:center; color:red; font-weight:bold;">Go back to the Select Product/Service Category in case more Product/Service Categories need to be added</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                           <%-- </asp:Panel>--%>
                        <%--<asp:Panel ID="Panel4" runat="server" Visible="False">--%>
                        <tr>
                            <td style="color:red; text-align:center; font-weight:bold;" colspan="4"><span style="font-weight:bold; font-size:14px;">*</span>After Selection of the Desired Product Category/s, Change the Base Rate as required and add the required Quantity</td>
                        </tr>
                        <tr>
                            <td colspan="4">
                                <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Product/Service Code">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Name">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Extra Specifications">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>' onkeypress="return validate(event)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Unit">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                       <%-- <asp:TemplateField HeaderText="Specification">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="specification" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="250px" onkeypress="return validate1(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>
                                       <%-- <asp:TemplateField HeaderText="Tax Applicable">
                                            <ItemTemplate>
                                                <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal">
                                                    <asp:ListItem>Yes</asp:ListItem>
                                                    <asp:ListItem Selected="True">No</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Input %(VAT/SERVICE TAX)">
                                            <ItemTemplate>
                                                <asp:DropDownList ID="vat_parsentage" runat="server" CssClass="dropdown_style">
                                                </asp:DropDownList>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>
                                        
                                        <asp:TemplateField HeaderText="Base Rate (RS)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center" Height="22px"  onkeypress="return validate(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="GST Rate (%)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Tax_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Quantity">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Quantity" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center" Height="22px" onkeypress="return validate(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>



                                        <asp:TemplateField HeaderText="Select">
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                                </EditItemTemplate>

                                                <HeaderTemplate>
                                                     <asp:CheckBox ID="checkAll" runat="server" onclick = "checkAll(this);" />
                                                </HeaderTemplate>

                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chk" runat="server" onclick = "Check_Click(this)" />
                                                </ItemTemplate>
                                                <HeaderStyle Width="3%" />
                                                <ItemStyle Width="3%" />
                                            </asp:TemplateField>


                                    </Columns>
                                    <FooterStyle BackColor="#CCCC99" />
                                    <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                    <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                </asp:GridView>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                            <%--</asp:Panel>--%>
                       
                         <tr>
                             <td colspan="4">
                                 <table>
                                     <tr>
                                         <td width="20%" style="font-weight:bold;">Add Payment Phase & Payment %age<br />(Select Payment Phase One By One)</td>
                                         <td width="5%"></td>
                                         <td width="30%"><asp:ListBox ID="listPhaseType" runat="server" Font-Size="14px" multiple="true" SelectionMode="Multiple" Rows="7" Width="250px" BackColor="#94b8ff" OnTextChanged="listPhaseType_TextChanged" AutoPostBack="True"></asp:ListBox></td>
                                         <td width="5%"></td>
                                         <td width="40%"><asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" BorderWidth="1px" BackColor="#94b8ff" CellPadding="3" CellSpacing="2" BorderStyle="None" BorderColor="#DEBA84" OnRowDeleting="GridView3_RowDeleting">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Payment Phase">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                         <asp:TemplateField HeaderText="Phase Description">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="PhaseDesc" runat="server"  Text='<%# Bind("PhaseDesc") %>' TextMode="MultiLine"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:TextBox ID="PhaseDesc" runat="server"  Text='<%# Bind("PhaseDesc") %>' TextMode="MultiLine"></asp:TextBox>
                                        </ItemTemplate>
                                       
                                    </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Payment %age">
                                            <%--<EditItemTemplate>
                                                <asp:TextBox ID="AmountPer" runat="server"  Text=""></asp:TextBox>
                                            </EditItemTemplate>--%>
                                            <ItemTemplate>
                                                <asp:TextBox ID="AmountPer" runat="server" AutoPostBack="true" Text='<%# Bind("AmountPer") %>'  OnTextChanged="AmountPer_TextChanged"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:CommandField ButtonType="Button" HeaderText="Delete" ShowDeleteButton="True" />
                                    </Columns>
                                </asp:GridView></td>
                                     </tr>
                                 </table>
                             </td>
                        </tr>
                       
                         <%--<tr>
                            
                            <td style="font-weight:bold; border:1px;"></td>
                            <td colspan="2" style="border:1px;">
                                
                             </td>
                            <td style=""></td>
                        </tr>--%>

                         <tr>
                            <td>&nbsp;</td>
                            <td colspan="2">
                                
                             </td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>
                                &nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>
                                <asp:GridView ID="gridps" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Visible="false" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Service/ProductCatagory">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductCatagory" runat="server" Text='<%# Bind("ProductCatagory") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductCatagory" runat="server" Text='<%# Bind("ProductCatagory") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Select">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox9" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkp" runat="server" Checked="true" onclick="Check_Click(this)" />
                                            </ItemTemplate>
                                            <HeaderStyle Width="3%" />
                                            <ItemStyle Width="3%" />
                                        </asp:TemplateField>
                                    </Columns>
                                    <FooterStyle BackColor="#CCCC99" />
                                    <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                    <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                </asp:GridView>
                            </td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="Button3" runat="server" CssClass="btn_style" Text="Save" OnClick="Button3_Click" />
                            </td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                &nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>

        <%--</ContentTemplate>
         <Triggers>
 
                <asp:PostBackTrigger ControlID="Button1"  />
                <asp:PostBackTrigger ControlID="Button2" />
                <asp:PostBackTrigger ControlID="Button3" />
              <asp:PostBackTrigger ControlID="GridView3" />
             
            </Triggers>
    </asp:UpdatePanel>--%>
</asp:Content>
