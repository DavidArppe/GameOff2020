using UnityEngine;
using System.Collections;

namespace RVP
{
    //Class for engines
    public abstract class Motor : MonoBehaviour
    {
        protected VehicleParent vp;
        public bool ignition;
        public float power = 1;

        [Tooltip("Throttle curve, x-axis = input, y-axis = output")]
        public AnimationCurve inputCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        protected float actualInput;//Input after applying the input curve

        [Header("Nitrous Boost")]
        public bool canBoost = true;
        [System.NonSerialized]
        public bool boosting;
        public float boost = 1;
        bool boostReleased;
        bool boostPrev;

        [Tooltip("X-axis = local z-velocity, y-axis = power")]
        public AnimationCurve boostPowerCurve = AnimationCurve.EaseInOut(0, 0.1f, 50, 0.2f);
        public float maxBoost = 1;
        public float boostBurnRate = 0.01f;
        public ParticleSystem[] boostParticles;

        [Header("Damage")]

        [Range(0, 1)]
        public float strength = 1;
        [System.NonSerialized]
        public float health = 1;
        public float damagePitchWiggle;
        public ParticleSystem smoke;
        float initialSmokeEmission;

        public virtual void Start()
        {
            vp = transform.GetTopmostParentComponent<VehicleParent>();

            if (smoke)
            {
                initialSmokeEmission = smoke.emission.rateOverTime.constantMax;
            }
        }

        public virtual void FixedUpdate()
        {
            health = Mathf.Clamp01(health);

            //Boost logic
            boost = Mathf.Clamp(boosting ? boost - boostBurnRate * Time.timeScale * 0.05f * TimeMaster.inverseFixedTimeFactor : boost, 0, maxBoost);
            boostPrev = boosting;

            if (canBoost && ignition && health > 0 && !vp.crashing && boost > 0 && (vp.hover ? vp.accelInput != 0 || Mathf.Abs(vp.localVelocity.z) > 1 : vp.accelInput > 0 || vp.localVelocity.z > 1))
            {
                if (((boostReleased && !boosting) || boosting) && vp.boostButton)
                {
                    boosting = true;
                    boostReleased = false;
                }
                else
                {
                    boosting = false;
                }
            }
            else
            {
                boosting = false;
            }

            if (!vp.boostButton)
            {
                boostReleased = true;
            }

            // TODO: FMOD Boost Sound?
            // if (boostLoopSnd && boostSnd)
            // {
            //     if (boosting && !boostLoopSnd.isPlaying)
            //     {
            //         boostLoopSnd.Play();
            //     }
            //     else if (!boosting && boostLoopSnd.isPlaying)
            //     {
            //         boostLoopSnd.Stop();
            //     }
            // 
            //     if (boosting && !boostPrev)
            //     {
            //         boostSnd.clip = boostStart;
            //         boostSnd.Play();
            //     }
            //     else if (!boosting && boostPrev)
            //     {
            //         boostSnd.clip = boostEnd;
            //         boostSnd.Play();
            //     }
            // }
        }

        public virtual void Update()
        {
            if (smoke)
            {
                ParticleSystem.EmissionModule em = smoke.emission;
                em.rateOverTime = new ParticleSystem.MinMaxCurve(health < 0.7f ? initialSmokeEmission * (1 - health) : 0);
            }
        }
    }
}
