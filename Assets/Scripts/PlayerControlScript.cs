using System.Collections;
using System.Collections.Generic;

using static SimpleAnimator;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{
    public float speed;
    new Rigidbody2D rigidbody;

    MovementFacing currentMovementFacing;
    MovementState currentMovementState;

    MovementFacing previousMovementFacing;
    MovementState previousMovementState;

    SimpleAnimator animator;

   
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<SimpleAnimator>();
    }

    void Update()
    {
        bool canMove = true;
        PerformActions(GetActionInput());
        if (canMove)
        {
            PerformMovement(GetDirectionInput());
        }
    }


    Vector2 GetDirectionInput()
    {
        float inputX = 0;
        float inputY = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            inputX = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            inputX = 1;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            inputY = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            inputY = -1;
        }
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.S))
        {
            inputY = -1;
        }

        return new Vector2(inputX, inputY);
    }

    bool[] GetActionInput()
    {
        bool[] input = new bool[2];
        input[0] = false;
        input[1] = false;
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Period))
        {
            input[0] = true;
        }
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Slash))
        {
            input[1] = true;
        }
        return input;
    }

    void PerformActions(bool[] input)
    {
        if (input[0])
        {
            PerformPrimaryAction();
        }
        else if (input[1])
        {
            PerformSecondaryAction();
        }
    }

    public void PerformPrimaryAction()
    {

    }

    public void PerformSecondaryAction()
    {

    }

    void PerformMovement(Vector2 input)
    {
        Vector3 delta;
        delta = new Vector3(speed * input.x, speed * input.y, 0);
        rigidbody.MovePosition(transform.position + delta);

        if (input.x != 0 || input.y != 0)
        {

            currentMovementState = MovementState.WALKING;

            if (input.x > 0)
            {
                currentMovementFacing = MovementFacing.RIGHT;
            }
            else if (input.x < 0)
            {
                currentMovementFacing = MovementFacing.LEFT;

            }
            else if (input.y > 0)
            {
                currentMovementFacing = MovementFacing.UP;
            }
            else if (input.y < 0)
            {
                currentMovementFacing = MovementFacing.DOWN;
            }
        }
        else
        {
            currentMovementState = MovementState.IDLING;
        }

        if (currentMovementFacing != previousMovementFacing || currentMovementState != previousMovementState)
        {
            animator.EnterAnimationState(currentMovementState, currentMovementFacing);
        }

        previousMovementState = currentMovementState;
        previousMovementFacing = currentMovementFacing;
    }

}
