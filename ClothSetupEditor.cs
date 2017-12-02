//https://twitter.com/LenZ_Chu
//create by: Eugene Chu
//Free to use this as u like! :3

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class ClothSetupEditor : EditorWindow
{
    public Cloth cloth;
    public Transform rootJoint;

    public float sphereSizeOffset;

    public int listCounti;

    public SphereCollider first;
    public SphereCollider second;

    public ClothSphereColliderPair[] pair;

    // gui
    bool showPosition = false;

    [SerializeField]
    public List<string> avoidWords = new List<string>();

    // Add menu named "My Window" to the Window menu

    [MenuItem("Window/ClothSetup")]
    static void Init()
    {
        ClothSetupEditor window = (ClothSetupEditor)EditorWindow.GetWindow(typeof(ClothSetupEditor));
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Auto create & setup sphere colliders to a Cloth component", MessageType.Info);
        EditorGUILayout.Space();

        cloth = (Cloth)EditorGUILayout.ObjectField("cloth", cloth, typeof(Cloth), true);
        rootJoint = (Transform)EditorGUILayout.ObjectField("rootJoint", rootJoint, typeof(Transform), true);
        EditorGUILayout.Space();


        //-- build
        if (cloth != null && rootJoint != null)
        {
            GUI.backgroundColor = new Color(0.5f, 1f, 1f);
            if (GUILayout.Button("Build"))
            {
                listCounti = 0;
                CheckJoints(rootJoint);

                pair = new ClothSphereColliderPair[listCounti]; //now create the list after knowing how many there are

                listCounti = 0;
                //specifics

                Undo.RecordObject(rootJoint, "c");
                if (rootJoint.GetComponent<SphereCollider>() == null) rootJoint.gameObject.AddComponent<SphereCollider>();  //if child doesn't have collider. add one

                Undo.RecordObject(rootJoint.GetComponent<SphereCollider>(), "offset size");
                rootJoint.GetComponent<SphereCollider>().radius += sphereSizeOffset; //offset size

                CollectJoints(rootJoint);


                cloth.sphereColliders = pair;

                if (GUI.changed) EditorUtility.SetDirty(cloth);
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("Require 'rootJoint' transform & 'cloth' component in scene", MessageType.Warning);
        }



        //--extra settings


        showPosition = EditorGUILayout.Foldout(showPosition, "Extra Settings");
        if (showPosition)
        {
            sphereSizeOffset = EditorGUILayout.FloatField("sphereSizeOffset", sphereSizeOffset);

            //list
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ends at avoided names within the chain (Case sensitive)");
            for (int i = 0; i < avoidWords.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(300));

                avoidWords[i] = EditorGUILayout.TextField(avoidWords[i]);

                if (GUILayout.Button("X"))
                {
                    avoidWords.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(40));
            if (GUILayout.Button("+"))
            {
                avoidWords.Add("");
            }
            EditorGUILayout.EndHorizontal();
        }

        //





    }






    //============tools=============//

    void CheckJoints(Transform _tr)
    {

        for (int i = 0; i < _tr.childCount; i++)
        {
            if (HasContainWord(_tr.GetChild(i))) return;

            if (_tr.GetChild(i).childCount > 0)
            {
                CheckJoints(_tr.GetChild(i));
            }
            listCounti += 1;
        }
    }


    void CollectJoints(Transform _tr)
    {



        for (int i = 0; i < _tr.childCount; i++)
        {

            if (HasContainWord(_tr.GetChild(i))) return;

            Undo.RecordObject(_tr, "c");
            Undo.RecordObject(_tr.GetChild(i), "c");
            if (_tr.GetComponent<SphereCollider>() == null) _tr.gameObject.AddComponent<SphereCollider>(); //if child doesn't have collider. add one
            if (_tr.GetChild(i).GetComponent<SphereCollider>() == null) _tr.GetChild(i).gameObject.AddComponent<SphereCollider>();  //if child doesn't have collider. add one

            cloth.sphereColliders = cloth.sphereColliders;
            first = _tr.GetComponent<SphereCollider>();
            second = _tr.GetChild(i).GetComponent<SphereCollider>();

            ///setup specifics
            Undo.RecordObject(second, "offset size");
            second.radius += sphereSizeOffset; //offset size
            first.enabled = false;
            second.enabled = false;

            pair[listCounti] = new ClothSphereColliderPair(first, second);
            listCounti += 1;
            Debug.Log("list num " + listCounti);

            if (_tr.GetChild(i).childCount > 0)
            {

                CollectJoints(_tr.GetChild(i));


            }


        }

    }


    bool HasContainWord(Transform _trChild)
    {
        for (int i = 0; i < avoidWords.Count; i++)
        {
            if (_trChild.name == avoidWords[i])
            {
                return true;
            }

        }
        return false;
    }

}