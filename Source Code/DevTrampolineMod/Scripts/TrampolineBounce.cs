using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineBounce : MonoBehaviour
{
    void Start()
    {
        //gameObject.layer = 9;
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.layer == 9)
        {
            if (other.gameObject.name == "Body Collider")
            {
                DevTrampolineMod.Plugin.findThisTrampoline = gameObject.transform.parent.parent.name;
                DevTrampolineMod.Plugin.Bounce(8f);
            }
        }
        if (gameObject.layer == 18)
        {
            if (other.gameObject.name == "LeftHandTriggerCollider" || other.gameObject.name == "RightHandTriggerCollider")
            {
                DevTrampolineMod.Plugin.findThisTrampoline = gameObject.transform.parent.parent.name;
                DevTrampolineMod.Plugin.Bounce(6f);
            }
        }
    }
}
