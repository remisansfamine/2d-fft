using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class FFT
{
    static public Complex[] Forward(Complex[] input)
    {
        int n = input.Length;

        if (n == 1)
            return input;

        int halfN = n / 2;

        Complex omegaN = Complex.Exp(-2f * Mathf.PI * Complex.ImaginaryOne / n);

        Complex[] evenInput = new Complex[halfN];
        Complex[] oddInput = new Complex[halfN];

        for (int i = 0; i < halfN; i++)
        {
            evenInput[i] = input[i * 2];
            oddInput[i] = input[i * 2 + 1];
        }

        Complex[] even = Forward(evenInput);
        Complex[] odd = Forward(oddInput);


        Complex[] result = new Complex[n];
        Complex omega = 1;

        for (int k = 0; k < halfN; k++)
        {
            result[k] = even[k] + omega * odd[k];
            result[k + halfN] = even[k] - omega * odd[k];
            omega *= omegaN;
        }

        return result;
    }

    static public Complex[] Inverse(Complex[] input)
    {
        int n = input.Length;

        for (int i = 0; i < n; i++)
        {
            input[i] = Complex.Conjugate(input[i]);
        }

        Complex[] transform = Forward(input);

        float inverseN = 1f / n;

        for (int i = 0; i < n; i++)
        {
            transform[i] = Complex.Conjugate(transform[i]) * inverseN;
        }

        return transform;
    }
}
