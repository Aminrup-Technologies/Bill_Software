<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Edit_quatation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm65" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
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

        .auto-style2 {
            height: 19px;
        }

        .auto-style3 {
            width: 70%;
        }

        .auto-style4 {
            width: 30%;
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

        function ValidateDelete1() {
            var answer = confirm("Want to Delete this Quotation?");
            if (!answer) {
                return false;
            }
        }

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
    <script type="text/javascript">

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

        function toggleReferenceFields(value) {
            document.getElementById('<%= hdnRefOption.ClientID %>').value = value;

            var nameField = document.getElementById('<%= txt_clientrefname.ClientID %>');
            var idField = document.getElementById('<%= txt_clientrefid.ClientID %>');
            var dateField = document.getElementById('<%= txt_clientrefdate.ClientID %>');

            if (value === 'Yes') {
                nameField.readOnly = false;
                idField.readOnly = false;
                dateField.readOnly = false;
                nameField.value = "";
                idField.value = "";
                dateField.value = "";
            } else {
                nameField.value = "N/A";
                idField.value = "N/A";
                dateField.value = "01-Jan-2000";
                nameField.readOnly = true;
                idField.readOnly = true;
                dateField.readOnly = true;
            }
        }

        function togglePanel() {
            var rbQt = document.getElementById('<%= rbQt.ClientID %>');
            var panel = document.getElementById('<%= PO_DataInputs.ClientID %>');
            var poFields = document.querySelectorAll('.po-mandatory');

            if (rbQt.checked) {
                panel.style.display = 'none';
                poFields.forEach(function (field) {
                    field.removeAttribute('required');
                });
            } else {
                panel.style.display = 'block';
                poFields.forEach(function (field) {
                    field.setAttribute('required', 'required');
                });
            }
        }

        function handlePackageForwardingChange(dropdown) {
            var selectedValue = dropdown.value;
            var manualInputPkgRow = document.getElementById("manualInputPkgRow");

            if (selectedValue == "3") { // Manual Input selected
                manualInputPkgRow.style.display = "table-row"; // Show the textbox row
            } else {
                manualInputPkgRow.style.display = "none"; // Hide the textbox row
            }
        }

        function handleDeliveryTermChange(dropdown) {
            var selectedValue = dropdown.value;
            var manualInputRow = document.getElementById("manualInputRow");

            if (selectedValue == "4") {
                manualInputRow.style.display = "table-row";
            } else {
                manualInputRow.style.display = "none";
            }
        }

        function checkAllOnLoad() {
            var GridView = document.getElementById('<%= gd_Service_Product.ClientID %>');
            if (!GridView) return;

            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                if (inputList[i].type == "checkbox") {
                    inputList[i].checked = true;
                    var row = inputList[i].parentNode.parentNode;
                    row.style.backgroundColor = "#84e26e";
                }
            }
        }

        window.onload = function () {
            //togglePanel();
            checkAllOnLoad();
        };

    </script>

    <asp:HiddenField ID="hdnRefOption" runat="server" Value="No" />

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Edit Quotation</span></td>
                </tr>
                <tr>
                    <td width="15%">&nbsp;</td>
                    <td width="35%" colspan="2">
                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
                    </td>
                    <td width="35%" colspan="2">&nbsp;</td>
                    <td width="15%">&nbsp;</td>
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
                    <td colspan="4">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;Client Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date(Quotataion)</td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>To Date(Quotation)</td>
                    <td>
                        <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Search Type</td>
                    <td colspan="2">
                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem>Only Client</asp:ListItem>
                            <asp:ListItem Selected="True">Only Date</asp:ListItem>
                            <asp:ListItem>Client &amp; Date</asp:ListItem>
                        </asp:RadioButtonList>
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
                    <td colspan="4" style="text-align: center">
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" />
                        &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />
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
                <tr id="SelectorGridRow" runat="server" visible="true">
                    <td colspan="6">
                        <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:27%;">
                                    <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showrm0" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="showid0" runat="server" Text="Quotation no"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:15%;"> 
                                    <asp:Label ID="Label12" runat="server" Text="Net Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit0" runat="server" Text="View"></asp:Label>
                                </td>
                               <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label1" runat="server" Text="Edit"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:27%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="addshowname0" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:18%;">
                                    <asp:Label ID="ID0" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:15%;">Rs. 
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <a href = "#" title="Print Quotation..." onclick="window.open('/corporate/business/print/NewQuotation.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                </td>
                                <td style="text-align:center; width:10%;">
                                   <asp:ImageButton ID="ImageButton1" runat="server" 
                                            CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select"  
                                            ImageUrl="~/corporate/business/WebImages/tick-icon.png" 
                                             ToolTip="Select" />
                                </td>
                                
                                
                                
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>--%>

                        <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#94B8FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <HeaderTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 25%;">
                                            <asp:Label ID="Label2" runat="server" Text="Client Name"></asp:Label>
                                        </td>

                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="showid" runat="server" Text="Quotation Number"></asp:Label>
                                        </td>

                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label6" runat="server" Text="Product Catagory"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label7" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label9" runat="server" Text="GST (INR)"></asp:Label>
                                        </td>


                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="Label1" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                        </td>

                                        <%--<td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label5" runat="server" Text="Last Mailer Date"></asp:Label>
                                </td>--%>

                                        <td style="text-align: center; width: 5%;">
                                            <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 4%;">
                                            <asp:Label ID="Label13" runat="server" Text="Edit"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 25%;">
                                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                        </td>


                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label10" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label11" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label12" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                        </td>


                                        <td style="text-align: center; width: 8%;">Rs. 
                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                            /-
                                        </td>

                                        <%--<td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("mailStatusDate") %>'></asp:Label>
                                </td>--%>



                                        <%-- <td style="text-align:center; width:5%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="View" CommandArgument='<%# Eval("ID") %>' 
                                        ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View"/>
                                </td>

                                <td style="text-align:center; width:4%;">
                                   <asp:ImageButton ID="ImageButton2" runat="server" CommandName="Delete" CommandArgument='<%# Eval("Quotation_no") %>' 
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" onclientclick="return ValidateDelete1();"/>
                                </td>
                                        --%>

                                        <td style="text-align: center; width: 5%;">
                                            <a href="#" title="Print Quotation..." onclick="window.open('/corporate/business/print/NewQuotation.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                        </td>
                                        <td style="text-align: center; width: 4%;">
                                            <asp:ImageButton ID="ImageButton1" runat="server"
                                                CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select"
                                                ImageUrl="~/corporate/business/WebImages/tick-icon.png"
                                                ToolTip="Select" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>
                    </td>
                </tr>
                <tr>
                    <td class="auto-style2"></td>
                    <td colspan="2" class="auto-style2"></td>
                    <td colspan="2" class="auto-style2"></td>
                    <td class="auto-style2"></td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:Panel ID="Panel1" runat="server" Visible="false">
                            <table cellpadding="0" cellspacing="0" class="auto-style1">
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">
                                        <asp:Label ID="Label17" runat="server" Text="Quotation / P.O. Number"></asp:Label>
                                    </td>
                                    <td width="35%">
                                        <asp:Label ID="lbl_recordno" runat="server" Text="Label" ForeColor="DarkBlue" Font-Bold="true" Font-Size="Medium"></asp:Label>
                                    </td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label16" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Select Client Name</td>
                                    <td>
                                        <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style"></asp:DropDownList>
                                    </td>
                                    <td>
                                        <asp:Label ID="Label15" runat="server" Text="1" Visible="False"></asp:Label>
                                    </td>
                                </tr>

                                <tr>
                                    <td class="auto-style2"></td>
                                    <td style="text-align: right;" class="auto-style2">Enable Reference Details&nbsp;:&nbsp;</td>
                                    <td class="auto-style2">
                                        <asp:RadioButton ID="rbYes" runat="server" GroupName="referenceOption" Text="Yes" onclick="toggleReferenceFields('Yes')" />
                                        <asp:RadioButton ID="rbNo" runat="server" GroupName="referenceOption" Text="No" onclick="toggleReferenceFields('No')" />
                                    </td>
                                    <td class="auto-style2"></td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;&nbsp;Reference Person Name&nbsp;:&nbsp;</td>
                                    <td>
                                        <asp:TextBox ID="txt_clientrefname" runat="server" CssClass="textbox_style" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Reference ID&nbsp;:&nbsp;</td>
                                    <td>
                                        <asp:TextBox ID="txt_clientrefid" runat="server" CssClass="textbox_style" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Reference Date&nbsp;:&nbsp;</td>
                                    <td>
                                        <asp:TextBox ID="txt_clientrefdate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"
                                            class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label3" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Quotation Date&nbsp;</td>
                                    <td>
                                        <asp:TextBox ID="txtquotationDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="Label4" runat="server" Text="*" ForeColor="Red"></asp:Label>&nbsp;Place Of Supply</td>
                                    <td>
                                        <asp:DropDownList ID="ddlPlaceOfSupply" runat="server" CssClass="dropdown_style">
                                        </asp:DropDownList>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>Select CGST/SGST for Intra-State OR IGST for Inter-State</td>
                                    <td>
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
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;<asp:Label ID="lbl_recordtype" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>Record / Document Type</td>
                                    <td>&nbsp;
                                        <asp:RadioButton ID="rbQt" runat="server" GroupName="recordOption" Text="Quotation" AutoPostBack="false" OnClick="togglePanel()" />&nbsp;&nbsp;
                                        <asp:RadioButton ID="rbPo" runat="server" GroupName="recordOption" Text="Purchase Order" AutoPostBack="false" OnClick="togglePanel()" />
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td colspan="2">
                                        <asp:Panel ID="PO_DataInputs" runat="server" Visible="true">
                                            <table cellpadding="2" cellspacing="2" class="auto-style1">
                                                <tr>
                                                    <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label5" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Delivery Order No.</td>
                                                    <td width="50%">
                                                        <asp:TextBox ID="txb_donumber" runat="server" CssClass="textbox_style po-mandatory" Width="110px"></asp:TextBox></td>
                                                </tr>
                                                <tr>
                                                    <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label8" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Ref. Contract No.</td>
                                                    <td width="50%">
                                                        <asp:TextBox ID="txb_ponumber" runat="server" CssClass="textbox_style po-mandatory" Width="110px"></asp:TextBox></td>
                                                </tr>
                                                <tr>
                                                    <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label14" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Purchase Order Date</td>
                                                    <td width="50%">
                                                        <asp:TextBox ID="txb_podate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label6" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Validity Start Date</td>
                                                    <td width="50%">
                                                        <asp:TextBox ID="txb_strtdt" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="50%" style="text-align: right;">&nbsp;<asp:Label ID="Label7" runat="server" Visible="true" Text="*" ForeColor="Red"></asp:Label>&nbsp;Validity End Date</td>
                                                    <td width="50%">
                                                        <asp:TextBox ID="txb_enddt" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker po-mandatory" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">
                                        <asp:Label ID="Label1" runat="server" Text="1" Visible="False"></asp:Label>
                                        <asp:Label ID="Label2" runat="server" Text="Label2" Visible="False"></asp:Label>
                                        <asp:Label ID="lblqno" runat="server" Text="lblqno" Visible="False"></asp:Label>
                                    </td>
                                    <td width="35%">Select Product &/or Service Category One by One</td>
                                    <td width="35%">
                                        <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style">
                                        </asp:DropDownList>
                                    </td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%" colspan="2" style="width: 70%; text-align: center">
                                        <asp:Button ID="Button2" runat="server" CssClass="btn_style" OnClick="Button2_Click" OnClientClick="return ValidateDataField10();" Text="Click to Retrieve Product &/or Service from the selected Category" Width="400px" />
                                    </td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>
                                        <asp:GridView ID="gridps" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Visible="true" Width="100%">
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
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td colspan="4">
                                        <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                            <RowStyle BackColor="#94B8FF" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="HSN CODE">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

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

                                                <asp:TemplateField HeaderText="Brand Name">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
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

                                                <asp:TemplateField HeaderText="Specification">
                                                    <EditItemTemplate>
                                                        <asp:Label ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:Label>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:Label>
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

                                                <asp:TemplateField HeaderText="Quantity">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Quantity" runat="server" onkeypress="return validate(event, this)"></asp:Label>
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
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <%--  <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%" class="auto-style3" colspan="2" style="text-align:center">
                                &nbsp;
                                
                            </td>
                            <td width="15%">&nbsp;</td>
                        </tr>--%>

                                <tr>
                                    <td style="text-align: center" colspan="2">
                                        <asp:Button ID="btnAddProduct" runat="server" CssClass="btn_style" OnClick="btnAddProduct_Click" CausesValidation="false" Text="Add Required Product &/or Service  against the Selected Category from the above Table" Width="500px" /></td>
                                    <td colspan="2" style="text-align: center; color: red; font-weight: bold;">Go back to the Select Product/Service Category in case more Product/Service Categories need to be added</td>
                                </tr>

                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td colspan="4">

                                        <%--<asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
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



                                                <asp:TemplateField HeaderText="Base Rate (RS)">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Sail_Rate" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" onkeypress="return validate(event)"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="GST RATE (%)">
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
                                                        <asp:TextBox ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" onkeypress="return validate(event)"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Select">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                                    </EditItemTemplate>

                                                    <HeaderTemplate>
                                                        <asp:CheckBox ID="checkAll" runat="server" Checked="true" onclick="checkAll(this);" />
                                                    </HeaderTemplate>

                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chk" runat="server" Checked="true" onclick="Check_Click(this)" />
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
                                        </asp:GridView>--%>

                                        <%--<asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                    <RowStyle BackColor="#94B8FF" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="Select">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chk" runat="server"  Checked="true"/>
                                               
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Service/Product Code">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Ser_pro_code" runat="server" Text='<%# Bind("Ser_pro_code") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Ser_pro_code" runat="server" Text='<%# Bind("Ser_pro_code") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Service/Product Name">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Ser_pro_Name" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Ser_pro_Name" runat="server" Text='<%# Bind("Ser_pro_Name") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Specification">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="specification" runat="server" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="250px" onkeypress="return validate1(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                     
                                        
                                        <asp:TemplateField HeaderText="Base Rate">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Sale_rate" runat="server" Text='<%# Bind("Sale_rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px"  onkeypress="return validate(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Output %(VAT/SERVICE TAX)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="service_Tax_Rate" runat="server" Text='<%# Bind("service_Tax_Rate") %>'></asp:Label>
                                                
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Quantity">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Total_quanty" runat="server" Text='<%# Bind("Total_quanty") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px"  onkeypress="return validate(event)"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <FooterStyle BackColor="#CCCC99" />
                                    <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                                    <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                </asp:GridView>--%>


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

                                                <asp:TemplateField HeaderText="Product/Service Category" Visible="true" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Brand Name" HeaderStyle-Width="8%" ItemStyle-Width="8%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>' onkeypress="return validate(event, this)"></asp:Label>
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

                                                <asp:TemplateField HeaderText="Specification" HeaderStyle-Width="10%" ItemStyle-Width="10%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Specification" runat="server" Text='<%# Bind("Specification") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Specification" runat="server" Text='<%# Bind("Specification") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px" Width="80%" onkeypress="return validate1(event)"></asp:TextBox>
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

                                                <asp:TemplateField HeaderText="Item No" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="ItemNo" runat="server" BorderColor="#333333" Text='<%# Bind("ItemNo") %>' BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Material No" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="MaterialNo" runat="server" BorderColor="#333333" Text='<%# Bind("MaterialNo") %>' BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Pack Size" HeaderStyle-Width="8%" ItemStyle-Width="8%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="PackSize" runat="server" BorderColor="#333333" Text='<%# Bind("PackSize") %>' BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style" Height="22px" Width="90%"></asp:TextBox>
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

                                                <asp:TemplateField HeaderText="GST (%)" HeaderStyle-Width="5%" ItemStyle-Width="5%">
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
                                                        <asp:TextBox ID="Discount_Rate" runat="server" BorderColor="#333333" Text='<%# Bind("discount_rate") %>' BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="QTY" HeaderStyle-Width="3%" ItemStyle-Width="3%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Quantity" runat="server" BorderColor="#333333" BorderStyle="Solid" Text='<%# Bind("Quantity") %>' BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="80%" onkeypress="return validate(event, this)"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Remarks" HeaderStyle-Width="10%" ItemStyle-Width="5%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox9" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="ItemRemarks" runat="server" BorderColor="#333333" BorderStyle="Solid" Text='<%# Bind("ItemRemarks") %>' BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="SL" HeaderStyle-Width="3%" ItemStyle-Width="3%">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="txtOrder" runat="server" Width="80%" Text='<%# Bind("Sl_no") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Height="22px" CssClass="center textbox_style" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                
                                                <asp:TemplateField HeaderText="Delivery Date" HeaderStyle-Width="12%" ItemStyle-Width="12%" Visible="false">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="DeliveryDate" runat="server" CssClass="datepicker"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="DeliveryDate" runat="server" Text='<%# Bind("DeliveryDate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="datepicker center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="rfvDeliveryDate" runat="server" ControlToValidate="DeliveryDate" ErrorMessage="*" ForeColor="Red" Display="Dynamic" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Department" HeaderStyle-Width="12%" ItemStyle-Width="12%" Visible="false">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="Department" runat="server"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Department" runat="server" Text='<%# Bind("Department") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="center textbox_style" Height="22px" Width="90%"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="rfvDepartment" runat="server" ControlToValidate="Department" ErrorMessage="*" ForeColor="Red" Display="Dynamic" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="Select" HeaderStyle-Width="5%" ItemStyle-Width="5%">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="TextBox6" runat="server" checked="true"></asp:TextBox>
                                                    </EditItemTemplate>

                                                    <HeaderTemplate>
                                                        <asp:CheckBox ID="checkAll" runat="server" Text="All" onclick="checkAll(this);" />
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
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">Overall Discount or Inflation
                                    </td>
                                    <td width="35%">
                                        <asp:RadioButtonList ID="RadioDiscountInflation" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem>Discount</asp:ListItem>
                                            <asp:ListItem>Inflation</asp:ListItem>
                                            <asp:ListItem Selected="True">NotApplicable</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr id="percentage" runat="server">
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">Percentage</td>
                                    <td width="35%">
                                        <asp:TextBox ID="txtPercentage" runat="server" Text="0" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21"></asp:TextBox></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>

                                <tr>
                                    <td colspan="4">
                                        <table>
                                            <tr>
                                                <td width="20%" style="font-weight: bold;">Add Payment Phase & Payment %age<br />
                                                    (Select Payment Phase One By One)</td>
                                                <td width="5%"></td>
                                                <td width="30%">
                                                    <asp:ListBox ID="listPhaseType" runat="server" Font-Size="14px" multiple="true" SelectionMode="Multiple" Rows="7" Width="250px" BackColor="#94b8ff" OnTextChanged="listPhaseType_TextChanged" AutoPostBack="True"></asp:ListBox></td>
                                                <td width="5%"></td>
                                                <td width="40%">
                                                    <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" BorderWidth="1px" BackColor="White" BorderColor="#E8F3FF" CellPadding="3" CellSpacing="2" BorderStyle="Solid" OnRowDeleting="GridView3_RowDeleting" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                                                        <RowStyle BackColor="#94B8FF" />
                                                        <Columns>
                                                            <asp:TemplateField HeaderText="Payment %" HeaderStyle-Width="25%" ItemStyle-Width="25%">
                                                                <%--<EditItemTemplate>
                                                            <asp:TextBox ID="AmountPer" runat="server"  Text=""></asp:TextBox>
                                                        </EditItemTemplate>--%>
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="AmountPer" runat="server" AutoPostBack="true" Text='<%# Bind("AmountPer") %>' Width="75%" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" Height="22px" OnTextChanged="AmountPer_TextChanged"></asp:TextBox>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Payment Phase / Term" HeaderStyle-Width="25%" ItemStyle-Width="25%">
                                                                <EditItemTemplate>
                                                                    <asp:TextBox ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:TextBox>
                                                                </EditItemTemplate>
                                                                <ItemTemplate>
                                                                    <asp:Label ID="PaymentPhase" runat="server" Text='<%# Bind("PaymentPhase") %>'></asp:Label>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Phase Description / Instruction" HeaderStyle-Width="25%" ItemStyle-Width="45%">
                                                                <EditItemTemplate>
                                                                    <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' Width="80%" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" TextMode="MultiLine"></asp:TextBox>
                                                                </EditItemTemplate>
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="PhaseDesc" runat="server" Text='<%# Bind("PhaseDesc") %>' Width="80%" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" TextMode="MultiLine"></asp:TextBox>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>

                                                            <asp:CommandField ButtonType="Button" HeaderText="Delete" ShowDeleteButton="True" HeaderStyle-Width="5%" ItemStyle-Width="5%" />
                                                        </Columns>
                                                        <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                                        <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                                                    </asp:GridView>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Validay Day Input (Days) :</td>
                                    <td>&nbsp;<asp:TextBox ID="txt_valdays" runat="server" Text="" CssClass="textbox_style" TextMode="Number" MaxLength="3"></asp:TextBox></td>
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
                                    <td style="text-align: right;">&nbsp;Particulars View Type :</td>
                                    <td>
                                        <asp:DropDownList ID="DDL_ItemViewType" runat="server" CssClass="dropdown_style">
                                            <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                            <asp:ListItem Text="Simple" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="Detailed" Value="2"></asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Discount Visibility :</td>
                                    <td>
                                        <asp:DropDownList ID="DDL_DiscountView" runat="server" CssClass="dropdown_style">
                                            <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                            <asp:ListItem Text="Yes" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="No" Value="2"></asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Delivery Tenure Selection (Weeks) :</td>
                                    <td>
                                        <asp:DropDownList ID="DDL_DeliveryTerms" runat="server" CssClass="dropdown_style" onchange="handleDeliveryTermChange(this)">
                                            <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                            <asp:ListItem Text="10-12" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="3-4" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="1-2" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="Manual Input" Value="4"></asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="manualInputRow" style="display: none;">
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Delivery Tenure Input (Weeks) :</td>
                                    <td>&nbsp;<asp:TextBox ID="txt_deltrms" runat="server" Text="0" CssClass="textbox_style" TextMode="SingleLine" MaxLength="5" placeholder="e.g., 1-2"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RFV_txt_deltrms" runat="server" ErrorMessage="Required" ControlToValidate="txt_deltrms" Display="Dynamic" InitialValue="0"></asp:RequiredFieldValidator>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Package Forwarding Option :</td>
                                    <td>
                                        <asp:DropDownList ID="DDL_pkgfrwd" runat="server" CssClass="dropdown_style" onchange="handlePackageForwardingChange(this)">
                                            <asp:ListItem Text="--SELECT--" Value="0" Selected="True"></asp:ListItem>
                                            <asp:ListItem Text="NILL" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="At Actuals" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="Manual Input" Value="3"></asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr id="manualInputPkgRow" style="display: none;">
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Package Forwarding Input :</td>
                                    <td>
                                        <asp:TextBox ID="txt_pkgfrwd" runat="server" Text="" CssClass="textbox_style" TextMode="SingleLine" MaxLength="50" placeholder="Enter package details"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RFV_txt_pkgfrwd" runat="server" ErrorMessage="Required" ControlToValidate="txt_pkgfrwd" Display="Dynamic"></asp:RequiredFieldValidator>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr>
                                    <td>&nbsp;</td>
                                    <td style="text-align: right;">&nbsp;Custom Remarks / Comments :</td>
                                    <td>&nbsp;<asp:TextBox ID="txt_remarks" runat="server" Text="" CssClass="textbox_style" TextMode="MultiLine" MaxLength="500" Rows="6" Height="80px" Columns="4" placeholder="Enter your remarks or comments..."></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RFV_txt_remarks" runat="server" ErrorMessage="Remarks are required." ControlToValidate="txt_remarks" Display="Dynamic"></asp:RequiredFieldValidator>
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


                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%" colspan="2" style="width: 70%; text-align: center">&nbsp;
                                        <asp:Button ID="btnSabe" runat="server" CausesValidation="false" Width="200px" CssClass="btn_style" OnClick="btnSabe_Click" Text="Update Existing" />
                                        &nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:Button ID="btnNew" runat="server" CausesValidation="false" Width="200px" CssClass="btn_style" OnClick="btnNew_Click" Text="Recreate as New Record" /></td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td width="15%">&nbsp;</td>
                                    <td width="35%">&nbsp;
                                    </td>
                                    <td width="35%">&nbsp;</td>
                                    <td width="15%">&nbsp;</td>
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
        </ContentTemplate>

        <Triggers>
            <asp:PostBackTrigger ControlID="btnSabe" />
            <asp:PostBackTrigger ControlID="Button2" />
            <asp:PostBackTrigger ControlID="btnAddProduct" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
