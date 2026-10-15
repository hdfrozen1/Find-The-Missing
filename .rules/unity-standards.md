# QUY CHUẨN VIẾT CODE C# UNITY

## 1. Quy tắc đặt tên (Naming Conventions)
- **PascalCase**: Dành cho tên Lớp (Class), Tên hàm (Method), và các biến Public (ví dụ: `PlayerController`, `TakeDamage()`, `CurrentHealth`).
- **camelCase**: Dành cho biến cục bộ và tham số hàm (ví dụ: `moveSpeed`, `damageAmount`).
- **_camelCase (có dấu gạch dưới)**: Dành cho các biến Private được khai báo ở đầu Class (ví dụ: `_rb`, `_isGrounded`).

## 2. Quy tắc tối ưu hiệu năng (Performance Optimization)
- KHÔNG sử dụng `GameObject.Find()` hoặc `GetComponent()` trong hàm `Update()`. Tất cả phải được lưu bộ nhớ đệm (Cache) ở hàm `Awake()` hoặc `Start()`.
- KHÔNG sử dụng chuỗi văn bản (String) để so sánh Tag, hãy dùng `CompareTag("Player")` thay vì `tag == "Player"`.
- Hãy sử dụng `Vector3.zero` thay vì `new Vector3(0,0,0)`.
- Sử dụng Object Pooling cho mọi đối tượng sinh ra/hủy đi liên tục (đạn, effect, enemy).
- Caching: Luôn cache các tham chiếu (GetComponent, Camera.main, Transform) tại Awake/Start.
- Data Structures: Chọn cấu trúc dữ liệu phù hợp (Array/List vs Dictionary) để tối ưu CPU. Sử dụng Struct thay vì Class cho các dữ liệu nhỏ, tồn tại ngắn hạn.
- Physics: Sử dụng `Layers collision matrix`, hạn chế `MeshCollider`, ưu tiên `Primitive Colliders`.


## 3. Cấu trúc một File Script chuẩn
Mọi file script bạn sinh ra phải có cấu trúc theo thứ tự từ trên xuống dưới như sau:
1. Biến Private có thuộc tính `[SerializeField]` (Để chỉnh được trên Inspector).
2. Biến Private nội bộ.
3. Biến Public (nếu có).
4. Các hàm vòng đời Unity (`Awake`, `Start`, `Update`).
5. Các hàm logic xử lý riêng.
6. Luôn sử dụng `RequireComponent` nếu class phụ thuộc vào component khác.
7. Sử dụng [Tooltip] và [Header] để Inspector dễ nhìn.