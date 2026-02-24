<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="add_chalan.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm38" %>

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
        function validate(key) {
            var keycode = (key.which) ? key.which : key.keyCode;
            var phn = document.getElementById('txtfillrequar');
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57)) {
                return false;
            } else {
                if (phn && phn.value.length < 50) return true;
                else return false;
            }
        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {
                        row.style.backgroundColor = "#FFFF99";
                        inputList[i].checked = true;
                    } else {
                        if (row.rowIndex % 2 == 0) row.style.backgroundColor = "#D5D5BF";
                        else row.style.backgroundColor = "white";
                        inputList[i].checked = false;
                    }
                }
            }
        }

        function fetchSuggestions(term) {
            // Trigger the search only if the user has typed 3 or more characters
            if (term.length >= 3) {
                $.ajax({
                    // Adjust the URL if your page name is slightly different
                    url: "add_chalan.aspx/GetDocumentNumbers",
                    data: JSON.stringify({ prefixText: term }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var datalist = $("#docNumbersList");
                        datalist.empty(); // Clear old suggestions

                        // Loop through the results and add them as options to the datalist
                        $.each(data.d, function (index, item) {
                            datalist.append("<option value='" + item + "'>");
                        });
                    },
                    error: function (response) {
                        console.log("Error fetching suggestions: " + response.responseText);
                    }
                });
            }
        }

        function fetchClientSuggestions(term) {
            // Trigger the search after 2 characters for client names
            if (term.length >= 2) {
                $.ajax({
                    url: "add_chalan.aspx/GetClientNames",
                    data: JSON.stringify({ prefixText: term }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var datalist = $("#clientList");
                        datalist.empty(); // Clear old suggestions

                        // Add new suggestions
                        $.each(data.d, function (index, item) {
                            datalist.append("<option value='" + item + "'>");
                        });
                    },
                    error: function (response) {
                        console.log("Error fetching clients: " + response.responseText);
                    }
                });
            }
        }

        function checkPartialQuantity(textBox) {
            // Get the typed amount and the max allowed amount
            var enteredValue = parseFloat(textBox.value) || 0;
            var maxValue = parseFloat(textBox.getAttribute("data-max")) || 0;

            // If they typed more than what is left, warn them and reset it to the max
            if (enteredValue > maxValue) {
                alert("You cannot deliver more than the remaining quantity (" + maxValue + ").");
                textBox.value = maxValue; // Auto-correct back to the max allowed
            }
                // If they delete everything, default it to 0
            else if (textBox.value.trim() === "") {
                textBox.value = 0;
            }
        }

        // Function to allow only numbers and decimals to textbox
        function validate(key) {
            var keycode = (key.which) ? key.which : key.keyCode;

            // Allow Backspace (8), Delete/Decimal point (46), and Numbers (48 to 57)
            if ((keycode == 8 || keycode == 46) || (keycode >= 48 && keycode <= 57)) {
                return true;
            }

            // Reject everything else (letters, symbols, etc.)
            return false;
        }

        function recalculateTotalDue() {
            var total = 0;

            // Find all the Due Qty textboxes using the specific class we added
            var textboxes = document.querySelectorAll('.calc-due-qty');

            // Loop through them and add up the numbers
            for (var i = 0; i < textboxes.length; i++) {
                // Parse the number, or treat it as 0 if the box is empty/invalid
                var val = parseFloat(textboxes[i].value) || 0;
                total += val;
            }

            // Find the footer label and update its text
            var footerLabel = document.querySelector('.total-due-label');
            if (footerLabel) {
                footerLabel.innerHTML = total;
            }
        }
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <ProgressTemplate>
            <div style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 9999; display: flex; justify-content: center; align-items: center;">
                <div style="background: white; padding: 20px; border-radius: 5px; text-align: center; font-family: Arial; box-shadow: 0 4px 8px rgba(0,0,0,0.2);">
                    <img src="../WebImages/aagrouplogo.png" alt="Loading..." style="width: 32px; height: 32px;" /><br />
                    <strong style="color: #19658A; margin-top: 10px; display: block;">Processing... Please wait.</strong>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Add DPCC (Delivery planning cum Challan)</span>>&nbsp;</td>
                </tr>
                <tr>
                    <td width="10%">&nbsp;</td>
                    <td width="40%" colspan="2">
                        <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label></td>
                    <td width="40%" colspan="2">&nbsp;</td>
                    <td width="10%">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" BackColor="#FFE6E6">
                            &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                            &nbsp;<asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td>Client Name</td>
                    <td>
                        <asp:TextBox ID="txtClientName" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="200px" placeholder="Type client name..." list="clientList" autocomplete="off" onkeyup="fetchClientSuggestions(this.value)"></asp:TextBox>
                        <datalist id="clientList"></datalist>
                    </td>
                    <td>Record Type</td>
                    <td>
                        <asp:DropDownList ID="cmbRecordType" runat="server" CssClass="dropdown_style">
                            <asp:ListItem Text="-- All Types --" Value=""></asp:ListItem>
                            <asp:ListItem Text="Quotation" Value="Quotation"></asp:ListItem>
                            <asp:ListItem Text="Purchase Order" Value="Purchase Order"></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date</td>
                    <td>
                        <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="130px"></asp:TextBox>
                    </td>
                    <td>To Date</td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="130px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>Doc / PO / DO No.</td>
                    <td colspan="3">
                        <asp:TextBox ID="txtDocNumber" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="300px" placeholder="Search QTN, PO, or DO No..." list="docNumbersList" autocomplete="off" onkeyup="fetchSuggestions(this.value)"></asp:TextBox>
                        <datalist id="docNumbersList"></datalist>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" style="text-align: center; padding-top: 15px;">
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search Records" />
                        &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset Filters" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="6">
                        <asp:Panel ID="Panel2" runat="server" Visible="true">
                            <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                                <FooterStyle BackColor="White" ForeColor="#000066" />
                                <AlternatingItemStyle BackColor="#94B8FF" />
                                <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                                <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                <HeaderTemplate>
                                    <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                        <tr>
                                            <td style="text-align: center; width: 6%;">
                                                <asp:Label ID="LabelSL0" runat="server" Text="SL No"></asp:Label></td>
                                            <td style="text-align: center; width: 16%;">
                                                <asp:Label ID="showid0" runat="server" Text="Quotation/PO no"></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="showrm0" runat="server" Text="Date"></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="Label17" runat="server" Text="DO Number"></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="Label18" runat="server" Text="PO Number"></asp:Label></td>
                                            <td style="text-align: center; width: 16%;">
                                                <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label></td>
                                            <td style="text-align: center; width: 15%;">
                                                <asp:Label ID="Label1" runat="server" Text="Product Catagory"></asp:Label></td>
                                            <td style="text-align: center; width: 13%;">
                                                <asp:Label ID="Label12" runat="server" Text="Net Amount"></asp:Label></td>
                                            <td style="text-align: center; width: 4%;">
                                                <asp:Label ID="edit0" runat="server" Text="Select"></asp:Label></td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                        <tr>
                                            <td style="text-align: center; width: 6%;">
                                                <asp:Label ID="LabelSL" runat="server" Text='<%# Container.ItemIndex + 1 %>'></asp:Label></td>
                                            <td style="text-align: center; width: 16%;">
                                                <asp:Label ID="ID0" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="addshowname0" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="Label17val" runat="server" Text='<%# Eval("DO_Number") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 10%;">
                                                <asp:Label ID="Label18val" runat="server" Text='<%# Eval("PO_Number") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 16%;">
                                                <asp:Label ID="Label13" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 15%;">
                                                <asp:Label ID="Label3" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label></td>
                                            <td style="text-align: center; width: 13%;">Rs.<asp:Label ID="Label16" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                                /- </td>
                                            <td style="text-align: center; width: 4%;">
                                                <asp:ImageButton ID="ImageButton1" runat="server" CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select" ImageUrl="~/corporate/business/WebImages/tick-icon.png" ToolTip="Select" />
                                            </td>
                                        </tr>
                                    </table>
                                </ItemTemplate>
                            </asp:DataList>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="6">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="Panel1" runat="server" Visible="false">
                            <table class="auto-style1">
                                <tr>
                                    <td>Client Name</td>
                                    <td>
                                        <asp:Label ID="lblClientName" ForeColor="#0000cc" Font-Bold="true" runat="server"></asp:Label>&nbsp;[<asp:Label ID="lblClient_Id" runat="server"></asp:Label>]</td>
                                    <td>P.O. No</td>
                                    <td>
                                        <asp:Label ID="lbl_ponumber" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td width="13%">ERP Record No</td>
                                    <td width="37%">
                                        <asp:Label ID="lblQuotation_no" runat="server" Font-Bold="true" ForeColor="#cc3300"></asp:Label>&nbsp;Created on&nbsp;<asp:Label ID="lblQuotation_date" Font-Bold="true" ForeColor="#993300" runat="server"></asp:Label>
                                    </td>
                                    <td width="13%">&nbsp;D.O. Number</td>
                                    <td width="37%">
                                        <asp:Label ID="lbl_donumber" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>Quotation Amount</td>
                                    <td>
                                        <asp:Label ID="lblGross_amount" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblNet_amount" runat="server"></asp:Label>
                                    </td>
                                    <td>&nbsp;Challan/ Delivery Date</td>
                                    <td>
                                        <asp:TextBox ID="txtinvoiceDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" ForeColor="#ff3300" Font-Bold="true" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                                        <asp:Label ID="lblservicetax" runat="server" Visible="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="">Delivery Address:</td>
                                    <td colspan="2">
                                        <asp:ListBox ID="FactoryAddress" runat="server" AutoPostBack="True" BorderStyle="Solid" BorderWidth="1px" Font-Bold="true" Font-Size="10px" multiple="true" Rows="3" SelectionMode="Multiple" Width="550px"></asp:ListBox>
                                    </td>
                                    <td>&nbsp;<asp:Label ID="Label2" runat="server" Text="<--- Select Delivery Address" Font-Italic="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td colspan="4">
                                        <asp:GridView ID="gd_Quotation" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 12px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%" OnRowDataBound="gd_Quotation_RowDataBound" ShowFooter="true">
                                            <Columns>
                                                <asp:TemplateField HeaderText="SL No" HeaderStyle-Width="5%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblSL" runat="server" Text='<%# Container.DataItemIndex + 1 %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Product ID" Visible="false">
                                                    <ItemTemplate>
                                                        <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="HSN Code" Visible="false">
                                                    <ItemTemplate>
                                                        <asp:Label ID="product_id" runat="server" Text='<%# Bind("product_id") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Product Name" HeaderStyle-Width="20%">
                                                    <ItemStyle HorizontalAlign="Left" />
                                                    <ItemTemplate>
                                                        <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Item No" HeaderStyle-Width="7%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="ItemNo" runat="server" Text='<%# Bind("ItemNo") %>' Font-Bold="true" ForeColor="#0033cc"></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Material No" HeaderStyle-Width="7%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="MaterialNo" runat="server" Text='<%# Bind("MaterialNo") %>' Font-Bold="true" ForeColor="#800000"></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Pack Size" HeaderStyle-Width="6%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="PackSize" runat="server" Text='<%# Bind("PackSize") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Department" HeaderStyle-Width="10%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="Department" runat="server" Text='<%# Bind("Department") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="DeliveryDate" HeaderStyle-Width="10%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="DeliveryDate" runat="server" Text='<%# Bind("DeliveryDate") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Quoted Qty" HeaderStyle-Width="6%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="Quantity" runat="server" Text='<%# Bind("Quantity") %>' Font-Bold="true" ForeColor="#990000"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:Label ID="lblTotalQuoted" runat="server" Font-Bold="true" ForeColor="Black" />
                                                    </FooterTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Delivered Qty" HeaderStyle-Width="6%">
                                                    <ItemTemplate>
                                                        <asp:Label ID="DeliveredQnt" runat="server" Text='<%# Bind("DeliveredQnt") %>' Font-Bold="true" ForeColor="#006600"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:Label ID="lblTotalDelivered" runat="server" Font-Bold="true" ForeColor="Black" />
                                                    </FooterTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Due Qty" HeaderStyle-Width="5%">
                                                    <ItemTemplate>
                                                        <asp:TextBox ID="Qty" runat="server"
                                                            Text='<%# Bind("RemainQny") %>'
                                                            data-max='<%# Eval("RemainQny") %>'
                                                            BorderColor="#CCCCCC"
                                                            Style="text-align: center;"
                                                            CssClass="textbox_U_Datalist_style calc-due-qty"
                                                            Width="95%"
                                                            BorderStyle="Solid"
                                                            BorderWidth="1px"
                                                            Font-Names="Tahoma, Geneva, sans-serif"
                                                            Font-Size="11px"
                                                            Height="22px"
                                                            onkeypress="return validate(event)"
                                                            onkeyup="recalculateTotalDue()"
                                                            onblur="checkPartialQuantity(this); recalculateTotalDue();">
                                                        </asp:TextBox>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:Label ID="lblTotalDue" runat="server" Font-Bold="true" ForeColor="Black" CssClass="total-due-label" />
                                                    </FooterTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Create" HeaderStyle-Width="5%">
                                                    <HeaderTemplate>
                                                        <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <asp:CheckBox ID="chk" runat="server" />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#CCCC99" />
                                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                        </asp:GridView>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4" style="text-align: center; padding: 20px;">
                                        <asp:Button ID="Button1" runat="server" CssClass="btn_style" Text="Create Challan" OnClick="Button1_Click" UseSubmitBehavior="false" OnClientClick="this.disabled=true; this.value='Processing...';" />
                                        &nbsp;<asp:Button ID="Button2" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Cancel" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="Button1" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
