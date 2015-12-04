using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WTalk.ProtoJson
{
     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ProtoMemberAttribute:Attribute
    {
        public int Position { get; private set; }
        public bool Optional { get; private set; }
        public ProtoMemberAttribute(int position, bool optional = false)
        {
            this.Position = position -1;
            this.Optional = optional;
        }
    }
}
