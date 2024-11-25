using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider bc;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
    }

    public void Equip(Collider playerCollider, Transform weaponAttachPoint)
    {
        rb.isKinematic = true;
        bc.isTrigger = true;
        transform.SetParent(weaponAttachPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Physics.IgnoreCollision(playerCollider, GetComponent<Collider>());
    }

    public void Drop(Collider playerCollider, Vector3 playerForward)
    {
        transform.parent = null;
        rb.isKinematic = false;
        bc.isTrigger = false;
        rb.AddForce(playerForward * 10, ForceMode.Impulse);
        StartCoroutine(DropCooldown(playerCollider));
    }

    IEnumerator DropCooldown(Collider playerCollider)
    {
        yield return new WaitForSeconds(3);
        //happen after cooldown
        Physics.IgnoreCollision(GetComponent<Collider>(), playerCollider, false);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            Debug.Log("Enemy has been hit");
        }
    }
}
