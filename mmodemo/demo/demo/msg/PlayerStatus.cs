using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace demo
{
    [ProtoContract]
    public class PlayerStatus
    {
        [ProtoMember(1)]
        public long id { get; set; }
        [ProtoMember(2)]
        public int x { get; set; }
        [ProtoMember(3)]
        public int y { get; set; }
        [ProtoMember(4)]
        public int layer { get; set; }
        [ProtoMember(5)]
        public int state { get; set; }
    }
}
