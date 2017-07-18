using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Trying.Normalizr
{
    public class TemporarySchemaConfigurator
    {
        internal IDictionary<string, ISchema> References { get; } = new Dictionary<string, ISchema>();

        public void Reference(string property, ISchema schema)
        {
            References.Add(property, schema);
        }
    }
}