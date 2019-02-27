using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class UnityExtensions
{

    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}

public class RayCastCamera : MonoBehaviour
{
    public class BouncyRay
    {
        bool hit = false;
        List<Ray> rays = new List<Ray>();
        List<float> lengths = new List<float>();
        Ray unfolded;

        public Vector2 uv;

        public BouncyRay(Ray ray)
        {
            rays.Add(ray);
        }
        public int Bounces
        {
            get
            {
                return lengths.Count;
            }
        }
        public float Length
        {
            get
            {
                float result = 0;
                foreach (float l in lengths)
                {
                    result += l;
                }
                return result;
            }
        }

        public Vector3 Origin
        {
            get
            {
                if (rays.Count > 0)
                    return rays[0].origin;
                return Vector3.one * float.NaN;
            }
        }

        public Vector3 End
        {
            get
            {
                if (hit && rays.Count > 0)
                {
                    int idx = rays.Count - 1;

                    return rays[idx].origin + rays[idx].direction * lengths[idx - 1];
                }
                return Vector3.one * float.NaN;
            }
        }

        public Ray Unfolded
        {
            get
            {
                return unfolded;
            }
        }

        void UpdateUnfoldedRay()
        {
            Ray last = rays[rays.Count - 1];
            Vector3 end = End;
            unfolded = new Ray(end, (last.origin - end).normalized * lengths[rays.Count - 2]);
        }

        public void AddBounce(Ray bounce, float length)
        {
            if (!hit)
            {
                lengths.Add(length);
                rays.Add(bounce);

                UpdateUnfoldedRay();
            }
        }
        public void AddHit(Ray bounce, float length)
        {
            if (!hit)
            {
                lengths.Add(length);
                rays.Add(bounce);
                hit = true;

                UpdateUnfoldedRay();
            }
        }

        public bool Hit
        {
            get { return hit; }
        }

        public Vector3[] GetPoints()
        {
            Vector3[] result = new Vector3[rays.Count + 1];
            for (int r = 0; r < rays.Count; ++r)
            {
                result[r] = rays[r].origin;
            }
            if (hit)
            {
                Ray last = rays[rays.Count - 1];
                result[rays.Count] = last.origin + last.direction * lengths[rays.Count - 2];
            }
            else if (lengths.Count > 1)
            {
                Ray last = rays[rays.Count - 1];
                result[rays.Count] = last.origin + last.direction * lengths[rays.Count - 2];
            }

            return result;
        }
    }

    [SerializeField] Camera otherCamera;
    [SerializeField] int size = 4;
    //[SerializeField] float maxRange = 100;

    float delta;
    public int count = 0;
    Vector2[] uv;
    Vector3[] xyz;
    BouncyRay[] rays;
    Camera _camera;

    [SerializeField] bool drawRays = false;
    [SerializeField] bool drawHits = true;
    [SerializeField] bool drawUnfold = true;
    [SerializeField] float hitRadius = 0.1f;
    [SerializeField] Color failColor = new Color(1, 1, 1, 0.25f);

    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask bounceMask;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    float coverture = 0;
    float lengthAvg = 0;
    float lengthDev = 0;
    int intersectionCount = 0;
    private void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label(new Rect(0, 0, 100, 50), "Coverture:" + (coverture * 100).ToString("0.0") + "%");
        GUI.Label(new Rect(0, 30, 100, 50), "Length:" + (lengthAvg).ToString("0.0") + "cm");
        GUI.Label(new Rect(0, 60, 100, 50), "Deviation:" + (lengthDev).ToString("0.0") + "cm");
    }

    BouncyRay physicalCenterBounce;
    public bool compute = false;
    private void OnDrawGizmos()
    {
        if (compute)
        {
            Compute();
        }

        if (drawRays || drawHits || drawUnfold)
        {
            foreach (BouncyRay ray in rays)
            {

                Vector2 uv = ray.uv;
                Gizmos.color = ray.Hit ? new Color(uv.x, uv.y, 0, 0.25f) : new Color(1, 1, 1, 0.25f);

                if (drawRays || drawHits)
                {
                    Vector3[] hits = ray.GetPoints();
                    for (int p = 1; p < hits.Length; ++p)
                    {
                        if (drawRays) Gizmos.DrawLine(hits[p - 1], hits[p]);
                        if (drawHits) Gizmos.DrawSphere(hits[p], 0.25f);
                    }
                }
                if (drawUnfold)
                {
                    Ray unfolded = ray.Unfolded;
                    Gizmos.DrawLine(unfolded.origin, unfolded.origin + unfolded.direction * ray.Length);
                }
            }
        }
        //this is the optical center
        {
            idxRayOpticalCenter = (size-1) / 2 + size*(size-1) / 2;
            BouncyRay ray = rays[idxRayOpticalCenter];
            Vector3[] hits = ray.GetPoints();
            Vector2 uv = ray.uv;
            Gizmos.color = Color.blue;

            for (int p = 1; p < hits.Length; ++p)
            {
                Gizmos.DrawLine(hits[p - 1], hits[p]);
                Gizmos.DrawSphere(hits[p], 0.25f);
            }

            Ray unfolded = ray.Unfolded;
            Gizmos.DrawLine(unfolded.origin, unfolded.origin + unfolded.direction * ray.Length);
        }
        {
            
            BouncyRay ray = physicalCenterBounce;
            Vector3[] hits = ray.GetPoints();
            Vector2 uv = ray.uv;
            Gizmos.color = Color.white;

            for (int p = 1; p < hits.Length; ++p)
            {
                Gizmos.DrawLine(hits[p - 1], hits[p]);
                Gizmos.DrawSphere(hits[p], 0.25f);
            }

            Ray unfolded = ray.Unfolded;
            Gizmos.DrawLine(unfolded.origin, unfolded.origin + unfolded.direction * ray.Length);
        }

        Gizmos.color = Color.red;
        Vector3 center = Vector3.zero;
        int intersections = 0;
        for (int r1 = 0; r1 < rays.Length; ++r1)
        {
            BouncyRay raycast1 = rays[r1];
            if (raycast1.Hit)
            {
                Ray ray1 = raycast1.Unfolded;
                for (int r2 = r1 + 1; r2 < rays.Length; ++r2)
                {
                    BouncyRay raycast2 = rays[r2];
                    if (raycast2.Hit)
                    {
                        Ray ray2 = raycast2.Unfolded;
                        Vector3 p1;
                        Vector3 p2;
                        if (Math3d.ClosestPointsOnTwoLines(out p1, out p2, ray1.origin, ray1.direction, ray2.origin, ray2.direction))
                        {
                            Gizmos.DrawLine(p1, p2);
                            center += (p1 + p2) / 2;
                            ++intersections;
                        }
                    }
                }
            }
        }
        Gizmos.DrawWireSphere(center / intersections, 0.1f);
    }

    public int idxRayOpticalCenter = 0;
    void Compute()
    {
        Awake();
        float delta = 1.0f / size;
        int lastCount = count;
        count = (size) * (size);
        if (rays == null || rays.Length != lastCount)
        {
            uv = new Vector2[count];
            xyz = new Vector3[count];
            rays = new BouncyRay[count];
        }

        int current = 0;
        Vector3 origin = this.transform.position;

        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {

                Vector2 uv = new Vector2(x, y) / (size-1);
                Ray ray = _camera.ViewportPointToRay(new Vector3(uv.x, uv.y, 1));

                rays[current] = new BouncyRay(ray)
                {
                    uv = uv
                };
                bool hit = CastRay(ray, ref rays[current]);

                ++current;
            }
        }

        // Sîmple stats
        idxRayOpticalCenter = (size - 1) / 2 + size * (size - 1) / 2;
        int hits = 0;
        lengthAvg = 0;
        foreach (BouncyRay r in rays)
        {
            if (r.Hit)
            {
                ++hits;
                lengthAvg += r.Length;
            }
        }

        coverture = ((float)hits) / count;
        lengthAvg = lengthAvg / hits;
        lengthDev = 0;
        foreach (BouncyRay r in rays)
        {
            lengthDev += Mathf.Abs(r.Length - lengthAvg);
        }

        // find the intersections
        intersectionCount = 0;
        Vector3 centroidIntersection = Vector3.zero;
        Vector3 avgCentroid = Vector3.zero;

        for (int r1 = 0; r1 < rays.Length; ++r1)
        {
            BouncyRay raycast1 = rays[r1];
            if (raycast1.Hit)
            {
                Ray ray1 = raycast1.Unfolded;
                for (int r2 = r1 + 1; r2 < rays.Length; ++r2)
                {
                    BouncyRay raycast2 = rays[r2];
                    if (raycast2.Hit)
                    {
                        Ray ray2 = raycast2.Unfolded;
                        Vector3 intersection;
                        if (Math3d.LineLineIntersection(out intersection, ray1.origin, ray1.direction, ray2.origin, ray2.direction))
                        {
                            centroidIntersection += intersection;
                            ++intersectionCount;
                        }
                    }
                }
                avgCentroid += ray1.origin;
            }
        }

        if (compute && intersectionCount > 0)
        {
            centroidIntersection /= intersectionCount;
            if (otherCamera != null)
            {
                Ray physicalCenterRay = new Ray(this.transform.position, this.transform.forward);
                physicalCenterBounce = new BouncyRay(physicalCenterRay);
                bool hit = CastRay(physicalCenterRay, ref physicalCenterBounce);

                Ray TheCenterRay = physicalCenterBounce.Unfolded;// rays[idxRayOpticalCenter].Unfolded;
                otherCamera.CopyFrom(_camera);
                otherCamera.transform.position = centroidIntersection;
                Debug.Log("" + idxRayOpticalCenter + ": " + TheCenterRay.origin);
                otherCamera.transform.rotation = Quaternion.LookRotation(TheCenterRay.origin - centroidIntersection, Vector3.down);
                Gizmos.DrawLine(TheCenterRay.origin, centroidIntersection);
            }
        }
        lengthDev /= hits;
    }

    bool CastRay(Ray ray, ref BouncyRay raycast)
    {
        bool result = false;
        if (raycast.Bounces < 5)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Rr = Ri - 2 N(Ri.N)
                Vector3 Ri = (hit.point - ray.origin).normalized;
                Vector3 reflection = Ri - 2 * hit.normal * (Vector3.Dot(Ri, hit.normal));
                float distance2 = Vector3.Distance(ray.origin, hit.point);

                // TODO: remplace with layers
                if (hit.transform.gameObject.GetComponent<ProjectionTarget>() != null)
                {
                    //length += distance2;
                    raycast.AddHit(new Ray(ray.origin, hit.point - ray.origin), distance2);
                    result = true;
                }
                // TODO: remplace with layers
                else if (hit.transform.gameObject.GetComponent<Mirror>() != null)
                {
                    raycast.AddBounce(new Ray(ray.origin, hit.point - ray.origin), distance2);
                    result = CastRay(new Ray(hit.point, reflection), ref raycast);
                }
                else
                {
                    raycast.AddBounce(new Ray(ray.origin, hit.point - ray.origin), distance2);
                }
            }
        }
        return result;
    }
}
