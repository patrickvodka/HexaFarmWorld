using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexPathfinding : MonoBehaviour
{
    public static HexPathfinding Instance;

    private void Awake()
    {
        Instance = this;
    }

    private readonly Vector3[] directions =
    {
        new Vector3(0, -1, 1),
        new Vector3(1, -1, 0),
        new Vector3(1, 0, -1),
        new Vector3(0, 1, -1),
        new Vector3(-1, 1, 0),
        new Vector3(-1, 0, 1)
    };

    public List<Vector3> FindPath(Vector3 start, Vector3 goal)
    {
        var grid = GameManager.Instance.wfc.HexGridDictionary;

        PriorityQueue<Vector3> frontier = new PriorityQueue<Vector3>();
        frontier.Enqueue(start, 0);

        Dictionary<Vector3, Vector3> cameFrom = new();
        Dictionary<Vector3, int> costSoFar = new();

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector3 current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (var dir in directions)
            {
                Vector3 next = current + dir;

                if (!grid.ContainsKey(next))
                    continue;

                int newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;

                    int priority = newCost + HexDistance(next, goal);

                    frontier.Enqueue(next, priority);

                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        List<Vector3> path = new();

        Vector3 temp = goal;

        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    int HexDistance(Vector3 a, Vector3 b)
    {
        return Mathf.RoundToInt(
            (Mathf.Abs(a.x - b.x)
            + Mathf.Abs(a.y - b.y)
            + Mathf.Abs(a.z - b.z)) / 2
        );
    }
}