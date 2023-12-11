<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="general_expences.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm43" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .style2
        {
            width: 100%;
        }
        .style3
        {
            color: #FFFFFF;
            font-weight: bold;
        }
        .style4
        {
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
	</script>
	
    <script type="text/javascript">
        function ValidateDelete() {
            if (document.getElementById('<%=cmbexpenceshead.ClientID%>').selectedIndex == 0) {
            alert("Please Select Expences head");
            document.getElementById('<%=cmbexpenceshead.ClientID%>').focus();
        return false;
    }

    

    }
</script>
 <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
    <table class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp;<span class="style3">General Expenses</span>&nbsp;</td>
        </tr>
        <tr>
            <td width="10%">
                &nbsp;</td>
                                <td width="40%">
                                    &nbsp;</td>
                                <td width="40%">
                                    &nbsp;</td>
                                <td width="10%">
                                    &nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    &nbsp;</td>
                                <td colspan="2">
                                    <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                                        BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                                        &nbsp;<asp:Image ID="imageTick" runat="server" 
                                            ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                                        &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                                    </asp:Panel>
                                </td>
                                <td>
                                    &nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;</td>
                            </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Expense Head&nbsp;</td>
            <td>
                <asp:DropDownList ID="cmbexpenceshead" runat="server" CssClass="dropdown_style">
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Payment Date&nbsp;</td>
            <td>
                <asp:TextBox ID="txtpaymetdate" runat="server" BorderColor="#CCCCCC" 
                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                    Width="110px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
                                <td>
                                    &nbsp;Payment Made To</td>
                                <td>
                                    <asp:TextBox ID="txtpaymentmadeto" runat="server" CssClass="textbox_style"></asp:TextBox>
            </td>
                                <td>
                                    &nbsp;</td>
                            </tr>
                            <tr>
                                <td>
                                    &nbsp;</td>
                                <td>
                                    &nbsp;Amount&nbsp;</td>
            <td>
                <asp:TextBox ID="txtamount" runat="server" CssClass="textbox_style"></asp:TextBox>
                                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Payment Mode&nbsp;</td>
            <td>
                <asp:RadioButtonList ID="RadioButtonList1" runat="server" AutoPostBack="True" 
                    onselectedindexchanged="RadioButtonList1_SelectedIndexChanged" 
                    RepeatDirection="Horizontal">
                    <asp:ListItem Selected="True">Cash</asp:ListItem>
                    <asp:ListItem>Cheque</asp:ListItem>
                    <asp:ListItem>DD</asp:ListItem>
                    <asp:ListItem>Online Transaction</asp:ListItem>
                    <asp:ListItem>Credit Card</asp:ListItem>
                </asp:RadioButtonList>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
            <div style="width:100%;" id="First" runat="server" visible="true">
                                Dated:<asp:TextBox ID="txtcashDate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
&nbsp;</div>
                            <div id="Second" runat="server" visible="false" class="style2">
                                Cheque/DD No.<asp:TextBox ID="txtDDno" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Drawee Bank&nbsp;
                                <asp:TextBox ID="txtBankName" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Dated:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtdddate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
                            </div>
                            <div style="width:100%;" id="Third" runat="server" visible="false">
                                NEFT Reference Number: <asp:TextBox ID="txtneftnumber" runat="server" 
                                    CssClass="textbox_style"></asp:TextBox>
                                <br />
                                From Account :&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtbankname1" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Dated:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtneftdate" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
                            </div>
                            
                            <div style="width:100%;" id="Four" runat="server" visible="false">
                                Credit Card no.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <asp:TextBox ID="txtcraditcard" runat="server" 
                                    CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Card Holder&#39;s Name.<asp:TextBox ID="txtcardholdername" runat="server" CssClass="textbox_style"></asp:TextBox>
                                <br />
                                Dated:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                <asp:TextBox ID="txtcreditcardno" runat="server" BorderColor="#CCCCCC" 
                                    BorderStyle="Solid" BorderWidth="1px" class="datepicker" 
                                    Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" 
                                    Width="110px"></asp:TextBox>
                            </div>
                </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp; Naration</td>
            <td>
                 <asp:TextBox ID="txtnaration" runat="server" CssClass="textbox_style" Height="274px" TextMode="MultiLine" Width="334px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td class="style4" colspan="2">
                <asp:Button ID="btnsave" runat="server" CssClass="btn_style" 
                    onclick="btnsave_Click" Text="Save" onclientclick="return ValidateDelete();"/>
                &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" 
                    onclick="btnreset_Click" Text="Reset" />
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;</td>
        </tr>
    </table>
    
     </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
