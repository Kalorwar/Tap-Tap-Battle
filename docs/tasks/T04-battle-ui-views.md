# T04: Боевые UI-вьюхи (код, без сцены)

## Цель
Готовые MonoBehaviour-вьюхи для боевого экрана: таймер босса, номер уровня, хаб прокачки (панель с кнопкой), всплывающие числа урона. Всё подписано на существующие сигналы, вёрстка рядов — программно, без префабов.

## Контекст
- Проект: `F:/Unity/Tap-Tap Battle` (Unity 6, 2D URP, Zenject, C# events, DOTween, TextMeshPro). Никакого R3/UniRx/UniTask — только C# `event`.
- Сигналы (все уже существуют, паттерн: static class + `event Action<T>` + Emit):
  - `Events/BossTimerSignal.cs`: `event Action<BossTimerInfo>`; `BossTimerInfo{bool IsRunning; float RemainingSeconds}` — эмитится на старте боя с боссом, каждую секунду и при стопе.
  - `Events/LevelCompletedSignal.cs`: `event Action<int>` (level) — уровень завершён.
  - `Events/UpgradeChangedSignal.cs`: `event Action` — куплена прокачка тапа.
  - `Events/AssistantChangedSignal.cs`: `event Action` — покупка/анлок ассистента.
  - `Events/EnemyHitSignal.cs`: `event Action<EnemyHitEvent>`; `EnemyHitEvent{float CurrentHealthPercent; float DamagePercent}` (структура в Enemies/Enemy.cs).
  - `Events/EnemySpawnedSignal.cs`: `event Action<EnemySpawnedInfo>`; `EnemySpawnedInfo{Enemy Enemy; bool IsBoss}`; `Enemy` имеет `MaxHealth` и `transform`.
  - `Events/EnemyKilledSignal.cs`: `event Action<EnemyKilledInfo>`; `EnemyKilledInfo{bool IsBoss; int Level}`.
- Сервисы (Zenject, инжектятся в поле/метод `[Inject] Construct(...)`):
  - `Upgrades/TapDamageUpgradeService.cs`: `int Level`, `float NextCost`, `float CurrentDamage`, `bool CanBuy`, `bool TryBuy()`.
  - `Assistants/AssistantService.cs`: `float TotalDps`, `int GetLevel(string id)`, `bool IsUnlocked(AssistantConfig)`, `float GetDps(config)`, `float GetCost(config)`, `bool TryBuy(config)`. Список конфигов: инжект `IReadOnlyList<AssistantConfig>` (забинден в контейнере); `AssistantConfig{string Id; string DisplayName; float BaseDps; float BaseCost; ...}`.
  - `Core/LevelCountService.cs`: `int LevelCount`.
  - `CurrencyController/ICurrencyController.cs`: `event OnCurrencyChange`, `float GetAmount(CurrencyType)`.
- Округление: `MathfRound.Round(float)`. Числа большие — показывай целые (`{(int)value}` или Mathf.FloorToInt).
- TMP: используй `TMPro.TextMeshProUGUI`. Шрифт подставится дефолтный (TMP Settings), не задавай `font` вручную.

## Что сделать (новые файлы в Assets/_Project/Scripts/UI/)

1. **BossTimerView.cs** — `[SerializeField] TextMeshProUGUI _text` (+ опционально RectTransform root для показа/скрытия). Подписка `BossTimerSignal`: IsRunning → показать `"{сек}"` (целые, без десятичных), не бег — скрыть. OnEnable/OnDisable подписки. Стартовое состояние — скрыт.
2. **LevelView.cs** — `[SerializeField] TextMeshProUGUI _text`; `[Inject] Construct(LevelCountService)`; старт: `"Уровень {N}"`; на `LevelCompletedSignal(level)` обновить. NB: сигнал эмитит УЖЕ новый уровень.
3. **UpgradeHubView.cs** — панель прокачки:
   - `[SerializeField] GameObject _panelRoot` (скрыта по умолчанию), `[SerializeField] Button _toggleButton` — открывает/закрывает;
   - `[SerializeField] RectTransform _rowsContainer` (на нём будет VerticalLayoutGroup — добавлю в сцене интегратор; тебе не нужно);
   - строки строит ПРОГРАММНО в Awake: строка = GameObject с HorizontalLayoutGroup + фоновое Image, внутри: TMP имя, TMP уровень/эффект, TMP цена, Button "Купить";
   - строка №0 — урон тапа: имя "Урон", эффект `{CurrentDamage}/тап", уровень `ур.{Level}`, цена `{NextCost}`; кнопка → `TapDamageUpgradeService.TryBuy()`;
   - далее по одной строке на каждого ассистента (только `IsUnlocked` — недоступные не показывать): имя `DisplayName`, эффект `{GetDps} DPS`, уровень, цена `GetCost` (если уровень 0 — кнопка "Нанять", эффект — базовый DPS);
   - доступность: не хватает монет — цена красным (#C33) и кнопка interactable=false; хватает — цена зелёным (#3C3). Обновлять: `UpgradeChangedSignal`, `AssistantChangedSignal`, `ICurrencyController.OnCurrencyChange` + после открытия панели;
   - строки-объекты пересоздавать при каждом апдейте НЕЛЬЗЯ — обновляй тексты/кнопки существующих (пересоздание только если изменился набор анлоков);
   - покупка ассистента после найма меняет кнопку с "Нанять" на "Улучшить" — это просто текст цены+уровня, без пересоздания.
4. **DamagePopupSystem.cs** — всплывающие числа урона у врага:
   - `[SerializeField] Canvas _canvas` (Screen Space Overlay), `[SerializeField] float _lifetime = 0.7f`, `_riseDistance = 60f`, `_fontSize = 36`, разлёт X ±30;
   - подписки: `EnemySpawnedSignal` → кэш текущего `Enemy` и его `MaxHealth`; `EnemyHitSignal` → урон = `DamagePercent * MaxHealth` (кэш), позиция врага `Camera.main.WorldToScreenPoint(enemy.position)`, спавн TMP текста `"-{урон}"` (целое) в `_canvas.transform` с якорем (0,0) и позицией чуть выше центра врага; анимация DOTween: подъём + фейд за `_lifetime`, потом Destroy;
   - пул не обязателен (Destroy достаточно), но не аллоцируй строку интерполяцией в горячем пути без нужды;
   - цвет: обычный тап — белый, если урон >= MaxHealth*0.25 — жёлтый/оранжевый.

## Ограничения
- ТОЛЬКО новые файлы: UI/BossTimerView.cs, UI/LevelView.cs, UI/UpgradeHubView.cs, UI/DamagePopupSystem.cs. Ничего существующего не менять. НЕ трогать сцену/префабы/.asset/LevelInstaller; не git commit; не запускать Unity; не mcp__UnityMCP__*.
- Стиль: 4 пробела, `_camelCase`, `[Inject] private void Construct(...)`, подписки строго симметричны (OnEnable/OnDisable или Subscribe/Dispose), комментарии англ. минимально, namespace `_Project.Scripts.UI`.

## Критерии готовности
- [ ] Все классы компилируются логически: сигналы/сервисы используются ровно с теми сигнатурами, что указаны
- [ ] BossTimerView: 30 → "30".."0", скрыт вне боя
- [ ] UpgradeHubView: без монет кнопки disabled, цены красные; после TryBuy тексты обновились без пересоздания строк
- [ ] DamagePopupSystem: урон 1 тап по HP 10 → текст "-1"; у DPS-тика так же; нет NRE при смерти врага до фейда (кэш чистится)
- [ ] Никаких ссылок на UnityEngine.UI в BossTimerView/LevelView кроме нужного

## Проверка
Без скриншотов — кодом и числами; мысленно прогони цикл: тап → EnemyHitSignal → popup; босс → таймер → таймаут → респавн. Застрял >20 мин — стоп и отчёт.

## Отчёт
Файлы; какие именно ссылки нужно провайрить в инспекторе (полный список serialized-полей по каждому классу); что не вышло.
