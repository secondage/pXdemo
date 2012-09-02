using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
namespace demo
{
    [ProtoContract]
    public class Register
    {
        [ProtoMember(1)]
        public string UserName { get; set; }
        [ProtoMember(2)]
        public string PWD { get; set; }
        [ProtoMember(3)]
        public string EMail { get; set; }
    }
}
