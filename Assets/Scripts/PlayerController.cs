using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    bool isMoving;
    bool isCrouching = false;
    bool isRunning = false;
    
    float vertical;
    float horizontal;
    public float acceleration = 5f;
    float movementMagnitude;

    public float rotationSpeed = 0.1f;

    Vector3 forward;

    public float vaultDst;

    public bool isGrounded = false;
    bool isFalling = false;

    public float gravity = 9.8f;

    public float stepHeight = 0.55f;
    public float stepSpeed = 0.12f;

    float dstToGround;

    Animator anim;

    void Start() {
        anim = this.GetComponent<Animator>();
    }

    void Update() {
        isGrounded = checkIsGrounded();
        if (isGrounded) {
            if (isFalling){
                anim.SetTrigger("Landing");
            }
            isFalling = false;
        }

        HandleMovementInput();
    }

    void FixedUpdate() {
        PseudoGravity();
    }

    public void HandleMovementInput() {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        isRunning = (Input.GetKey(KeyCode.LeftShift)) ? true : false;
        vertical = (isRunning) ? Input.GetAxis("Vertical") : Mathf.Clamp(Input.GetAxis("Vertical"), -0.5f, 0.5f);
        horizontal = (isRunning) ? Input.GetAxis("Horizontal") : Mathf.Clamp(Input.GetAxis("Horizontal"), -0.5f, 0.5f);
        movementMagnitude = new Vector2(vertical, horizontal).sqrMagnitude;
        isMoving = (movementMagnitude != 0) ? true : false;

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        if (movementMagnitude > 0) {
            Vector3 targetRotation = (vertical > 0) ? forward * vertical : forward * -vertical;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), rotationSpeed);
        }

        HandleMovementInteractions();
        CallMovementAnimation();
    }

    void HandleMovementInteractions() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            anim.SetTrigger("Jump");
        }
    }

    void CallMovementAnimation() {
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isFalling", isFalling);
        anim.SetFloat("Vertical", vertical, 0.5f, acceleration);
        anim.SetFloat("Horizontal", horizontal, 0.5f, acceleration);
    }

    void PseudoGravity() {
        Vector3 targetPosition = transform.position;

        if (FindFloor() < targetPosition.y && !isGrounded) {
            transform.position += (-Vector3.up * gravity) / 50f;
            if (dstToGround > stepHeight)
                isFalling = true;
            return;
        }
        if (FindFloor() > targetPosition.y) {
            targetPosition.y = FindFloor();
            transform.position = Vector3.Lerp(transform.position, targetPosition, stepSpeed);
        }
    }

    bool checkIsGrounded() {
        Vector3 origin = transform.position;
        origin.y += 1f;

        RaycastHit hit;
        if (Physics.Raycast(origin, -Vector3.up, out hit, 1f)) {
            return true;
        }
        else 
            return false;
    }

    float FindFloor() {
        Vector3 origin = transform.position;
        float upperValue = 0;
        float lowerValue = 0;

        RaycastHit hit;
        if(Physics.Raycast(origin + new Vector3(0, stepHeight, 0), -Vector3.up, out hit)) {
            upperValue = hit.distance;
        }
        if(Physics.Raycast(origin, -Vector3.up, out hit)) {
            dstToGround = hit.distance;
            lowerValue = hit.distance;
        }

        if (upperValue - stepHeight < 0)
            return origin.y + (stepHeight - upperValue);
        else 
            return origin.y - lowerValue;
    }
}
