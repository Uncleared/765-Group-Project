using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float survivalTime = 0f;

    public float hungerRate;
    public NeuralNet net;

    bool followFood = false;
    public float speed = 3f;
    public float bounds = 30f;
    public float eatMagnitude = 0.5f;
    public LayerMask foodLayerMask;
    public LayerMask agentLayerMask;
    public float foodDetectRadius = 3f;
    public float agentDetectRadius = 12f;

    public float health = 100f;

    public float foodDot = 0f;
    public float agentRequiredDistance = 5f;

    Vector3 velocity;

    MeshRenderer meshRenderer;
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

                // For eating
                if(delta.magnitude < eatMagnitude)
                {
                    health += 50f;
                    if(health > 100f)
                    {
                        health = 100f;
                    }
                    Destroy(hitCollider.gameObject);
                }
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

        //foreach(Vector3 d in displacements)
        //{
        //    print(d.magnitude);
        //}
        return foodDirection.normalized;
    }

    public int GetCloseAgents()
    {
        Vector3 foodDirection = Vector3.zero;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, agentRequiredDistance, agentLayerMask);
        List<Collider> colliderList = hitColliders.ToList();

        int count = 0;
        colliderList.ForEach(x =>
        {
            if(x.gameObject != gameObject)
            {
                count++;
            }
        });
        
        return count;
    }

    public Vector3 DetectAgentDirection()
    {
        Vector3 foodDirection = Vector3.zero;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.agentDetectRadius, agentLayerMask);

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
        return foodDirection.normalized;
    }

    public float DetectFoodValue()
    {
        currentFoodDirection = DetectFoodDirection();
        float dot = Vector3.Dot(transform.right, currentFoodDirection.normalized);

        return dot/2f + 0.5f;
    }

    public float DetectAgentValue()
    {
        currentFoodDirection = DetectAgentDirection();
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
        meshRenderer = GetComponent<MeshRenderer>();
        if(Random.value > 0.5f)
        {
            followFood = true;
        }
        net = new NeuralNet(3, 5, 1);
        //DetectFood();
    }

    void Evaluate()
    {
        float foodValue = DetectFoodValue();
        float agentValue = DetectAgentValue();
        double decision = net.Compute(foodValue, agentValue, health/10f)[0];
        Vector3 foodVelocity = DetectFoodDirection();
        Vector3 agentVelocity = DetectAgentDirection();

        bool seesFood = foodVelocity.magnitude > 0.05f;
        bool seesAgent = agentVelocity.magnitude > 0.05f;

        if(seesAgent && seesFood)
        {
            velocity = Vector3.Lerp(foodVelocity, agentVelocity, (float)decision);
        }
        else
        {
            if (seesFood)
            {
                velocity = foodVelocity;
            }
            if (seesAgent)
            {
                velocity = agentVelocity;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        survivalTime += Time.deltaTime;

        int closeAgents = GetCloseAgents();
        float finalHungerRate = hungerRate;
        if (closeAgents == 3)
        {
            finalHungerRate *= 0.5f;
        }
        else if (closeAgents == 2)
        {
            finalHungerRate *= 0.6f;

        }
        else if (closeAgents == 1)
        {
            finalHungerRate *= 0.7f;
        }
        //if(closeAgents > 0f)
        //{
        //    finalHungerRate = 0f;
        //}
        // Check if close enough to agent
        // If so, then hunger rate depletes slower
        health -= Time.deltaTime * finalHungerRate;

        // Color the agent appropriately
        meshRenderer.material.color = new Color(health/100f, health/100f, health/100f);
        if(health < 0f)
        {
            gameObject.SetActive(false);
        }

        bool reachedXBound = transform.position.x < -bounds || transform.position.x > bounds;
        bool reachedZBound = transform.position.z < -bounds || transform.position.z > bounds;
        // Check bounds
        if (reachedXBound)
        {
            if(transform.position.x < -bounds)
            {
                transform.position = new Vector3(bounds, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(-bounds, transform.position.y, transform.position.z);
            }
        }

        if (reachedZBound)
        {
            if (transform.position.z < -bounds)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, bounds);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -bounds);
            }
        }
        Evaluate();

        if(velocity.magnitude > 0.03f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * 2f);
        }
  
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
