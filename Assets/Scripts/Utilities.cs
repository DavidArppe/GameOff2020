using UnityEngine;

public class Utilities
{
    public static float ActualSmoothstep(float edge0, float edge1, float x)
    {
        x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return x * x * (3 - 2 * x);
    }
}
