using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject troopsToSpawn;
    bool Spawned = false;
    public List<Transform> spawnPoints = new List<Transform>();
    List<UnitBase> followingUnits = new List<UnitBase>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool returnOrdered = false;
    // Update is called once per frame
    void Update()
    {
        if (returnOrdered)
        {
            if (followingUnits.Count == 0)
            {
                returnOrdered = false;
                Spawned = false;
            }
        }

        if (Spawned)
        {
            if (Input.GetKeyDown(KeyCode.G) && returnOrdered == false)
            {
                List<ulong> followingIDs = new List<ulong>();
                foreach (UnitBase u in followingUnits)
                {
                    followingIDs.Add(u.id);
                }

                Transform playerTransform = PlayerController.mainPlayer.transform;
                MovementComponent.HandleMovement(followingIDs, playerTransform.position + playerTransform.forward * -5f, MovementComponent.FormationType.LineFormation);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                returnOrdered = true;
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    Transform point = spawnPoints[i];
                    ulong id = followingUnits[i].id;
                    UnitBase u = UnitBase.units[id];
                    if (u != null)
                    {
                        MovementComponent.HandleMovement(new List<ulong>() { id }, spawnPoints[i].position, MovementComponent.FormationType.NoFormation);
                        if (u.TryGetComponent(out MovementComponent movementComponent))
                        {
                            movementComponent.OnReachedDestinationEvent += () =>
                            {
                                followingUnits.Remove(u);
                                Destroy(u);
                            };
                        }
                    }
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.L) && returnOrdered == false)
            {
                if (troopsToSpawn)
                {
                    Spawned = true;
                    for (int i = 0; i < spawnPoints.Count; i++)
                    {
                        GameObject g = GameObject.Instantiate(troopsToSpawn);
                        Vector3 position = spawnPoints[i].position;
                        Quaternion rotation = spawnPoints[i].rotation;
                        g.transform.SetPositionAndRotation(position, rotation);
                        UnitBase u = g.GetComponent<UnitBase>();
                        followingUnits.Add(u);
                    }

                    System.Action action = () =>
                    {
                        List<ulong> followingIDs = new List<ulong>();
                        foreach (var u in followingUnits)
                        {
                            followingIDs.Add(u.id);
                        }

                        if (PlayerController.mainPlayer)
                        {
                            ulong pid = PlayerController.mainPlayer.GetComponent<UnitBase>().id;
                            MovementComponent.FollowUnit(followingIDs, pid);
                        }
                    };

                    StartCoroutine(DelayedMove(action));
                }
            }
        }
    }

    IEnumerator DelayedMove(System.Action action)
    {
        yield return new WaitForFixedUpdate();
        action.Invoke();
    }
}
