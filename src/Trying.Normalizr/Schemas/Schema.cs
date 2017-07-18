using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trying.Normalizr.Schemas
{
    internal class Schema<T> : ISchema
    {
        public PropertyInfo IdAttribute { get; }

        public IDictionary<string, ISchema> References { get; }

        public Type Type { get; }

        public string Name { get;  }

        public Schema(string name, PropertyInfo idAttribute, IDictionary<string, ISchema> references)
        {
            Name = name;
            IdAttribute = idAttribute;
            Type = typeof(T);
            References = references;
        }
    }
}
