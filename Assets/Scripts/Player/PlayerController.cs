using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// This Script if used for retrieving input and updating the 'PlayerMotor' Script for movement
    /// </summary>

    #region Speed,Sensitivity,weapon attributes
    [SerializeField]   //makes all objects public under field
    private float NormalSpeed = 5f;
    private float mSpeed;
    public float SprintSpeed = 10f;
    public float lookSensitivity = 3;
    public float WeaponDamage = 10f;
    public float JumpForce = 5f;

    #endregion

    #region Keycode values 
    public KeyCode LeaveRoomKey = KeyCode.Escape;
    public KeyCode Jump = KeyCode.Space;
    public KeyCode Fire_Primary = KeyCode.Mouse0;
    public KeyCode Sprint = KeyCode.LeftShift;
    #endregion

    #region Refrences
    private PlayerMotor motor;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
    }

    void Update()
    {   
        //for sprinting
        if (Input.GetKeyDown(Sprint))
        {
            mSpeed = SprintSpeed;
        }
        else
        {
            mSpeed = NormalSpeed;
        }
        
        //For checking for Weapon firing 
        if (Input.GetKeyDown(Fire_Primary))
        {
            motor.FireWeapon(WeaponDamage);
        }

        //Getting input for leaveing lobby
        if(Input.GetKeyDown(LeaveRoomKey))
        {
            GameManager.instance.LeaveRoom();
        }

        //Getting input for jump
        if(Input.GetKeyDown(Jump))
        {
            motor.Jump(JumpForce);
        }

        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        Vector3 movHorizontal = transform.right * xMov;
        Vector3 movVertical = transform.forward * zMov;

        //Final Movement vector
        Vector3 velocity = (movHorizontal + movVertical).normalized * mSpeed;
        //Applying the movement
        motor.Translate(velocity);

        //Calculating rotation as a vector
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        //Applying Rotation
        motor.Rotate(rotation);

        float xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 camrotation = new Vector3(xRot, 0f, 0f) * lookSensitivity;
        motor.RotateCamera(camrotation);

    }
    #endregion
}