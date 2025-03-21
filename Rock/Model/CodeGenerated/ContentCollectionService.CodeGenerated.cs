//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

using System;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// ContentCollection Service class
    /// </summary>
    public partial class ContentCollectionService : Service<ContentCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCollectionService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public ContentCollectionService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( ContentCollection item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// ContentCollection View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( ContentCollection ) )]
    public partial class ContentCollectionViewModelHelper : ViewModelHelper<ContentCollection, ContentCollectionBag>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override ContentCollectionBag CreateViewModel( ContentCollection model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new ContentCollectionBag
            {
                IdKey = model.IdKey,
                CollectionKey = model.CollectionKey,
                Description = model.Description,
                EnableRequestFilters = model.EnableRequestFilters,
                EnableSegments = model.EnableSegments,
                FilterSettings = model.FilterSettings,
                LastIndexDateTime = model.LastIndexDateTime,
                LastIndexItemCount = model.LastIndexItemCount,
                Name = model.Name,
                TrendingEnabled = model.TrendingEnabled,
                TrendingGravity = model.TrendingGravity,
                TrendingMaxItems = model.TrendingMaxItems,
                TrendingWindowDay = model.TrendingWindowDay,
                CreatedDateTime = model.CreatedDateTime,
                ModifiedDateTime = model.ModifiedDateTime,
                CreatedByPersonAliasId = model.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = model.ModifiedByPersonAliasId,
            };

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );
            return viewModel;
        }
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class ContentCollectionExtensionMethods
    {
        /// <summary>
        /// Clones this ContentCollection object to a new ContentCollection object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static ContentCollection Clone( this ContentCollection source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as ContentCollection;
            }
            else
            {
                var target = new ContentCollection();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this ContentCollection object to a new ContentCollection object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ContentCollection CloneWithoutIdentity( this ContentCollection source )
        {
            var target = new ContentCollection();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another ContentCollection object to this ContentCollection object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this ContentCollection target, ContentCollection source )
        {
            target.Id = source.Id;
            target.CollectionKey = source.CollectionKey;
            target.Description = source.Description;
            target.EnableRequestFilters = source.EnableRequestFilters;
            target.EnableSegments = source.EnableSegments;
            target.FilterSettings = source.FilterSettings;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.LastIndexDateTime = source.LastIndexDateTime;
            target.LastIndexItemCount = source.LastIndexItemCount;
            target.Name = source.Name;
            target.TrendingEnabled = source.TrendingEnabled;
            target.TrendingGravity = source.TrendingGravity;
            target.TrendingMaxItems = source.TrendingMaxItems;
            target.TrendingWindowDay = source.TrendingWindowDay;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }

        /// <summary>
        /// Creates a view model from this entity
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson" >The currentPerson.</param>
        /// <param name="loadAttributes" >Load attributes?</param>
        public static ContentCollectionBag ToViewModel( this ContentCollection model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new ContentCollectionViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
