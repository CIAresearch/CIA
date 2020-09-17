using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using CIAResearch;

namespace com_ciaresearch.Blocks.BackgroundCheck
{
    [DisplayName( "CIA Request List" )]
    [Category( "CIA" )]
    [Description( "Lists all the CIA background check requests." )]

    public partial class CIAResearchRequestList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fRequest.ApplyFilterClick += fRequest_ApplyFilterClick;
            fRequest.DisplayFilterValue += fRequest_DisplayFilterValue;

            gRequest.DataKeyNames = new string[] { "Id" };
            gRequest.Actions.ShowAdd = false;
            gRequest.IsDeleteEnabled = false;
            gRequest.RowDataBound += gRequest_RowDataBound;
            gRequest.GridRebind += gRequest_GridRebind;
            gRequest.RowSelected += gRequest_RowSelected;
        }

        protected void gRequest_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var item = e.Row.DataItem as BackgroundCheckItem;
            if ( item != null )
            {
                if ( string.IsNullOrWhiteSpace( item.ResponseDate ) )
                {
                    e.Row.Cells[4].CssClass = "clearinside";
                    e.Row.Cells[5].CssClass = "clearinside";
                    e.Row.Cells[6].CssClass = "clearinside";
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fRequest_ApplyFilterClick( object sender, EventArgs e )
        {
            fRequest.SaveUserPreference( "First Name", tbFirstName.Text );
            fRequest.SaveUserPreference( "Last Name", tbLastName.Text );
            fRequest.SaveUserPreference( "Request Date Range", drpRequestDates.DelimitedValues );
            fRequest.SaveUserPreference( "Response Date Range", drpResponseDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRequest_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Request Date Range":
                case "Response Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRequest_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null && bc.PersonAlias != null )
                {
                    int personId = e.RowKeyId;
                    try
                    {
                        Response.Redirect( string.Format( "~/Person/{0}", bc.PersonAlias.PersonId ), false );
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    catch ( ThreadAbortException )
                    {
                        // Can safely ignore this exception
                    }
                }
            }
        }

        protected void gRequest_Data( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null && bc.ResponseDate != null )
                {
                    var document = bc.ResponseDocument;
                    if ( document == null )
                    {
                        return;
                    }

                    var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" );
                    Response.Redirect( filePath + "?Guid=" + document.Guid.ToString() );
                }
            }
        }

        protected void gRequest_Rerequest( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null )
                {
                    bc.ResponseDate = null;
                    bc.ResponseData = null;
                    bc.ResponseDocumentId = null;
                    rockContext.SaveChanges();
                }

                CIAResearch.CIAResearch.ClearReport( bc, rockContext );
            }
        }

        protected void gRequest_Link( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null && bc.ResponseDate != null )
                {
                    Response.Redirect( CIAResearch.CIAResearch.GenerateReportUrl( bc.RequestId ) );
                }
            }
        }

        #endregion
        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbFirstName.Text = fRequest.GetUserPreference( "First Name" );
            tbLastName.Text = fRequest.GetUserPreference( "Last Name" );
            drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
            drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.Get( typeof( CIAResearch.CIAResearch ) ).Id;
                var qry = new BackgroundCheckService( rockContext )
                    .Queryable( "PersonAlias.Person" ).AsNoTracking()
                    .Where( g =>
                        g.PersonAlias != null &&
                        g.PersonAlias.Person != null )
                    .Where( g =>
                        g.ProcessorEntityTypeId == entityTypeId );

                // FirstName
                string firstName = fRequest.GetUserPreference( "First Name" );
                if ( !string.IsNullOrWhiteSpace( firstName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                        t.PersonAlias.Person.NickName.StartsWith( firstName ) );
                }

                // LastName
                string lastName = fRequest.GetUserPreference( "Last Name" );
                if ( !string.IsNullOrWhiteSpace( lastName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.LastName.StartsWith( lastName ) );
                }

                // Request Date Range
                var drpRequestDates = new DateRangePicker();
                drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
                if ( drpRequestDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.RequestDate >= drpRequestDates.LowerValue.Value );
                }

                if ( drpRequestDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpRequestDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.RequestDate < upperDate );
                }

                // Response Date Range
                var drpResponseDates = new DateRangePicker();
                drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
                if ( drpResponseDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.ResponseDate >= drpResponseDates.LowerValue.Value );
                }

                if ( drpResponseDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpResponseDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.ResponseDate < upperDate );
                }

                List<Rock.Model.BackgroundCheck> items = null;

                items = qry.OrderByDescending( d => d.RequestDate ).ToList();

                gRequest.DataSource = items.Select( b => new BackgroundCheckItem
                {
                    Name = b.PersonAlias.Person.LastName + ", " + b.PersonAlias.Person.NickName,
                    Id = b.Id,
                    BackgroundCheck = b,
                    PersonId = b.PersonAlias.PersonId,
                    Person = b.PersonAlias.Person,
                    HasWorkflow = b.WorkflowId.HasValue,
                    RequestDate = b.RequestDate.ToString( "MM/dd/yyyy" ),
                    ResponseDate = b.ResponseDate.HasValue ? b.ResponseDate.Value.ToString( "MM/dd/yyyy" ) : "",
                    TrackingNumber = b.RequestId
                } ).ToList();

                gRequest.DataBind();
            }
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        private class BackgroundCheckItem : Rock.Utility.RockDynamic
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public Rock.Model.BackgroundCheck BackgroundCheck { get; set; }
            public int PersonId { get; set; }
            public Person Person { get; set; }
            public bool HasWorkflow { get; set; }
            public string RequestDate { get; set; }
            public string ResponseDate { get; set; }
            public string TrackingNumber { get; set; }
        }
        #endregion
    }
}