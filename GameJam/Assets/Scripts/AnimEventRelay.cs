using UnityEngine;

public class AnimEventRelay : MonoBehaviour
{
    // Called by the animation event (no params, public)
    public void OnDeathAnimationFinished()
    {
        var hp = GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.OnDeathAnimationFinished();
    }
}
