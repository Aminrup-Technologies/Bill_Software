<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Approve_PR.aspx.cs" Inherits="Bill_Software.corporate.business.app.Approve_PR" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 { width: 100%; }
        .style2 { color: #FFFFFF; font-weight: bold; }
        .pr-header th { background: #0b5f8a; color: white; padding: 8px; font-size: 12px; text-align: center; }
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .filter-panel { display: flex; flex-wrap: wrap; gap: 12px; align-items: end; margin: 12px 0; padding: 10px; background: #F4FAFF; border: 1px solid #cfe3ff; }
        .filter-panel label { display: block; font-size: 11px; font-weight: 600; margin-bottom: 3px; }
        .filter-panel .textbox_style, .filter-panel .dropdown_style { width: 160px; }
    </style>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />
    <script src="calender/jquery-1.7.1.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () { $(".datepicker").datepicker({ dateFormat: "dd-M-yy", changeMonth: true, changeYear: true }); });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;Approve Purchase Requisitions</span></td>
        </tr>
        <tr>
            <td colspan="4">
                <div class="filter-panel">
                    <div>
                        <label>Document Number</label>
                        <asp:TextBox ID="txtDocNo" runat="server" CssClass="textbox_style"></asp:TextBox>
                    </div>
                    <div>
                        <label>From Date</label>
                        <asp:TextBox ID="txtFromDate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox>
                    </div>
                    <div>
                        <label>To Date</label>
                        <asp:TextBox ID="txtToDate" runat="server" CssClass="textbox_style datepicker"></asp:TextBox>
                    </div>
                    <div>
                        <label>Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="dropdown_style">
                            <asp:ListItem Text="Submitted" Value="Submitted" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="All Eligible" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn_style" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn_style" OnClick="btnClear_Click" CausesValidation="false" />
                    </div>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="DataList1" runat="server" AutoGenerateColumns="False" Width="100%" CssClass="pr-header"
                        AllowPaging="True" PageSize="20" GridLines="Both" BorderColor="#e0e0e0" BorderStyle="Solid" BorderWidth="1px"
                        OnPageIndexChanging="DataList1_PageIndexChanging" OnRowCommand="DataList1_RowCommand" OnRowDataBound="DataList1_RowDataBound"
                        EmptyDataText="No submitted requisitions pending approval.">
                        <HeaderStyle BackColor="#0b5f8a" ForeColor="White" Font-Bold="True" HorizontalAlign="Center" />
                        <RowStyle Font-Size="12px" />
                        <AlternatingRowStyle BackColor="#f4f9ff" />
                        <Columns>
                            <asp:TemplateField HeaderText="Sl" ItemStyle-Width="5%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblSlNo" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="clientName" HeaderText="Client" ItemStyle-Width="25%" />
                            <asp:BoundField DataField="ReqNo" HeaderText="PR No" ItemStyle-Width="20%" ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="CreatedBy" HeaderText="Created By" ItemStyle-Width="15%" />
                            <asp:BoundField DataField="CreatedOn" HeaderText="Created On" DataFormatString="{0:dd-MMM-yyyy HH:mm tt}" ItemStyle-Width="15%" />
                            <asp:TemplateField HeaderText="Status" ItemStyle-Width="10%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="View" ItemStyle-Width="5%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnView" runat="server" CommandName="View" CommandArgument='<%# Eval("ReqNo") %>'
                                        ImageUrl="~/corporate/business/WebImages/viewicon.png" ToolTip="View / Modify PR" Height="18" Width="18" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Approve" ItemStyle-Width="5%" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnApprove" runat="server" CommandName="Approve" CommandArgument='<%# Eval("ReqNo") %>'
                                        Text="Approve" CssClass="btn_style" OnClientClick="return confirm('Approve this PR?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </td>
        </tr>
    </table>
</asp:Content>
