<%@ Page Title="Direct Proforma" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Direct_Proforma.aspx.cs" Inherits="Bill_Software.corporate.business.app.Direct_Proforma" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        /* General Grid Styling */
        .Grid td { text-align: center; font-size: 11px; padding: 6px 4px; border-color: #ddd; border-width: 1px; border-style: solid; vertical-align:middle; }
        .Grid th { padding: 8px 4px; }
        .center { text-align: center; }
        .textbox_style { border: 1px solid #ccc; padding: 4px; border-radius: 3px; font-size:11px; }
        .textbox_style:focus { border-color:#19658A; outline:none; background-color:#fdfdfd; }
        
        .btn_style { background-color: #19658A; color: white; border: 1px solid #104e6b; padding: 6px 15px; cursor: pointer; border-radius: 3px; font-weight:bold; }
        .btn_style:hover { background-color: #124d6b; }
        .dropdown_style { padding: 4px; border: 1px solid #ccc; border-radius:3px; }

        /* Wizard Steps */
        .step-container { display: flex; justify-content: space-between; margin-bottom: 20px; padding: 15px 50px; background-color: #fff; border-bottom: 1px solid #eee; }
        .step-item { flex: 1; text-align: center; padding: 10px; border-bottom: 3px solid #e0e0e0; color: #aaa; font-weight: bold; font-size: 14px; cursor:default; }
        .step-item.active { border-bottom-color: #19658A; color: #19658A; }
        .step-item.completed { border-bottom-color: #28a745; color: #28a745; }
        
        .form-section { margin: 0 auto; width: 95%; background: #fff; padding: 25px; border: 1px solid #eee; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
        .nav-buttons { margin-top: 20px; text-align: center; padding: 15px; background: #f9f9f9; border-top: 1px solid #ddd; }
        
        /* Calculated Fields Highlight */
        .calc-text { font-weight:bold; color:#444; display:block; }
        .calc-taxable { font-weight:bold; color:#006600; background-color:#e8f5e9; padding:2px 5px; border-radius:3px; }
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
            UpdateCounter(); 
            RecalculateAll(); // Recalculate totals whenever page/panel loads
        });

        function validate(key, element) {
            var keycode = (key.which) ? key.which : key.keyCode;
            if (!(keycode == 8 || keycode == 46) && (keycode < 48 || keycode > 57) && keycode != 46) return false;
            return true;
        }

        // --- 1. Selection & Filtering (Step 2) ---
        function UpdateCounter() {
            var count = 0;
            var grid = document.getElementById("<%= gridProdWithCat.ClientID %>");
            if (grid) {
                var inputs = grid.getElementsByTagName("input");
                for (var i = 0; i < inputs.length; i++) {
                    if (inputs[i].type == "checkbox" && inputs[i].id.indexOf("checkAll") == -1 && inputs[i].checked) count++;
                }
            }
            var lbl = document.getElementById("lblSelectedCount");
            if(lbl) lbl.innerText = count;
        }

        function Check_Click(objRef) {
            var row = objRef.parentNode.parentNode;
            row.style.backgroundColor = objRef.checked ? "#d4edda" : (row.rowIndex % 2 == 0 ? "#f2f2f2" : "white");
            UpdateCounter();
        }

        function checkAll(objRef) {
            var GridView = objRef.parentNode.parentNode.parentNode;
            var inputList = GridView.getElementsByTagName("input");
            for (var i = 0; i < inputList.length; i++) {
                var row = inputList[i].parentNode.parentNode;
                if (inputList[i].type == "checkbox" && objRef != inputList[i]) {
                    inputList[i].checked = objRef.checked;
                    row.style.backgroundColor = objRef.checked ? "#d4edda" : (row.rowIndex % 2 == 0 ? "#f2f2f2" : "white");
                }
            }
            UpdateCounter();
        }

        function FilterGrid() {
            var input = document.getElementById("txtQuickFilter");
            var filter = input.value.toUpperCase();
            var table = document.getElementById("<%= gridProdWithCat.ClientID %>");
            var tr = table.getElementsByTagName("tr");
            for (var i = 1; i < tr.length; i++) {
                var tds = tr[i].getElementsByTagName("td");
                // Indices depend on VISIBLE columns. Hidden columns are NOT in DOM.
                // Assuming [0]=Chk, [1]=Name, [2]=Spec, [3]=HSN, [4]=Unit...
                if (tds.length > 3) {
                    var txtName = tds[1].textContent || tds[1].innerText;
                    var txtSpec = tds[2].textContent || tds[2].innerText;
                    var txtHSN = tds[3].textContent || tds[3].innerText;
                    if (txtName.toUpperCase().indexOf(filter) > -1 || txtSpec.toUpperCase().indexOf(filter) > -1 || txtHSN.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = "";
                    } else {
                        tr[i].style.display = "none";
                    }
                }
            }
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

        // --- 2. Instant Math Calculations (Step 3) ---
        function CalculateRow(inputElement) {
            var row = inputElement.closest("tr");
            
            // Get Values safely
            var qty = parseFloat(row.querySelector(".js-qty").value) || 0;
            var rate = parseFloat(row.querySelector(".js-rate").value) || 0;
            var discPer = parseFloat(row.querySelector(".js-disc-per").value) || 0;
            
            // Logic
            var gross = qty * rate;
            var discAmt = (gross * discPer) / 100;
            var taxable = gross - discAmt;

            // Update UI Spans
            row.querySelector(".js-gross").innerText = gross.toFixed(2);
            row.querySelector(".js-disc-amt").innerText = discAmt.toFixed(2);
            row.querySelector(".js-taxable").innerText = taxable.toFixed(2);
        }

        function RecalculateAll() {
            var grid = document.getElementById("<%= gd_Service_Product.ClientID %>");
            if (grid) {
                var rows = grid.getElementsByTagName("tr");
                for (var i = 1; i < rows.length; i++) {
                    var input = rows[i].querySelector(".js-qty"); // Find input to trigger
                    if (input) CalculateRow(input);
                }
            }
        }
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            
            <div style="background-color:#19658A; color:white; padding:12px; font-weight:bold; font-size:16px; border-radius:4px 4px 0 0;">
                <img src="../../WebImages/Plus_icon.png" style="vertical-align:middle; width:20px; filter: brightness(0) invert(1);" /> 
                Create Direct Proforma Invoice
            </div>

            <div class="step-container">
                <div id="step1" runat="server" class="step-item active">1. Client & Setup</div>
                <div id="step2" runat="server" class="step-item">2. Select Products</div>
                <div id="step3" runat="server" class="step-item">3. Review & Save</div>
            </div>

            <asp:MultiView ID="mvInvoice" runat="server" ActiveViewIndex="0">
                
                <asp:View ID="vSetup" runat="server">
                    <div class="form-section">
                        <table style="width:100%; border-spacing: 10px; border-collapse: separate;">
                            <tr>
                                <td width="15%" style="vertical-align:top; padding-top:8px;"><strong>Select Client:</strong> <span style="color:red">*</span></td>
                                <td>
                                    <asp:DropDownList ID="cmbClient" runat="server" CssClass="dropdown_style" Width="350px" AutoPostBack="true" OnSelectedIndexChanged="cmbClient_SelectedIndexChanged"></asp:DropDownList>
                                    
                                    <asp:Panel ID="pnlClientInfo" runat="server" Visible="false" style="margin-top:10px; background-color:#f4fbfd; border:1px solid #b6e1ef; padding:12px; border-radius:4px; border-left:4px solid #19658A;">
                                        <table style="width:100%; font-size:11px; color:#444;">
                                            <tr>
                                                <td width="15%"><strong>ID:</strong> <asp:Label ID="lblclientID" runat="server"></asp:Label></td>
                                                <td width="25%"><strong>GST:</strong> <asp:Label ID="lblClientGST" runat="server" ForeColor="#d9534f" Font-Bold="true"></asp:Label></td>
                                                <td><strong>State:</strong> <asp:Label ID="lblClientState" runat="server" Font-Bold="true"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td colspan="3" style="padding-top:6px;"><strong>Address:</strong> <asp:Label ID="lblClientAddress" runat="server"></asp:Label></td>
                                            </tr>
                                            <tr>
                                                <td colspan="3" style="padding-top:6px;"><strong>Place of Supply:</strong> <asp:Label ID="lblPlaceOfSupply" runat="server" ForeColor="#0275d8" Font-Bold="true"></asp:Label> <span style="font-size:10px; color:#777;">(City)</span></td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </td>
                            </tr>
                            <tr>
                                <td><strong>Date:</strong> <span style="color:red">*</span></td>
                                <td><asp:TextBox ID="txtinvoiceDate" runat="server" CssClass="datepicker dropdown_style" Width="100px"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td><strong>Tax Type:</strong> <span style="color:red">*</span></td>
                                <td>
                                    <asp:RadioButtonList ID="RadioButtonGst" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="1" Selected="True"> Intra-State (CGST/SGST)</asp:ListItem>
                                        <asp:ListItem Value="0"> Inter-State (IGST)</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                        </table>
                        <div class="nav-buttons">
                            <asp:Label ID="lblStep1Error" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label><br/>
                            <asp:Button ID="btnNextToProd" runat="server" Text="Next: Select Products >>" CssClass="btn_style" OnClick="btnNextToProd_Click" Width="200px" Height="30px" />
                        </div>
                    </div>
                </asp:View>

                <asp:View ID="vProducts" runat="server">
                    <div style="padding:10px;">
                        
                        <div style="background-color:#f8f9fa; padding:10px; border:1px solid #ddd; border-radius:5px; margin-bottom:10px;">
                            <table style="width:100%">
                                <tr>
                                    <td>Category: <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style" Width="220px" AutoPostBack="true" OnSelectedIndexChanged="cmbproduct_service_SelectedIndexChanged"></asp:DropDownList></td>
                                    <td style="border-left:1px solid #ccc; padding-left:10px;">
                                        Global Search: <asp:TextBox ID="txtSearchProduct" runat="server" CssClass="textbox_style" Width="180px" placeholder="Name, ID, HSN..."></asp:TextBox>
                                        <asp:Button ID="btnSearchProduct" runat="server" Text="Find" CssClass="btn_style" OnClick="btnSearchProduct_Click" />
                                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn_style" OnClick="btnClearSearch_Click" BackColor="#777" BorderColor="#555" />
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:5px;">
                            <div>
                                <strong>⚡ Filter:</strong> <input type="text" id="txtQuickFilter" onkeyup="FilterGrid()" placeholder="Type to filter..." style="width:150px; padding:3px; border:1px solid #ccc;" />
                                <input type="button" value="All" onclick="ShowRows('all')" style="padding:2px 8px; cursor:pointer;" />
                                <input type="button" value="Selected" onclick="ShowRows('checked')" style="padding:2px 8px; cursor:pointer; background-color:#d4edda;" />
                            </div>
                            <div style="text-align:right;">
                                <span style="background-color:#19658A; color:white; padding:4px 8px; border-radius:3px; font-size:12px;">
                                    <span id="lblSelectedCount">0</span> Selected
                                </span>
                                &nbsp; Rows:
                                <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" CssClass="dropdown_style" Width="60px">
                                    <asp:ListItem Text="50" Value="50"></asp:ListItem>
                                    <asp:ListItem Text="100" Value="100"></asp:ListItem>
                                    <asp:ListItem Text="200" Value="200" Selected="True"></asp:ListItem>
                                    <asp:ListItem Text="500" Value="500"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <asp:Panel ID="Panel2" runat="server" ScrollBars="Vertical" Height="450px" BorderStyle="Solid" BorderWidth="1px" BorderColor="#ccc">
                            <asp:GridView ID="gridProdWithCat" runat="server" AutoGenerateColumns="False" 
                                BackColor="White" BorderColor="#eee" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" Width="100%"
                                AllowPaging="True" PageSize="200" OnPageIndexChanging="gridProdWithCat_PageIndexChanging">
                                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                <AlternatingRowStyle BackColor="White" />
                                <RowStyle BackColor="#f9f9f9" />
                                <PagerStyle BackColor="#e9ecef" ForeColor="#006699" HorizontalAlign="Right" />
                                <PagerSettings Mode="NumericFirstLast" PageButtonCount="10" FirstPageText="First" LastPageText="Last" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Select">
                                        <HeaderTemplate><asp:CheckBox ID="checkAll" runat="server" onclick="checkAll(this);" /></HeaderTemplate>
                                        <ItemTemplate><asp:CheckBox ID="chkdtp" runat="server" onclick="Check_Click(this)" /></ItemTemplate>
                                        <HeaderStyle Width="30px" />
                                    </asp:TemplateField>
                                    
                                    <asp:TemplateField HeaderText="Product Name">
                                        <ItemTemplate><asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label></ItemTemplate>
                                        <ItemStyle HorizontalAlign="Left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Spec"><ItemTemplate><asp:Label ID="Brand" runat="server" Text='<%# Bind("Brand") %>'></asp:Label></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="HSN"><ItemTemplate><asp:Label ID="lblHSN" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Unit"><ItemTemplate><asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Rate"><ItemTemplate><asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' CssClass="textbox_style center" Width="70px" onkeypress="return validate(event, this)"></asp:TextBox></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="GST %"><ItemTemplate><asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>'></asp:Label></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Disc %"><ItemTemplate><asp:TextBox ID="Discount_Rate" runat="server" Text="0" CssClass="textbox_style center" Width="40px" onkeypress="return validate(event, this)"></asp:TextBox></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Stock"><ItemTemplate><asp:Label ID="SQuantity" runat="server" Text='<%# Bind("Quantity") %>'></asp:Label></ItemTemplate><ItemStyle BackColor="#d4edda" /></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Qty"><ItemTemplate><asp:TextBox ID="IQuantity" runat="server" CssClass="textbox_style center" Width="50px" Text="1" onkeypress="return validate(event, this)"></asp:TextBox></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Remarks"><ItemTemplate><asp:TextBox ID="ItemRemarks" runat="server" CssClass="textbox_style" Width="90%"></asp:TextBox></ItemTemplate></asp:TemplateField>
                                    
                                    <asp:TemplateField Visible="false">
                                        <ItemTemplate>
                                            <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductID") %>'></asp:Label>
                                            <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                            <asp:Label ID="Type" runat="server" Text='<%# Bind("Type") %>'></asp:Label>
                                            <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>

                        <div class="nav-buttons">
                            <asp:Button ID="btnBackToSetup" runat="server" Text="<< Back" CssClass="btn_style" OnClick="btnBackToSetup_Click" BackColor="#777" BorderColor="#555" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnAddAndStay" runat="server" Text="Add Selected & Continue" CssClass="btn_style" OnClick="btnAddProduct_Click" Width="220px" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btnReview" runat="server" Text="Review & Save >>" CssClass="btn_style" OnClick="btnReview_Click" BackColor="Green" BorderColor="DarkGreen" />
                            <br /><asp:Label ID="lblStep2Msg" runat="server" Font-Bold="true"></asp:Label>
                        </div>
                    </div>
                </asp:View>

                <asp:View ID="vReview" runat="server">
                    <div style="padding: 20px;">
                        <h3 style="color:#19658A; margin-top:0;">3. Final Review & Edit</h3>
                        
                        <asp:GridView ID="gd_Service_Product" runat="server" AutoGenerateColumns="False" 
                            Width="100%" CellPadding="5" CssClass="Grid" 
                            OnRowCommand="gd_Service_Product_RowCommand" 
                            OnRowDeleting="gd_Service_Product_RowDeleting">
                            
                            <HeaderStyle BackColor="#19658A" Font-Bold="True" ForeColor="White" />
                            <RowStyle BackColor="#fcfcfc" />
                            <Columns>
                                <asp:TemplateField HeaderText="Action" ItemStyle-Width="30px">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="btnDelete" runat="server" CommandName="Delete" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" ToolTip="Remove Item" Width="16px" OnClientClick="return confirm('Remove this item?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Order" ItemStyle-Width="40px">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnUp" runat="server" CommandName="MoveUp" CommandArgument='<%# Container.DataItemIndex %>' Text="▲" style="text-decoration:none; color:#19658A; font-weight:bold;"></asp:LinkButton>
                                        <asp:LinkButton ID="btnDown" runat="server" CommandName="MoveDown" CommandArgument='<%# Container.DataItemIndex %>' Text="▼" style="text-decoration:none; color:#19658A; font-weight:bold;"></asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ProductName" HeaderText="Product" />
                                <asp:BoundField DataField="Brand" HeaderText="Spec" />
                                
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="IQuantity" runat="server" Text='<%# Bind("SQuantity") %>' Width="50px" CssClass="center js-qty textbox_style" onkeyup="CalculateRow(this)" autocomplete="off"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' Width="70px" CssClass="center js-rate textbox_style" onkeyup="CalculateRow(this)" autocomplete="off"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Gross"><ItemTemplate><span class="js-gross calc-text">0.00</span></ItemTemplate><ItemStyle HorizontalAlign="Right"/></asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Disc %">
                                    <ItemTemplate>
                                        <asp:TextBox ID="Discount_Rate" runat="server" Text='<%# Bind("Discount_Rate") %>' Width="40px" CssClass="center js-disc-per textbox_style" onkeyup="CalculateRow(this)" autocomplete="off"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Disc Amt"><ItemTemplate><span class="js-disc-amt" style="color:red;">0.00</span></ItemTemplate><ItemStyle HorizontalAlign="Right"/></asp:TemplateField>
                                <asp:TemplateField HeaderText="Taxable Val"><ItemTemplate><span class="js-taxable calc-taxable">0.00</span></ItemTemplate><ItemStyle HorizontalAlign="Right"/></asp:TemplateField>

                                <asp:TemplateField HeaderText="Tax %">
                                    <ItemTemplate><asp:Label ID="Tax_Rate" runat="server" Text='<%# Bind("Tax_Rate") %>' CssClass="js-tax-per"></asp:Label></ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField Visible="false">
                                    <ItemTemplate>
                                        <asp:Label ID="ProductID" runat="server" Text='<%# Bind("ProductId") %>'></asp:Label>
                                        <asp:Label ID="Product_code" runat="server" Text='<%# Bind("Product_code") %>'></asp:Label>
                                        <asp:Label ID="ProductName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                                        <asp:Label ID="Unit" runat="server" Text='<%# Bind("Unit") %>'></asp:Label>
                                        <asp:Label ID="ProductOrServiceCat" runat="server" Text='<%# Bind("ProductOrServiceCat") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                        <div class="nav-buttons">
                            <asp:Button ID="btnBackToProd" runat="server" Text="<< Add More Products" CssClass="btn_style" OnClick="btnBackToProd_Click" BackColor="#777" BorderColor="#555" />
                            &nbsp;&nbsp;
                            <asp:Button ID="btn_finalsave" runat="server" Text="Generate Proforma Invoice" CssClass="btn_style" Width="250px" OnClick="btn_finalsave_Click" />
                        </div>
                        
                        <div style="text-align:center; padding:10px;">
                            <asp:Panel ID="PanelMsg" runat="server" Visible="false" style="padding:10px; border:1px solid #ddd; display:inline-block;">
                                <asp:Label ID="lblMessage" runat="server" Font-Bold="true" Font-Size="Large"></asp:Label>
                            </asp:Panel>
                        </div>
                    </div>
                </asp:View>

            </asp:MultiView>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>