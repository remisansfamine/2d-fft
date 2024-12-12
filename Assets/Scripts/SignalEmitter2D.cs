using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Windows;
using TMPro;

public class SignalEmitter2D : MonoBehaviour
{
    [SerializeField] private Texture2D _texture;

    [Header("Parameters")]
    [SerializeField] private Texture2D[] _textures;
    [SerializeField] private Texture2D[] _filters;
    [SerializeField] private AnimationCurve _spectrumRemapCurve;

    [Header("UI")]
    [SerializeField] private MeshRenderer MR_Source;
    [SerializeField] private MeshRenderer MR_Fourier;
    [SerializeField] private MeshRenderer MR_Filter;
    [SerializeField] private MeshRenderer MR_FilterInverse;
    [SerializeField] private MeshRenderer MR_Filtered;
    [SerializeField] private MeshRenderer MR_Target;
    [SerializeField] private TMP_Text _textTextures;
    [SerializeField] private TMP_Text _textFilters;


    private int _currentTexture = 0;
    private int _currentFilter = 0;


    public void ScrollTexture(int add)
    {
        _currentTexture = (_currentTexture + add + _textures.Length) % _textures.Length;
        MR_Source.material.mainTexture = _textures[_currentTexture];
        _textTextures.text = _textures[_currentTexture].name;
    }

    public void ScrollFilters(int add)
    {
        _currentFilter = (_currentFilter + add + _filters.Length) % _filters.Length;
        MR_Filter.material.mainTexture = _filters[_currentFilter];
        _textFilters.text = _filters[_currentFilter].name;

    }

    private void Start()
    {
        ScrollTexture(0);
        ScrollFilters(0);
    }

    public void Process()
    {
        Texture2D texture = _textures[_currentTexture];
        Texture2D filter = _filters[_currentFilter];

        int width = texture.width;
        int height = texture.height;

        Texture2D fourierTexture = new Texture2D(width, height);
        Texture2D filterInverseTexture = new Texture2D(width, height);
        Texture2D filteredTexture = new Texture2D(width, height);
        Texture2D targetTexture = new Texture2D(width, height);

        fourierTexture.filterMode = FilterMode.Point;
        filterInverseTexture.filterMode = FilterMode.Point;
        filteredTexture.filterMode = FilterMode.Point;
        targetTexture.filterMode = FilterMode.Point;

        /*
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                fourierTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
                filterInverseTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
                filteredTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
                targetTexture.SetPixel(i, j, new Color(0, 0, 0, 1f));
            }
        }

        fourierTexture.Apply();
        filterInverseTexture.Apply();
        filteredTexture.Apply();
        targetTexture.Apply();
        */

        Color[] filterColors = filter.GetPixels();

        for (int i = 0; i < 3; i++)
        {
            Complex[][] complexTexture = FFT2D.TextureToComplex(texture, i);

            Complex[][] fourierSource = FFT2D.Forward(complexTexture);

            FFT2D.FillTextureChannelFromComplexSpectrum(fourierSource, width, height, i, ref fourierTexture, _spectrumRemapCurve);

            Complex[][] filteredFourier = FFT2D.MultiplyByColorChannel(fourierSource, filterColors, width, height, i);

            FFT2D.FillTextureChannelFromComplexSpectrum(filteredFourier, width, height, i, ref filteredTexture, _spectrumRemapCurve);

            Complex[][] inverseFourier = FFT2D.Inverse(filteredFourier);

            FFT2D.FillTextureChannelFromComplex(inverseFourier, width, height, i, ref targetTexture);

            Complex[][] filterComplex = FFT2D.TextureToComplex(filter, i);
            filterComplex = FFT2D.OffsetHalf(filterComplex, width, height);
            Complex[][] filterInverse = FFT2D.Inverse(filterComplex);

            FFT2D.FillTextureChannelFromComplex(filterInverse, width, height, i, ref filterInverseTexture);
        }

        MR_Fourier.material.mainTexture = fourierTexture;
        MR_FilterInverse.material.mainTexture = filterInverseTexture;
        MR_Filtered.material.mainTexture = filteredTexture;
        MR_Target.material.mainTexture = targetTexture;
    }


}
