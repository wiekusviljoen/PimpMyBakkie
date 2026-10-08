using UnityEngine;
namespace PimpMyBakkie.Prototype
{
    public sealed class ProceduralBakkie : MonoBehaviour
    {
        Material body,dark;
        void Start(){ body=Mat(new Color(.12f,.16f,.14f)); dark=Mat(new Color(.025f,.03f,.03f)); Build(); }
        void Build()
        {
            Cube("Body",new Vector3(0,.9f,0),new Vector3(3.8f,.75f,7),body);
            Cube("Cab",new Vector3(0,1.65f,1.1f),new Vector3(3.5f,1.25f,3),body);
            Cube("Bonnet",new Vector3(0,1.35f,-2),new Vector3(3.5f,.35f,1.6f),body);
            Cube("Bed",new Vector3(0,1.3f,2.6f),new Vector3(3.6f,.8f,2.1f),body);
            for(int s=-1;s<=1;s+=2){ Wheel(new Vector3(s*1.9f,.75f,-2.15f)); Wheel(new Vector3(s*1.9f,.75f,2.15f)); }
        }
        void Wheel(Vector3 p)
        {
            var w=GameObject.CreatePrimitive(PrimitiveType.Cylinder); w.transform.SetParent(transform);
            w.transform.position=p; w.transform.rotation=Quaternion.Euler(90,0,0); w.transform.localScale=new Vector3(.85f,.35f,.85f);
            w.GetComponent<Renderer>().material=dark;
        }
        void Cube(string n,Vector3 p,Vector3 s,Material m)
        {
            var c=GameObject.CreatePrimitive(PrimitiveType.Cube); c.name=n; c.transform.SetParent(transform);
            c.transform.localPosition=p; c.transform.localScale=s; c.GetComponent<Renderer>().material=m;
        }
        Material Mat(Color c){ var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=c; return m; }
    }
}