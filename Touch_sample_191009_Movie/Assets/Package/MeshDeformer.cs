using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f; // 弾性力？　元：20f
	public float damping = 5f;      // 減衰   元：5f

	public Mesh deformingMesh;  //変形するメッシュ
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

    public GameObject Device_Tip;

    //[SerializeField]
    //CubeSphere cube;    //hirose 190801

    float uniformScale = 1f;    //均一のスケール?
    /* メッシュとその頂点を取得し，元の頂点を変形した頂点にコピー */
    void Start () {
		deformingMesh = GetComponent<MeshFilter>().mesh;    //メッシュの取得
		originalVertices = deformingMesh.vertices;          //元の頂点
		displacedVertices = new Vector3[originalVertices.Length];   //表示される頂点
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];             //表示される頂点を元の頂点に
		}
		vertexVelocities = new Vector3[originalVertices.Length];    //頂点速度
	}

	void Update () {
		uniformScale = transform.localScale.x;  //親のオブジェクトから見た相対的なスケール
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);    // 頂点の更新
		}
		deformingMesh.vertices = displacedVertices; //変形するメッシュの頂点
		deformingMesh.RecalculateNormals();         //メッシュの法線を再計算
        //cube.meshcollider.sharedMesh = deformingMesh;   //hirose 190801
	}
    //頂点を更新する   いじったらメッシュが動く
	void UpdateVertex (int i) {
        //Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
        //Vector3 displacement_device = device_tip - originalVertices[i];     // hirose 190822 デバイスとオブジェクトの変位
        Vector3 velocity = vertexVelocities[i];     //頂点速度
		Vector3 displacement = displacedVertices[i] - originalVertices[i];  //変位    //編集？ 元：Vector3 displacement = displacedVertices[i] - originalVertices[i]
        displacement *= uniformScale;   //変異の値を相対的に？
		velocity -= displacement * springForce * Time.deltaTime;    //速度から変異*弾性力*最後のフレームを完了するのに要した時間の値を引く   //編集？   元：velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;                  //速度に1-減衰*最後のフレームを完了するのに要した時間の値を掛ける        //編集？   元：velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity; //計算した値を頂点速度に
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);  //表示される頂点に速度*(最後のフレームを完了するのに要した時間の値/均一のスケール?)を足す
    }
    //変形する力を加える
	public void AddDeformingForce (Vector3 point, float force) {
        Debug.Log("point" + point);
        Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
        //Vector3 displacement_device = device_tip - point;     // hirose 190822 デバイスとオブジェクトの変位
        float displacement_device = device_tip.magnitude - point.magnitude;
        //force = force / (1f + displacement_device);   // hirose 190829
        //force = (displacement_device - 3f) * 10f;
        Debug.Log("差" + displacement_device);
        Debug.Log("force" + force);
        point = transform.InverseTransformPoint(point);     //Rayの接触点のワールド座標をローカル座標に変更
       
        
        for (int i = 0; i < displacedVertices.Length; i++) {
            //Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
            //Vector3 displacement_device = device_tip - originalVertices[i];     // hirose 190822 デバイスとオブジェクトの変位
            //if (vertexVelocities[i].magnitude >= device_tip.magnitude || originalVertices[i].magnitude <= device_tip.magnitude) //hirose 190822
            //{
            AddForceToVertex(i, point, force);  //頂点に力を加える
            //}
		}
	}
    //頂点に力を加える
	void AddForceToVertex (int i, Vector3 point, float force) {
        //Vector3 device_tip = Device_Tip.transform.position;     // hirose 190822    デバイスの先端の座標を取得
        //Vector3 displacement_device = device_tip - originalVertices[i];     // hirose 190822 デバイスとオブジェクトの変位
        Vector3 pointToVertex = displacedVertices[i] - point;   //表示される頂点-接触点   頂点ごとの変形力の方向と距離

        //force = force / (1f + displacement_device.magnitude);    // hirose 190829

        pointToVertex *= uniformScale;  //頂点と接触点の差を均一化？
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude); //減衰力 = 元の力　/ (1+pointToVertexの2乗の長さ)（力を距離の2乗で割る）元：float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;      //速度（Δv） = 減衰力 * 最後のフレームを完了するのに要した時間の値　ここ編集？    元：float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity; //頂点速度　+= 正規化されたpointToVertex * 速度  ベクトルの方向がわかる（正規化により）
    }
}