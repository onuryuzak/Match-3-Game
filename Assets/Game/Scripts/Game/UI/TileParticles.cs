using UnityEngine;
using UnityEngine.Assertions;


public class TileParticles : MonoBehaviour
{
    public ParticleSystem fragmentParticles;

    private void Awake()
    {
        Assert.IsNotNull(fragmentParticles);
    }
}