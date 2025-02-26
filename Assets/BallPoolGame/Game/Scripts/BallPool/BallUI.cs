using UnityEngine;

public class BallUI : MonoBehaviour
{
    [SerializeField] private int id;
    public int ID => id;

    [SerializeField] private GameObject colorNull;

    public void SetNull()
    {
        colorNull.SetActive(true);
    }

    public void Restart()
    {
        colorNull.SetActive(false);
    }
}
