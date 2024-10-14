<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonGroupPicker.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Tutorials.PersonGroupPicker" %>

<asp:Label ID="lblPerson" runat="server" Text="Select Person:" />
<Rock:PersonPicker ID="ppPerson" runat="server" />
<br />
<asp:Label ID="lblGroup" runat="server" Text="Select Group:" />
<Rock:GroupPicker ID="gpGroup" runat="server" />
<br />
<asp:Button ID="btnAddToGroup" runat="server" Text="Add to Group" OnClick="btnAddToGroup_Click" />
<asp:Label ID="lblResult" runat="server" Text="" />