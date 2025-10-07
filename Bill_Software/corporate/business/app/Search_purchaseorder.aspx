<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Search_purchaseorder.aspx.cs" Inherits="Bill_Software.corporate.business.app.Search_purchaseorder" %>

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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript" lang="javascript"></script>

    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript" lang="javascript">
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
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Search Quotation</span></td>
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
                    <td colspan="2">Client Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date(Quotation)</td>
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
                <tr>
                    <td colspan="6">
                        <%--<asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="showid0" runat="server" Text="Quotation no"></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="showrm0" runat="server" Text="Quotation Date"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:35%;">
                                    <asp:Label ID="Label9" runat="server" Text="Client Name"></asp:Label>
                                </td>
                               
                                 <td style="text-align:center; width:20%;"> 
                                    <asp:Label ID="Label12" runat="server" Text="Net Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="edit0" runat="server" Text="View"></asp:Label>
                                </td>
                               
                                
                               
                                
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="ID0" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:15%;">
                                    <asp:Label ID="addshowname0" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:35%;">
                                    <asp:Label ID="Label13" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                </td>
                                
                               
                                <td style="text-align:center; width:20%;">Rs. 
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:10%;">
                                    <a href = "#" title="Print Quotation..." onclick="window.open('/corporate/business/print/NewQuotation.aspx?ID=<%# DataBinder.Eval (Container.DataItem,"ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" /></a>
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


                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label1" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                        </td>

                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label5" runat="server" Text="Last Mailer Date"></asp:Label>
                                        </td>

                                        <td style="text-align: center; width: 5%;">
                                            <asp:Label ID="edit" runat="server" Text="View"></asp:Label>
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


                                        <td style="text-align: center; width: 10%;">Rs. 
                                    <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                            /-
                                        </td>

                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label3" runat="server" Text='<%# Eval("mailStatusDate") %>'></asp:Label>
                                        </td>



                                        <td style="text-align: center; width: 5%;">
                                            <asp:ImageButton ID="ImageButton1" runat="server" CommandName="View" CommandArgument='<%# Eval("ID") %>'
                                                ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>
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
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
