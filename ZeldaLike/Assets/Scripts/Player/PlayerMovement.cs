using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{

    #region INSPECTOR_FIELDS

    [Header("Ground Movement Values")]
    [Tooltip("Acceleration of the player")]
    [SerializeField]
    private float acceleration;
    [Tooltip("How fast the player can move")]
    [SerializeField]
    private float maxSpeed;
    [Tooltip("Gravitational acceleration")]
    [SerializeField]
    private float gravity;
    [Tooltip("How quickly the player slows")]
    [SerializeField]
    private float brakingRate;
    [Tooltip("Maximum falling speed the player can reach")]
    [SerializeField]
    private float terminalVelocity;

    [Header("Jumping Values")]
    [SerializeField]
    [Tooltip("Whether the player can jump or not")]
    private bool canJump;
    [Tooltip("How high the player can jump when on the ground")]
    [SerializeField]
    private float groundJumpHeight;
    [Tooltip("How high the player can jump when in the air")]
    [SerializeField]
    private float airJumpHeight;
    [Tooltip("How many midair jumps the player gets")]
    [SerializeField]
    [Range(0, 5)]
    private int midairJumps;
    [Tooltip("How much acceleration is applied in the movement direction when jumping")]
    [SerializeField]
    private float airJumpCorrectionVelocity;
    [Tooltip("Proportion between the jump velocity and original")]
    [SerializeField]
    [Range(0, 1.0f)]
    private float airJumpCorrectionAlpha;
    [SerializeField]
    [Tooltip("How long after jumping the player must wait before jumping again")]
    private float jumpCooldown;
    [Tooltip("Midair default correction speed")]
    [SerializeField]
    private float midairSpeed;
    [Tooltip("Max air speed")]
    [SerializeField]
    private float maximumAirSpeed;
    [Tooltip("How long the player can not be interacting the ground but still be able to do a ground jump")]
    [SerializeField]
    private float groundMissGimme;

    #endregion

    #region PROPERTIES
    private Vector3 curr_acceleration;
    private bool left, right, up, down, noinput, jump;
    private int currJumps;

    private float currJumpCoolDown;

    private float currGroundMissGimme;

    #endregion

    #region METHODS

    /// <summary>
    /// Assigns keyinput
    /// </summary>
    private void GetKeyInput()
    {
        left = Input.GetKey(KeyCode.A);
        right = Input.GetKey(KeyCode.D);
        up = Input.GetKey(KeyCode.W);
        down = Input.GetKey(KeyCode.S);
        if(canJump)
            jump = Input.GetKeyDown(KeyCode.Space);

        noinput = !(left | right | up | down);
    }

    /// <summary>
    /// Handles calls to specific movement modifications that impact
    /// the rigidbody values the player has
    /// </summary>
    private void Move()
    {

        //Decreases time to zero for the jump cooldown
        currJumpCoolDown -= Time.deltaTime;
        currJumpCoolDown = (currJumpCoolDown <= 0 ? 0 : currJumpCoolDown);

        //Continue if player is hitting the ground
        if (this.GetComponentInChildren<PlayerDetection>().HitGround())
        {
            //Reset the ground variables
            currJumps = 0;
            currGroundMissGimme = groundMissGimme;
            
            //Set the movement to match the camera rotation instead of air velocity
            Vector3 newAcceleration = GetBaseMovementOnInput() * acceleration;
            newAcceleration = RotateMovementTowardCameraAngle(newAcceleration);
            curr_acceleration = SetMovementToMatchGroundNormal(newAcceleration);

            this.GetComponent<Rigidbody>().velocity += curr_acceleration * Time.deltaTime;
            EnsureMaxSpeedReached();

            //Apply friction forces if player is not active
            if (noinput)
            {
                ApplyBraking();
            }

            DoGroundJump();
            
        }
        else
        //Player is in the air
        {

            //Allow some relaxation on how late after the ground player can jump. To account for movement discrepancy
            //across the terrain or being nice to player reaction time
            currGroundMissGimme -= Time.deltaTime;
            currGroundMissGimme = (currGroundMissGimme < 0 ? 0 : currGroundMissGimme);

            //Apply an acceleration in direction of camera, if air direction changing is allowed
            Vector3 currAcc = GetBaseMovementOnInput();
            currAcc = RotateMovementTowardCameraAngle(currAcc);
            ApplyAirMovement(currAcc);

            //Calculate and apply gravity
            HandleGravityVector();
            this.GetComponent<Rigidbody>().velocity += curr_acceleration * Time.deltaTime;

            //Unlike ground which must check in 3D, max Airspeed has a horizontal air speed (x-z) and a vertical air speed (y)
            EnforceMaxAirSpeed();

            //If the player has been off the ground short enough time let them do a ground jump
            if (currGroundMissGimme <= 0)
            {
                DoAirJump(currAcc);
            }
            else
            {
                DoGroundJump();
            }

        }
    }

    /// <summary>
    /// Gets the input and converts into a movement vector
    /// </summary>
    private Vector3 GetBaseMovementOnInput()
    {
        Vector3 currMovement = Vector3.zero;

        if (left)
        {
            currMovement += new Vector3(-1, 0, 0);
        }
        if (right)
        {
            currMovement += new Vector3(1, 0, 0);
        }
        if (up)
        {
            currMovement += new Vector3(0, 0, 1);
        }
        if (down)
        {
            currMovement += new Vector3(0, 0, -1);
        }

        return currMovement;
    }

    /// <summary>
    /// Input movement for when the player is in the air
    /// </summary>
    /// <param name="currMovement"></param>
    private void ApplyAirMovement(Vector3 currMovement)
    {
        currMovement *= midairSpeed;
        
        //Get the original planar velocity and a new velocity
        Vector3 og_velocity = this.GetComponent<Rigidbody>().velocity - 
            (Vector3.up * this.GetComponent<Rigidbody>().velocity.y);
        Vector3 newVel = og_velocity + (currMovement * Time.deltaTime);
        newVel = new Vector3(newVel.x, 0, newVel.y);

        if(Vector3.Dot(newVel, og_velocity) >= 0)
        {
            this.curr_acceleration = currMovement;
        }
        else
        {
            this.curr_acceleration = Vector3.zero;
        }

    }

    /// <summary>
    /// Maximum air velocity
    /// </summary>
    private void EnforceMaxAirSpeed()
    {
        Vector3 planar_velocity = this.GetComponent<Rigidbody>().velocity -
            (Vector3.up * this.GetComponent<Rigidbody>().velocity.y);

        if(planar_velocity.magnitude > maximumAirSpeed)
        {
            this.GetComponent<Rigidbody>().velocity = (planar_velocity.normalized * maximumAirSpeed)
                + (Vector3.up * this.GetComponent<Rigidbody>().velocity.y);
        }
    }

    /// <summary>
    /// Jump to apply if player is on the ground
    /// </summary>
    private void DoGroundJump()
    {
        if (jump && currJumpCoolDown <= 0)
        {
            this.GetComponent<Rigidbody>().velocity /= 2;
            float vel = CalculateJumpForce(groundJumpHeight);
            this.GetComponent<Rigidbody>().velocity += Vector3.up * vel;
            currGroundMissGimme = 0;
            currJumpCoolDown = jumpCooldown;
        }
    }

    /// <summary>
    /// Jump if player is in air and related air correction calculations
    /// </summary>
    /// <param name="movementVector"></param>
    private void DoAirJump(Vector3 movementVector)
    {
        if (jump && currJumps < midairJumps && currJumpCoolDown <= 0)
        {
            float jumpspeed = CalculateJumpForce(airJumpHeight);

            float up = this.GetComponent<Rigidbody>().velocity.y;

            up = (up > 0 ? up + jumpspeed : jumpspeed);

            if (!movementVector.Equals(Vector3.zero))
            {

                Vector3 newVel = this.GetComponent<Rigidbody>().velocity - (Vector3.up * up);

                movementVector = Vector3.Lerp(newVel, movementVector * airJumpCorrectionVelocity, airJumpCorrectionAlpha);
            }

            movementVector = new Vector3(movementVector.x, up, movementVector.z);
            this.GetComponent<Rigidbody>().velocity = movementVector;
            currJumps++;
            currJumpCoolDown = jumpCooldown;
        }
    }

    /// <summary>
    /// Transforms the current movement vector to match the camera coordinates
    /// </summary>
    private Vector3 RotateMovementTowardCameraAngle(Vector3 currMovement)
    {
        if(PlayerCamera.playerCam == null)
        {
            return currMovement;
        }

        Quaternion rotation = PlayerCamera.playerCam.GetCameraRotation();

        Vector3 rotated_angle = rotation * currMovement;
        Vector3 y_projection = Vector3.up * Vector3.Dot(rotated_angle, Vector3.up);

        return (rotated_angle - y_projection).normalized * Vector3.Magnitude(currMovement);

    }

    /// <summary>
    /// Transforms the current movement vector to match the ground normal
    /// </summary>
    private Vector3 SetMovementToMatchGroundNormal(Vector3 currMovement)
    {
        Vector3 groundNormal = this.GetComponentInChildren<PlayerDetection>().GetGroundNormal();

        Vector3 horiz = Vector3.Cross(groundNormal, Vector3.up).normalized;

        if (groundNormal.Equals(Vector3.up))
        {
            horiz = Vector3.right;
        }

        Vector3 straight = Vector3.Cross(groundNormal, horiz);

        Vector3 horiz_proj = Vector3.Dot(horiz, currMovement) * horiz;
        Vector3 straight_proj = Vector3.Dot(straight, currMovement) * straight;

        return horiz_proj + straight_proj;
    }

    /// <summary>
    /// SLows the player down if there is no input
    /// </summary>
    private void ApplyBraking()
    {
        this.GetComponent<Rigidbody>().velocity = Vector3.Lerp(this.GetComponent<Rigidbody>().velocity,
            Vector3.zero, Time.deltaTime * brakingRate);
        if (Vector3.Distance(this.GetComponent<Rigidbody>().velocity, Vector3.zero) <= 0.5f)
        {
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// For the given jump height and default gravity, calculate how much force must be applied (assumes frictionless ground)
    /// </summary>
    private float CalculateJumpForce(float height)
    {
        //Assume initial is correct
        float jf = height;

        //use decay algorithm to approach actual height
        for (int i = 0; i < 20; i++)
        {
            //Derive where velocity would be 0
            float t = jf / gravity;

            //Apply time to calulcate height reached
            float h_f_jump = (jf * t) - (0.5f * gravity * t * t);

            //Find the difference between heights
            float dff = height - h_f_jump;

            //If height is off by a certain margin, then repeat after adjusting by decay growth
            if (Mathf.Abs(dff) > 0.025f)
            {
                jf += (Mathf.Sign(dff) * (height / (i + 1.0f)));
            }
            else
            {
                return jf;
            }

        }

        return jf;

    }

    private void EnsureMaxSpeedReached()
    {
        if(Vector3.Magnitude(this.GetComponent<Rigidbody>().velocity) > maxSpeed)
        {
            this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity.normalized * maxSpeed;
        }

    }

    /// <summary>
    /// Adds acceleration due to gravity to the velocity of the rigidbody
    /// </summary>
    private void HandleGravityVector()
    {
        this.curr_acceleration += Vector3.down * gravity;

        Vector3 vel = this.GetComponent<Rigidbody>().velocity;
        if (vel.y < -this.terminalVelocity)
        {
            this.GetComponent<Rigidbody>().velocity = new Vector3(vel.x, -terminalVelocity, vel.z);
        }

    }

    #endregion

    #region MONOBEHAVIOR

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetKeyInput();
        Move();

    }

    public Vector3 GetDirection()
    {
        return PlayerCamera.playerCam.GetCameraRotation() * Vector3.forward;
    }
    #endregion
}
