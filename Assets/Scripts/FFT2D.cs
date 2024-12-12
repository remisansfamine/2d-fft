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

    static public float GetMax(float[][] input, int width, int height)
    {
        float max = 0.0f;
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

    static public void FloatToTexture(float[][] input, int width, int height, int channel, ref Texture2D texture)
    {
        float imax = 1f / GetMax(input, width, height);

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
        Complex[][] fourier = new Complex[width][];
        Complex[][] transpose = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            transform[i] = FFT.Forward(input[i]);
        }

        for (int i = 0; i < width; i++)
        {
            transpose[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                transpose[i][j] = transform[j][i];
            }
            fourier[i] = FFT.Forward(transpose[i]);
        }

        return fourier;
    }

    static public Complex[][] Inverse(Complex[][] input)
    {
        int width = input.GetLength(0);
        int height = input.First().GetLength(0);

        Complex[][] transform = new Complex[width][];
        Complex[][] fourier = new Complex[width][];
        Complex[][] transpose = new Complex[width][];

        for (int i = 0; i < width; i++)
        {
            transform[i] = FFT.Inverse(input[i]);
        }

        for (int i = 0; i < width; i++)
        {
            transpose[i] = new Complex[height];
            for (int j = 0; j < height; j++)
            {
                transpose[i][j] = transform[j][i] / (float)(width * height);
            }
            fourier[i] = FFT.Inverse(transpose[i]);
        }

        return fourier;
    }
}
