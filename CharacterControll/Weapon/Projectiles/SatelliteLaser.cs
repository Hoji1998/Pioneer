using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatelliteLaser : Projectile
{
    public void ImpactFeedback()
    {
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
    }
}
