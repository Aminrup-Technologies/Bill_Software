<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewQuotation.aspx.cs" Inherits="Bill_Software.corporate.business.print.NewQuotation" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Quotation Page</title>
    <link rel="shortcut icon" href="../../Image/kvqafabioc.png" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" />
    <style type="text/css">
        table {
            border-collapse: collapse;
        }

        th, td {
            border: 0px solid #c6c7cc;
            font-family: 'Century Gothic';
            font-size: 12px;
            padding: 3px 5px;
        }

        .bold {
            font-weight: bold;
        }

        .gap {
            line-height: 0.5px;
        }

        .gap1 {
            padding: 15px 5px;
        }

        .trheight {
            line-height: 0.5px;
        }

        .header, .hide {
            visibility: hidden;
            height: 120px;
        }

        .footer, .hide {
            visibility: hidden;
        }



        @media print {
            #footer {
                display: block;
                position: fixed;
                bottom: 0px;
            }

            #bodycontain {
                padding-bottom: 25px;
                overflow-y: auto;
            }

            #Button1 {
                visibility: hidden;
            }

            #Button2 {
                visibility: hidden;
            }

            .pagebrake {
                page-break-inside: avoid;
            }

            .pagebrake1 {
                page-break-before: always;
            }
        }

        @page {
            margin: 6mm 6mm 6mm 16mm;
        }

        .auto-style1 {
            height: 125px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <table border='0' width='844px'>
            <thead id='header'>
                <tr>
                    <th style='width: 100%'>
                        <img src="../WebImages/flame-ex_hdrtop.png" width="100%" height="150px"></th>
                </tr>
                <tr>
                    <th></th>
                </tr>

            </thead>
            <tfoot style='width: 100%;'>
                <tr>
                    <td width='100%'>
                        <table width='100%' border='0'>
                            <tr>
                                <td colspan='4'>
                                    <br>
                                    &nbsp;</td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </tfoot>
            <tbody style="font-family: 'Century Gothic'; font-size: 12px; padding: 3px 5px; border: 0px solid #c6c7cc;">
                <tr>
                    <td id='bodycontain' width='100%' style='font-weight: bold'>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                            </tr>
                        </table>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                            </tr>
                        </table>
                        <table border='0' width='100%'>
                            <tr>
                                <td class='add' style='vertical-align: top' width='53%'>
                                    <table border='0' width='100%' class='address'>
                                        <tr>
                                            <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>To,</td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'></td>
                                            <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                                </td>
                            </tr>
                            <%--<tr>
                                <td class='add' style='vertical-align: top' width='53%'>
                                    <table border='0' width='100%' class='address'>
                                        <tr>
                                            <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Kind Attention</td>
                                            <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                            <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                                <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                                </td>
                            </tr>--%>
                            <%--<tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Kind Attention</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblrename" runat="server" Visible="false"></asp:Label><asp:Label ID="lbl_refname" runat="server"></asp:Label></td>
                            </tr>--%>
                            <tr id="ref_desg" runat="server" visible="false">
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'></td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>&nbsp;</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbldeg" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Company Name</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblClient" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class='' style='width: 30%; vertical-align: top; padding: 1px 5px;'>Address</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 68%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="txtaddres" runat="server"></asp:Label><br />
                                    <asp:Label ID="lblcity" runat="server"></asp:Label>-<asp:Label ID="lblpincode" runat="server"></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td style='vertical-align: top;' width='2%'></td>
                    <td class='qno' style='vertical-align: top; background-color: #d9d3d3;' width='45%'>
                        <table border='0' width='100%' class='quotation'>
                            <tr>
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Quotation Date</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbldate" runat="server"></asp:Label></td>
                            </tr>
                            <tr>
                                <td class="" style='width: 38%; vertical-align: top; padding: 1px 5px;'>Quotation Number</td>
                                <td class="" style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class="" style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblqnumber" runat="server"></asp:Label></td>
                            </tr>
                            <tr id="client_code" runat="server" visible="false">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Client Code</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblClientCode" runat="server"></asp:Label></td>
                            </tr>
                            <tr id="Tr1" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'></td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>&nbsp;</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'></td>
                            </tr>
                            <tr id="Tr2" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>RFQ No.</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_refid" runat="server"></asp:Label></td>
                            </tr>
                            <tr id="Tr3" runat="server" visible="true">
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>RFQ Date</td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lbl_refdate" runat="server"></asp:Label></td>
                            </tr>
                            <asp:Panel ID="pnlPanGst" runat="server" Visible="false">
                                <tr>
                                    <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Client PAN Number</td>
                                    <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                    <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                        <asp:Label ID="lblPanno" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>Client GST Number</td>
                                    <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>:</td>
                                    <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                        <asp:Label ID="lblGstno" runat="server"></asp:Label></td>
                                </tr>
                            </asp:Panel>
                            <tr>
                                <td class='' style='width: 38%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup1" runat="server"></asp:Label></td>
                                <td class='' style='width: 2%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup2" runat="server"></asp:Label></td>
                                <td class='' style='width: 60%; vertical-align: top; padding: 1px 5px;'>
                                    <asp:Label ID="lblplaceofsup3" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </tbody>
        </table>

        <table border='0' width='100%'>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
        </table>

        <table border='0' width='100%'>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
            <tr>
                <td class='sub' style='text-align: center; font-weight: bold; font-size: 12px; text-decoration: underline;'>Sub: RFQ for
                                    <asp:Label ID='lblservice' runat='server'></asp:Label>
                    <asp:Label ID='lblprimary_service' runat='server'></asp:Label></td>
            </tr>
            <tr>
                <td class='gap' style='text-align: center; font-weight: bold;'>&nbsp</td>
            </tr>
        </table>

        <table border='0' width='100%' class='bodytext pagebrake'>
            <tr>
                <td class='' style='text-align: left; font-weight: bold;'>Dear Sir/Madam,
                                    <asp:Label ID='lbltital' runat='server'></asp:Label>&nbsp;<asp:Label ID="lbllname" runat="server"></asp:Label></td>
            </tr>
            <%--<tr>
                <td class="gap" style="">&nbsp</td>
            </tr>--%>
            <%--<tr>
                <td class='' style='text-align: justify; font-weight: 100'>
                    <span class='bold'>Thank you for showing interest in our Organization.</span>
                </td>
            </tr>--%>
            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>
            <%--<tr>
                                <td class='' style='text-align: justify; font-weight: 100'>We are pleased to <span class='bold'>offer</span> our <span class='bold'>Quote</span> detailing the <span class='bold'>Technical & Commercial Terms</span> for the <span class='bold'>
                                    <asp:Label ID="lblPrimaryService" runat="server"></asp:Label>.</span>
                                </td>
                            </tr>--%>
            <tr>
                <td class='' style='text-align: justify; font-weight: 100'>This is with reference to our discussion for 
                                    <asp:Label ID="lblPrimaryService" runat="server"></asp:Label>
                    , We are pleased to submit our best proposal as below:
                </td>
            </tr>


            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>
            <%--<tr id="CompIntro" runat="server" visible="false">
                                <td class='' style='text-align: justify; font: italic; font-weight: 100'>
                                    <span class='bold'>Aminrup Technologies</span> is an <span class='bold'>ISO 9001:2015 Certified Company</span> dealing in the all types of <span class='bold'>Industrial Supplies & Commissioning of Turnkey Projects</span> for <span class='bold'>Fire Safety & Security Systems</span> in compliance with the <span class='bold'>Social & Technical Audit Requirements. Aminrup Technologies focuses</span> on designing a <span class='bold'>Safe Workplace</span> by providing the <span class='bold'>Best Quality Products with Services at Reasonable Rates</span>. We are committed to building Long-Lasting Relationships with our Clients & Community by providing a high standard of Product Quality and On-Time Delivery, while ensuring our Integrity, Performance and Customer Satisfaction. Aminrup Technologies has developed Multi-Sector Expertise in diverse Industry Verticals such as Paper, Jute, Metals, Engineering, Export, Retail Chains, Construction, Computer Hardware & Software, Rubber, Plastic, Steel, Chemical, Electrical, Leather, Garments, Food, Education etc. Aminrup Technologies deals with the below Industrial Supplies & Commissioning of related Turnkey Projects:
                                </td>
                            </tr>--%>

            <tr id="CompIntro" runat="server" visible="true">
                <td class='' style='text-align: justify; font: italic; font-weight: 100'>Started in the year 2004, with <span class='bold'>A&A Associates</span> as the first company of the group followed by <span class='bold'>Flame Ex</span> , A&A Group has positioned itself as a premier <span class='bold'>Industrial Distribution Company</span> in the Eastern part of India.
                </td>
            </tr>
            <%--<tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>--%>
            <%--<tr>
                                <td class="" style="text-align: justify; font-weight: 100">
                                     <span class='bold'>CREATIVE VISUAL DISPLAY PRINTING PRODUCTS:</span> We offer an extensive range of specially designed Creative Visual Display Printing Products designed in English as well as relevant Local Languages comprising of Department ID Boards, Customised Safety Signages, Sign Boards, Banners, Evacuation Plans & Posters for BSCI, ETI, WRAP, SA8000, CT-PAT, Quality, Environmental Concerns, Occupational Health & Safety, Minimum Wage Abstracts, Factory's Act Abstracts, Fire Safety and various Management Systems Policies & Procedures, Machine Instructions etc. These Creative Visual Display Printing Products are available in Art Paper, Gumming Sheets, Vinyl Sunboard with Lamination, Canvas, Acrylic Sheets etc
                                </td>
                            </tr>
                            <tr>
                                <td  class="gap" style="">&nbsp</td>
                            </tr>--%>
            <tr id="CompBranding" runat="server" visible="false">
                <td class="" style="text-align: justify; font-weight: 100">
                    <span class='bold'>CORPORATE BRANDING SOLUTIONS:</span> We offer an extensive range of customised Visiting cards, Brochures, Catalogues, Annual Reports, Diaries, Calendars, Books, Magazines, Labels, Stationeries, POP Materials, Flyers, Corporate Invites, Corporate Profile, Note Books, Note Pads, Journals, Employee ID Cards, Fire Trained & First Trained Batches, Websites etc.
                </td>
            </tr>
            <%--<tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>--%>
            <tr id="CompProducts" runat="server" visible="false">
                <td class="" style="text-align: justify; font-weight: 100">
                    <span class='bold'>FIRE DETECTION, ALARM & FIRE FIGHTING SYSTEM:</span> We undertake the complete process of Installation, Supply, Testing, and Commissioning of the Products Range including all types of Fire Extinguishers, Fire Hydrant/Sprinklers System, Smoke Detectors, Fire Alarm and Detection System, Fire Buckets, Emergency Lights, Fire Blankets, Spill Containment Provisions etc. In addition, we also under take Annual Maintenance Contract for Hydrant System & Fire Alarm System Rectification, Servicing, and Fire Extinguisher’s Refilling Job.
                </td>
            </tr>
            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>
            <%--<tr>
                                <td class="" style="text-align: justify; font-weight: 100">
                                     	<span class='bold'>CCTV SURVEILLANCE SYSTEM:</span> The Security Systems that we offer are high on demand due to their effective Performance & Advanced Technology. The Salient features of our CCTV Surveillance Systems are High Security, Efficient Performance & Clear Images
                                </td>
                            </tr>
                            <tr>
                                <td  class="gap" style="">&nbsp</td>
                            </tr>--%>
            <tr id="CompProd" runat="server" visible="false">
                <td class="" style="text-align: justify; font-weight: 100">
                    <span class='bold'>INDUSTRIAL SAFETY ITEMS & PPEs:</span> Our complete gamut of Industrial Safety Items & Personal Protective Equipments including First Aid Kit & Medical Supplies; Suggestion Boxes, Bay Marking Tapes, Rubber Mats etc., delivered by us is highly appreciated for their Rugged Construction, Compact Designs, Easy Installation, Optimum Performance, and Longer Functional Life.
                </td>
            </tr>
            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>
            <%--<tr>
                                <td class="" style="text-align: justify; font-weight: 100;">
                                     	<span class='bold'>FIRE SAFETY INSPECTION:</span> Aminrup Technologies conducts Fire Safety Inspections to assess the Fire Safety Procedures, Installations, Fire Safety Hazards at Workplace & verify whether the Occupier of the Building is complying with the Statutory & Legislative Requirements, National Building Code of India, Relevant Indian Standards on Fire Prevention and Life Safety Measures prevailed from time to time. 
                                </td>
                            </tr>--%>

            <%--<tr>
                <td class="gap" style="">&nbsp</td>
            </tr>--%>
            <%--<tr>
                <td class="" style="">OUR CLIENTS</td>
            </tr>--%>

            <%--<tr id="clients_img" runat="server" visible="false">
                <td class="" style="height: 250px">
                    <img src="../WebImages/clientsbg.png" width='100%' height='250px' />
                </td>
            </tr>--%>

            <tr>
                <td class="gap" style="">&nbsp</td>
            </tr>


            <%--  <tr>
                                <td class="" style="text-align: justify; font-weight: 100"><span class="bold"> KVQA East</span> is a strategic <span class="bold">Associate Partner</span> of <span class="bold">KVQA Certifications Pvt. Ltd. KVQA Certifications Pvt. Ltd.</span>, is established as an <span class="bold">Independent Third-Party Assessment</span> and <span class="bold">Certification Body</span> with the main Objective to provide <span class="bold"> Value-Added Certification & Assessment Services</span> to its <span class="bold">Invaluable Clients. KVQA</span> has developed <span class="bold">Multi-Sector Expertise</span> of <span class="bold">Auditing</span> backed by <span class="bold">Qualified Auditors</span> to augment its Resources with specialists of experience in diverse Industry Segments. KVQA Certifications Pvt. Ltd., covers a wide spectrum of Certification Scopes such as Paper, Printing & Publishing, Metals, Engineering, Export, Retailers, Construction, Computer Hardware & Software, Rubber & Plastics, Chemicals, Electrical & Electronics, Wood, Banking, Leather, Garments, Food, Education & many more.<br />
                                    </td>
                            </tr>
                             <tr>
                                <td class="" style="text-align: justify; font-weight: 100"> KVQA Certifications Pvt. Ltd., is a <span class="bold">Leader</span> in the <span class="bold">Certification Business</span> serving approximately <span class="bold">10,000 Valued</span> and <span class="bold">Satisfied Clients Worldwide</span> conforming to different <span class="bold">Scopes of Certification</span>. Among the range of Certifications, KVQA’s portfolio includes <span class="bold">ISO 9001:2015 (QMS), ISO 14001:2015 (EMS), OHSAS 18001</span> for Health & Safety, <span class="bold">ISO 22000 (HACCP), ISO/IEC 27001</span> (Information Security Management Systems), <span class="bold">ISO 28000</span> (Supply Chain Security Management Systems), and many other industry nominations.<br />
                                    </td>
                            </tr>--%>
        </table>

        <%--    <table border="0" width="100%" class="fees pagebrake">
                            <tr>
                                <td class="" style="text-align: left; font-weight: bold;">OUR FEES</td>
                            </tr>
                            <tr>
                                <td class="gap" style="">&nbsp</td>
                            </tr>
                           
                            <tr>
                                <td class="" style="">
                                        <asp:Label ID="lblcgstsgstOrigst" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="auto-style1"></td>
                            </tr>
                        </table>--%>

        <%--<table border="0" width="100%" class='Payment pagebrake'>
                            <tr>
                                <td>
                                    <asp:Label ID="lblserviceamo" runat="server"></asp:Label>
                                </td>
                            </tr>
                        </table>--%>

        <asp:Label ID="lblserviceamo" runat="server"></asp:Label>

        <br />

        <asp:Label ID="lblPayment" runat="server"></asp:Label>

        <br />

        <table border="0" width="100%" class="PAYMENTTERMS" id="tbl_paymentterms" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">PAYMENT TERMS</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">All Payments shall be made through Demand Draft/Pay Orders/At Par Payable Cheques/Telegraphic Transfer in favour of “Aminrup Technologies”.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">All Invoices shall be paid by the Client within Seven (7) Days of the Date of Invoice unless otherwise agreed in writing by the Company. In the event of Late Payment, the Company shall be entitled to charge interest on any overdue amounts (computed from the due date to the date of actual payment) at a rate of the lesser of (a) one and half percent (1.5%) per month; or (b) Maximum Rate permitted by Law. GST at Current Rates is payable in addition to the Amount Quoted in accordance with the HSN Code.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">The Client is liable to pay the Advance Payment in accordance with the Schedule of Payments specified above along with a formal Work Order. The Advance Payment is non-refundable unless the Company fails to provide the Services and is at fault for such failure. Where the failure is not the Company’s Fault, no refund will be made.
                </td>
            </tr>

            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">The Company may raise Interim Invoice/s for the Balance Amount as per the Schedule of Payment submitted above.
                </td>
            </tr>

            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">If any amount of the invoice is disputed by the Client, the Client shall inform the Company of the grounds for such dispute within Seven (7) Days of Delivery of the Goods and shall pay to the Company the Value of the Invoice less the Disputed Amount in accordance with these Payment Terms.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">Bank Charges, if any, shall be borne by the Client. The Customer shall compensate the Company for any judicial or extrajudicial costs, including extrajudicial collection costs and costs of legal assistance which the Company incurs as a result of the Customer’s non-fulfilment or late or inadequate fulfilment of its obligations.  
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">The Company shall also reserve the Right to Withdraw or Suspend all or any of its Services to the Client, till such time that the raised Invoice is settled.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i>
                </td>
                <td class="" style="text-align: justify; font-weight: 100">The Company reserves the right to increase a Quoted Fee in the event that the Client requests a Variation to the work agreed.
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <br />

        <asp:Label ID="lblPrimaryServicePoint" runat="server"></asp:Label>

        <table border="0" width="100%" class="RETURNSSHORTAGES" id="tbl_RETURNSSHORTAGES" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">RETURNS & SHORTAGES</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">The Quantity of Materials delivered needs to be inspected by the Client at the time of Delivery itself. Any Shortages must be notified on the Delivery Challan with the Authorized Signatory’s Signature & Corporate Stamp at the time of Delivery itself.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">Any Quality Related Issues need to be notified within 48 Hours from the time of Delivery of the Materials.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">Unless the Customer has inspected the Delivered Goods and given written notice to the Company within 48 Hours from the time of Delivery of the Materials that the Goods do not comply with the relevant Specifications or Quality Descriptions, the Goods are deemed to have been accepted in Good Order & Condition by the Client.  
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">If the Client observes any Quality Related Issues with the Delivered Materials, it is recommended not to use the Defective Goods till Return of the Damaged Goods is executed.
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">All Defective Items requested for Replacement or Return by the Client shall be thoroughly inspected and a determination will be made if eligibility of requirements is met for Replacement or Exchange.  
                </td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100; vertical-align: top">
                    <i class="fa fa-arrow-circle-right" style="color: #c8152a"></i></td>
                <td class="" style="text-align: justify; font-weight: 100">In case the Company finds the Delivered Items proposed for Replacement or Return by the Client in a tampered condition, the Return Request shall not be entertained.
                </td>
            </tr>

            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <%--<table border='0' width='100%' class='PrimaryService'>
                            <tr><td colspan='2' class='' style='text-align: left; font-weight: bold;'>Primary Service Name</td></tr>
                            <tr><td colspan='2' class='gap' style=''>&nbsp</td></tr>

                            <tr><td class='' style='text-align: justify; font-weight: 100;  vertical-align: top'><i class='fa fa-arrow-circle-right' style='color: #c8152a'></i></td>
                                <td class='' style='text-align: justify;  font-weight: 100'>
                                    
                                </td>
                            </tr>

                            <tr><td colspan='2' class='gap' style=''>&nbsp</td></tr>
                        </table>--%>

        <table border="0" width="100%" class="CONFIDENTIALITY pagebrake" id="tbl_CONFIDENTIALITY" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">CONFIDENTIALITY</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100">Both Parties shall maintain strict confidence and shall not disclose to any third party any information or material relating to the other or the other's business which comes into that party's possession and shall not use such information and material. This provision shall not, however, apply to information or material which is or becomes public knowledge other than by breach by a party of this clause.
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>


        <table border="0" width="100%" class="INDEMNIFICATION pagebrake" id="tbl_INDEMNIFICATION" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">INDEMNIFICATION</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100">The Client agrees to save and hold the Company harmless from any claims, demands, liabilities, costs, expenses or judgments arising in whole or in part, directly or indirectly, out of the negligence or lack of care by Buyer or Buyer’s customers, agents, employees or invitees involving the use of the goods supplied by Seller.  This indemnification shall include all costs, attorney’s fees and other expenses paid or incurred by or imposed upon Seller in connection with the defence of any such claim.
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="TERMINATION pagebrake" id="tbl_TERMINATION" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">TERMINATION</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100">This Agreement shall remain in full force and effect until terminated by any one of the parties as set out in this Agreement. Each Party may terminate this Agreement by Written Notice to the other Party under the following circumstances: (i) If the other Party commits a Material Breach of this Agreement and fail to rectify such within 10 Working Days after the other Party’s Written Notice; (ii) If the other Party becomes Insolvent, unable to pay its debts as they fall due, or subject to Bankruptcy Proceedings, Receivership, Dissolution, Liquidation, Wind-Up or otherwise Discontinue Business; (iii) For convenience after serving the other party a written notice 60 days prior to termination.
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="FORCEMAJEURE pagebrake" id="tbl_FORCEMAJEURE" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">FORCE MAJEURE</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100">Neither party shall be liable to the other for any failure to perform any of its obligations (except Payment Obligations) under the Agreement during any period in which such performance is delayed by any circumstances beyond a party's reasonable control including, without limitation, fire, flood, war, embargo, strike, riot, or the intervention of any governmental authority ("Force Majeure Event") provided that the delayed party shall provide the other party with prompt written notice of the Force Majeure Event. The Affected Party shall immediately notify the other Party in writing of the Causes and Expected Duration of any such Occurrence.
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="GOVERNINGLAWJURISDICTION pagebrake" id="tbl_GOVERNINGLAWJURISDICTION" runat="server" visible="false">
            <tr>
                <td colspan="2" class="" style="text-align: left; font-weight: bold;">GOVERNING LAW & JURISDICTION</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td class="" style="text-align: justify; font-weight: 100">The Agreement and any Non-Contractual Obligations shall be interpreted according to Indian law and the Kolkata High Court shall have exclusive jurisdiction.</td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_VALIDITYOFTHEOFFER" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">VALIDITYOF THE OFFER</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">This Offer is valid for
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Days from the Date of Submission.<br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">This Offer is valid for
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Days from the Date of Submission.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="tbl_tx" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">GST APPLICABILITY</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;"><span>GST will be <asp:Label ID="Label1" runat="server" Font-Bold="true" Text="charged extra"></asp:Label>&nbsp;item-wise as applicable under the prevailing GST laws based on HSN/SAC classification.</span><br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">This Offer is valid for
                    <asp:Label ID="lbl_valdays" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Days from the Date of Submission.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table1" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">DELIVERY TERMS</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Within 
                    <asp:Label ID="lbl_deliverytrms" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;from the Date of Receipt of all Technical Clearance.<br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Within 
                    <asp:Label ID="lbl_deliverytrms" runat="server" Text="15" Font-Bold="true"></asp:Label>&nbsp;Weeks from the Date of Receipt of all Technical Clearance.<br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table2" runat="server" visible="false">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">MATERIAL ACCEPTANCE</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Material once invoiced cannot be returned back
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Material once invoiced cannot be returned back
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table3" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold; width: 30%;">PACKING & FORWARDING</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">Charges will be 
                    <asp:Label ID="lbl_pkging" runat="server" Text="15" Font-Bold="true"></asp:Label><br />
                </td>
            </tr>
            <%--<tr>
                <td class="" style="text-align: justify; font-weight: 100">Charges will be 
                    <asp:Label ID="lbl_pkging" runat="server" Text="15" Font-Bold="true"></asp:Label><br />
                </td>
            </tr>--%>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="DELIVERY pagebrake" id="Table4" runat="server" visible="true">
            <tr>
                <td class="" style="text-align: left; font-weight: bold;">SPECIAL NOTE / INSTRUCTIONS</td>
                <td class="" style="text-align: left; font-weight: 100; width: 70%;">
                    <asp:Label ID="lbl_remarks" runat="server" Text="N/A"></asp:Label>&nbsp;<br />
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
            <tr>
                <td colspan="2" class="" style="text-align: justify; font-weight: 100">We trust the above offer is in line with your requirement and we are looking forward to receive your valued order at the earliest.<br />
                    Please feel free to contact us for any further clarifications in this regard.<br />
                    <br />
                    Thanking you and assuring you of our best and prompt services always.<br />
                    <br />
                    <br />
                    <br />
                    Thanks & Regards,
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2" class="gap" style="">&nbsp</td>
            </tr>
        </table>

        <table border="0" width="100%" class="FORKVQAEAST">
            <%--<tr>
                <td class="gap" style="">&nbsp</td>
            </tr>--%>
            <tr class="trheight">
                <td class="" style="text-align: left; font-weight: bold;">FOR FLAME-EX</td>
            </tr>

            <tr>
                <td>
                    <img src="../WebImages/flmx_authsign.png" width="150PX" /></td>
            </tr>

            <tr class="trheight">
                <td class="" style="text-align: left; font-weight: bold;">Authorized Signatory</td>
            </tr>
        </table>

        <table id='footer' border='0' width='844px'>
            <tr>
                <td style='height: auto;' width='100%'>
                    <img src="../WebImages/flame-ex_hdrbtm.png" width='100%' />
                </td>
            </tr>
        </table>

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" OnClientClick="document.getElementById('header').className ='header'; document.getElementById('footer').className ='footer'; window.print()" Text="Print Without Header & Footer" />
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" OnClientClick="window.print()" Text="Print With Header & Footer" />

    </form>
</body>
</html>
