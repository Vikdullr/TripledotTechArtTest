using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Graphic))]
public class NormalizedRectUVs : MonoBehaviour, IMeshModifier
{
    private Graphic m_Graphic;
    private RectTransform m_RectTransform;

    protected void Awake()
    {
        m_Graphic = GetComponent<Graphic>();
        m_RectTransform = GetComponent<RectTransform>();
    }

    protected void OnEnable()
    {
        if (m_Graphic != null)
        {
            m_Graphic.SetVerticesDirty();
        }
    }

    protected void OnDisable()
    {
        if (m_Graphic != null)
        {
            m_Graphic.SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    protected void OnValidate()
    {
        if (m_Graphic == null) m_Graphic = GetComponent<Graphic>();
        if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();

        if (m_Graphic != null)
        {
            m_Graphic.SetVerticesDirty();
        }
    }
#endif

    public void ModifyMesh(VertexHelper vh)
    {
        if (!enabled || m_RectTransform == null || m_Graphic == null)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        if (vertices.Count == 0)
            return;

        Rect rect = m_RectTransform.rect;
        float rWidth = rect.width;
        float rHeight = rect.height;

        if (Mathf.Approximately(rWidth, 0f)) rWidth = 1f;
        if (Mathf.Approximately(rHeight, 0f)) rHeight = 1f;

        UIVertex tempVertex = new UIVertex();
        for (int i = 0; i < vertices.Count; i++)
        {
            tempVertex = vertices[i];

            float normalizedX = (tempVertex.position.x - rect.xMin) / rWidth;
            float normalizedY = (tempVertex.position.y - rect.yMin) / rHeight;

            tempVertex.uv3 = new Vector2(normalizedX, normalizedY);
            vertices[i] = tempVertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);

    }

    public void ModifyMesh(Mesh mesh)
    {
        if (!enabled || m_RectTransform == null) return;

        Vector3[] meshVertices = mesh.vertices;
        List<Vector2> newUVs = new List<Vector2>(meshVertices.Length);

        Rect rect = m_RectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        if (width == 0) width = 1f;
        if (height == 0) height = 1f;

        for (int i = 0; i < meshVertices.Length; i++)
        {
            float normalizedX = (meshVertices[i].x - rect.xMin) / width;
            float normalizedY = (meshVertices[i].y - rect.yMin) / height;
            newUVs.Add(new Vector2(normalizedX, normalizedY));
        }

        mesh.SetUVs(3, newUVs);

    }
}