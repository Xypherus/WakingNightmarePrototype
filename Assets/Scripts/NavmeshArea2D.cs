using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

public class NavmeshArea2D : MonoBehaviour {

    public float resolution;
    public LayerMask layerMask;
    public List<NavmeshAgent2D> agents;
    public Dictionary<NavmeshAgent2D, NavmeshNode2D[,]> meshes;
    public Bounds bounds;

    private int xcount;
    private int ycount;

    new Collider2D collider;

    float updateRate = 0.2f;
    float lastUpdate;

	// Use this for initialization
	void Start () {
        collider = GetComponent<CompositeCollider2D>();
        InitializeGrid();
	}

    public Vector2 WorldToGridPosition(Vector2 worldPosition) {
        Vector2 gridPosition = new Vector2(Mathf.RoundToInt((worldPosition.x - bounds.min.x) / resolution), Mathf.CeilToInt((worldPosition.y - bounds.min.y) / resolution));
        return gridPosition;
    }
	
	// Update is called once per frame
	void Update () {
        if (resolution <= 0) { resolution = 0.1f; }
    }

    void DrawDebugLines() {
        for(int y = 0; y < ycount; y++)
        {
            for(int x = 0; x < xcount; x++)
            {
                foreach (NavmeshNode2D.NavmeshNodeConnection2D connection in meshes[agents[0]][x,y].connections)
                {
                    if (connection.a.type == NavmeshNode2D.NodeType.None || connection.jump) { continue; }
                    Color lineColor = Color.white;
                    switch (meshes[agents[0]][x,y].type)
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

                    Debug.DrawLine(connection.a.position, connection.b.position, lineColor);
                }
            }
        }
    }

    public void UpdateGrid(Collider2D testCollider) {
        if (!(Time.time >= lastUpdate + updateRate)) { return; }
        foreach (KeyValuePair<NavmeshAgent2D, NavmeshNode2D[,]> mesh in meshes) {
            UpdateGrid(testCollider, mesh.Key);
        }
    }

    public void UpdateGrid(Collider2D testCollider, NavmeshAgent2D agent) {
        Vector2 min = WorldToGridPosition(testCollider.bounds.min);
        Vector2 max = WorldToGridPosition(testCollider.bounds.max);
        int xstart = (int)min.x;
        int ystart = (int)min.y;
        int xlimit = (int)max.x;
        int ylimit = (int)max.y;

        int width = xlimit - xstart;
        int height = ylimit - ystart;

        for (int x = xstart - (width); x < (xlimit + (width)) && x < xcount-1; x++) {
            if (x < 0) { x = 0; }
            
            for (int y = ystart - (height); y < (ylimit+(height)) && y < ycount-1; y++) {
                if (y < 0) { y = 0; }
                int radius;

                if (agent.jumpDistance <= resolution) { radius = 1; }
                else { radius = Mathf.RoundToInt(agent.jumpDistance / resolution); }

                for (int newx = x - radius; newx < x + radius; newx++) {
                    if (newx > xcount-1) { break; }
                    else if (newx < 0 ) { continue; }
                    for (int newy = y - radius; newy < y + radius; newy++) {
                        if (newy > ycount-1 || newy < 0) { continue; }
                        else if (newx == x && newy == y) { continue; }

                        if (Mathf.Abs(x- newx) <= 1 && Mathf.Abs(y-newy) <= 1)
                        {
                            meshes[agent][x, y].Connect(meshes[agent][newx, newy]);
                        }
                        else if (Vector2.Distance(meshes[agent][newx, newy].position, meshes[agent][x, y].position) <= agent.jumpDistance)
                        {
                            meshes[agent][x, y].Connect(meshes[agent][newx, newy], true);
                        }
                    }
                }
                meshes[agent][x, y].Update();
            }
        }
        lastUpdate = Time.time;
    }

    public void InitializeGrid() {
        xcount = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / resolution) + 1;
        ycount = Mathf.CeilToInt((bounds.max.y - bounds.min.y) / resolution) + 1;

        meshes = new Dictionary<NavmeshAgent2D, NavmeshNode2D[,]>();
        foreach (NavmeshAgent2D agent in agents) {
            meshes.Add(agent, new NavmeshNode2D[xcount,ycount]);
            for (int x = 0; x < xcount; x++) {
                for (int y = 0; y < ycount; y++) {
                    meshes[agent][x, y] = new NavmeshNode2D(agent,(Vector2) bounds.min + (new Vector2(x * resolution,y * resolution)));
                    meshes[agent][x, y].Update();
                }
            }
            InitializeGrid(agent);
        }
    }

    public void InitializeGrid(NavmeshAgent2D agent) {

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
                        else if (Vector2.Distance(meshes[agent][newx, newy].position, meshes[agent][x, y].position) <= agent.jumpDistance)
                        {
                            meshes[agent][x, y].Connect(meshes[agent][newx, newy], true);
                        }
                    }
                }
                meshes[agent][x, y].Update();
            }
        }
    }

    public NavmeshNode2D NodeAtPoint(Vector2 position, NavmeshAgent2D agent) {
        Vector2 gridPos = new Vector2(Mathf.RoundToInt((position.x - bounds.min.x)/resolution), Mathf.RoundToInt((position.y -bounds.min.y) / resolution));
        if (gridPos.x < 0) { gridPos.x = 0; }
        else if (gridPos.x >= xcount) { gridPos.x = xcount-1; }
        if (gridPos.y < 0) { gridPos.y = 0; }
        else if (gridPos.y >= ycount) { gridPos.y = ycount-1; }
        return meshes[agent][(int) gridPos.x,(int) gridPos.y];
    }

    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents*2);
        if (Application.isEditor) {
            InitializeGrid();
        }
        for (int x = 0; x < xcount; x++) {
            for (int y = 0; y < ycount; y++) {
                switch (meshes[agents[0]][x, y].surfaceType) {
                    case NavmeshNode2D.SurfaceType.None:
                        Gizmos.color = Color.blue;
                        break;
                    case NavmeshNode2D.SurfaceType.Flat:
                        Gizmos.color = Color.green;
                        break;
                    case NavmeshNode2D.SurfaceType.Ledge:
                        Gizmos.color = Color.yellow;
                        break;
                }
                Gizmos.DrawSphere(meshes[agents[0]][x, y].position, resolution / 4);
            }
        }

        DrawDebugLines();
        
    }
}

public class NavmeshNode2D
{
    public class NavmeshNodeConnection2D
    {
        public NavmeshNode2D a;
        public NavmeshNode2D b;
        public bool jump;

        public NavmeshNodeConnection2D(NavmeshNode2D a, NavmeshNode2D b, bool jump = false)
        {
            this.a = a;
            this.b = b;
            this.jump = jump;
        }
    }

    public List<NavmeshNodeConnection2D> connections;
    public Vector3 position;
    public enum NodeType { None, Walkable, Crawlable, Ladder, Air, Ledge };
    public NodeType type;
    public enum SurfaceType { Ledge, Flat, None };
    public SurfaceType surfaceType;
    public NavmeshAgent2D agent;
    public Ladder ladder;

    public float gcost;
    public float fcost;

    NavmeshArea2D area;

    public NavmeshNode2D()
    {
        area = GameObject.FindObjectOfType<NavmeshArea2D>();
        agent = null;
        type = NodeType.None;
        surfaceType = SurfaceType.None;
        position = Vector3.zero;
        connections = new List<NavmeshNodeConnection2D>();
        ladder = null;
        Update();
    }

    public NavmeshNode2D(NavmeshAgent2D agent, Vector2 position)
    {
        type = NodeType.None;
        surfaceType = SurfaceType.None;
        area = GameObject.FindObjectOfType<NavmeshArea2D>();
        connections = new List<NavmeshNodeConnection2D>();
        this.agent = agent;
        this.position = position;
        ladder = null;
        Update();
    }

    public void Connect(NavmeshNode2D destination, bool jump = false)
    {
        foreach (NavmeshNodeConnection2D duplicate in connections)
        {
            if (duplicate.b == destination) { return; }
        }

        if (jump)
        {
            if (destination.type == NodeType.Air )
            {
                return;
            }
        }

        NavmeshNodeConnection2D connection = new NavmeshNodeConnection2D(this, destination, jump);
        connections.Add(connection);
    }

    public void Connect(List<NavmeshNode2D> nodes)
    {
        foreach (NavmeshNode2D node in nodes)
        {
            Connect(node);
        }
        Update();
    }

    public void RemoveAllConnections()
    {
        foreach (NavmeshNodeConnection2D connection in connections)
        {
            connection.b.RemoveConnection(connection.a);
        }
        connections.Clear();
    }

    public void RemoveConnection(NavmeshNode2D destination)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].b.position == destination.position)
            {
                connections.Remove(connections[i]);
                i--;
            }
        }
    }

    public bool ConnectedToType(NodeType connectionType, out NavmeshNodeConnection2D connectedNode) {
        foreach (NavmeshNodeConnection2D connection in connections) {
            if (connection.b.type == connectionType && !connection.jump) { connectedNode = connection; return true; }
        }
        connectedNode = null;
        return false;
    }

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

    public void Validate()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Node Validation");
        Collider2D asset = Physics2D.OverlapCircle(position, 0, area.layerMask);
        if (asset)
        {
            Ladder isLadder = asset.GetComponent<Ladder>();
            if (isLadder)
            {
                type = NodeType.Ladder;
                ladder = isLadder;
            }
            else
            {
                type = NodeType.None;
                //RemoveAllConnections();
            }
        }
        else { type = NodeType.Air; }

        UnityEngine.Profiling.Profiler.EndSample();

        DetectSurface();
    }

    void DetectSurface()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Surface Detection");
        /* Legacy
        RaycastHit2D surface = Physics2D.Raycast(position, Vector2.down, area.resolution, area.layerMask);
        NodeType type = NodeType.None;
        if (surface && agent)
        {
            if (!surface.collider.GetComponent<Ladder>())
            {
                type = NodeType.Walkable;
                RaycastHit2D ceiling = Physics2D.Raycast(surface.point, surface.normal, agent.height, area.layerMask);
                if (ceiling)
                {
                    if (!ceiling.collider.GetComponent<Ladder>())
                    {
                        Thread t = new Thread(() =>
                        {
                            float clearence = Vector2.Distance(ceiling.point, surface.point);
                            if (clearence == 0f)
                            {
                                type = type;
                                if (type == NodeType.None)
                                {
                                    RemoveAllConnections();
                                }
                            }
                            else if (clearence < agent.crouchHeight && clearence > 0f)
                            {
                                type = NodeType.None;
                                RemoveAllConnections();
                            }
                            else if (clearence < agent.height) { type = NodeType.Crawlable; }
                        });
                        t.Start();
                    }
                }
            }
        }
        else if (!surface) { type = NodeType.Air; }
        this.type = type;
        */
        if (type != NodeType.None && !ladder)
        {
            foreach (NavmeshNodeConnection2D connection in connections)
            {
                if (connection.b.type != NodeType.None) { continue; }
                if (connection.b.position.y < position.y)
                {
                    RaycastHit2D cieling = Physics2D.Raycast(position, Vector2.up, agent.height, area.layerMask);

                    if (cieling)
                    {
                        float height = Vector2.Distance(cieling.point, position);

                        if (height > agent.crouchHeight) { type = NodeType.Crawlable; surfaceType = SurfaceType.Flat; }
                        else if (height < agent.crouchHeight) { type = NodeType.None; }
                        else { type = NodeType.Walkable; surfaceType = SurfaceType.Flat; }
                    }
                    else { type = NodeType.Walkable; }
                }
                else
                {
                    type = NodeType.Air;

                }
            }
            
            foreach (NavmeshNodeConnection2D connection in connections)
            {
                if (connection.b.type != NodeType.Walkable && connection.b.type != NodeType.Crawlable) { continue; }
                if (position.x != connection.b.position.y) {
                    type = NodeType.Ledge; surfaceType = SurfaceType.Ledge;
                }
            }
        }
        else { type = NodeType.None; }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void Update()
    {
        Validate();
    }

    void TestNode()
    {
        if (Physics2D.Raycast(position, Vector2.down, area.resolution, area.layerMask) && surfaceType != SurfaceType.Flat &&
            !Physics2D.OverlapCircle(position, 0f, area.layerMask))
        {
            Debug.LogError("this node is broken");
            Debug.DrawRay(position, Vector2.up * 0.4f, Color.red);
        }
    }
}

