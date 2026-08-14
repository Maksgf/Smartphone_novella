// Arcane Afterglow — Unity-native fantasy messenger demo. A tactile obsidian phone remains the desktop focal artifact.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SmartphoneNovella
{
    public sealed class FantasyMessengerBootstrap : MonoBehaviour
    {
        private enum View { Onboarding, Discover, Messages, Profile, Skills, Settings, Chat }

        private readonly Color ink = Hex("#080a16");
        private readonly Color obsidian = Hex("#11172a");
        private readonly Color parchment = Hex("#f7ecda");
        private readonly Color muted = Hex("#9c9cad");
        private readonly Color brass = Hex("#efb34f");
        private readonly Color ember = Hex("#c96e32");
        private readonly Color mulberry = Hex("#4d304d");

        [Header("Project-local visual assets")]
        [SerializeField] private Sprite veylanNightBackdrop;
        [SerializeField] private Sprite emberOrbMark;
        [SerializeField] private Sprite miyaPortrait;
        [SerializeField] private Sprite liaraPortrait;
        [SerializeField] private Sprite astraPortrait;
        [SerializeField] private Sprite leraPortrait;

        private Canvas canvas;
        private CanvasScaler scaler;
        private RectTransform phone;
        private RectTransform phoneScreen;
        private RectTransform headerRoot;
        private RectTransform pageRoot;
        private RectTransform navRoot;
        private RectTransform overlayRoot;
        private Font runtimeFont;
        private View currentView = View.Onboarding;
        private int onboardingStep;
        private int candidateIndex;
        private bool profileCreated;
        private bool matchedMiya;
        private int affection = 18;
        private int xp = 12;
        private int level = 1;
        private int skillPoints = 1;
        private float incomingReplyAt = -1f;
        private string notification = "";
        private int lastWidth;
        private int lastHeight;
        private readonly List<string> conversation = new List<string>
        {
            "Мия: Привет. Я уже заметила твою нить среди огней Вейлана.",
            "Мия: Если не боишься тишины, расскажи, что ищешь этой ночью."
        };

        private readonly Candidate[] candidates =
        {
            new Candidate("Мия", "27", "Кошколюдка · Архивистка луны", "Лунный лицей", "Я собираю невозможные сноски и оставляю окно открытым для созвездий.", "Старые карты · Лунный чай · Тайные двери", "#4c2d49"),
            new Candidate("Лиара", "25", "Эльфийка · Хранительница троп", "Опушка Эшвуда", "После дождя пути слышнее. Я знаю, где начинаются почти все из них.", "Грозы · Дикие тропы · Звёзды", "#274a3b"),
            new Candidate("Астра", "26", "Странница · Гостья между мирами", "Седьмое небо", "Я пришла с разбитым компасом и небом, которого здесь никто не помнит.", "Сигнальные огни · Ночные поезда · Тишина", "#38305e")
        };

        private void Awake()
        {
            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildCanvas();
            BuildWorld();
            BuildHandset();
            ConfigureViewport();
            Render();
        }

        private void Update()
        {
            if (lastWidth != Screen.width || lastHeight != Screen.height)
            {
                ConfigureViewport();
                Render();
            }

            if (incomingReplyAt > 0f && Time.unscaledTime >= incomingReplyAt)
            {
                incomingReplyAt = -1f;
                conversation.Add("Мия: Мне нравится, когда разговор не торопится. Продолжим?");
                affection += 4;
                xp += 4;
                notification = "Новое письмо от Мии";
                if (currentView == View.Chat) Render();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(transform, false);
        }

        private void BuildCanvas()
        {
            var root = new GameObject("Fantasy Messenger Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildWorld()
        {
            var backdrop = ImagePanel(canvas.transform, "Veylan Night", ink);
            Stretch(backdrop.rectTransform, 0f, 0f, 0f, 0f);

            if (veylanNightBackdrop != null)
            {
                backdrop.sprite = veylanNightBackdrop;
                backdrop.color = Color.white;
                backdrop.preserveAspect = false;
            }

            var horizon = ImagePanel(canvas.transform, "City Horizon", veylanNightBackdrop != null ? new Color(.02f, .03f, .08f, .38f) : Hex("#0c1121"));
            Stretch(horizon.rectTransform, 0f, 0f, 0f, 0f);
            var moon = ImagePanel(horizon.transform, "Ember Moon", brass);
            Anchor(moon.rectTransform, .17f, .72f, 126f, 126f);
            var moonGlow = ImagePanel(horizon.transform, "Moon Glow", new Color(brass.r, brass.g, brass.b, .12f));
            Anchor(moonGlow.rectTransform, .17f, .72f, 240f, 240f);

            float[] positions = { .04f, .13f, .22f, .33f, .46f, .58f, .69f, .82f, .92f };
            float[] heights = { 150f, 210f, 116f, 258f, 170f, 232f, 132f, 192f, 144f };
            for (int i = 0; i < positions.Length; i++)
            {
                var building = ImagePanel(horizon.transform, "Veylan silhouette", i % 2 == 0 ? Hex("#0a0d19") : Hex("#0e1323"));
                Anchor(building.rectTransform, positions[i], 0f, 140f, heights[i]);
                var lantern = ImagePanel(building.transform, "Lantern", new Color(brass.r, brass.g, brass.b, .72f));
                Anchor(lantern.rectTransform, .50f, .55f, 5f, 5f);
            }

            var leftCopy = TextLabel(canvas.transform, "ВЕЙЛАН НЕ СПИТ\n\nВыбирай нити,\nкоторые отвечают.", 24, parchment, TextAnchor.MiddleLeft, FontStyle.Bold);
            Anchor(leftCopy.rectTransform, .07f, .48f, 270f, 150f);
            var rightCopy = TextLabel(canvas.transform, "ПРОЛОГ v0.1\nПисьма углей", 18, parchment, TextAnchor.MiddleRight, FontStyle.Bold);
            Anchor(rightCopy.rectTransform, .86f, .76f, 250f, 96f);
        }

        private void BuildHandset()
        {
            phone = ImagePanel(canvas.transform, "Desktop Handset Frame", Hex("#05060b")).rectTransform;
            phoneScreen = ImagePanel(phone, "Obsidian Glass", obsidian).rectTransform;
            headerRoot = CreateRect(phoneScreen, "Phone Header");
            pageRoot = CreateRect(phoneScreen, "Phone Content");
            navRoot = CreateRect(phoneScreen, "Phone Navigation");
            overlayRoot = CreateRect(phoneScreen, "Phone Overlay");
        }

        private void ConfigureViewport()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            bool desktop = Screen.width > Screen.height;
            scaler.referenceResolution = desktop ? new Vector2(1920f, 1080f) : new Vector2(1080f, 1920f);
            if (desktop)
            {
                Anchor(phone, .5f, .5f, 540f, 1012f);
                SetOffsets(phoneScreen, 10f, 10f, 10f, 10f);
            }
            else
            {
                Stretch(phone, 0f, 0f, 0f, 0f);
                SetOffsets(phoneScreen, 0f, 0f, 0f, 0f);
            }
        }

        private void Render()
        {
            Clear(headerRoot);
            Clear(pageRoot);
            Clear(navRoot);
            Clear(overlayRoot);
            RenderHeader();
            if (currentView == View.Onboarding) RenderOnboarding();
            else
            {
                RenderNavigation();
                switch (currentView)
                {
                    case View.Discover: RenderDiscover(); break;
                    case View.Messages: RenderMessages(); break;
                    case View.Profile: RenderProfile(); break;
                    case View.Skills: RenderSkills(); break;
                    case View.Settings: RenderSettings(); break;
                    case View.Chat: RenderChat(); break;
                }
            }
        }

        private void RenderHeader()
        {
            Stretch(headerRoot, 0f, 0f, 0f, 0f);
            var header = ImagePanel(headerRoot, "Header Material", new Color(.07f, .09f, .16f, .98f));
            Stretch(header.rectTransform, 0f, 0f, 0f, 0f);
            header.rectTransform.anchorMin = new Vector2(0f, 1f);
            header.rectTransform.anchorMax = new Vector2(1f, 1f);
            header.rectTransform.pivot = new Vector2(.5f, 1f);
            header.rectTransform.sizeDelta = new Vector2(0f, 102f);
            header.rectTransform.anchoredPosition = Vector2.zero;

            var notch = ImagePanel(header.transform, "Speaker Notch", Hex("#020308"));
            Anchor(notch.rectTransform, .5f, .88f, 136f, 26f);
            var mark = ImagePanel(header.transform, "Ember Signal", brass);
            Anchor(mark.rectTransform, .08f, .42f, 38f, 38f);
            if (emberOrbMark != null)
            {
                mark.sprite = emberOrbMark;
                mark.color = Color.white;
                mark.preserveAspect = true;
            }
            else TextLabel(mark.transform, "✦", 24, Hex("#1f1205"), TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            var title = TextLabel(header.transform, currentView == View.Onboarding ? "DEMO v0.1\nСоздать аккаунт" : "Fantasy\nMESSENGER · ПРОЛОГ v0.1", currentView == View.Onboarding ? 23 : 17, parchment, TextAnchor.MiddleLeft, FontStyle.Bold);
            Anchor(title.rectTransform, .19f, .41f, 264f, 48f);
            var status = TextLabel(header.transform, currentView == View.Onboarding ? $"{onboardingStep + 1} / 3" : (string.IsNullOrWhiteSpace(notification) ? "◌ связь активна" : "✦ новое письмо"), 12, string.IsNullOrWhiteSpace(notification) ? muted : brass, TextAnchor.MiddleRight, FontStyle.Bold);
            Anchor(status.rectTransform, .90f, .41f, 138f, 34f);
        }

        private void RenderOnboarding()
        {
            Stretch(pageRoot, 24f, 34f, 24f, 126f);
            var steps = TextLabel(pageRoot, "АККАУНТ          ТВОЯ ИСТОРИЯ          ОБРАЗ", 11, brass, TextAnchor.UpperCenter, FontStyle.Bold);
            Top(steps.rectTransform, 0f, 48f);
            var heading = TextLabel(pageRoot, onboardingStep == 0 ? "Кем тебя\nзапомнит город?" : onboardingStep == 1 ? "Добавь несколько\nнастоящих слов." : "Выбери лицо\nв переписке.", 46, parchment, TextAnchor.UpperLeft, FontStyle.Bold);
            Top(heading.rectTransform, 84f, 128f);

            if (onboardingStep == 0)
            {
                CreateChoice(168f, "✓   alex@veyla.demo", true, () => { });
                CreateChoice(226f, "max@veil.demo", false, () => { });
                CreateChoice(284f, "dan@ember.demo", false, () => { });
                CreateChoice(372f, "✓   moon-arc-47", true, () => { });
                CreateChoice(430f, "ember-route-9", false, () => { });
                CreateChoice(488f, "starling-22", false, () => { });
            }
            else if (onboardingStep == 1)
            {
                CreateChoice(178f, "✓   Alex", true, () => { });
                CreateChoice(236f, "Max", false, () => { });
                CreateChoice(294f, "Dan", false, () => { });
                var about = TextLabel(pageRoot, "Люблю путешествовать, искать новое и иногда оставлять окно открытым для города.", 18, parchment, TextAnchor.UpperLeft, FontStyle.Italic);
                Top(about.rectTransform, 386f, 150f);
            }
            else
            {
                string[] portraits = { "✦\nAlex", "◌\nMax", "◆\nDan" };
                for (int i = 0; i < portraits.Length; i++)
                {
                    var portrait = ButtonPanel(pageRoot, "Avatar", portraits[i], 15, i == 0 ? new Color(brass.r, brass.g, brass.b, .24f) : new Color(.12f, .14f, .23f, 1f), parchment, () => { });
                    portrait.GetComponent<RectTransform>().anchorMin = new Vector2(.18f + i * .32f, .50f);
                    portrait.GetComponent<RectTransform>().anchorMax = portrait.GetComponent<RectTransform>().anchorMin;
                    portrait.GetComponent<RectTransform>().sizeDelta = new Vector2(126f, 156f);
                }
            }
            var next = ButtonPanel(pageRoot, "Onboarding Continue", onboardingStep < 2 ? "Продолжить   ›" : "Создать профиль   ✦", 16, ember, Hex("#160d04"), () =>
            {
                if (onboardingStep < 2) onboardingStep++;
                else { profileCreated = true; currentView = View.Discover; }
                Render();
            });
            Bottom(next.GetComponent<RectTransform>(), 0f, 60f);
        }

        private void RenderDiscover()
        {
            Stretch(pageRoot, 24f, 116f, 24f, 120f);
            if (candidateIndex >= candidates.Length)
            {
                var complete = TextLabel(pageRoot, "✦\n\nКолода на сегодня закончилась.\nОткрой Письма: взаимные знаки продолжают жить в фоне.", 24, parchment, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(complete.rectTransform, 24f, 84f, 24f, 84f);
                return;
            }
            Candidate girl = candidates[candidateIndex];
            var kicker = TextLabel(pageRoot, $"НОВЫЕ НИТИ · {candidates.Length - candidateIndex} АНКЕТЫ", 11, brass, TextAnchor.UpperLeft, FontStyle.Bold);
            Top(kicker.rectTransform, 0f, 24f);
            var title = TextLabel(pageRoot, "Отклики", 42, parchment, TextAnchor.UpperLeft, FontStyle.Bold);
            Top(title.rectTransform, 24f, 58f);
            var world = TextLabel(pageRoot, girl.world, 11, brass, TextAnchor.UpperRight, FontStyle.Bold);
            Top(world.rectTransform, 24f, 28f);
            var card = ImagePanel(pageRoot, "Stained Glass Candidate", Hex(girl.tint));
            Stretch(card.rectTransform, 0f, 108f, 0f, 148f);
            var portrait = ImagePanel(card.transform, "Character Sigil", new Color(.04f, .05f, .10f, .72f));
            Anchor(portrait.rectTransform, .5f, .70f, 154f, 154f);
            Sprite candidatePortrait = PortraitFor(girl.name);
            if (candidatePortrait != null)
            {
                portrait.sprite = candidatePortrait;
                portrait.color = Color.white;
                portrait.preserveAspect = true;
            }
            else TextLabel(portrait.transform, "✦", 68, parchment, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            var name = TextLabel(card.transform, $"{girl.name}, {girl.age}", 32, parchment, TextAnchor.MiddleCenter, FontStyle.Bold);
            Anchor(name.rectTransform, .5f, .47f, 340f, 46f);
            var role = TextLabel(card.transform, girl.role, 13, muted, TextAnchor.MiddleCenter, FontStyle.Normal);
            Anchor(role.rectTransform, .5f, .40f, 420f, 28f);
            var bio = TextLabel(card.transform, $"“{girl.bio}”", 19, parchment, TextAnchor.MiddleCenter, FontStyle.Italic);
            Anchor(bio.rectTransform, .5f, .24f, 430f, 92f);
            var tags = TextLabel(card.transform, girl.tags, 12, parchment, TextAnchor.MiddleCenter, FontStyle.Normal);
            Anchor(tags.rectTransform, .5f, .10f, 440f, 34f);
            var pass = ButtonPanel(pageRoot, "Pass Signal", "×  Погасить нить", 14, Hex("#212536"), parchment, AdvanceCandidate);
            Bottom(pass.GetComponent<RectTransform>(), 0f, 0f);
            pass.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f); pass.GetComponent<RectTransform>().anchorMax = new Vector2(.44f, 0f); pass.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f); pass.GetComponent<RectTransform>().sizeDelta = new Vector2(-8f, 52f);
            var accept = ButtonPanel(pageRoot, "Accept Signal", "✦  Принять знак", 14, ember, Hex("#180d04"), AcceptCandidate);
            accept.GetComponent<RectTransform>().anchorMin = new Vector2(.48f, 0f); accept.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f); accept.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f); accept.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
        }

        private void RenderMessages()
        {
            Stretch(pageRoot, 24f, 116f, 24f, 120f);
            var kicker = TextLabel(pageRoot, "КОРРЕСПОНДЕНЦИЯ", 11, brass, TextAnchor.UpperLeft, FontStyle.Bold); Top(kicker.rectTransform, 0f, 24f);
            var title = TextLabel(pageRoot, "Письма", 42, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(title.rectTransform, 24f, 58f);
            if (!matchedMiya)
            {
                var empty = TextLabel(pageRoot, "✦\n\nПока тихо\nНайдите взаимный знак в Откликах — первое письмо появится здесь.", 22, muted, TextAnchor.MiddleCenter, FontStyle.Bold);
                Stretch(empty.rectTransform, 12f, 92f, 12f, 90f);
                return;
            }
            var row = ButtonPanel(pageRoot, "Miya Chat", "✦   Мия\n      Мне нравится, когда разговор не торопится.", 17, new Color(.15f, .12f, .21f, 1f), parchment, () => { currentView = View.Chat; notification = ""; Render(); });
            Top(row.GetComponent<RectTransform>(), 106f, 78f);
            var trail = TextLabel(pageRoot, "⌁  Вейлан · связь между мирами активна", 12, muted, TextAnchor.LowerLeft, FontStyle.Normal); Bottom(trail.rectTransform, 0f, 26f);
        }

        private void RenderChat()
        {
            Stretch(pageRoot, 20f, 116f, 20f, 120f);
            var title = TextLabel(pageRoot, "Мия\nв сети сейчас", 22, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(title.rectTransform, 0f, 45f);
            float y = 74f;
            foreach (string line in conversation)
            {
                bool player = line.StartsWith("Ты:");
                var bubble = ImagePanel(pageRoot, player ? "Your Reply" : "Miya Reply", player ? new Color(ember.r, ember.g, ember.b, .42f) : new Color(.15f, .17f, .25f, .92f));
                bubble.rectTransform.anchorMin = new Vector2(player ? .18f : 0f, 1f);
                bubble.rectTransform.anchorMax = new Vector2(player ? 1f : .82f, 1f);
                bubble.rectTransform.pivot = new Vector2(player ? 1f : 0f, 1f);
                bubble.rectTransform.anchoredPosition = new Vector2(player ? 0f : 0f, -y);
                bubble.rectTransform.sizeDelta = new Vector2(0f, 50f);
                var copy = TextLabel(bubble.transform, line, 13, parchment, TextAnchor.MiddleLeft, FontStyle.Normal); Stretch(copy.rectTransform, 14f, 4f, 14f, 4f);
                y += 61f;
            }
            var prompt = TextLabel(pageRoot, "ВЫБЕРИ ОТВЕТ", 11, brass, TextAnchor.LowerLeft, FontStyle.Bold); Bottom(prompt.rectTransform, 122f, 20f);
            var replyA = ButtonPanel(pageRoot, "Warm Reply", "Привет. Рад, что наши дороги всё-таки пересеклись.     +5 ♥ · +8 XP", 12, new Color(.19f, .15f, .14f, 1f), parchment, () => SendReply("Привет. Рад, что наши дороги всё-таки пересеклись.", 5));
            Bottom(replyA.GetComponent<RectTransform>(), 62f, 48f);
            var replyB = ButtonPanel(pageRoot, "Curious Reply", "Что ты нашла сегодня в архиве?     +7 ♥ · +8 XP", 12, new Color(.19f, .15f, .14f, 1f), parchment, () => SendReply("Что ты нашла сегодня в архиве?", 7));
            Bottom(replyB.GetComponent<RectTransform>(), 4f, 48f);
        }

        private void RenderProfile()
        {
            Stretch(pageRoot, 24f, 116f, 24f, 120f);
            var kicker = TextLabel(pageRoot, "ЛИЧНОЕ ДОСЬЕ", 11, brass, TextAnchor.UpperLeft, FontStyle.Bold); Top(kicker.rectTransform, 0f, 24f);
            var title = TextLabel(pageRoot, "Alex", 42, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(title.rectTransform, 24f, 56f);
            var player = ImagePanel(pageRoot, "Player Card", new Color(.12f, .15f, .23f, .96f)); Top(player.rectTransform, 104f, 112f);
            var seal = ImagePanel(player.transform, "Level Seal", brass); Anchor(seal.rectTransform, .11f, .52f, 58f, 58f); TextLabel(seal.transform, level.ToString(), 28, ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            var playerTitle = TextLabel(player.transform, $"Уровень {level}\nНити ведут туда, где нужен ответ.", 18, parchment, TextAnchor.MiddleLeft, FontStyle.Bold); Anchor(playerTitle.rectTransform, .27f, .52f, 310f, 68f);
            var experience = TextLabel(pageRoot, $"ОПЫТ                                      {xp % 30} / 30 XP", 12, brass, TextAnchor.UpperLeft, FontStyle.Bold); Top(experience.rectTransform, 236f, 26f);
            var progress = ImagePanel(pageRoot, "Experience Track", new Color(1f, 1f, 1f, .12f)); Top(progress.rectTransform, 268f, 10f);
            var fill = ImagePanel(progress.transform, "Experience Fill", brass); fill.rectTransform.anchorMin = new Vector2(0f, 0f); fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((xp % 30) / 30f), 1f); fill.rectTransform.offsetMin = Vector2.zero; fill.rectTransform.offsetMax = Vector2.zero;
            var stats = TextLabel(pageRoot, $"{(matchedMiya ? 1 : 0)}\nСВЯЗЕЙ          {conversation.Count}\nПИСЕМ          {candidateIndex}\nЗНАКОВ", 15, parchment, TextAnchor.MiddleCenter, FontStyle.Bold); Top(stats.rectTransform, 298f, 74f);
            var skills = ButtonPanel(pageRoot, "Skills", $"✦    Очки навыков · {skillPoints} доступно\n       Новые уровни открывают сильные ответы.", 15, new Color(brass.r, brass.g, brass.b, .14f), parchment, () => { currentView = View.Skills; Render(); }); Top(skills.GetComponent<RectTransform>(), 386f, 72f);
            var relationships = TextLabel(pageRoot, matchedMiya ? $"СВЯЗИ\n\n✦   Мия                                      ♥ {affection}" : "СВЯЗИ\n\nПоявится после первого взаимного знака.", 16, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(relationships.rectTransform, 478f, 90f);
        }

        private void RenderSkills()
        {
            Stretch(pageRoot, 24f, 116f, 24f, 120f);
            var title = TextLabel(pageRoot, $"Созвездие\nнавыков                      {skillPoints} SP", 38, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(title.rectTransform, 0f, 86f);
            string[] skills = { "✦  Flirt        Lv.1/3\n    Смелые, тёплые ответы звучат увереннее.", "⌁  Empathy      Lv.0/3\n    Замечает эмоциональные сигналы.", "◆  Confidence   Lv.0/3\n    Открывает прямые и дерзкие реплики." };
            for (int i = 0; i < skills.Length; i++)
            {
                int index = i;
                var row = ButtonPanel(pageRoot, "Skill", skills[i], 15, new Color(.12f, .14f, .22f, 1f), parchment, () => UpgradeSkill(index));
                Top(row.GetComponent<RectTransform>(), 108f + i * 92f, 76f);
            }
            var note = TextLabel(pageRoot, "Навыки влияют на варианты ответов прямо в переписке.", 13, muted, TextAnchor.LowerLeft, FontStyle.Italic); Bottom(note.rectTransform, 0f, 42f);
        }

        private void RenderSettings()
        {
            Stretch(pageRoot, 24f, 116f, 24f, 120f);
            var title = TextLabel(pageRoot, "Настройка\nсигнала", 38, parchment, TextAnchor.UpperLeft, FontStyle.Bold); Top(title.rectTransform, 0f, 86f);
            string[] settings = { "Музыка                                      ВКЛ", "Звуковые эффекты                           ВКЛ", "Уведомления                                ВКЛ" };
            for (int i = 0; i < settings.Length; i++)
            {
                var setting = ButtonPanel(pageRoot, "Setting", settings[i], 16, new Color(.12f, .14f, .22f, 1f), parchment, () => { });
                Top(setting.GetComponent<RectTransform>(), 106f + i * 68f, 54f);
            }
            var reset = ButtonPanel(pageRoot, "Reset Demo", "Сбросить демо\nВернуться к созданию профиля", 15, new Color(.35f, .12f, .12f, .62f), parchment, ResetDemo);
            Top(reset.GetComponent<RectTransform>(), 338f, 72f);
        }

        private void RenderNavigation()
        {
            Stretch(navRoot, 0f, 0f, 0f, 0f);
            var nav = ImagePanel(navRoot, "Navigation Material", new Color(.05f, .06f, .12f, .98f));
            nav.rectTransform.anchorMin = Vector2.zero; nav.rectTransform.anchorMax = new Vector2(1f, 0f); nav.rectTransform.pivot = new Vector2(.5f, 0f); nav.rectTransform.sizeDelta = new Vector2(0f, 98f);
            string[] labels = { "◌\nПисьма", "✦\nОтклики", "◇\nДосье" };
            Action[] actions = { () => Go(View.Messages), () => Go(View.Discover), () => Go(View.Profile) };
            for (int i = 0; i < labels.Length; i++)
            {
                var button = ButtonPanel(nav.transform, "Navigation", labels[i], 12, new Color(.10f, .12f, .19f, currentView == (i == 0 ? View.Messages : i == 1 ? View.Discover : View.Profile) ? 1f : .45f), currentView == (i == 0 ? View.Messages : i == 1 ? View.Discover : View.Profile) ? brass : muted, actions[i]);
                var rt = button.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(i / 3f, .12f); rt.anchorMax = new Vector2((i + 1) / 3f, .88f); rt.offsetMin = new Vector2(8f, 0f); rt.offsetMax = new Vector2(-8f, 0f);
            }
        }

        private void AdvanceCandidate()
        {
            candidateIndex++;
            Render();
        }

        private void AcceptCandidate()
        {
            if (candidateIndex == 0)
            {
                matchedMiya = true;
                ShowMatchOverlay();
                return;
            }
            candidateIndex++;
            notification = "Нить принята — ответ может прийти позже";
            Render();
        }

        private void ShowMatchOverlay()
        {
            Stretch(overlayRoot, 0f, 0f, 0f, 0f);
            var dim = ImagePanel(overlayRoot, "Match Dim", new Color(0f, 0f, 0f, .70f)); Stretch(dim.rectTransform, 0f, 0f, 0f, 0f);
            var card = ImagePanel(dim.transform, "Mutual Match", new Color(.15f, .10f, .21f, 1f)); Anchor(card.rectTransform, .5f, .50f, 430f, 438f);
            var title = TextLabel(card.transform, "Взаимная симпатия\nНить сошлась", 34, parchment, TextAnchor.MiddleCenter, FontStyle.Bold); Anchor(title.rectTransform, .5f, .70f, 360f, 105f);
            var sigil = TextLabel(card.transform, "✦     ♥     ◌", 40, brass, TextAnchor.MiddleCenter, FontStyle.Bold); Anchor(sigil.rectTransform, .5f, .48f, 340f, 62f);
            var copy = TextLabel(card.transform, "Ваши знаки совпали. Первое письмо уже ждёт в Письмах.", 16, muted, TextAnchor.MiddleCenter, FontStyle.Normal); Anchor(copy.rectTransform, .5f, .32f, 330f, 70f);
            var open = ButtonPanel(card.transform, "Open Match Chat", "Открыть письмо   ›", 16, ember, ink, () => { currentView = View.Chat; notification = ""; Render(); }); Anchor(open.GetComponent<RectTransform>(), .5f, .16f, 306f, 54f);
        }

        private void SendReply(string reply, int affectionGain)
        {
            conversation.Add("Ты: " + reply);
            affection += affectionGain;
            xp += 8;
            if (xp >= level * 30) { level++; skillPoints++; notification = "Новый уровень — получен очко навыка"; }
            incomingReplyAt = Time.unscaledTime + 1.1f;
            Render();
        }

        private void UpgradeSkill(int skill)
        {
            if (skillPoints < 1) { notification = "Нужно новое очко навыка"; Render(); return; }
            skillPoints--;
            notification = "Навык усилен";
            Render();
        }

        private void ResetDemo()
        {
            onboardingStep = 0;
            candidateIndex = 0;
            profileCreated = false;
            matchedMiya = false;
            affection = 18;
            xp = 12;
            level = 1;
            skillPoints = 1;
            notification = "";
            currentView = View.Onboarding;
            Render();
        }

        private void Go(View destination)
        {
            currentView = destination;
            Render();
        }

        private void CreateChoice(float top, string label, bool selected, Action action)
        {
            var choice = ButtonPanel(pageRoot, "Onboarding Choice", label, 15, selected ? new Color(brass.r, brass.g, brass.b, .18f) : new Color(.09f, .11f, .18f, 1f), parchment, action);
            Top(choice.GetComponent<RectTransform>(), top, 46f);
        }

        private Sprite PortraitFor(string displayName)
        {
            switch (displayName)
            {
                case "Мия": return miyaPortrait;
                case "Лиара": return liaraPortrait;
                case "Астра": return astraPortrait;
                case "Лера": return leraPortrait;
                default: return null;
            }
        }

        private Image ImagePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text TextLabel(Transform parent, string content, int size, Color color, TextAnchor alignment, FontStyle style)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = runtimeFont;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button ButtonPanel(Transform parent, string name, string label, int fontSize, Color background, Color foreground, Action action)
        {
            var image = ImagePanel(parent, name, background);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1.15f);
            colors.pressedColor = new Color(.86f, .86f, .86f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.onClick.AddListener(() => action?.Invoke());
            var copy = TextLabel(image.transform, label, fontSize, foreground, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(copy.rectTransform, 10f, 5f, 10f, 5f);
            return button;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
        }

        private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetOffsets(RectTransform rect, float left, float bottom, float right, float top) => Stretch(rect, left, bottom, right, top);

        private static void Anchor(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x, y);
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void Top(RectTransform rect, float margin, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = new Vector2(0f, -margin);
        }

        private static void Bottom(RectTransform rect, float margin, float height)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(.5f, 0f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = new Vector2(0f, margin);
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        private readonly struct Candidate
        {
            public readonly string name;
            public readonly string age;
            public readonly string role;
            public readonly string world;
            public readonly string bio;
            public readonly string tags;
            public readonly string tint;
            public Candidate(string name, string age, string role, string world, string bio, string tags, string tint)
            {
                this.name = name; this.age = age; this.role = role; this.world = world; this.bio = bio; this.tags = tags; this.tint = tint;
            }
        }
    }

    internal static class RectTransformExtensions
    {
        public static void Stretch(this RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
