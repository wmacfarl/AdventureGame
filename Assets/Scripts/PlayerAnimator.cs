using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script handles the animation for the player and probably could work for other characters as well.
 * 
 * This script is designed to be easy to quickly get working with simple animations just by dragging and dropping images into the appropriate
 * lists in the inspector.  
 * 
 * I wrote it to use as a starting-point for Game Jam projects where the speed of the initial setup --getting something working quickly 
 * with new assetsis a high priority.  It may be the case that more complicated projects should use Unity's animation system to take advantage
 * of it's built-in event callbacks and state trees, but the overhead of creating new animations and animators is too much for a jam game.
 */


[RequireComponent(typeof(SpriteRenderer))]

public class PlayerAnimator : MonoBehaviour
{
    //If we have sprites for vertical movement set this to true.  Otherwise we use our horizontal sprites.
    public bool HasVerticalSprites;
    
    //Drag Sprites into these fields in the inspector when you set up your project.  Each list contains all of the frames of a given animation.
    [SerializeField]
    List<Sprite> walkLeftSprites, walkUpSprites, walkDownSprites, walkRightSprites, 
                 walkCarryLeftSprites,walkCarryUpSprites, walkCarryDownSprites, walkCarryRightSprites,
                 liftUpSprites, liftDownSprites, liftLeftSprites, liftRightSprites, 
                 placingUpSprites, placingDownSprites, placingLeftSprites, placingRightSprites,
                 idleLeftSprites, idleUpSprites, idleDownSprites, idleRightSprites,
                 idleCarryLeftSprites, idleCarryUpSprites, idleCarryDownSprites, idleCarryRightSprites;

    //The length (in seconds) of the total animation cycle for walking and idling.  Set this in the inspector.
    [SerializeField]
    float walkCycleDuration, idleCycleDuration;

    //The length (in seconds) of the total animation cycle for lifting and placing.  These are set by the PlayerControlScript to correspond to
    //the length of time that we prevent the player from moving while lifting/placing
    float placeCycleDuration, liftCycleDuration;

    //Data about our current animation state
    List<Sprite> currentSpriteList;
    int currentFrameIndex;
    float currentFrameDuration;

    //The time when we last switched frames.  Used to decide when to switch frames again;
    float timeAtLastFrame = 0;

    SpriteRenderer spriteRenderer;

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

    //These two methods are used by PlayerControlScript to set the animation durations for lifting and placing.  These need to agree with 
    //the input control timers so shouldn't be set by this script.
    public void setLiftCycleDuration(float duration)
    {
        liftCycleDuration = duration;
    }

    public void setPlaceCycleDuration(float duration)
    {
        placeCycleDuration = duration;
    }
    void Start()
    {   
        //Get required component
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        //Cycle the current animation based on how much time has passed since we last switched frames
        if (Time.time - timeAtLastFrame > currentFrameDuration)
        {
            timeAtLastFrame = Time.time;
            currentFrameIndex++;
            if(currentFrameIndex >= currentSpriteList.Count)
            {
                currentFrameIndex = 0;
            }
            spriteRenderer.sprite = currentSpriteList[currentFrameIndex];
        }
    }

    //Each movementState has its own cycle duration
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
            throw new System.Exception("Invalid MovementState: " + movementState);
        }
    }

    //Called from PlayerControlScript whenever our animation should change.
    public void EnterAnimationState(MovementState movementState, MovementFacing movementFacing, CarryingState carryingState)
    {
        if (!HasVerticalSprites)
        {
            SetVerticalSprites(movementFacing);
        }

        currentSpriteList = GetAnimationSprites(movementState, movementFacing, carryingState);
        currentFrameDuration = GetCycleDuration(movementState) / currentSpriteList.Count;
        currentFrameIndex = 0;
        timeAtLastFrame = Time.time;
    }

    //If we don't have vertical sprites, we should use whichever horizontal sprites correspond to our most recent facing as vertical sprites
    //We call this function in EnterAnimationState whenever we change directions.
    public void SetVerticalSprites(MovementFacing movementFacing)
    {
        if (movementFacing == MovementFacing.RIGHT)
        {
            idleUpSprites = idleRightSprites;
            idleDownSprites = idleRightSprites;
            idleCarryDownSprites = idleCarryRightSprites;
            idleCarryUpSprites = idleCarryRightSprites;
            liftUpSprites = liftRightSprites;
            liftDownSprites = liftRightSprites;
            placingUpSprites = liftRightSprites;
            placingDownSprites = liftRightSprites;
            walkUpSprites = walkRightSprites;
            walkDownSprites = walkRightSprites;
            walkCarryDownSprites = walkCarryRightSprites;
            walkCarryUpSprites = walkCarryRightSprites;
        }
        else if (movementFacing == MovementFacing.LEFT)
        {
            idleUpSprites = idleLeftSprites;
            idleDownSprites = idleLeftSprites;
            idleCarryDownSprites = idleCarryLeftSprites;
            idleCarryUpSprites = idleCarryLeftSprites;
            liftUpSprites = liftLeftSprites;
            liftDownSprites = liftLeftSprites;
            placingUpSprites = liftLeftSprites;
            placingDownSprites = liftLeftSprites;
            walkUpSprites = walkLeftSprites;
            walkDownSprites = walkLeftSprites;
            walkCarryDownSprites = walkCarryLeftSprites;
            walkCarryUpSprites = walkCarryLeftSprites;
        }
    }

    //This is just a long series of if-else statements to get the animation sprites for a given set of player states.
    //Add more conditionals to this as you add more state possibilities.
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

        throw new System.Exception("NO ANIMATION STATE.  Facing = " + movementFacing + ", State = " + movementState);
    }

}
