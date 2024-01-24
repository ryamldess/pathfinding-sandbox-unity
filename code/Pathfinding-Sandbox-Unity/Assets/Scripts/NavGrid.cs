using UnityEngine;

public class NavGrid : MonoBehaviour
{
    /// <summary>
    /// Given the current and desired location, return a path to the destination
    /// </summary>
    public NavGridPathNode[] GetPath(Vector3 origin, Vector3 destination)
    {
        return new NavGridPathNode[]
        {
            new() { Position = origin },
            new() { Position = destination }
        };
    }
}
