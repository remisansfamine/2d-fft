using System.Numerics;

using UnityEngine;



static public class FFTUtils
{
	static public Complex[][] CompressByPercentage(Complex[][] data, int srcWidth, int srcHeight, out int dstWidth, out int dstHeight, float percent)
	{
		Complex[][] offset_data = OffsetHalf(data, srcWidth, srcHeight);

		float percent_dec = 1 - percent;

		int halfCompressedWidth = Mathf.FloorToInt(srcWidth * percent_dec * 0.5f);
		int halfCompressedHeight =  Mathf.FloorToInt(srcHeight * percent_dec * 0.5f);

		int isWidthEven = srcWidth % 2;
		int isHeightEven = srcHeight % 2;

		int middleX = srcWidth / 2;
		int middleY = srcHeight / 2;

		dstWidth = halfCompressedWidth * 2 + isWidthEven;
		dstHeight = halfCompressedHeight * 2 + isHeightEven;
		
		Complex[][] result = new Complex[dstWidth][];

		for (int i = 0; i < dstWidth; i++)
		{
			result[i] = new Complex[dstHeight];

			for (int j = 0; j < dstHeight; j++)
			{
				result[i][j] = offset_data[middleX - halfCompressedWidth + i][middleY - halfCompressedHeight + j];
			}
		}
		
		return result;
	}
	
	static public Complex[][] Extract(Complex[][] data, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
	{
		int middleX = dstWidth / 2;
		int middleY = dstHeight / 2;
		
		int halfCompressedWidth = srcWidth / 2;
		int halfCompressedHeight = srcHeight / 2;
		
		(int minX, int maxX) = (middleX - halfCompressedWidth, middleX + halfCompressedWidth - 1);
		(int minY, int maxY) = (middleY - halfCompressedHeight, middleY + halfCompressedHeight - 1);
		
		Complex[][] offset_result = new Complex[dstWidth][];
		
		for (int i = 0; i < dstWidth; i++)
		{
			offset_result[i] = new Complex[dstHeight];

			for (int j = 0; j < dstHeight; j++)
			{
				if (__IsIn(i, minX, maxX) && __IsIn(j, minY, maxY))
					offset_result[i][j] = data[i - minX][j - minY];
				else
					offset_result[i][j] = new Complex(0, 0);
			}
		}
		
		Complex[][] result = OffsetHalf(offset_result, dstWidth, dstHeight);
		return result;

		bool __IsIn(float value, float min, float max)
		{
			return value >= min && value <= max;
		}
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
	
	static public Complex[][] MultiplyByColorChannel(Complex[][] input, Color[] colors, int width, int height, int channel)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				input[i][j] *= colors[( ( i + width / 2 ) % width ) * height + ( ( j + height / 2 ) % height )][channel];
			}
		}

		return input;
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

	static public float[][] ComplexToFloatLogged(Complex[][] input, int width, int height)
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

		return ( min, max );
	}
	
	static public float Remap(float value, float srcMin, float srcMax, float dstMin, float dstMax)
	{
		return ( ( value - srcMin ) / ( srcMax - srcMin ) ) * ( dstMax - dstMin ) + dstMin;
	}
}
