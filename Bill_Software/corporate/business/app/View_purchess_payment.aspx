<%@ Page Title="" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="View_purchess_payment.aspx.cs" Inherits="Bill_Software.corporate.business.app.WebForm47" %>
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
            height: 18px;
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
                &nbsp;<span class="style3"> View Purchese Payment</span>&nbsp;</td>
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
            <td class="style4">
            </td>
            <td class="style4" colspan="2">
                <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" 
                    BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                    &nbsp;<asp:Image ID="imageTick" runat="server" 
                        ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                    &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                </asp:Panel>
        
            </td>
            <td class="style4">
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;</td>
            <td colspan="2">
        
            <asp:Panel ID="PanelError" runat="server" BorderColor="#FF3300" 
                BorderStyle="Solid" BorderWidth="1px" Visible="False">
                &nbsp;<asp:Image ID="Image1" runat="server" Height="16px" 
                    ImageUrl="~/corporate/business/WebImages/Cross_icon.png.png" 
                    Width="16px" />
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
            <td colspan="4">
            <div style="width:100%; overflow:auto;">
                <asp:DataList ID="DataList1" runat="server" BorderColor="#666666" 
                    BorderStyle="Solid" BorderWidth="1px" Font-Bold="False" Font-Size="11px" 
                    ForeColor="#2D2D2D" GridLines="Both" Width="140%">
                    <FooterStyle BackColor="White" ForeColor="#000066" />
                    <AlternatingItemStyle BackColor="#94B8FF" />
                    <SeparatorStyle BorderColor="#666666" BorderStyle="Solid" BorderWidth="1px" />
                    <SelectedItemStyle BackColor="#669999" ForeColor="White" Font-Bold="True" />
                    <HeaderStyle BackColor="#006699" ForeColor="White" Font-Bold="True" />
                    <HeaderTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table1" width="100%">
                            <tr>
                            <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label7" runat="server" Text="Payment ID"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label8" runat="server" Text="Payment Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label12" runat="server" Text="Purchese ID"></asp:Label>
                                </td>
                                 <td style="text-align:center; width:8%;">
                                    <asp:Label ID="showrm" runat="server" Text="Purchese Date"></asp:Label>
                                </td>
                                <td style="text-align:center; width:20%;">
                                    <asp:Label ID="showid" runat="server" Text="Vendor Name"></asp:Label>
                                </td>
                               
                                <td style="text-align:center; width:9%;">
                                    <asp:Label ID="Label2" runat="server" Text="Purchese Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:9%;">
                                    <asp:Label ID="Label13" runat="server" Text="Payment Amount"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label19" runat="server" Text="Payment Mode"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label6" runat="server" Text="Instrument no"></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label14" runat="server" Text="Instrument Date"></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label1" runat="server" Text="View"></asp:Label>
                                </td>
                                
                            </tr>
                        </table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <table border="0" cellpadding="0" cellspacing="0" class="table2" width="100%">
                            <tr>
                            <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label9" runat="server" Text='<%# Eval("Payment_ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="Label10" runat="server" Text='<%# Eval("Payment_Date") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="ID" runat="server" Text='<%# Eval("Purchess_ID") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:8%;">
                                    <asp:Label ID="addshowname" runat="server" Text='<%# Eval("Purchess_Date") %>'></asp:Label>
                                </td>
                                 <td style="text-align:center; width:20%;">
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("Vendor_Name") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:9%;">Rs. 
                                    <asp:Label ID="Label11" runat="server" Text='<%# Eval("Net_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:9%;">Rs. 
                                    <asp:Label ID="Label15" runat="server" Text='<%# Eval("Given_amount") %>'></asp:Label> /-
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label16" runat="server" Text='<%# Eval("type") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label17" runat="server" Text='<%# Eval("Ch_no") %>'></asp:Label>
                                </td>
                                <td style="text-align:center; width:7%;">
                                    <asp:Label ID="Label18" runat="server" Text='<%# Eval("Ch_date") %>'></asp:Label>
                                </td>
                                
                                <td style="text-align:center; width:7%;">
                                     <a href = "#" title="Print Invoice..." onclick="window.open('/corporate/business/print/purchess_payment.aspx?Payment_ID=<%# DataBinder.Eval (Container.DataItem,"Payment_ID")%>', 'popupwindow','width=900px,height=800px,scrollbars=yes');return true">
                                                <img alt="" height="25px" src="../WebImages/viewicon.png" />
                                </td>
                                
                            </tr>
                        </table>
                    </ItemTemplate>
                    
                </asp:DataList>
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
    </table>
</asp:Content>
