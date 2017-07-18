using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Trying.Normalizr
{
    public class SchemaConfigurator<T>
    {
        internal PropertyInfo IdAttribute { get; set; }

        internal IDictionary<string, ISchema> References { get; } = new Dictionary<string, ISchema>();

        public void Reference(Expression<Func<T, object>> propertySelector, ISchema schema)
        {
            var expressionBody = GetMemberExpression(propertySelector);
            if (expressionBody == null) throw new ArgumentException(nameof(propertySelector));

            var propertyInfo = expressionBody.Member as PropertyInfo;
            if (propertyInfo == null) throw new ArgumentException(nameof(propertySelector));

            References.Add(propertyInfo.Name, schema);
        }

        public void Id(Expression<Func<T, object>> propertySelector)
        {
            var expressionBody = GetMemberExpression(propertySelector);
            if (expressionBody == null) throw new ArgumentException(nameof(propertySelector));

            var propertyInfo = expressionBody.Member as PropertyInfo;
            if (propertyInfo == null) throw new ArgumentException(nameof(propertySelector));

            IdAttribute = propertyInfo;
        }

        private static MemberExpression GetMemberExpression(Expression<Func<T, object>> expression)
        {
            var body = expression.Body;
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            return body as MemberExpression;
        }
    }
}