using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

public class SignalEmitter : MonoBehaviour
{
    private void Start()
    {
        FourierTest();
    }

    public void FourierTest()
    {
        Complex[] signal = new[]
        {
            new Complex(0,0),
            new Complex(1,0),
            new Complex(2,0),
            new Complex(3,0),
            new Complex(4,0),
            new Complex(5,0),
            new Complex(6,0),
            new Complex(7,0)
        };

        Complex[] result = FFT.Forward(signal);

        Complex[] expected = new[]
        {
            new Complex(28, 0),
            new Complex(-4.0f, 9.6569f),
            new Complex(-4.0f, 4.0f),
            new Complex(-4.0f, 1.6569f),
            new Complex(-4, 0.0f),
            new Complex(-4, -1.6569f),
            new Complex(-4, -4.0000f),
            new Complex(-4, -9.6569f)
        };

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(System.Math.Round(expected[i].Real, 4), System.Math.Round(result[i].Real, 4));
            Assert.AreEqual(System.Math.Round(expected[i].Imaginary, 4), System.Math.Round(result[i].Imaginary, 4));
        }

        result = FFT.Inverse(result);

        for (int i = 0; i < result.Length; i++)
        {
            Assert.AreEqual(System.Math.Round(signal[i].Real, 4), System.Math.Round(result[i].Real, 4));
            Assert.AreEqual(System.Math.Round(signal[i].Imaginary, 4), System.Math.Round(result[i].Imaginary, 4));
        }
    }
}
