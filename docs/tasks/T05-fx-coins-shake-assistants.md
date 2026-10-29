# T05: FX — монетки, шейк камеры, активация ассистентов

## Цель
Три небольших визуальные системы: полёт монет к счётчику при убийстве врага, короткий шейк камеры на тап, включение GameObject'ов ассистентов после их найма.

## Контекст
- Проект: `F:/Unity/Tap-Tap Battle` (Unity 6, 2D URP, Zenject, C# events, DOTween, TMP). R3/UniRx/UniTask запрещены — только C# `event`.
- Сигналы (существуют):
  - `Events/EnemyKilledSignal.cs`: `event Action<EnemyKilledInfo>`; `EnemyKilledInfo{bool IsBoss; int Level}`.
  - `Events/EnemySpawnedSignal.cs`: `event Action<EnemySpawnedInfo>`; `EnemySpawnedInfo{Enemy Enemy; bool IsBoss}` — кэшируй Enemy, при EnemyKilledSignal его transform ещё валиден (Destroy откладывается до конца кадра).
  - `Events/TapEvent.cs`: `event Action OnTapped`.
  - `Events/AssistantChangedSignal.cs`: `event Action`.
- Сервис: `Assistants/AssistantService.cs` — `int GetLevel(string id)`; конфиги: `Assistants/AssistantConfig.cs` — `string Id`. Инжект `IReadOnlyList<AssistantConfig>` доступен (забинден).
- Кэш текущего врага: подписка EnemySpawnedSignal (см. DpsTicker в Assistants/ как образец).

## Что сделать

1. **Assets/_Project/Scripts/UI/CoinFlySystem.cs** — «магнитные» монетки при убийстве:
   - `[SerializeField] RectTransform _targetRect` (иконка валюты в UI), `[SerializeField] Canvas _canvas`, `[SerializeField] int _coinsMin = 4`, `_coinsMax = 7`, `[SerializeField] float _spawnRadius = 40f`, `_flightDuration = 0.5f`, `_stagger = 0.03f`;
   - монетка — программный GameObject: Image (без спрайта, белый квадрат 14x14, цвет #FFD34D — спрайт подставит интегратор позже через serialized поле `[SerializeField] Sprite _coinSprite` (если null — обычный белый), RectTransform якорь (0,0);
   - на EnemyKilledSignal: позиция врага `Camera.main.WorldToScreenPoint(enemy.position)` → спавн N монеток с рандомным смещением в радиусе, DOTween Sequence: мелкий подскок (scale 0→1 + случайный сдвиг), затем DOAnchorPos к `_targetRect` с ease InQuad, фейд в конце, Destroy;
   - если `_canvas.renderMode != ScreenSpaceOverlay` — просто работай в координатах канваса без конвертации (защитись от null Camera.main);
   - кнопки UI не должны блокироваться: монетки `raycastTarget = false`.
2. **Assets/_Project/Scripts/FX/TapCameraShake.cs** — MonoBehaviour на камере:
   - `[SerializeField] float _duration = 0.1f`, `_strength = 4f`; на TapEvent — DOTween `DOShakePosition` (или ручной sin-шейк, не критично); последовательные тапы перезапускают шейк без накопления смещения (восстанавливай стартовую позицию).
3. **Assets/_Project/Scripts/Assistants/AssistantActivator.cs** — MonoBehaviour:
   - `[Serializable] public class AssistantBinding { public AssistantConfig Config; public GameObject SceneObject; }`
   - `[SerializeField] private List<AssistantBinding> _bindings = new();`
   - `[Inject] Construct(IReadOnlyList<AssistantConfig> configs)` не нужен — работай только с `_bindings`;
   - OnEnable: применить состояние (foreach binding: если уровень ассистента > 0 — SceneObject.SetActive(true)); подписка AssistantChangedSignal → переприменить; OnDisable — отписка;
   - уровень узнавай через `[Inject] Construct(AssistantService service)` и `service.GetLevel(config.Id)`;
   - SceneObject == null — просто пропускать (не все ассистенты имеют визуал).
   - ВАЖНО: объекты ассистентов в сцене изначально ВЫКЛЮЧЕНЫ — твой компонент их включает. SetActive(false) при уровне 0 не делать (не выключай то, что юзер включил руками — только включение).

## Ограничения
- ТОЛЬКО новые файлы: UI/CoinFlySystem.cs, FX/TapCameraShake.cs (+ папка FX), Assistants/AssistantActivator.cs. Существующее не менять. НЕ трогать сцену/префабы/.asset/LevelInstaller; не git commit; не запускать Unity; не mcp__UnityMCP__*.
- Стиль: 4 пробела, `_camelCase`, подписки симметричны, namespace `_Project.Scripts.UI` / `_Project.Scripts.FX` / `_Project.Scripts.Assistants`.

## Критерии готовности
- [ ] CoinFlySystem: убийство → 4-7 монеток летят в цель за ~0.5с, Destroy после, повторные убийства ок (нет утечки GO)
- [ ] TapCameraShake: камера возвращается точно в исходную позицию при спаме тапов
- [ ] AssistantActivator: нанят CatMage (уровень 1) → его GO активен; уровень 0 → не трогаем
- [ ] Нигде нет NRE-путей (враг умер/камера null)

## Проверка
Без скриншотов — кодом; прогони мысленно серию из 10 убийств подряд. Застрял >20 мин — стоп и отчёт.

## Отчёт
Файлы; serialized-поля для провайринга в инспекторе (списком по каждому классу); что не вышло.
