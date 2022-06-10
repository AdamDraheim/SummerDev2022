using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodemap : MonoBehaviour
{

    [Header("Map values")]
    public LayerMask walkable;

    public Vector2 origin;
    public Vector2 size;

    public float raycastHeight;
    public float raycastDistance;

    [Header("Node values")]
    [Range(0.25f, 5.0f)]
    public float spacing;
    public float maxHeightDiff;

    [Header("Search Specs")]
    [Tooltip("How many search steps the algorithm will take")]
    public int maxSearchSize;
    [Tooltip("How expensive each step toward a goal costs")]
    public float stepCost;
    [Tooltip("How deep the recursive functions will go in finding an optimal path")]
    public int heuristicDepth;

    private Node[,] nodemap;
    public GameObject player;

    //Values for heuristics that may change per iteration
    private float[] heuristicVariables;
    private Node srcNode;
    private Node refNode;

    public static Nodemap nodeMapManager;

    public delegate float CalculateHeuristic();

    private int iter;

    public void Awake()
    {
        nodeMapManager = this;

        //Heuristic Variable 0 is always current step
        heuristicVariables = new float[1];

        ConstructMap();
        BuildNeighbors();
        iter = 0;

    }

    public void Update()
    {
        GenerateFlowMap(player.transform.position);
    }

    private void ConstructMap()
    {
        int sizeX = (int)(size.x / spacing);
        int sizeY = (int)(size.y / spacing);

        float shift = spacing / 2.0f;


        nodemap = new Node[sizeX, sizeY];

        for(int widx = 0; widx < sizeX; widx++)
        {
            for(int hidx = 0; hidx < sizeY; hidx++) 
            {
                Vector3 rayOrigin = new Vector3(origin.x + (widx * spacing) + (hidx % 2 == 0 ? shift : 0), 
                    raycastHeight, origin.y + (hidx * spacing)); 

                

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, walkable))
                {
                    Node newNode = new Node(hit.point);
                    nodemap[widx, hidx] = newNode;
                }
            }
        }

    }

    private void BuildNeighbors()
    {
        int sizeX = (int)(size.x / spacing);
        int sizeY = (int)(size.y / spacing);

        for (int widx = 0; widx < sizeX; widx++)
        {
            for (int hidx = 0; hidx < sizeY; hidx++)
            {
                Node currNode = nodemap[widx, hidx];

                if (currNode != null)
                {
                    if(hidx - 1 > 0)
                    {
                        AddNeighbor(currNode, nodemap[widx, hidx - 1]);
                    }
                    if (hidx + 1 < sizeY)
                    {
                        AddNeighbor(currNode, nodemap[widx, hidx + 1]);
                    }
                    if (widx - 1 > 0)
                    {
                        AddNeighbor(currNode, nodemap[widx-1, hidx]);
                    }
                    if (widx + 1 < sizeX)
                    {
                        AddNeighbor(currNode, nodemap[widx+1, hidx]);
                    }
                    if(hidx + 2 < sizeY)
                    {
                        AddNeighbor(currNode, nodemap[widx, hidx+2]);
                    }
                    if (hidx - 2 > 0)
                    {
                        AddNeighbor(currNode, nodemap[widx, hidx - 2]);
                    }

                    //Because every other row is offset, depending on offset neighbor is going
                    //to be reversed
                    if (hidx % 2 == 1)
                    {
                        if (widx > 0 && hidx > 0)
                        {
                            AddNeighbor(currNode, nodemap[widx - 1, hidx - 1]);
                        }

                        if (widx - 1 > sizeX && hidx < sizeY - 1)
                        {
                            AddNeighbor(currNode, nodemap[widx - 1, hidx + 1]);
                        }
                    }
                    else
                    {
                        //y_idx should never be zero but just in case
                        if (widx < sizeX - 1 && hidx > 0)
                        {
                            AddNeighbor(currNode, nodemap[widx + 1, hidx - 1]);
                        }

                        if (widx < sizeX - 1 && hidx < sizeY - 1)
                        {
                            AddNeighbor(currNode, nodemap[widx + 1, hidx + 1]);

                        }
                    }

                }
            }
        }
    }

    public Vector3 GetAStarPath(Vector3 startPos, Vector3 goalPos)
    {

        Node startNode = GetNearestNode(startPos);
        startNode.SetPrevious(null);
        Node goalNode = GetNearestNode(goalPos);

        List <Node> visited = new List<Node>();
        visited.Add(startNode);
        PriorityQueue<Node> nodeSearch = new PriorityQueue<Node>();

        nodeSearch.Enqueue(startNode, 0);

        Node current = startNode;

        int steps = 0;
        //Run through algorithm for max search steps, break if found sooner
        for(; steps < maxSearchSize; steps++)
        {
            if(nodeSearch.GetSize() == 0)
            {
                return Vector3.zero;
            }

            current = nodeSearch.Dequeue();

            if (current.Equals(goalNode))
            {
                break;
            }

            foreach(Node neighbor in current.GetNeighbors())
            {
                //make sure don't add node twice
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.SetPrevious(current);

                    //heuristic is previous value - previous target distance + new target distance + stepCost
                    float heuristic = current.GetValue() +
                        -Vector3.Distance(current.GetPosition(), goalNode.GetPosition())
                        + Vector3.Distance(neighbor.GetPosition(), goalNode.GetPosition()) 
                        + stepCost;

                    nodeSearch.Enqueue(neighbor, heuristic);
                }
            }
        }

        //Couldn't find a path in enough steps
        if(steps >= maxSearchSize)
        {
            Debug.Log("Unable to find a A* path in " + maxSearchSize + " steps");
            return new Vector3(0, 0, 0);
        }

        //Find the direction
        Vector3 direction = Vector3.zero;
        while(current.GetPrevious() != null)
        {
            //interpolate between old direction and new direction
            direction = (direction * 0.4f) + (0.6f * (current.GetPosition() - current.GetPrevious().GetPosition()));
            //Normalize the direction if it is not zero (in case start and end node are same don't do anything)
            if (direction.magnitude != 0)
            {
                direction = direction.normalized;
            }
            current = current.GetPrevious();
        }

        return direction;
    }

    /// <summary>
    /// Finds the best location away from the target. Let range=-1 if no limit to range
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="avoidOrigin"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    public Vector3 MoveAwayFromTarget(Vector3 startPos, Vector3 avoidOrigin, float range=-1)
    {

        Node currNode = GetNearestNode(startPos);
        refNode = GetNearestNode(avoidOrigin);
        currNode.SetNext(null);

        if (range < 0)
        {
            currNode = Search(currNode, refNode, 
                delegate { return MaximizeDistance(currNode, refNode); });
        }
        else
        {
            string[] heuristics = new string[]{ "MaximizeDistanceClamped", "StepCost"};
            float[] args = new float[] { range, 0};
            float[] scalars = new float[] { 1, 1 };

            srcNode = currNode;

            currNode = Search(currNode, refNode,
                delegate { return CombineHeuristic(heuristics, args, scalars); });
        }

        //Reach the last node to go to, until no more to reach
        //If the currDepth is greater than avoid origin, then could be an infinite loop and break
        int currDepth;
        for(currDepth = 0; currDepth < heuristicDepth; currDepth++)
        {

            //Make sure next node is not null
            if(currNode.GetNext() != null)
                currNode = currNode.GetNext();

        }

        //Once know best position, figure out how to get there
        return GetAStarPath(startPos, currNode.GetPosition());
    }

    /// <summary>
    /// Searches for the best node that fits the heuristic criteria
    /// </summary>
    /// <param name="currNode"></param>
    /// <param name="goalNode"></param>
    /// <param name="heuristic"></param>
    /// <param name="scoreThreshold"></param>
    /// <returns></returns>
    private Node Search(Node currNode, Node goalNode, CalculateHeuristic heuristic, float scoreThreshold=-1.0f)
    {
        //Make data structures
        PriorityQueue<Node> toSearch = new PriorityQueue<Node>();
        List<Node> visited = new List<Node>();

        //Set up starting node
        Node current = currNode;
        current.SetValue2(0);
        heuristicVariables[0] = 0;

        srcNode = current;
        refNode = goalNode;

        current.SetValue(heuristic());

        visited.Add(current);

        //Node that we consider to be our best heuristic thus far
        Node best = current;

        toSearch.Enqueue(currNode, 0);

        for (int step = 0; step < maxSearchSize; step++)
        {
            //Break out of loop if search size is zero
            if(toSearch.GetSize() == 0)
            {
                break;
            }

            current = toSearch.Dequeue();

            //Compare the current node heuristic to the current best heuristic
            if(current.GetValue() > best.GetValue())
            {
                best = current;
            }

            //If scorethreshold is considered and the heuristic outperforms it, then break
            if(scoreThreshold > 0 && current.GetValue() > scoreThreshold)
            {
                break;
            }
            
            foreach (Node node in current.GetNeighbors())
            {
                if (!visited.Contains(node))
                {
                    node.SetValue2(current.GetValue2() + 1);
                    heuristicVariables[0] = node.GetValue2();

                    srcNode = node;
                    node.SetValue(heuristic());
                    
                    node.SetPrevious(current);
                    visited.Add(node);
                    //enqueue the negative so we get highest heuristics first
                    toSearch.Enqueue(node, -node.GetValue());
                }
            }
        }
        //Debug.Log(currNode.GetPosition() + " " + best.GetPosition() + " " + best.GetValue() + " " + best.GetValue2());
        return best;
    }


    private void GenerateFlowMap(Vector3 position)
    {

        iter += 1;
        iter %= 2;

        Node startPoint = GetNearestNode(position);

        Queue<Node> toVisit = new Queue<Node>();

        startPoint.visited = iter;
        toVisit.Enqueue(startPoint);
        int count = 0;
        while(toVisit.Count > 0)
        {
            count += 1;
            Node current = toVisit.Dequeue();

            foreach(Node node in current.GetNeighbors())
            {
                if (node.visited != iter)
                {
                    node.visited = iter;
                    node.SetMovementDirection((current.GetPosition() - node.GetPosition()).normalized);
                    toVisit.Enqueue(node);
                }
            }
        }

    }

    public Vector3 GetFlowDirectionAtPosition(Vector3 pos)
    {
        return GetNearestNode(pos).GetMovementDirection();
    }

    private float MaximizeDistance(Node current, Node avoid)
    {
        return Vector3.Distance(current.GetPosition(), avoid.GetPosition());
    }

    private float MaximizeDistanceClamped(Node current, Node avoid, float maxValue)
    {
        float dist = Vector3.Distance(current.GetPosition(), avoid.GetPosition());

        dist = (dist > maxValue ? maxValue : dist);

        return dist;
    }

    private float MinimizeDistance(Node current, Node avoid, float maxValue)
    {
        float dist = Vector3.Distance(current.GetPosition(), avoid.GetPosition());

        return maxValue - dist;
    }


    /// <summary>
    /// Combination heuristic calculation where can specify algorithms for determining heuristics
    /// </summary>
    /// <param name="srcNode"></param>
    /// <param name="refNode"></param>
    /// <param name="heuristics"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private float CombineHeuristic(string[] heuristics, float[] args, float[] scalars)
    {
        float heuristicRes = 0;
        int argsIdx = 0;
        int scalarsIdx = 0;
        
        foreach(string heuristic in heuristics)
        {
            float add = 0;
            switch (heuristic)
            {

                case "MaximizeDistance":
                    add = MaximizeDistance(srcNode, refNode);
                    break;
                case "MaximizeDistanceClamped":
                    
                    add = MaximizeDistanceClamped(srcNode, refNode, args[argsIdx]);
                    argsIdx += 1;
                    break;
                case "MinimizeDistance":
                    add = MinimizeDistance(srcNode, refNode, args[argsIdx]);
                    argsIdx += 1;
                    break;
                case "StepCost":
                    add = heuristicVariables[(int)(args[argsIdx])] * -stepCost;
                    argsIdx += 1;
                    break;
                default:
                    Debug.LogError("No heuristic algorithm named " + heuristic);
                    break;
            }
            heuristicRes += scalars[scalarsIdx] * add;
            scalarsIdx += 1;
        }

        return heuristicRes;


    }


    private Node GetNearestNode(Vector3 pos)
    {

        int nearestX = (int)((pos.x - origin.x) / spacing);
        int nearestY = (int)((pos.z - origin.y) / spacing);

        Node current = nodemap[nearestX, nearestY];

        return current;
    }

    private void AddNeighbor(Node curr, Node neighbor)
    {
        if (neighbor != null)
        {
            if (Mathf.Abs(curr.GetPosition().y - neighbor.GetPosition().y) < maxHeightDiff)
            {
                curr.AddNeighbor(neighbor);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        
        if (nodemap != null)
        {

            int sizeX = (int)(size.x / spacing);
            int sizeY = (int)(size.y / spacing);
            for (int widx = 0; widx < sizeX; widx++)
            {
                for (int hidx = 0; hidx < sizeY; hidx++)
                {
                    if (nodemap[widx, hidx] != null)
                    {
                        Gizmos.DrawCube(nodemap[widx, hidx].GetPosition(), Vector3.one / 5);

                        //Uncomment to see navmesh neighbors
                        /*
                        if (widx > 10 && widx < 50 && hidx > 10 && hidx < 50)
                        {
                            foreach (Node neighbor in nodemap[widx, hidx].GetNeighbors())
                            {
                                Gizmos.DrawLine(nodemap[widx, hidx].GetPosition(), neighbor.GetPosition());
                            }
                        }
                        */
                    }
                }
            }
        }
    }
}

public class Node
{

    private Vector3 position;
    private List<Node> neighbors;

    //runtime values
    private float value;
    private float value2;
    private Node previous;
    private Node next;
    public int visited;

    private Vector3 MovementDirection;

    public Node(Vector3 pos)
    {
        this.position = pos;
        this.neighbors = new List<Node>();
        this.value = 0;
        this.value2 = 0;
        this.previous = null;
        this.next = null;
    }

    public void AddNeighbor(Node neighbor)
    {
        this.neighbors.Add(neighbor);
    }

    public List<Node> GetNeighbors()
    {
        return this.neighbors;
    }

    public Vector3 GetPosition()
    {
        return this.position;
    }

    public float GetValue()
    {
        return this.value;
    }

    public void SetValue(float value)
    {
        this.value = value;
    }

    public float GetValue2()
    {
        return this.value2;
    }

    public void SetValue2(float value)
    {
        this.value2 = value;
    }

    public void SetPrevious(Node pre)
    {
        this.previous = pre;
    }

    public Node GetPrevious()
    {
        return this.previous;
    }

    public void SetNext(Node pre)
    {
        this.next = pre;
    }

    public Node GetNext()
    {
        return this.next;
    }

    public void SetMovementDirection(Vector3 dir)
    {
        this.MovementDirection = dir;
    }

    public Vector3 GetMovementDirection()
    {
        return this.MovementDirection;
    }


}
