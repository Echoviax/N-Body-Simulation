using System.Linq;
using UnityEngine;

public struct Particle
{
    public Vector4 positionMass; // more efficient due to GPUs reading in 16 byte blocks
    public Vector3 velocity;
    public Vector4 color;
}

public class Simulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int particleCount = 1000;
    public float gravityStrength = 100.0f;
    public float maxColorSpeed = 5.0f;
    public float spawnDiameter = 10f;

    [Header("References")]
    public ComputeShader computeShader;
    public Material particleMaterial;

    private int particleSize = (16 * 2) + (12 * 1);
    private ComputeBuffer particleBuffer;
    private int kernelHandle;
    private const float THREAD_GROUP_SIZE = 128; // must match compute

    void Start()
    {
        kernelHandle = computeShader.FindKernel("CSMain");

        particleBuffer = new ComputeBuffer(particleCount, particleSize);

        Particle[] particleArray = StartSpiral();

        //Particle[] particleArray = new Particle[particleCount];

        //for (int i = 0; i < particleCount; i++)
        //{
        //    Vector4 pos = Random.insideUnitSphere * 10f;
        //    pos.w = 1.0f;

        //    particleArray[i] = new Particle
        //    {
        //        positionMass = pos,
        //        velocity = Vector3.zero,
        //        color = Vector4.zero
        //    };
        //}

        particleBuffer.SetData(particleArray);
        computeShader.SetBuffer(kernelHandle, "_ParticleBuffer", particleBuffer);
        particleMaterial.SetBuffer("_ParticleBuffer", particleBuffer);
    }

    Particle[] StartSpiral(Vector2 centerPos, int particleCount, float diameter)
    {
        Particle[] particleArray = new Particle[particleCount];
        Vector4 posMass;

        float sunMass = 332942f;

        for (int i = 0; i < particleCount - 1; i++)
        {
            Vector2 pos = centerPos + Random.insideUnitCircle * diameter;
            float distance = (pos - centerPos).magnitude;

            Vector2 r = new Vector2(pos.x - centerPos.x, pos.y - centerPos.y);
            Vector2 a = new Vector2(r.x / distance, r.y / distance);
            
            float esc = Mathf.Sqrt((gravityStrength * sunMass) / (distance));
            Vector2 velocity = new Vector2(-a.y * esc, a.x * esc);
            //Debug.Log(pos);
            //Debug.Log(velocity);

            posMass = pos;
            posMass.w = 1;
            particleArray[i] = new Particle
            {
                positionMass = posMass,
                velocity = velocity,
                color = Vector4.zero
            };
        }

        posMass = centerPos;
        posMass.w = sunMass;
        particleArray[particleCount - 1] = new Particle
        {
            positionMass = posMass,
            velocity = Vector3.zero,
            color = Vector4.zero
        };

        return particleArray;
    }

    Particle[] StartSpiral()
    {
        return StartSpiral(Vector2.zero, particleCount, spawnDiameter);
    }

    Particle[] StartCollidingSpirals(float distance)
    {
        Particle[] galaxyOne = StartSpiral(new Vector2(distance, 0), particleCount/2, spawnDiameter/2);
        Particle[] galaxyTwo = StartSpiral(new Vector2(-distance, 0), particleCount/2, spawnDiameter/2);

        return galaxyOne.Concat(galaxyTwo).ToArray();
    }

    Particle[] StartCollidingSpirals()
    {
        return StartCollidingSpirals(spawnDiameter);
    }

    void Update()
    {
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetFloat("_GravityStrength", gravityStrength);
        computeShader.SetFloat("_MaxColorSpeed", maxColorSpeed);
        computeShader.SetFloat("_ParticleCount", particleCount);

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