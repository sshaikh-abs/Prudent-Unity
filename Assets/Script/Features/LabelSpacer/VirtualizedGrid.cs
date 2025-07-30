using EasyButtons;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

public class VirtualizedGrid : MonoBehaviour
{
    public Vector3 cellDimensions;
    public Vector3 gridDimensions;
    public float zOffset = 0.1f;

    public List<Cell> cells = new List<Cell>();

    public List<Transform> target = new List<Transform>();
    public List<Transform> preMarked = new List<Transform>();
    public List<Transform> occupiency = new List<Transform>();

    [Button]
    public void GenerateGrid()
    {
        cells.Clear();
        int rows = Mathf.FloorToInt(gridDimensions.x / cellDimensions.x);
        int columns = Mathf.FloorToInt(gridDimensions.y / cellDimensions.y);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector3 cellPosition = new Vector3(
                    row * cellDimensions.x,
                    column * cellDimensions.y,
                    0
                );
                cells.Add(new Cell()
                {
                    position = cellPosition
                });
            }
        }
    }

    public Cell GetSampledCell(Vector3 gameObjectPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(gameObjectPosition);
        int row = Mathf.FloorToInt(localPosition.x / cellDimensions.x);
        int column = Mathf.FloorToInt(localPosition.y / cellDimensions.y);
        int totalRows = Mathf.FloorToInt(gridDimensions.x / cellDimensions.x);
        int totalColumns = Mathf.FloorToInt(gridDimensions.y / cellDimensions.y);

        if (row < 0 || row >= totalRows || column < 0 || column >= totalColumns)
            return null; // Out of bounds

        int index = (row * totalColumns) + column;
        Cell sampledCell = cells[index];

        if (!sampledCell.occupied)
            return sampledCell;

        // Search outward from the sampled cell for the nearest unoccupied neighbor
        int searchRadius = 1;
        while (searchRadius < Mathf.Max(totalRows, totalColumns))
        {
            List<Cell> neighbours = GetNeighboursWithRadius(row, column, searchRadius, totalRows, totalColumns);

            // Sort neighbors by distance to original position
            neighbours.Sort((a, b) =>
                Vector3.Distance(localPosition, a.position).CompareTo(
                    Vector3.Distance(localPosition, b.position))
            );

            foreach (var cell in neighbours)
            {
                if (!cell.occupied)
                    return cell;
            }

            searchRadius++;
        }

        return null; // No available cell found
    }

    private List<Cell> GetNeighboursWithRadius(int centerRow, int centerColumn, int radius, int totalRows, int totalColumns)
    {
        List<Cell> neighbours = new List<Cell>();

        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (Mathf.Abs(i) != radius && Mathf.Abs(j) != radius) continue; // Only edge cells of this ring

                int neighbourRow = centerRow + i;
                int neighbourColumn = centerColumn + j;

                if (neighbourRow >= 0 && neighbourRow < totalRows &&
                    neighbourColumn >= 0 && neighbourColumn < totalColumns)
                {
                    int index = neighbourRow * totalColumns + neighbourColumn;
                    neighbours.Add(cells[index]);
                }
            }
        }

        return neighbours;
    }

    private void ResetVisiting()
    {
        foreach (var cell in cells)
        {
            cell.occupied = false;
        }
    }

    public void MarkOccupancy(List<Transform> gameObjectPositions)
    {
        foreach (var position in gameObjectPositions)
        {
            Cell sampledCell = GetSampledCell(position.position);
            if (sampledCell == null)
            {
                continue;
            }
            sampledCell.occupied = true;
        }
    }

    public List<(Cell, Transform)> GetPresentedCells(List<Transform> gameObjectPositions, List<Transform> occupancies)
    {
        ResetVisiting();
        MarkOccupancy(occupancies);
        List<(Cell, Transform)> presentedCells = new List<(Cell, Transform)>();
        foreach (var position in gameObjectPositions)
        {
            Cell sampledCell = GetSampledCell(position.position);
            if(sampledCell == null)
            {
                continue;
            }
            if (sampledCell != null && !presentedCells.Exists(c => c.Item1 == sampledCell))
            {
                sampledCell.occupied = true;
                presentedCells.Add((sampledCell, position));
            }
        }
        return presentedCells;
    }

    public void UpdatePositionsInGrid()
    {
        if (target == null)
        {
            return;
        }

        target.Clear();
        preMarked.Clear();

        foreach (Transform item in transform)
        {
            if(!item.name.Contains("PLATENAME_") || item.name.Contains("_MOVE"))
            {
                target.Add(item);
            }

            preMarked.Add(item);
        }

        Vector3 gridD = gridDimensions;

        if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.right)) > 0.1f)
        {
            gridD.z = gridDimensions.x;
            gridD.y = gridDimensions.y;
            gridD.x = gridDimensions.z;
        }

        Vector3 distanceChecker = transform.position - (gridD / 2f);

        //plateNameTarget = plateNameTarget.OrderBy(t => Vector3.Distance(t.position, distanceChecker)).ToList();
        target = target.OrderBy(t => Vector3.Distance(t.position, distanceChecker)).ToList();

        //plateNameTarget.AddRange(target);
        //target = plateNameTarget;

        var presentingCells = GetPresentedCells(target, preMarked);

        foreach (var sampledCell in presentingCells)
        {
            Vector3 pos = transform.TransformPoint(sampledCell.Item1.position);
            //Vector3 pos = PatternFetcher.RotatePositionByDirections(sampledCell.Item1.position + transform.position, transform.right, transform.up);
            sampledCell.Item2.GetChild(0).transform.position = pos - (transform.forward * zOffset);
            sampledCell.Item2.GetComponent<LineCreator>().UpdateLine();
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var cell in cells)
        {
            Vector3 pos = transform.TransformPoint(cell.position);

            Vector3 cellD = cellDimensions;

            if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.right)) > 0.1f)
            {
                cellD.z = cellDimensions.x;
                cellD.y = cellDimensions.y;
                cellD.x = cellDimensions.z;
            }

            //var pos = PatternFetcher.RotatePositionByDirections(cell.position + transform.position, transform.right, transform.up);
            Gizmos.DrawWireCube(pos, cellD);
        }

        if(target == null)
        {
            return;
        }

        target.Clear();
        foreach (Transform item in transform)
        {
            target.Add(item);
        }

        target = target.OrderBy(t => Vector3.Distance(t.position, transform.position)).ToList();

        var presentingCells = GetPresentedCells(target, target);

        Gizmos.color = Color.blue;
        foreach (var item in target)
        {
            //Gizmos.DrawSphere(item.position, 0.1f); // Adjust radius if needed
        }

        Gizmos.color = Color.red;
        foreach (var sampledCell in presentingCells)
        {
            Vector3 pos = transform.TransformPoint(sampledCell.Item1.position);
            if(!Application.isPlaying)
            {
                //pos = PatternFetcher.RotatePositionByDirections(sampledCell.Item1.position + transform.position, transform.right, transform.up);
                sampledCell.Item2.GetChild(0).transform.position = pos;
            }

            Vector3 cellD = cellDimensions;

            if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.right)) > 0.1f)
            {
                cellD.z = cellDimensions.x;
                cellD.y = cellDimensions.y;
                cellD.x = cellDimensions.z;
            }
            Gizmos.DrawWireCube(pos, cellD);
        }
    }
}

public class Cell
{
    public Vector3 position;
    public bool occupied = false;
}
