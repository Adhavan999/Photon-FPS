using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    /// <summary>
    /// This script is used to control movement and actions of the player and recieves commands from the 'PlayerController' Script
    /// </summary>

    #region Fields and Refrences
    public Camera cam;
    public GameObject gun;
    private Vector3 mVelocity = Vector3.zero;
    private Vector3 mRotation = Vector3.zero;
    private Vector3 mCameraRotation = Vector3.zero;
    private float distanceToFloor;
    private Rigidbody rb;
    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        distanceToFloor = GetComponent<CapsuleCollider>().height / 2;
    }

    void Update()
    {
        PerformTranslation();
        PerformRotation();
        PerformCameraRotation();
    }
    #endregion

    #region Update methods
    public void RotateCamera(Vector3 CameraRotation)
    {
        mCameraRotation = CameraRotation;
    }

    public void Translate(Vector3 Velocity)
    {
        mVelocity = Velocity;
    }

    public void Rotate(Vector3 Rotation)
    {
        mRotation = Rotation;
    }

    public void Jump(float JumpForce)
    {
        if( Physics.Raycast(transform.position, -Vector3.up, distanceToFloor + 0.1f) )
        {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }

    public void PerformTranslation()
    {
        if(mVelocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + mVelocity * Time.deltaTime);
        }
    }

    public void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(mRotation));
    }

    public void PerformCameraRotation()
    {
        gun.transform.Rotate(-mCameraRotation);
        cam.transform.Rotate(-mCameraRotation);
    }
    #endregion

    #region Player Action Methods
    public void FireWeapon(float WeaponDamage)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            Debug.Log("Gameobject fired at:" + hit.collider.gameObject.name);
            if ((gameObject.tag == "Red" && hit.collider.gameObject.tag == "Blue") || (gameObject.tag == "Blue" && hit.collider.gameObject.tag == "Red"))
            {
                PhotonView PV = hit.collider.gameObject.GetComponent<PhotonView>();
                PV.RPC("TakeDamage", RpcTarget.All, WeaponDamage);
            }
        }
    }
    #endregion

    
}
