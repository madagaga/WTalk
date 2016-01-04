using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.ProtoJson;

namespace WTalk.Core.Reflection
{
    public class ProtoJsonPropertyAccessor : CompiledPropertyAccessor<object>
    {
        public bool IsProtoContract { get; private set; }
        public bool IsList { get; private set; }
        public Type UnderlyingType { get; private set; }
        public bool IsEnum { get; private set; }

        public bool IsOptional { get; private set; }
        public int Position { get; private set; }

        public ProtoJsonPropertyAccessor(PropertyInfo p) :base(p)
        {
            TypeInfo ti = p.PropertyType.GetTypeInfo();            
            IsList = p.PropertyType != typeof(string) && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ti);

            ProtoMemberAttribute attribute = p.GetCustomAttribute<ProtoMemberAttribute>();
            IsOptional = attribute.Optional;
            Position = attribute.Position;
            
            if (IsList)
            {
                UnderlyingType = ti.GenericTypeArguments.FirstOrDefault();
                ti = UnderlyingType.GetTypeInfo();
            }

            IsProtoContract = ti.IsDefined(typeof(ProtoContractAttribute), false);
            IsEnum = ti.IsEnum;
            
        }
    }
}
