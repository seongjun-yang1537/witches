using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class InfluenceMapVisualizer : MonoBehaviour
{
    [Header("������ �׸���")]
    public InfluenceGrid grid;

    [Header("����� �ּ� �Ӱ�ġ (��)")]
    [Tooltip("�� �� �̳��� netInfluence�� ���� ó���˴ϴ�.")]
    public float epsilon = 0.1f;

    [Header("���� ����")]
    public Color redColor = new Color(1, 0, 0, 0.5f);
    public Color blueColor = new Color(0, 0, 1, 0.5f);

    private Texture2D tex;
    private MeshRenderer mr;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        // �׸��� ũ�⿡ ���� �ؽ�ó ����
        tex = new Texture2D(grid.cols, grid.rows)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        // ���� ���̴� ���
        mr.material = new Material(Shader.Find("Unlit/Transparent"));
        mr.material.mainTexture = tex;
    }

    void Update()
    {
        // �� ����
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
                // ���⸸ �ٲ��ּ���: red - blue �� ���
                float val = red[x, y] - blue[x, y];

                Color c;
                if (val > epsilon)
                {
                    // Red �켼
                    float t = Mathf.InverseLerp(epsilon, 1f, val);
                    c = new Color(redColor.r, redColor.g, redColor.b, redColor.a * t);
                }
                else if (val < -epsilon)
                {
                    // Blue �켼
                    float t = Mathf.InverseLerp(-epsilon, -1f, val);
                    c = new Color(blueColor.r, blueColor.g, blueColor.b, blueColor.a * t);
                }
                else
                {
                    // �߸� ���� ����
                    c = Color.clear;
                }

                tex.SetPixel(x, h - 1 - y, c);
            }
        }

        tex.Apply();
    }
}
