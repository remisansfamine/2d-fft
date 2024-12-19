using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using TMPro;

using UnityEditor;



public class SignalEmitter2DFilter : MonoBehaviour
{
    private RenderTexture _drawableTexture;
    [SerializeField] private Texture2D _texture;

    [Header("Parameters")]
    [SerializeField] private Texture2D[] _textures;
    [SerializeField] private Texture2D[] _filters;
    [SerializeField] private AnimationCurve _spectrumRemapCurve;
    [SerializeField] private Texture2D _currentBrush;
    [SerializeField] private UnityEngine.Vector2 _currentBrushSize = new UnityEngine.Vector2(10f, 10f);

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
        Graphics.Blit(_filters[_currentFilter], _drawableTexture);
        _textFilters.text = _filters[_currentFilter].name;
    }

    private void InitializeFilterTexture()
    {
        Texture2D texture = _textures[_currentTexture];

        int width = texture.width;
        int height = texture.height;

        _drawableTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        _drawableTexture.Create();

        MR_Filter.material.mainTexture = _drawableTexture;
    }

    private void Start()
    {
        InitializeFilterTexture();
        ScrollTexture(0);
        ScrollFilters(0);
    }

    private void OnDestroy()
    {
        _drawableTexture?.Release();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && Input.GetMouseButtonDown(0))
        {
            if (hit.transform.gameObject == MR_Filter.gameObject)
            {
                UnityEngine.Vector2 uv = hit.textureCoord;

                int x = (int)(uv.x * _drawableTexture.width);
                int y = (int)(uv.y * _drawableTexture.height);

                Graphics.Blit(_currentBrush, _drawableTexture, _currentBrushSize, new UnityEngine.Vector2(x, y));
            }
        }
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

        Color[] filterColors = filter.GetPixels();

        for (int i = 0; i < 3; i++)
        {
            Complex[][] complexTexture = TextureUtils.TextureToComplex(texture, i);

            Complex[][] fourierSource = FFT2D.Forward(complexTexture);

            TextureUtils.FillTextureChannelFromComplexSpectrum(fourierSource, width, height, i, ref fourierTexture, _spectrumRemapCurve);

            Complex[][] filteredFourier = FFTUtils.MultiplyByColorChannel(fourierSource, filterColors, width, height, i);

            TextureUtils.FillTextureChannelFromComplexSpectrum(filteredFourier, width, height, i, ref filteredTexture, _spectrumRemapCurve);

            Complex[][] inverseFourier = FFT2D.Inverse(filteredFourier);

            TextureUtils.FillTextureChannelFromComplex(inverseFourier, width, height, i, ref targetTexture);

            Complex[][] filterComplex = TextureUtils.TextureToComplex(filter, i);
            filterComplex = FFTUtils.OffsetHalf(filterComplex, width, height);
            Complex[][] filterInverse = FFT2D.Forward(filterComplex);

            TextureUtils.FillTextureChannelFromComplexSpectrum(filterInverse, width, height, i, ref filterInverseTexture, _spectrumRemapCurve);
            //TextureUtils.FillTextureChannelFromComplex(filterInverse, width, height, i, ref filterInverseTexture);
        }

        MR_Fourier.material.mainTexture = fourierTexture;
        MR_FilterInverse.material.mainTexture = filterInverseTexture;
        MR_Filtered.material.mainTexture = filteredTexture;
        MR_Target.material.mainTexture = targetTexture;
    }


}
