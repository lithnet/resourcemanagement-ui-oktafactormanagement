<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Factors.aspx.cs" Inherits="Lithnet.ResourceManagement.UI.OktaFactorManagement.Factors" UICulture="auto" Culture="auto" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title runat="server" id="pageTitle">
        <asp:Literal runat="server" Text="<%$Resources:PageTitle%>"></asp:Literal></title>
    <link rel="stylesheet" href="styles.css" />
    <link rel="stylesheet" href="common-layout.css" />
</head>

<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div class="main">
            <div class="wrapper">
                <div id="header" class="lithnet-header">
                    <img src="lithnet16.png" alt="Lithnet" />
                </div>
                <h1>
                    <asp:Label ID="lbHeader" runat="server" Text="<%$Resources:PageTitle%>" />
                </h1>

                <div class="formcontent">
                    <asp:Table runat="server" ID="userInfoTable" CssClass="userDataTable" />

                    <asp:Table runat="server" ID="factorTable" CssClass="factorTable" />

                    <asp:UpdatePanel runat="server" ID="up2" UpdateMode="Conditional">

                        <ContentTemplate>

                            <div id="resultRow" runat="server">
                                <div id="divPasswordSetMessage" runat="server">
                                    <asp:Label ID="lbPasswordSetMessage" runat="server" />
                                </div>

                                <div id="divWarning" class="warning" runat="server">
                                    <asp:Label ID="lbWarning" runat="server" />
                                </div>
                            </div>

                            <div class="buttonRow">
                                <asp:Button ID="btClose"
                                    runat="server"
                                    OnClientClick="ClosePage(); return false;"
                                    CssClass="button"
                                    Visible="true"
                                    Text="<%$Resources:Close%>" />
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>

    </form>

    <script>
        function ClosePage() {
            open(location, '_self').close();
            return false;
        }

        function ResetPage() {
            window.location.href = "<%=this.Request.RawUrl%>";
        }
    </script>
</body>
</html>
