using UnityEngine;

public static class Targeting
{
    public static Transform FindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player ? player.transform : null;
    }

    public static Vector2 DirTo(Transform from, Transform to)
        => (to.position - from.position).normalized;
}
