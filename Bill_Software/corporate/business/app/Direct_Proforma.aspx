<%@ Page Title="Direct Proforma" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Direct_Proforma.aspx.cs" Inherits="Bill_Software.corporate.business.app.Direct_Proforma" %>

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

        .textbox_style {
            border: 1px solid #333;
        }

        /* Stepper Styles */
        .step-container {
            display: flex;
            justify-content: space-between;
            margin-bottom: 20px;
            padding: 0 50px;
            background-color: #fff;
            padding-top: 10px;
        }

        .step-item {
            flex: 1;
            text-align: center;
            padding: 10px;
            border-bottom: 3px solid #ddd;
            color: #aaa;
            font-weight: bold;
            cursor: default;
        }

            .step-item.active {
                border-bottom-color: #19658A;
                color: #19658A;
            }

            .step-item.completed {
                border-bottom-color: green;
                color: green;
            }

        .nav-buttons {
            margin-top: 20px;
            text-align: center;
            padding: 10px;
            background: #f9f9f9;
            border-top: 1px solid #ddd;
        }

        .form-section {
            margin: 20px auto;
            width: 80%;
            border: 1px solid #eee;
            padding: 20px;
            box-shadow: 0 0 5px rgba(0,0,0,0.1);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_pageLoaded(function () {
            $(".datepicker").datepicker({ dateFormat: 'dd-M-yy', changeMonth: true, changeYear: true });
            UpdateCounter(); // Ensure counter updates on postback
        });

        function validate(key, element) {
            var keycode = (key.which) ? key.which : key.keyCode;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57) && keycode != 46) {
                return false;
            }
            return true;
        }

        // --- Selection Logic & Counter ---
        function UpdateCounter() {
            var count = 0;
            var grid = document.getElementById("<%= gridProdWithCat.ClientID %>");
            if (grid) {
                var inputs = grid.getElementsByTagName("input");
                for (var i = 0; i < inputs.length; i++) {
                    if (inputs[i].type == "checkbox" && inputs[i].id.indexOf("checkAll") == -1) {
                        if (inputs[i].checked) count++;
                    }
                }
            }
            var counterLbl = document.getElementById("lblSelectedCount");
            if (counterLbl) counterLbl.innerText = count;
        }

        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;
            if (objRef.checked) { row.style.backgroundColor = "#84e26e"; }
            else {
                if (row.rowIndex % 2 == 0) row.style.backgroundColor = "#C2D69B";
                else row.style.backgroundColor = "white";
            }
            UpdateCounter();
        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    if (objRef.checked) {
                        inputList[i].checked = true; row.style.backgroundColor = "#84e26e";
                    } else {
                        inputList[i].checked = false;
                        if (row.rowIndex % 2 == 0) row.style.backgroundColor = "#C2D69B";
                        else row.style.backgroundColor = "white";
                    }
                }
            }
            UpdateCounter();
        }

        function ShowRows(mode) {
            var grid = document.getElementById("<%= gridProdWithCat.ClientID %>");
            if (!grid) return;
            var tr = grid.getElementsByTagName("tr");
            for (var i = 1; i < tr.length; i++) {
                var checkbox = tr[i].getElementsByTagName("input")[0];
                if (checkbox && checkbox.type == "checkbox") {
                    if (mode == 'all') tr[i].style.display = "";
                    else if (mode == 'checked') tr[i].style.display = (checkbox.checked) ? "" : "none";
                    else if (mode == 'unchecked') tr[i].style.display = (!checkbox.checked) ? "" : "none";
                }
            }
        }

        function FilterGrid() {
            var input = document.getElementById("txtQuickFilter");
            var filter = input.value.toUpperCase();
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            var tr = table.getElementsByTagName("tr");

            for (var i = 1; i < tr.length; i++) {
                var tds = tr[i].getElementsByTagName("td");

                // CORRECTED INDICES BASED ON VISIBLE COLUMNS:
                // [0] = Checkbox
                // [1] = Product Name
                // [2] = Specification
                // [3] = HSN Code

                if (tds.length > 3) {
                    var txtName = tds[1].textContent || tds[1].innerText; // Index 1: Product Name
                    var txtSpec = tds[2].textContent || tds[2].innerText; // Index 2: Spec
                    var txtHSN = tds[3].textContent || tds[3].innerText;  // Index 3: HSN

                    // Check if filter matches Name, Spec, or HSN
                    if (txtName.toUpperCase().indexOf(filter) > -1 ||
                        txtSpec.toUpperCase().indexOf(filter) > -1 ||
                        txtHSN.toUpperCase().indexOf(filter) > -1) {

                        tr[i].style.display = ""; // Show
                    } else {
                        tr[i].style.display = "none"; // Hide
                    }
                }
            }
        }
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Create Direct Proforma Invoice</span></td>
                </tr>
            </table>

            <div class="step-container">
                <div id="step1" runat="server" class="step-item active">1. Client & Setup</div>
                <div id="step2" runat="server" class="step-item">2. Select Products</div>
                <div id="step3" runat="server" class="step-item">3. Review & Save</div>
            </div>

            <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">

                <asp:View ID="vSetup" runat="server">
                    <div class="form-section">
                        <h3 style="color: #19658A; border-bottom: 1px solid #eee; padding-bottom: 10px; margin-bottom: 20px;">
                            <img src="../WebImages/representative.png" style="vertical-align: middle; width: 24px;" />
                            Step 1: Invoice Details
                        </h3>

                        <table style="width: 100%; border-spacing: 15px; border-collapse: separate;">
                            <tr>
                                <td width="20%" style="vertical-align: top; padding-top: 8px;"><strong>Select Client:</strong> <span style="color: red">*</span></td>
                                <td width="80%">
                                    <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style" Width="300px" AutoPostBack="true" OnSelectedIndexChanged="cmbClient_SelectedIndexChanged"></asp:DropDownList>

                                    <asp:Panel ID="pnlClientInfo" runat="server" Visible="false" Style="margin-top: 15px; background-color: #f8faff; border: 1px solid #b3d7ff; border-left: 4px solid #19658A; padding: 15px; border-radius: 4px; width: 90%;">
                                        <div style="font-weight: bold; color: #19658A; margin-bottom: 5px;">Client Details:</div>
                                        <table style="width: 100%; font-size: 11px; color: #444;">
                                            <tr>
                                                <td style="width: 15%; color: #777;">ID:</td>
                                                <td>
                                                    <asp:Label ID="lblclientID" runat="server" Font-Bold="true"></asp:Label></td>
                                                <td style="width: 15%; color: #777;">GST No:</td>
                                                <td>
                                                    <asp:Label ID="lblClientGST" runat="server" Font-Bold="true"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="vertical-align: top; color: #777;">Address:</td>
                                                <td colspan="3">
                                                    <asp:Label ID="lblClientAddress" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td style="color: #777;">State:</td>
                                                <td>
                                                    <asp:Label ID="lblClientState" runat="server"></asp:Label></td>
                                                <td style="color: #777;">Place of Supply:</td>
                                                <td>
                                                    <asp:Label ID="lblPlaceOfSupply" runat="server"></asp:Label></td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </td>
                            </tr>

                            <tr>
                                <td><strong>Proforma Date:</strong> <span style="color: red">*</span></td>
                                <td>
                                    <asp:TextBox ID="txtinvoiceDate" runat="server" CssClass="datepicker dropdown_style" Width="120px" Style="padding: 5px; border: 1px solid #ccc;"></asp:TextBox></td>
                            </tr>

                            <tr>
                                <td><strong>Tax Type:</strong></td>
                                <td>
                                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal" CellSpacing="10">
                                        <asp:ListItem Value="1" Selected="True"> Intra-State (CGST/SGST) </asp:ListItem>
                                        <asp:ListItem Value="0"> Inter-State (IGST) </asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                        </table>

                        <div style="text-align: center; margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;">
                            <asp:Label ID="lblStep1Error" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label><br />
                            <asp:Button ID="btnNextToProd" runat="server" Text="Next: Select Products >>" CssClass="btn_style" OnClick="btnNextToProd_Click" Width="220px" Height="35px" Font-Size="13px" />
                        </div>
                    </div>
                </asp:View>

                <asp:View ID="vProducts" runat="server">
                    <div style="padding: 10px 20px;">

                        <div style="background-color: #f8f9fa; padding: 15px; border: 1px solid #ddd; border-radius: 5px; margin-bottom: 10px;">
                            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;">
                                <div>
                                    <strong>Select Product Category:</strong>
                                    <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style" Width="200px" AutoPostBack="true" OnSelectedIndexChanged="cmbproduct_service_SelectedIndexChanged"></asp:DropDownList>
                                    <asp:Button ID="Button3" runat="server" Text="Load Category" CssClass="btn_style" OnClick="Button3_Click" />
                                </div>
                                <div style="border-left: 1px solid #ccc; padding-left: 15px;">
                                    <strong>Global ERP Search:</strong>
                                    <asp:TextBox ID="txtSearchProduct" runat="server" CssClass="textbox_style" Width="150px" placeholder="Name, HSN, ID..."></asp:TextBox>
                                    <asp:Button ID="btnSearchProduct" runat="server" Text="🔍 Find" CssClass="btn_style" OnClick="btnSearchProduct_Click" />
                                    <asp:Button ID="btnClearSearch" runat="server" Text="✖ Clear" CssClass="btn_style" OnClick="btnClearSearch_Click" BackColor="#777" BorderColor="#555" />
                                </div>
                            </div>

                            <hr style="border: 0; border-top: 1px solid #eee; margin: 10px 0;" />

                            <div style="display: flex; justify-content: space-between; align-items: center;">
                                <div>
                                    <strong>⚡ Quick Filter:</strong>
                                    <input type="text" id="txtQuickFilter" onkeyup="FilterGrid()" placeholder="Type to filter list..." style="width: 180px; padding: 3px; border: 1px solid #ccc;" />
                                    <span style="margin-left: 10px;">Show:</span>
                                    <input type="button" value="All" onclick="ShowRows('all')" style="padding: 3px 8px; cursor: pointer;" />
                                    <input type="button" value="Selected" onclick="ShowRows('checked')" style="padding: 3px 8px; cursor: pointer; background-color: #e8f5e9;" />
                                </div>
                                <div style="text-align: right;">
                                    <span style="background-color: #19658A; color: white; padding: 5px 10px; border-radius: 4px; font-weight: bold;">
                                        <span id="lblSelectedCount">0</span> Selected
                                    </span>
                                    &nbsp; | &nbsp;
                                    Rows:
                                    <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" CssClass="dropdown_style" Width="60px">
                                        <asp:ListItem Text="50" Value="50"></asp:ListItem>
                                        <asp:ListItem Text="100" Value="100"></asp:ListItem>
                                        <asp:ListItem Text="200" Value="200" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="500" Value="500"></asp:ListItem>
                                        <asp:ListItem Text="1000" Value="1000"></asp:ListItem>
                                        <asp:ListItem Text="2500" Value="2500"></asp:ListItem>
                                        <asp:ListItem Text="5000" Value="5000"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <asp:Panel ID="Panel2" runat="server" ScrollBars="Vertical" Height="450px" BorderStyle="Solid" BorderWidth="1px" BorderColor="#ccc">
                            <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" Width="100%"
                                AllowPaging="True" PageSize="200" OnPageIndexChanging="gridProdWithCat_PageIndexChanging">
                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                <AlternatingRowStyle BackColor="White" />
                                <RowStyle BackColor="#94B8FF" />
                                <PagerStyle BackColor="#E8F3FF" ForeColor="#006699" HorizontalAlign="Right" />
                                <PagerSettings Mode="NumericFirstLast" PageButtonCount="10" FirstPageText="First" LastPageText="Last" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Select">
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" />
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" />
                                        </ItemTemplate>
                                        <HeaderStyle Width="30px" />
                                    </asp:TemplateField>

                                    <asp:TemplateField Visible="false">
                                        <ItemTemplate>
                                            <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Product Name" HeaderStyle-Width="25%">
                                        <ItemTemplate>
                                            <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Spec" HeaderStyle-Width="15%">
                                        <ItemTemplate>
                                            <asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="HSN" HeaderStyle-Width="8%">
                                        <ItemTemplate>
                                            <asp:Label ID="lblHSN" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Unit" HeaderStyle-Width="5%">
                                        <ItemTemplate>
                                            <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Rate (RS)" HeaderStyle-Width="8%">
                                        <ItemTemplate>
                                            <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="textbox_style center" Width="90%" onkeypress="return validate(event, this)"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="GST %" HeaderStyle-Width="5%">
                                        <ItemTemplate>
                                            <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Disc %" HeaderStyle-Width="5%">
                                        <ItemTemplate>
                                            <asp:TextBox ID="Discount_Rate" runat="server" Text="0" CssClass="textbox_style center" Width="90%" onkeypress="return validate(event, this)"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Stock" HeaderStyle-BackColor="Green" ItemStyle-BackColor="LightGreen" HeaderStyle-Width="6%">
                                        <ItemTemplate>
                                            <asp:Label ID="SQuantity" runat="server" Text='<%# Bind("Quantity") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Qty" HeaderStyle-Width="6%">
                                        <ItemTemplate>
                                            <asp:TextBox ID="IQuantity" runat="server" CssClass="textbox_style center" Width="90%" Text="1" onkeypress="return validate(event, this)"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Remarks" HeaderStyle-Width="10%">
                                        <ItemTemplate>
                                            <asp:TextBox ID="ItemRemarks" runat="server" CssClass="textbox_style center" Width="95%"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>

                        <div class="nav-buttons">
                            <asp:Button ID="btnBackToSetup" runat="server" Text="<< Back" CssClass="btn_style" OnClick="btnBackToSetup_Click" BackColor="#777" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnAddAndStay" runat="server" Text="Add Selected & Continue" CssClass="btn_style" OnClick="btnAddProduct_Click" Width="200px" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnReview" runat="server" Text="Review & Save >>" CssClass="btn_style" OnClick="btnReview_Click" BackColor="Green" />
                            <br />
                            <asp:Label ID="lblStep2Msg" runat="server" Font-Bold="true"></asp:Label>
                        </div>
                    </div>
                </asp:View>

                <asp:View ID="vReview" runat="server">
                    <div style="padding: 20px;">
                        <h3 style="color: #19658A;">3. Final Review</h3>
                        <p style="color: #666;">Review quantities and rates before generating the invoice.</p>

                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" Width="100%">
                            <RowStyle BackColor="#94B8FF" />
                            <HeaderStyle BackColor="#19658A" Font-Bold="True" ForeColor="White" />
                            <Columns>
                                <asp:BoundField DataField="ProductName" HeaderText="Product" />
                                <asp:BoundField DataField="Brand" HeaderText="Specification" />
                                <asp:TemplateField HeaderText="Invoice Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="IQuantity" runat="server" Text='<%# Bind("SQuantity") %>' Width="50px" CssClass="center"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' Width="70px" CssClass="center"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Tax %">
                                    <ItemTemplate>
                                        <asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Disc %">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Discount_Rate" runat="server" Text='<%# Bind("Discount_Rate") %>' Width="50px" CssClass="center"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField Visible="false">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductId") %>'></asp:Label>
                                        <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                        <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                        <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                        <div class="nav-buttons">
                            <asp:Button ID="btnBackToProd" runat="server" Text="<< Add More Products" CssClass="btn_style" OnClick="btnBackToProd_Click" BackColor="#777" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btn_finalsave" runat="server" CssClass="btn_style" Width="250px" Text="Generate Proforma Invoice" OnClick="btn_finalsave_Click" />
                        </div>

                        <div style="text-align: center; padding: 10px;">
                            <asp:Panel ID="PanelMsg" runat="server" Visible="false">
                                <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Font-Size="Large"></asp:Label>
                            </asp:Panel>
                        </div>
                    </div>
                </asp:View>

            </asp:MultiView>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
