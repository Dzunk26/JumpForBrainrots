# Coding Style — Move-Stop-Move

Tài liệu này mô tả phong cách code **đang thực sự được dùng** trong dự án, rút ra từ
`Assets/_Game/Scripts` và `Assets/_Game/Extensions`. Chỉ ghi lại những quy tắc được áp dụng
nhất quán; các chỗ còn lệch được liệt kê riêng ở cuối file.

> Code của asset bên thứ 3 (`Layer Lab`, `Voxel Arsenal`, `TutorialInfo`) không thuộc phạm vi tài liệu này.

---

## 1. Định dạng chung

### 1.1. Ngoặc nhọn cùng dòng, `else` xuống dòng mới
Dấu `{` luôn nằm cùng dòng với khai báo; `else` / `else if` bắt đầu ở dòng mới sau `}`. Thụt lề 4 space.

```csharp
// Joystick/GameInput.cs
if (inputVector.sqrMagnitude > maxMovement * maxMovement) {
    inputVector = inputVector.normalized;
}
else {
    inputVector = inputVector / maxMovement;
    inputVector = HandleStickDeadzone(inputVector);
}
```

### 1.2. Guard clause một dòng, không ngoặc
Điều kiện thoát sớm viết trên một dòng `if (...) return;`, đặt ở đầu method.

```csharp
// Obstacle.cs
private void Update() {
    if (!isFade) return;
    ...
}
```

### 1.3. Mỗi file một class chính, tên file = tên class, không namespace
Không dùng `namespace`, không dùng `#region`. Khối `using` mặc định của Unity
(`System.Collections`, `System.Collections.Generic`, `UnityEngine`) được giữ nguyên.

```csharp
// Equipment/Hat.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hat : Equipment {

}
```

### 1.4. Enum khai báo ở đầu file của class dùng nó chính
Không tách enum ra file riêng.

```csharp
// Character/CharacterVisual.cs
public enum AnimState {
    Idle,
    Run,
    Attack,
    ...
}

public class CharacterVisual : MonoBehaviour {
```

Tương tự: `EquipmentType` trong `Equipment.cs`, `PlayerState` trong `Player.cs`, `PoolType` trong `PoolControl.cs`.

### 1.5. Thứ tự member trong class
1. Public property (`TF`, `IsXxx =>`, `Instance`)
2. Field `[SerializeField]`
3. Field `protected` / `private` không serialize
4. Unity callback (`Awake`, `OnEnable`, `Start`, `Update`, `OnTriggerEnter`...)
5. Method `public` → `protected` → `private`

```csharp
// Character/Bot.cs
public class Bot : Character {
    public bool IsDestination => (currentTargetPoint - TF.position).sqrMagnitude < 0.001f;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stateTimerMax = 5f;

    private BotState currentState;
    private Vector3 currentTargetPoint;

    private void OnEnable() { ... }
    private void Update() { ... }

    public override void OnInit() { ... }
```

---

## 2. Quy tắc đặt tên

### 2.1. Class — PascalCase, hậu tố theo vai trò

| Hậu tố | Ý nghĩa | Ví dụ |
|---|---|---|
| `SO` | ScriptableObject | `LevelCharacterConfigSO`, `ListLevelCharacterConfigSO` |
| `Visual` | Component chỉ lo phần hiển thị | `CharacterVisual`, `WeaponVisual`, `PlayerDetectTriggerVisual` |
| `Manager` | Singleton quản lý hệ thống | `DataManager`, `UIManager` |
| `State` | State tĩnh của state machine | `IdleState`, `PatrolState`, `AttackState` |

Logic và hiển thị tách thành cặp class: `Equipment` ↔ `EquipmentVisual`, `Character` ↔ `CharacterVisual`.

```csharp
// Equipment/WeaponVisual.cs
public class WeaponVisual : EquipmentVisual {
    public override void OnChangeEquipment(Equipment newEquipment) { ... }
}
```

### 2.2. Field private/protected — camelCase, không tiền tố
Không dùng `_`, `m_`. Luôn ghi rõ access modifier. Field chỉnh trong Inspector dùng
`[SerializeField] private` (hoặc `protected` nếu lớp con cần), khai báo trên cùng một dòng.

```csharp
// BaseProjectile.cs
[SerializeField] protected float shootSpeed = 5f;
[SerializeField] protected Collider weaponCollider;

protected Vector3 shootDir;
private Character owner;
private bool isCollided;
```

### 2.3. Field public — camelCase, chỉ dùng cho class dữ liệu thuần
Field public chỉ xuất hiện trong class chứa dữ liệu (`[System.Serializable]` config, `EventArgs`, bảng stat).

```csharp
// Pool/PoolControl.cs
[System.Serializable]
public class PoolConfig {
    public GameUnit prefab;
    public Transform parent;
    public int amount;
}
```

### 2.4. Biến bool — tiền tố `is`
```csharp
protected bool isFlying;      // BaseProjectile
protected bool isAttacking;   // Character
private bool isFade = false;  // Obstacle
private bool isFirstTouch = true; // GameInput
```

### 2.5. Bộ đếm thời gian / giới hạn — cặp `xxx` + `xxxMax`
Giá trị giới hạn là `[SerializeField]`, biến đếm là private.

```csharp
// Character/Bot.cs
[SerializeField] private float stateTimerMax = 5f;
[SerializeField] private int attackLimitMax = 3;

private float stateTimer;
private int attackLimit;
```

Tương tự: `stopTimer`/`stopTimerMax` (BaseProjectile), `deadTimer`/`deadTimerMax` (Player).

### 2.6. Hằng số — `UPPER_SNAKE_CASE` trong static class `Constant`
Tag có hậu tố `_TAG`. Hằng được khai báo `public static string` (không dùng `const`).

```csharp
// Manager/Constant.cs
public static class Constant {
    public static string PLAYER_DATA = "Player Data";

    public static string PROJECTILE_TAG = "Projectile";
    public static string CHARACTER_TAG = "Character";
    public static string OBSTACLE_TAG = "Obstacle";
    public static string WALL_TAG = "Wall";
}
```

### 2.7. Property — PascalCase; property bool dùng expression-bodied `=>`
```csharp
public bool IsOutOfRange => Vector3.Distance(startPoint, TF.position) >= characterAttackRange; // BaseProjectile
public bool IsDestination => (currentTargetPoint - TF.position).sqrMagnitude < 0.001f;        // Bot
```

### 2.8. Event — `On` + động từ, kiểu `EventHandler`; handler tên `Publisher_OnEvent`
Dữ liệu đi kèm đặt trong class lồng `XxxEventArgs : EventArgs`. Luôn raise bằng `?.Invoke(this, ...)`.

```csharp
// Joystick/GameInput.cs
public event EventHandler<TouchEventArgs> OnFingerDown;
public event EventHandler OnFingerUp;

public class TouchEventArgs : EventArgs {
    public Vector2 touchPosition;
}
...
OnFingerUp?.Invoke(this, EventArgs.Empty);
```

```csharp
// Joystick/FloatingJoystick.cs — handler: <TênPublisher>_<TênEvent>
GameInput.Instance.OnFingerDown += GameInput_OnFingerDown;

private void GameInput_OnFingerDown(object sender, GameInput.TouchEventArgs e) {
    OnTouchFingerDown(e.touchPosition);
}
```

### 2.9. Method — PascalCase, tiền tố theo mục đích

| Tiền tố | Dùng cho | Ví dụ |
|---|---|---|
| `OnInit` / `OnDespawn` | Vòng đời object (xem §4.2) | `Character.OnInit()`, `BaseProjectile.OnDespawn()` |
| `On` + sự kiện | Phản ứng khi có gì đó xảy ra | `OnHitted`, `OnDetectTarget`, `OnEquipmentChanged`, `OnLevelUp` |
| `OnEnter` / `OnExecute` + State | Hook của state machine | `OnEnterPatrol`, `OnExecuteAttack` |
| `Get` | Trả về dữ liệu | `GetEquipmentType`, `GetWorldAttackRange`, `GetClosestTarget` |
| `Is` / `Has` / `Can` / `Match` | Trả về bool | `IsMoving`, `HasTarget`, `CanLevelUp`, `MatchLevelID` |
| `Handle` | Xử lý logic mỗi frame / chuẩn hoá input | `HandlePlayerState`, `HandleMovement`, `HandleStickDeadzone` |
| `IE` | Coroutine | `IEAttack` |

```csharp
// Character/Character.cs
public bool HasTarget() {
    return targets.Count > 0;
}

private bool CanLevelUp(int point, int requiredPoint) {
    return point >= requiredPoint;
}

private IEnumerator IEAttack(Vector3 direction) {
    yield return new WaitForSeconds(attackDelay);
    Attack(direction);
}
```

### 2.10. Dữ liệu cấu hình được đọc qua method `GetXxx()`, không qua property
Các class chứa config (Equipment, SO, Stat) giữ field `private`/`protected` và mở ra bằng method getter.

```csharp
// Equipment/Equipment.cs
[SerializeField] protected float attackRangeModifier;

public float GetAttackRangeModifier() {
    return attackRangeModifier;
}
```

### 2.11. Tham số trùng tên field — gán qua `this.`
```csharp
// BaseProjectile.cs
public virtual void OnInit(Vector3 shootDir, Character owner) {
    this.owner = owner;
    this.shootDir = shootDir;
    ...
}
```

---

## 3. Tổ chức thư mục

```
Assets/_Game/
├── Scripts/            # code gameplay, chia theo feature
│   ├── Character/      # Character (base), Player, Bot + các component con
│   ├── Equipment/      # Equipment + XxxVisual tương ứng
│   ├── StateMachine/   # state tĩnh của Bot
│   ├── Pool/           # GameUnit, SimplePool, PoolControl
│   ├── SO/             # class ScriptableObject
│   ├── Data/           # save/load
│   ├── Manager/        # Cache, Constant
│   ├── Joystick/       # input
│   └── Camera/
├── Extensions/         # framework tái sử dụng, không phụ thuộc gameplay
│   ├── Singleton/
│   └── UIManager/
├── Animations/  Scenes/  Prefabs/  Resources/  ScriptableObjects/  Sounds/
```

- Thư mục `_Game` có dấu `_` để tách khỏi asset bên thứ 3 và nằm đầu danh sách.
- Mỗi feature là một thư mục; class cha và các class con/visual của nó nằm chung
  (`Equipment/` chứa `Equipment`, `Weapon`, `Hat`, `WeaponVisual`, `HatVisual`...).
- Code tái sử dụng được giữa các dự án (Singleton, UIManager) đặt trong `Extensions/`, không trong `Scripts/`.
- UI prefab được load từ `Resources/UI/`:

```csharp
// Extensions/UIManager/UIManager.cs
UICanvas[] prefabs = Resources.LoadAll<UICanvas>("UI/");
```

---

## 4. Pattern kiến trúc

### 4.1. Singleton generic `Singleton<T>`
Manager kế thừa `Singleton<T>` và được truy cập qua `Xxx.Instance`. Instance được tìm lười
(`FindAnyObjectByType`) và tự tạo GameObject nếu chưa có.

```csharp
// Extensions/Singleton/Singleton.cs
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    public static T Instance {
        get {
            if (instance == null) {
                instance = FindAnyObjectByType<T>();
                ...
```

```csharp
public class DataManager : Singleton<DataManager> { ... }
public class UIManager : Singleton<UIManager> { ... }
public class GameInput : Singleton<GameInput> { ... }

inputVector = GameInput.Instance.GetMovementVectorNormalized(); // Player.cs
```

### 4.2. Khởi tạo thủ công bằng `OnInit()` / `OnDespawn()`
Mỗi object có cặp `OnInit()` (reset toàn bộ trạng thái) và `OnDespawn()` (dọn dẹp / trả về pool).
Object được pool gọi `OnInit()` trong `OnEnable()`. Lớp con override và gọi `base.OnInit()` trước.
Khi cần tham số, `OnInit` nhận tham số (projectile).

```csharp
// Character/Bot.cs
private void OnEnable() {
    OnInit();
}

public override void OnInit() {
    base.OnInit();
    ChangeState(BotStates.Idle);
}
```

```csharp
// Equipment/Weapon.cs — lấy từ pool xong gọi OnInit với tham số
BaseProjectile projectile = SimplePool.GetFromPool<BaseProjectile>(projectilePrefab.PoolType, position, rotation);
projectile.OnInit(shootDir, owner);
```

### 4.3. Object pooling — `GameUnit` + `SimplePool`
- Mọi object được pool kế thừa `GameUnit` và có một giá trị trong enum `PoolType`.
- `PoolControl` preload pool trong `Awake()` từ mảng `PoolConfig` cấu hình trong Inspector.
- Lấy ra: `SimplePool.GetFromPool<T>()`; trả về: `SimplePool.ReturnToPool(this)` trong `OnDespawn()`.

```csharp
// BaseProjectile.cs
public abstract class BaseProjectile : GameUnit {
    ...
    public void OnDespawn() {
        isFlying = false;
        SimplePool.ReturnToPool(this);
    }
```

```csharp
// Pool/PoolControl.cs
private void Awake() {
    for (int i = 0; i < poolConfigs.Length; i++) {
        SimplePool.PreLoad(poolConfigs[i].prefab, poolConfigs[i].amount, poolConfigs[i].parent);
    }
}
```

### 4.4. Cache `Transform` qua property `TF`
Class nào truy cập transform thường xuyên đều khai báo property `TF` lazy-cache, và dùng `TF` thay cho `transform`.

```csharp
// Pool/GameUnit.cs (lặp lại y hệt trong CameraFollow, CharacterVisual, EquipmentVisual, PlayerDetectTriggerVisual)
private Transform tf;
public Transform TF {
    get {
        if (tf == null) {
            tf = transform;
        }

        return tf;
    }
}
```

### 4.5. Cache component theo `Collider` — static class `Cache`
Trong callback va chạm, không gọi `GetComponent` trực tiếp mà lấy qua `Cache`.
Luôn kiểm tra tag bằng `CompareTag(Constant.XXX_TAG)` trước.

```csharp
// Character/CharacterDetectTrigger.cs
private void OnTriggerEnter(Collider other) {
    if (other.CompareTag(Constant.CHARACTER_TAG)) {
        Character otherCharacter = Cache.GetCharacter(other);
        ...
```

```csharp
// Manager/Cache.cs
public static Character GetCharacter(Collider collider) {
    if (!dictCharacter.ContainsKey(collider)) {
        Character character = collider.GetComponent<Character>();
        dictCharacter[collider] = character;
    }

    return dictCharacter[collider];
}
```

Tên animation cũng được lấy qua cache từ enum, không dùng string literal:

```csharp
// Character/CharacterVisual.cs
animName = Cache.GetAnimName(currentAnimState);
animator.SetTrigger(animName);
```

### 4.6. State machine
**Bot** — state là static class gồm 3 hàm `OnEnter` / `OnExecute` / `OnExit`, chỉ chuyển tiếp sang
method `OnEnterXxx` / `OnExecuteXxx` của `Bot`. Các state được đăng ký thành `BotState` (bộ 3 `Action<Bot>`)
trong `BotStates`. Chuyển state qua `ChangeState()`.

```csharp
// StateMachine/PatrolState.cs
public static class PatrolState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterPatrol();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecutePatrol();
    }

    public static void OnExit(Bot bot) {

    }
}
```

```csharp
// StateMachine/BotStates.cs
public static readonly BotState Patrol = new BotState(PatrolState.OnEnter, PatrolState.OnExecute, PatrolState.OnExit);

// Character/Bot.cs
public void ChangeState(BotState newState) {
    currentState?.OnExit(this);
    currentState = newState;
    currentState?.OnEnter(this);
}
```

**Player** — state đơn giản hơn: enum `PlayerState` + `switch` trong `HandlePlayerState()` gọi mỗi `Update`.

```csharp
// Character/Player.cs
switch (currentState) {
    case PlayerState.Idle:
        characterVisual.OnIdle();
        if (IsMoving()) {
            ChangeState(PlayerState.Running);
        }
        ...
        break;
```

### 4.7. Event C# chuẩn `EventHandler`
Giao tiếp một-nhiều (input → joystick UI) dùng `event EventHandler<T>`, đăng ký trong `Start`/`OnEnable`,
huỷ trong `OnDisable`. Xem ví dụ ở §2.8.

```csharp
// Joystick/GameInput.cs
private void OnEnable() {
    EnhancedTouchSupport.Enable();
    Etouch.Touch.onFingerDown += Touch_onFingerDown;
    ...
}

private void OnDisable() {
    Etouch.Touch.onFingerDown -= Touch_onFingerDown;
    ...
}
```

### 4.8. ScriptableObject cho config
- Có `[CreateAssetMenu()]`, tên class hậu tố `SO`.
- Field là `[SerializeField] private`, đọc qua `GetXxx()`; tìm theo ID bằng `MatchXxx(id)`.
- Danh sách config được bọc bằng một SO khác (`ListXxxSO`) cung cấp hàm tra cứu.

```csharp
// SO/LevelCharacterConfigSO.cs
[CreateAssetMenu()]
public class LevelCharacterConfigSO : ScriptableObject {
    [SerializeField] private int levelID;
    [SerializeField] private float scaleModifier;

    public bool MatchLevelID(int levelID) {
        return this.levelID == levelID;
    }

    public float GetScaleModifier() {
        return scaleModifier;
    }
```

```csharp
// SO/ListLevelCharacterConfigSO.cs
[SerializeField] private List<LevelCharacterConfigSO> levelCharacterSOs;

public LevelCharacterConfigSO GetLevelGrowthByLevelID(int levelID) { ... }
```

### 4.9. Character là "hub" điều phối các component con
`Character` không tự làm hết mà giữ tham chiếu `[SerializeField]` tới các component chuyên trách
và chuyển tiếp lời gọi. Component con giữ tham chiếu ngược lại `Character` khi cần callback.

```csharp
// Character/Character.cs
[SerializeField] protected CharacterStat characterStat;
[SerializeField] protected CharacterEquipment characterEquipment;
[SerializeField] protected CharacterVisual characterVisual;
[SerializeField] protected CharacterDetectTrigger characterDetectTrigger;

public void OnEquipmentChanged(Equipment newEquipment) {
    Equipment oldEquipment = characterEquipment.GetEquipmentByType(newEquipment.GetEquipmentType());

    characterStat.OnEquipmentChanged(newEquipment, oldEquipment);
    characterVisual.OnEquipmentChanged(newEquipment);
    characterEquipment.OnEquipmentChanged(newEquipment);
}
```

### 4.10. Kế thừa với abstract / virtual + gọi `base`
Hành vi khác nhau giữa các biến thể được đưa vào method `abstract` (template method);
override method `virtual` luôn gọi `base.Xxx()` trước rồi mới mở rộng.

```csharp
// BaseProjectile.cs
protected abstract void Fly();

// AxeProjectile.cs
public override void OnInit(Vector3 shootDir, Character owner) {
    base.OnInit(shootDir, owner);
    currentAngle = 0f;
    TF.rotation = Quaternion.identity;
}

protected override void Fly() {
    TF.position += shootDir * shootSpeed * Time.deltaTime;
    ...
}
```

### 4.11. Delay bằng `Invoke` + `nameof`
Tên method truyền cho `Invoke` luôn dùng `nameof`, không viết string tay.

```csharp
// Extensions/UIManager/UICanvas.cs
Invoke(nameof(CloseDirectly), time);

// BaseProjectile.cs
DelayFunct(nameof(OnDespawn), delayStop);
```

---

## 5. Comment và log

### 5.1. Comment `//` một dòng, tiếng Việt không dấu, viết thường
Không dùng XML doc (`///`). Comment đặt ngay trên method để mô tả chức năng (trong code framework),
hoặc cuối dòng để giải thích *lý do*.

```csharp
// Pool/SimplePool.cs
// lay phan tu khoi pool
public static T GetFromPool<T>(PoolType poolType, Vector3 position, Quaternion rotation) where T : GameUnit {
```

```csharp
// BaseProjectile.cs
DelayFunct(nameof(StopFly), delayStop); // delay de tao cam giac projectile va vao obstacle
```

Comment ngắn tiếng Anh cuối dòng cũng được dùng, đặc biệt `// for testing` để đánh dấu code tạm:

```csharp
[SerializeField] private Weapon weapon; // for testing                                       // Character.cs
Debug.DrawLine(TF.position, TF.position + moveDir * characterStat.GetWorldAttackRange()); // for testing  // Player.cs
```

### 5.2. Code phụ thuộc hệ thống chưa có được comment-out tại chỗ
Khi một đoạn phụ thuộc class chưa tồn tại (ví dụ `GameManager`), code được giữ lại dạng comment
thay vì xoá, để bật lại sau.

```csharp
// Joystick/GameInput.cs
private void Touch_onFingerDown(Finger touchedFinger) {
    //if (!GameManager.Instance.IsPlayingGame()) return;
```

### 5.3. Log: `Debug.LogError` + `return` khi dùng sai API; `Debug.Log` cho thông tin
Không có logger riêng. Chuỗi log nối bằng `+`, không dùng string interpolation `$""`.

```csharp
// Pool/SimplePool.cs
if (!poolInstances.ContainsKey(poolType)) {
    Debug.LogError(poolType + " is not loaded");
    return null;
}
```

```csharp
// Data/DataManager.cs
Debug.Log("No data was found. Initializing data to defaults");
```

---

## 6. Những điều dự án KHÔNG làm

| Tránh | Thay bằng | Ví dụ trong dự án |
|---|---|---|
| `GetComponent` trong trigger / `Update` | `Cache.GetXxx(collider)`; `GetComponent` chỉ ở `Awake` | `Cache.GetCharacter(other)` |
| So sánh tag bằng string: `other.tag == "Wall"` | `CompareTag(Constant.WALL_TAG)` | `raycastHit.collider.CompareTag(Constant.OBSTACLE_TAG)` |
| String literal cho tên animation | `Cache.GetAnimName(AnimState.X)` | `CharacterVisual.ChangeAnimState` |
| `Instantiate`/`Destroy` cho object sinh liên tục (projectile, bot) | `SimplePool` | `Weapon.Fire` |
| Field `public` để hiện trong Inspector | `[SerializeField] private` | toàn bộ MonoBehaviour |
| Tiền tố `_` / `m_` cho field | camelCase trần | `private float stopTimer;` |
| `var` | Kiểu tường minh | `Character character = Cache.GetCharacter(other);` |
| `Invoke("Name", t)` | `Invoke(nameof(Name), t)` | `UICanvas.Close` |
| `GameObject.Find` / `FindObjectOfType` rải rác trong gameplay | `Singleton<T>.Instance` hoặc tham chiếu `[SerializeField]` | chỉ `Singleton` và `DataManager.OnInit` gọi Find |
| `namespace`, `#region`, XML doc `///` | — | không file nào dùng |
| Khởi tạo logic trong constructor / `Start` rải rác | `OnInit()` tập trung, gọi từ `OnEnable` | `Bot`, `Player`, `Obstacle` |

---

## 7. Điểm chưa nhất quán (không coi là quy tắc)

Các chỗ dưới đây lệch khỏi phần lớn codebase; khi viết code mới nên theo quy tắc ở trên:

- **So sánh khoảng cách**: đa số dùng `sqrMagnitude` so với bình phương ngưỡng, nhưng
  `BaseProjectile.IsOutOfRange` và `Character.GetClosestTarget` dùng `Vector3.Distance`.
- **Đặt tên field**: `FloatingJoystick.RectTransform` (PascalCase) và `GameUnit.PoolType` (public PascalCase)
  khác quy tắc camelCase.
- **Access modifier**: field trong class `Pool` (`SimplePool.cs`) không ghi `private`.
- **`transform` trực tiếp**: `Obstacle.IsOutOfDetectRadius` dùng `transform.position` thay vì cache.
- **Vị trí file**: các class projectile (`BaseProjectile`, `KnifeProjectile`, `AxeProjectile`, `BommerangProjectile`)
  và `Obstacle` nằm ở gốc `Scripts/` thay vì thư mục feature riêng.
- **Log debug còn sót**: `Debug.Log("On Patrol")` trong `Bot.OnExecutePatrol`, `Debug.Log(character)` trong
  `CharacterDetectTrigger`.
- **Lỗi chính tả trong tên**: `RemoveInvalidTagets`, `UnequipOldEquiment`, `OnFirstTourch`, `Bommerang`,
  `ATTACK_RANG_CONVERTER`.
