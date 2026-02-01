using UnityEngine;

public class Collector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IITEm item = collision.gameObject.GetComponent<IITEm>();
        if(item != null)
        {
            item.Collect();
        }
    }
}
