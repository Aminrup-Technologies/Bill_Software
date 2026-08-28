<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="QuickAction.aspx.cs" Inherits="Bill_Software.corporate.business.app.QuickAction" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Action Processed | Flame-Ex</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f7f6;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }

        .card {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
            text-align: center;
            max-width: 400px;
            width: 90%;
        }

        .icon {
            font-size: 60px;
            margin-bottom: 20px;
        }

        .success {
            color: #28a745;
        }

        .error {
            color: #dc3545;
        }

        h2 {
            margin: 0 0 10px;
            color: #333;
        }

        p {
            color: #666;
            font-size: 15px;
            margin-bottom: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="card">
            <asp:Literal ID="litIcon" runat="server"></asp:Literal>
            <h2>
                <asp:Label ID="lblTitle" runat="server"></asp:Label></h2>
            <p>
                <asp:Label ID="lblMessage" runat="server"></asp:Label></p>
        </div>
    </form>
</body>
</html>
