<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="patty_cash_expences.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm56" %>
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

    <table cellpadding="0" cellspacing="0" class="style2">
    <tr>
        <td bgcolor="#19658A" colspan="4">
            &nbsp;<span class="style3">Petty cash Expenses</span>&nbsp;</td>
    </tr>
    <tr>
        <td width="15%">
            &nbsp;</td>
        <td width="35%">
            &nbsp;</td>
        <td width="35%">
            &nbsp;</td>
        <td width="15%">
            &nbsp;</td>
    </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td colspan="2">
                                    <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                                        BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" 
                                        Visible="False">
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
            <td colspan="2">
                <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" 
                    BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" 
                        ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                    &nbsp;<asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
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
        <td>
            &nbsp;</td>
        <td>
            &nbsp;&nbsp;</td>
        <td>
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;Cash Status&nbsp;</td>
            <td>
                <asp:DropDownList ID="cmbcashstatus" runat="server" CssClass="dropdown_style">
                    <asp:ListItem>Cash In</asp:ListItem>
                    <asp:ListItem>Cash Out</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>&nbsp;</td>
        </tr>
    <tr>
        <td>
            &nbsp;</td>
        <td>
            &nbsp;Date&nbsp;</td>
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
            &nbsp;Payment Made To / Recieved By&nbsp;</td>
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
            &nbsp;Expense Head&nbsp;</td>
        <td>
            <asp:DropDownList ID="cmbexpenceshead" runat="server" CssClass="dropdown_style">
            </asp:DropDownList>
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
            <asp:RadioButtonList ID="RadioButtonList1" runat="server" 
                RepeatDirection="Horizontal">
                <asp:ListItem Selected="True">Cash</asp:ListItem>
            </asp:RadioButtonList>
        </td>
        <td>
            &nbsp;</td>
    </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Naration</td>
            <td>
                <asp:TextBox ID="txtnaration" runat="server" CssClass="textbox_style" 
                    Height="250px" TextMode="MultiLine" Width="360px"></asp:TextBox>
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td>
                &nbsp;Closing Balance&nbsp;</td>
            <td>
                <asp:Label ID="lblclosingbalance" runat="server"></asp:Label>
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
                    onclick="btnsave_Click" onclientclick="return ValidateDelete();" Text="Save" />
                &nbsp;<asp:Button ID="btnreset" runat="server" CssClass="btn_style" 
                    onclick="btnreset_Click" Text="Reset" />
            </td>
            <td>
                &nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td class="style4" colspan="2">
                <asp:DataList ID="DataList20" runat="server" BorderColor="#666666" 
                                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                                    ForeColor="#2D2D2D" GridLines="Both">
                                    <FooterStyle BackColor="White" ForeColor="#000066" />
                                    <AlternatingItemStyle BackColor="#94B8FF" />
                                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                                    <ItemTemplate>
                                        <table>
                                            <tr>
                                                <td>
                                                   <a href = "#" title="Print Voutcher..." onclick="window.open('/corporate/business/print/Patty_cash_expencess_voutcher.aspx?payment_id=<%# DataBinder.Eval (Container.DataItem,"payment_id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>
                                </asp:DataList>
            </td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td class="style4" colspan="2">&nbsp;</td>
            <td>&nbsp;</td>
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
 
 <Triggers>
 
                <asp:PostBackTrigger ControlID="btnsave"  />
                <asp:PostBackTrigger ControlID="btnreset" />
                
            </Triggers>
    </asp:UpdatePanel>
</asp:Content>
