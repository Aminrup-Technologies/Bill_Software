<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Seartch_quotation.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm24" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .table1 { border-collapse: collapse; }
        .table1 td { text-align: left; border: 1px solid #666666; width: 100%; }
        .table2 { border-collapse: collapse; }
        .table2 td { text-align: left; border: 1px solid #666666; width: 100%; border-top: none; }
        .search-options { padding: 5px; }

        /* Fix for Select2 dropdown text color */
        .select2-container--default .select2-results__option {
            color: #333333 !important; 
            text-align: left;
        }
        .select2-container--default .select2-selection--single .select2-selection__rendered {
            color: #333333 !important; 
            font-weight: normal;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript" lang="javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript" lang="javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>

    <script type="text/javascript" lang="javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        // This runs on first load AND every time the UpdatePanel refreshes
        prm.add_pageLoaded(function () {
            // Re-initialize Datepicker
            $(".datepicker").datepicker({
                dateFormat: 'dd-M-yy',
                changeMonth: true,
                changeYear: true
            });
            
            // NEW: Initialize Select2 on the dropdown
            $("#<%= cmbvendor.ClientID %>").select2({
                placeholder: "-- Search and Select Client --",
                allowClear: true,
                width: '90%'
            });
        });
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td></td>
                    <td colspan="2">Client Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style" Width="90%" AutoPostBack="True" OnSelectedIndexChanged="cmbvendor_SelectedIndexChanged">
                        </asp:DropDownList>
                    </td>
                    <td></td>
                </tr>

                <tr>
                    <td width="15%">&nbsp;</td>
                    <td width="35%" colspan="2"><asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label></td>
                    <td width="35%" colspan="2">&nbsp;</td>
                    <td width="15%">&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="pnlClientDetails" runat="server" Visible="false" BackColor="#f4f8fa" BorderColor="#19658A" BorderStyle="Solid" BorderWidth="1px" style="padding: 10px; margin-top: 5px; font-family: Tahoma, Geneva, sans-serif; font-size: 11px;">
                            <table width="100%" cellpadding="3" cellspacing="0">
                                <tr>
                                    <td width="15%"><strong>Address:</strong></td>
                                    <td width="35%"><asp:Label ID="lblCAddress" runat="server" ForeColor="#333333"></asp:Label></td>
                                    <td width="15%"><strong>City & State:</strong></td>
                                    <td width="35%"><asp:Label ID="lblCCityState" runat="server" ForeColor="#333333"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><strong>PAN No:</strong></td>
                                    <td><asp:Label ID="lblCPan" runat="server" ForeColor="#333333"></asp:Label></td>
                                    <td><strong>GST/Tax No:</strong></td>
                                    <td><asp:Label ID="lblCGST" runat="server" ForeColor="#333333"></asp:Label></td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date</td>
                    <td>
                        <asp:TextBox ID="txtfromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>To Date</td>
                    <td>
                        <asp:TextBox ID="txttodate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="110px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Quotation Number</td>
                    <td colspan="2">
                        <asp:TextBox ID="txtQutNo" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="90%"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">Search Type</td>
                    <td colspan="2" class="search-options">
                        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" CellSpacing="5">
                            <asp:ListItem Value="All">All Records</asp:ListItem>
                            <asp:ListItem Value="Client">Only Client</asp:ListItem>
                            <asp:ListItem Value="Date" Selected="True">Only Date</asp:ListItem>
                            <asp:ListItem Value="ClientDate">Client & Date</asp:ListItem>
                            <asp:ListItem Value="QutNo">Quotation No</asp:ListItem>
                        </asp:RadioButtonList>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">&nbsp;</td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4" style="text-align: center">
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" Height="30px" Width="100px" BackColor="#19658A" ForeColor="White" />
                        &nbsp;
                        <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" Height="30px" Width="100px" BackColor="#cccccc" />
                        &nbsp;
                        <asp:Button ID="btnExport" runat="server" Text="Export to Excel" OnClick="btnExport_Click" BackColor="#4CAF50" ForeColor="White" BorderStyle="None" Height="30px" Width="120px" Font-Bold="true" style="cursor:pointer;" Visible="false" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6">&nbsp;</td>
                </tr>

                <tr>
                    <td colspan="6">
                        <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#94B8FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <HeaderTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 25%;"><asp:Label ID="Label2" runat="server" Text="Client Name"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="showid" runat="server" Text="Quotation Number"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label6" runat="server" Text="Product Catagory"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label7" runat="server" Text="AMOUNT BEFORE GST"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label9" runat="server" Text="GST (INR)"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label1" runat="server" Text="TOTAL AMOUNT"></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label5" runat="server" Text="Last Mailer Date"></asp:Label></td>
                                        <td style="text-align: center; width: 5%;"><asp:Label ID="edit" runat="server" Text="View"></asp:Label></td>
                                    </tr>
                                </table>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 25%;"><asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label10" runat="server" Text='<%# Eval("Services") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label11" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label12" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 10%;">Rs. <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-</td>
                                        <td style="text-align: center; width: 10%;"><asp:Label ID="Label3" runat="server" Text='<%# Eval("mailStatusDate") %>'></asp:Label></td>
                                        <td style="text-align: center; width: 5%;">
                                            <asp:ImageButton ID="ImageButton1" runat="server" CommandName="View" CommandArgument='<%# Eval("ID") %>' ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View" />
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>
                    </td>
                </tr>
            </table>
        </ContentTemplate>
        <Triggers>
            <%-- THIS IS CRITICAL: It tells the UpdatePanel to allow the Export button to do a full page refresh so the file downloads --%>
            <asp:PostBackTrigger ControlID="btnExport" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
