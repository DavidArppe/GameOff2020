using UnityEngine;
using System.Collections;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Effects/Tire Screech Audio", 1)]

    //Class for playing tire screech sounds
    public class TireScreech : MonoBehaviour
    {
        public FMODUnity.StudioEventEmitter tireScreechEmitter;

        VehicleParent vp;
        Wheel[] wheels;
        float slipThreshold;
        GroundSurface surfaceType;

        void Start()
        {
            vp = transform.GetTopmostParentComponent<VehicleParent>();
            wheels = new Wheel[vp.wheels.Length];

            //Get wheels and average slip threshold
            for (int i = 0; i < vp.wheels.Length; i++)
            {
                wheels[i] = vp.wheels[i];
                if (vp.wheels[i].GetComponent<TireMarkCreate>())
                {
                    float newThreshold = vp.wheels[i].GetComponent<TireMarkCreate>().slipThreshold;
                    slipThreshold = i == 0 ? newThreshold : (slipThreshold + newThreshold) * 0.5f;
                }
            }
        }

        void Update()
        {
            float screechAmount = 0;
            bool allPopped = true;
            bool nonePopped = true;
            float alwaysScrape = 0;

            for (int i = 0; i < vp.wheels.Length; i++)
            {
                if (wheels[i].connected)
                {
                    if (Mathf.Abs(F.MaxAbs(wheels[i].sidewaysSlip, wheels[i].forwardSlip, alwaysScrape)) - slipThreshold > 0)
                    {
                        if (wheels[i].popped)
                        {
                            nonePopped = false;
                        }
                        else
                        {
                            allPopped = false;
                        }
                    }

                    if (wheels[i].grounded)
                    {
                        surfaceType = GroundSurfaceMaster.surfaceTypesStatic[wheels[i].contactPoint.surfaceType];

                        if (surfaceType.alwaysScrape)
                        {
                            alwaysScrape = slipThreshold + Mathf.Min(0.5f, Mathf.Abs(wheels[i].rawRPM * 0.001f));
                        }
                    }

                    screechAmount = Mathf.Max(screechAmount, Mathf.Pow(Mathf.Clamp01(Mathf.Abs(F.MaxAbs(wheels[i].sidewaysSlip, wheels[i].forwardSlip, alwaysScrape)) - slipThreshold), 2));
                }
            }

            //Set sound volume and pitch
            if (screechAmount > 0)
            {
                if (!tireScreechEmitter.IsPlaying())
                {
                    tireScreechEmitter.Play();
                    tireScreechEmitter.EventInstance.setVolume(0);
                }
                else
                {
                    tireScreechEmitter.EventInstance.getVolume(out float volume);
                    tireScreechEmitter.EventInstance.getPitch(out float pitch);
                    
                    tireScreechEmitter.EventInstance.setVolume(Mathf.Lerp(volume, GlobalControl.vehiclesVolumeStatic * screechAmount * ((vp.groundedWheels * 1.0f) / (wheels.Length * 1.0f)), 2 * Time.deltaTime));
                    tireScreechEmitter.EventInstance.setPitch(Mathf.Lerp(pitch, 0.6f + screechAmount * 0.3f, 2 * Time.deltaTime));
                }
            }
            else if (tireScreechEmitter.IsPlaying())
            {
                tireScreechEmitter.Stop();
                tireScreechEmitter.EventInstance.setVolume(0);
            }
        }
    }
}
