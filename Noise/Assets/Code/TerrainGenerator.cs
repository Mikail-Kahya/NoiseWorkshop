using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public ComputeShader computeShader;
    public int width = 1024;
    public int height = 1024;

    public float noiseScale = 10.0f;
    public bool useQuinticInterpolation = true;

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
        computeShader.SetBool("NoiseQuinticInterpolation", useQuinticInterpolation);

        // threadSizeZ is not implements as it's not needed right now
        computeShader.Dispatch(0, (int)(width / threadSizeX), (int)(height / threadSizeY), 1);
    }
}