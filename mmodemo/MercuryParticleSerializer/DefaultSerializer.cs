namespace MercuryParticleSerializer
{
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
    using ProjectMercury;
    using ProjectMercury.ContentPipeline;

    //[Export(typeof(ISerializerPlugin))]
    public sealed class DefaultSerializer 
    {
        public ParticleEffect Deserialize(string filePath)
        {
            XDocument xmlDocument = XDocument.Load(filePath);

            using (XmlReader reader = xmlDocument.CreateReader())
            {
                
                return IntermediateSerializer.Deserialize<ParticleEffect>(reader, ".\\");
            }
        }
    }
}
