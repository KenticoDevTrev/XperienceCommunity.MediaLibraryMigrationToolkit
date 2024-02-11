<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Run.aspx.cs" Inherits="CMSApp.CMSPages.MigrationTemp.Run" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <p>
                1. Build the Media List (based on Configurations)
                <asp:Button runat="server" ID="btnBuildList" OnClick="btnBuildList_Click" Text="Build Media List" />
            </p>
            <p>
                2. Start converting Attachments and relative links to Media Files with /getmedia link
                <asp:Button runat="server" ID="btnConvert" OnClick="btnConvert_Click" Text="Convert" />
            </p>
            <p>
                3. Scan and report on media files now in use
                <asp:Button runat="server" ID="btnCheckResult" OnClick="btnCheckResults" Text="Check Results" />
            </p>
            <p>
                4. Migrate to Azure (First set Azure configurations), this only does Media Files
                <asp:Button runat="server" ID="btnMigrateToAzure" OnClick="btnMigrateToAzure_Click" Text="Migrate to Azure" />
            </p>
            <p>
                5. Clear Attachments (make sure to check media files for errors first)
                <asp:Button runat="server" ID="btnRemoveAttachments" OnClick="btnRemoveAttachments_Click" Text="Clear Attachments" />
            </p>
        </div>
    </form>
</body>
</html>
