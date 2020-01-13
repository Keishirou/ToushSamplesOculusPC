using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BasePosition : MonoBehaviour {
    public GameObject[] marker = new GameObject[4];
    public Material mat;

    [SerializeField]private Vector3[] vertex = new Vector3[4];
    private Mesh mesh;

    private TextAsset saveData;
    private bool flag_load;

    // Use this for initialization
    void Start () {
        mesh = new Mesh();
        flag_load = false;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3[] vec = new Vector3[2];

        if (!flag_load)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                vec[i] = marker[i].transform.position - marker[marker.Length - i - 1].transform.position;
            }

            vertex[0] = marker[2].transform.position - vec[1];
            vertex[1] = marker[0].transform.position + vec[0];
            vertex[2] = marker[3].transform.position - vec[0];
            vertex[3] = marker[1].transform.position + vec[1];
                        
            //Debug.Log("point1 = " + marker[0].transform.position);
            //Debug.Log("point2 = " + marker[1].transform.position);
            //Debug.Log("point3 = " + marker[2].transform.position);
            //Debug.Log("point4 = " + marker[3].transform.position);
        }

        mesh.vertices = new Vector3[] {
            vertex[0],
            vertex[1] ,
            vertex[2],
            vertex[3],
        };

        mesh.uv = new Vector2[] {
            new Vector2 (0, 0),
            new Vector2 (0, 1),
            new Vector2 (1, 0),
            new Vector2 (1, 1),
        };

        mesh.triangles = new int[] {
            0, 1, 2,
            1, 3, 2,
        };

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = mat;

        // yama 181016 ここでコライダー付けないと接触判定取れない
        GetComponent<MeshCollider>().sharedMesh = mesh;
        //GetComponent<MeshCollider>().sharedMaterial = physicMaterial;

        // yama 181016 現在のマーカ位置をセーブデータに書き込み
        if (Input.GetKey("s"))
        {
            //StreamWriter textFile = new StreamWriter("../Touch_sample_181015/Assets/Resources/SaveData.txt", false);// TextData.txtというファイルを新規で用意
            StreamWriter textFile = new StreamWriter(Directory.GetCurrentDirectory() + "/Assets/Resources/SaveData.txt", false);// TextData.txtというファイルを新規で用意

            for (int i = 0; i < 4; i++)
            {
                textFile.WriteLine(vertex[i].x.ToString());
                textFile.WriteLine(vertex[i].y.ToString());
                textFile.WriteLine(vertex[i].z.ToString());
            }

            textFile.Flush();// StreamWriterのバッファに書き出し残しがないか確認
            textFile.Close();// ファイルを閉じる

            Debug.Log("Save OK");
        }
        // yama 181016 セーブデータからマーカ位置を読み込み
        if (Input.GetKey("l"))
        {
            saveData = Resources.Load<TextAsset>("SaveData");
            string[] textLines = saveData.text.Split('\n');
            Debug.Log(textLines[0]);

            for(int i = 0; i < textLines.Length / 3; i++)
            {
                vertex[i].x = float.Parse(textLines[i * 3]);
                vertex[i].y = float.Parse(textLines[i * 3 + 1]);
                vertex[i].z = float.Parse(textLines[i * 3 + 2]);
            }

            flag_load = true;

            Debug.Log("Load OK");
        }
        // yama 181016 読み込んだマーカ位置をリセット
        if (Input.GetKey("r"))
        {
            flag_load = false;

            Debug.Log("Reset OK");
        }
    }
}
