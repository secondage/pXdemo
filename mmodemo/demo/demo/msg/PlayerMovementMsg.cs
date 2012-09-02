using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using Beetle;
using Microsoft.Xna.Framework;

namespace demo
{
    [ProtoContract]
    public class PlayerMoveRequest
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public float[] Target { get; set; }
        [ProtoMember(3)]
        public float[] Position { get; set; }
    }

    [ProtoContract]
    public class PlayerTargetChanged
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public float[] Target { get; set; }
        [ProtoMember(3)]
        public float[] Position { get; set; }
    }

    [ProtoContract]
    public class PlayerPositionUpdate
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public float[] Position { get; set; }
        [ProtoMember(3)]
        public int State { get; set; }
        [ProtoMember(4)]
        public int Dir { get; set; }
    }

    [ProtoContract]
    public class PlayerPositioReport
    {
        [ProtoMember(1)]
        public float[] Position { get; set; }
    }

    [ProtoContract]
    public class PlayerStopRequest
    {
        [ProtoMember(1)]
        public float[] Position { get; set; }
    }
}
