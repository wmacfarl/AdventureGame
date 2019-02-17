using System.Collections;
using System.Collections.Generic;

using static SimpleAnimator;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{
    public float speed;
    new Rigidbody2D rigidbody;

    bool canTakeInput = true;

    MovementFacing currentMovementFacing;
    public MovementState currentMovementState;
    CarryingState currentCarryingState;

    MovementFacing previousMovementFacing;
    MovementState previousMovementState;
    CarryingState previousCarryingState;

    SimpleAnimator animator;
    List<Collision2D> CurrentCollisions;
    new BoxCollider2D collider;
    GameObject objectIAmCarrying = null;

    public Vector2Int numberOfRaycastsPerSide;
    public float raycastPadding;
    public float raycastDistance;
    public LayerMask objectLayers;
    public float pickUpAnimationLength;
    public float putDownAnimationLength;
    public Vector2 carryOffset = new Vector2(0, 1.3f);

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<SimpleAnimator>();
        CurrentCollisions = new List<Collision2D>();
        collider = GetComponent<BoxCollider2D>();
        currentMovementState = MovementState.IDLING;
        currentMovementFacing = MovementFacing.RIGHT;
        animator.EnterAnimationState(currentMovementState, currentMovementFacing, currentCarryingState );
        animator.liftCycleDuration = pickUpAnimationLength;
        animator.placeCycleDuration = putDownAnimationLength;
    }

    void Update()
    {
        if (canTakeInput)
        {
            PerformActions(GetActionInput());
            {
                PerformMovement(GetDirectionInput());
            }
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

    IEnumerator BeginLift(GameObject objectToPickUp)
    {
        canTakeInput = false;
        currentMovementState = MovementState.LIFTING;
        yield return new WaitForSeconds(pickUpAnimationLength);
        CompleteLift(objectToPickUp);
        canTakeInput = true;
    }

    public void CompleteLift(GameObject objectToPickUp)
    {
        objectToPickUp.GetComponent<PickupableScript>().GetPickedUp(gameObject, carryOffset);
        currentCarryingState = CarryingState.CARRYING;
        currentMovementState = MovementState.IDLING;
        objectIAmCarrying = objectToPickUp;
    }

    public void PerformPrimaryAction()
    {
        if (objectIAmCarrying == null)
        {
            GameObject objectToPickUp = GetObjectIShouldPickUp();
            if (objectToPickUp != null)
            {
                StartCoroutine(BeginLift(objectToPickUp));
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

    IEnumerator BeginPlaceObject()
    {
        canTakeInput = false;
        currentMovementState = MovementState.PLACING;
        yield return new WaitForSeconds(putDownAnimationLength);
        CompletePlaceObject();
        canTakeInput = true;
    }

    void CompletePlaceObject()
    {
        float putDownDistance = GetPlaceDistance();
        objectIAmCarrying.GetComponent<PickupableScript>().GetPutDown(MovementFacingToVector2(currentMovementFacing)*putDownDistance);
        currentCarryingState = CarryingState.NOT_CARRYING;
        currentMovementState = MovementState.IDLING;
        objectIAmCarrying = null;
    }


    Vector2 MovementFacingToVector2(MovementFacing facing)
    {
        if (facing == MovementFacing.RIGHT)
        {
            return Vector2.right;
        }else if (facing == MovementFacing.LEFT)
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

    float GetPlaceDistance()
    {
        float placeDistance = 0;
        objectIAmCarrying.GetComponent<BoxCollider2D>().enabled = true; ;
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
                placeDistance = objectIAmCarrying.GetComponent<BoxCollider2D>().bounds.extents.y*2;
                Debug.Log("down ... placeDistance = " + placeDistance);
            }
        }
        objectIAmCarrying.GetComponent<BoxCollider2D>().enabled = false;
        return placeDistance *1.1f;
    }

    bool CanIPlaceObject()
    {
        return objectIAmCarrying.GetComponent<PickupableScript>().CanIBePutDown(MovementFacingToVector2(currentMovementFacing ) * GetPlaceDistance());
    }

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
            if (currentMovementState != MovementState.LIFTING && currentMovementState != MovementState.PLACING)
            {
                currentMovementState = MovementState.IDLING;
            }
        }

        if (currentMovementFacing != previousMovementFacing || currentMovementState != previousMovementState || currentCarryingState != previousCarryingState)
        {
            animator.EnterAnimationState(currentMovementState, currentMovementFacing, currentCarryingState);
        }

        previousMovementState = currentMovementState;
        previousMovementFacing = currentMovementFacing;
        previousCarryingState = currentCarryingState;
    }

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
