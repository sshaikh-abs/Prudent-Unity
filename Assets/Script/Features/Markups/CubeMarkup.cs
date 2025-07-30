using UnityEngine;

public class CubeMarkup : BaseMarkup
{
    public Vector3 dimensions;

    public override void ValidateShape()
    {
        transform.localScale = dimensions;
        CalculateArea();
    }

    public override void CalculateArea()
    {
        data.area = dimensions.x * dimensions.y * dimensions.z;
    }

    public override void ParseDimensions()
    {
        switch (data.markupType)
        {
            case "cuboid":
               dimensions = new Vector3(data.dimensions[0], data.dimensions[1], data.dimensions[2]);
                break;
            case "cube":
                dimensions = Vector3.one * data.dimensions[0];
                break;
            default:
                break;
        }
    }
}
