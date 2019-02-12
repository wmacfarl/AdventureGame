using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimator : MonoBehaviour
{
    public List<Sprite> walkLeftSprites;
    public List<Sprite> walkUpSprites;
    public List<Sprite> walkDownSprites;
    public List<Sprite> walkRightSprites;

    public List<Sprite> liftUpSprites;
    public List<Sprite> liftDownSprites;
    public List<Sprite> liftLeftSprites;
    public List<Sprite> liftRightSprites;

    public List<Sprite> idleLeftSprites;
    public List<Sprite> idleUpSprites;
    public List<Sprite> idleDownSprites;
    public List<Sprite> idleRightSprites;

    public float walkCycleDuration;
    public float idleCycleDuration;
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

    public enum MovementState
    {
        IDLING = 0,
        WALKING = 1,
        LIFTING = 2
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
        EnterAnimationState(MovementState.IDLING, MovementFacing.RIGHT);
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
        }
        else
        {
            Debug.Log("INVALID MOVEMENT STATE");
            return -1;
        }
    }

    public void EnterAnimationState(MovementState movementState, MovementFacing movementFacing)
    {
        currentSpriteList = GetAnimationSprites(movementState, movementFacing);
        currentFrameDuration = GetCycleDuration(movementState) / currentSpriteList.Count;
        currentFrameIndex = 0;
        timeAtLastFrame = Time.time;
    }

    List<Sprite> GetAnimationSprites(MovementState movementState, MovementFacing movementFacing)
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

        else if (movementState == MovementState.WALKING && movementFacing == MovementFacing.LEFT)
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
        else
        {
            Debug.Log("NO ANIMATION STATE.  Facing = " + movementFacing +", State = " + movementState);
            return null;
        }
    }

    float GetAnimationDuration(List<Sprite> sprites) {

        return -1f;
    }



}
