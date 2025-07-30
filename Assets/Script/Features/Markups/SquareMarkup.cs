using UnityEngine;

public class SquareMarkup : BaseMarkup
{
    public Vector2 dimensions = Vector2.one;

    public override void ValidateShape()
    {
        transform.localScale = new Vector3(dimensions.x, 1f, dimensions.y);
        CalculateArea();
    }

    public override void CalculateArea()
    {
        data.area = (dimensions.x * dimensions.y);
    }

    public override void ParseDimensions()
    {
        switch (data.markupType)
        {
            case "square":
                dimensions = Vector2.one * data.dimensions[0];
                break;
            case "rectangle":
                dimensions = new Vector2(data.dimensions[0], data.dimensions[1]);
                break;
            default:
                break;
        }
    }
}
