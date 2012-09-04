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
using System.Collections;
//


namespace demo.animation
{
    public delegate void UpdateValue<T>(ref T value);
    public delegate void Interpolation2Value<T>(ref T start, ref T end, float factor, out T result);
    public delegate T Interpolation2ValueNoRef<T>(T start, T end, float factor);

    public class InterpolatioAnimationMgr
    {
       static public void Update(GameTime gametime)
       {
           Animation<float>.UpdateAnimation2ValueList(gametime);
           Animation<Vector2>.UpdateAnimation2ValueList(gametime);
           Animation<Vector3>.UpdateAnimation2ValueList(gametime);
           Animation<Vector4>.UpdateAnimation2ValueList(gametime);
       }
    }

    public class Animation<T>
    {
        public Animation()
        {
        
        }

        private class InterpolationBlock
        {

        }

        private class InterpolationBlock2Value : InterpolationBlock
        {
            public double Duration { get; set; }
            public T StartValue; //属性不能作为ref和out使用
            public T EndValue;
            public double InterTime { get; set; }
            public T CurrentValue;
            public UpdateValue<T> UpdateValueMethod { get; set; }
            public Interpolation2Value<T> Interpolation2ValueMethod { get; set; }
            public Interpolation2ValueNoRef<T> Interpolation2ValueNoRefMethod { get; set; }
        }


        private static List<InterpolationBlock2Value> listBlock2Value = new List<InterpolationBlock2Value>();

        public static void CreateAnimation2Value(T start, T end, double dur, Interpolation2Value<T> intermethod, UpdateValue<T> updatevaluemethod)
        {
            try
            {
                InterpolationBlock2Value ib = new InterpolationBlock2Value();
                ib.Duration = dur;
                ib.InterTime = 0.0;
                ib.StartValue = start;
                ib.EndValue = end;
                ib.UpdateValueMethod = updatevaluemethod;
                ib.Interpolation2ValueMethod = intermethod;
                listBlock2Value.Add(ib);
            }
            catch (Exception e)
            {
                Log.WriteLine(e.Message);
            }
        }

        public static void CreateAnimation2ValueNoRef(T start, T end, double dur, Interpolation2ValueNoRef<T> intermethod, UpdateValue<T> updatevaluemethod)
        {
            try
            {
                InterpolationBlock2Value ib = new InterpolationBlock2Value();
                ib.Duration = dur;
                ib.InterTime = 0.0;
                ib.StartValue = start;
                ib.EndValue = end;
                ib.UpdateValueMethod = updatevaluemethod;
                ib.Interpolation2ValueNoRefMethod = intermethod;
                listBlock2Value.Add(ib);
            }
            catch (Exception e)
            {
                Log.WriteLine(e.Message);
            }
        }

        public static void UpdateAnimation2ValueList(GameTime gametime)
        {
            for (int i = 0; i < listBlock2Value.Count; ++i)
            {
                InterpolationBlock2Value ib = listBlock2Value[i];
                ib.InterTime += gametime.ElapsedGameTime.TotalSeconds;
                if (ib.InterTime >= ib.Duration)
                {
                    ib.UpdateValueMethod(ref ib.EndValue);
                    listBlock2Value.Remove(ib);
                }
                else
                {
                    if (ib.Interpolation2ValueMethod != null)
                    {
                        ib.Interpolation2ValueMethod(ref ib.StartValue, ref ib.EndValue, (float)(ib.InterTime / ib.Duration), out ib.CurrentValue);
                    }
                    else
                    {
                        ib.CurrentValue = ib.Interpolation2ValueNoRefMethod(ib.StartValue, ib.EndValue, (float)(ib.InterTime / ib.Duration));
                    }
                    ib.UpdateValueMethod(ref ib.CurrentValue);
                }
            }
        }
    }
}
