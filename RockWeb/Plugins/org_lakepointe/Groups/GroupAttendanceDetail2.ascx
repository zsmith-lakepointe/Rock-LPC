<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceDetail2.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GroupAttendanceDetail2" %>
<style>
    .member-attendance-photo {
        height:50px;
        width:50px;
    }
    .memberItem{
        height:150px;

        /*border-bottom: 1px solid #000;*/

    }

    .memberItem .memberName {
        font-size:12pt;
        font-weight:600;

    }
</style>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="lbPrintAttendanceRoster" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="Group Attendance" />
                </h1>
                <Rock:ButtonDropDownList ID="bddlCampus" runat="server" FormGroupCssClass="panel-options pull-right" Title="All Campuses" SelectionStyle="Checkmark"
                    OnSelectionChanged="bddlCampus_SelectionChanged" DataTextField="Name" DataValueField="Id" />
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbNotice" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <asp:CustomValidator ID="cvAttendance" runat="server" Display="None" />

                <asp:Panel ID="pnlDetails" runat="server">

                    <div class="row">
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lOccurrenceDate" runat="server" Label="Attendance For" />
                            <Rock:DatePicker ID="dpOccurrenceDate" runat="server" Label="Attendance For" AllowFutureDateSelection="false" Required="true" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lLocation" runat="server" Label="Location" />
                            <Rock:RockDropDownList ID="ddlLocation" runat="server" Label="Location" DataValueField="Key" DataTextField="Value"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged" />
                        </div>
                        <div class="col-sm-4">
                            <Rock:RockLiteral ID="lSchedule" runat="server" Label="Schedule" />
                            <Rock:RockDropDownList ID="ddlSchedule" runat="server" Label="Schedule" DataValueField="Key" DataTextField="Value" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:RockCheckBox ID="cbDidNotMeet" runat="server" Text="We Did Not Meet" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">

                            <div class="js-roster">
                                <div class="panel-labels clearfix">
                                    <h4 class="js-members-label">
                                        <asp:Literal ID="lMembers" runat="server" />
                                    </h4>
                                    <Rock:Toggle ID="tglSort" runat="server" OnText="Last Name" OnCssClass="btn-primary" OffCssClass="btn-outline-primary" ActiveButtonCssClass="btn-primary" ButtonSizeCssClass="btn-xs" OffText="First Name" AutoPostBack="true" OnCheckedChanged="tglSort_CheckedChanged" Checked="true" Label="Sort by" />
                                </div>
<%--                                <asp:ListView ID="lvMembers" runat="server">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("PersonId") %>' />
                                        <Rock:RockCheckBox ID="cbMember" runat="server" Checked='<%# Eval("Attended") %>' OnDataBinding="cbMember_DataBinding"/>
                                    </ItemTemplate>
                                </asp:ListView>--%>
                                <asp:Repeater ID="rptMembers" runat="server" OnItemDataBound="rptMembers_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:Panel id="pnlRow" runat="server">
                                            <div  class="col-xs-6 memberItem" >
                                                <asp:HiddenField ID="hfMember" runat="server" />
                                                <div class="row">
                                                    <div class="col-sm-2 col-xs-4">
                                                        <asp:CheckBox ID="cbMember" runat="server" />
                                                    </div>
                                                    <div class="col-sm-4 col-xs-8">
                                                        <asp:Literal ID="lMemberImage" runat="server" />
                                                    </div>
                                                    <div class="col-sm-6 col-xs-12">
                                                        <span class=" memberName"><asp:Literal ID="lMemberName" runat="server" /></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </asp:Panel>

                                    </ItemTemplate>
                                </asp:Repeater>
                                <div class="pull-left margin-b-md margin-r-md">
                                    <Rock:PersonPicker ID="ppAddPerson" runat="server" OnSelectPerson="ppAddPerson_SelectPerson" />
                                </div>
                                <div class="pull-left margin-b-lg">
                                    <asp:LinkButton ID="lbAddMember" runat="server" CssClass="btn btn-default" OnClick="lbAddMember_Click" CausesValidation="false" Visible="false"><i class="fa fa-plus"></i> Add Group Member</asp:LinkButton>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">

                            <asp:Panel ID="pnlPendingMembers" runat="server" Visible="false">
                                <h4>
                                    <asp:Literal ID="lPendingMembers" runat="server" /></h4>
                                <asp:ListView ID="lvPendingMembers" runat="server" OnItemCommand="lvPendingMembers_ItemCommand">
                                    <ItemTemplate>
                                        <div class="form-group">
                                            <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                                            <asp:Label ID="lName" runat="server" Text='<%# Eval("FullName") %>' />
                                            <asp:LinkButton ID="lbAdd" runat="server" ToolTip="Add Person to Group" CausesValidation="false" CommandName="Add" CommandArgument='<%# Eval("Id") %>' CssClass="js-add-member"><i class="fa fa-plus"></i></asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:ListView>
                            </asp:Panel>

                        </div>
                    </div>
                    <asp:Panel ID="pnlHeadcount" runat="server" CssClass="row" Visible="false">
                        <div class="col-sm-6">
                            <Rock:NumberBox ID="tbHeadCount" NumberType="Integer" runat="server"  Label="Headcount" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockLiteral ID="lDidAttendCount" runat="server" Label="Attendance Count" />
                        </div>
                    </asp:Panel>
        

<%--                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NumberUpDown runat="server" ID="nudAnonymous" CssClass="input-mini" NumberType="Integer" Label="Guests" Minimum="0" />
                        </div>
                    </div>--%>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="dtNotes" runat="server" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.AttendanceOccurrence, Rock" PropertyName="Notes"></Rock:DataTextBox>
                        </div>
                    </div>


                    <Rock:NotificationBox ID="nbPrintRosterWarning" runat="server" NotificationBoxType="Warning" />

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false"></asp:LinkButton>
                        <asp:LinkButton ID="lbPrintAttendanceRoster" runat="server" ToolTip="Print Attendance Roster" CssClass="btn btn-default btn-sm pull-right" OnClick="lbPrintAttendanceRoster_Click" CausesValidation="false"><i class="fa fa-clipboard"></i></asp:LinkButton>
                    </div>

                </asp:Panel>

            </div>

        </div>

        <script>
            Sys.Application.add_load(function () {
                // toggle all checkboxes
                $('.js-members-label').on('click', function (e) {

                    var container = $(this).parent();
                    var isChecked = container.hasClass('all-checked');

                    container.find('input:checkbox').each(function () {
                        $(this).prop('checked', !isChecked);
                    });

                    if (isChecked) {
                        container.removeClass('all-checked');
                    }
                    else {
                        container.addClass('all-checked');
                    }

                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
