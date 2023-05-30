using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public LayerMask foodLayerMask;
    public LayerMask agentLayerMask;
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.foodDetectRadius, foodLayerMask);

        List<Vector3> displacements = new List<Vector3>();
        foreach (var hitCollider in hitColliders)
        {
            // Ignore itself
            if (hitCollider.gameObject != gameObject)
            {
                // Do a dot product check
                Vector3 delta = hitCollider.transform.position - transform.position;
                if (Vector3.Dot(delta.normalized, transform.forward) > 0f)
                {
                    displacements.Add(delta);
                }
            }
        }
        displacements.Sort((d1, d2) => d1.magnitude.CompareTo(d2.magnitude));
        if(displacements.Count > 0)
        {
            foodDirection = displacements[0];
        }

        foreach(Vector3 d in displacements)
        {
            print(d.magnitude);
        }
        return foodDirection;
    }


    public Vector3 DetectAgentDirection(LayerMask layerMask)
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

    public float DetectAgentValue(LayerMask layerMask)
    {
        currentFoodDirection = DetectAgentDirection(layerMask);
        float dot = Vector3.Dot(transform.right, currentFoodDirection.normalized);

        return dot / 2f + 0.5f;
    }


    public void SeekFood()
    {

    }

    public void SeekAgent()
    {

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

        if (Input.GetKeyDown(KeyCode.A))
        {
            foodDot = DetectAgentValue(agentLayerMask);
        }
    }
}
