using System.Linq;
using System.Numerics;



static public class FFT2D
{
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
