﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Storm.Attributes;

namespace Storm.Flexible {
    public class Gravitating : MonoBehaviour {
        

        [SerializeField]
        private float gravitationStrength;

        [SerializeField]
        private float velocityDecay = 0.5f;

        [SerializeField]
        [ReadOnly]
        public GameObject Target;

        private Vector3 velocity;

        private Rigidbody2D rb;

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update() {

        }

        void FixedUpdate() {
            if (Target == null) {
                return;
            }

            // Factor in rigidbody physics, if need be.
            if (rb != null) {
                transform.position = transform.position + new Vector3(rb.velocity.x, rb.velocity.y, 0);

                // Phase out rigidbody velocity over time.
                rb.velocity *= velocityDecay;
            }

            transform.position = Vector3.SmoothDamp(transform.position, Target.transform.position, ref velocity, gravitationStrength);
        }
    
    
        
        public void SetTarget(GameObject target) {
            Target = target;
        }

        public void ClearTarget() {
            Target = null;
        }

        public void SetGravity(float strength) {
            gravitationStrength = strength;
        }
    }

}

