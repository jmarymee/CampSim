using UnityEngine;
using System.Collections;

	public class CarController : MonoBehaviour {

		public float TurnSpeed = 1000f;
		public float Speed = 1000f;
		private Rigidbody Car;

		void Start () 
		{
			Car = GetComponent<Rigidbody>();
		}
		
		void Update () 
		{
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			Car.AddTorque(0f,h*TurnSpeed*Time.deltaTime,0f);
			Car.AddForce(transform.forward*v*Speed*Time.deltaTime);

		}
	}
