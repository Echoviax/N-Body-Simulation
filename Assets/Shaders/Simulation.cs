using UnityEngine;
using UnityEngine.UIElements;

public struct Particle
{
    public Vector4 positionMass; // more efficient due to GPUs reading in 16 byte blocks
    public Vector3 velocity;
    public Vector4 color;
}

public class Simulation : MonoBehaviour
{
    public int particleCount = 1000;
    public float gravityStrength = 100.0f;

    public float maxColorSpeed = 5.0f;

    public ComputeShader computeShader;
    public Material particleMaterial;

    private int particleSize = sizeof(float) * 11;
    private ComputeBuffer particleBuffer;
    private int kernelHandle;

    void Start()
    {
        kernelHandle = computeShader.FindKernel("CSMain");

        particleBuffer = new ComputeBuffer(particleCount, particleSize);

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
        particleMaterial.SetBuffer("_ParticleBuffer", particleBuffer);
    }

    void Update()
    {
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetFloat("_GravityStrength", gravityStrength);
        computeShader.SetFloat("_MaxColorSpeed", maxColorSpeed);
        computeShader.SetFloat("_ParticleCount", particleCount);

        int threadGroups = (particleCount + 63) / 64;
        computeShader.Dispatch(kernelHandle, threadGroups, 1, 1);
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