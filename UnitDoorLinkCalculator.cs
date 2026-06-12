using System.Globalization;

namespace PikminUnitEditor;

internal static class UnitDoorLinkCalculator
{
    //-------------------------------------------------------------------------------
    // UnitDefinition の door links を route の最短経路距離から再計算する処理
    //-------------------------------------------------------------------------------
    public static UnitDefinition RecalculateDoorLinks(UnitDefinition unitDefinition, RouteFile route)
    {
        if (unitDefinition.Doors.Count == 0 || route.Waypoints.Count == 0)
        {
            return unitDefinition;
        }

        Dictionary<(int FromDoor, int ToDoor), int> existingTekiFlags = unitDefinition.Doors
            .SelectMany(door => door.DoorLinks.Select(link => new KeyValuePair<(int, int), int>((door.Index, link.DoorId), link.TekiFlag)))
            .GroupBy(entry => entry.Key)
            .ToDictionary(group => group.Key, group => group.First().Value);
        Dictionary<int, List<RouteEdge>> graph = BuildUndirectedRouteGraph(route);
        List<DoorDefinition> orderedDoors = unitDefinition.Doors.OrderBy(door => door.Index).ToList();
        List<DoorDefinition> recalculatedDoors = new();

        foreach (DoorDefinition door in orderedDoors)
        {
            if (!route.Waypoints.ContainsKey(door.WayPointIndex))
            {
                recalculatedDoors.Add(door with { DoorLinks = Array.Empty<DoorLinkDefinition>() });
                continue;
            }

            Dictionary<int, float> distances = CalculateShortestWaypointDistances(door.WayPointIndex, graph);
            List<DoorLinkDefinition> links = new();
            foreach (DoorDefinition otherDoor in orderedDoors)
            {
                if (door.Index == otherDoor.Index ||
                    !route.Waypoints.ContainsKey(otherDoor.WayPointIndex) ||
                    !distances.TryGetValue(otherDoor.WayPointIndex, out float distance))
                {
                    continue;
                }

                int tekiFlag = existingTekiFlags.TryGetValue((door.Index, otherDoor.Index), out int existingTekiFlag)
                    ? existingTekiFlag
                    : 1;
                links.Add(new DoorLinkDefinition(distance, otherDoor.Index, tekiFlag));
            }

            recalculatedDoors.Add(door with { DoorLinks = links });
        }

        return unitDefinition with { Doors = recalculatedDoors };
    }

    //-------------------------------------------------------------------------------
    // RouteFile のリンクを双方向グラフとして構築する処理
    //-------------------------------------------------------------------------------
    private static Dictionary<int, List<RouteEdge>> BuildUndirectedRouteGraph(RouteFile route)
    {
        Dictionary<int, List<RouteEdge>> graph = route.Waypoints.Keys
            .ToDictionary(index => index, _ => new List<RouteEdge>());

        foreach (RouteWaypoint waypoint in route.Waypoints.Values)
        {
            foreach (int linkedIndex in waypoint.Links)
            {
                if (!route.Waypoints.TryGetValue(linkedIndex, out RouteWaypoint? linkedWaypoint))
                {
                    continue;
                }

                float distance = CalculateDistance(waypoint, linkedWaypoint);
                AddRouteEdge(graph, waypoint.Index, linkedIndex, distance);
                AddRouteEdge(graph, linkedIndex, waypoint.Index, distance);
            }
        }

        return graph;
    }

    //-------------------------------------------------------------------------------
    // Route グラフへ重複を避けて辺を追加する処理
    //-------------------------------------------------------------------------------
    private static void AddRouteEdge(Dictionary<int, List<RouteEdge>> graph, int fromIndex, int toIndex, float distance)
    {
        if (!graph.TryGetValue(fromIndex, out List<RouteEdge>? edges))
        {
            return;
        }

        RouteEdge? existingEdge = edges.FirstOrDefault(edge => edge.ToIndex == toIndex);
        if (existingEdge is null)
        {
            edges.Add(new RouteEdge(toIndex, distance));
            return;
        }

        if (distance < existingEdge.Distance)
        {
            edges.Remove(existingEdge);
            edges.Add(new RouteEdge(toIndex, distance));
        }
    }

    //-------------------------------------------------------------------------------
    // 起点 WayPoint から全 WayPoint への最短距離を求める処理
    //-------------------------------------------------------------------------------
    private static Dictionary<int, float> CalculateShortestWaypointDistances(int startIndex, Dictionary<int, List<RouteEdge>> graph)
    {
        Dictionary<int, float> distances = graph.Keys.ToDictionary(index => index, _ => float.PositiveInfinity);
        PriorityQueue<int, float> queue = new();
        distances[startIndex] = 0f;
        queue.Enqueue(startIndex, 0f);

        while (queue.Count > 0)
        {
            int currentIndex = queue.Dequeue();
            float currentDistance = distances[currentIndex];
            foreach (RouteEdge edge in graph[currentIndex])
            {
                float nextDistance = currentDistance + edge.Distance;
                if (nextDistance >= distances[edge.ToIndex])
                {
                    continue;
                }

                distances[edge.ToIndex] = nextDistance;
                queue.Enqueue(edge.ToIndex, nextDistance);
            }
        }

        return distances
            .Where(entry => !float.IsPositiveInfinity(entry.Value))
            .ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    //-------------------------------------------------------------------------------
    // WayPoint 間の X/Z 平面距離を計算する処理
    //-------------------------------------------------------------------------------
    private static float CalculateDistance(RouteWaypoint first, RouteWaypoint second)
    {
        float dx = second.X - first.X;
        float dz = second.Z - first.Z;
        return MathF.Sqrt((dx * dx) + (dz * dz));
    }

    private sealed record RouteEdge(int ToIndex, float Distance)
    {
        public override string ToString()
        {
            return $"{ToIndex}:{Distance.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
