﻿<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>

<main>
        <div class="container">
			
			<!-- Start Content Area -->
			
			<!-- Ajax Error -->
			<div class="alert alert-danger ajax-error" style="display:none">
				<p><strong>Error</strong></p>
				<span class="ajax-error-message"></span>
			</div>

			<div class="row">
				<div class="col-md-12">
					<Rock:Zone Name="Feature" runat="server" />
				</div>
			</div>

			<div class="row">
				<div class="col-md-12">
					<Rock:Zone Name="Main" runat="server" />
				</div>
			</div>
		</div>
		<div class="container-fluid">
		

			<Rock:Zone Name="Section A" runat="server" />
			

			<div class="row">
				<div class="col-md-4">
					<Rock:Zone Name="Section B" runat="server" />
				</div>
				<div class="col-md-4">
					<Rock:Zone Name="Section C" runat="server" />
				</div>
				<div class="col-md-4">
					<Rock:Zone Name="Section D" runat="server" />
				</div>
			</div>
		</div>

        <!-- End Content Area -->

	</main>
        
</asp:Content>

