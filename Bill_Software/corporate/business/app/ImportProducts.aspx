<%@ Page Title="XML Uploader for Products" Language="C#" MasterPageFile="~/corporate/business/app/Bill.Master" AutoEventWireup="true" CodeBehind="ImportProducts.aspx.cs" Inherits="Bill_Software.corporate.business.app.ImportProducts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }

        .style2 {
            color: #FFFFFF;
            font-weight: bold;
        }

        .Grid td {
            text-align: center;
            font-size: 10px;
            line-height: 200%;
            border-color: #2D2D2D;
            border-width: 1px;
            border-style: solid;
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

        .auto-style3 {
            height: 18px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="calender/jquery-1.7.1.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.core.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.widget.js" type="text/javascript" language="javascript"></script>
    <script src="calender/jquery.ui.datepicker.js" type="text/javascript" language="javascript"></script>
    <link href="calender/jquery.ui.all.css" rel="stylesheet" type="text/css" />

    <script type="text/javascript">
        function validateForm1() {
            var fileUploader = document.getElementById("<%= fileUploader.ClientID %>");
            var rb1 = document.getElementById("rb1");
            var rb2 = document.getElementById("rb2");
            var rb3 = document.getElementById("rb3");

            // Check if a file is selected
            if (fileUploader.value.trim() === "") {
                alert("Please select a file to upload.");
                return false; // Prevent further action
            }

            if (rb1.checked || rb2.checked || rb3.checked) {
                return true; // At least one is checked
            } else {
                alert("Please select either 'Yes' or 'No'.");
                return false; // Prevent further action
            }

            return true; // If all validations pass
        }

        function validateForm() {
            var grid = document.getElementById('<%= GridView1.ClientID %>');
            var ddlProductName = document.getElementById('<%= ddlProductName.ClientID %>');
            var ddlQuantity = document.getElementById('<%= ddlQuantity.ClientID %>');
            var ddlUnit = document.getElementById('<%= ddlUnit.ClientID %>');
            var ddlRate = document.getElementById('<%= ddlRate.ClientID %>');
            var ddltype = document.getElementById('<%= cmbproduct_service.ClientID %>');

            // Check if GridView has rows (excluding header row)
            var hasRows = grid && grid.rows.length > 1;

            if (!hasRows) {
                alert("Error: The GridView does not contain any data.");
                return false;
            }

            // Validate dropdown selections
            if (ddlProductName.value === "" || ddlProductName.value === "--Select--") {
                alert("Please select a valid column for Product Name.");
                ddlProductName.focus();
                return false;
            }
            if (ddlQuantity.value === "" || ddlQuantity.value === "--Select--") {
                alert("Please select a valid column for Quantity.");
                ddlQuantity.focus();
                return false;
            }
            if (ddlUnit.value === "" || ddlUnit.value === "--Select--") {
                alert("Please select a valid column for Unit.");
                ddlUnit.focus();
                return false;
            }
            if (ddlRate.value === "" || ddlRate.value === "--Select--") {
                alert("Please select a valid column for Rate.");
                ddlRate.focus();
                return false;
            }
            if (ddltype.value === "" || ddltype.value === "--Select--") {
                alert("Please select a valid Product Type");
                ddltype.focus();
                return false;
            }

            return true; // If all validations pass
        }

    </script>
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table cellpadding="0" cellspacing="0" class="auto-style1">
                <tr>
                    <td colspan="6" bgcolor="#19658A"><span class="style2">&nbsp;Upload and Process Tally Stock File</span></td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="4">
                        <asp:Panel ID="PanelOK" runat="server" BackColor="#EEFFDD" BorderColor="#006600" BorderStyle="Solid" BorderWidth="1px" Visible="False">
                            &nbsp;<asp:Image ID="imageTick" runat="server" ImageUrl="~/corporate/business/WebImages/tick-icon.png" />
                            &nbsp;<asp:Label ID="lblOk" runat="server"></asp:Label>
                        </asp:Panel>
                    </td>
                    <td>&nbsp;</td>
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
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr id="Row_Uploader" runat="server" visible="true">
                    <td>&nbsp;</td>
                    <td colspan="2" style="padding-left: 200px;">&nbsp;Upload File (.xml)</td>
                    <td colspan="2">
                        <asp:FileUpload ID="fileUploader" runat="server" CssClass="textbox_style" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr id="Row_XMLType" runat="server" visible="true">
                    <td>&nbsp;</td>
                    <td colspan="2" style="padding-left: 200px;">&nbsp;XML Type</td>
                    <td colspan="2">&nbsp;
                        <asp:RadioButton ID="rb1" runat="server" GroupName="referenceOption" Text="Product Only" />
                        <asp:RadioButton ID="rb2" runat="server" GroupName="referenceOption" Text="GST" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr id="Row_UploaderBtns" runat="server" visible="true">
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;
                <asp:Button ID="btnUpload" runat="server" Text="Upload & Extract Data" Width="200px" CssClass="btn_style" OnClientClick="return validateForm1();" OnClick="btnUpload_Click" />
                        &nbsp;
                        <asp:Button ID="btnreset" runat="server" CssClass="btn_style" OnClick="btnreset_Click" Text="Reset" />&nbsp;&nbsp;
                    <asp:HyperLink ID="lnkDownload" runat="server" Visible="false" Text="Download Log File" CssClass="btn btn-primary" />
                    </td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr id="Row_GridView" runat="server" visible="false">
                    <td colspan="6">
                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                            <RowStyle BackColor="#94B8FF" />
                            <Columns>
                                <asp:BoundField DataField="SL" HeaderText="SL" />
                                <asp:BoundField DataField="Product Name" HeaderText="Product Name" />
                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                                <asp:BoundField DataField="Unit" HeaderText="Unit" />
                                <asp:BoundField DataField="Rate" HeaderText="Rate" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount" />
                            </Columns>
                            <FooterStyle BackColor="#CCCC99" />
                            <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                            <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                        </asp:GridView>
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

                <tr id="Row_Panel1" runat="server" visible="false">
                    <td colspan="6">
                        <asp:Panel ID="Panel1" runat="server" Visible="False">
                            <table class="auto-style1">
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%;">&nbsp;Select Product / Servive Type</td>
                                    <td style="width: 40%;">&nbsp;
                                        <asp:DropDownList ID="cmbproduct_service" runat="server" CssClass="dropdown_style" Width="200px">
                                        </asp:DropDownList>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%;">&nbsp;Product Brand</td>
                                    <td style="width: 40%;">&nbsp;<asp:TextBox ID="txt_ProductBrand" runat="server" CssClass="textbox_style"></asp:TextBox></td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="row1" runat="server" visible="false">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for Product Name :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddlProductName" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="row2" runat="server" visible="false">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for Quantity :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddlQuantity" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="row3" runat="server" visible="false">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for UNIT :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddlUnit" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="row4" runat="server" visible="false">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for Rate :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddlRate" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="RowInsert" runat="server" visible="false">
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%; text-align: right;">&nbsp;<asp:Label ID="Label1" runat="server" Text="Create new records with new PRODUCT ID" ForeColor="Green"></asp:Label></td>
                                    <td style="width: 40%;">&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnInsert" runat="server" Text="Insert Data" CssClass="btn_style" OnClientClick="return validateForm();" OnClick="btnInsert_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <%--<asp:HyperLink ID="HL_InsertSucessLog" runat="server" Visible="false" Text="INSERT Success Log File" CssClass="btn btn-primary" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:HyperLink ID="HL_InsertFailureLog" runat="server" Visible="false" Text="INSERT Failure Log File" CssClass="btn btn-primary" />--%>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr id="RowUpdate" runat="server" visible="false">
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%; text-align: right;">&nbsp;<asp:Label ID="Label2" runat="server" Text="Update exisitng records against Product Name" ForeColor="Brown"></asp:Label></td>
                                    <td style="width: 40%;">&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnUpdate" runat="server" Text="Update Data" CssClass="btn_style" OnClientClick="return validateForm();" OnClick="btnUpdate_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <%--<asp:HyperLink ID="HL_UpdateSucessLog" runat="server" Visible="false" Text="UPDATE Success Log File" CssClass="btn btn-primary" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:HyperLink ID="HL_UpdateFailureLog" runat="server" Visible="false" Text="UPDATE Failure Log File" CssClass="btn btn-primary" />--%>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr id="RowUpsert" runat="server" visible="true">
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%; text-align: right;">&nbsp;<asp:Label ID="Label3" runat="server" Text="Update exisitng records against Product Name & if Not in DB, Create new records with new PRODUCT ID" ForeColor="Blue"></asp:Label></td>
                                    <td style="width: 40%;">&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnUpsert" runat="server" Text="Upsert Data" CssClass="btn_style" OnClientClick="return validateForm();" OnClick="btnUpsert_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <%--<asp:HyperLink ID="HL_UpsertSucessLog" runat="server" Visible="false" Text="UPSERT Success Log File" CssClass="btn btn-primary" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:HyperLink ID="HL_UpsertFailureLog" runat="server" Visible="false" Text="UPSERT Failure Log File" CssClass="btn btn-primary" />--%>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </asp:Panel>
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

                <tr id="Row2_Gridview" runat="server" visible="false">
                    <td colspan="6">
                        <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="False" BackColor="White" BorderColor="#E8F3FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="4" CssClass="Grid" ForeColor="Black" Style="margin-left: 0px; font-size: 11px; font-family: Arial, Helvetica, sans-serif; text-align: center;" Width="100%">
                            <RowStyle BackColor="#94B8FF" />
                            <Columns>
                                <asp:BoundField DataField="SL" HeaderText="SL" />
                                <asp:BoundField DataField="Product Name" HeaderText="Product Name" />
                                <asp:BoundField DataField="HSN Code" HeaderText="HSN Code" />
                                <asp:BoundField DataField="GST Rate" HeaderText="GST Rate" />
                                <asp:BoundField DataField="CGST" HeaderText="CGST" />
                                <asp:BoundField DataField="SGST" HeaderText="SGST" />
                                <asp:BoundField DataField="IGST" HeaderText="IGST" />
                            </Columns>
                            <FooterStyle BackColor="#CCCC99" />
                            <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
                            <SelectedRowStyle BackColor="HighlightText" Font-Bold="True" ForeColor="White" />
                            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                            <AlternatingRowStyle BackColor="White" BorderStyle="Solid" BorderWidth="1px" />
                        </asp:GridView>
                    </td>
                </tr>
                
                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>

                <tr id="Row_Panel2" runat="server" visible="false">
                    <td colspan="6">
                        <asp:Panel ID="Panel2" runat="server" Visible="False">
                            <table class="auto-style1">
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
                                <tr id="Tr2" runat="server" visible="true">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for Product Name :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddl_ProductName" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="Tr3" runat="server" visible="true">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for HSN Code :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddl_HSNCode" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr id="Tr4" runat="server" visible="true">
                                    <td>&nbsp;</td>
                                    <td>&nbsp;Select Mapping Column for GST Rate :</td>
                                    <td>&nbsp;<asp:DropDownList ID="ddl_GSTRate" runat="server" CssClass="dropdown_style" Width="100px"></asp:DropDownList></td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>

                                <tr id="Tr7" runat="server" visible="true">
                                    <td style="width: 10%;">&nbsp;</td>
                                    <td style="width: 40%; text-align: right;">&nbsp;<asp:Label ID="Label5" runat="server" Text="Update exisitng records against Product Name" ForeColor="Brown"></asp:Label></td>
                                    <td style="width: 40%;">&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btn_GstData_Update" runat="server" Text="Update GST Data" CssClass="btn_style" OnClick="btn_GstData_Update_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <%--<asp:HyperLink ID="HL_UpdateSucessLog" runat="server" Visible="false" Text="UPDATE Success Log File" CssClass="btn btn-primary" />&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:HyperLink ID="HL_UpdateFailureLog" runat="server" Visible="false" Text="UPDATE Failure Log File" CssClass="btn btn-primary" />--%>
                                    </td>
                                    <td style="width: 10%;">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>

                <tr>
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr id="row5" runat="server" visible="false">
                    <td>&nbsp;</td>
                    <td colspan="2">&nbsp;</td>
                    <td colspan="2">&nbsp;&nbsp;
                
                    </td>
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
        <Triggers>
            <asp:PostBackTrigger ControlID="btnUpload" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
