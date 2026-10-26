# Tutorial System

Thư mục này không phụ thuộc gì vào game Jump For Brainrots. Muốn dùng cho game khác thì copy nguyên thư mục `TutorialSystem` (kèm các file `.meta`) sang game đó.

Yêu cầu: URP và TextMeshPro.

## Thành phần
- `Runtime/TutorialController.cs`: chạy lần lượt các bước, kéo mũi tên, hiện câu hướng dẫn, phát event.
- `Runtime/TutorialStep.cs`: lớp gốc của một bước. Mỗi game kế thừa lớp này để viết bước riêng.
- `Runtime/Steps/ReachTargetStep.cs`: bước dùng chung "đi tới chỗ X".
- `Runtime/TutorialPathArrow.cs`, `Shaders/`, `Materials/`, `Prefabs/`: mũi tên cầu vồng.
- `Editor/ToolCreateTutorialPathArrow.cs`: menu `Tools/Tutorial/Create Rainbow Path Arrow`, dùng để tạo lại prefab và material.

## Dựng tutorial trong game mới
1. Tạo một GameObject `Tutorial` và gắn `TutorialController`.
2. Gán cho controller:
   - `Path Arrow`: prefab mũi tên.
   - `Path Start Point`: một object con của player.
   - `Message Text` (tuỳ chọn), hoặc nối `On Message Changed` sang UI riêng của game.
3. Mỗi bước là một GameObject con có một component kế thừa `TutorialStep`. Kéo các bước vào list `Steps` theo thứ tự chạy.
4. Chọn cách lưu tiến độ:
   - Không cần lưu: bật `Is Start On Awake`.
   - Có lưu: viết một script nhỏ gọi `StartTutorial(bướcĐãLưu)` khi load và đọc `CurrentStepIndex` khi save. Xem `Scripts/Tutorial/TutorialSave.cs` của game này để làm mẫu.

## Viết một bước mới
```csharp
public class OpenShopStep : TutorialStep {
    [SerializeField] private Transform shopPoint;

    public override bool IsCompleted() => ShopUI.IsOpen;                // khi nào bước xong
    public override bool TryGetTargetPosition(out Vector3 position) {   // mũi tên chỉ tới đâu
        position = shopPoint.position;
        return true;
    }
    // tuỳ chọn: IsReady() (đợi game sẵn sàng), OnStepStart(), OnStepEnd()
}
```

Nếu game báo sự kiện thay vì kiểm tra từng frame, cho `IsCompleted()` trả về `false` rồi gọi `controller.CompleteCurrentStep()` khi sự kiện xảy ra.
