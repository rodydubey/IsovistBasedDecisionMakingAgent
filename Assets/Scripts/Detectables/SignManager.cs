using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SignManager : MonoBehaviour {
    [SerializeField] private GameObject signPrefab;
    [SerializeField] private SignLayout signLayout;
    [SerializeField] private GameObject directionPrefab;


    public void GenerateSigns() {
        signLayout.GenerateSignage();

        foreach (var child in transform.GetComponentsInChildren<Transform>()) {
            if(child == transform) continue;
            DestroyImmediate(child.gameObject);
        }
        
        var signs = signLayout.signage.information;
        foreach (var sign in signs) {
            var t = Instantiate(signPrefab, transform).transform;
            t.position = new Vector3(-sign.x, sign.z, -sign.y);
            t.localScale = new Vector3(sign.size.width, sign.size.height, sign.size.width);
            var s = t.GetComponent<Sign>();
            s.id = sign.id;
            foreach (var dir in sign.sign) {
                s.directions.Add(dir.direction);
                s.destinations.Add(dir.room);
            }
        }
    }

    public void GenerateDirections() {
        var i = 1;
        foreach (var child in transform.GetComponentsInChildren<Transform>().Where(x=>x != transform)) {
            //if(!child.Equals(transform)) DestroyImmediate(child.GetComponentsInChildren<Transform>().First(x => !x.Equals(child)).gameObject
            var t = Instantiate(directionPrefab, child).transform;
            //t.eulerAngles = child.eulerAngles;
            t.localScale = new Vector3(3, 3, 3);
            foreach (var dir in t.GetComponentsInChildren<Transform>().Skip(1)) {
                Debug.Log(dir.position);
                //if(dir.Equals(child)) continue;
                if (i == 1) {
                    //Handles.DrawLine(dir.position + Vector3.down, dir.position + Vector3.down * 10);
                    //Debug.DrawLine(dir.position + Vector3.down, dir.position + Vector3.down * 10);
                }
                if(!Physics.Raycast(dir.position + Vector3.down, Vector3.down, 10f, LayerMask.NameToLayer("Floor"))) {
                    //DestroyImmediate(dir.gameObject);
                }
            }

            i = 0;
        }
    }
}