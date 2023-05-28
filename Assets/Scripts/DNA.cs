using UnityEngine;

public class DNA : MonoBehaviour
{
    // Holds the Red, Green and Blue values;
    public float r;
    public float g;
    public float b;

    public void Start() {
        print("hellpo");
    }


    void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            Destroy(gameObject);
        }
    }
}