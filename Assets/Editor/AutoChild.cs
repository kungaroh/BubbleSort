using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoChild : EditorWindow
{
    private const float proximityThreshold = 1.3f;


    [MenuItem("Tools/Henners Menu/Assign Parent-Child Relationship")]
    private static void AutoAssignChildren()
    {
        
        GameObject[] tubes = GameObject.FindGameObjectsWithTag("Tube");

        foreach (GameObject tube in tubes)
        {
            Collider2D tubeCollider = tube.GetComponent<Collider2D>();

            if (tubeCollider != null)
            {
                Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(tube.transform.position, proximityThreshold);

                foreach (Collider2D nearbyObject in nearbyObjects)
                {
                    if (nearbyObject.gameObject != tube && !nearbyObject.CompareTag("Tube"))
                    {
                        // Make nearbyObject a child of tube
                        nearbyObject.transform.parent = tube.transform;

                        Debug.Log($"{nearbyObject.name} is now a child of {tube.name}");
                    }
                }
            }
        }
    }
}
