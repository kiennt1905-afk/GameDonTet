using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // THÊM DÒNG NÀY ĐỂ SỬ DỤNG TEXT MESH PRO

public class BanhChungManager : MonoBehaviour
{
    [Header("Các vật phẩm ở mâm trên (Kết quả đang gói)")]
    public GameObject nhanKetQua;
    public GameObject voKetQua;
    public GameObject stickerKetQua;
    public GameObject dayKetQua;

    [Header("Cài đặt mâm dưới & Thành phẩm")]
    public GameObject banhChungHoanChinhPrefab; 

    [Header("UI Giao Diện")]
    public TextMeshProUGUI txtThoiGian; // Khung chữ đếm ngược
    public TextMeshProUGUI txtSoBanh;   // Khung chữ đếm số bánh

    [Header("Cài đặt Game")]
    public float thoiGianChoi = 30f; // 30 giây
    public float thoiGianConLai;
    private int soBanhDaGoi = 0;

    // Biến theo dõi tiến độ
    public int tienDoHienTai = 0; 
    private bool daXuLyHetGio = false;

    void Start()
    {
        thoiGianConLai = thoiGianChoi;
        ResetMamTren();
        
        // Hiển thị text ban đầu
        if (txtSoBanh != null)
        {
            txtSoBanh.text = "Số bánh: " + soBanhDaGoi;
        }

        // --- MỞ KHÓA CHUỘT ---
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Liên tục ép mở khóa chuột phòng trường hợp FirstPersonController khởi chạy sau và khóa lại
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Đếm ngược thời gian
        if (thoiGianConLai > 0)
        {
            thoiGianConLai -= Time.deltaTime;
            
            // Cập nhật text thời gian (Làm tròn số cho đẹp)
            if (txtThoiGian != null)
            {
                txtThoiGian.text = "Thời gian: " + Mathf.CeilToInt(thoiGianConLai) + "s";

                // --- ĐOẠN ĐỔI MÀU CHỮ ---
                if (thoiGianConLai <= 10f)
                {
                    txtThoiGian.color = Color.red; // Dưới hoặc bằng 10 giây chuyển sang màu đỏ
                }
                else
                {
                    txtThoiGian.color = Color.black; // Trên 10 giây giữ nguyên màu đen
                }
            }

            if (thoiGianConLai <= 0)
            {
                thoiGianConLai = 0;
                if (txtThoiGian != null)
                {
                    txtThoiGian.text = "HẾT GIỜ!"; // Hiện chữ hết giờ
                    txtThoiGian.color = Color.red; // Đổi sang màu đỏ
                }
                Debug.Log("HẾT GIỜ! Bạn đã gói được: " + soBanhDaGoi + " cái bánh.");

                if (!daXuLyHetGio)
                {
                    daXuLyHetGio = true;
                    StartCoroutine(QuayVeSceneNhaSau5Giay());
                }
            }
        }
    }

    private System.Collections.IEnumerator QuayVeSceneNhaSau5Giay()
    {
        yield return new WaitForSeconds(5f);

        // Đánh dấu hoàn thành Task nếu có TaskManager
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask("Chung_Cake");
        }

        // --- LƯU SỐ BÁNH (DontDestroyOnLoad) ---
        if (BanhChungScore.Instance == null)
        {
            GameObject go = new GameObject("BanhChungScore");
            go.AddComponent<BanhChungScore>();
        }
        BanhChungScore.Instance.LuuSoBanh(soBanhDaGoi);
        // ─────────────────────────────────────────

        string returnScene = PlayerPrefs.GetString("ReturnScene", "Nha");
        Debug.Log("Hết 5s -> Quay lại Scene: " + returnScene);
        SceneManager.LoadScene(returnScene);
    }

    public void HoanThanhBuoc(int buocCuaVatPham)
    {
        // Nếu đã hết giờ thì block luôn, không cho gói tiếp
        if (thoiGianConLai <= 0) return;

        if (buocCuaVatPham == 0)
        {
            if (nhanKetQua != null) nhanKetQua.SetActive(true);
            tienDoHienTai = 1;
        }
        else if (buocCuaVatPham == 1)
        {
            if (nhanKetQua != null) nhanKetQua.SetActive(false); 
            if (voKetQua != null) voKetQua.SetActive(true);
            tienDoHienTai = 2;
        }
        else if (buocCuaVatPham == 2)
        {
            if (stickerKetQua != null) stickerKetQua.SetActive(true);
            tienDoHienTai = 3;
        }
        else if (buocCuaVatPham == 3)
        {
            if (dayKetQua != null) dayKetQua.SetActive(true);
            tienDoHienTai = 4;
            
            HoanThanhMotCaiBanh();
        }
    }

    void HoanThanhMotCaiBanh()
    {
        soBanhDaGoi++;
        
        // Cập nhật lại UI số bánh
        if (txtSoBanh != null)
        {
            txtSoBanh.text = "Số bánh: " + soBanhDaGoi;
        }
        
        // 1. Phân loại cột và tầng
        int cot = (soBanhDaGoi % 2 == 0) ? 1 : 0;
        int tang = (soBanhDaGoi - 1) / 2;

        // 2. Tọa độ xếp bánh chuẩn bạn đã lấy
        float toaDoX = (cot == 0) ? 6.277f : 6.897f;
        float toaDoY = -55.713f + (tang * 0.164f);

        // 3. Sinh ra bánh
        Vector3 viTriSpawn = new Vector3(toaDoX, toaDoY, -39.08f);
        if (banhChungHoanChinhPrefab != null)
        {
            Instantiate(banhChungHoanChinhPrefab, viTriSpawn, Quaternion.identity);
        }

        // Lập tức reset mâm trên
        ResetMamTren();
    }

    void ResetMamTren()
    {
        tienDoHienTai = 0;
        if (nhanKetQua != null) nhanKetQua.SetActive(false);
        if (voKetQua != null) voKetQua.SetActive(false);
        if (dayKetQua != null) dayKetQua.SetActive(false);
        if (stickerKetQua != null) stickerKetQua.SetActive(false);
    }
}