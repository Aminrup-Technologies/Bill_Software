<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Edit_quatation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm65" %>
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
         .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
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
    function ValidateDelete1() {
        var answer = confirm("Want to Delete this Quotation?");
        if (!answer) {
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
                    <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" onclick="btnSertch_Click" Text="Search" />
                    &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" onclick="btnreset_Click" Text="Reset" />
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
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="Label2" runat="server" Text="Client Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="showid" runat="server" Text="Quotation Number"></asp:Label>
                                </td>

                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label6" runat="server" Text="Product Catagory"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label7" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label9" runat="server" Text="GST (INR)"></asp:Label>
                                </td>

                               
                                <td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label1" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                </td>

                                <td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label5" runat="server" Text="Last Mailer Date"></asp:Label>
                                </td>

                                <td style="text-align:center; width:5%;">
                                    <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
                                </td>
                                <td style="text-align:center; width:4%;">
                                    <asp:Label ID="Label13" runat="server" Text="Edit"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                  <td style="text-align:center; width:25%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>


                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label12" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                </td>

                               
                                <td style="text-align:center; width:8%;">Rs. 
                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-
                                </td>

                                 <td style="text-align:center; width:8%;"> 
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("mailStatusDate") %>'></asp:Label>
                                </td>

                              

                               <%-- <td style="text-align:center; width:5%;">
                                    <asp:ImageButton ID="ImageButton1" runat="server" CommandName="View" CommandArgument='<%# Eval("ID") %>' 
                                        ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View"/>
                                </td>

                                <td style="text-align:center; width:4%;">
                                   <asp:ImageButton ID="ImageButton2" runat="server" CommandName="Delete" CommandArgument='<%# Eval("Quotation_no") %>' 
                                        ImageUrl="~/corporate/business/WebImages/delete.png" ToolTip="Delete" onclientclick="return ValidateDelete1();"/>
                                </td>
                                --%>

                                <td style="text-align:center; width:5%;">
                                    <a href = "#" title="Print Quotation..." onclick="window.open('/corporate/business/print/NewQuotation.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                </td>
                                <td style="text-align:center; width:4%;">
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
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%"><asp:Label ID="Label1" runat="server" Text="1" Visible="False"></asp:Label>
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
                            <td width="35%">
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%" colspan="2" style="width: 70%; text-align: center">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" OnClick="Button2_Click" onclientclick="return ValidateDataField10();" Text="Click to Retrieve Product &/or Service from the selected Category" Width="400px" />
                            </td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>

                        <tr>
                            <td colspan="4">
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
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
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
                               <td style="text-align:center" colspan="2"><asp:Button ID="btnAddProduct" runat="server" CssClass="btn_style" OnClick="btnAddProduct_Click" Text="Add Required Product &/or Service  against the Selected Category from the above Table" Width="500px"/></td>
                               <td colspan="2" style="text-align:center; color:red; font-weight:bold;">Go back to the Select Product/Service Category in case more Product/Service Categories need to be added</td>
                        </tr>

                           <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
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

                                       
                                        
                                        <asp:TemplateField HeaderText="Base Rate (RS)">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Sail_Rate" runat="server" Text='<%# Bind("Sail_Rate") %>' BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" Height="22px"  onkeypress="return validate(event)"></asp:TextBox>
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
                                                     <asp:CheckBox ID="checkAll" runat="server" checked="true" onclick = "checkAll(this);" />
                                                </HeaderTemplate>

                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chk" runat="server" checked="true" onclick = "Check_Click(this)" />
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

                            </td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">Discount or Inflation
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
                            <td width="35%"><asp:TextBox ID="txtPercentage" runat="server" Text="0" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" CssClass="textbox_style21" ></asp:TextBox></td>
                            <td width="15%">&nbsp;</td>
                        </tr>

                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">&nbsp;</td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>

                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%" colspan="2" style="width: 70%; text-align: center">
                                &nbsp;<asp:Button ID="btnSabe" runat="server" CssClass="btn_style" OnClick="btnSabe_Click" Text="Save" />
&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
                            </td>
                            <td width="35%">&nbsp;</td>
                            <td width="15%">&nbsp;</td>
                        </tr>
                        <tr>
                            <td width="15%">&nbsp;</td>
                            <td width="35%">
                                &nbsp;
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
 
                <asp:PostBackTrigger ControlID="btnSabe"  />
                <asp:PostBackTrigger ControlID="Button2"  />
             <asp:PostBackTrigger ControlID="btnAddProduct"  />
             
             
            </Triggers>
    </asp:UpdatePanel>
</asp:Content>
