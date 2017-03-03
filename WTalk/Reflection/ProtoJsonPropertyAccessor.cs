using coreJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.ProtoJson;

namespace WTalk.Core.Reflection
{
    [DebuggerDisplay("Name={Name}, Position={Position}")]
    public class ProtoJsonPropertyAccessor : CompiledPropertyAccessor<object>
    {
        public AttributeTypeEnum ProtoJsonType { get; private set; }
        public bool IsOptional { get; private set; }
        public int Position { get; private set; }
        
        
        public ProtoJsonPropertyAccessor(PropertyInfo p) :base(p)
        {

            // after base is initialized GenericType contains 
            TypeInfo ti = GenericType.GetTypeInfo();     

            ProtoMemberAttribute attribute = p.GetCustomAttribute<ProtoMemberAttribute>();
            if (attribute != null)
            {
                IsOptional = attribute.Optional;
                Position = attribute.Position;
            }

            ProtoJsonType = AttributeTypeEnum.Value;

            if (IsEnum)
                ProtoJsonType = AttributeTypeEnum.Enum;            
            else if (ti.IsDefined(typeof(ProtoContractAttribute), false))
                    ProtoJsonType = AttributeTypeEnum.ProtoContract;
                
            
            
        }
    }
}
