﻿using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float speed = 5f;
    [SerializeField]
    private float lookSensativity = 3f;

    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterfuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f; 
    
    public float GetThruserFuelAmount()
    {
        return thrusterFuelAmount;
    }
    [SerializeField]
    private LayerMask environmentMask; 

    [Header("Spring Settings:")]
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    // Component caching
	private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;


	void Start ()
	{
		motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
	}

    void Update()
    {
        //Setting target position for spring
        //This makes the pyshics act right when it comes to applying gravity when flying over objects
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }


        //Calculate movement velocity as a 3D Vector
        float _xMov = Input.GetAxis("Horizontal");
        float _zMov = Input.GetAxis("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov; // transform.right is local direction, considering which way we're facing
        Vector3 _movVertical = transform.forward * _zMov;
        
        //final movement vector
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

        //Animate movement
        animator.SetFloat("ForwardVelocity", _zMov); 

        //Apply movement
        motor.Move(_velocity);


        //Calculate rotation as a 3D vector (turning around)
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensativity;

        //Apply rotation
        motor.Rotate(_rotation);

        //Calculate camera rotation as a 3D vector (turning around)
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensativity;

        //Apply camera rotation
        motor.RotateCamera(_cameraRotationX);

        //calculate the thrusterforce based on player input
        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if (thrusterFuelAmount >= 0.01f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }
         
        }
        else
        {
            thrusterFuelAmount += thrusterfuelRegenSpeed * Time.deltaTime;
            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        //Apply the thruster Force

        motor.ApplyThruster(_thrusterForce);
    }

    private void SetJointSettings (float _jointSpring)
    {
        joint.yDrive = new JointDrive { 
            positionSpring =_jointSpring,
            maximumForce = jointMaxForce};
    }
}
