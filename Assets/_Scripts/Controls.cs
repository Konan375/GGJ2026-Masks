using UnityEngine;

public class Controls : MonoBehaviour
{


    public float speed = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorrizontal = Input.GetAxis("Horrizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movemnet = new Vector3(moveVertical, moveHorrizontal, speed);

        transform.Translate(movemnet * speed * Time.deltaTime);
    }

}
