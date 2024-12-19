using System.Numerics;
using UnityEngine;

using TMPro;



public class SignalEmitter2DFilter : MonoBehaviour
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
        Texture2D filteredTexture = new Texture2D(width, height);
        Texture2D targetTexture = new Texture2D(width, height);
        Texture2D filterInverseTexture = new Texture2D(width, height);

        fourierTexture.filterMode = FilterMode.Point;
        filteredTexture.filterMode = FilterMode.Point;
        targetTexture.filterMode = FilterMode.Point;
        filterInverseTexture.filterMode = FilterMode.Point;
        
        Color[] fourierColors = fourierTexture.GetPixels();
        Color[] filteredColors = filteredTexture.GetPixels();
        Color[] targetColors = targetTexture.GetPixels();
        Color[] filterInverseColors = filterInverseTexture.GetPixels();

        Color[] filterColors = filter.GetPixels();

        for (int i = 0; i < 3; i++)
        {
            Complex[][] complexTexture = TextureUtils.TextureToComplex(texture, i);

            Complex[][] fourierSource = FFT2D.Forward(complexTexture);

            TextureUtils.FillTextureChannelFromComplexSpectrum(fourierSource, width, height, i, ref fourierColors, _spectrumRemapCurve);

            Complex[][] filteredFourier = FFTUtils.MultiplyByColorChannel(fourierSource, filterColors, width, height, i);

            TextureUtils.FillTextureChannelFromComplexSpectrum(filteredFourier, width, height, i, ref filteredColors, _spectrumRemapCurve);

            Complex[][] inverseFourier = FFT2D.Inverse(filteredFourier);

            TextureUtils.FillTextureChannelFromComplex(inverseFourier, width, height, i, ref targetColors);

            Complex[][] filterComplex = TextureUtils.TextureToComplex(filter, i);
            filterComplex = FFTUtils.OffsetHalf(filterComplex, width, height);
            Complex[][] filterInverse = FFT2D.Forward(filterComplex);

            TextureUtils.FillTextureChannelFromComplexSpectrum(filterInverse, width, height, i, ref filterInverseColors, _spectrumRemapCurve);
        }
        
        fourierTexture.SetPixels(fourierColors);
        fourierTexture.Apply();
        filteredTexture.SetPixels(filteredColors);
        filteredTexture.Apply();
        targetTexture.SetPixels(targetColors);
        targetTexture.Apply();
        filterInverseTexture.SetPixels(filterInverseColors);
        filterInverseTexture.Apply();

        MR_Fourier.material.mainTexture = fourierTexture;
        MR_Filtered.material.mainTexture = filteredTexture;
        MR_Target.material.mainTexture = targetTexture;
        MR_FilterInverse.material.mainTexture = filterInverseTexture;
    }


}
