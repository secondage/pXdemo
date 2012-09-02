using System;
using System.Collections.Generic;
using System.Text;
using Beetle;
namespace demo
{
    public class ProtobufAdapter:IMessage
    {
        public object Message
        {
            get;
            set;
        }
        public static bool Send(Beetle.TcpChannel channel, object msg)
        {
            ProtobufAdapter adapter = new ProtobufAdapter();
            adapter.Message = msg;
            return channel.Send(adapter);

        }
        public void Load(Beetle.BufferReader reader)
        {
            string type = "demo.";
            type += reader.ReadString();
            Beetle.ByteArraySegment segment = mArrayPool.Pop();
            reader.ReadByteArray(segment);
            using (System.IO.Stream stream = new System.IO.MemoryStream(segment.Array,0,segment.Count))
            {
                Message = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, Type.GetType(type));
            }
            mArrayPool.Push(segment);
        }
        public void Save(Beetle.BufferWriter writer)
        {
            writer.Write(Message.GetType().Name);
            Beetle.ByteArraySegment segment = mArrayPool.Pop();
            using(System.IO.Stream stream = new System.IO.MemoryStream(segment.Array))
            {
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, Message);
                segment.SetInfo(0, (int)stream.Position);
                
            }
            writer.Write(segment);
            mArrayPool.Push(segment);
            
        }
        private static ByteArrayPool mArrayPool = new ByteArrayPool(100, 1024 * 8);
    }
}
