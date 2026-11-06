<%@ Page Title="Add Proforma Invoice" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Add_proforma.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm30" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .add-proforma { color: #2d2d2d; font-family: Tahoma, Geneva, sans-serif; font-size: 12px; }
        .add-proforma .pi-header { background-color: #19658A; color: #fff; padding: 12px 16px; font-weight: bold; font-size: 16px; border-radius: 4px 4px 0 0; }
        .add-proforma .pi-messages,
        .add-proforma .form-section,
        .add-proforma .selection-wrap { margin: 12px auto 0; width: 95%; box-sizing: border-box; }
        .add-proforma .form-section { background: #fff; padding: 25px; border: 1px solid #eee; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
        .add-proforma .section-title { margin: 0 0 16px; color: #19658A; font-size: 14px; font-weight: bold; }
        .add-proforma .filter-grid,
        .add-proforma .summary-grid { display: grid; grid-template-columns: 180px minmax(140px, 1fr) 180px minmax(140px, 1fr); gap: 12px 16px; align-items: center; }
        .add-proforma .span-3 { grid-column: 2 / span 3; }
        .add-proforma .filter-label { font-weight: bold; color: #333; }
        .add-proforma .nav-buttons { margin-top: 20px; text-align: center; padding: 15px; background: #f9f9f9; border-top: 1px solid #ddd; }
        .add-proforma .nav-buttons .btn_style { margin: 0 4px; }
        .add-proforma .btn_style { background-color: #19658A; color: #fff; border: 1px solid #104e6b; padding: 6px 15px; cursor: pointer; border-radius: 3px; font-weight: bold; }
        .add-proforma .btn_style:hover { background-color: #124d6b; }
        .add-proforma .dropdown_style { padding: 4px; border: 1px solid #ccc; border-radius: 3px; max-width: 100%; }
        .add-proforma input.datepicker { border: 1px solid #ccc; padding: 4px; border-radius: 3px; font-family: Tahoma, Geneva, sans-serif; font-size: 11px; height: 22px; width: 110px; box-sizing: border-box; }
        .add-proforma .pi-ok,
        .add-proforma .pi-error { padding: 8px 10px; margin-bottom: 8px; }
        .add-proforma .pi-ok { background-color: #EEFFDD; border: 1px solid #006600; }
        .add-proforma .pi-error { border: 1px solid #FF3300; }
        .add-proforma .pi-ok img,
        .add-proforma .pi-error img { vertical-align: middle; margin-right: 6px; }
        .add-proforma .selection-wrap { overflow-x: auto; background: #fff; border: 1px solid #eee; box-shadow: 0 2px 8px rgba(0,0,0,0.05); padding: 12px; }
        .add-proforma .table1,
        .add-proforma .table2 { border-collapse: collapse; width: 100%; min-width: 860px; }
        .add-proforma .table1 td,
        .add-proforma .table2 td { text-align: center; border: 1px solid #666666; vertical-align: middle; }
        .add-proforma .table2 td { border-top: none; }
        .add-proforma .c-client { width: 15%; }
        .add-proforma .c-date,
        .add-proforma .c-no,
        .add-proforma .c-pre,
        .add-proforma .c-gst,
        .add-proforma .c-net { width: 10%; }
        .add-proforma .c-cat { width: 20%; }
        .add-proforma .c-sel { width: 5%; }
        @media (max-width: 800px) {
            .add-proforma .filter-grid,
            .add-proforma .summary-grid { grid-template-columns: 1fr; }
            .add-proforma .span-3 { grid-column: auto; }
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

    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="add-proforma">
                <div class="pi-header">Add Proforma Invoice</div>

                <asp:Label ID="lblclientId" runat="server" Visible="False"></asp:Label>

                <div class="pi-messages">
                    <asp:Panel ID="PanelOK" runat="server" CssClass="pi-ok" Visible="False">
                        <asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                        <asp:Label ID="lblOk" runat="server"></asp:Label>
                    </asp:Panel>
                    <asp:Panel ID="PanelError" runat="server" CssClass="pi-error" Visible="False">
                        <asp:Image ID="Image1" runat="server" Height="16px" ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" Width="16px" />
                        <asp:Label ID="lblErrorMsg" runat="server"></asp:Label>
                    </asp:Panel>
                </div>

                <div class="form-section">
                    <div class="section-title">Quotation Filter</div>
                    <div class="filter-grid">
                        <div class="filter-label">Client Name</div>
                        <div class="span-3">
                            <asp:DropDownList ID="cmbvendor" runat="server" CssClass="dropdown_style">
                            </asp:DropDownList>
                        </div>
                        <div class="filter-label">From Date(Quotation)</div>
                        <div>
                            <asp:TextBox ID="txttodate" runat="server" class="datepicker"></asp:TextBox>
                        </div>
                        <div class="filter-label">To Date(Quotation)</div>
                        <div>
                            <asp:TextBox ID="txtfromDate" runat="server" class="datepicker"></asp:TextBox>
                        </div>
                        <div class="filter-label">Search Type</div>
                        <div class="span-3">
                            <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem>Only Client</asp:ListItem>
                                <asp:ListItem Selected="True">Only Date</asp:ListItem>
                                <asp:ListItem>Client &amp; Date</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>
                    </div>
                    <div class="nav-buttons">
                        <asp:Button ID="btnSertch" runat="server" CssClass="btn_style" OnClick="btnSertch_Click" Text="Search" />
                        <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />
                    </div>
                </div>

                <div class="selection-wrap">
                    <div class="section-title">Quotation Selection</div>
                    <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%" OnItemCommand="DataList1_ItemCommand">
                        <FooterStyle BackColor="White" ForeColor="#000066" />
                        <AlternatingItemStyle BackColor="#f4fbfd" />
                        <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                        <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                        <HeaderStyle BackColor="#19658A" Font-Bold="True" ForeColor="White" />
                        <HeaderTemplate>
                            <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                                <tr>
                                    <td class="c-client">
                                        <asp:Label ID="Label2" runat="server" Text="Client Name"></asp:Label>
                                    </td>
                                    <td class="c-date">
                                        <asp:Label ID="showrm" runat="server" Text="Quotation Date"></asp:Label>
                                    </td>
                                    <td class="c-no">
                                        <asp:Label ID="showid" runat="server" Text="Quotation Number"></asp:Label>
                                    </td>
                                    <td class="c-cat">
                                        <asp:Label ID="Label6" runat="server" Text="Product Catagory"></asp:Label>
                                    </td>
                                    <td class="c-pre">
                                        <asp:Label ID="Label7" runat="server" Text="AMOUNT BEFORE GST (INR)"></asp:Label>
                                    </td>
                                    <td class="c-gst">
                                        <asp:Label ID="Label9" runat="server" Text="GST (INR)"></asp:Label>
                                    </td>
                                    <td class="c-net">
                                        <asp:Label ID="Label1" runat="server" Text="AMOUNT INCLUSIVE OF GST (INR)"></asp:Label>
                                    </td>
                                    <td class="c-sel">
                                        <asp:Label ID="Label13" runat="server" Text="Select"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                                <tr>
                                    <td class="c-client">
                                        <asp:Label ID="Label4" runat="server" Text='<%# Eval("Client_Name") %>'></asp:Label>
                                    </td>
                                    <td class="c-date">
                                        <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Quotation_date") %>'></asp:Label>
                                    </td>
                                    <td class="c-no">
                                        <asp:Label ID="ID" runat="server" Text='<%# Eval("Quotation_no") %>'></asp:Label>
                                    </td>
                                    <td class="c-cat">
                                        <asp:Label ID="Label10" runat="server" Text='<%# Eval("PServiceName") %>'></asp:Label>
                                    </td>
                                    <td class="c-pre">
                                        <asp:Label ID="Label11" runat="server" Text='<%# Eval("sub_total") %>'></asp:Label>
                                    </td>
                                    <td class="c-gst">
                                        <asp:Label ID="Label12" runat="server" Text='<%# Eval("service_tax1") %>'></asp:Label>
                                    </td>
                                    <td class="c-net">Rs.
                                        <asp:Label ID="Label8" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label>
                                        /-
                                    </td>
                                    <td class="c-sel">
                                        <asp:ImageButton ID="ImageButton2" runat="server"
                                            CommandArgument='<%# Eval("Quotation_no") %>' CommandName="Select"
                                            ImageUrl="~/corporate/business/WebImages/tick-icon.png"
                                            ToolTip="Select" />
                                    </td>
                                </tr>
                            </table>
                        </ItemTemplate>
                    </asp:DataList>
                </div>

                <asp:Panel ID="Panel1" runat="server" Visible="false">
                    <div class="form-section">
                        <div class="section-title">Proforma Details</div>
                        <div class="summary-grid">
                            <div class="filter-label">Invoice Date</div>
                            <div class="span-3">
                                <asp:TextBox ID="txtinvoiceDate" runat="server" class="datepicker"></asp:TextBox>
                            </div>
                            <div class="filter-label">Quotation No</div>
                            <div>
                                <asp:Label ID="lblQuotation_no" runat="server"></asp:Label>
                            </div>
                            <div class="filter-label">Quotation Date</div>
                            <div>
                                <asp:Label ID="lblQuotation_date" runat="server"></asp:Label>
                            </div>
                            <div class="filter-label">Client ID</div>
                            <div>
                                <asp:Label ID="lblClient_Id" runat="server"></asp:Label>
                            </div>
                            <div class="filter-label">Client Name</div>
                            <div>
                                <asp:Label ID="lblClientName" runat="server"></asp:Label>
                            </div>
                            <div class="filter-label">Invoice Amount</div>
                            <div>
                                <asp:Label ID="lblGross_amount" runat="server" Visible="False"></asp:Label>
                                <asp:Label ID="lblNet_amount" runat="server"></asp:Label>
                            </div>
                            <div>
                                <asp:Label ID="lblservicetax" runat="server" Visible="False"></asp:Label>
                                <asp:Label ID="lblsubtotal" runat="server" Visible="False"></asp:Label>
                            </div>
                        </div>
                        <div class="nav-buttons">
                            <asp:Button ID="Button1" runat="server" CssClass="btn_style" Text="Save" OnClick="Button1_Click" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="Button1" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
