using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    public Animator animator;

    public const string LOCOMOTION_NORMAL = "Locomotion_normal",
                        LOCOMOTION_STRAFE = "Locomotion_strafe";

    private string animationState;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        movementComponent.OnStateChangeEvent += OnStateChanged;
        ChangeAnimationState(LOCOMOTION_NORMAL);
    }

    private void OnEnable()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        movementComponent.OnStateChangeEvent += OnStateChanged;
    }

    private void OnDisable()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        movementComponent.OnStateChangeEvent -= OnStateChanged;
    }

    private void OnDestroy()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        movementComponent.OnStateChangeEvent -= OnStateChanged;
    }

    public void ChangeAnimationState(string newStateName) {
        if (newStateName != animationState)
        {
            animator.Play(newStateName);
            animationState = newStateName;
        }
    }

    private void OnStateChanged(MovementComponent.MovementState state)
    {
        switch (state)
        {
            case MovementComponent.MovementState.Idle:
                ChangeAnimationState(LOCOMOTION_NORMAL);
                targetValue = 0.0f;
                break;
            case MovementComponent.MovementState.Moving:
                ChangeAnimationState(LOCOMOTION_NORMAL);
                targetValue = 0.5f;
                break;
            case MovementComponent.MovementState.Running:
                ChangeAnimationState(LOCOMOTION_NORMAL);
                targetValue = 1f;
                break;
            default:
                break;
        }
    }

    float currentLerpTime;
    float targetValue;
    private void StartAnimatorMoveSpeedLerp()
    {
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > 1)
        {
            currentLerpTime = Time.deltaTime;
        }

        float newMoveSpeed = currentLerpTime;
        animator.SetFloat("MoveSpeed", Mathf.Lerp(animator.GetFloat("MoveSpeed"), targetValue, currentLerpTime));
    }

    // Update is called once per frame
    void Update()
    {
        StartAnimatorMoveSpeedLerp();
    }
}
