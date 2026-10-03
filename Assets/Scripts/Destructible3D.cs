using UnityEngine;

public class Destructible3D : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public GameObject onCollectEffect;
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        // Check if the other object has a PlayerController2D component
        if (other.GetComponent<PlayerController>() != null)
        {
            Destroy(gameObject, .1f);
            //if (other.GetComponent<PlayerController>().powered)
            //{
            //    other.GetComponent<PlayerController>().powered = false;
            //    other.GetComponent<Renderer>().material.color = Color.white;

            //    // Destroy the destructible
            //    Destroy(gameObject, .1f);
            //}
            // Instantiate the particle effect
            //Instantiate(onCollectEffect, transform.position, transform.rotation);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        // Check if the other object has a PlayerController2D component
        if (other.GetComponent<PlayerController2D>() != null)
        {
            if (other.GetComponent<PlayerController2D>().powered)
            {
                other.GetComponent<PlayerController2D>().powered = false;
                other.GetComponent<Renderer>().material.color = Color.white;

                // Destroy the destructible
                Destroy(gameObject,.1f);
            }
            // Instantiate the particle effect
            //Instantiate(onCollectEffect, transform.position, transform.rotation);
        }


    }
}
