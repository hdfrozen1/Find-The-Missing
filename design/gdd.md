# GDD — Find The Missing

## 1. Tổng quan

**Tên tạm:** Find The Missing

**Thể loại:** 3D Observation / Puzzle

**Nền tảng mục tiêu:** Android

## Technology Stack

**Engine:** Unity 6 (6000.x) trở lên

**Render Pipeline:** URP (Universal Render Pipeline)

**Input:** Unity Input System Package

**Camera:** Perspective Camera, 2 camera split-screen bằng Viewport Rect

**Physics / Interaction:** Unity Physics 3D + Collider + Physics.Raycast

**Target orientation:** Portrait (màn hình dọc)

**Core gameplay:**

Người chơi quan sát hai thế giới 3D gần như giống nhau được hiển thị song song. Mỗi bên có một hoặc nhiều vật thể mà bên còn lại không có. Người chơi xoay camera 360° để quan sát môi trường và tìm ra vật bị thiếu.

Khác với game "Spot the Difference", game không yêu cầu tìm những thay đổi rất nhỏ như màu sắc, texture hoặc chi tiết trên vật thể. Điểm khác biệt chủ yếu là **một vật thể hoàn chỉnh xuất hiện ở một bên nhưng không xuất hiện ở bên kia**.

Ví dụ:

```text
LEFT                         RIGHT

House                        House
Tree                         Tree
Table                        Table
Chair                        -
Lamp                         Lamp
```

Người chơi phải nhận ra rằng **Chair** chỉ tồn tại ở bên trái.

---

# 2. Mục tiêu thiết kế

## 2.1. Trải nghiệm chính

Game tập trung vào:

- Quan sát.
- Ghi nhớ vị trí tương đối của vật thể.
- So sánh hai thế giới.
- Xoay camera để tìm kiếm.
- Cảm giác "À, bên kia thiếu cái này!"

## 2.2. Nguyên tắc thiết kế

### Không cần zoom

Các vật thể khác nhau phải đủ lớn để người chơi có thể nhận biết bằng mắt thường.

Camera có khoảng cách cố định hoặc FOV cố định.

Không sử dụng pinch-to-zoom trong gameplay cơ bản.

### Có thể xoay 360°

Người chơi có thể quan sát môi trường từ mọi hướng:

- Xoay trái/phải 360°.
- Xoay lên/xuống.
- Có thể giới hạn góc nhìn dọc nếu cần để tránh camera quay quá mức.

### Điểm khác nhau rõ ràng

Không ưu tiên các khác biệt quá nhỏ.

Ví dụ phù hợp:

- Một chiếc ghế bị thiếu.
- Một cái cây bị thiếu.
- Một chiếc đèn bị thiếu.
- Một chiếc xe bị thiếu.
- Một bức tượng bị thiếu.

Ví dụ không ưu tiên:

- Một cái cốc nhỏ hơn 5%.
- Một texture đổi màu rất nhẹ.
- Một nút nhỏ trên máy.
- Một chi tiết nhỏ trên khuôn mặt.

---

# 3. Gameplay Loop

Gameplay cơ bản:

```text
Start Level
    ↓
Quan sát hai thế giới
    ↓
Xoay camera
    ↓
Phát hiện vật bị thiếu
    ↓
Click vào vật thể
    ↓
Kiểm tra Raycast
    ↓
Đúng?
 ┌──┴──┐
No    Yes
 │      │
Không   Found
làm gì    ↓
       Feedback
          ↓
      Found Count
          ↓
    Đã tìm hết?
      ┌───┴───┐
     No      Yes
      │        │
   Continue  Complete
```

---

# 4. Cấu trúc màn hình

Game hướng tới thiết bị di động sử dụng **màn hình dọc (portrait)**. Vì vậy gameplay được chia thành hai viewport theo chiều dọc:

```text
┌─────────────────────────────┐
│                             │
│            TOP              │
│                             │
│       Camera Top            │
│                             │
│                             │
├─────────────────────────────┤
│                             │
│           BOTTOM            │
│                             │
│       Camera Bottom         │
│                             │
│                             │
└─────────────────────────────┘
```

- Camera Top render nửa trên màn hình.
- Camera Bottom render nửa dưới màn hình.
- Hai camera luôn đồng bộ vị trí và rotation.
- Hai camera nhìn cùng một môi trường cơ bản nhưng sử dụng Culling Mask khác nhau để hiển thị các vật thể khác nhau.
- Việc chia trên/dưới phù hợp với màn hình điện thoại dọc vì mỗi viewport vẫn có chiều rộng lớn và đủ không gian để quan sát vật thể 3D.
- Người chơi có thể xoay camera bằng thao tác kéo trên màn hình; input điều khiển chung cả hai camera.
- Khi người chơi tap vào nửa trên, sử dụng Camera Top để Raycast.
- Khi người chơi tap vào nửa dưới, sử dụng Camera Bottom để Raycast.

## 4.1. Tương tác với hai viewport

Có thể xác định viewport dựa trên vị trí chạm theo trục Y:

```text
Screen Height
     │
     ▼
┌─────────────────────────────┐
│          TOP                │  ← Touch → Camera Top
│        Y > 50%              │
├─────────────────────────────┤
│          BOTTOM             │  ← Touch → Camera Bottom
│        Y < 50%              │
└─────────────────────────────┘
```

Gameplay vẫn sử dụng **một hệ thống camera rotation chung**. Không tồn tại hai góc nhìn độc lập; hai camera chỉ khác nhau ở Culling Mask và viewport.


---

# 5. Kiến trúc Scene

Prototype sử dụng **một Scene duy nhất**.

Ví dụ:

```text
Scene
│
├── Environment
│   ├── Ground
│   ├── House
│   ├── Trees
│   ├── Roads
│   └── Props
│
├── MissingObjects
│   ├── MissingObject_01
│   ├── MissingObject_02
│   └── MissingObject_03
│
├── CameraSystem
│   ├── CameraRig
│   ├── TopCamera
│   └── BottomCamera
│
└── GameManager
```

---

# 6. Layer System

Sử dụng các Layer chính:

```text
Shared
Missing_Top
Missing_Bottom
```

## Shared

Chứa các vật thể xuất hiện ở cả hai bên.

Ví dụ:

```text
Ground
House
Tree
Road
Lamp
```

Các object này chỉ tồn tại một lần trong Scene nhưng cả hai camera đều render chúng.

## Missing_Top

Chứa vật thể chỉ xuất hiện ở bên trái.

Ví dụ:

```text
Chair
```

## Missing_Bottom

Chứa vật thể chỉ xuất hiện ở bên phải.

Ví dụ:

```text
Bicycle
```

Camera:

```text
TopCamera
    Culling Mask:
        Shared
        Missing_Top

BottomCamera
    Culling Mask:
        Shared
        Missing_Bottom
```

---

# 7. Camera System

## 7.1. Camera Top

Chức năng:

- Render thế giới bên trái.
- Sử dụng viewport khoảng 0–50% màn hình.
- Render Layer Shared + Missing_Top.

## 7.2. Camera Bottom

Chức năng:

- Render thế giới bên phải.
- Sử dụng viewport khoảng 50–100% màn hình.
- Render Layer Shared + Missing_Bottom.

## 7.3. Camera đồng bộ

Hai camera phải:

- Có cùng Position.
- Có cùng Rotation.
- Có cùng FOV.
- Có cùng Projection Type.

Khi người chơi xoay camera:

```text
Input
  ↓
CameraController
  ↓
TopCamera
BottomCamera
```

Không xử lý input riêng cho từng camera.

---

## 7.4. Unity 6 Camera Implementation Notes

Unity 6 hỗ trợ `Viewport Rect` để xác định phần màn hình mà camera render. Hai camera tạo split-screen trên/dưới:

```text
Top Camera:
X = 0
Y = 0.5
W = 1
H = 0.5

Bottom Camera:
X = 0
Y = 0
W = 1
H = 0.5
```

Unity cũng hỗ trợ `Culling Mask` để camera chỉ render các Layer được chọn. Đây là nền tảng của kiến trúc `Shared / Missing_Top / Missing_Bottom`.

Hai camera sử dụng Perspective Projection. Không cần RenderTexture cho prototype vì mỗi camera có thể render trực tiếp vào một phần màn hình thông qua Viewport Rect.

# 8. Input System

Project sử dụng **Unity Input System Package**, là lựa chọn được khuyến nghị cho project mới trên Unity 6 thay cho Legacy Input Manager.

## Touch Drag

Người chơi kéo ngón tay trên màn hình để xoay camera.

```text
Touch Delta
    ↓
Input System
    ↓
CameraController
    ↓
Yaw / Pitch
    ↓
Camera Top + Camera Bottom
```

## Tap

Khi người chơi thả ngón tay mà khoảng dịch chuyển nhỏ hơn ngưỡng drag, Input System phát hiện Tap để InteractionController xử lý.

## Không Zoom

Không hỗ trợ pinch zoom hoặc mouse wheel zoom. FOV/camera distance được thiết lập cố định.

## Phân biệt Top / Bottom

```text
┌─────────────────────────────┐
│            TOP              │
│          Y ≥ 50%            │
├─────────────────────────────┤
│           BOTTOM            │
│           Y < 50%           │
└─────────────────────────────┘
```

Có thể dùng `Camera.ScreenToViewportPoint()` để chuyển tọa độ screen sang viewport space.

# 9. Missing Object

Mỗi vật thể có thể là đáp án cần tìm sẽ có component:

```csharp
MissingObject
```

Cấu trúc:

```text
MissingObject
├── MeshRenderer
├── Collider
└── MissingObject.cs
```

## Chức năng

- Đánh dấu object là vật thể cần tìm.
- Nhận sự kiện Found.
- Kích hoạt feedback.
- Thông báo GameManager rằng một vật đã được tìm thấy.
- Ngăn người chơi tìm lại cùng một vật nhiều lần.

## Collider

Collider nên đủ lớn để người chơi dễ chạm.

Không nhất thiết phải khớp tuyệt đối với mesh.

Ví dụ:

```text
        Mesh
     ┌────────┐
     │  Chair │
     └────────┘

     Collider
   ┌────────────┐
   │            │
   │   Chair    │
   │            │
   └────────────┘
```

---

# 10. Interaction System

## Raycast

Khi người chơi tap:

```csharp
Ray ray = camera.ScreenPointToRay(touchPosition);

if (Physics.Raycast(ray, out RaycastHit hit))
{
    MissingObject missing =
        hit.collider.GetComponent<MissingObject>();

    if (missing != null)
    {
        missing.Found();
    }
}
```

Trong prototype, chỉ cần kiểm tra trực tiếp `MissingObject`.

Sau này có thể mở rộng thành interface:

```csharp
IInteractable
```

để hỗ trợ các loại interaction khác.

---

# 11. Feedback System

Khi tìm đúng:

```text
Tap
 ↓
MissingObject.Found()
 ↓
Visual Feedback
 ↓
Audio Feedback
 ↓
GameManager.RegisterFound()
```

Feedback prototype có thể gồm:

- Highlight.
- Outline.
- Particle.
- Scale animation.
- Sound effect.
- Found marker.

Không cần làm tất cả ngay.

Prototype tối thiểu chỉ cần một hiệu ứng đơn giản:

```text
Object
 ↓
Highlight / Outline
```

---

# 12. GameManager

`GameManager` quản lý trạng thái của level.

## Chức năng

- Khởi tạo level.
- Đếm tổng số Missing Object.
- Đếm số object đã tìm.
- Nhận thông báo khi tìm đúng.
- Kiểm tra điều kiện hoàn thành.
- Chuyển sang trạng thái Complete.

Ví dụ:

```csharp
public class GameManager : MonoBehaviour
{
    public int totalMissing;
    public int foundMissing;

    public void RegisterFound()
    {
        foundMissing++;

        if (foundMissing >= totalMissing)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        // Level complete
    }
}
```

---

# 13. Level Completion

Điều kiện:

```text
foundMissing == totalMissing
```

Sau khi hoàn thành:

```text
Level Complete
```

Prototype chỉ cần:

- Hiển thị "Complete".
- Hoặc chuyển scene.
- Hoặc reload level.

Chưa cần hệ thống sao, điểm số hoặc phần thưởng.

---

# 14. Các Class chính

## GameManager

**Trách nhiệm:**

- Quản lý trạng thái gameplay.
- Theo dõi tiến trình tìm vật.
- Kiểm tra hoàn thành level.

Không nên:

- Điều khiển camera.
- Xử lý Raycast trực tiếp.
- Chứa logic riêng của MissingObject.

---

## CameraController

**Trách nhiệm:**

- Nhận input xoay camera.
- Quản lý yaw/pitch.
- Cập nhật rotation.
- Đồng bộ TopCamera và BottomCamera.

Không nên:

- Quản lý level.
- Kiểm tra object được click.

---

## InputController

**Trách nhiệm:**

- Đọc Touch Input.
- Phân biệt Drag và Tap.
- Cung cấp dữ liệu input cho các hệ thống khác.

Prototype có thể gộp InputController vào CameraController, nhưng nên tách khi hệ thống lớn hơn.

---

## InteractionController

**Trách nhiệm:**

- Nhận Tap.
- Xác định người chơi đang tap ở viewport trái hay phải.
- Chọn camera tương ứng.
- Tạo Ray.
- Raycast.
- Xác định object được tương tác.

Flow:

```text
Tap
 ↓
InteractionController
 ↓
Left/Bottom Camera
 ↓
Raycast
 ↓
MissingObject
```

---

## MissingObject

**Trách nhiệm:**

- Đại diện cho một vật cần tìm.
- Xử lý trạng thái chưa tìm / đã tìm.
- Kích hoạt feedback.
- Gọi GameManager.

Trạng thái:

```text
NotFound
    ↓
Found
```

---

## FeedbackController

Có thể có hoặc chưa cần trong prototype.

**Trách nhiệm:**

- Highlight.
- Outline.
- Particle.
- Sound.
- Animation.

Có thể ban đầu đặt logic feedback trực tiếp trong `MissingObject`, sau đó tách thành class riêng khi hệ thống lớn hơn.

---

# 15. Class Dependency

Kiến trúc prototype:

```text
                ┌─────────────────┐
                │  InputController│
                └────────┬────────┘
                         │
                         ▼
                ┌─────────────────┐
                │ CameraController│
                └────────┬────────┘
                         │
                 ┌───────┴───────┐
                 ▼               ▼
           Top Camera      Bottom Camera


Touch Tap
    │
    ▼
┌─────────────────────┐
│InteractionController│
└──────────┬──────────┘
           │
           ▼
       Raycast
           │
           ▼
    ┌──────────────┐
    │ MissingObject│
    └───────┬──────┘
            │
            ▼
     GameManager
            │
            ▼
     Level Complete
```

---

# 16. Prototype Scope

Prototype đầu tiên **chỉ cần**:

### Scene

- Một môi trường 3D nhỏ.
- Khoảng 10–20 object.
- 1–3 Missing Object.

### Camera

- 2 camera.
- Split screen 50/50.
- Đồng bộ rotation.
- Xoay 360° ngang.
- Xoay dọc.
- Không zoom.

### Interaction

- Touch drag.
- Tap.
- Raycast.
- Collider.

### Gameplay

- MissingObject.
- Found state.
- Counter.
- Complete state.

### Feedback

- Một hiệu ứng highlight.
- Một sound effect.

---

# 17. Những thứ chưa nên làm trong Prototype

Không cần triển khai ngay:

- Save system.
- Level selection.
- Ads.
- IAP.
- Online.
- Account.
- Analytics.
- Object pooling.
- Complex UI.
- Procedural generation.
- Hệ thống hint.
- Leaderboard.
- Nhiều loại camera.
- Animation phức tạp.
- Shader phức tạp.

Mục tiêu là kiểm chứng **core gameplay**, không phải hoàn thiện game.

---

# 18. Performance Strategy

Vì game hướng tới Android, scene nên được thiết kế nhẹ ngay từ đầu.

Ưu tiên:

- Low-poly / optimized mesh.
- Texture kích thước hợp lý.
- Ít material.
- Ít real-time light.
- Hạn chế real-time shadow.
- Tránh post-processing nặng.
- Hạn chế particle.
- Dùng baked lighting nếu phù hợp.
- Dùng LOD cho object lớn/có khoảng cách xa.
- Không tạo hai bản của các object Shared.

Kiến trúc:

```text
             Scene
               │
      ┌────────┴────────┐
      │                 │
    Shared           Difference
      │                 │
      │          ┌──────┴──────┐
      │          │             │
      │      TopOnly      BottomOnly
      │
      └──────────┬──────────┘
                 │
          ┌──────┴──────┐
          │             │
      TopCamera    BottomCamera
```

Chỉ các object thực sự khác nhau mới cần phiên bản riêng.

---

# 19. Thiết kế Level

Một level nên có:

```text
Environment
│
├── Shared Objects
│
├── Missing Object 1
├── Missing Object 2
├── Missing Object 3
└── ...
```

Mỗi Missing Object cần:

- Vị trí đủ dễ quan sát.
- Kích thước đủ lớn.
- Không bị che hoàn toàn.
- Không nằm quá gần những vật thể có hình dạng tương tự.
- Có sự khác biệt rõ ràng.

Ví dụ tốt:

```text
LEFT                     RIGHT

🌳                       🌳
🏠                       🏠
🚗                       🚗
🪑                       [missing]
💡                       💡
```

Ví dụ không tốt:

```text
LEFT                     RIGHT

🌳                       🌳
🏠                       🏠
tiny_coin                [missing]
🚗                       🚗
```

---

# 20. Tiêu chí thành công của Prototype

Prototype được xem là thành công khi người chơi có thể:

1. Nhìn thấy hai thế giới 3D cạnh nhau.
2. Xoay camera và cả hai bên thay đổi góc nhìn đồng bộ.
3. Quan sát được toàn bộ môi trường mà không cần zoom.
4. Nhận ra một vật thể xuất hiện ở một bên nhưng không có ở bên kia.
5. Tap vào vật thể.
6. Game xác định đúng Missing Object.
7. Object được đánh dấu đã tìm.
8. Counter tăng.
9. Tìm hết các vật và nhận trạng thái Level Complete.

Nếu 9 điều này hoạt động mượt trên thiết bị Android mục tiêu, core gameplay của **Find The Missing** đã được chứng minh.

---

# 21. Roadmap sau Prototype

Sau khi prototype ổn định:

### Phase 2 — Level System

- LevelData.
- Level loading.
- Nhiều level.
- Randomized missing objects nếu cần.

### Phase 3 — UX

- Main Menu.
- Level Select.
- Progress.
- Pause.
- Settings.
- Hint.

### Phase 4 — Visual

- Highlight đẹp hơn.
- Animation.
- Particle.
- Camera transition.
- UI polish.
- Sound design.

### Phase 5 — Optimization

- Profiling trên Android.
- LOD.
- Occlusion Culling.
- Batching.
- Texture optimization.
- Lighting optimization.

### Phase 6 — Release

- Save system.
- Ads.
- Analytics.
- Android build.
- Device testing.
- Google Play release.

---

# 22. Unity 6 Technical Baseline

Tài liệu này được thiết kế cho **Unity 6 (6000.x) trở lên**.

Các thành phần nền tảng:

- Camera + Perspective Projection.
- Viewport Rect cho split-screen.
- Culling Mask + Layers.
- Unity Physics 3D.
- `Physics.Raycast()`.
- `ScreenToViewportPoint()`.
- `ViewportPointToRay()`.
- Unity Input System Package.
- URP.

Không phụ thuộc vào API hoặc kiến trúc riêng của Unity 2022/2021.

# 23. Core Design Summary

**Find The Missing** không phải là game tìm một thay đổi cực nhỏ giữa hai hình ảnh.

Nó là một game quan sát 3D:

> **Hai thế giới gần giống nhau. Một thế giới có một vật mà thế giới kia không có. Người chơi phải xoay camera và quan sát để tìm ra vật bị thiếu.**

Kiến trúc cốt lõi:

```text
1 Scene
2 Cameras
3 Layers

Shared
Missing_Top
Missing_Bottom
```

Và gameplay:

```text
Look
 ↓
Rotate
 ↓
Compare
 ↓
Find Missing Object
 ↓
Tap
 ↓
Raycast
 ↓
Found
 ↓
Repeat
 ↓
Complete
```

Đây là phạm vi phù hợp để xây dựng prototype đầu tiên mà chưa cần over-engineering.
