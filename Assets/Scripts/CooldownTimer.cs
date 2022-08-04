using UnityEngine;

[System.Serializable]
public class CooldownTimer
{
    public float cooldownAmount = 0.5f;

    private float _cooldownCompleteTime;

    public bool CooldownComplete => Time.time > _cooldownCompleteTime;

    public void StartCooldown()
    {
        _cooldownCompleteTime = Time.time + cooldownAmount;
    }
}
