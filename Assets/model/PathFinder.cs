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
        Func<Node, Node, double> distance,
        Func<Node, Node, double> estimate)
        where Node : IHasNeighbours<Node>
    {
        //set of already checked nodes
        var closed = new HashSet<Node>();
        //queued nodes in open set
        var queue = new PriorityQueue<double, Path<Node>>();
        queue.Enqueue(0, new Path<Node>(start));

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
                queue.Enqueue(newPath.TotalCost + estimate(n, destination), newPath);
            }
        }

        return null;
    }
}