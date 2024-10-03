using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feeder : MonoBehaviour
{
    public bool sheep;
    public bool food = false;
    public GameObject foodGO;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Target" && Input.GetKey(KeyCode.Space))
        {
            PlayerInteractions player = collision.gameObject.GetComponent<PlayerInteractions>();
            if(!food && player.hayComida && sheep)
            {
                food = true;
                FoodVisibility(food);
                HUD.Score += 10;
                collision.gameObject.GetComponent<PlayerInteractions>().hayComida = false;
                collision.gameObject.GetComponent<PlayerInteractions>().foodVisibility();

            }
        }
    }

    public void FoodVisibility(bool food)
    {
        foodGO.SetActive(food);
    }
}
