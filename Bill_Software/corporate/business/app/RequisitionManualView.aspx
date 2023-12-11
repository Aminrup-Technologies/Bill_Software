<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="RequisitionManualView.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm72" %>
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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" class="auto-style1">
        <tr>
            <td colspan="4" bgcolor="#19658A">&nbsp;<span class="style2">&nbsp;View Requisition</span></td>
        </tr>
        <tr>
            <td width="15%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="35%">&nbsp;</td>
            <td width="15%">&nbsp;</td>
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
            <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                                <td style="text-align:center; width:15%;"> 
                                    <asp:Label ID="Label6" runat="server" Text="Requi No"></asp:Label>
                                </td>
                                <td style="text-align:center; width:40%;">
                                    <asp:Label ID="Label3" runat="server" Text="Client Name"></asp:Label>
                                </td>
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="showid" runat="server" Text="Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="showrm" runat="server" Text="View"></asp:Label>
                                </td>
                               
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                                <td style="text-align:center; width:15%;"> 
                                    <asp:Label ID="Label7" runat="server" Text='<%# Eval("ReqNo") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:40%;"> 
                                    <asp:Label ID="Label5" runat="server" Text='<%# Eval("clientName") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Date") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:20%;">
                                    <a href = "#" title="Print Requisition..." onclick="window.open('/corporate/business/print/RequisitionNew.aspx?ReqNo=<%# DataBinder.Eval (Container.DataItem,"ReqNo")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                            </tr>
                        </table>
                    </ItemTemplate>
                </asp:DataList>
            </td>
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
        <tr>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Content>
