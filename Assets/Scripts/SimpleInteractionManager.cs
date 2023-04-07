using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleInteractionManager : MonoBehaviour
{
    [SerializeField] SimpleAICharacter LinkedCharacter;
    [SerializeField] LayerMask RaycastMask = ~0;
    [SerializeField] float RaycastRange = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) 
        { 
            // convert the mouse location to a ray
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // perform the raycast
            RaycastHit hitInfo;
            if (Physics.Raycast(cameraRay, out hitInfo, RaycastRange, RaycastMask, QueryTriggerInteraction.Ignore)) 
            { 
                LinkedCharacter.MoveTo(hitInfo.point);
            }
        }
    }
}
