<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_chalan.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm39" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .table1 { border-collapse: collapse; background-color: #006699; color: white; }
        .table1 td { text-align: left; border: 1px solid #666666; width: 100%; padding: 5px; }
        .table2 { border-collapse: collapse; }
        .table2 td { text-align: left; border: 1px solid #666666; width: 100%; border-top: none; padding: 5px; }
        .badge-red { color: #cc0000; font-weight: bold; }
        .badge-green { color: #009900; font-weight: bold; }
        .badge-blue { color: #0033cc; font-weight: bold; }
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

        // Auto-suggest for Document Numbers
        function fetchSuggestions(term) {
            if (term.length >= 3) {
                $.ajax({
                    url: "View_chalan.aspx/GetDocumentNumbers",
                    data: JSON.stringify({ prefixText: term }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var datalist = $("#docNumbersList");
                        datalist.empty();
                        $.each(data.d, function (index, item) {
                            datalist.append("<option value='" + item + "'>");
                        });
                    }
                });
            }
        }

        // Auto-suggest for Client Names
        function fetchClientSuggestions(term) {
            if (term.length >= 2) {
                $.ajax({
                    url: "View_chalan.aspx/GetClientNames",
                    data: JSON.stringify({ prefixText: term }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        var datalist = $("#clientList");
                        datalist.empty();
                        $.each(data.d, function (index, item) {
                            datalist.append("<option value='" + item + "'>");
                        });
                    }
                });
            }
        }
    </script>

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
        <ProgressTemplate>
            <div style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0,0,0,0.5); z-index: 9999; display: flex; justify-content: center; align-items: center;">
                <div style="background: white; padding: 20px; border-radius: 5px; text-align: center; font-family: Arial; box-shadow: 0 4px 8px rgba(0,0,0,0.2);">
                    <img src="~/corporate/business/WebImages/loading.gif" alt="Loading..." style="width: 32px; height: 32px;" /><br />
                    <strong style="color:#19658A; margin-top:10px; display:block;">Fetching Records...</strong>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A" style="padding: 5px;">
                        <span class="style2">&nbsp;View DPCC (Delivery planning cum Challan)</span>
                    </td>
                </tr>
                
                <tr><td colspan="6">&nbsp;</td></tr>
                <tr>
                    <td width="5%">&nbsp;</td>
                    <td width="15%">Client Name</td>
                    <td width="30%">
                        <asp:TextBox ID="txtClientName" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="250px" placeholder="Type client name..." list="clientList" autocomplete="off" onkeyup="fetchClientSuggestions(this.value)"></asp:TextBox>
                        <datalist id="clientList"></datalist>
                    </td>
                    <td width="15%">Doc / PO / DO No.</td>
                    <td width="30%">
                        <asp:TextBox ID="txtDocNumber" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="250px" placeholder="Search Challan, QTN, PO..." list="docNumbersList" autocomplete="off" onkeyup="fetchSuggestions(this.value)"></asp:TextBox>
                        <datalist id="docNumbersList"></datalist>
                    </td>
                    <td width="5%">&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td>From Date (Challan)</td>
                    <td>
                        <asp:TextBox ID="txtFromDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="130px"></asp:TextBox>
                    </td>
                    <td>To Date (Challan)</td>
                    <td>
                        <asp:TextBox ID="txtToDate" runat="server" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" class="datepicker" Font-Names="Tahoma, Geneva, sans-serif" Font-Size="11px" Height="22px" Width="130px"></asp:TextBox>
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="6" style="text-align: center; padding: 15px;">
                        <asp:Button ID="btnSearch" runat="server" CssClass="btn_style" Text="Search Records" OnClick="btnSearch_Click" />
                        &nbsp;<asp:Button ID="btnReset" runat="server" CssClass="btn_style" Text="Reset Filters" OnClick="btnReset_Click" />
                    </td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" BorderStyle="Solid" BorderWidth="1px" Visible="False" BackColor="#FFE6E6" style="padding: 10px; margin-bottom: 10px;">
                            <asp:Label ID="lblErrorMsg" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td colspan="6">
                        <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemDataBound="DataList1_ItemDataBound">
                            <FooterStyle BackColor="White" ForeColor="#000066" />
                            <AlternatingItemStyle BackColor="#E8F3FF" />
                            <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            
                            <HeaderTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 4%;">Sl. No.</td>
                                        <td style="text-align: center; width: 10%;">Challan No</td>
                                        <td style="text-align: center; width: 8%;">Challan Date</td>
                                        <td style="text-align: center; width: 8%;">Timeline</td>
                                        <td style="text-align: center; width: 10%;">Quotation No</td>
                                        <td style="text-align: center; width: 8%;">DO No</td>
                                        <td style="text-align: center; width: 8%;">PO No</td>        
                                        <td style="text-align: center; width: 16%;">Client Name</td>
                                        <td style="text-align: center; width: 10%;">Product Catagory</td>
                                        <td style="text-align: center; width: 6%;">Consignee</td>
                                        <td style="text-align: center; width: 6%;">Transporter</td>
                                        <td style="text-align: center; width: 6%;">Consignor</td>
                                    </tr>
                                </table>
                            </HeaderTemplate>
                            
                            <ItemTemplate>
                                <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                    <tr>
                                        <td style="text-align: center; width: 4%;">
                                            <asp:Label ID="lblSlNo" runat="server"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%; font-weight: bold;">
                                            <asp:Label ID="Label7" runat="server" Text='<%# Eval("Chalan_No") %>' ForeColor="#cc3300"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="Label5" runat="server" Text='<%# Eval("Chalan_Date") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="lblDaysLeft" runat="server"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_No") %>'></asp:Label>
                                            <br /><span style="color:gray; font-size:9px;"><%# Eval("Quotation_Date") %></span>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="do_number" runat="server" Text='<%# Eval("DO_Number") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 8%;">
                                            <asp:Label ID="Label12" runat="server" Text='<%# Eval("PO_Number") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 16%;">
                                            <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>' Font-Bold="true" ForeColor="#0033cc"></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 10%;">
                                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <a href="#" title="Print Consignee Copy..." onclick="window.open('/corporate/business/print/NewChhalan.aspx?Chalan_No=<%# DataBinder.Eval(Container.DataItem,"Chalan_No")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return false;">
                                                <img alt="View" height="25px" src="../WebImages/viewicon.png" style="border:0;" />
                                            </a>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <a href="#" title="Print Transporter Copy..." onclick="window.open('/corporate/business/print/NewChhalanDuplicate.aspx?Chalan_No=<%# DataBinder.Eval(Container.DataItem,"Chalan_No")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return false;">
                                                <img alt="View" height="25px" src="../WebImages/viewicon.png" style="border:0;" />
                                            </a>
                                        </td>
                                        <td style="text-align: center; width: 6%;">
                                            <a href="#" title="Print Consignor Copy..." onclick="window.open('/corporate/business/print/NewChhalanTriplicate.aspx?Chalan_No=<%# DataBinder.Eval(Container.DataItem,"Chalan_No")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return false;">
                                                <img alt="View" height="25px" src="../WebImages/viewicon.png" style="border:0;" />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:DataList>
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>