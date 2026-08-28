<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="NewUpdate_product.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm70" %>

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
    </style>

    <script type="text/javascript">
        <%--function ValidateField() {
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
                <%--{ id: '<%=cmdProduct.ClientID%>', message: "Select Product Category", isDropdown: true },--%>
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
    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <table cellpadding="0" cellspacing="1" class="style1">
        <tr>
            <td bgcolor="#19658A" colspan="4">&nbsp;<span class="style2">New Products Update</span></td>
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
            <td>
                <asp:Label ID="lblid" runat="server" Visible="False"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>

        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; PRODUCT / SERVICE CATAGORY</td>
            <td>
                <asp:Label ID="lblproductname" runat="server"></asp:Label>
            </td>
            <td>&nbsp;</td>
        </tr>

        <%--<tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;PRODUCT / SERVICE</td>
            <td>
                <asp:DropDownList ID="ddlProOrSer" runat="server" CssClass="dropdown_style" Width="300px" Enabled="false">
                    <asp:ListItem>Product</asp:ListItem>
                    <asp:ListItem>Service</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; PRODUCT / SERVICE SPECIFICATIONS&nbsp;</td>
            <td>
                <asp:TextBox ID="txtSubProdName" runat="server" CssClass="textbox_U_style" Enabled="False"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;EXTRA SPECIFICATIONS</td>
            <td>
                <asp:TextBox ID="txtBrand" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;HSN / SAC CODE</td>
            <td>
                <asp:TextBox ID="txtProductCode" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp;&nbsp;UNIT</td>
            <td>
                <asp:TextBox ID="txtUnit" runat="server" CssClass="textbox_U_style" Width="300px" Enabled="false"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; BASE RATE (RS)</td>
            <td>
                <asp:TextBox ID="txtSalerate" runat="server" CssClass="textbox_U_style" Enabled="False"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;&nbsp;&nbsp; Tax Rate (%)</td>
            <td>
                <asp:DropDownList ID="cmbtax" runat="server" CssClass="dropdown_style" Enabled="False">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>--%>

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
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label18" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE NAME&nbsp;</td>
            <td>
                <asp:TextBox ID="txtSubProductsName" runat="server" CssClass="textbox_U_style" Width="300px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;<asp:Label ID="Label19" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;PRODUCT / SERVICE Specifications'&nbsp;</td>
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
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2" style="text-align: center">
                <asp:Button ID="btnSave" runat="server" CssClass="btn_style" Text="Update"
                    OnClientClick="return ValidateField();" OnClick="btnSave_Click" Visible="False" />
                &nbsp;<asp:Button ID="btnedit" runat="server" CssClass="btn_style" Text="Edit" OnClick="btnedit_Click" />
                &nbsp;<asp:Button ID="btnback" runat="server" CssClass="btn_style" Text="Back" OnClick="btnback_Click" />
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
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
