using UnityEngine;

public struct OctreeNode // Section 2.1, Hsin Hung
{
    Vector4 centerOfMass; // xyz pos, w mass
    Vector3 boundsMin;
    Vector3 boundsMax;
    int firstChildIndex;
    int particleIndex;
};

public struct Particle
{
    public Vector4 positionMass; // more efficient due to GPUs reading in 16 byte blocks
    public Vector3 velocity;
    public Vector4 color;
}

public class Simulation : MonoBehaviour
{
    public int particleCount = 1000;
    public int octreeNodes = 100000;
    public float gravityStrength = 100.0f;

    public float maxColorSpeed = 5.0f;

    public ComputeShader computeShader;
    public Material particleMaterial;

    private int particleSize = sizeof(float) * 11;
    private int octreeNodeSize = sizeof(float) * 10 + sizeof(int) * 2;
    private ComputeBuffer particleBuffer;
    private ComputeBuffer octreeBuffer;
    private int kernelHandle;
    private int kernelResetTree;
    private const float THREAD_GROUP_SIZE = 64;

    void Start()
    {
        kernelHandle = computeShader.FindKernel("CSMain");
        kernelResetTree = computeShader.FindKernel("CSMain_ResetTree");

        particleBuffer = new ComputeBuffer(particleCount, particleSize);
        octreeBuffer = new ComputeBuffer(octreeNodes, octreeNodeSize);

        Particle[] particleArray = new Particle[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            Vector4 positionMass = Random.insideUnitSphere * 10;
            positionMass.w = 1; // mass

            particleArray[i] = new Particle
            {
                positionMass = positionMass,
                velocity = Vector3.zero,
                color = Vector4.zero,
            };
        }

        particleBuffer.SetData(particleArray);
        computeShader.SetBuffer(kernelHandle, "_ParticleBuffer", particleBuffer);
        computeShader.SetBuffer(kernelResetTree, "_OctreeBuffer", octreeBuffer);
        particleMaterial.SetBuffer("_ParticleBuffer", particleBuffer);
    }

    void Update()
    {
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetFloat("_GravityStrength", gravityStrength);
        computeShader.SetFloat("_MaxColorSpeed", maxColorSpeed);
        computeShader.SetFloat("_ParticleCount", particleCount);
        computeShader.SetInt("_OctreeNodeCount", octreeNodes);

        //int threadGroups = Mathf.CeilToInt(octreeNodes / THREAD_GROUP_SIZE);
        //computeShader.Dispatch(kernelResetTree, threadGroups, 1, 1);

        int particleThreadGroups = Mathf.CeilToInt(particleCount / THREAD_GROUP_SIZE);
        computeShader.Dispatch(kernelHandle, particleThreadGroups, 1, 1);
    }

    void OnRenderObject()
    {
        particleMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, particleCount);
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Release();
        }
    }
}