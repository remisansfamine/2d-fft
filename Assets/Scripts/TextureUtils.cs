using System.Numerics;

using UnityEngine;



static public class TextureUtils
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
				result[i][j] = new Complex(texture.GetPixel(i, j)[index], 0d);
			}
		}

		return result;
	}

	
	static private void FloatToTexture(float[][] input, int width, int height, int channel, ref Texture2D texture)
	{
		( float _, float max ) = FFTUtils.GetMinMax(input, width, height);
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

	static private void FloatToTextureRemapped(float[][] input, int width, int height, int channel,
													   ref Texture2D texture, AnimationCurve curve)
	{
		( float min, float max ) = FFTUtils.GetMinMax(input, width, height);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Color pixel = texture.GetPixel(i, j);

				pixel[channel] = curve.Evaluate(FFTUtils.Remap(input[i][j], min, max, 0f, 1f));

				texture.SetPixel(i, j, pixel);
			}
		}

		texture.Apply();
	}

	static public void FillTextureChannelFromComplex(Complex[][] input, int width, int height, int channel, ref Texture2D texture)
	{
		float[][] f = FFTUtils.ComplexToFloat(input, width, height);
		FloatToTexture(f, width, height, channel, ref texture);
	}

	static public void FillTextureChannelFromComplexSpectrum(Complex[][] input, int width, int height, int channel,
															 ref Texture2D texture, AnimationCurve curve, bool offset = true)
	{
		float[][] f = FFTUtils.ComplexToFloatLogged(offset ? FFTUtils.OffsetHalf(input, width, height) : input, width, height);

		FloatToTextureRemapped(f, width, height, channel, ref texture, curve);
	}
}
