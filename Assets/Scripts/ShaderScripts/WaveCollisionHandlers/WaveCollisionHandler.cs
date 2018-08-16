using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaveCollisionHandler : MonoBehaviour
{
    public abstract void CollideWithWave(WavePositionInfo waveInfo);
}
