using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public ComputeShader computeShader;
    // Coord
    public int width = 1024;
    public int height = 1024;
    [Range(0.01f, 100.0f)]
    public float noiseScale = 10.0f;


    [Range(1, 15)]
    public int octaveCount = 6;
    public float lacunarity = 2.0f;
    public float gain = 1.0f;
    
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

    private RenderTexture noise_textute;
    private uint threadSizeX;
    private uint threadSizeY;
    private uint threadSizeZ;

    void Start()
    {
        noise_textute = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        noise_textute.enableRandomWrite = true;
        noise_textute.Create();

        computeShader.GetKernelThreadGroupSizes(0, out threadSizeX, out threadSizeY, out threadSizeZ);
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.material.SetTexture("_Noise", noise_textute);
    }

    void Update()
    {
        computeShader.SetTexture(0, "Noise", noise_textute);
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
        computeShader.Dispatch(0, (int)(width / threadSizeX), (int)(height / threadSizeY), 1);
    }
}