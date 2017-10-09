﻿using System;
using KelpNet.Common;
using KelpNet.Common.Functions;
using KelpNet.Common.Tools;

namespace KelpNet.Functions.Connections
{
    [Serializable]
    public class EmbedID : NeedPreviousInputFunction
    {
        public NdArray Weight;

        public EmbedID(int inputCount, int outputCount, Real[,] initialW = null, string name = "EmbedID") : base(name, inputCount, outputCount)
        {
            this.Weight = new NdArray(inputCount, outputCount);

            if (initialW == null)
            {
                Initializer.InitWeight(this.Weight);
            }
            else
            {
                //単純に代入しないのはサイズのチェックを兼ねるため
                this.Weight.Data = Real.GetArray(initialW);
            }

            this.Parameters = new[] { this.Weight };

            NeedPreviousForward = NeedPreviousForwardCpu;
            NeedPreviousBackward = NeedPreviousBackwardCpu;
        }

        protected NdArray NeedPreviousForwardCpu(NdArray x)
        {
            Real[] result = new Real[x.Data.Length * this.OutputCount];

            for (int b = 0; b < x.BatchCount; b++)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    for (int j = 0; j < this.OutputCount; j++)
                    {
                        result[i * this.OutputCount + j + b * x.Length * this.OutputCount] = this.Weight.Data[(int)x.Data[i + b * x.Length] * this.OutputCount + j];
                    }
                }
            }

            return NdArray.Convert(result, new[] { x.Length, this.OutputCount }, x.BatchCount);
        }

        protected NdArray NeedPreviousBackwardCpu(NdArray gy, NdArray prevInput)
        {
            for (int b = 0; b < gy.BatchCount; b++)
            {
                for (int i = 0; i < prevInput.Length; i++)
                {
                    for (int j = 0; j < this.OutputCount; j++)
                    {
                        this.Weight.Grad[(int)prevInput.Data[i + b * prevInput.Length] * this.OutputCount + j] += gy.Data[i + j + b * gy.Length];
                    }
                }
            }

            return new NdArray();
        }
    }
}
