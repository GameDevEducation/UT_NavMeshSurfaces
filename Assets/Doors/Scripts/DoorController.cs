using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{
    public enum EDoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    [SerializeField] List<string> TagsToCheckFor;
    [SerializeField] bool FacingMatters = false;
    [SerializeField] NavMeshObstacle LinkedObstacle;
    [SerializeField] NavMeshLink DoorLink;
    [SerializeField] bool IsLocked = false;

    public bool IsOpen => CurrentState == EDoorState.Open;

    bool WasLocked;
    bool InForwardsMode = true;

    List<GameObject> HeldOpenBy = new List<GameObject>();

    EDoorState CurrentState = EDoorState.Closed;
    Animator DoorAnimController;

    // Start is called before the first frame update
    void Start()
    {
        DoorAnimController = GetComponent<Animator>();
        WasLocked = IsLocked;

        if (DoorLink != null)
            DoorLink.enabled = !IsLocked;
    }

    // Update is called once per frame
    void Update()
    {
        // determine target state
        EDoorState targetState = HeldOpenBy.Count > 0 ? EDoorState.Open : EDoorState.Closed;

        // has the lock status changed?
        if (WasLocked != IsLocked)
        {
            WasLocked = IsLocked;

            if (DoorLink != null)
                DoorLink.enabled = !IsLocked;
        }

        // do we have nothing we need to do?
        if (targetState == CurrentState)
            return;
        if (targetState == EDoorState.Open && CurrentState == EDoorState.Opening)
            return;
        if (targetState == EDoorState.Closed && CurrentState == EDoorState.Closing)
            return;

        // are we trying to open?
        if (targetState == EDoorState.Open)
        {
            CurrentState = EDoorState.Opening;

            if (FacingMatters)
            {
                // get the vector to the opener
                var vecToOpener = HeldOpenBy[0].transform.position - transform.position;
                vecToOpener.y = 0f;

                // use dot product to check which side the opener is on
                InForwardsMode = Vector3.Dot(vecToOpener, transform.forward) < 0;

                DoorAnimController.SetTrigger(InForwardsMode ? "RequestOpen_Forward" : "RequestOpen_Backward");
            }
            else
                DoorAnimController.SetTrigger("RequestOpen");            
        }
        else if (targetState == EDoorState.Closed)
        {
            CurrentState = EDoorState.Closing;

            if (LinkedObstacle != null)
                LinkedObstacle.enabled = true;

            if (FacingMatters)
                DoorAnimController.SetTrigger(InForwardsMode ? "RequestClose_Forward" : "RequestClose_Backward");
            else
                DoorAnimController.SetTrigger("RequestClose");
        }
    }

    public void OnOpeningCompleted()
    {
        CurrentState = EDoorState.Open;

        if (LinkedObstacle != null)
            LinkedObstacle.enabled = false;

        Debug.Log("Open!");
    }

    public void OnClosingCompleted()
    {
        CurrentState = EDoorState.Closed;

        Debug.Log("Close");
    }

    public void RequestOpen(GameObject requestor)
    {
        HeldOpenBy.Add(requestor);
    }
    
    public void RequestClose(GameObject requestor)
    {
        HeldOpenBy.Remove(requestor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsLocked)
            return;

        if (TagsToCheckFor.Count > 0 && !TagsToCheckFor.Contains(other.tag))
            return;
        
        RequestOpen(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (TagsToCheckFor.Count > 0 && !TagsToCheckFor.Contains(other.tag))
            return;

        RequestClose(other.gameObject);
    }
}
