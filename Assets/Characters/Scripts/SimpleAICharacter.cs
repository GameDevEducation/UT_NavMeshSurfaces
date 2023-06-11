using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAICharacter : MonoBehaviour
{
    [SerializeField] float NearestPointSearchRange = 0.5f;

    NavMeshAgent LinkedAgent;
    DoorNavLink ActiveNavLink;

    bool DestinationSet = false;
    bool OffMeshLinkInProgress = false;

    private void Awake()
    {
        LinkedAgent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!LinkedAgent.pathPending && !LinkedAgent.isOnOffMeshLink &&
            (LinkedAgent.remainingDistance <= LinkedAgent.stoppingDistance))
        {
            DestinationSet = false;
        }

        if (LinkedAgent.isOnOffMeshLink && !OffMeshLinkInProgress)
        {
            OffMeshLinkInProgress = true;

            // look for our door link
            var navMeshLink = LinkedAgent.navMeshOwner as NavMeshLink;
            if (navMeshLink != null)
            {
                if (navMeshLink.gameObject.TryGetComponent<DoorNavLink>(out ActiveNavLink))
                {
                    ActiveNavLink.StartTraversal(LinkedAgent, () =>
                    {
                        OffMeshLinkInProgress = false;
                        ActiveNavLink = null;
                    });
                    return;
                }
            }

            StartCoroutine(FollowOffMeshLink_Lerp());
        }
    }

    IEnumerator FollowOffMeshLink_Lerp()
    {
        LinkedAgent.updatePosition = false;
        LinkedAgent.updateRotation = false;
        LinkedAgent.updateUpAxis = false;

        Vector3 newPosition = transform.position;
        Vector3 endPosition = LinkedAgent.currentOffMeshLinkData.endPos;
        while (!Mathf.Approximately(Vector3.SqrMagnitude(endPosition - transform.position), 0f))
        {
            newPosition = Vector3.MoveTowards(transform.position, endPosition, LinkedAgent.speed * Time.deltaTime);
            transform.position = newPosition;

            yield return new WaitForEndOfFrame();
        }

        OffMeshLinkInProgress = false;
        LinkedAgent.CompleteOffMeshLink();

        LinkedAgent.updatePosition = true;
        LinkedAgent.updateRotation = true;
        LinkedAgent.updateUpAxis = true;
    }

    public void MoveTo(Vector3 newDestination)
    {
        NavMeshHit hitResult;

        if (OffMeshLinkInProgress)
            return;

        if (NavMesh.SamplePosition(newDestination, out hitResult, NearestPointSearchRange, NavMesh.AllAreas))
        {
            LinkedAgent.SetDestination(hitResult.position);
            DestinationSet = true;
        }
    }
}
