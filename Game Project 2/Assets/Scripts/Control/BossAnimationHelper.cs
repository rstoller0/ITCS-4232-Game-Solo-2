using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationHelper : MonoBehaviour
{
    [SerializeField] private Boss1_4 bossScript;

    public void Step()
    {
        bossScript.Step();
    }

    public void SetPhase(int phaseChange)
    {
        bossScript.SetPhase(phaseChange);
    }

    //dagger swipe functions
    #region
    public void DaggerSwipe()
    {
        bossScript.DaggerSwipe();
    }
    #endregion

    //dagger pound functions
    #region
    public void DaggerPound()
    {
        bossScript.DaggerPound();
    }
    #endregion

    public void AttackStart()
    {
        bossScript.AttackStart();
    }

    public void AttackStop()
    {
        bossScript.AttackStop();
    }

    public void ThrowDagger()
    {
        bossScript.ThrowDagger();
    }

    //death functions
    #region
    public void StayDead()
    {
        bossScript.StayDead();
    }
    #endregion
}
