using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public LayerMask layerMask;
    public float foodDetectRadius = 3f;

    public float foodDot = 0f;
    public void DetectDirection()
    {


    }

    [SerializeField]
    Vector3 currentFoodDirection;

    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + currentFoodDirection);
    }

    public Vector3 DetectFoodDirection()
    {
        Vector3 foodDirection = Vector3.zero;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.foodDetectRadius, layerMask);

        List<Vector3> validDirections = new List<Vector3>();
        foreach (var hitCollider in hitColliders)
        {
            // Ignore itself
            if(hitCollider.gameObject != gameObject)
            {
                // Do a dot product check
                Vector3 delta = hitCollider.transform.position - transform.position;
                if(Vector3.Dot(delta.normalized, transform.forward) > 0f)
                {
                    validDirections.Add(delta);
                }
            }
        }

        // Normalize it
        if(validDirections.Count > 0)
        {
            foreach(Vector3 validDir in validDirections)
            {
                foodDirection += validDir;
            }
            foodDirection /= validDirections.Count;
        }
        return foodDirection;
    }

    public float DetectFoodValue()
    {
        currentFoodDirection = DetectFoodDirection();
        float dot = Vector3.Dot(transform.right, currentFoodDirection.normalized);

        return dot/2f + 0.5f;
    }

    // Start is called before the first frame update
    void Start()
    {
        //DetectFood();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            foodDot = DetectFoodValue();
        }
    }
}
