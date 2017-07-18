using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trying.Normalizr
{
    public abstract class NormalizeRegistry
    {
        private readonly IDictionary<Type, ISchema> _schamas = new Dictionary<Type, ISchema>();

        protected ISchema Schema<T>(string name, Action<SchemaConfigurator<T>> configuration)
        {
            var schema = Normalizr.Schema(name, configuration);

            if (_schamas.ContainsKey(typeof(T))) throw new Exception($"{typeof(T).Name} is already added");
            _schamas.Add(typeof(T), schema);

            return schema;
        }

        protected ISchema Schema<T>(string name, Action<SchemaConfigurator<T>, NormalizeRegistry> configuration)
        {
            var configurator = new SchemaConfigurator<T>();
            configuration.Invoke(configurator, this);

            var schema = Normalizr.Schema(name, configurator);

            if (_schamas.ContainsKey(typeof(T))) throw new Exception($"{typeof(T).Name} is already added");
            _schamas.Add(typeof(T), schema);

            return schema;
        }

        public ISchema Resolve<T>()
        {
            if (_schamas.TryGetValue(typeof(T), out ISchema schema))
            {
                return schema;
            }

            throw new Exception($"{typeof(T).Name} is not added");
        }
    }
}
