using UnityEngine;

public class SphereMarkup : BaseMarkup
{
    public float radius = 1f;

    public override void ValidateShape()
    {
        transform.localScale = new Vector3(radius, (data.markupType == "sphere") ? radius : transform.localScale.y, radius);
        CalculateArea();
    }

    public override void CalculateArea()
    {
        if(data.markupType == "sphere")
        {
            data.area = (4f / 3f) * Mathf.PI * radius * radius * radius;
        }
        else
        {
            data.area = Mathf.PI * radius * radius;
        }
    }

    public override void ParseDimensions()
    {
        radius = data.dimensions[0];
    }
}
