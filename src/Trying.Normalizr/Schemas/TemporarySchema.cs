using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Trying.Normalizr.Schemas
{
    internal class TemporarySchema : ISchema
    {
        public PropertyInfo IdAttribute { get; }

        public IDictionary<string, Func<object, IDictionary<Type, object>, JToken, object>> Transforms { get; }

        public IDictionary<string, ISchema> References { get; }

        public Type Type { get; }

        public string Name { get; }

        public TemporarySchema(IDictionary<string, ISchema> references)
        {
            References = references;
        }
    }
}
