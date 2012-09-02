using System;
using System.Collections.Generic;
using System.Text;

namespace demo
{
    public class HeadSizePackage:Beetle.HeadSizeOfPackage
    {
        public HeadSizePackage(Beetle.TcpChannel channel)
            : base(channel)
        {
        }
        protected override Beetle.IMessage ReadMessageByType(Beetle.BufferReader reader, out object typeTag)
        {
            typeTag = "ProtobufAdapter";
            return new ProtobufAdapter();
          
        }

        protected override void WriteMessageType(Beetle.IMessage msg, Beetle.BufferWriter writer)
        {
           
        }
    }
}
