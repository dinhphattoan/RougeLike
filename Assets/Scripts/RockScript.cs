using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour
{
    ItemPunctuate itemPunctuate;
    public ParticleSystem rockParticle;
    [Header("Rock attributes")]

    public int rockHealth;
    // Start is called before the first frame update
    void Start()
    {
        itemPunctuate = GetComponent<ItemPunctuate>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void GetHit()
    {
        rockHealth -= 1;
        if (rockHealth <= 0)
        {
            var particle =Instantiate(rockParticle, new Vector3(transform.position.x,transform.position.y,transform.position.z), Quaternion.identity);
            particle.Play();
            FindObjectOfType<GameManager>().SpawnResourceMaterial(transform.position,5,"Rock");
            Destroy(this.transform.gameObject);
        }
        else
        {
            itemPunctuate.Punctuate();
        }
    }

}
