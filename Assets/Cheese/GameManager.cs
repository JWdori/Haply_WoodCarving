using System.Linq;
using System.Threading.Tasks;
using BzKovSoft.ObjectSlicer;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    bool _inProgress;
    GameObject _slice;
    Material[] _materials;
    float _pointY;

    public Transform _box;
    public Transform _katana;

    public async Task Cut(GameObject target)
    {
        var sliceable = target.GetComponent<IBzMeshSlicer>();
        if (sliceable == null)
        {
            return;
        }

        Plane plane = new Plane(Vector3.right, 0f);
        var result = await sliceable.SliceAsync(plane);

        if (!result.sliced)
        {
            return;
        }

        _inProgress = true;
        _pointY = float.MaxValue;
        _slice = result.resultObjects.First(_ => _.side).gameObject; // there should be per one part for each side for a simple cube
        _box = result.resultObjects.First(_ => !_.side).gameObject.transform;
        var meshFilter = _slice.GetComponent<MeshFilter>();
        float centerX = meshFilter.sharedMesh.bounds.center.x;

        _materials = _slice.GetComponent<MeshRenderer>().materials;
        foreach (var material in _materials)
        {
            material.SetFloat("_PointX", centerX);
        }
    }

    public GameManager()
    {
        instance = this;
    }

    public void MoveBox(float x)
    {
        var pos = _box.position;
        pos.x = x;
        _box.position = pos;
    }

    public void MoveKatana(float y)
    {
        var pos = _katana.position;
        pos.y = y;
        _katana.position = pos;

        if (!_inProgress)
        {
            return;
        }

        float pointY = _slice.transform.InverseTransformPoint(pos).y;
        if (_pointY > pointY)
        {
            _pointY = pointY;
        }

        foreach (var material in _materials)
        {
            material.SetFloat("_PointY", _pointY);
        }

        if (y <= 0f)
        {
            _slice.GetComponent<Rigidbody>().isKinematic = false;
            _inProgress = false;
        }
    }
}
