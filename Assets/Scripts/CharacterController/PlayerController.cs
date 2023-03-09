using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStatsComponent))]
public class PlayerController : MonoBehaviour
{
    // We need to get our main player which we are supposed to control
    public static PlayerController mainPlayer;

    [Header("SETTINGS")]
    public float rotateSmoothingFactor = 0.1f;
    public float moveSmoothingFactor = 0.1f;

    [Header("DEBUG")]
    [SerializeField]
    float targetMoveSpeed;
    float currentMoveSpeed;

    Animator anim;
    float turnSmoothingVelocity;
    float moveSmoothingVelocity;
    PlayerStatsComponent playerStats;
    Transform cam;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        if (mainPlayer == null)
            mainPlayer = this;
        anim = GetComponent<Animator>();
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        playerStats = GetComponent<PlayerStatsComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), Input.GetKey(KeyCode.Mouse1), Input.GetKeyDown(KeyCode.Mouse0), Input.GetKey(KeyCode.LeftShift));
    }

    public void Move(Vector2 input, bool block, bool attack, bool running)
    {
        Vector2 inputDirection = input.normalized;

        if (block)
        {
            if (attack)
            {
                anim.SetTrigger("Attack");
            }

            if (!IsPlaying("Attack"))
            { //if not attacking, strafe with shield up
                anim.SetBool("Strafe", true);

                float targetRotation = cam.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothingVelocity, rotateSmoothingFactor);
                targetMoveSpeed = playerStats.strafeSpeed;

                targetMoveSpeed = targetMoveSpeed * inputDirection.magnitude; //set to 0 if no input
                currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, targetMoveSpeed, ref moveSmoothingVelocity, moveSmoothingFactor);



                Vector3 camForward = Vector3.Scale(cam.up, new Vector3(1, 0, 1)).normalized;
                Vector3 move = inputDirection[1] * cam.forward + inputDirection[0] * cam.right;
                move.y = 0;
                move.Normalize();

                anim.SetFloat("strafeX", transform.InverseTransformDirection(move).x);
                anim.SetFloat("strafeY", transform.InverseTransformDirection(move).z);

                //rb.velocity = inputDirection * currentMoveSpeed;
                float velocityY = rb.velocity.y;
                rb.velocity = new Vector3(move.x * currentMoveSpeed / 1.5f, velocityY, move.z * currentMoveSpeed / 1.5f);
                //rb.velocity = move * currentMoveSpeed;
            }
        }
        else
        {
            anim.SetBool("Strafe", false);
            if (inputDirection != Vector2.zero)
            {
                float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg + cam.eulerAngles.y; //Get target rotation in degrees
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothingVelocity, rotateSmoothingFactor); //smoothly rotate around transform Y axis
            }

            if (running)
            {
                targetMoveSpeed = playerStats.runSpeed;
            }
            else
            {
                targetMoveSpeed = playerStats.walkSpeed;
            }
            targetMoveSpeed = targetMoveSpeed * inputDirection.magnitude; //set to 0 if no input
            currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, targetMoveSpeed, ref moveSmoothingVelocity, moveSmoothingFactor);

            Vector3 move = transform.forward * currentMoveSpeed / 5;
            float velocityY = rb.velocity.y;
            rb.velocity = new Vector3(move.x * currentMoveSpeed, velocityY, move.z * currentMoveSpeed);
            anim.SetFloat("MoveSpeed", targetMoveSpeed / playerStats.runSpeed, moveSmoothingFactor, Time.deltaTime);
        }
    }

    bool IsPlaying(string animationName)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
}
