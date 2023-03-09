using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolComponent : MonoBehaviour
{
    public GameObject PatrolLeader;
    private MovementComponent patrolLeaderMove;
    public GameObject PatrolFollower;
    public List<UnitBase> PatrolFollowerList = new List<UnitBase>();
    public ushort amountOfFollowers;
    
    public bool isCircular;
    public bool startReverse = false;
    public List<Transform> waypoints = new List<Transform>();
    public int waypointID;

    // Start is called before the first frame update
    void Start()
    {
        GameObject instancePatrolLeader = Instantiate(PatrolLeader, transform.position, transform.rotation);
        patrolLeaderMove = instancePatrolLeader.GetComponent<MovementComponent>();
        patrolLeaderMove.OnReachedDestinationEvent += OnReachedDestination;

        for (int i = 0; i < amountOfFollowers; i++)
        {
            GameObject instanceFollower = Instantiate(PatrolFollower, transform.position, transform.rotation);
            UnitBase followerUnit = instanceFollower.GetComponent<UnitBase>();
            PatrolFollowerList.Add(followerUnit);
        }
        StartCoroutine(DelayedFollow());

        patrolLeaderMove.Move(waypoints[waypointID].position);
    }

    IEnumerator DelayedFollow()
    {
        yield return new WaitForFixedUpdate();
        if (amountOfFollowers != 0)
        {
            List<ulong> patrolFollowers = new List<ulong>();
            foreach (var f in PatrolFollowerList)
            {
                patrolFollowers.Add(f.id);
                f.GetComponent<MovementComponent>().SetMovementType(MovementComponent.MovementState.Moving);
            }
            MovementComponent.FollowUnit(patrolFollowers, patrolLeaderMove.GetComponent<UnitBase>().id); 
        }

        patrolLeaderMove.SetMovementType(MovementComponent.MovementState.Moving);
    }

    void OnReachedDestination()
    {
        if (isCircular)
        {
            if (startReverse)
            {
                if (waypointID == waypoints.Count - 1)
                {
                    waypointID = 0;
                }
                waypointID++;
            }
            else
            {
                if (waypointID == 0)
                {
                    waypointID = waypoints.Count - 1;
                }
                waypointID--;
            }
        }
        else
        {
            if (startReverse)
            {
                waypointID--;
            }
            else
            {
                waypointID++;
            }

            if (waypointID == waypoints.Count - 1 || waypointID == 0)
            {
                startReverse = !startReverse;
            }
        }

        waypointID = Mathf.Clamp(waypointID, 0, waypoints.Count - 1);

        patrolLeaderMove.Move(waypoints[waypointID].position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
