using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Windows;

public class SignalEmitter2D : MonoBehaviour
{
    [SerializeField] private Texture2D _texture;

    [SerializeField] private MeshRenderer MR_Source;
    [SerializeField] private MeshRenderer MR_Fourier;
    [SerializeField] private MeshRenderer MR_Filtered;
    [SerializeField] private MeshRenderer MR_Target;

    private void Start()
    {
        Test();
    }

    public void Test()
    {
        int width = _texture.width;
        int height = _texture.height;

        Texture2D fourierTexture = new Texture2D(width, height);
        Texture2D filteredTexture = new Texture2D(width, height);
        Texture2D targetTexture = new Texture2D(width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                fourierTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
                filteredTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
                targetTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
            }
        }

        fourierTexture.Apply();
        filteredTexture.Apply();
        targetTexture.Apply();

        for (int i = 0; i < 3; i++)
        {
            Complex[][] complexTexture = FFT2D.TextureToComplex(_texture, i);

            Complex[][] fourierSource = FFT2D.Forward(complexTexture);

            FFT2D.FillTextureChannelFromComplex(fourierSource, width, height, i, ref fourierTexture);

            Complex[][] inverseFourier = FFT2D.Inverse(fourierSource);

            FFT2D.FillTextureChannelFromComplex(inverseFourier, width, height, i, ref targetTexture);
        }

        MR_Fourier.material.mainTexture = fourierTexture;
        MR_Target.material.mainTexture = targetTexture;
    }


}
