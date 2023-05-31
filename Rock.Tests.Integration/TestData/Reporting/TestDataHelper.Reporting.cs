// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Reporting.DataFilter;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Reporting
        {
            #region Data Views

            public class CreateDataViewArgs : CreateEntityActionArgsBase
            {
                public string Name;
                public string AppliesToEntityTypeIdentifier;
            }

            public static DataView CreateDataView( CreateDataViewArgs args )
            {
                DataView newDataView = null;

                var rockContext = new RockContext();
                var dataViewService = new DataViewService( rockContext );
                if ( args.Guid != null )
                {
                    newDataView = dataViewService.Get( args.Guid.Value );
                    if ( newDataView != null )
                    {
                        if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( args.ExistingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteDataView( args.Guid.Value );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }

                            newDataView = null;
                        }
                    }
                }

                if ( newDataView == null )
                {
                    newDataView = new DataView();
                }

                var entityTypeService = new EntityTypeService( rockContext );
                var entityType = entityTypeService.GetByIdentifierOrThrow( args.AppliesToEntityTypeIdentifier );

                newDataView.EntityTypeId = entityType.Id;
                newDataView.Name = args.Name;
                newDataView.Guid = args.Guid ?? Guid.NewGuid();
                newDataView.ForeignKey = args.ForeignKey;

                dataViewService.Add( newDataView );

                rockContext.SaveChanges();

                // Add a root-level filter to return all records.
                newDataView.DataViewFilter = new DataViewFilter
                {
                    ExpressionType = FilterExpressionType.Filter,
                    EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Reporting.DataFilter.PropertyFilter ) )
                };

                rockContext.SaveChanges();

                return newDataView;
            }

            public static bool DeleteDataView( Guid dataViewGuid )
            {
                bool success = false;
                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    success = DeleteDataView( rockContext, dataViewGuid );

                    rockContext.SaveChanges();
                } );

                return success;
            }

            /// <summary>
            /// Remove DataViews flagged with the current test record tag.
            /// </summary>
            /// <param name="dataContext"></param>
            /// <returns></returns>
            public static bool DeleteDataView( RockContext dataContext, Guid dataViewGuid )
            {
                var dataViewService = new DataViewService( dataContext );

                var dataView = dataViewService.Get( dataViewGuid );

                if ( dataView == null )
                {
                    return false;
                }

                var dataViewFilterService = new DataViewFilterService( dataContext );

                DeleteDataViewFilter( dataView.DataViewFilter, dataViewFilterService, dataContext );

                dataView = dataViewService.Get( dataViewGuid );

                if ( dataView != null )
                {
                    var isDeleted = dataViewService.Delete( dataView );

                    if ( !isDeleted )
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Deletes the data view filter.
            /// </summary>
            /// <param name="dataViewFilter">The data view filter.</param>
            /// <param name="service">The service.</param>
            private static void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service, RockContext rockContext )
            {
                if ( dataViewFilter == null )
                {
                    return;
                }

                rockContext = rockContext ?? new RockContext();

                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service, rockContext );
                }

                dataViewFilter.DataViewId = null;
                dataViewFilter.RelatedDataViewId = null;

                rockContext.SaveChanges();

                service.Delete( dataViewFilter );

                rockContext.SaveChanges();
            }

            /// <summary>
            /// Remove DataViews flagged with the current test record tag.
            /// </summary>
            /// <param name="dataContext"></param>
            /// <returns></returns>
            public static int DeleteDataViewsByRecordTag( RockContext dataContext, string recordTag )
            {
                // Remove DataViews associated with the current test record tag.
                var dataViewService = new DataViewService( dataContext );

                var dataViewGuidList = dataViewService.Queryable()
                    .Where( dv => dv.ForeignKey == recordTag )
                    .Select( dv => dv.Guid )
                    .ToList();

                var recordsDeleted = 0;
                foreach ( var guid in dataViewGuidList )
                {
                    var success = DeleteDataView( dataContext, guid );
                    if ( success )
                    {
                        recordsDeleted++;
                    }
                }

                TestHelper.Log( $"Delete Test Data: {recordsDeleted} DataViews deleted." );

                return recordsDeleted;
            }

            #endregion

            public static void AddDataViewsForGroupsModule()
            {
                const string _recordTag = "GroupsTestData";

                var dataContext = new RockContext();

                // Remove existing Data Views.
                DeleteDataView( dataContext, TestGuids.DataViews.LocationsInsideArizona.AsGuid() );
                DeleteDataView( dataContext, TestGuids.DataViews.LocationsOutsideArizona.AsGuid() );

                dataContext.SaveChanges();

                // Add Data View Category "Groups".
                const string categoryDataViewName = "Groups";

                TestHelper.Log( $"Adding Data View Category \"{ categoryDataViewName }\"..." );

                var entityTypeId = EntityTypeCache.Get( typeof( global::Rock.Model.DataView ) ).Id;

                var coreHelper = new CoreModuleDataFactory( _recordTag );

                var locationsCategory = coreHelper.CreateCategory( categoryDataViewName, TestGuids.Category.DataViewLocations.AsGuid(), entityTypeId );

                coreHelper.AddOrUpdateCategory( dataContext, locationsCategory );

                dataContext.SaveChanges();

                // Get Data View service.
                int categoryId = CategoryCache.GetId( TestGuids.Category.DataViewLocations.AsGuid() ) ?? 0;

                DataViewFilter rootFilter;

                // Create Data View: Locations Inside Arizona
                const string dataViewLocationsInsideArizona = "Locations in the state of Arizona";

                TestHelper.Log( $"Adding Data View \"{ dataViewLocationsInsideArizona }\"..." );

                var service = new DataViewService( dataContext );

                var dataViewInside = new DataView();
                dataViewInside.IsSystem = false;
                dataViewInside.Name = dataViewLocationsInsideArizona;
                dataViewInside.Description = "Locations that are within the state of Arizona.";
                dataViewInside.EntityTypeId = EntityTypeCache.GetId( typeof( global::Rock.Model.Location ) );
                dataViewInside.CategoryId = categoryId;
                dataViewInside.Guid = TestGuids.DataViews.LocationsInsideArizona.AsGuid();
                dataViewInside.ForeignKey = _recordTag;

                service.Add( dataViewInside );

                dataContext.SaveChanges();

                rootFilter = new DataViewFilter();
                rootFilter.ExpressionType = FilterExpressionType.GroupAll;

                dataViewInside.DataViewFilter = rootFilter;

                var inStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.EqualTo, Value = "AZ" };

                rootFilter.ChildFilters.Add( inStateFilter.GetFilter() );

                dataContext.SaveChanges();

                // Create Data View: Locations Outside Arizona
                const string dataViewLocationsOutsideArizona = "Locations outside Arizona";

                TestHelper.Log( $"Adding Data View \"{ dataViewLocationsOutsideArizona }\"..." );

                var dataViewOutside = new DataView();
                dataViewOutside.IsSystem = false;
                dataViewOutside.Name = dataViewLocationsOutsideArizona;
                dataViewOutside.Description = "Locations that are not within the state of Arizona.";
                dataViewOutside.EntityTypeId = EntityTypeCache.GetId( typeof( global::Rock.Model.Location ) );
                dataViewOutside.CategoryId = categoryId;
                dataViewOutside.Guid = TestGuids.DataViews.LocationsOutsideArizona.AsGuid();
                dataViewOutside.ForeignKey = _recordTag;

                service.Add( dataViewOutside );

                dataContext.SaveChanges();

                rootFilter = new DataViewFilter();

                rootFilter.ExpressionType = FilterExpressionType.GroupAll;
                dataViewOutside.DataViewFilter = rootFilter;

                var notInStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.NotEqualTo, Value = "AZ" };

                rootFilter.ChildFilters.Add( notInStateFilter.GetFilter() );

                dataContext.SaveChanges();
            }
        }
    }
}
