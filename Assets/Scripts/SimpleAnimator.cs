using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimator : MonoBehaviour
{
    public List<Sprite> walkLeftSprites;
    public List<Sprite> walkUpSprites;
    public List<Sprite> walkDownSprites;
    public List<Sprite> walkRightSprites;

    public List<Sprite> walkCarryLeftSprites;
    public List<Sprite> walkCarryUpSprites;
    public List<Sprite> walkCarryDownSprites;
    public List<Sprite> walkCarryRightSprites;

    public List<Sprite> liftUpSprites;
    public List<Sprite> liftDownSprites;
    public List<Sprite> liftLeftSprites;
    public List<Sprite> liftRightSprites;

    public List<Sprite> placingUpSprites;
    public List<Sprite> placingDownSprites;
    public List<Sprite> placingLeftSprites;
    public List<Sprite> placingRightSprites;


    public List<Sprite> idleLeftSprites;
    public List<Sprite> idleUpSprites;
    public List<Sprite> idleDownSprites;
    public List<Sprite> idleRightSprites;

    public List<Sprite> idleCarryLeftSprites;
    public List<Sprite> idleCarryUpSprites;
    public List<Sprite> idleCarryDownSprites;
    public List<Sprite> idleCarryRightSprites;


    public float walkCycleDuration;
    public float idleCycleDuration;
    public float placeCycleDuration;
    public float liftCycleDuration;

    int currentAnimationFrame;

    float walkFrameDuration;
    float idleFrameDuration;
    float liftFrameDuration;

    int currentFrameIndex;
    public List<Sprite> currentSpriteList;
    float currentFrameDuration;
    float timeAtLastFrame = 0;

    SpriteRenderer sprite;

    public enum CarryingState
    {
        NOT_CARRYING = 0,
        CARRYING = 1
    }

    public enum MovementState
    {
        IDLING = 0,
        WALKING = 1,
        LIFTING = 2,
        PLACING = 3
    }

    public enum MovementFacing
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3
    }


    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeAtLastFrame > currentFrameDuration)
        {
            timeAtLastFrame = Time.time;
            currentFrameIndex++;
            if(currentFrameIndex >= currentSpriteList.Count)
            {
                currentFrameIndex = 0;
            }
            sprite.sprite = currentSpriteList[currentFrameIndex];

        }
    }

    float GetCycleDuration(MovementState movementState)
    {
        if (movementState == MovementState.IDLING)
        {
            return idleCycleDuration;
        }
        else if (movementState == MovementState.LIFTING)
        {
            return liftCycleDuration;
        }
        else if (movementState == MovementState.WALKING)
        {
            return walkCycleDuration;
        }else if (movementState == MovementState.PLACING)
        {
            return placeCycleDuration;
        }
        else
        {
            Debug.Log("INVALID MOVEMENT STATE");
            return -1;
        }
    }

    public void EnterAnimationState(MovementState movementState, MovementFacing movementFacing, CarryingState carryingState)
    {
        currentSpriteList = GetAnimationSprites(movementState, movementFacing, carryingState);
        currentFrameDuration = GetCycleDuration(movementState) / currentSpriteList.Count;
        currentFrameIndex = 0;
        timeAtLastFrame = Time.time;
    }

    List<Sprite> GetAnimationSprites(MovementState movementState, MovementFacing movementFacing, CarryingState carryingState)
    {
        if (carryingState == CarryingState.NOT_CARRYING)
        {
            if (movementState == MovementState.IDLING && movementFacing == MovementFacing.LEFT)
            {
                return idleLeftSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.RIGHT)
            {
                return idleRightSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.UP)
            {
                return idleUpSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.DOWN)
            {
                return idleDownSprites;
            }

            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.LEFT)
            {
                return walkLeftSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.RIGHT)
            {
                return walkRightSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.UP)
            {
                return walkUpSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.DOWN)
            {
                return walkDownSprites;
            }

            else if (movementState == MovementState.LIFTING && movementFacing == MovementFacing.LEFT)
            {
                return liftLeftSprites;
            }
            else if (movementState == MovementState.LIFTING && movementFacing == MovementFacing.RIGHT)
            {
                return liftRightSprites;
            }
            else if (movementState == MovementState.LIFTING && movementFacing == MovementFacing.UP)
            {
                return liftUpSprites;
            }
            else if (movementState == MovementState.LIFTING && movementFacing == MovementFacing.DOWN)
            {
                return liftDownSprites;
            }
        }
        else
        {
            if (movementState == MovementState.IDLING && movementFacing == MovementFacing.LEFT)
            {
                return idleCarryLeftSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.RIGHT)
            {
                return idleCarryRightSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.UP)
            {
                return idleCarryUpSprites;
            }
            else if (movementState == MovementState.IDLING && movementFacing == MovementFacing.DOWN)
            {
                return idleCarryDownSprites;
            }

            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.LEFT)
            {
                return walkCarryLeftSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.RIGHT)
            {
                return walkCarryRightSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.UP)
            {
                return walkCarryUpSprites;
            }
            else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.DOWN)
            {
                return walkCarryDownSprites;
            }

            else if (movementState == MovementState.PLACING && movementFacing == MovementFacing.LEFT)
            {
                return placingLeftSprites;
            }
            else if (movementState == MovementState.PLACING && movementFacing == MovementFacing.RIGHT)
            {
                return placingRightSprites;
            }
            else if (movementState == MovementState.PLACING && movementFacing == MovementFacing.UP)
            {
                return placingUpSprites;
            }
            else if (movementState == MovementState.PLACING && movementFacing == MovementFacing.DOWN)
            {
                return placingDownSprites;
            }
        }


            Debug.Log("NO ANIMATION STATE.  Facing = " + movementFacing + ", State = " + movementState);
            return null;
    }

    float GetAnimationDuration(List<Sprite> sprites) {

        return -1f;
    }



}
