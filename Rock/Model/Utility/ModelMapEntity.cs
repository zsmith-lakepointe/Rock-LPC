using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Class ModelMapEntity. This class allows non RockEntity&lt;T&gt; types to be displayed in the Rock ModelMap with impersonation by implementing <see cref="Rock.Data.IEntity" />.
    /// </summary>
    /// <seealso cref="Rock.Data.IEntity" />
    public class ModelMapEntity : IEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [NotMapped]
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets the <see cref="Id" /> as a hashed identifier that can be used
        /// to lookup the entity later. This hides the actual <see cref="Id" />
        /// number so that individuals cannot attempt to guess the next sequential
        /// identifier numbers.
        /// </summary>
        /// <value>The hashed identifier key.</value>
        public string IdKey { get; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the int foreign identifier.
        /// </summary>
        /// <value>The int foreign identifier.</value>
        public virtual int? ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the Guid foreign identifier.
        /// </summary>
        /// <value>The Guid foreign identifier.</value>
        public virtual Guid? ForeignGuid { get; set; }

        /// <summary>
        /// Gets or sets the string foreign identifier.
        /// </summary>
        /// <value>The string foreign identifier.</value>
        public virtual string ForeignKey { get; set; }

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>The type id.</value>
        public virtual int TypeId { get; }

        /// <summary>
        /// Gets the unique type name of the entity.  Typically this is the qualified name of the class
        /// </summary>
        /// <value>The name of the entity type.</value>
        public virtual string TypeName { get; }

        /// <summary>
        /// Gets the encrypted key.
        /// </summary>
        /// <value>The encrypted key.</value>
        public virtual string EncryptedKey { get; }

        /// <summary>
        /// Gets the context key.
        /// </summary>
        /// <value>The context key.</value>
        public virtual string ContextKey { get; }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        /// <value>The validation results.</value>
        [NotMapped]
        public virtual List<ValidationResult> ValidationResults { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public virtual bool IsValid { get; }

        /// <summary>
        /// Gets or sets the additional lava fields.
        /// </summary>
        /// <value>The additional lava fields.</value>
        public virtual Dictionary<string, object> AdditionalLavaFields { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>IEntity.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual IEntity Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a dictionary containing the majority of the entity object's properties
        /// </summary>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Dictionary<string, object> ToDictionary()
        {
            throw new NotImplementedException();
        }
    }
}
