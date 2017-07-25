using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Trying.Normalizr.Schemas
{
    internal class Schema<T> : ISchema
    {
        public IDictionary<string, Func<object, IDictionary<Type, object>, JToken, object>> Transforms { get; }

        public PropertyInfo IdAttribute { get; }

        public IDictionary<string, ISchema> References { get; }

        public Type Type { get; }

        public string Name { get; }


        public Schema(string name, PropertyInfo idAttribute, IDictionary<string, ISchema> references, IDictionary<string, Func<object, IDictionary<Type, object>, JToken, object>> transforms)
        {
            Name = name;
            IdAttribute = idAttribute;
            References = references;
            Transforms = transforms;
        }
    }
}
