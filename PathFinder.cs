using System;
using System.Collections.Generic;
using System.Linq;

public static class PathFinder
{
    //distance f-ion should return distance between two adjacent nodes
    //estimate should return distance between any node and destination node
    public static Path<Node> FindPath<Node>(
        Node start,
        Node destination,
        Func distance,
        Func estimate)
        where Node : IHasNeighbours
    {
        //set of already checked nodes
        var closed = new HashSet();
        //queued nodes in open set
        var queue = new PriorityQueue> ();
        queue.Enqueue(0, new Path(start));

        while (!queue.IsEmpty)
        {
            var path = queue.Dequeue();

            if (closed.Contains(path.LastStep))
                continue;
            if (path.LastStep.Equals(destination))
                return path;

            closed.Add(path.LastStep);

            foreach (Node n in path.LastStep.Neighbours)
            {
                double d = distance(path.LastStep, n);
                //new step added without modifying current path
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
            }
        }

        return null;
    }
}