using System;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "Layout", menuName = "Sign Layouts")]
public class SignLayout : ScriptableObject {
    [SerializeField] private TextAsset signJson;
    public SignMeta signage;

    public void GenerateSignage() {
        signage = JsonUtility.FromJson<SignMeta>(signJson.text);
    }
    
    [Serializable]
    public struct SignDirections {
        public string room;
        public string direction;
    }
    
    [Serializable]
    public struct SignSize {
        public float width;
        public float height;
    }

    [Serializable] public struct SignInfo {
        public int id;
        public float x;
        public float y;
        public float z;
        public SignDirections[] sign;
        public SignSize size;
    }

    [Serializable]
    public struct SignMeta {
        public string building;
        public string level;
        public SignInfo[] information;
    }
}