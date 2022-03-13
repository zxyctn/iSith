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

	public Material hoverMaterial;
	public Material hightlightedMaterial;
	public Material defaultMaterial;

	private float threshold = 0.1f;

	// Start is called before the first frame update

	void Awake()
	{
		PIPpositionLF = Vector3.zero;
		scene = GameObject.Find("Scene");
		rightHandController = GameObject.Find("RightHand Controller");
		leftHandController = GameObject.Find("LeftHand Controller");

		if (rightHandController != null)
			rightXRController = rightHandController.GetComponent<XRController>();

		if (leftHandController != null)
			leftXRController = leftHandController.GetComponent<XRController>();

		PIP = GameObject.Find("PIP");

		if (PIP != null)
			PIP.SetActive(false);

		collisionDetector = PIP.GetComponent<CollisionDetector>();
		PIP.SetActive(false);
	}

	void Start()
	{
		//PIP.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 leftHandRayPoint;
		Vector3 rightHandRayPoint;

		bool notParallel = ClosestPointsOnTwoLines(
			out leftHandRayPoint, out rightHandRayPoint,
			rightHandController.transform.position, rightHandController.transform.forward,
			leftHandController.transform.position, leftHandController.transform.forward
		);

		if (collisionDetector.collided && selectedObject == null)
			PIP.GetComponent<MeshRenderer>().material = hoverMaterial;
		else if (!collisionDetector.collided)
			PIP.GetComponent<MeshRenderer>().material = defaultMaterial;

		if (notParallel)
		{
			float dist = Vector3.Distance(leftHandRayPoint, rightHandRayPoint);

			if (dist < threshold)
			{
				PIP.SetActive(true);
				PIP.transform.position = leftHandRayPoint + (rightHandRayPoint - leftHandRayPoint) / 2;
			}

			bool rightTriggerButton = false;
			rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerButton);

			bool leftTriggerButton = false;
			leftXRController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerButton);

			bool leftGripButton = false;
			leftXRController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out leftGripButton);

			if (rightTriggerButton != rightTriggerButtonLF)
			{
				if (rightTriggerButton)
				{
					if (collisionDetector.collided && selectedObject == null)
					{
						Debug.Log("Collided with Object: " + collisionDetector.collidedObject.name);
						SelectObject(GameObject.Find(collisionDetector.collidedObject.name));
						PIP.GetComponent<MeshRenderer>().material = hightlightedMaterial;
					}

				}
				else if (selectedObject != null)
				{
					DeselectObject();
					PIP.GetComponent<MeshRenderer>().material = defaultMaterial;
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
					newRotation.y = xDiff * 100f + zDiff * 100f;
					newRotation += selectedObjectRotate.transform.rotation.eulerAngles;
					selectedObjectRotate.transform.rotation = Quaternion.Euler(newRotation);
					//Debug.Log("xDiff:" + xDiff);
					//Debug.Log("yDiff:" + yDiff);
					//Debug.Log("zDiff:" + zDiff);
					//if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
					//{
					//	Vector3 newRotation = Vector3.zero;
					//	newRotation.y = xDiff * 500f;
					//	newRotation += selectedObjectRotate.transform.rotation.eulerAngles;
					//	selectedObjectRotate.transform.rotation = Quaternion.Euler(newRotation);
					//}
					//else
					//{
					//	Vector3 newRotation = Vector3.zero;
					//	newRotation.x = yDiff * 500f;
					//	newRotation += selectedObjectRotate.transform.rotation.eulerAngles;
					//	selectedObjectRotate.transform.rotation = Quaternion.Euler(newRotation);
					//}
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
					Debug.Log("xDiff:" + xDiff);
					Debug.Log("yDiff:" + yDiff);
					Debug.Log("zDiff:" + zDiff);
					Vector3 newRotation = Vector3.zero;
					newRotation.z = xDiff * 100f + zDiff * 100f;
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


	private void SelectObject(GameObject go)
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
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
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
