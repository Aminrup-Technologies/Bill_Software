<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="RequisitionManual.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm71" %>
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
        .auto-style2 {
            width: 100px;
            background: url('../../WebProperty/images/btn_bg.jpg') repeat-x 0 0;
            border: 1px solid #7a8912;
            font: bold 11px/24px Arial, Helvetica, sans-serif;
            color: #fff;
            -webkit-border-radius: 3px;
            -moz-border-radius: 3px;
            border-radius: 3px;
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
        if (document.getElementById('<%=cmbClient.ClientID%>').selectedIndex == 0) {
            alert("Please Select Client Name.");
            document.getElementById('<%=cmbClient.ClientID%>').focus();
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



    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
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
            <td>Vendor</td>
            <td>
                <asp:TextBox ID="txtVendor" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Requisition Date&nbsp;</td>
            <td><asp:TextBox ID="txtquotationDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
                        </td>
            <td>
                        </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Gst Rate</td>
            <td>
                <asp:DropDownList ID="cmdGst" runat="server" CssClass="dropdown_style">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>14.5</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>28</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Cheque No</td>
            <td>
                <asp:TextBox ID="txtCheckno" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Issue Date</td>
            <td>
                <asp:TextBox ID="txtIssueDate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Bank</td>
            <td>
                <asp:TextBox ID="txtBankName" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>IFS Code</td>
            <td>
                <asp:TextBox ID="txtIFSCode" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
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
            <td>&nbsp;</td>
            <td>Description</td>
            <td>
                <asp:TextBox ID="txtDescription" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Size</td>
            <td>
                <asp:TextBox ID="txtSize" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px"  Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Qnty</td>
            <td>
                <asp:TextBox ID="txtQnty" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>Rate</td>
            <td>
                <asp:TextBox ID="txtRate" runat="server" BorderColor="Black" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="320px"></asp:TextBox>
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
            <td colspan="2" style="text-align: center">
                <asp:Button ID="Button4" runat="server" Text="Add Product" CssClass="btn_style" OnClick="Button4_Click"/>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <%--<asp:GridView ID="DynamicGrid" runat="server"></asp:GridView>--%>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2" style="width:70%">
                <asp:GridView ID="repredataGrid" runat="server" CssClass="mydatagrid" CellPadding="10" CellSpacing="14"
                                    AllowPaging="True" PagerStyle-CssClass="pager" HeaderStyle-CssClass="header"
                                    HorizontalAlign="Center" PageSize="9" RowStyle-CssClass="rows"
                                    OnPageIndexChanging="repredataGrid_PageIndexChanging"
                                    OnRowCommand="repredataGrid_RowCommand" OnRowDeleting="repredataGrid_RowDeleting" OnRowDataBound="repredataGrid_RowDataBound">

                                    <Columns>
                                        <asp:TemplateField HeaderText="Remove" ShowHeader="False">
                                            <ItemTemplate>
                                                <asp:ImageButton ID="ImgbtnCancelOrder" runat="server" CausesValidation="false"
                                                    ImageUrl="../WebImages/delete.png" OnClientClick="Javascript: return confirm('Are you sure to delete row?');" CommandName="Delete"
                                                    CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
               </asp:GridView>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="4">
               
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="2" style="text-align:center">
                <asp:Button ID="btnSave" runat="server" CssClass="auto-style2" Text="Save" OnClick="btnSave_Click" />
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

        </ContentTemplate>
        <%-- <Triggers>
 
                <asp:PostBackTrigger ControlID="Button1"  />
                <asp:PostBackTrigger ControlID="Button2" />
                <asp:PostBackTrigger ControlID="Button3" />
                
            </Triggers>--%>
    </asp:UpdatePanel>
</asp:Content>
