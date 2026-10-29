# T08: Meta-UI — оффлайн-попап, достижения, daily rewards, квесты (вьюхи)

## Цель
Четыре вьюхи под уже описанные контракты сервисов (сервисы пишутся параллельно другим воркером — НЕ жди их, используй контракты ниже; сверить реальный код можно по указанным путям после появления файлов, но сигнатуры зафиксированы).

## Контракты сервисов (фиксированы!)
### OfflineProgressService (`Core/OfflineProgressService.cs`)
- `float PendingCoins`, `double AwaySeconds`, `void Claim()`
- событие-триггер `OfflineProgressSignal.OnOfflineProgress` (Action) — после Claim

### AchievementService (`Achievements/AchievementService.cs`)
- `IReadOnlyList<AchievementEntry> Entries`; `AchievementEntry{string Id; string DisplayName; string GoalText; float Goal; float Progress; bool IsUnlocked; int DiamondReward}`
- `AchievementUnlockedSignal.OnAchievementUnlocked` : `Action<AchievementInfo{Id, DisplayName, DiamondReward}>`
- `AchievementsChangedSignal.OnAchievementsChanged` : `Action`

### DailyRewardService (`Core/DailyRewardService.cs`)
- `bool CanClaim`, `int CurrentStreak`, `int NextDay` (1..7), `void Claim()`, `string DescribeDay(int day1to7)`
- `DailyRewardClaimedSignal(Action<DailyRewardInfo{Day, RewardType, RewardAmount, Streak}>)`, `DailyChangedSignal(Action)`

### DailyQuestService (`Core/DailyQuestService.cs`)
- `IReadOnlyList<DailyQuest> Quests`; `DailyQuest{Id, Description, Goal, Progress, Completed, Claimed, RewardType, RewardAmount, BonusDiamonds}`
- `bool TryClaim(DailyQuest)`, `double SecondsRemaining`
- `QuestsChangedSignal(Action)`

Общие: валюта `ICurrencyController` (`GetAmount`, `OnCurrencyChange`, `CurrencyType`); TMP (`TMPro.TextMeshProUGUI`, шрифт подставится дефолтный — НЕ задавай `font`); DOTween; Zenject `[Inject] private void Construct(...)`; числа целые (`Mathf.FloorToInt`); валюту显示 "💎 N" для алмазов, "N" для монет.

## Что сделать (новые файлы в Assets/_Project/Scripts/UI/)

1. **OfflineEarningsView.cs** — попап по центру при старте, если было оффлайн-начисление:
   - `[SerializeField] GameObject _panelRoot`, `TextMeshProUGUI _amountText`, `TextMeshProUGUI _awayText`, `Button _claimButton`
   - OnEnable: если `PendingCoins > 0` → показать: "Пока вас не было", "{N} монет", "Вас не было {H ч M мин" (из AwaySeconds; <1ч → "{M мин"); Claim по кнопке → скрыть панель (подпишись и на OfflineProgressSignal — по нему тоже скрыть)
   - Claim-обработчик добавляется в Awake (`_claimButton.onClick.AddListener`)

2. **AchievementsView.cs** — панель со списком:
   - `[SerializeField] Button _toggleButton`, `GameObject _panelRoot`, `RectTransform _rowsContainer` (VerticalLayoutGroup добавит интегратор)
   - Строки ПРОГРАММНО (образец: `UpgradeHubView.CreateRow` — Image-фон + HorizontalLayoutGroup + LayoutElement): TMP имя, TMP прогресс "{progress}/{goal}", TMP "{💎N}", статус-текст ("✓" если IsUnlocked)
   - Обновление: AchievementsChangedSignal + при открытии; строки пересоздавать ТОЛЬКО если их count изменился, иначе обновлять тексты
   - Unlock во время игры: AchievementsUnlockedSignal → всплывающий тост: программный TMP поверх экрана (верхний край, якорь top-stretch), "Достижение: {name} +💎{n}", DOTween показ 2с + фейд, Destroy

3. **DailyRewardsView.cs** — панель 7 дней:
   - `[SerializeField] Button _toggleButton`, `GameObject _panelRoot`, `RectTransform _rowsContainer`
   - 7 строк: "День N" + описание награды (`DescribeDay`) + статус: NextDay и CanClaim → кнопка "Забрать" активна (по ней `Claim()`), забранные дни "✓", будущие — затемнены (alpha 0.4)
   - Обновление: DailyChangedSignal + при открытии. Ряды создавать один раз (7 штук), обновлять статусы
   - На toggle-кнопке индикатор: если CanClaim — анимация пульсации scale DOTween (yoyo, ∞; стоп при открытии панели/потере CanClaim)

4. **DailyQuestsView.cs** — панель квестов:
   - `[SerializeField] Button _toggleButton`, `GameObject _panelRoot`, `RectTransform _rowsContainer`
   - Строка на квест: описание, "{progress}/{goal}", кнопка "Забрать" (активна только если Completed && !Claimed → `TryClaim(quest)`), Claimed → "✓"
   - Заголовок панели: таймер до сброса — "{H:MM} до обновления" (SecondsRemaining); обновлять раз в секунду (корутина или Time.unscaledTime-кэш в Update; только пока панель открыта)
   - Обновление: QuestsChangedSignal + при открытии; строки пересоздавать только если поменялся состав (id-строки)

## Ограничения
- ТОЛЬКО перечисленные 4 файла. НЕ трогать сервисы/события/сцену/LevelInstaller/чужое; не git commit; не Unity; не mcp__UnityMCP__*.
- Стиль: 4 пробела, `_camelCase`, симметричные подписки, namespace `_Project.Scripts.UI`, комментарии англ. минимально.

## Критерии
- [ ] OfflineEarningsView: PendingCoins=36000, Away=10800 → "36000 монет", "3 ч 0 мин"; Claim → панель скрыта
- [ ] AchievementsView: 5 строк, прогресс живой; unlock → тост появился и уничтожился через ~2с
- [ ] DailyRewardsView: день 3 CanClaim → только кнопка у дня 3 активна; забранные ✓
- [ ] DailyQuestsView: boss2 Completed → кнопка активна; TryClaim → ✓; таймер тикает только при открытой панели
- [ ] Нигде нет обращений к сервисам до инжекта (OnEnable guard или проверка на null, как в образце UpgradeHubView)

## Проверка
Без скриншотов — кодом; мысленно прогони полный цикл дня. Застрял >20 мин — стоп и отчёт.

## Отчёт
Файлы; serialized-поля списком; что не вышло.
