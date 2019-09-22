using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIK : MonoBehaviour {
    private Vector3 rightFootPosition, leftFootPosition, rightFootIkPosition, leftFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastHipPositionY, lastLeftFootPositionY, lastRightFootPositionY;

    [Range(0,2)][SerializeField]
    private float stepUpRaycastDistance = 0.55f;
    [Range(0,2)][SerializeField]
    private float stepDownRaycastDistance = 0.55f;
    [SerializeField] private LayerMask environmentLayer;

    [SerializeField]
    private float hipOffset = 0f;
    [Range(0,1)][SerializeField]
    private float hipIkSpeed = 0.28f;
    [Range(0,1)][SerializeField]
    private float footIkSpeed = 0.18f;

    public string leftFootRotationCurve = "RotateLeftFoot";
    public string rightFootRotationCurve = "RotateRightFoot";

    public bool showDebugLines = true;

    Animator anim;

    void Start() {
        anim = this.GetComponent<Animator>();
    }

    private void FixedUpdate() {
        FindFootTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        FindFootTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation);
        FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation);
    }

    private void OnAnimatorIK(int layerIndex) {

        MoveHipHeight();

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootRotationCurve));

        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootRotationCurve));

        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
    }

    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY) {
        Vector3 targetIkPosition = anim.GetIKPosition(foot);

        if (positionIkHolder != Vector3.zero) {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, footIkSpeed);
            targetIkPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);
            
            anim.SetIKRotation(foot, rotationIkHolder);
        }

        anim.SetIKPosition(foot, targetIkPosition);
    }

    private void MoveHipHeight() {
        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastHipPositionY == 0) {
            lastHipPositionY = anim.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(lastHipPositionY, newPelvisPosition.y, hipIkSpeed);

        anim.bodyPosition = newPelvisPosition;

        lastHipPositionY = anim.bodyPosition.y;
    }

    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 footIkPosition, ref Quaternion footIkRotation) {
        RaycastHit feetOutHit;

        if (showDebugLines)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (stepDownRaycastDistance + stepUpRaycastDistance), Color.yellow);
    
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, stepDownRaycastDistance + stepUpRaycastDistance, environmentLayer)) {
            footIkPosition = fromSkyPosition;
            footIkPosition.y = feetOutHit.point.y + hipOffset;
            footIkRotation = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }

        footIkPosition = Vector3.zero;
    }

    private void FindFootTarget(ref Vector3 footPosition, HumanBodyBones foot) {
        footPosition = anim.GetBoneTransform(foot).position;
        footPosition.y = transform.position.y + stepUpRaycastDistance;
    }
}
