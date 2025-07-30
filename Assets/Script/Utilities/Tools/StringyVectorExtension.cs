using UnityEngine;

public static class StringyVectorExtension
{
    public static StringyVector3 GetStringyVector(this Vector3 value)
    {
        return new StringyVector3()
        {
            x = value.x.ToString(),
            y = value.y.ToString(),
            z = value.z.ToString()
        };
    }

}
