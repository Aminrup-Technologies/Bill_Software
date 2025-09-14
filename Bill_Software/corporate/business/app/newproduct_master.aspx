<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="newproduct_master.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm69" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .table1 {
            border-collapse: collapse;
        }

            .table1 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
            }

        .table2 {
            border-collapse: collapse;
        }

            .table2 td {
                text-align: left;
                border: 1px solid #666666;
                width: 100%;
                border-top: none;
            }

        .auto-style1 {
            height: 19px;
        }
    </style>
    <script type="text/javascript">
        <%--function ValidateField() {
            if (document.getElementById('<%=txtSubProductsName.ClientID%>').value == "") {
                alert("Provide Products Name.");
                document.getElementById('<%=txtSubProductsName.ClientID%>').focus();
                return false;
            }

            if (document.getElementById('<%=txtSalerate.ClientID%>').value == "") {
                alert("Provide Sale Rate.");
                document.getElementById('<%=txtSalerate.ClientID%>').focus();
                return false;
            }
            if (document.getElementById('<%=cmbtax.ClientID%>').selectedIndex == 0) {
                alert("Please Select Tax.");
                document.getElementById('<%=cmbtax.ClientID%>').focus();
                return false;
            }
        }--%>

        function ValidateField() {
            // Array of required field IDs and their corresponding alert messages
            var fields = [
                { id: '<%=cmdProduct.ClientID%>', message: "Select Product Category", isDropdown: true },
                { id: '<%=ddlProOrSer.ClientID%>', message: "Select Business Type", isDropdown: true },
                { id: '<%=txtSubProductsName.ClientID%>', message: "Provide Product Name" },
                { id: '<%=txtproducttype.ClientID%>', message: "Provide Product Specifications" },
                <%--{ id: '<%=TextBox1.ClientID%>', message: "Provide value for Extra Specifications." },--%>
                { id: '<%=txtBrand.ClientID%>', message: "Provide Brand Name" },
                { id: '<%=txtProductCode.ClientID%>', message: "Provide HSN Code" },
                { id: '<%=txtUnit.ClientID%>', message: "Provide UOM" },
                { id: '<%=TextBox2.ClientID%>', message: "Provide Opening Stock Value" },
                <%--{ id: '<%=TextBox3.ClientID%>', message: "Provide value for TextBox3." },--%>
                { id: '<%=txtSalerate.ClientID%>', message: "Provide Sale Rate." },
                { id: '<%=cmbtax.ClientID%>', message: "Please Select Tax Slab", isDropdown: true },
                { id: '<%=txtfromDate.ClientID%>', message: "Provide Expiry Date." },
                { id: '<%=TextBox4.ClientID%>', message: "Provide Sales Note" }
            ];

            for (var i = 0; i < fields.length; i++) {
                var field = document.getElementById(fields[i].id);

                if (field) {
                    if (fields[i].isDropdown) {
                        if (field.selectedIndex === 0) {
                            alert(fields[i].message);
                            field.focus();
                            return false;
                        }
                    } else {
                        if (field.value.trim() === "") {
                            alert(fields[i].message);
                            field.focus();
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        function ResetFields() {
            var fields = [
                '<%=txtSubProductsName.ClientID%>',
                '<%=txtproducttype.ClientID%>',
                '<%=TextBox1.ClientID%>',
                '<%=txtBrand.ClientID%>',
                '<%=txtProductCode.ClientID%>',
                '<%=txtUnit.ClientID%>',
                '<%=TextBox2.ClientID%>',
                '<%=TextBox3.ClientID%>',
                '<%=txtSalerate.ClientID%>',
                '<%=txtfromDate.ClientID%>',
                '<%=TextBox4.ClientID%>'
            ];

            // Reset textboxes
            for (var i = 0; i < fields.length; i++) {
                var field = document.getElementById(fields[i]);
                if (field) {
                    field.value = "";
                }
            }

            // Reset dropdowns
            var dropdowns = [
                '<%=cmdProduct.ClientID%>',
                '<%=ddlProOrSer.ClientID%>',
                '<%=cmbtax.ClientID%>'
            ];

            for (var j = 0; j < dropdowns.length; j++) {
                var dropdown = document.getElementById(dropdowns[j]);
                if (dropdown) {
                    dropdown.selectedIndex = 0;
                }
            }

            return false; // Prevents any unintended form submission
        }


    </script>
    <script type="text/javascript">
        function ValidateDelete1() {
            var answer = confirm("Want to Delete this Products?");
            if (!answer) {
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

        function checkDuplicates() {
            var productName = $("#<%= txtSubProductsName.ClientID %>").val().trim();
            var category = $("#<%= cmdProduct.ClientID %>").val();

            $("#<%= lblDupMessage.ClientID %>").text("");
            $("#<%= lblSimilar.ClientID %>").text("");

            if (!productName) {
                $("#<%= lblDupMessage.ClientID %>").text("Please enter a product name to check.");
                return;
            }

            // Call the server-side WebMethod
            PageMethods.GetDuplicateInfo(productName, category,
                function(result) {
                    // result is the object returned by your WebMethod
                    if (result.foundExact) {
                        var msg = "Exact product exists: Id=" + result.existingId;
                        if (result.productID) msg += " (ProductID: " + result.productID + ")";
                        $("#<%= lblDupMessage.ClientID %>").text(msg);
                    } else {
                        $("#<%= lblDupMessage.ClientID %>").text("No exact match found. You may proceed.");
                    }

                    if (result.similar && result.similar.length > 0) {
                        $("#<%= lblSimilar.ClientID %>").text("Similar products: " + result.similar.join(" | "));
                    }
                },
                function(err) {
                    $("#<%= lblDupMessage.ClientID %>").text("Error: " + err.get_message());
                }
            );
        }
    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    </asp:ScriptManager>
    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">New Products Details</span></td>
        </tr>
        <tr>
            <td width="20%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="30%">&nbsp;</td>
            <td width="20%">&nbsp;</td>
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

                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300"
                    BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px"
                        ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png"
                        Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
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
            <td>&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE CATAGORY</td>
            <td>
                <asp:DropDownList ID="cmdProduct" runat="server" CssClass="dropdown_style" Width="300px" AutoPostBack="True" OnSelectedIndexChanged="cmdProduct_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label17" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;BUSINESS TYPE&nbsp;</td>
            <td>
                <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style" Width="300px">
                    <asp:ListItem>--Select--</asp:ListItem>
                    <asp:ListItem>Product</asp:ListItem>
                    <asp:ListItem>Service</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label18" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE NAME&nbsp;</td>
            <td>
                <asp:TextBox ID="txtSubProductsName" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
                &nbsp;<asp:Label ID="lblSimilar" runat="server" ForeColor="Gray"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label18" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE NAME&nbsp;</td>
            <td>
                <asp:TextBox ID="txtSubProductsName" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
                &nbsp;
                <!-- Check duplicates button -->
                <asp:Button ID="btnCheckDup" runat="server" Text="Check Duplicates" OnClientClick="checkDuplicates(); return false;" CssClass="btn btn_style" />

                <!-- show results -->
                <div id="dupResultPanel" style="margin-top: 6px;">
                    <asp:Label ID="lblDupMessage" runat="server" ForeColor="Crimson" />
                    <br />
                    <asp:Label ID="lblSimilar" runat="server" ForeColor="Gray" />
                </div>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<%--<asp:Label ID="Label19" runat="server" Text="*" ForeColor="Red"></asp:Label>--%>&nbsp; PRODUCT / SERVICE Specifications'&nbsp;</td>
            <td>
                <asp:TextBox ID="txtproducttype" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp; PRODUCT / SERVICE Extra Specifications'&nbsp;</td>
            <td>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label21" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;MAKE / BRAND NAME &nbsp;</td>
            <td>
                <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox></td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label22" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;HSN / SAC CODE</td>
            <td>
                <asp:TextBox ID="txtProductCode" runat="server" CssClass="textbox_U_style" Width="300px" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label23" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;UOM</td>
            <td>
                <asp:TextBox ID="txtUnit" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label20" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;OPENING / STOCK QNTY'</td>
            <td>
                <asp:TextBox ID="TextBox2" runat="server" CssClass="textbox_U_style" Width="300px" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp; MOQ VALUE'&nbsp;</td>
            <td>
                <asp:TextBox ID="TextBox3" runat="server" CssClass="textbox_U_style" Width="300px" onkeypress="return validate(event)"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label24" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;BASE RATE (RS)</td>
            <td>
                <asp:TextBox ID="txtSalerate" runat="server" CssClass="textbox_U_style" onkeypress="return validate(event)" Width="300px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label25" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;GST RATE (%)</td>
            <td>
                <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style" Width="300px">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>


        <tr>
            <td class="auto-style1"></td>
            <td class="auto-style1">&nbsp;&nbsp; Expiry Date'&nbsp;</td>
            <td class="auto-style1">
                <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
            </td>
            <td class="auto-style1"></td>
        </tr>
        <tr>
            <td class="auto-style1"></td>
            <td class="auto-style1">&nbsp;&nbsp; Sale Note'&nbsp;</td>
            <td class="auto-style1">
                <asp:TextBox ID="TextBox4" runat="server" CssClass="textbox_U_style" Width="300px" Text="N/A"></asp:TextBox>
            </td>
            <td class="auto-style1"></td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp; </td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <%--<tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Tax Rate</td>
            <td>
                <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style" Width="300px">
                </asp:DropDownList>
            </td>
            <td>
                &nbsp;</td>
        </tr>--%>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2" style="text-align: center">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Save"
                    OnClientClick="return ValidateField();" OnClick="btnSave_Click" />
                &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClientClick="return ResetFields();" Text="Reset" />
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
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666"
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="10px"
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%"
                    OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align: center; width: 5%;">
                                    <asp:Label ID="showid" runat="server" Text="ID"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label12" runat="server" Text="PRODUCT / SERVICE"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">
                                    <asp:Label ID="showrm" runat="server" Text="PRODUCT / SERVICE CATEGORY"></asp:Label>
                                </td>

                                <td style="text-align: center; width: 15%;">
                                    <asp:Label ID="Label5" runat="server" Text="PRODUCT / SERVICE NAME"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 18%;">
                                    <asp:Label ID="Label14" runat="server" Text="PRODUCT / SERVICE TYPE"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label11" runat="server" Text="MAKE / BRAND"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 7%;">
                                    <asp:Label ID="Label1" runat="server" Text="HSN / SAC CODE"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label6" runat="server" Text="BASE RATE (RS)"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:Label ID="Label8" runat="server" Text="GST RATE (%)"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 4%;">
                                    <asp:Label ID="Label9" runat="server" Text="Edit"></asp:Label>
                                </td>
                                <td style="text-align: center; width: 3%;">
                                    <asp:Label ID="edit" runat="server" Text="Delete"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align: center; width: 5%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Id") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label2" runat="server" Text='<%# Eval("Type") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 12%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("ProductOrServiceCat") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 15%;">
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("ProductName") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 18%;">
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Product_catagory") %>'></asp:Label>
                                </td>

                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Brand") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 7%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Product_code") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 10%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Sail_Rate") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 6%;">
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("Tax_Rate") %>'></asp:Label>
                                </td>
                                <td style="text-align: center; width: 4%;">
                                    <asp:ImageButton ID="ImageButton3" runat="server" CommandName="Edit" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/edit1.png" ToolTip="Edit" />
                                </td>
                                <td style="text-align: center; width: 3%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" OnClientClick="return ValidateDelete1();" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>

                </asp:DataList>

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
    </table>
</asp:Content>
