using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class iSith : MonoBehaviour
{
    private GameObject scene = null;

    private GameObject rightHandController;
    private GameObject leftHandController;
    private XRController rightXRController;
    private XRController leftXRController;

    private CollisionDetector collisionDetector;
    private GameObject selectedObject;
    private GameObject selectedObjectRotate;
    private bool rightTriggerButtonLF = false;

    private GameObject PIP;
    private Vector3 PIPpositionLF;

    private float threshold = 0.3f;

    private XRInteractorLineVisual rightRayRenderer;
    private XRInteractorLineVisual leftRayRenderer;

    private Gradient redGradient;
    private Gradient cyanGradient;
    private Gradient greenGradient;

    void Awake()
    {
        redGradient = new Gradient();
        redGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0.0f),
                new GradientColorKey(Color.red, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        cyanGradient = new Gradient();
        cyanGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.cyan, 0.0f),
                new GradientColorKey(Color.cyan, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        greenGradient = new Gradient();
        greenGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.green, 0.0f),
                new GradientColorKey(Color.green, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        PIPpositionLF = Vector3.zero;
        scene = GameObject.Find("Scene");
        PIP = GameObject.Find("PIP");
        rightHandController = GameObject.Find("RightHand Controller");
        leftHandController = GameObject.Find("LeftHand Controller");

        if (rightHandController != null)
        {
            rightXRController = rightHandController.GetComponent<XRController>();
            rightRayRenderer = rightHandController.GetComponent<XRInteractorLineVisual>();
        }

        if (leftHandController != null)
        {
            leftXRController = leftHandController.GetComponent<XRController>();
            leftRayRenderer = leftHandController.GetComponent<XRInteractorLineVisual>();
        }

        if (PIP != null)
            PIP.SetActive(false);

        collisionDetector = PIP.GetComponent<CollisionDetector>();
    }

    void Start() { }

    void Update()
    {
        Vector3 leftHandRayPoint;
        Vector3 rightHandRayPoint;

        bool notParallel = ClosestPointsOnTwoLines(
            out leftHandRayPoint,
            out rightHandRayPoint,
            rightHandController.transform.position,
            rightHandController.transform.forward,
            leftHandController.transform.position,
            leftHandController.transform.forward
        );

        if (collisionDetector.collided && selectedObject == null)
        {
            rightRayRenderer.invalidColorGradient = cyanGradient;
            leftRayRenderer.invalidColorGradient = cyanGradient;
        }
        else if (!collisionDetector.collided)
        {
            rightRayRenderer.invalidColorGradient = redGradient;
            leftRayRenderer.invalidColorGradient = redGradient;
        }

        // Check if the lines are not parallel and the distance between is smaller than threshold
        if (notParallel && Vector3.Distance(leftHandRayPoint, rightHandRayPoint) < threshold)
        {
            // Activate sphere
            PIP.SetActive(true);

            // Place the sphere in middle
            PIP.transform.position = leftHandRayPoint + (rightHandRayPoint - leftHandRayPoint) / 2;

            // Moves and drags object
            bool rightTriggerButton = false;
            rightXRController.inputDevice.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out rightTriggerButton
            );
            // Keeps y-axis zero for the dragged object
            bool rightGripButton = false;
            rightXRController.inputDevice.TryGetFeatureValue(
                CommonUsages.gripButton,
                out rightGripButton
            );
            // Rotates on z-axis
            bool leftTriggerButton = false;
            leftXRController.inputDevice.TryGetFeatureValue(
                CommonUsages.triggerButton,
                out leftTriggerButton
            );
            // Rotates on x-axis
            bool leftGripButton = false;
            leftXRController.inputDevice.TryGetFeatureValue(
                CommonUsages.gripButton,
                out leftGripButton
            );

            // If both right trigger and button are pressed, keep y-axis value zero
            if (rightTriggerButton && rightGripButton && selectedObject != null)
            {
                Vector3 pos = new Vector3(
                    selectedObject.transform.position.x,
                    0,
                    selectedObject.transform.position.z
                );
                selectedObject.transform.SetPositionAndRotation(
                    pos,
                    selectedObject.transform.rotation
                );
            }

            // If an object is being moved/dragged, change the color of the line to green
            if (rightTriggerButton && selectedObject != null)
            {
                rightRayRenderer.invalidColorGradient = greenGradient;
                leftRayRenderer.invalidColorGradient = greenGradient;
            }

            if (rightTriggerButton != rightTriggerButtonLF)
            {
                if (rightTriggerButton)
                {
                    if (collisionDetector.collided && selectedObject == null)
                    {
                        Debug.Log("Collided with Object: " + collisionDetector.collidedObject.name);
                        SelectObject(GameObject.Find(collisionDetector.collidedObject.name));
                        rightRayRenderer.invalidColorGradient = greenGradient;
                        leftRayRenderer.invalidColorGradient = greenGradient;
                    }
                }
                else if (selectedObject != null)
                {
                    DeselectObject();
                    rightRayRenderer.invalidColorGradient = redGradient;
                    leftRayRenderer.invalidColorGradient = redGradient;
                }

                rightTriggerButtonLF = rightTriggerButton;
            }
            if (leftTriggerButton)
            {
                if (collisionDetector.collided && selectedObjectRotate == null)
                {
                    selectedObjectRotate = collisionDetector.collidedObject;
                }

                if (PIPpositionLF == Vector3.zero)
                {
                    PIPpositionLF = PIP.transform.position;
                }
                else
                {
                    float xDiff = PIP.transform.position.x - PIPpositionLF.x;
                    float yDiff = PIP.transform.position.y - PIPpositionLF.y;
                    float zDiff = PIP.transform.position.z - PIPpositionLF.z;
                    Vector3 newRotation = Vector3.zero;
                    newRotation.y = xDiff * 50f + zDiff * 50f;
                    newRotation += selectedObjectRotate.transform.rotation.eulerAngles;
                    selectedObjectRotate.transform.rotation = Quaternion.Euler(newRotation);
                    PIPpositionLF = PIP.transform.position;
                }
            }
            else if (leftGripButton)
            {
                if (collisionDetector.collided && selectedObjectRotate == null)
                {
                    selectedObjectRotate = collisionDetector.collidedObject;
                }

                if (PIPpositionLF == Vector3.zero)
                {
                    PIPpositionLF = PIP.transform.position;
                }
                else
                {
                    float xDiff = PIP.transform.position.x - PIPpositionLF.x;
                    float yDiff = PIP.transform.position.y - PIPpositionLF.y;
                    float zDiff = PIP.transform.position.z - PIPpositionLF.z;
                    Vector3 newRotation = Vector3.zero;
                    newRotation.z = xDiff * 50f + zDiff * 50f;
                    newRotation += selectedObjectRotate.transform.rotation.eulerAngles;
                    selectedObjectRotate.transform.rotation = Quaternion.Euler(newRotation);
                    PIPpositionLF = PIP.transform.position;
                }
            }
            else
            {
                selectedObjectRotate = null;
                PIPpositionLF = Vector3.zero;
            }
        }
        else
        {
            PIP.SetActive(false);
        }
    }

    private void SelectObject(GameObject go, bool noYAxis = false)
    {
        selectedObject = go;
        selectedObject.transform.SetParent(collisionDetector.transform, true); // worldPositionStays = true
    }

    private void DeselectObject()
    {
        selectedObject.transform.SetParent(scene.transform, true);
        selectedObject = null;
    }

    // References:
    // https://answers.unity.com/questions/759630/point-of-intersection-between-two-rays.html
    // http://wiki.unity3d.com/index.php/3d_Math_functions
    // The link is obsolete now, but can be accessed at: https://web.archive.org/web/20210507045029/http://wiki.unity3d.com/index.php/3d_Math_functions
    public static bool ClosestPointsOnTwoLines(
        out Vector3 closestPointLine1,
        out Vector3 closestPointLine2,
        Vector3 linePoint1,
        Vector3 lineVec1,
        Vector3 linePoint2,
        Vector3 lineVec2
    )
    {
        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {
            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }
        else
        {
            return false;
        }
    }
}
