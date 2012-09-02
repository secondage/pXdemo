using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using Beetle;
using Microsoft.Xna.Framework;

namespace demo
{
    [ProtoContract]
    public class PlayerTimeSyncMsg
    {
        [ProtoMember(1)]
        public double Duration { get; set; }
        [ProtoMember(2)]
        public double Total { get; set; }
    }

    

}
