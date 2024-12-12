using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class FFT2D
{
    static public Complex[][] MultiplyByColorChannel(Complex[][] input, Color[] colors, int width, int height, int channel)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                input[i][j] *= colors[((i + width / 2) % width) * height + ((j + height / 2) % height)][channel];
            }
        }

        return input;
    }


    static public Complex[][] TextureToComplex(Texture2D texture, int index)
    {
        int width = texture.width;
        int height = texture.height;

        Complex[][] result = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            result[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                result[i][j] = new Complex((double)texture.GetPixel(i, j)[index], 0d);
            }
        }

        return result;
    }

    static float Remap(float value, float srcMin, float srcMax, float dstMin, float dstMax)
    {
        return ((value - srcMin) / (srcMax - srcMin)) * (dstMax - dstMin) + dstMin;
    }

    static public float GetMax(float[][] input, int width, int height)
    {
        float max = -float.MaxValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float spectrum = input[i][j];
                if (max < spectrum)
                    max = spectrum;
            }
        }
        return max;
    }

    static public float GetMin(float[][] input, int width, int height)
    {
        float min = float.MaxValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float spectrum = input[i][j];
                if (min > spectrum)
                    min = spectrum;
            }
        }
        return min;
    }

    static public (float, float) GetMinMax(float[][] input, int width, int height)
    {
        float min = float.MaxValue;
        float max = -float.MaxValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float spectrum = input[i][j];
                if (min > spectrum)
                    min = spectrum;
                if (max < spectrum)
                    max = spectrum;
            }
        }
        return (min, max);
    }

    static public (float, float) GetMinMax(Complex[][] input, int width, int height)
    {
        float min = float.MaxValue;
        float max = -float.MaxValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float spectrum = Mathf.Abs((float)input[i][j].Real);
                if (min > spectrum)
                    min = spectrum;
                if (max < spectrum)
                    max = spectrum;
            }
        }
        return (min, max);
    }


    static public float[][] ComplexToFloat(Complex[][] input, int width, int height)
    {
        float[][] result = new float[width][];

        for (int i = 0; i < width; i++)
        {
            result[i] = new float[height];
            for (int j = 0; j < height; j++)
            {
                result[i][j] = Mathf.Abs((float)input[i][j].Real);
            }
        }

        return result;
    }

    static public float[][] ComplexToFloatLog(Complex[][] input, int width, int height)
    {
        float[][] result = new float[width][];

        for (int i = 0; i < width; i++)
        {
            result[i] = new float[height];
            for (int j = 0; j < height; j++)
            {
                result[i][j] = Mathf.Log10(Mathf.Abs((float)input[i][j].Real) + 1);
            }
        }

        return result;
    }

    static public void FloatToTexture(float[][] input, int width, int height, int channel, ref Texture2D texture)
    {
        float max = GetMax(input, width, height);
        float imax = 1f / max;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = texture.GetPixel(i, j);

                pixel[channel] = input[i][j] * imax;

                texture.SetPixel(i, j, pixel);
            }
        }

        texture.Apply();
    }

    static public void FloatToTextureCenteredRemapped(float[][] input, int width, int height, int channel, ref Texture2D texture, AnimationCurve curve)
    {
        (float min, float max) = GetMinMax(input, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = texture.GetPixel(i, j);

                pixel[channel] = curve.Evaluate(Remap(input[(i + width / 2) % width][(j + height / 2) % height], min, max, 0f, 1f));

                texture.SetPixel(i, j, pixel);
            }
        }

        texture.Apply();
    }

    static public Complex[][] OffsetHalf(Complex[][] input, int width, int height)
    {
        Complex[][] result = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            result[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                result[i][j]  = input[(i + width / 2) % width][(j + height / 2) % height];
            }
        }

        return result;
    }

    static public void ComplexToTexture(Complex[][] input, int width, int height, int channel, ref Texture2D texture)
    {
        (float min, float max) = GetMinMax(input, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Color pixel = texture.GetPixel(i, j);

                pixel[channel] = Remap(Mathf.Abs((float)input[i][j].Real), 0f, max, 0f, 1f);

                texture.SetPixel(i, j, pixel);
            }
        }

        texture.Apply();
    }

    static public void FillTextureChannelFromComplexSpectrum(Complex[][] input, int width, int height, int channel, ref Texture2D texture, AnimationCurve curve)
    {
        float[][] f = ComplexToFloatLog(input, width, height);
        FloatToTextureCenteredRemapped(f, width, height, channel, ref texture, curve);
    }

    static public void FillTextureChannelFromComplex(Complex[][] input, int width, int height, int channel, ref Texture2D texture)
    {
        float[][] f = ComplexToFloat(input, width, height);
        FloatToTexture(f, width, height, channel, ref texture);
    }



    static public Complex[][] Forward(Complex[][] input)
    {
        int width = input.GetLength(0);
        int height = input.First().GetLength(0);

        Complex[][] transform = new Complex[width][];
        Complex[][] fourier = new Complex[height][];
        Complex[][] transpose = new Complex[height][];
        Complex[][] retranspose = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            transform[i] = FFT.Forward(input[i]);
        }

        for (int i = 0; i < height; i++)
        {
            transpose[i] = new Complex[width];
            for (int j = 0; j < width; j++)
            {
                transpose[i][j] = transform[j][i];
            }
            fourier[i] = FFT.Forward(transpose[i]);
        }

        for (int i = 0; i < width; i++)
        {
            retranspose[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                retranspose[i][j] = fourier[j][i];
            }
        }

        return retranspose;
    }

    static public Complex[][] Inverse(Complex[][] input)
    {
        int width = input.GetLength(0);
        int height = input.First().GetLength(0);

        Complex[][] transform = new Complex[width][];
        Complex[][] fourier = new Complex[height][];
        Complex[][] transpose = new Complex[height][];
        Complex[][] retranspose = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            transform[i] = FFT.Inverse(input[i]);
        }

        for (int i = 0; i < height; i++)
        {
            transpose[i] = new Complex[width];
            for (int j = 0; j < width; j++)
            {
                transpose[i][j] = transform[j][i] / (float)(width * height);
            }
            fourier[i] = FFT.Inverse(transpose[i]);
        }

        for (int i = 0; i < width; i++)
        {
            retranspose[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                retranspose[i][j] = fourier[j][i];
            }
        }

        return retranspose;
    }
}
