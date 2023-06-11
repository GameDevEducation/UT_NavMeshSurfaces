using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshLink))]
public class DoorNavLink : MonoBehaviour
{
    [SerializeField] DoorController LinkedDoor;

    public class TraversalData
    {
        enum EState
        {
            MovingToDoor,
            WaitingForDoorToOpen,
            MovingThroughDoor,
            Finished
        }

        NavMeshAgent Agent;
        Action OnCompleteFn;
        DoorController Door;
        EState State = EState.MovingToDoor;

        public TraversalData(DoorController inDoor, NavMeshAgent inAgent, Action inOnCompleteFn)
        {
            Door = inDoor;
            Agent = inAgent;
            OnCompleteFn = inOnCompleteFn;

            Agent.updatePosition = false;
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
        }

        public bool Tick(float deltaTime)
        {
            if (State == EState.MovingToDoor)
            {
                Door.RequestOpen(Agent.gameObject);
                State = EState.WaitingForDoorToOpen;
            }

            if (State == EState.WaitingForDoorToOpen)
            {
                if (Door.IsOpen)
                    State = EState.MovingThroughDoor;
            }

            if (State == EState.MovingThroughDoor)
            {
                Vector3 currentPosition = Agent.transform.position;
                Vector3 endPosition = Agent.currentOffMeshLinkData.endPos;

                if (!Mathf.Approximately(Vector3.SqrMagnitude(endPosition - currentPosition), 0f))
                {
                    Vector3 newPosition = Vector3.MoveTowards(currentPosition, endPosition, Agent.speed * Time.deltaTime);
                    Agent.transform.position = newPosition;
                }
                else
                    State = EState.Finished;
            }

            if (State == EState.Finished)
            {
                Door.RequestClose(Agent.gameObject);

                Agent.CompleteOffMeshLink();

                Agent.updatePosition = true;
                Agent.updateRotation = true;
                Agent.updateUpAxis = true;

                if (OnCompleteFn != null)
                    OnCompleteFn();

                return true;
            }

            return false;
        }
    }

    List<TraversalData> ActiveTraversals = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int index = ActiveTraversals.Count - 1; index >= 0; index--) 
        { 
            var traversal = ActiveTraversals[index];

            if (traversal.Tick(Time.deltaTime))
                ActiveTraversals.RemoveAt(index);
        }
    }

    public void StartTraversal(NavMeshAgent agent, System.Action onCompleteFn)
    {
        ActiveTraversals.Add(new TraversalData(LinkedDoor, agent, onCompleteFn));
    }
}
