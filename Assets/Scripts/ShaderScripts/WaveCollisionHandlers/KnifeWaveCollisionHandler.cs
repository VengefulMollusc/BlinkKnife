using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeWaveCollisionHandler : WaveCollisionHandler
{
    private KnifeController knifeController;
    private const float returnDepth = 0.2f;

    // Use this for initialization
    void Start()
    {
        knifeController = GetComponent<KnifeController>();
    }

    public override void CollideWithWave(WavePositionInfo waveInfo)
    {
        if (waveInfo.position.y - transform.position.y > returnDepth)
        {
            knifeController.ReturnKnifeTransition();
        }
    }
}
