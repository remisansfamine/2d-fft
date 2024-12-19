using System.Numerics;
using UnityEngine;

using TMPro;

using UnityEngine.UI;

using Vector3 = UnityEngine.Vector3;



public class SignalEmitter2DCompress : MonoBehaviour
{
    [SerializeField] private Texture2D _texture;

    [Header("Parameters")]
    [SerializeField] private Texture2D[] _textures;
    [SerializeField] private AnimationCurve _spectrumRemapCurve;

    [Header("UI")]
    [SerializeField] private MeshRenderer MR_Source;
    [SerializeField] private MeshRenderer MR_Fourier;
    [SerializeField] private MeshRenderer MR_Compressed;
    [SerializeField] private MeshRenderer MR_Target;
    [SerializeField] private TMP_Text _textTextures;
    [SerializeField] private TMP_Text _percentText;

    [SerializeField] private Slider _scrollbar;

    private float _compressionPercent;
    
    private int _currentTexture = 0;


    public void ScrollTexture(int add)
    {
        _currentTexture = (_currentTexture + add + _textures.Length) % _textures.Length;
        MR_Source.material.mainTexture = _textures[_currentTexture];
        _textTextures.text = _textures[_currentTexture].name;
    }

    public void SetCompressionPercent()
    {
        SetCompressionPercent(_scrollbar.value);
    }

    private void SetCompressionPercent(float percent)
    {
        _compressionPercent = percent * 0.01f;
        _percentText.text = $"{percent}%";
    }

    private void Start()
    {
        ScrollTexture(0);
        SetCompressionPercent(50f);
    }

    public void Process()
    {
        Texture2D texture = _textures[_currentTexture];

        int width = texture.width;
        int height = texture.height;
        
        float percent_dec = 1 - _compressionPercent;
        
        float scale = 10 * percent_dec;
        MR_Compressed.transform.localScale = new Vector3(scale, scale, scale);

        int compressedWidth = Mathf.FloorToInt(width * percent_dec * 0.5f) * 2 + width % 2;
        int compressedHeight =  Mathf.FloorToInt(height * percent_dec * 0.5f) * 2 + height % 2;
        
        Texture2D fourierTexture = new Texture2D(width, height);
        Texture2D compressedTexture = new Texture2D(compressedWidth, compressedHeight);
        Texture2D targetTexture = new Texture2D(width, height);
        
        Color[] fourierColors = fourierTexture.GetPixels();
        Color[] compressedColors = compressedTexture.GetPixels();
        Color[] targetColors = targetTexture.GetPixels();
        
        fourierTexture.filterMode = FilterMode.Point;
        compressedTexture.filterMode = FilterMode.Point;
        targetTexture.filterMode = FilterMode.Point;
        
        for (int i = 0; i < 3; i++)
        {
            Complex[][] complexTexture = TextureUtils.TextureToComplex(texture, i);

            Complex[][] fourierSource = FFT2D.Forward(complexTexture);

            TextureUtils.FillTextureChannelFromComplexSpectrum(fourierSource, width, height, i, ref fourierColors, _spectrumRemapCurve);

            Complex[][] compressedFourier = FFTUtils.CompressByPercentage(fourierSource, width, height, out int compWidth, out int compHeight, _compressionPercent);

            TextureUtils.FillTextureChannelFromComplexSpectrum(compressedFourier, compWidth, compHeight, i, ref compressedColors, _spectrumRemapCurve, false);

            Complex[][] extractedFourier = FFTUtils.Extract(compressedFourier, compWidth, compHeight, width, height);
            Complex[][] inverseFourier = FFT2D.Inverse(extractedFourier);

            TextureUtils.FillTextureChannelFromComplex(inverseFourier, width, height, i, ref targetColors);
        }
        
        fourierTexture.SetPixels(fourierColors);
        fourierTexture.Apply();
        compressedTexture.SetPixels(compressedColors);
        compressedTexture.Apply();
        targetTexture.SetPixels(targetColors);
        targetTexture.Apply();
        
        MR_Fourier.material.mainTexture = fourierTexture;
        MR_Compressed.material.mainTexture = compressedTexture;
        MR_Target.material.mainTexture = targetTexture;
    }
}
