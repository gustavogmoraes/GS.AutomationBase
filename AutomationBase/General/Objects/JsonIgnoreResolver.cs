using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AutomationBase.General.Objects
{
    public class JsonIgnoreResolver : DefaultContractResolver
    {
        private readonly HashSet<string> _ignoreProps;

        public JsonIgnoreResolver(IEnumerable<string> propNamesToIgnore, bool ignore = true)
        {
            _ignoreProps = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (_ignoreProps.Contains(property.PropertyName))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }
}