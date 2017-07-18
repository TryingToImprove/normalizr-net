using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trying.Normalizr.Schemas
{
    internal class TemporarySchema : ISchema
    {
        public PropertyInfo IdAttribute { get; }

        public IDictionary<string, ISchema> References { get; }

        public Type Type { get; }

        public string Name { get; }

        public TemporarySchema(IDictionary<string, ISchema> references)
        {
            References = references;
        }
    }
}
