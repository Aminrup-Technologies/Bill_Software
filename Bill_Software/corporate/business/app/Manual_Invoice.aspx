<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Manual_Invoice.aspx.cs" Inherits="Bill_Software.corporate.business.app.Manual_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
        }

        .center {
            text-align: center;
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

        function validateRowSelection(gridViewId) {
            console.log("GridView ID being passed: " + gridViewId);

            var gridView = document.getElementById('ContentPlaceHolder1_Panel2');
            if (!gridView) {
                console.log("GridView not found. Exiting validation.");
                alert("GridView not found.");
                return false;
            }

            console.log("GridView found. Proceeding with validation.");

            // Get all the rows in the GridView (excluding header)
            var rows = gridView.getElementsByTagName('tr');
            var isRowSelected = false;

            // Loop through each row to check if any row is selected
            for (var i = 1; i < rows.length; i++) {  // Skipping the first row (header)
                var checkBox = rows[i].querySelector("input[type='checkbox']");

                if (checkBox && checkBox.checked) {
                    isRowSelected = true;
                    break;  // Exit the loop if at least one row is selected
                }
            }

            // Log the result of the row selection
            if (isRowSelected) {
                console.log("At least one row is selected.");
                return true;
            } else {
                console.log("No row is selected.");
                alert("Please select at least one row.");
                return false;
            }
        }


        function validate(key, element) {
            var keycode = (key.which) ? key.which : key.keyCode;

            // Reference the element that triggered the event
            var phn = element;

            // Allow numbers (0-9), backspace (8), and delete (46)
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
                return false;
            } else {
                return phn.value.length < 50;
            }
        }

        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;
            console.log("row", row);
            if (objRef.checked) {
                row.style.backgroundColor = "#84e26e";
            }

            else {
                if (row.rowIndex % 2 == 0) {
                    row.style.backgroundColor = "#C2D69B";
                }
                else {
                    row.style.backgroundColor = "white";
                }
            }

            var GridView = row.parentNode;
            console.log("GridView", GridView);
            var inputList = GridView.getElementsByTagName("input");
            console.log("GridView-inputList", inputList);
            for (var i = 0; i < inputList.length; i++) {
                var headerCheckBox = inputList[0];
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

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            console.log("GridView", GridView);
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {
                        row.style.backgroundColor = "#84e26e";
                        inputList[i].checked = true;
                    }

                    else {
                        if (row.rowIndex % 2 == 0) {
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


    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>--%>
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Add Direct TAX Invoice</span>>&nbsp;</td>
        </tr>
        <tr>
            <td width="10%">&nbsp;</td>
            <td width="40%" colspan="2">
                <asp:Label ID="lblclientID" runat="server" Visible="False"></asp:Label>
            </td>
            <td width="40%" colspan="2">&nbsp;</td>
            <td width="10%">&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
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
            <td colspan="4">
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
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
            <td colspan="2">&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select Client / Customer Name</td>
            <td colspan="2">
                <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style" Width="250px" AutoPostBack="false"></asp:DropDownList></td>
            <td>
                <asp:Label ID="Label1" runat="server" Text="1" Visible="False"></asp:Label></td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4" style="text-align: center">&nbsp;<asp:Button ID="Button1" runat="server" Text="Click to Retrieve Client Addresses" CssClass="btn_style" OnClick="Button1_Click" Width="200px" /></td>
            <td>&nbsp;</td>
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
            <td colspan="2">&nbsp;<asp:Label ID="Label4" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select Client / Customer Billing Address</td>
            <td colspan="2">
                <asp:ListBox ID="FactoryAddress" runat="server" AutoPostBack="True" BorderStyle="Solid" BorderWidth="1px" Font-Size="10px" multiple="true" Rows="3" SelectionMode="Multiple" Width="100%"></asp:ListBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;<asp:Label ID="Label2" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select TAX Invoice Date</td>
            <td colspan="2">
                <asp:TextBox ID="txtinvoiceDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker dropdown_style" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;<asp:Label ID="Label3" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select CGST/SGST for Intra-State OR IGST for Inter-State</td>
            <td colspan="2">
                <asp:Panel ID="panelGst" runat="server">
                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Value="1"> CGST/SGST </asp:ListItem>
                        <asp:ListItem Value="0"> IGST </asp:ListItem>
                    </asp:RadioButtonList>

                </asp:Panel>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td colspan="4" style="text-align: center">&nbsp;<asp:Button ID="Button2" runat="server" Text="Click to Retrieve Product/Service Category" CssClass="btn_style" OnClick="Button2_Click" Width="280px" /></td>
            <td>&nbsp;</td>
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
            <td colspan="2">&nbsp;<asp:Label ID="Label5" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select Product &/or Service Category One by One</td>
            <td colspan="2">&nbsp;<asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style" Width="250px"></asp:DropDownList></td>
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
            <td colspan="4" style="text-align: center">&nbsp;<asp:Button ID="Button3" runat="server" Text="Click to Retrieve Product &/or Service from the selected Category" CssClass="btn_style" OnClick="Button3_Click" Width="400px" /></td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="6">
                <asp:Panel ID="Panel2" runat="server" Visible="true">
                    <table cellpadding="0" cellspacing="0" class="auto-style1">
                        <tr>
                            <td style="color: red; text-align: center; font-weight: bold;" colspan="4"><span style="font-weight: 900; font-size: 14px;">*</span>Click the Checkbox to Select the Desired Product/Service</td>
                        </tr>
                        <tr>

                            <td colspan="4">
                                <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>


                                        <asp:TemplateField HeaderText="Product ID">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service" Visible="false">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category" Visible="false">
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

                                        <asp:TemplateField HeaderText="HSN CODE">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
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

                                        <asp:TemplateField HeaderText="Base Rate (RS)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' onkeypress="return validate(event, this)"></asp:Label>
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

                                        <asp:TemplateField HeaderText="Stock Quantity" HeaderStyle-BackColor="Green" ItemStyle-BackColor="LightGreen">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Quantity" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Select">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>

                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>

                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" />
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
                            <td colspan="2" style="text-align: center; color: red; font-weight: bold;">Go back to the Select Product/Service Category in case more Product/Service Categories need to be added</td>
                            <td colspan="2" style="text-align: center">
                                <asp:Button ID="btnAddProduct" runat="server" CssClass="btn_style" Text="Add Required Product &/or Service against the Selected Category from the above Table" OnClientClick="return validateRowSelection('<%= gridProdWithCat.ClientID %>');" Width="500px" OnClick="btnAddProduct_Click" />
                            </td>

                        </tr>

                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>

                        <tr>
                            <td colspan="4">
                                <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="HSN CODE" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product ID" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service" Visible="false" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Category" Visible="false" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Product/Service Name" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Extra Specifications" HeaderStyle-Width="15%" ItemStyle-Width="15%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>' onkeypress="return validate(event, this)"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Unit" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Item No" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="ItemNo" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Material No" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="MaterialNo" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Pack Size" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="PackSize" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Base Rate (RS)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Width="80%" CssClass="center textbox_style" Height="22px" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="GST Rate (%)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Tax_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Discount (%)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Discount_Rate" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Text="0" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Stock Quantity" HeaderStyle-BackColor="Green" ItemStyle-BackColor="LightGreen">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="SQuantity" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="SQuantity" runat="server" Text='<%# Bind("SQuantity") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Invoice Quantity" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="IQuantity" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="IQuantity" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Remarks" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="ItemRemarks" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Select" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                            </EditItemTemplate>

                                            <HeaderTemplate>
                                                <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                            </HeaderTemplate>

                                            <ItemTemplate>
                                                <asp:CheckBox ID="chk" runat="server" onclick="Check_Click(this)" />
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
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;<asp:Label ID="Label6" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select CGST/SGST for Intra-State OR IGST for Inter-State</td>
                            <td>&nbsp;<asp:TextBox ID="txt_tcs_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox>&nbsp;@&nbsp;<asp:TextBox ID="txt_tcs_percent" runat="server" CssClass="textbox_U_style" Width="50px" Text=""></asp:TextBox>&nbsp;%</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;Freight Charges</td>
                            <td>&nbsp;<asp:TextBox ID="txt_delivery_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox>&nbsp;@&nbsp;
                                        <asp:DropDownList ID="DDL_vat_parsentage" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList>
                                        %</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td style="text-align:right;">&nbsp;Other Charges&nbsp;<asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style" Width="110px" Text=""></asp:TextBox></td>
                            <td>&nbsp;<asp:TextBox ID="txt_othr_amnt" runat="server" CssClass="textbox_U_style" Width="110px" Text="0"></asp:TextBox></td>
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
                                <asp:Button ID="btn_finalsave" runat="server" CssClass="btn_style" Width="150px" Text="Create TAX Invoice" OnClick="btn_finalsave_Click"/>
                            </td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
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
    <%--</ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="Button1" />
        </Triggers>
    </asp:UpdatePanel>--%>
</asp:Content>
