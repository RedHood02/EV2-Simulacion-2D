using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float moveX;
    public float rotationSpeed;

    public KeyCode attackKey;

    public Transform projectileSpawnPoint;
    public GameObject bulletObj;

    void Update()
    {
        GetAxis();
        RotatePlayer();

        if (Input.GetKeyDown(attackKey)) Attack();
    }


    void GetAxis()
    {
        moveX = Input.GetAxisRaw("Horizontal");
    }



    void RotatePlayer()
    {
        float rotationAmount = -moveX * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationAmount, Space.Self);
    }


    void Attack()
    {
        Instantiate(bulletObj, projectileSpawnPoint.position, Quaternion.identity);
        Debug.Log("Attacked");
    }
}
