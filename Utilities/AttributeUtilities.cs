using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace CIAResearch.Utilities
{
    internal static class AttributeUtilities
    {
        internal static List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var entityType = EntityTypeCache.Get( typeof( CIAResearch ) );
            if ( entityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == entityType.Id )
                    .ToList();
            }

            return null;
        }

        internal static string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }
    }
}
