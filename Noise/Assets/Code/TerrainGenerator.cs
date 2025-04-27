using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public ComputeShader computeShader;
    // Coord
    public int width = 1024;
    public int height = 1024;
    [Range(0.01f, 100.0f)]
    public float noiseScale = 10.0f;

    // fBm
    [Range(1, 15)]
    public int octaveCount = 6;
    public float lacunarity = 2.0f;
    public float gain = 1.0f;

    // Normal
    private Material meshMaterial;
    [Range(0.0f, 100.0f)]
    public float displacement = 10.0f;
    public float aoIntensity = 1.0f;
    public float normalIntensity = 1.0f;
    
    // Swapping
    public bool useQuinticInterpolation = true;

    public enum NoiseType
    {
        value,
        gradient,
        voronoi
    };
    public NoiseType noiseType = NoiseType.value;

    public enum HashType
    {
        hashInt,
        hashFloat
    };
    public HashType hashType = HashType.hashInt;

    private RenderTexture noise_texture;
    private RenderTexture normal_texture;
    private RenderTexture ao_texture;

    private uint threadSizeX;
    private uint threadSizeY;
    private uint threadSizeZ;

    void Start()
    {
        noise_texture = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        noise_texture.enableRandomWrite = true;
        noise_texture.Create();
        normal_texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        normal_texture.enableRandomWrite = true;
        normal_texture.Create();
        ao_texture = new RenderTexture(width, height, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        ao_texture.enableRandomWrite = true;
        ao_texture.Create();

        computeShader.GetKernelThreadGroupSizes(0, out threadSizeX, out threadSizeY, out threadSizeZ);
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetTexture("_Noise", noise_texture);
        meshRenderer.material.SetTexture("_Normal", normal_texture);
        meshRenderer.material.SetTexture("_AO", ao_texture);
        meshMaterial = meshRenderer.material;
    }

    void Update()
    {
        meshMaterial.SetFloat("_Displacement", displacement);
        computeShader.SetTexture(0, "Noise", noise_texture);
        computeShader.SetInt("NoiseWidth", width);
        computeShader.SetInt("NoiseHeight", height);
        computeShader.SetFloat("NoiseScale", noiseScale);

        computeShader.SetInt("OctaveCount", octaveCount);
        computeShader.SetFloat("Lacunarity", lacunarity);
        computeShader.SetFloat("Gain", gain);

        computeShader.SetBool("NoiseQuinticInterpolation", useQuinticInterpolation);
        computeShader.SetInt("NoiseType", (int) noiseType);
        computeShader.SetInt("HashType", (int)hashType);

        // threadSizeZ is not implements as it's not needed right now
        Dispatch(0);
        
        computeShader.SetTexture(1, "Normal", normal_texture);
        computeShader.SetTexture(1, "NoiseRead", noise_texture);
        computeShader.SetFloat("NormalIntensity", normalIntensity);
        Dispatch(1);

        computeShader.SetTexture(2, "AO", ao_texture);
        computeShader.SetTexture(2, "NoiseRead", noise_texture);
        computeShader.SetFloat("AOIntensity", aoIntensity);
        Dispatch(2);
    }

    void Dispatch(int channel)
    {   
        computeShader.Dispatch(channel, (int)(width / threadSizeX), (int)(height / threadSizeY), 1);
    }
}