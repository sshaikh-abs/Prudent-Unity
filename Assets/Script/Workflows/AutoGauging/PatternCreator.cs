using System.Collections.Generic;
using UnityEngine;

public class PatternCreator : MonoBehaviour
{
    public Vector2 dimensions;
    [ReadOnly] public float area;
    [Range(0f, 0.5f)] public float cornerOffset = 0f;
    [Range(0f, 1f)] public float linePointSpread = 0.2f;

    public Vector3 up;
    public Vector3 forward;

    //2.236068

    public struct PatternBoxes
    {
        public Vector3 position;
        public Vector2 size;
    }

    private void OnValidate()
    {
        area = dimensions.x * dimensions.y;
    }

    public List<PatternBoxes> GetBoxes(Vector2 dimensions)
    {
        List<PatternBoxes> boxes = new List<PatternBoxes>();

        float maxArea = 5f;
        float squareSize = Mathf.Sqrt(maxArea);

        Vector3 origin = transform.position;

        float width = dimensions.x;
        float height = dimensions.y;

        float coveredX = 0f;
        float coveredY = 0f;

        // 1. Draw full square tiles (maxArea = 5m²)
        int countX = Mathf.FloorToInt(width / squareSize);
        int countY = Mathf.FloorToInt(height / squareSize);

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                Vector3 center = origin + new Vector3(
                    x * squareSize + squareSize / 2f,
                    y * squareSize + squareSize / 2f,
                    origin.z
                );

                boxes.Add(new PatternBoxes
                {
                    position = center,
                    size = new Vector2(squareSize, squareSize)
                });
            }
        }

        coveredX = countX * squareSize;
        coveredY = countY * squareSize;

        float leftoverX = width - coveredX;
        float leftoverY = height - coveredY;

        // 2. Fill right vertical strip (leftoverX x fullHeight)
        if (leftoverX > 0.01f)
        {
            for (int y = 0; y < countY; y++)
            {
                Vector3 center = origin + new Vector3(
                    coveredX + leftoverX / 2f,
                    y * squareSize + squareSize / 2f,
                    origin.z
                );

                boxes.Add(new PatternBoxes
                {
                    position = center,
                    size = new Vector2(leftoverX, squareSize)
                });
            }
        }

        // 3. Fill top horizontal strip (fullWidth x leftoverY)
        if (leftoverY > 0.01f)
        {
            for (int x = 0; x < countX; x++)
            {
                Vector3 center = origin + new Vector3(
                    x * squareSize + squareSize / 2f,
                    coveredY + leftoverY / 2f,
                    origin.z
                );

                boxes.Add(new PatternBoxes
                {
                    position = center,
                    size = new Vector2(squareSize, leftoverY)
                });
            }
        }

        // 4. Fill remaining top-right corner (leftoverX x leftoverY)
        if (leftoverX > 0.01f && leftoverY > 0.01f)
        {
            Vector3 center = origin + new Vector3(
                coveredX + leftoverX / 2f,
                coveredY + leftoverY / 2f,
                origin.z
            );

            boxes.Add(new PatternBoxes
            {
                position = center,
                size = new Vector2(leftoverX, leftoverY)
            });
        }

        return boxes;
    }

    public Vector2[] Get5Points(Vector2 center, Vector2 halfSize)
    {
        Vector2[] corners = new Vector2[0];

        // 4 corners in XZ plane
        corners = new Vector2[]
        {
            center + new Vector2(-halfSize.x, -halfSize.y), // Bottom-left
            center + new Vector2( halfSize.x, -halfSize.y), // Bottom-right
            center, // center
            center + new Vector2(-halfSize.x,  halfSize.y), // Top-left
            center + new Vector2( halfSize.x,  halfSize.y), // Top-right
        };

        return corners;
    }
    public Vector2[] Get3Points(Vector2 size, Vector2 center, Vector2 halfSize)
    {
        Vector2[] corners = new Vector2[0];

        Vector2 axis;
        float extent;

        if (size.x >= size.y)
        {
            axis = Vector2.right;
            extent = halfSize.x;
        }
        else
        {
            axis = Vector2.up;
            extent = halfSize.y;
        }

        float offset = extent * linePointSpread;

        corners = new Vector2[]
        {
            center - axis * offset,
            center,
            center + axis * offset
        };

        return corners;
    }
    public Vector2[] Get2Points(Vector2 size, Vector2 center, Vector2 halfSize)
    {
        Vector2[] corners = new Vector2[0];

        Vector2 axis;
        float extent;

        if (size.x >= size.y)
        {
            axis = Vector2.right;
            extent = halfSize.x;
        }
        else
        {
            axis = Vector2.up;
            extent = halfSize.y;
        }

        float twoPointOffset = extent * linePointSpread;

        corners = new Vector2[]
        {
            center - axis * twoPointOffset,
            center + axis * twoPointOffset
        };
        return corners;
    }
    public Vector2[] Get1Point(Vector2 center)
    {
        return new Vector2[]
        {
            center
        };
    }

    public Vector2[] GetPoints(PatternBoxes box)
    {
        Vector2 center = box.position;
        Vector2 halfSize = box.size / 2f;
        Vector2[] corners = new Vector2[0];
        float area = box.size.x * box.size.y;

        if (area >= 5f)
        {
            corners = Get5Points(center, halfSize);
        }
        else if (area >= 1f && area < 5f)
        {
            corners = Get3Points(box.size, center, halfSize);
        }
        else if (area > 0.5f && area < 1f)
        {
            corners = Get2Points(box.size, center, halfSize);
        }
        else
        {
            corners = Get1Point(center);
        }

        return corners;
    }

    public static Vector3 RotatePositionByDirections(Vector3 position, Vector3 dir1, Vector3 dir2)
    {
        // Ensure the input directions are normalized
        Vector3 xAxis = dir1.normalized;
        Vector3 yAxis = dir2.normalized;

        // Compute the third orthogonal vector using cross product
        Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;

        // Create a rotation matrix from the new basis
        Matrix4x4 rotationMatrix = new Matrix4x4();

        // Each column of the matrix is a basis vector
        rotationMatrix.SetColumn(0, xAxis);  // X direction
        rotationMatrix.SetColumn(1, yAxis);  // Y direction
        rotationMatrix.SetColumn(2, zAxis);  // Z direction
        rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1)); // Homogeneous coordinate

        // Rotate the position
        return rotationMatrix.MultiplyPoint3x4(position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        var boxes = GetBoxes(dimensions);

        foreach (var box in boxes)
        {
            //Gizmos.DrawWireCube(box.position, box.size);

            Vector2[] corners = GetPoints(box);

            foreach (var corner in corners)
            {
                Vector3 adjusted = Vector3.Lerp(corner, box.position, cornerOffset);
                adjusted.z = transform.position.z; // Ensure Z position is consistent
                Vector3 offset = - (new Vector3(dimensions.x, dimensions.y, 0f) / 2f);
                Vector3 actualPosition = adjusted + offset;

                actualPosition = RotatePositionByDirections((adjusted + offset) - transform.position, transform.right, transform.up) + transform.position;

                Gizmos.DrawSphere(actualPosition, 0.05f); // Adjust radius if needed
            }
        }
    }
}
