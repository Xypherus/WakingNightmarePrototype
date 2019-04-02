using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// A component used to create a navmesh within a specific area. This object creates a grid of nodes for each given agent, and updates those nodes corresponding to the environment.
/// </summary>
public class NavmeshArea2D : MonoBehaviour {
    #region Editor Variables
    [Tooltip("The spacing between the nodes. lower is more accurate, but less efficient.")]
    public float resolution;
    [Tooltip("The layers that will effect the navigable area of the navmesh.")]
    public LayerMask layerMask;
    [Tooltip("The agents to create navmeshes for")]
    public List<NavmeshAgent2D> agents;
    [Tooltip("The 2D box to limit the creation of the navmesh.")]
    public Bounds bounds;
    public int debugIndex;
    public bool showDebug;
    #endregion

    public Dictionary<NavmeshAgent2D, NavmeshNode2D[,]> meshes;

    private int xcount;
    private int ycount;

    new Collider2D collider;

    float updateRate = 0.02f;
    float lastUpdate;

    // Use this for initialization
    protected void Start () {
        collider = GetComponent<CompositeCollider2D>();
        InitializeGrid();
	}

	
	// Update is called once per frame
	protected void Update () {
        if (resolution <= 0) { resolution = 0.1f; }

        DrawDebugLines();
    }

    /// <summary>
    /// Convert a position in world space to the integer position in grid space. Useful for indexing into the Navmesh grid.
    /// </summary>
    /// <param name="worldPosition">The position to convert to grid space.</param>
    /// <returns>The converted position from world space to grid space.</returns>
    public Vector2Int WorldToGridPosition(Vector2 worldPosition) {
        Vector2Int gridPosition = new Vector2Int(Mathf.RoundToInt((worldPosition.x - bounds.min.x) / resolution), Mathf.CeilToInt((worldPosition.y - bounds.min.y) / resolution));
        return gridPosition;
    }

    public int GetSize() {
        return xcount * ycount;
    }

    private void DrawDebugLines() {
        if (!showDebug) { return; }
        for(int y = 0; y < ycount; y++)
        {
            for(int x = 0; x < xcount; x++)
            {
                foreach (NavmeshNode2D.NavmeshNodeConnection2D connection in meshes[agents[ debugIndex]][x,y].connections)
                {
                    if (connection.a.type == NavmeshNode2D.NodeType.None || 
                        connection.b.type == NavmeshNode2D.NodeType.None || 
                        connection.b.type == NavmeshNode2D.NodeType.Air  /*||
                        connection.jump*/) { continue; }
                    Color lineColor = Color.white;
                    switch (meshes[agents[debugIndex]][x,y].type)
                    {
                        case NavmeshNode2D.NodeType.Walkable:
                            lineColor = Color.blue;
                            break;
                        case NavmeshNode2D.NodeType.Crawlable:
                            lineColor = Color.yellow;
                            break;
                        case NavmeshNode2D.NodeType.Ladder:
                            lineColor = Color.magenta;
                            break;
                        default:
                            continue;
                    }

                    Debug.DrawLine(connection.a.worldPosition, connection.b.worldPosition, lineColor);
                }
            }
        }
    }

    /// <summary>
    /// Update the Navmesh grid within the bounds of a given collider.
    /// </summary>
    /// <param name="testCollider">Update the nodes within and around this collider.</param>
    public void UpdateGrid(Collider2D testCollider) {
        if (!(Time.time >= lastUpdate + updateRate)) { return; }
        foreach (KeyValuePair<NavmeshAgent2D, NavmeshNode2D[,]> mesh in meshes) {
            UpdateGrid(testCollider, mesh.Key);
        }
    }

    private void UpdateGrid(Collider2D testCollider, NavmeshAgent2D agent) {
        Vector2 min = WorldToGridPosition(new Vector2 (testCollider.bounds.min.x - 1, testCollider.bounds.min.y - 1));
        Vector2 max = WorldToGridPosition(new Vector2(testCollider.bounds.max.x + 1, testCollider.bounds.max.y + 1));
        int xstart = (int)min.x;
        int ystart = (int)min.y;
        int xlimit = (int)max.x;
        int ylimit = (int)max.y;

        for (int x = xstart; x < (xlimit) && x < xcount-1; x++) {
            if (x < 0) { x = 0; }
            
            for (int y = ystart; y < (ylimit) && y < ycount-1; y++) {
                if (y < 0) { y = 0; }
                int radius;

                if (agent.jumpDistance <= resolution) { radius = 1; }
                else { radius = Mathf.RoundToInt(agent.jumpDistance / resolution); }
                NavmeshNode2D node = meshes[agent][x, y];
                node.connections.Clear();
                node.Update();

                UnityEngine.Profiling.Profiler.BeginSample("Checking Connections");

                for (int newx = x - radius; newx < x + radius; newx++) {
                    if (newx > xcount-1) { break; }
                    else if (newx < 0 ) { continue; }
                    for (int newy = y - radius; newy < y + radius; newy++) {
                        if (newy > ycount-1 || newy < 0) { continue; }
                        else if (newx == x && newy == y) { continue; }

                        NavmeshNode2D checkNode = meshes[agent][newx, newy];

                        if (Mathf.Abs(x- newx) <= 1 && Mathf.Abs(y-newy) <= 1)
                        {
                            node.Connect(checkNode);
                        }
                        else if (Vector2.Distance(checkNode.worldPosition, node.worldPosition) <= agent.jumpDistance)
                        {
                            node.Connect(checkNode, true);
                        }
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
        lastUpdate = Time.time;
    }

    /// <summary>
    /// Initialize the Navmesh grid.
    /// </summary>
    public void InitializeGrid() {
        xcount = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / resolution) + 1;
        ycount = Mathf.CeilToInt((bounds.max.y - bounds.min.y) / resolution) + 1;

        meshes = new Dictionary<NavmeshAgent2D, NavmeshNode2D[,]>();
        foreach (NavmeshAgent2D agent in agents) {
            meshes.Add(agent, new NavmeshNode2D[xcount,ycount]);
            for (int x = 0; x < xcount; x++) {
                for (int y = 0; y < ycount; y++) {
                    meshes[agent][x, y] = new NavmeshNode2D(agent, this, (Vector2) bounds.min + (new Vector2(x * resolution,y * resolution)), new Vector2Int(x,y));
                }
            }
            InitializeGrid(agent);
        }
    }
    
    private void InitializeGrid(NavmeshAgent2D agent) {

        for (int x = 0; x < xcount-1; x++) {
            for(int y = 0; y < ycount-1; y++) {
                int radius;

                if (agent.jumpDistance <= resolution) { radius = 1; }
                else { radius = Mathf.RoundToInt(agent.jumpDistance / resolution); }
                
                
                for (int newx = x - radius; newx <= x + radius; newx++)
                {
                    if (newx > xcount-1) { break; }
                    else if (newx < 0) { continue; }
                    for (int newy = y - radius; newy <= y + radius; newy++)
                    {
                        if (newy > ycount-1 || newy < 0) { continue; }
                        else if (newx == x && newy == y) { continue; }

                        if (Mathf.Abs(x - newx) <= 1 && Mathf.Abs(y - newy) <= 1)
                        {
                            meshes[agent][x, y].Connect(meshes[agent][newx, newy]);
                        }
                        else if (Vector2.Distance(meshes[agent][newx, newy].worldPosition, meshes[agent][x, y].worldPosition) <= agent.jumpDistance)
                        {
                            meshes[agent][x, y].Connect(meshes[agent][newx, newy], true);
                        }
                    }
                }
                meshes[agent][x, y].Update();
            }
        }

    }

    /// <summary>
    /// Get specific node types inside a circle with a given radius.
    /// </summary>
    /// <param name="agent">Required to specify which Navmesh to check</param>
    /// <param name="origin">The center of the circle in world space</param>
    /// <param name="checkTypes">The nodes to check for</param>
    /// <param name="radius">The radius of the circle to check</param>
    /// <returns>A list of nodes, if any found. Null if no nodes found.</returns>
    public List<NavmeshNode2D> NodesOfTypeInRange(NavmeshAgent2D agent, Vector2 origin, List<NavmeshNode2D.NodeType> checkTypes, float radius) {
        Vector2Int gridOrigin = WorldToGridPosition(origin);
        int gridRadius = Mathf.RoundToInt(radius / resolution);
        List<NavmeshNode2D> nodes = new List<NavmeshNode2D>();

        for (int newx = gridOrigin.x - gridRadius; newx <=gridOrigin.x + gridRadius; newx++)
        {
            if (newx > xcount - 1) { break; }
            else if (newx < 0) { continue; }
            for (int newy = gridOrigin.y - gridRadius; newy <=gridOrigin.y + gridRadius; newy++)
            {
                if (Vector2.Distance(origin, meshes[agent][newx, newy].worldPosition) > radius) { continue; }

                if (checkTypes.Contains(meshes[agent][newx, newy].type)) {
                    nodes.Add(meshes[agent][newx, newy]);
                }
            }
        }

        if (nodes.Count > 0) { return nodes; }
        else { return null; }
    }

    /// <summary>
    /// Check if a straight line from the origin of a node in a given direction contains another node of a certain type.
    /// </summary>
    /// <param name="gridPosition">The grid position of the origin node.</param>
    /// <param name="direction">The direction to cast. This will be normalized to an 8-way direction.</param>
    /// <param name="types">The types to check for.</param>
    /// <param name="agent">Required to specify which mesh to use.</param>
    /// <param name="distance">How far the cast will check.</param>
    /// <returns>The node with the given type if found. Null if none found.</returns>
    public NavmeshNode2D CastForType(Vector2Int gridPosition, Vector2 direction, List<NavmeshNode2D.NodeType> types, NavmeshAgent2D agent, int distance = 1000) {

        UnityEngine.Profiling.Profiler.BeginSample("Grid Cast");
        Vector2Int position = gridPosition;

        position.x = Mathf.Clamp(position.x + Mathf.RoundToInt(Mathf.Clamp(direction.x, -1, 1)), 0, xcount - 1);
        position.y = Mathf.Clamp(position.y + Mathf.RoundToInt(Mathf.Clamp(direction.y, -1, 1)), 0, ycount - 1);
        int count = 0;
        do
        {
            NavmeshNode2D node = meshes[agent][position.x, position.y];
            if (types.Contains(node.type))
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return node;
            }

            position.x = Mathf.Clamp(position.x + Mathf.RoundToInt(Mathf.Clamp(direction.x, -1, 1)), 0, xcount - 1);
            position.y = Mathf.Clamp(position.y + Mathf.RoundToInt(Mathf.Clamp(direction.y, -1, 1)), 0, ycount - 1);
            count++;

        }
        while (count < distance);

        UnityEngine.Profiling.Profiler.EndSample();
        return null;
    }

    /// <summary>
    /// Find the node in the navmesh that is closest to the given position.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="agent">Used to specify which mesh to use.</param>
    /// <returns></returns>
    public NavmeshNode2D NodeAtPoint(Vector2 position, NavmeshAgent2D agent) {
        if (meshes == null) { return null; }
        else if (meshes[agent] == null) { return null; }
        NavmeshNode2D node = null;

        Vector2 gridPos = new Vector2(Mathf.RoundToInt((position.x - bounds.min.x)/resolution), Mathf.RoundToInt((position.y -bounds.min.y) / resolution));
        if (gridPos.x < 0) { gridPos.x = 0; }
        else if (gridPos.x >= xcount) { gridPos.x = xcount-1; }
        if (gridPos.y < 0) { gridPos.y = 0; }
        else if (gridPos.y >= ycount) { gridPos.y = ycount-1; }

        node = meshes[agent][(int) gridPos.x,(int) gridPos.y];

        if (agent.NodeIsTraversible(node)) { return node; }
        else {
            NavmeshNode2D upone = meshes[agent][(int)gridPos.x, (int)gridPos.y + 1];
            NavmeshNode2D downone = meshes[agent][(int)gridPos.x, (int)gridPos.y - 1];
            if (agent.NodeIsTraversible(upone)) { return upone; }
            else if (agent.NodeIsTraversible(downone)) { return downone; }
            else { return null; }
        }
    }

    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents*2);

        if (Application.isEditor && showDebug) {
            InitializeGrid();
        }

        DrawDebugLines();

        for (int x = 0; x < xcount && showDebug; x++)
        {
            for (int y = 0; y < ycount; y++)
            {
                if (meshes[agents[debugIndex]][x, y].type == NavmeshNode2D.NodeType.Air) { continue; }

                switch (meshes[agents[debugIndex]][x, y].type)
                {
                    case NavmeshNode2D.NodeType.None:
                        Gizmos.color = Color.red;
                        break;
                    case NavmeshNode2D.NodeType.Walkable:
                        Gizmos.color = Color.green;
                        break;
                    case NavmeshNode2D.NodeType.Crawlable:
                        Gizmos.color = Color.yellow;
                        break;
                    case NavmeshNode2D.NodeType.Ledge:
                        Gizmos.color = Color.blue;
                        break;
                    case NavmeshNode2D.NodeType.Ladder:
                        Gizmos.color = Color.magenta;
                        break;
                }
                Gizmos.DrawSphere(meshes[agents[debugIndex]][x, y].worldPosition, resolution / 4);
            }
        }
    }
}

/// <summary>
/// A type used to deliniate navigable and non-navigable points in the environment.
/// </summary>

public class NavmeshNode2D
{
    /// <summary>
    /// A type used to link two NavmeshNode2D objects.
    /// </summary>
    public class NavmeshNodeConnection2D
    {
        public NavmeshNode2D a;
        public NavmeshNode2D b;
        public bool jump;

        /// <summary>
        /// Create a connection between two NavmeshNode2D objects.
        /// </summary>
        /// <param name="a">Source node.</param>
        /// <param name="b">Destination node.</param>
        /// <param name="jump">Can the destination only be reached by jumping?</param>
        public NavmeshNodeConnection2D(NavmeshNode2D a, NavmeshNode2D b, bool jump = false)
        {
            this.a = a;
            this.b = b;
            this.jump = jump;
        }
    }

    public List<NavmeshNodeConnection2D> connections;
    public Vector3 worldPosition;
    public Vector2Int gridPosition;
    public enum NodeType { None, Walkable, Crawlable, Ladder, Air, Ledge };
    public NodeType type;
    public enum SurfaceType { Ledge, Flat, None };
    public SurfaceType surfaceType;
    public NavmeshAgent2D agent;
    public NavmeshArea2D area;
    public Ladder ladder;

    public float gcost;
    public float hcost;
    public float fcost;
    public NavmeshNode2D parent;

    public float GetWeight() {
        switch (type) {
            case NodeType.Crawlable:
                return 1;
            case NodeType.Walkable:
                return 1;
            case NodeType.Ladder:
                return 0;
            case NodeType.Ledge:
                return 2;
            case NodeType.Air:
                return 100;
            case NodeType.None:
                return Mathf.Infinity;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Create a navigable Node. DEFAULT
    /// </summary>
    public NavmeshNode2D()
    {
        agent = null;
        type = NodeType.None;
        surfaceType = SurfaceType.None;
        worldPosition = Vector3.zero;
        connections = new List<NavmeshNodeConnection2D>();
        ladder = null;
    }

    /// <summary>
    /// Create a navigable Node.
    /// </summary>
    /// <param name="agent">The corresponding agent this node will be navigable for.</param>
    /// <param name="worldPosition">The position of this node in world coordinates.</param>
    /// <param name="gridPosition">The index position in the NavmeshArea2D area grid.</param>
    public NavmeshNode2D(NavmeshAgent2D agent, NavmeshArea2D area, Vector3 worldPosition, Vector2Int gridPosition)
    {
        this.area = area;
        type = NodeType.None;
        surfaceType = SurfaceType.None;
        connections = new List<NavmeshNodeConnection2D>();
        this.agent = agent;
        this.worldPosition = worldPosition;
        this.gridPosition = gridPosition;
        ladder = null;

        Validate();
    }

    /// <summary>
    /// Create a connection between this node and a given destination node.
    /// </summary>
    /// <param name="destination">The node to connect to.</param>
    /// <param name="jump">Does the agent have to jump to reach the destination?</param>
    public void Connect(NavmeshNode2D destination, bool jump = false)
    {
        UnityEngine.Profiling.Profiler.BeginSample("Creating Connection");


        for(int i = 0; i < connections.Count; i++)
        {
            if (connections[i].b == destination) { UnityEngine.Profiling.Profiler.EndSample(); return; }
        }

        if (jump)
        {
            if ((type == NodeType.None) || (destination.type == NodeType.Air || destination.type == NodeType.None) )
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return;
            }
        }

        if (Physics2D.Linecast(worldPosition, destination.worldPosition, 1 << LayerMask.NameToLayer("Environment"))) {
            UnityEngine.Profiling.Profiler.EndSample();
            return;
        }
                
        NavmeshNodeConnection2D connection = new NavmeshNodeConnection2D(this, destination, jump);
        connections.Add(connection);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    /// <summary>
    /// Create connections from this node to a list of other nodes.
    /// </summary>
    /// <param name="nodes">The nodes to connect.</param>
    public void Connect(List<NavmeshNode2D> nodes)
    {
        foreach (NavmeshNode2D node in nodes)
        {
            Connect(node);
        }
        Update();
    }

    /// <summary>
    /// Remove ALL connections attributed to this node. Either as a source, or a destination.
    /// </summary>
    public void RemoveAllConnections()
    {
        foreach (NavmeshNodeConnection2D connection in connections)
        {
            connection.b.RemoveConnection(connection.a);
        }
        connections.Clear();
    }

    /// <summary>
    /// Remove any connection from this node to the given destination.
    /// </summary>
    /// <param name="destination">The destination to remove.</param>
    public void RemoveConnection(NavmeshNode2D destination)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].b == destination)
            {
                connections.Remove(connections[i]);
                i--;
            }
        }
    }

    public NavmeshNode2D.NavmeshNodeConnection2D GetConnection(NavmeshNode2D destination) {
        foreach (NavmeshNodeConnection2D connection in connections) {
            if (connection.b == destination) { return connection; }
        }

        return null;
    }

    /// <summary>
    /// Check if the given node is DIRECTLY connected to a node with the given type.
    /// </summary>
    /// <param name="connectionType">The type to check</param>
    /// <param name="theseConnections"></param>
    /// <returns></returns>
    public bool ConnectedToType(NodeType connectionType, out List<NavmeshNodeConnection2D> theseConnections) {
        if (ConnectedToTypes(new List<NodeType> { connectionType }, out theseConnections)) { return true; }
        return false;
    }

    /// <summary>
    /// Check if the given node is DIRECTLY connected to a node with any of the given types.
    /// </summary>
    /// <param name="types">The types to check for.</param>
    /// <param name="theseConnections">The nodes that were found to be connected and had the given types.</param>
    /// <returns>True if connected to a node with any of the given types. False if the Node is NOT connected to any nodes of the given types.</returns>
    public bool ConnectedToTypes(List<NodeType> types, out List<NavmeshNodeConnection2D> theseConnections)
    {
        theseConnections = new List<NavmeshNodeConnection2D>();

        foreach (NavmeshNodeConnection2D connection in connections)
        {
            if (types.Contains(connection.b.type) && !connection.jump) { theseConnections.Add(connection); }
        }

        if (theseConnections.Count > 0)
        {
            return true;
        }
        else { return false; }
    }

    /// <summary>
    /// Check if the given node is DIRECTLY connected to a node with any of the given types.
    /// </summary>
    /// <param name="types">The types to check for.</param>
    /// <returns>True if connected to a node with any of the given types. False if the Node is NOT connected to any nodes of the given types.</returns>
    public bool ConnectedToTypes(List<NodeType> types)
    {
        List<NavmeshNodeConnection2D> theseConnections = new List<NavmeshNodeConnection2D>();

        foreach (NavmeshNodeConnection2D connection in connections)
        {
            if (!connection.jump && types.Contains(connection.b.type)) { return true; }
        }

        return false;
    }

    /// <summary>
    /// Check if the node is colliding with something or not.
    /// </summary>
    public void Validate()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Node Validation");
        //if the world position of this node is inside a piece of the environment,
        Collider2D[] assets = Physics2D.OverlapCircleAll(worldPosition, 0, area.layerMask);
        Collider2D asset = null;

        foreach (Collider2D selection in assets) {
            string layerName = LayerMask.LayerToName(selection.gameObject.layer);
            if (layerName == "Environment")
            {
                asset = selection;
                break;
            }
            else if (layerName == "Ladder") { asset = selection;  }
        }

        if (asset)
        {
            //check if that piece of environment is a Ladder.
            Ladder ladder = asset.GetComponent<Ladder>();
            if (ladder)
            {
                //if so, the NodeType is Ladder. Store the ladder object for later.
                type = NodeType.Ladder;
                this.ladder = ladder;
            }
            else
            {
                //if no, the NodeType is none because it is colliding with a solid object.
                type = NodeType.None;
            }
        }
        //if it is not inside a piece of the environment,
        else {
            type = NodeType.Air;
            DetectLedge();
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    /// <summary>
    /// Changes the NodeType based on the current validation and proximity to other node types.
    /// </summary>
    public void DetectSurface()
    {
        NavmeshNode2D.NodeType oldType = type;

        UnityEngine.Profiling.Profiler.BeginSample("Surface Detection");
        if (type == NodeType.Air || type == NodeType.Ladder)
        {
            NavmeshNode2D surface = area.CastForType(gridPosition, Vector2.down, new List<NodeType> { NodeType.None}, agent, 1);

            if (surface != null) {

                //Check if there is a cieling, and 
                NavmeshNode2D ceiling = area.CastForType(gridPosition, Vector2.up, new List<NodeType> { NodeType.None }, agent, (int) (agent.GetSize().y*area.resolution));
                if (ceiling != null)
                {
                    //check if the distance between that ceiling is less than the agent height. if so,
                    float clearence = Vector2.Distance(worldPosition, ceiling.worldPosition) - area.resolution;
                    if (clearence <= agent.GetSize().y)
                    {
                        //check if it is less than or equal to the agent's crouch height. if that is true, the NodeType is NodeType.None. 
                        if (clearence <= agent.crouchHeight) { type = oldType; }
                        //If not, the NodeType is NodeType.Crawlable.
                        else { type = NodeType.Crawlable; }
                    }
                    //if the distance is greater than agent height, the NodeType is NodeType.Walkable.
                    else { type = NodeType.Walkable; }
                }
                //if there is no ceiling, the NodeType is Walkable
                else { type = NodeType.Walkable; }
            }
        }

        if (type == NodeType.Walkable || type == NodeType.Crawlable ) {
            List<NavmeshNodeConnection2D> connections = new List<NavmeshNodeConnection2D>();
            ConnectedToTypes(new List<NodeType> { NodeType.Air, NodeType.Ladder}, out connections);

            foreach (NavmeshNodeConnection2D connection in connections) {
                connection.b.DetectLedge();
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    void DetectLedge() {
        UnityEngine.Profiling.Profiler.BeginSample("Ledge Detection");
        if (type != NodeType.Air && type != NodeType.Ladder) { UnityEngine.Profiling.Profiler.EndSample(); return; }

        NavmeshNode2D.NodeType oldType = type;

        List<NavmeshNodeConnection2D> connections = new List<NavmeshNodeConnection2D>();

        if (ConnectedToTypes(new List<NodeType> { NodeType.Walkable, NodeType.Crawlable }, out connections)) {
            if (connections.Count == 1)
            {
                if (connections[0].b.gridPosition.y == gridPosition.y)
                {
                    if (area.CastForType(gridPosition, Vector2.down, new List<NodeType> { NodeType.Crawlable, NodeType.Walkable, NodeType.None }, agent, 1) == null)
                    {
                        type = NodeType.Ledge;
                        surfaceType = SurfaceType.Ledge;
                        UnityEngine.Profiling.Profiler.EndSample();
                        LedgeJumps();
                        return;                        
                    }
                }
            }
        }

        type = oldType;
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void LedgeJumps() {
        List<NavmeshNode2D> jumps = new List<NavmeshNode2D>();
        jumps = area.NodesOfTypeInRange(agent, worldPosition, new List<NodeType> { NodeType.Crawlable, NodeType.Walkable, NodeType.Ledge }, agent.jumpDistance);
        if (jumps.Count == 0) { return; }

        foreach (NavmeshNode2D jump in jumps) {
            if (jump == this) { continue; }
            else { Connect(jump, true); jump.Connect(this, true); }

            Debug.DrawLine(worldPosition, jump.worldPosition, Color.blue);
        }

    }

    /// <summary>
    /// Update the validity and node type based on the environment surrounding the node.
    /// </summary>
    public void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Updating Node");
        Validate();

        
        DetectSurface();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public static NodePointer GetPointer(NavmeshNode2D node) {
        NodePointer pointer;
        if (node == null) { pointer.node = new NavmeshNode2D(); }
        else { pointer.node = node; }

        return pointer;
    }
}

public struct NodePointer {
    public NavmeshNode2D node;
}


