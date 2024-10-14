using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Rock;
using Rock.Data;
using Rock.Model;
public partial class Plugins_org_lakepointe_HelloWorld : Rock.Web.UI.RockBlock
{
   

protected override void OnLoad( EventArgs e )
    {
        base.OnLoad( e );

        if ( !Page.IsPostBack )
        {
            var currentPerson = GetCurrentPerson();
        }
    }

    private Person GetCurrentPerson()
    {
        var personId = CurrentPersonId; // This will give you the ID of the current person
        if ( personId.HasValue )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PersonService( rockContext ).Get( personId.Value );
            }
        }
        return null;
    }
    protected void btnChangeText_Click( object sender, EventArgs e )
    {
        if ( CurrentPerson == null )
        {
            lblHello.InnerText = "Hello Stranger!";
        }
        else {
            lblHello.InnerText = $"Hello {CurrentPerson.NickName}";
        }
        
    }
}