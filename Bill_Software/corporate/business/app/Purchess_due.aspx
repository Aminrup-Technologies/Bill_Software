<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="Purchess_due.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm53" %>
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
         .table1{ border-collapse:collapse;}
        .table1 td{ text-align:left; border:1px solid #666666; width:100%; }
        .table2{ border-collapse:collapse;}
        .table2 td{ text-align:left; border:1px solid #666666; width:100%; border-top:none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
     <table cellpadding="0" cellspacing="0" class="style2">
        <tr>
            <td bgcolor="#19658A" colspan="4">
                &nbsp; <span class="style3">Purchesse Due</span>&nbsp;</td>
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
            <td colspan="4">
            <div id="main_div" runat="server" style="width:100%; overflow:auto;">
            <div id="first_div" runat="server">
                <table class="style2">
                    <tr>
                        <td colspan="4">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="100%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:13%;">
                                    <asp:Label ID="Label7" runat="server" Text="Purchesse ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:12%;">
                                    <asp:Label ID="Label8" runat="server" Text="Purchesse Date"></asp:Label>
                                </td>
                                
                                
                                
                                <td style="text-align:center; width:25%;">
                                    <asp:Label ID="showid" runat="server" Text="Vendor Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label2" runat="server" Text="Total Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label13" runat="server" Text="Due Amount"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:10%;">
                                    <asp:Label ID="Label1" runat="server" Text="View"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                            <td style="text-align:center; width:13%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Purches_Id") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:12%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Purches_date") %>'></asp:Label>
                                </td>
                               
                                
                                 <td style="text-align:center; width:25%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:20%;">Rs.
                                    <asp:Label ID="Label24" runat="server" Text='<%# Eval("Total_purches_rate") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:20%;">Rs. 
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Due_amount") %>'></asp:Label> /-
                                </td>
                                
                                <td style="text-align:center; width:10%;">
                                     <a href = "#" title="Print Purchasse Bill..." onclick="window.open('/corporate/business/print/purches_bill.aspx?Purches_Id=<%# DataBinder.Eval (Container.DataItem,"Purches_Id")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
                        </td>
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
            </div>
            
            </div>
                </td>
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
</asp:Content>
