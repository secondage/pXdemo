using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ProjectMercury;
using ProjectMercury.Renderers;
using System.Reflection;
using System.IO;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace demo
{
    public class ParticleEffectWrapper : RenderChunk
    {
        static private Matrix idmatrix = new Matrix();
        static private Vector3 campos = new Vector3();
        static private SpriteBatchRenderer spritebatchrenderer;
#if WINDOWS
        static Assembly assemblyMercuryParticleSerializer;
        static Assembly assemblyMercuryContentPipeline;
#endif


        public static void Initialise(IGraphicsDeviceService g)
        {
#if WINDOWS
            assemblyMercuryParticleSerializer = Assembly.LoadFrom(Path.Combine(Application.StartupPath, "MercuryParticleSerializer.dll"));
            assemblyMercuryContentPipeline = Assembly.LoadFrom(Path.Combine(Application.StartupPath, "ProjectMercury.ContentPipeline.dll"));
#endif
            spritebatchrenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = g,
                Transformation = Matrix.CreateTranslation(0, 0, 0),
            };
            spritebatchrenderer.LoadContent(null);
        }

        public static void SetTransformation(Matrix matrix)
        {
            spritebatchrenderer.Transformation = matrix;
        }

        private ParticleEffect particleeffect;

        public void Load(string path)
        {
            try
            {
#if PARTICLE_NOCONTENT
                if (assemblyMercuryParticleSerializer != null)
                {
                    Type type = assemblyMercuryParticleSerializer.GetType("MercuryParticleSerializer.DefaultSerializer");
                    MethodInfo dsGetMethod = type.GetMethod("Deserialize");
                    object obj = assemblyMercuryParticleSerializer.CreateInstance(type.FullName, true);
                    if (dsGetMethod != null && obj != null)
                    {
                        particleeffect = dsGetMethod.Invoke(obj, new string[] { @"particles/" + path +".xml" }) as ParticleEffect;
                        for (int i = 0; i < particleeffect.Emitters.Count; i++)
                        {
                            particleeffect.Emitters[i].ParticleTexture = GameConst.Content.Load<Texture2D>(@"particles/" + particleeffect.Emitters[i].ParticleTextureAssetPath);
                            particleeffect.Emitters[i].Initialise();
                        }
                    }
                }
#else
                particleeffect = GameConst.Content.Load<ParticleEffect>(@"particles/" + path);
                for (int i = 0; i < particleeffect.Emitters.Count; i++)
                {
                    particleeffect.Emitters[i].ParticleTexture = GameConst.Content.Load<Texture2D>(@"particles/" + particleeffect.Emitters[i].ParticleTextureAssetPath);
                    particleeffect.Emitters[i].Initialise();
                }
#endif
            }
            catch
            {

            }
        }

        public void Trigger(ref Vector3 p)
        {
            particleeffect.Trigger(ref p);
        }

        public override void Update(GameTime gametime)
        {
            particleeffect.Update((float)gametime.ElapsedGameTime.TotalSeconds);
            base.Update(gametime);
        }

        public override void Render(SpriteBatch sb)
        {
            if (state == RenderChunkState.Hide || state == RenderChunkState.Invisible)
                return;
            sb.End();
            spritebatchrenderer.RenderEffect(particleeffect, ref idmatrix, ref idmatrix, ref idmatrix, ref campos);
            sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            base.Render(sb);
        }
    }
}
