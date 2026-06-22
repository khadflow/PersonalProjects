using UnityEngine;

namespace StarterAssets
{
    public class AttackColliders
    {
        private Collider RightFist;
        private Collider LeftFist;

        private Collider RightLeg;
        private Collider LeftLeg;

        private float RightFistTimeout;
        private float RightLegTimeout;

        private float LeftFistTimeout;
        private float LeftLegTimeout;

        private float Timeout = 0.8f;

        public AttackColliders(Animator animator)
        {
            // Search the entire transform hierarchy under the animator for the correct joint bones
            Transform rightHandBone = FindBoneUniversal(animator.transform, "RightHand");
            Transform leftHandBone = FindBoneUniversal(animator.transform, "LeftHand");
            Transform rightFootBone = FindBoneUniversal(animator.transform, "RightLeg");
            Transform leftFootBone = FindBoneUniversal(animator.transform, "LeftLeg");

            // Extract the colliders seamlessly from whatever child object holds your script
            if (rightHandBone != null)
            {
                RightFist = GetColliderFromTriggerScript(rightHandBone);
            }

            if (leftHandBone != null)
            {
                LeftFist = GetColliderFromTriggerScript(leftHandBone);
            }

            if (rightFootBone != null)
            {
                RightLeg = GetColliderFromTriggerScript(rightFootBone);
            }

            if (leftFootBone != null)
            {
                LeftLeg = GetColliderFromTriggerScript(leftFootBone);
            }

            // Keep your existing setup code above, just add this block right before the deactivations:
            DeactivateRightFist();
            DeactivateLeftFist();
            DeactivateRightLeg();
            DeactivateRightLeg();
        }

        /// <summary>
        /// Recursively scans the hierarchy to find a bone containing our target naming phrases.
        /// </summary>
        private Transform FindBoneUniversal(Transform currentTransform, string targetBoneName)
        {
            string currentNameClean = currentTransform.name.ToLower().Replace("_", "").Replace(":", "");
            string targetClean = targetBoneName.ToLower();

            // If the bone name ends with our target (e.g. "mixamorig:righthand" ends with "righthand")
            if (currentNameClean.EndsWith(targetClean))
            {
                return currentTransform;
            }

            // Keep searching deeper down the skeletal tree
            foreach (Transform child in currentTransform)
            {
                Transform found = FindBoneUniversal(child, targetBoneName);
                if (found != null) return found;
            }

            return null;
        }

        private Collider GetColliderFromTriggerScript(Transform boneTransform)
        {
            // Loop ONLY through the immediate, first-level child objects of this bone
            foreach (Transform child in boneTransform)
            {
                AttackColliderTrigger triggerScript = child.GetComponent<AttackColliderTrigger>();
                if (triggerScript != null)
                {
                    return child.GetComponent<Collider>();
                }
            }

            // Return null safely if the specific bone doesn't have its own collider object attached
            return null;
        }

        // Activation
        public void ActivateRightFist() {
            if (RightFist != null)
            {
                RightFist.enabled = true;
                RightFistTimeout = Time.time + Timeout;
            }
        }

        public void ActivateLeftFist()
        {
            if (LeftFist != null)
            {
                LeftFist.enabled = true;
                LeftFistTimeout = Time.time + Timeout;
            }
        }

        public void ActivateRightLeg()
        {
            if (RightLeg != null)
            {
                RightLeg.enabled = true;
                RightLegTimeout = Time.time + Timeout;
            }
        }

        public void ActivateLeftLeg()
        {
            if (LeftLeg != null)
            {
                LeftLeg.enabled = true;
                LeftLegTimeout = Time.time + Timeout;
            }
        }

        // Deactivation
        public void DeactivateRightFist()
        {
            if (RightFist != null)
            {
                RightFist.enabled = false;
            }
        }

        public void DeactivateLeftFist()
        {
            if (LeftFist != null)
            {
                LeftFist.enabled = false;
            }
        }

        public void DeactivateRightLeg()
        {
            if (RightLeg != null)
            {
                RightLeg.enabled = false;
            }
        }

        public void DeactivateLeftLeg()
        {
            if (LeftLeg != null)
            {
                LeftLeg.enabled = false;
            }
        }

        // Timeouts
        public float GetRightFistTimeout()
        {
            return RightFistTimeout;
        }

        public float GetLeftFistTimeout()
        {
            return LeftFistTimeout;
        }

        public float GetRightLegTimeout()
        {
            return RightLegTimeout;
        }

        public float GetLeftLegTimeout()
        {
            return LeftLegTimeout;
        }
    }
}