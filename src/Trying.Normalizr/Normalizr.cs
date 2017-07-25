using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Trying.Normalizr.Schemas;

namespace Trying.Normalizr
{
    public class Normalizr
    {
        private readonly IDictionary<ISchema, IDictionary<JToken, JToken>> _entities = new Dictionary<ISchema, IDictionary<JToken, JToken>>();
        private readonly IDictionary<Type, object> _resolver;

        private readonly ISchema _schema;

        public Normalizr(ISchema schema, IDictionary<Type, object> resolver = null)
        {
            _resolver = resolver;
            _schema = schema;
        }

        public NormalizedResult Normalize(object input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var queue = new Queue<Walkable>();
            object result = null;

            if (_schema is TemporarySchema)
            {
                var token = JToken.FromObject(input);
                if (token == null) throw new InvalidOperationException();

                queue.Enqueue(new Walkable(_schema, token));
            }
            else
            {
                if (input is IEnumerable)
                {
                    var tokenArray = JArray.FromObject(input);
                    if (tokenArray == null) throw new InvalidOperationException();

                    foreach (var item in tokenArray) queue.Enqueue(new Walkable(_schema, item));
                    result = tokenArray.Select(x => x[_schema.IdAttribute.Name].ToObject<object>()).ToList();
                }
                else
                {
                    var token = JToken.FromObject(input);
                    if (token == null) throw new InvalidOperationException();

                    queue.Enqueue(new Walkable(_schema, token));
                    result = token[_schema.IdAttribute.Name].ToObject<object>();
                }
            }

            while (queue.Count > 0)
            {
                var workingItem = queue.Dequeue();

                if (!(workingItem.Schema is TemporarySchema))
                {
                    if (!_entities.TryGetValue(workingItem.Schema, out IDictionary<JToken, JToken> entities))
                    {
                        entities = new Dictionary<JToken, JToken>();
                        _entities.Add(workingItem.Schema, entities);
                    }

                    if (!entities.TryGetValue(workingItem.Object[workingItem.Schema.IdAttribute.Name],
                        out JToken entity))
                    {
                        if (workingItem.Schema.Transforms.Any())
                        {
                            foreach (var transformPair in workingItem.Schema.Transforms)
                            {
                                workingItem.Object[transformPair.Key] = JToken.FromObject(
                                    transformPair.Value.Invoke(workingItem.Object[transformPair.Key], _resolver, workingItem.Object)
                                );
                            }
                        }

                        entities.Add(workingItem.Object[workingItem.Schema.IdAttribute.Name], workingItem.Object);
                    }
                }

                foreach (var schemaSetup in workingItem.Schema.References)
                {
                    var token = workingItem.Object[schemaSetup.Key];
                    if (token == null || !token.HasValues) continue;

                    if (token.Type == JTokenType.Array)
                    {
                        var tokenArray = token as JArray;
                        if (tokenArray == null) throw new InvalidOperationException();

                        foreach (var item in tokenArray) queue.Enqueue(new Walkable(schemaSetup.Value, item));
                        workingItem.Object[schemaSetup.Key] = JToken.FromObject(tokenArray.Select(x => x[schemaSetup.Value.IdAttribute.Name]));
                    }
                    else
                    {
                        queue.Enqueue(new Walkable(schemaSetup.Value, token));
                        workingItem.Object[schemaSetup.Key] = token[schemaSetup.Value.IdAttribute.Name];
                    }
                }
            }

            return new NormalizedResult
            {
                Entities = _entities.ToDictionary(x => x.Key.Name, x => x.Value.ToDictionary(y => y.Key.ToObject<object>(), y => (object)y.Value)),
                Result = result
            };
        }

        private class Walkable
        {
            public Walkable(ISchema schema, JToken data)
            {
                Schema = schema;
                Object = data;
            }

            public ISchema Schema { get; }

            public JToken Object { get; }
        }

        public static ISchema Schema<T>(string name, Action<SchemaConfigurator<T>> configuration)
        {
            var configurator = new SchemaConfigurator<T>();

            configuration.Invoke(configurator);

            return Schema(name, configurator);
        }

        internal static ISchema Schema<T>(string name, SchemaConfigurator<T> configuration)
        {
            return new Schema<T>(name, configuration.IdAttribute, configuration.References, configuration.Transforms);
        }

        public static NormalizedResult Normalize(ISchema schema, object input, IDictionary<Type, object> resolver = null)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var normalizr = new Normalizr(schema, resolver);
            return normalizr.Normalize(input);
        }

        public static ISchema Temporary(Action<TemporarySchemaConfigurator> configuration)
        {
            var configurator = new TemporarySchemaConfigurator();

            configuration.Invoke(configurator);

            return new TemporarySchema(configurator.References);
        }
    }
}
