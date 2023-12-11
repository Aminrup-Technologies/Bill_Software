<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Vendor_quotation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm89" %>
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
             width: 100%;
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
         .redio
          {
             border:none;
         }
   
        .auto-style2 {
            width: 40%;
        }
   
        .auto-style3 {
            height: 24px;
        }
   
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.core.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.widget.js" type="text/javascript" language="javascript"></script>
	<script src="calender/jquery.ui.datepicker.js" type="text/javascript" language="javascript"></script>
    <script type="text/javascript" src="top_scroll/jquery.min.js"></script>
    <script type="text/javascript" src="top_scroll/autocomplete.js"></script>
	
<link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
<script type="text/javascript" language="javascript">
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function () {
        $(".datepicker").datepicker({
            dateFormat: 'dd-M-yy',
            changeMonth: true,
            changeYear: true
        });
    });
	</script>
     <script type="text/javascript">
       <%-- function AutoComplete() {
            var names = $('#<%= hdnNames.ClientID %>').val();
            if (names != null && names != "") {
                var arrOfNames = names.split("?");
                $("#<%=gst.ClientID %>").autocomplete({
                    source: arrOfNames
                });
            }
        }--%>
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
<script type="text/javascript">
    function ValidateDataField10() {

 }
</script>
<asp:ScriptManager ID="ScriptManager1" runat="server">
</asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
     <table class="style1">
   <tr>
        <td width="10%">
            &nbsp;</td>
        <td colspan="2" width="40%">
            <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>
        </td>
        <td colspan="2" width="40%">
            &nbsp;</td>
        <td width="10%">
            &nbsp;</td>
    </tr>
    <tr>
        <td>
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
            <asp:Label ID="Label2" runat="server" Text="1" Visible="False"></asp:Label>
        </td>
    </tr>
    <tr>
        <td class="auto-style3">
            </td>
        <td width="15%" class="auto-style3">
            <span class="style3"></span>Client Name&nbsp;</td>
        <td width="25%" class="auto-style3">
            <asp:DropDownList ID="cmbclient" runat="server" AutoPostBack="True" CssClass="dropdown_style">
            </asp:DropDownList>
        </td>
        <td class="auto-style3">
            </td>
    </tr>
          <tr>
                            <td>&nbsp;</td>
                            <td colspan="2" style="text-align:center">
                                <asp:Button ID="Button2" runat="server" CssClass="btn_style" Text="Select Quotation from I2I Software" OnClick="Button2_Click" Width="200px"/>
                            </td>
                            <td>&nbsp;</td>
                        </tr>
          <tr>
              <td>&nbsp;</td>
              <td colspan="2" style="text-align:center">&nbsp;</td>
              <td>&nbsp;</td>
         </tr>
          <tr>
              <td colspan="6">
                  <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%"  OnItemCommand="DataList1_ItemCommand">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showid0" runat="server" Text="Quotation no"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showrm0" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label1" runat="server" Text="Product/Service Category"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label12" runat="server" Text="Amount Before GST(INR)"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label3" runat="server" Text="GST(INR)"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label4" runat="server" Text="Amount inclusive of GST(INR)"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="edit0" runat="server" Text="Select"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="ID0" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="addshowname0" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                 </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                 </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label6" runat="server" Text='<%# Eval("Gst") %>'></asp:Label>
                                 </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("Net_Amount") %>'></asp:Label>
                                 </td>
                                 <td style="text-align:center; width:15%;">

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
             <td colspan="6">&nbsp;</td>
         </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                <asp:Panel ID="Panel3" runat="server" Visible="false">
                <table class="auto-style1">
                    <tr>
                        <td>Quotation No</td>
                        <td>
                            <asp:Label ID="lblQuotation_no" runat="server"></asp:Label>
                        </td>
                        <td>Quotation Date</td>
                        <td>
                            <asp:Label ID="lblQuotation_date" runat="server"></asp:Label>
                        </td>
                        <%--<td>Client Name</td>
                        <td>
                            <asp:Label ID="lblClientName" runat="server"></asp:Label>
                        </td>--%>
                    </tr>
                    <tr>
                        <td>Amount Without GST</td>
                        <td>
                            <asp:Label ID="amtwithoutgst" runat="server"></asp:Label>
                        </td>
                        <td>GST Amount</td>
                        <td>
                            <asp:Label ID="amountgst" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>Amount inclusive of GST</td>
                        <td>
                            <asp:Label ID="amtwith_gst" runat="server" Visible="False"></asp:Label>
                            <asp:Label ID="amtwithgst" runat="server"></asp:Label>
                        </td>
                     </tr>
               </table>
                    </asp:Panel>
         <tr>
            <td>&nbsp;</td>
            <td>Select Vendor</td>
            <td>
                <asp:DropDownList ID="cmbvendor" runat="server" AutoPostBack="True" CssClass="dropdown_style" Width="155px" OnSelectedIndexChanged="gst_TextChanged">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>

       <tr>
        <td width="25%">
            &nbsp;</td>
        <td width="15%">
            <span>Vendor GST No</span></td>
           <td>
            <asp:TextBox ID="gst" runat="server" CssClass="textbox_style" Enabled="True" Width="155px"></asp:TextBox>
           <%--  <asp:HiddenField ID="hdnNames" runat="server" />--%>
        </td>
    </tr>
            <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                 <asp:Panel ID="Panel4" runat="server" Visible="false">
                 <table class="auto-style1">
                    <tr>
                        <td>Add Vendor Quotation or PI Number</td>
                        <td>
                            <asp:Textbox ID="pinumber" runat="server"></asp:Textbox>
                        </td>
                        <td>Add Vendor Quotation or PI Date</td>
                        <td>
                            <asp:TextBox ID="pidate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Vendor Quotation or PI Amount without GST</td>
                        <td>
                            <asp:Textbox ID="piamtwithoutgst" runat="server"></asp:Textbox>
                        </td>
                        <td>Vendor Quotation or PI GST Amount</td>
                        <td>
                            <asp:Textbox ID="pigstamount" runat="server" ></asp:Textbox>
                        </td>
                    </tr>
                 </table>
                    </asp:Panel>
                <tr>
            <td>&nbsp;</td>
            <td colspan="4">
                 <asp:Panel ID="Panel5" runat="server" Visible="false">
                 <table class="auto-style1">
                    <tr>
                        <td>Add Vendor Non Registered Bill</td>
                        <td>
                            <asp:Textbox ID="nonrgstbill" runat="server"></asp:Textbox>
                        </td>
                        <td>Add Vendor Non Registered Bill Date</td>
                        <td>
                            <asp:TextBox ID="nonrgstbilldate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>Vendor Non Registered Bill Amount</td>
                        <td>
                            <asp:Textbox ID="nonrgstamt" runat="server"></asp:Textbox>
                        </td>
                        <td>Vendor Quotation or PI GST Amount</td>
                        <td>
                            <asp:Textbox ID="pigstamt" runat="server"></asp:Textbox>
                        </td>
                    </tr>
                 </table>
                    </asp:Panel>
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
&nbsp;
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
        <td colspan="6">
            <asp:Panel ID="Panel1" runat="server" Visible="False">
                <table class="auto-style1">
                    <tr>
                        <td style="width:10%;">&nbsp;</td>
                        <td style="width:10%;">&nbsp;</td>
                    </tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td class="auto-style2">&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </asp:Panel>
        </td>
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

</ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>


