using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trying.Normalizr
{
    public interface ISchema
    {
        PropertyInfo IdAttribute { get; }

        IDictionary<string, ISchema> References { get; }

        Type Type { get; }

        string Name { get; }
    }
}
