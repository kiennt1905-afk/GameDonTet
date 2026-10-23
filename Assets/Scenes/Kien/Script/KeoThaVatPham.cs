using UnityEngine;

public class KeoThaVatPham : MonoBehaviour
{
    [Header("Cấu hình vật phẩm")]
    public int idCuaBuocNay; // 0:Nhân, 1:Vỏ, 2:Sticker, 3:Dây
    public BanhChungManager manager;
    public Transform khuVucDich;     

    private Vector3 viTriBanDau;
    private float zToaDo;

    void Start()
    {
        // Lưu lại vị trí ban đầu trên bàn
        viTriBanDau = transform.position;
    }

    void OnMouseDown()
    {
        if (khuVucDich == null || Camera.main == null) return;
        zToaDo = Camera.main.WorldToScreenPoint(transform.position).z;
    }

    void OnMouseDrag()
    {
        if (khuVucDich == null || Camera.main == null) return;
        Vector3 toaDoChuot = Input.mousePosition;
        toaDoChuot.z = zToaDo;
        transform.position = Camera.main.ScreenToWorldPoint(toaDoChuot);
    }

    void OnMouseUp()
    {
        if (khuVucDich == null) return;

        float khoangCach = Vector3.Distance(transform.position, khuVucDich.position);

        // Kiểm tra xem thả có đúng chỗ, đúng tiến độ và game CÒN THỜI GIAN không
        if (manager != null && khoangCach < 1.5f && manager.tienDoHienTai == idCuaBuocNay && manager.thoiGianConLai > 0)
        {
            manager.HoanThanhBuoc(idCuaBuocNay);
        }
        
        // QUAN TRỌNG: Dù thả đúng hay thả sai, vật phẩm luôn bay về chỗ cũ để dùng lại
        transform.position = viTriBanDau;
    }
}