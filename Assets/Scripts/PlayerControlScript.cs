using System.Collections;
using System.Collections.Generic;
using static PlayerAnimator;
using UnityEngine;

/*  This script handles basic player controls for a top-down, 2D, SNES-era-Zelda-like adventure game.  The player can walk around using the 
 *  arrow keys and WASD.
 *  
 *  There are two action buttons.  "Z" and "." are the primary action buttons.  They are used to pick up and put down objects, and to 
 *  examine/talk-to NPCs and interactables.
 *  
 *  The "X" and "/" keys perform a secondary action.  This might be an attack or using a selected item.  At the moment they just 
 *  call the PerformSecondaryAction() function which is left to be defined for the specific game.
 *  
 *  and can pick up and put down objects using the "Z" and "/" keys.
 *  
 *  Collision detection for movement is handled using Unity's built-in physics.  The player has a rigidbody and obstacles have colliders.  
 *  Movement is done using the rigidBody.movePosition function which takes a target position as an argument and handles the physics involved 
 *  in moving the gameObject to that position.  I am not sure if there is any advantage or disadvantage to using this versus writing our own
 *  collision detection using raycasts.
 *  
 *  The check to determine what objects are in front of you (in order to pick one of them up) is done with a number of raycasts.  Objects that
 *  can be picked up should be on the "Objects" layer and have a PickupableScript component.
 *  
 *  The player's  state is defined by three enums defined in PlayerAnimator
 *    their MovementFacing (UP, DOWN, LEFT, RIGHT)
 *    their MovementState(IDLING, LIFTING, PLACING, WALKING)
 *    their CarryingState(CARRYING, NOT_CARRYING)
 *  
 *  Whenevever one of these states changes, PlayerControlScript calls PlayerAnimator.EnterAnimationState to set the correct animations.
 *  
 */
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(BoxCollider2D))]

public class PlayerControlScript : MonoBehaviour
{
    //Serialized Fields can and should be set in the inspector rather than in code.

    [SerializeField] //The player's movementSpeed
    public float speed;

    [Header("Object Detection Raycasts")]  //The following public variables set the properties of our object detection process.  
                                           //They are used in the GetObjectsIAmFacing() function

    [SerializeField]                   //The number of raycasts we perform on each axis to determine whether there is an object in front of us.
    Vector2Int numberOfRaycastsPerSide;//More raycasts will allow us to notice smaller objects.

    [SerializeField]     //The distance between the edge of the player's collider and the origin of the raycasts
    float raycastPadding;

    [SerializeField]    //How far in front of me I should check for an adjacent object
    float raycastDistance;

    [SerializeField]    //The layers we raycast on to find objects.  Any objects we want to interact with need to be in this layer.
    LayerMask objectLayers;

    [SerializeField]    //How long we freeze our controls when we are picking up and putting down an object
    float liftingActionDuration, placingActionDuration;

    [SerializeField]    //The position we place an object that we're holding, relative to the origin of the player.
    Vector2 carryOffset = new Vector2(0, 1.3f);

    bool canTakeInput = true; //Set to false when we are in animations or dialogue or otherwise can't control the player.

    // These taken together define the state of the player.  
    // If ever the current state is different from the previous state we trigger a new animation.
    MovementFacing currentMovementFacing;
    MovementState currentMovementState;
    CarryingState currentCarryingState;

    MovementFacing previousMovementFacing;
    MovementState previousMovementState;
    CarryingState previousCarryingState;

    GameObject objectIAmCarrying = null; //The object I am carrying.  Is null if I'm not carrying anything.

    //Required Components
    new Rigidbody2D rigidbody;
    PlayerAnimator animator;
    new BoxCollider2D collider;


    void Start()
    {
        //Get required components.
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<PlayerAnimator>();
        collider = GetComponent<BoxCollider2D>();

        //Set current state to default idle-right
        currentMovementState = MovementState.IDLING;
        currentMovementFacing = MovementFacing.RIGHT;

        //Enter starting animation state and ensure that the animation duration for lifting and placing objects agrees with the amount of time
        //that we freeze our controls.
        animator.EnterAnimationState(currentMovementState, currentMovementFacing, currentCarryingState);
        animator.setLiftCycleDuration(liftingActionDuration);
        animator.setPlaceCycleDuration(placingActionDuration);
    }

    void Update()
    {
        if (canTakeInput)
        {
            PerformActions(GetActionInput());
            PerformMovement(GetDirectionInput());
        }

        SetAnimation();
    }

    //Direction input is binary and returns a Vector with values -1, 0, or 1 for each axis
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

        return new Vector2(inputX, inputY);
    }

    //Action input is binary and returns an array of two bools, one for each action button.
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

    GameObject GetNpcToTalkTo()
    {
        List<GameObject> ObjectsIAmFacing = GetObjectsIAmFacing();
        GameObject closetNPC = null;
        float closestNpcDistance = 10000;
        foreach (GameObject objectIAmFacing in ObjectsIAmFacing)
        {
            float distanceBetweenMeAndObject = Vector2.SqrMagnitude(transform.position - objectIAmFacing.transform.position);
            if (objectIAmFacing.GetComponent<ConversationScript>() != null && distanceBetweenMeAndObject < closestNpcDistance)
            {
                closestNpcDistance = distanceBetweenMeAndObject;
                closetNPC = objectIAmFacing;
            }
        }
        return closetNPC;
    }

    //Primary action interacts with whatever is in front of us.  If that object is Pickupable, we pick it up.  If we are holding an object
    //this places it on the ground in front of us if there is room.
    public void PerformPrimaryAction()
    {
        if (objectIAmCarrying == null)
        {
            GameObject objectToPickUp = GetObjectIShouldPickUp();
            if (objectToPickUp != null)
            {
                StartCoroutine(BeginLift(objectToPickUp));
            }
            else
            {
                GameObject npcToTalkTo = GetNpcToTalkTo();

                if (npcToTalkTo != null)
                {
                    npcToTalkTo.GetComponent<ConversationScript>().GetTalkedTo();
                }
            }
        }
        else
        {
            if (CanIPlaceObject())
            {
                StartCoroutine(BeginPlaceObject());
            }
        }
    }

    //This is a coroutine that sets ours tate to LIFTING and then waits pickUpAnimationLength seconds before calling CompleteLift()
    IEnumerator BeginLift(GameObject objectToPickUp)
    {
        canTakeInput = false;
        currentMovementState = MovementState.LIFTING;
        yield return new WaitForSeconds(liftingActionDuration);
        CompleteLift(objectToPickUp);
        canTakeInput = true;
    }

    //Called by BeingLift() after the animation is done.  Sets objectIAmCarrying
    public void CompleteLift(GameObject objectToPickUp)
    {
        objectToPickUp.GetComponent<PickupableScript>().GetPickedUp(gameObject, carryOffset);
        currentCarryingState = CarryingState.CARRYING;
        currentMovementState = MovementState.IDLING;
        objectIAmCarrying = objectToPickUp;
    }

    //This is a coroutine that sets ours tate to PLACING and then waits pickUpAnimationLength seconds before calling CompleteLift()
    IEnumerator BeginPlaceObject()
    {
        canTakeInput = false;
        currentMovementState = MovementState.PLACING;
        yield return new WaitForSeconds(placingActionDuration);
        CompletePlaceObject();
        canTakeInput = true;
    }

    //Called by BeingLift() after the animation is done.  Sets objectIAmCarrying to null
    void CompletePlaceObject()
    {
        float putDownDistance = GetPlaceDistance();
        objectIAmCarrying.GetComponent<PickupableScript>().GetPutDown(MovementFacingToVector2(currentMovementFacing) * putDownDistance);
        currentCarryingState = CarryingState.NOT_CARRYING;
        currentMovementState = MovementState.IDLING;
        objectIAmCarrying = null;
    }


    Vector2 MovementFacingToVector2(MovementFacing facing)
    {
        if (facing == MovementFacing.RIGHT)
        {
            return Vector2.right;
        } else if (facing == MovementFacing.LEFT)
        {
            return Vector2.left;
        }
        else if (facing == MovementFacing.UP)
        {
            return Vector2.up;
        }
        else if (facing == MovementFacing.DOWN)
        {
            return Vector2.down;
        }
        else
        {
            return new Vector2();
        }
    }

    //GetPlaceDistance returns a float representing how far in front of us we should place the object we are putting down.
    float GetPlaceDistance()
    {
        float placeDistance = 0;

        //We use the bounds of the object's collider to figure out where to place the object, and the bounds are set to zero if the collider
        //is not enabled, so we turn the collider on during this function.
        objectIAmCarrying.GetComponent<BoxCollider2D>().enabled = true; ;

        //These distances are calculated assuming that the origin of the sprites/colliders for the player and the object is the bottom center
        if (currentMovementFacing == MovementFacing.RIGHT || currentMovementFacing == MovementFacing.LEFT)
        {
            placeDistance = collider.bounds.extents.x + objectIAmCarrying.GetComponent<BoxCollider2D>().bounds.extents.x;
        }
        else
        {
            if (currentMovementFacing == MovementFacing.UP)
            {
                placeDistance = collider.bounds.extents.y * 2;
            }
            else if (currentMovementFacing == MovementFacing.DOWN)
            {
                placeDistance = objectIAmCarrying.GetComponent<BoxCollider2D>().bounds.extents.y * 2;
            }
        }

        //Turn the collider back off in case our placement fails for some reason and we end up still holding the object.
        objectIAmCarrying.GetComponent<BoxCollider2D>().enabled = false;

        return placeDistance;
    }

    //Returns true if there is space in front of me to place the object I am holding
    bool CanIPlaceObject()
    {
        return objectIAmCarrying.GetComponent<PickupableScript>().CanIBePutDown(MovementFacingToVector2(currentMovementFacing) * GetPlaceDistance());
    }

    //Returns the closest pickupable object in front of me.  This handles the case where I am adjacent to and facing two different objects
    //that I could pick up and I need to decide which one.
    GameObject GetObjectIShouldPickUp()
    {
        List<GameObject> ObjectsIAmFacing = GetObjectsIAmFacing();
        GameObject closetObjectICanPickup = null;
        float closestObjectDistance = 10000;
        foreach (GameObject objectIAmFacing in ObjectsIAmFacing)
        {
            float distanceBetweenMeAndObject = Vector2.SqrMagnitude(transform.position - objectIAmFacing.transform.position);
            if (objectIAmFacing.GetComponent<PickupableScript>() != null && distanceBetweenMeAndObject < closestObjectDistance)
            {
                closestObjectDistance = distanceBetweenMeAndObject;
                closetObjectICanPickup = objectIAmFacing;
            }
        }
        return closetObjectICanPickup;
    }

    public void PerformSecondaryAction()
    {
    }

    //Returns a MovementFacing if passed a vector corresponding to one of the cardinal directions
    MovementFacing Vector2ToFacing(Vector2 directionVector)
    {
        if (directionVector == Vector2.right)
        {
            return MovementFacing.RIGHT;
        }
        else if (directionVector == Vector2.left)
        {
            return MovementFacing.LEFT;

        }
        else if (directionVector == Vector2.up)
        {
            return MovementFacing.UP;

        }
        else if (directionVector == Vector2.down)
        {
            return MovementFacing.DOWN;
        }
        else
        {
            throw new System.Exception("Invalid directionVector: " + directionVector);
        }
    }

    //Sets the currentMovementState to WALKINg or IDLING based on the direction input
    void SetMovementState(Vector2 input)
    {
        if (input.x != 0 || input.y != 0)
        {
            currentMovementState = MovementState.WALKING;
        }
        else if (currentMovementState != MovementState.LIFTING && currentMovementState != MovementState.PLACING)
        {
            currentMovementState = MovementState.IDLING;
        }
    }

    //Sets the currentMovementFacing based on the direciton input.  If we are moving diagonally we set the facing to the horizontal direction
    void SetMovementFacing(Vector2 input)
    {
        if (input.x != 0)
        {
            currentMovementFacing = Vector2ToFacing(new Vector2(input.x, 0));
        }
        else
        {
            if (input.y != 0)
            {
                currentMovementFacing = Vector2ToFacing(new Vector2(0, input.y));
            }
        }
    }

    //Checks if the player state has changed from the previous frame and if so tells the animator.
    void SetAnimation()
    {
        MovementFacing actingCurrentMovementFacing = currentMovementFacing;
        if (animator.HasVerticalSprites == false && (currentMovementFacing == MovementFacing.UP || currentMovementFacing == MovementFacing.DOWN))
        {
            actingCurrentMovementFacing = previousMovementFacing;
        }
        if (actingCurrentMovementFacing != previousMovementFacing || currentMovementState != previousMovementState || currentCarryingState != previousCarryingState)
        {
            animator.EnterAnimationState(currentMovementState, actingCurrentMovementFacing, currentCarryingState);
        }

        previousMovementState = currentMovementState;
        previousMovementFacing = currentMovementFacing;
        previousCarryingState = currentCarryingState;

    }

    //Moves the player and sets the facing and movementState based on the diection input
    void PerformMovement(Vector2 input)
    {
        Vector3 delta;
        delta = new Vector3(speed * input.x, speed * input.y, 0);
        rigidbody.MovePosition(transform.position + delta);
        SetMovementState(input);
        SetMovementFacing(input);
    }

    //Uses a series raycasts on objectLayer to return a list of objects we are facing and adjacent to
    List<GameObject> GetObjectsIAmFacing()
    {
        Bounds bounds = collider.bounds;
        Vector3 raycastCorner = new Vector3();
        Vector3 raycastDirection = new Vector3();
        Vector3 perpendicularDeltaBetweenRaycasts = new Vector3();
        int currentRaycastsPerSide = 0;

        if (currentMovementFacing == MovementFacing.UP)
        {
            raycastCorner = new Vector3(bounds.min.x + raycastPadding, bounds.max.y, 0);
            perpendicularDeltaBetweenRaycasts = new Vector3((2*bounds.extents.x - raycastPadding * 2) / (numberOfRaycastsPerSide.x-1), 0, 0);
            currentRaycastsPerSide = numberOfRaycastsPerSide.y;
            raycastDirection = Vector3.up;
        }
        else if (currentMovementFacing == MovementFacing.DOWN)
        {
            raycastCorner = new Vector3(bounds.min.x + raycastPadding, bounds.min.y, 0);
            perpendicularDeltaBetweenRaycasts = new Vector3((2*bounds.extents.x - raycastPadding * 2) / (numberOfRaycastsPerSide.x-1), 0, 0);
            currentRaycastsPerSide = numberOfRaycastsPerSide.y;
            raycastDirection = Vector3.down;
        }
        else if (currentMovementFacing == MovementFacing.LEFT)
        {
            raycastCorner = new Vector3(bounds.min.x, bounds.min.y + raycastPadding, 0);
            perpendicularDeltaBetweenRaycasts = new Vector3(0, (2 * bounds.extents.y - raycastPadding * 2) / (numberOfRaycastsPerSide.y-1), 0);
            currentRaycastsPerSide = numberOfRaycastsPerSide.x;
            raycastDirection = Vector3.left;
        }
        else if (currentMovementFacing == MovementFacing.RIGHT)
        {
            raycastCorner = new Vector3(bounds.max.x, bounds.min.y + raycastPadding, 0);
            perpendicularDeltaBetweenRaycasts = new Vector3(0, (2*  bounds.extents.y - raycastPadding * 2) / (numberOfRaycastsPerSide.y-1), 0);
            currentRaycastsPerSide = numberOfRaycastsPerSide.x;
            raycastDirection = Vector3.right;
        }

        List<GameObject> objectsIAmFacing = new List<GameObject>();
        for (int raycastCount = 0; raycastCount<currentRaycastsPerSide; raycastCount++)
        {
            Vector2 origin = raycastCorner + perpendicularDeltaBetweenRaycasts * raycastCount;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, raycastDirection, raycastDistance, objectLayers);
            Debug.DrawRay(origin, raycastDirection * raycastDistance, Color.blue);
            foreach (RaycastHit2D hit in hits)
            {
                if (objectsIAmFacing.Contains(hit.transform.gameObject) == false)
                {
                    objectsIAmFacing.Add(hit.transform.gameObject);
                }
            }
        }
        return objectsIAmFacing;
    }
}
