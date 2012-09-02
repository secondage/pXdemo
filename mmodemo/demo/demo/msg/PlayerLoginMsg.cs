using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using Beetle;
using Microsoft.Xna.Framework;

namespace demo
{
    public enum LoginResult
    {
        Failed_Password = -4,
        Failed_Notfound = -3,
        Failed_AlreadyLogin = -2,
        Failed = -1,
        Succeed = 1,
    }


    [ProtoContract]
    public class PlayerLoginRequestMsg
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public string Password { get; set; }
    }

    [ProtoContract]
    public class PlayerLoginMsg
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public float[] Position { get; set; }
        [ProtoMember(4)]
        public float Speed { get; set; }
        [ProtoMember(5)]
        public int HP { get; set; }
        [ProtoMember(6)]
        public int MaxHP { get; set; }
        [ProtoMember(7)]
        public int ATK { get; set; }
        [ProtoMember(8)]
        public int DEF { get; set; }
    }


    [ProtoContract]
    public class PlayerLoginSelfMsg
    {
        [ProtoMember(1)]
        public long ClientID { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public float[] Position { get; set; }
        [ProtoMember(4)]
        public float Speed { get; set; }
        [ProtoMember(5)]
        public int HP { get; set; }
        [ProtoMember(6)]
        public int MaxHP { get; set; }
        [ProtoMember(7)]
        public int ATK { get; set; }
        [ProtoMember(8)]
        public int DEF { get; set; }
    }



    [ProtoContract]
    public class PlayerLoginResultMsg
    {
        [ProtoMember(1)]
        public LoginResult Result { get; set; }
        [ProtoMember(2)]
        public long ClientID { get; set; }
        //[ProtoMember(2)]
        //public long ClientID { get; set; }
    }

}
