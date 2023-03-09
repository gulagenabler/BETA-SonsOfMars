using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementComponent : MonoBehaviour
{
    public enum MovementState : byte {
        Idle,
        Moving,
        Running
    }

    public enum FormationType : byte {
        NoFormation = 0,
        LineFormation = 1
    }

    public MovementState movementState;
    private MovementState prevMovementState;
    protected Transform target;
    protected Vector3 lastPos;
    public int formationID = -1;
    public delegate void OnReachedDestinationDelegate();
    public OnReachedDestinationDelegate OnReachedDestinationEvent;
    public delegate void OnStateChangeDelegate(MovementState state);
    public OnStateChangeDelegate OnStateChangeEvent;
    public delegate void OnRecalculationEndDelegate();
    public OnReachedDestinationDelegate OnRecalculationEndEvent;
    public Vector3 velocity { get; private set; }
    public NavMeshAgent agent;
    PlayerStatsComponent playerStats;
    //private Path path;
    private bool startCoolDown = false;

    private float coolDown = 0;

    public delegate void OnTargetChangedDelegate();
    public OnTargetChangedDelegate OnTargetChangedEvent;
    // We will need to reintroduce spatial hash to check target more efficiently.
    protected Vector3 lastTargetPos;
    public float radius = 0.1f;
    //public float endReachedDistance = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        InitNavigation();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateMovementState();
    }

    private void UpdateMovement()
    {
        if (target)
        {
            if ((lastTargetPos - target.position).sqrMagnitude != 0)
            {
                CalculatePath();
                OnTargetChangedEvent += OnTargetChanged;
            }

            if (startCoolDown)
            {
                if (coolDown < 5)
                {
                    coolDown += Time.fixedDeltaTime;
                }
                else
                {
                    coolDown = 0;
                    startCoolDown = false;
                    target.transform.position = transform.position;
                }
            }
            lastTargetPos = target.position;
        }
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void OnTargetChanged()
    {
        OnTargetChangedEvent -= OnTargetChanged;
    }

    public void InitNavigation()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
        if (!agent)
            agent = gameObject.AddComponent<NavMeshAgent>();
        playerStats = GetComponent<PlayerStatsComponent>();
        agent.radius = 0.23f;
        if (playerStats)
            agent.speed = playerStats.runSpeed;
    }

    void SetSpeed(float speed)
    {
        if (agent)
        {
            agent.speed = speed;
        }
    }

    void CalculatePath()
    {
        // no target then no calculation
        if (target == null) return;

        //if (path == null) {
        //    path = new ABPath();
        //}

        // Will rework this later

        if (agent)
        {
            agent.SetDestination(target.position);
        }

        if (PathfinderHandler.instance)
        {
            //PathfinderHandler.instance.RequestPathfinding(transform.position, target.position, this);
        }

        //if (path.vectorPath.Count < 3) {
        //    if (PathfinderHandler.instance.BresenhamCheckCollision(transform.position, target.position))
        //    {
        //        seeker.pathCallback += OnPathComplete;
        //        Path p = seeker.StartPath(transform.position, target.position, OnPathComplete);
        //    }
        //    else
        //    {
        //        path = new ABPath();// seeker.GetCurrentPath();
        //        path.vectorPath = new List<Vector3>() { target.position };
        //    }
        //} else {
        //    if (MapGenerator.instance.BresenhamCheckCollision(transform.position, path.vectorPath[0]))
        //    {
        //        Path p = seeker.StartPath(transform.position, path.vectorPath[0]);
        //        path.vectorPath.InsertRange(1, p.vectorPath);
        //    } 
        //    if (MapGenerator.instance.BresenhamCheckCollision(path.vectorPath[path.vectorPath.Count - 2], path.vectorPath[path.vectorPath.Count - 1]))
        //    {
        //        Path p = seeker.StartPath(path.vectorPath[path.vectorPath.Count - 2], path.vectorPath[path.vectorPath.Count - 1]);
        //        path.vectorPath.InsertRange(path.vectorPath.Count - 2, p.vectorPath);
        //    }
        //}
    }

    public void OnStateChange(MovementState state)
    {
        switch (state)
        {
            case MovementState.Idle:
                if (target)
                {
                    Vector3 distToTarget = target.position - transform.position;
                    if (distToTarget.sqrMagnitude != 0)
                    {
                        startCoolDown = true;
                    }
                }
                break;
            case MovementState.Moving:
                startCoolDown = false;
                coolDown = 0;
                break;
        }

        if (ReachedDestinationOrGaveUp())
        {
            OnReachedDestinationEvent?.Invoke();
        }
    }

    public void TurnOffAvoidance() {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
        if (!agent)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.avoidancePriority = 100;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    private void OnEnable() {
        OnStateChangeEvent += OnStateChange;
        OnReachedDestinationEvent += OnReachedDestination;
        OnTargetChangedEvent += OnTargetChanged;
    }

    public void OnReachedDestination()
    {
        if (target != null) {
            transform.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
        }
        OnReachedDestinationEvent -= OnReachedDestination;
    }

    private void OnDestroy()
    {
        DeleteTarget();
        OnReachedDestinationEvent -= OnReachedDestination;
        OnStateChangeEvent -= OnStateChange;
        OnTargetChangedEvent -= OnTargetChanged;
    }

    protected void UpdateMovementState() {
        Vector3 deltaPos = (lastPos - transform.position);
        float deltaSqrMagnitude = agent.velocity.sqrMagnitude;
        switch (movementState) {
            case MovementState.Idle:
            case MovementState.Moving:
            case MovementState.Running:
                if (deltaSqrMagnitude != 0.0f) {
                    if (playerStats) {
                        if (deltaSqrMagnitude > (playerStats.walkSpeed * playerStats.walkSpeed) + 0.25f) {
                            ChangeMovementState(MovementState.Running);
                        } else {
                            ChangeMovementState(MovementState.Moving);
                        }
                    } else {
                        ChangeMovementState(MovementState.Moving);
                    }
                } else {
                    ChangeMovementState(MovementState.Idle);
                }
                break;
        }
        velocity = deltaPos / Time.fixedDeltaTime;
        lastPos = transform.position;
    }

    public void SetMovementType(MovementState state) {
        if (!agent || !playerStats) { Debug.Log("No agent or stats?"); return; }
        switch (state)
        {
            case MovementState.Idle:
                agent.speed = 0;
                break;
            case MovementState.Moving:
                agent.speed = playerStats.walkSpeed;
                break;
            case MovementState.Running:
                agent.speed = playerStats.runSpeed;
                break;
            default:
                break;
        }
    }

    public void ChangeMovementState(MovementState newstate)
    {
        movementState = newstate;
        if (prevMovementState != movementState)
        {
            prevMovementState = movementState;
            OnStateChangeEvent?.Invoke(movementState);
        }
    }

    public void Move(Vector3 position) {
        if (target == null)
        {
            GameObject gt = new GameObject();
            gt.name = "Point: " + gt.GetInstanceID();
            if (gameObject.TryGetComponent(out UnitBase unit))
            {
                gt.name += " " + unit.id;
            }
            target = gt.transform;
        }
        target.position = position;
        Vector3 dir = (target.position - transform.position).normalized;
        if (dir == Vector3.zero)
            dir = transform.forward;
        target.eulerAngles = new Vector3(0, Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y, 0);
        CalculatePath();
        OnReachedDestinationEvent += OnReachedDestination;
    }

    private void OnDisable()
    {
        OnStateChangeEvent += OnStateChange;
        OnReachedDestinationEvent += OnReachedDestination;
        OnTargetChangedEvent += OnTargetChanged;
    }

    public Transform GetMoveTarget() {
        return target;
    }

    public void DeleteTarget()
    {
        if (target)
        {
            ResetTarget();
            Destroy(target.gameObject);
        }
    }

    public virtual void ResetTarget()
    {
        // First we check if the target is valid
        if (target) {
            // No matter what we do must make the parent null
            target.SetParent(null, true);
        }
    }

    public static void NoFormation(List<ulong> unitIds, Vector3 position)
    {
        int count = unitIds.Count;
        for (ulong i = 0; i < System.Convert.ToUInt64(count); i++)
        {
            UnitBase unit = UnitBase.units[unitIds[System.Convert.ToInt32(i)]];
            unit.OnClearTaskEvent?.Invoke();

            MovementComponent movementComponent = unit.GetComponent<MovementComponent>();
            if (movementComponent)
            {
                movementComponent.formationID = 0;
                movementComponent.ResetTarget();
                movementComponent.Move(position);
            }
        }
    }

    public static void LineFormation(List<ulong> unitIds, Vector3 position)
    {
        FormationParentComponent fp = null;
        LineFormation(unitIds, position, out fp);
    }


    // Refactor time again
    // We can do several changes once again
    // 1. First we should make functions to add and delete components at will. A possible TargetComponent?
    // 2. It will pathfind itself to FormationParentComponent's child member
    // 3. A new formation manager
    // 4. Formation ID = 0 or -1 will always be used for units that doesn't follow any formation rules
    //     or maybe do we even need it?
    // 5. Formation parent will hold value of its child and make them not collide with each other.
    // 6. If formation is updated then we can simply go by unit's assigned formation id saved within the unit itself. 
    //    From formation that specific unit will remove all related collision
    // 7. We can reuse formation parent game object instead of straight up deleting if we want. Wonder how to use that.....
    // 8. For adding formation we will send allocation request. Function may need a return value.
    // 9. We may need a check to see if we are re-updating the same formation parent for same amount unit with similar value. Encoded string or hashcoding?
    //    depending on the result we will just do simple target change.
    // 10. We can make use of OnTransformChildrenChanged for destruction trigger of formation parent. Updating the children itself too?
    public static void LineFormation(List<ulong> unitIds, Vector3 position, out FormationParentComponent fpComp)
    {
        
        // Get unit counts
        int count = unitIds.Count;

        // Initialize the avg position
        Vector3 avgPos = Vector3.zero;

        // Create the parent formation
        GameObject fp = new("Parent formation");
        fp.name += fp.GetInstanceID();

        // Register the parent formation
        // Register will first check if its the same formation
        // If it is then no new parent creation and rather return the existing one
        // If there are no parent then make one
        // If it belongs to a parent already but a new movement command called then it'll remove links to old parent and make new one

        float lowestSpeed = Mathf.Infinity;

        // Loop over all units to set the line formation
        for (ulong i = 0; i < System.Convert.ToUInt64(count); i++)
        {
            UnitBase unit = UnitBase.units[unitIds[System.Convert.ToInt32(i)]];
            
            // We must ensure to clear all tasks before hand.
            if (unit.OnClearTaskEvent != null) {
                unit.OnClearTaskEvent();
            }

            // Initialize the movement component
            MovementComponent movementComponent = unit.GetComponent<MovementComponent>();
            if (movementComponent.agent.speed < lowestSpeed)
            {
                lowestSpeed = movementComponent.agent.speed;
            }

            // Setting the position
            //float _x = ((int)i % N);
            //float _z = ((int)i / N);

            // its natural to have movment component
            if (movementComponent)
            {
                // We must reset the target to ensure that target got no parent
                movementComponent.ResetTarget();
                movementComponent.Move(position);
                Transform target = movementComponent.GetMoveTarget();
                target.localRotation = Quaternion.identity;
                target.localScale = new Vector3(1, 1, 1);

                // Then we set a new parent that we just created
                target.SetParent(fp.transform);
            }
            avgPos += unit.transform.position;
        }

        // Get the center point of units so that we can set up formation point
        avgPos = avgPos / count;

        fpComp = null;

        // Now we set the parent
        if (fp)
        {
            fp.transform.position = avgPos;
            fpComp = fp.AddComponent<FormationParentComponent>();
            MovementComponent formationMovement = AddMovementComponent(fp, true);
            formationMovement.TurnOffAvoidance();
            Destroy(formationMovement.GetComponent<CharacterController>());
            formationMovement.Move(position);
            formationMovement.SetSpeed(lowestSpeed * 0.95f);
            Transform formationMovementTarget = formationMovement.GetMoveTarget();
            fp.transform.position = position;
            fp.transform.eulerAngles = new Vector3(0, Quaternion.LookRotation((formationMovementTarget.position - avgPos).normalized, Vector3.up).eulerAngles.y, 0);
            fp.transform.localScale = new Vector3(1f, 1, 1f);
            fpComp.UpdateFormation(FormationType.LineFormation, avgPos);
        }
    }
    
    public static MovementComponent AddMovementComponent(GameObject g, bool isPathfindingOn, float radius = 0.1f)
    {
        MovementComponent m = g.AddComponent<MovementComponent>();
        m.radius = radius;
        //CharacterController ch_ctrl = g.AddComponent<CharacterController>();
        //AddLocalAvoidance(m);
        return m;
    }

    public static void HandleMovement(List<ulong> unitIds, Vector3 position, FormationType formationType) {
        if (unitIds.Count == 1) { NoFormation(unitIds, position); return; }
        switch (formationType)
        {
            case FormationType.NoFormation:
                NoFormation(unitIds, position);
                break;
            case FormationType.LineFormation:
                LineFormation(unitIds, position);
                break;
            default:
                break;
        }
    }

    public static void FollowUnit(List<ulong> unitIds, ulong targetId)
    {
        if (unitIds.Contains(targetId)) return;
        int count = unitIds.Count;
        UnitBase targetUnit = UnitBase.units[targetId];
        MovementComponent.LineFormation(unitIds, targetUnit.transform.position, out FormationParentComponent fpComp);

        if (fpComp != null)
        {
            MovementComponent fpMovement = fpComp.GetComponent<MovementComponent>();
            Transform fpMovementTarget = fpMovement.GetMoveTarget();
            fpMovementTarget.SetParent(targetUnit.transform, true);
            Vector3 localPos = new Vector3(0, 0, -fpComp.size.x);
            if (unitIds.Count <= 7) localPos = new Vector3(0, 0, -1);
            fpMovementTarget.localPosition = localPos;
            fpMovementTarget.localEulerAngles = Vector3.zero;
            fpMovement.TurnOffAvoidance();
            fpComp.UpdateFormation(FormationType.LineFormation, fpComp.transform.position);
        }
    }

    public static void FollowUnit(Dictionary<int, UnitBase> units, ulong targetId)
    {
        List<ulong> ids = new List<ulong>();
        foreach (KeyValuePair<int, UnitBase> u in units)
        {
            ids.Add(u.Value.id);
        }

        FollowUnit(ids, targetId);
    }
}
