using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class InfluenceMapVisualizer : MonoBehaviour
{
    [Header("참조할 그리드")]
    public InfluenceGrid grid;

    [Header("영향력 최소 임계치 (ε)")]
    [Tooltip("이 값 이내의 netInfluence는 투명 처리됩니다.")]
    public float epsilon = 0.1f;

    [Header("영역 색상")]
    public Color redColor = new Color(1, 0, 0, 0.5f);
    public Color blueColor = new Color(0, 0, 1, 0.5f);

    private Texture2D tex;
    private MeshRenderer mr;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        // 그리드 크기에 맞춰 텍스처 생성
        tex = new Texture2D(grid.cols, grid.rows)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        // 투명 쉐이더 사용
        mr.material = new Material(Shader.Find("Unlit/Transparent"));
        mr.material.mainTexture = tex;
    }

    void Update()
    {
        // 맵 갱신
        grid.UpdateMap();
        RenderTexture();
    }

    void RenderTexture()
    {
        var blue = grid.GetBlueInfluence();
        var red = grid.GetRedInfluence();

        int w = grid.cols;
        int h = grid.rows;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                // 여기만 바꿔주세요: red - blue 로 계산
                float val = red[x, y] - blue[x, y];

                Color c;
                if (val > epsilon)
                {
                    // Red 우세
                    float t = Mathf.InverseLerp(epsilon, 1f, val);
                    c = new Color(redColor.r, redColor.g, redColor.b, redColor.a * t);
                }
                else if (val < -epsilon)
                {
                    // Blue 우세
                    float t = Mathf.InverseLerp(-epsilon, -1f, val);
                    c = new Color(blueColor.r, blueColor.g, blueColor.b, blueColor.a * t);
                }
                else
                {
                    // 중립 영역 투명
                    c = Color.clear;
                }

                tex.SetPixel(x, h - 1 - y, c);
            }
        }

        tex.Apply();
    }
}
