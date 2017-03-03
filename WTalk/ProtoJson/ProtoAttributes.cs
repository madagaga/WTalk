using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Core.ProtoJson
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ProtoContractAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ProtoMemberAttribute : Attribute
    {
        int _position;

        public int Position
        {
            get { return _position -1; }
            set { _position = value; }
        }
        public bool Optional { get; set; }       
    }
}
