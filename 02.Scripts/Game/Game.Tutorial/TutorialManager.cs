using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Game.Magic;
using Game.Scroll;
using Game.Request;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using Game.Desk;

namespace Game.Tutorial
{
    public class TutorialStep
    {
        //public string Text { get; set; }                        // 표시할 튜토리얼 텍스트
        public LocalizedString Text { get; set; }               // 다국어 텍스트 참조
        public Vector2 UIPosition { get; set; }                 // UI 창 위치(RectTransform Pos)
        public float UIWidth { get; set; }                      // UI 창 너비
        public float UIHeight { get; set; }                     // UI 창 높이
        public bool isShown { get; set; }                       // 설명 UI 보여짐 여부
        public bool HasConfirmButton { get; set; }              // 확인 버튼 유무
        public bool HasSkipButton { get; set; }                 // 튜토리얼 스킵 버튼 유무
        public bool isHighlight { get; set; }                   // 하이라이팅 패널 유무
        public RectTransform highlightingObject { get; set; }   // 하이라이팅 할 오브젝트
        public RectTransform activeObject { get; set; }         // 활성화 시킬 오브젝트(올빼미 같은거)
        public List<Button> ButtonsToDisable { get; set; }      // 비활성화 버튼 리스트

        public int FontSize { get; set; } = 22;                                             // 기본 폰트 크기
        public TextAlignmentOptions Alignment { get; set; } = TextAlignmentOptions.Left;    // 기본 정렬 (왼쪽 정렬)
        public float LeftMargin { get; set; } = 15;                                         // 기본 왼쪽 마진
    }

    public class TutorialManager : Singleton<TutorialManager>
    {
        private bool tutorialComplete = false;
        public bool TutorialComplete
        {
            get { return tutorialComplete; }
            set { tutorialComplete = value; }
        }

        [Header("캔버스")]
        public Canvas canvas;

        [Header("튜토리얼 UI창")]
        public RectTransform tutorialUI;        // 튜토리얼 UI 창
        public Animator tutorialAnimator;       // Animator 컴포넌트
        public TextMeshProUGUI tutorialTextUI;  // 텍스트 UI
        public Button confirmButton;            // 확인 버튼
        public Button skipButton;               // 스킵 버튼
        public RectTransform skipCheckUI;       // 스킵 확인 창
        public Button foldButton;               // UI 접기 버튼(스크롤 롤)
        private Vector2 previousPosition = Vector2.zero; // 이전 UIPosition 저장
        private bool isFold = false;            // 접혔는지 확인하는 부울
        private bool whileFoldAnim = false;     // Fold Anim 실행중인지 확인하는 부울
        private bool isStepping = false;        // 스텝 진행으로 인한 튜토리얼 UI의 접힘 펼침 과정 중임을 나타내기 위한 부울

        [Header("침대 씬 하이라이팅")]
        public RectTransform highlightingPanel; // 하이라이팅 패널
        public RectTransform dateUIImage;       // 달력 UI(튜토리얼 용 빈 껍데기)
        public RectTransform deskUIImage;       // 책상 UI(튜토리얼 용 빈 껍데기)
        public RectTransform windowUIImage;     // 창문 UI(튜토리얼 용 빈 껍데기)
        public RectTransform questBoardUIImage; // 퀘스트 보드 UI(튜토리얼 용 빈 껍데기)
        public RectTransform bedUIImage;        // 침대 UI(튜토리얼 용 빈 껍데기)
        public RectTransform shopUIImage;       // 상점 UI(튜토리얼 용 빈 껍데기)

        [Header("책상 씬 하이라이팅")]
        public RectTransform scrollUIImage;     // 스크롤 UI(튜토리얼 용 빈껍데기)
        public RectTransform inkdrawerUIImage;  // 잉크 서랍 UI(튜토리얼 용 빈껍데기)
        public RectTransform inkUIImage;        // 잉크 UI(튜토리얼 용 빈껍데기)
        public RectTransform drawerUIImage;     // 인벤토리 서랍 UI(튜토리얼 용 빈껍데기)
        public RectTransform recipeUIImage;     // 레시피 스크롤 UI(튜토리얼 용 빈껍데기)
        public RectTransform stampUIImage;      // 도장 UI(튜토리얼 용 빈껍데기)
        public RectTransform goBedUIImage;      // 침실 버튼 UI(얘는 기능 있어야 함)

        [Header("실제 기능 있는 오브젝트")]
        // public RectTransform reinforceRune;     // 강화 버튼
        // public RectTransform weakeningRune;     // 약화 버튼
        // public RectTransform expansionRune;     // 확장 버튼
        // public RectTransform reductionRune;     // 축소 버튼
        public RectTransform runeDrawer;

        [Header("호버링 UI")]
        // 마우스 호버링 시 나타나는 UI
        public GameObject hoverUI; // 호버링 UI 프리팹
        private bool isHovering = false; // 현재 마우스가 버튼 위에 있는지 여부

        [Header("비활성화 할 버튼 목록")]
        public Button dateButton;               // 달력 버튼
        public Button bedButton;                // 침대 버튼
        public Button questBoard;               // 퀘스트 보드 버튼
        public Button bedRoomButton;            // 침실 가는 버튼
        public Button fireInkButton;            // 불의 기운 잉크
        public Button airInkButton;             // 공기의 기운 잉크
        public Button waterInkButton;           // 물의 기운 잉크
        public Button earthInkButton;           // 땅의 기운 잉크
        public Button reinforceButton;          // 강화의 룬석
        public Button weakeningButton;          // 약화의 룬석
        public Button expansionButton;          // 확장의 룬석
        public Button reductionButton;          // 축소의 룬석
        public Button recipeBookButton;         // 레시피 북
        public Button stampButton;              // 인장

        [Header("튜토리얼 스텝")]
        #region 튜토리얼 스텝
        [SerializeField] private int currentStepIndex = 0;
        public int CurrentStepIndex => currentStepIndex;
        private List<TutorialStep> tutorialSteps = new List<TutorialStep>()
        {
            new TutorialStep {  // 0
                //Text = "마법 학교를 졸업한 당신, \n이제 학자금 대출 상환이\n시작될 날이 머지않았습니다.\n\n불경기로 취업에 실패한 당신은\n대출을 갚기 위해 작은 사업을\n시작하기로 결심합니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_0" },
                UIPosition = new Vector2(0, 0), // 중앙
                UIWidth = 800,
                UIHeight = 800,
                HasConfirmButton = true,
                isHighlight = true,
                FontSize = 28,                              // 폰트 크기
                Alignment = TextAlignmentOptions.Center,    // 중앙 정렬
                LeftMargin = 0                              // 왼쪽 마진
            },
            new TutorialStep {  // 1
                //Text = "학교에서 배운 마법 지식을 활용해 사람들의 의뢰를 받아 \n마법 스크롤을 제작하며, 점차 빚을 갚아 나가 봅시다.\n\n어쩌면 당신의 재능 덕분에 예상\n보다 빨리 부자가 될지도 모릅니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_1" },
                UIPosition = new Vector2(0, 0),
                UIWidth = 800,
                UIHeight = 800,
                HasConfirmButton = true,
                isHighlight = true,
                FontSize = 28,                              // 폰트 크기
                Alignment = TextAlignmentOptions.Center,    // 중앙 정렬
                LeftMargin = 0                              // 왼쪽 마진
            },
            new TutorialStep {  // 2
                //Text = "이제 당신만의 여정을 시작하세요.\n번영과 행운이 함께하길!",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_2" },
                UIPosition = new Vector2(0, 0),
                UIWidth = 800,
                UIHeight = 800,
                HasConfirmButton = true,
                HasSkipButton = true,      // 일단 스킵 구현 안해서 false로 해 놓음.
                isHighlight = true,
                FontSize = 28,                              // 폰트 크기
                Alignment = TextAlignmentOptions.Center,    // 중앙 정렬
                LeftMargin = 0                              // 왼쪽 마진
            },
            new TutorialStep {  // 3
                //Text = "당신은 정해진 날에\n대출금을 나눠 갚아야 합니다.\n달력을 클릭하면 \n날짜를 확인할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_3" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후, 할당.
            },
            new TutorialStep {  // 4
                //Text = "당신의 작업 공간인 책상입니다.\n이곳에서 당신은 스크롤을 만들 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_4" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 5
                //Text = "이곳은 창문입니다.\n올빼미가 이곳으로 날아와,\n당신에게 의뢰를 전달해 줄겁니다.\n완성한 스크롤도 이 창문을 통해\n올빼미에게 전달할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_5" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 6
                //Text = "책상에서 일어날 필요 없습니다.\n앉아서 편하게 일하세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_6" },
                UIPosition = new Vector2(-260, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 7
                //Text = "이곳은 당신의 침대입니다.\n별로 좋아보이진 않지만 아직은\n비싼 침대를 살 돈이 없습니다.\n클릭하면 다음 날이 되어\n새 의뢰를 받을 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_7" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 8
                //Text = "그럼 이제 책상에서 일을 시작해 볼까요?",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_8" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
                ButtonsToDisable = new List<Button>{}       // 달력, 침대 버튼
            },  // 여기까지가 침대 씬 튜토리얼.
            new TutorialStep {  // 9
                //Text = "가운데 종이는\n마법진을 새길 양피지입니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_9" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 10
                //Text = "마법의 잉크가 놓여질\n서랍입니다.", -> 잉크 내용도 추가됨
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_10" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 11
                //Text = "불, 공기, 물, 땅의 기운이 담긴\n마법의 잉크입니다.\n잉크를 찍어 양피지 위에 놓는다면\n기운이 담긴 문양이 새겨집니다.", -> 인벤 설명으로 변경
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_11" },
                UIPosition = new Vector2(300, -300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 12
                //Text = "마법 속성의 레시피가 저장될\n레시피 북입니다.\n당신이 만들어 본 속성이\n자동으로 기록됩니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_12" },
                UIPosition = new Vector2(300, -300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당.
            },
            new TutorialStep {  // 13
                //Text = "다음으로 창문을 열고\n부엉이가 가져온 편지를 클릭해봅시다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_13" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
                activeObject = null,     // 실행 후 할당.
                ButtonsToDisable = new List<Button>{}   // 침실 버튼
            },
            new TutorialStep {  // 14
                //Text = "교수님이 알려주신 예시대로\n기운 2개를 조합해 봅시다.\n잉크를 찍은 뒤, 양피지 위에 놓으세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_14" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 15
                //Text = "조합된 기운이 마법 속성을 만들 수 있다면, 속성에 맞는 \n마법의 이름이 새겨지고\n두 기운을 감싸는 원이 생기게 됩니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_15" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 16
                //Text = "이제 속성을 결정 짓는\n부여를 해보도록 하겠습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_16" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 17
                //Text = "마법의 이름을 클릭하여\n부여를 할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_17" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 18
                //Text = "이제 도장을 사용해\n스크롤을 확정 지어봅시다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_18" },
                UIPosition = new Vector2(300, -300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 19
                //Text = "스크롤 위에 도장을 찍어주세요.\n스크롤을 확정 지을 때\n속성이 두 개 이상이라면\n확정이 되지 않으니 주의하세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_19" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 20
                //Text = "이제 완성한 스크롤을 교수님께 보내어,\n아직 수업 내용을 잊지 않았다는 것을\n보여주세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_20" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 21
                //Text = "완성된 스크롤의 윗부분을 클릭한 뒤,\n의뢰서 위에 올려 놓고\n부엉이에게 전해줍니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_21" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 22
                //Text = "다음 의뢰가 도착했습니다.\n남은 의뢰를 마무리하고\n하루를 마칩시다.", // 힌트 알려주는 내용 추가함.
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_22" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 23
                //Text = "혹시 새 속성을 해금하기 힘들다면\n레시피 북의 돋보기를 눌러\n힌트를 사용해보세요.",   // 침대 알려주는 내용으로 변경함.
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_23" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
                highlightingObject = null   // 실행 후 할당. 침대방 버튼 할당할거임
            },
            new TutorialStep {  // 24
                //Text = "이제 강화와 약화, 연장과 단축 주문을\n사용할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_24" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 25
                //Text = "교수님께 기억을 잊지 않았음을 보여줄\n마법 스크롤을 만들어 볼까요?",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_25" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 26
                //Text = "기운들을 배치하고 부여까지 완료하세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_26" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 27
                //Text = "훌륭합니다.\n이제 다음 단계로 넘어 가 보겠습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_27" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 28
                //Text = "강화와 약화는 강화의 룬석과 약화의 룬석을\n클릭하여 할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_28" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 29
                //Text = "속성을 강화해 보세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_29" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 30
                //Text = "속성을 약화해 보세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_30" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 31
                //Text = "이제 연장과 단축로 넘어가겠습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_31" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 32
                //Text = "속성을 연장해보세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_32" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 33
                //Text = "속성을 단축해보세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_33" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 34
                //Text = "이제 강화와 약화, 연장과 단축을\n원하는대로 하여 스크롤을 완성하고,\n부엉이에게 전달해주세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_34" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = false,
                isHighlight = false,
            },
            new TutorialStep {  // 35
                //Text = "고생하셨습니다.\n이제 남은 의뢰를 계속해주세요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_35" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 36
                //Text = "이런, 이 의뢰는 지금의 양피지로는\n해결할 수 없군요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_36" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 37
                //Text = "이번엔 특별히 제가 양피지를\n업그레이드 해드릴게요.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_37" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 38
                //Text = "이제 양피지에 기운을\n3개까지 새길 수 있게 되었습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_38" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 39
                //Text = "기운 3개를 새겨 속성을 만들 때에는\n기운 3개를 겹치면 안되고,\n기운 2개로 생성된 외접원에\n나머지 하나를 겹쳐야 합니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_39" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            },
            new TutorialStep {  // 40
                //Text = "이제 기운 3개를 사용하여\n의뢰에 맞는 마법을 만들어보세요.\n스크롤 제작 숙련도가 증가한다면\n앞으로 더 많은 기운을 사용할 수 있습니다.",
                Text = new LocalizedString { TableReference = "TutorialTable", TableEntryReference = "step_40" },
                UIPosition = new Vector2(-650, 300),
                UIWidth = 650,
                UIHeight = 500,
                HasConfirmButton = true,
                isHighlight = true,
            }
        };
        #endregion

        void Awake()
        {
            // 언어 변경 이벤트 구독
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            tutorialComplete = GameData.SaveInfo?.tutorialComplete ?? tutorialComplete;
        }

        private void OnDestroy()
        {
            // 언어 변경 이벤트 구독 해제
            LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        }

        // 언어 변경 시 호출되는 메서드
        private void OnLanguageChanged(UnityEngine.Localization.Locale locale)
        {
            Debug.Log($"Language changed to: {locale.LocaleName}");

            // 현재 스텝의 텍스트를 강제로 갱신
            var currentStep = tutorialSteps[currentStepIndex];
            if (currentStep.Text != null)
            {
                var localizedStringEvent = tutorialTextUI.GetComponent<LocalizeStringEvent>();
                if (localizedStringEvent != null)
                {
                    localizedStringEvent.StringReference = currentStep.Text;
                    localizedStringEvent.RefreshString(); // 강제 갱신
                }
            }
        }

        void Start()
        {
            if (!tutorialComplete && DayManager.Instance.day == 0)
            {
                // 초기화: 스텝 별 하이라이팅 오브젝트 설정
                InitializeHighlightingSteps();
                // 초기화: 스텝 별 비활성화 버튼 설정
                InitializeDisableButton();

                // 강화 약화 확장 축소 룬석 비활성화.(스텝 진행하다 활성화 시킴. 스킵해도 활성화 시켜야 함)
                SetFalseTutorialRuneStone();

                // 도장, 레시피 북 드래그 비활성화.(스텝 진행하다 활성화 시킴. 스킵해도 활성화 시켜야 함)
                SetFalseStampDrag();
                SetFalseRecipeDrag();

                RefreshTutorialLanguage();

                ShowTutorialStep(currentStepIndex);
            }
            else if (!tutorialComplete && DayManager.Instance.day == 1)
            {
                DisableHighlightingPanel();
                DisableScroll();

                currentStepIndex = 24;
                // ShowTutorialStep(currentStepIndex);
            }
            else
            {
                tutorialComplete = true;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 하이라이팅에 필요한 오브젝트 설정
        /// </summary>
        private void InitializeHighlightingSteps()
        {
            // 튜토리얼 스텝별 하이라이팅 오브젝트 설정
            tutorialSteps[3].highlightingObject = dateUIImage;      // 달력 하이라이팅
            tutorialSteps[4].highlightingObject = deskUIImage;      // 책상 하이라이팅
            tutorialSteps[5].highlightingObject = windowUIImage;    // 창문 하이라이팅
            tutorialSteps[6].highlightingObject = questBoardUIImage;// 퀘스트 보드 하이라이팅
            tutorialSteps[7].highlightingObject = bedUIImage;       // 침대 하이라이팅

            tutorialSteps[9].highlightingObject = scrollUIImage;    // 스크롤 하이라이팅
            tutorialSteps[10].highlightingObject = inkdrawerUIImage;// 잉크 서랍 하이라이팅
            tutorialSteps[11].highlightingObject = drawerUIImage;   // 인벤토리 서랍 하이라이팅
            tutorialSteps[12].highlightingObject = recipeUIImage;   // 레시피 북 하이라이팅
            tutorialSteps[18].highlightingObject = stampUIImage;    // 도장 하이라이팅
            tutorialSteps[23].highlightingObject = goBedUIImage;    // 침실 버튼 하이라이팅(얘는 기능 있어야 함)
        }

        private void InitializeDisableButton()
        {
            tutorialSteps[8].ButtonsToDisable = new List<Button> { dateButton, bedButton, questBoard };

            tutorialSteps[13].ButtonsToDisable = new List<Button> { bedRoomButton, fireInkButton, airInkButton, waterInkButton, earthInkButton, stampButton, recipeBookButton };
            tutorialSteps[14].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton, recipeBookButton };
            tutorialSteps[17].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton, recipeBookButton };
            tutorialSteps[19].ButtonsToDisable = new List<Button> { bedRoomButton, recipeBookButton };
            tutorialSteps[21].ButtonsToDisable = new List<Button> { bedRoomButton };
            tutorialSteps[21].ButtonsToDisable = new List<Button> { bedRoomButton, recipeBookButton };
            tutorialSteps[24].ButtonsToDisable = new List<Button> { stampButton };
            tutorialSteps[26].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton };
            tutorialSteps[29].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton };
            tutorialSteps[30].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton };
            tutorialSteps[32].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton };
            tutorialSteps[33].ButtonsToDisable = new List<Button> { bedRoomButton, stampButton };
            tutorialSteps[34].ButtonsToDisable = new List<Button> { bedRoomButton };
        }

        public void RefreshTutorialLanguage()
        {
            // 현재 스텝 텍스트 갱신
            var localizedStringEvent = tutorialTextUI.GetComponent<LocalizeStringEvent>();
            if (localizedStringEvent != null)
            {
                // 현재 스텝의 로컬라이즈 문자열 다시 설정
                TutorialStep currentStep = tutorialSteps[currentStepIndex];
                localizedStringEvent.StringReference = currentStep.Text;

                // 텍스트를 다시 갱신
                localizedStringEvent.RefreshString();
            }
        }

        private void SetFalseTutorialRuneStone()
        {
            runeDrawer.gameObject.SetActive(false);
            // reinforceRune.gameObject.SetActive(false);
            // weakeningRune.gameObject.SetActive(false);
            // expansionRune.gameObject.SetActive(false);
            // reductionRune.gameObject.SetActive(false);
        }

        private void SetTrueTutorialRuneStone()
        {
            runeDrawer.gameObject.SetActive(true);
            // reinforceRune.gameObject.SetActive(true);
            // weakeningRune.gameObject.SetActive(true);
            // expansionRune.gameObject.SetActive(true);
            // reductionRune.gameObject.SetActive(true);
        }

        private void SetFalseStampDrag()
        {
            DragableObject dragObject = stampButton.gameObject.GetComponent<DragableObject>();
            if (dragObject != null)
            {
                dragObject.DisableDragScript();
            }
            else
            {
                Debug.LogError("DragableObject 컴포넌트를 찾을 수 없습니다!");
            }
        }

        private void SetTrueStampDrag()
        {
            DragableObject dragObject = stampButton.gameObject.GetComponent<DragableObject>();
            if (dragObject != null)
            {
                dragObject.EnableDragScript();
            }
            else
            {
                Debug.LogError("DragableObject 컴포넌트를 찾을 수 없습니다!");
            }
        }

        private void SetFalseRecipeDrag()
        {
            DragableObject dragObject = recipeBookButton.gameObject.GetComponent<DragableObject>();
            if (dragObject != null)
            {
                dragObject.DisableDragScript();
            }
            else
            {
                Debug.LogError("DragableObject 컴포넌트를 찾을 수 없습니다!");
            }
        }

        private void SetTrueRecipeDrag()
        {
            DragableObject dragObject = recipeBookButton.gameObject.GetComponent<DragableObject>();
            if (dragObject != null)
            {
                dragObject.EnableDragScript();
            }
            else
            {
                Debug.LogError("DragableObject 컴포넌트를 찾을 수 없습니다!");
            }
        }

        public void DayOneTutoReset()
        {
            // 도장 인벤에 넣기
            DragableObject dragObject = stampButton.gameObject.GetComponent<DragableObject>();
            if (dragObject != null)
            {
                dragObject.LocateStamp();
            }
            // 도장 드래그 못하게 하기
            SetFalseStampDrag();

            // 도장 버튼 비활성화 하기
            DisableButtons(tutorialSteps[24].ButtonsToDisable); // 24 스텝
        }

        public void ShowTutorialStep(int index)
        {
            isStepping = true;
            confirmButton.interactable = false;     // 확인 버튼 연타 방지

            if (currentStepIndex > 0)
            {
                var previousStep = tutorialSteps[currentStepIndex - 1];
                if (previousStep.ButtonsToDisable != null && previousStep.ButtonsToDisable.Count > 0)
                {
                    EnableButtons(previousStep.ButtonsToDisable);
                }
            }

            TutorialStep step = tutorialSteps[index];   // 현재 스텝에 해당하는 TutorialStep

            // if (step.isHighlight)
            //     EnableHighlightingPanel();

            if (!tutorialUI.gameObject.activeSelf)
                tutorialUI.gameObject.SetActive(true);

            // 이미 보여진 스텝이라면 아무 작업도 하지 않음
            if (step.isShown)
            {
                Debug.Log($"Step {index} is already shown. Skipping.");
                return;
            }

            // 하이라이팅 패널 활성화 설정
            if (step.isHighlight)
            {
                EnableHighlightingPanel();
            }
            else
            {
                DisableHighlightingPanel();
            }

            // 스텝별 이벤트 세팅
            if (currentStepIndex == 19)     // 도장 찍을 차례에 도장 드래그 활성화
            {
                SetTrueStampDrag();
            }
            if (currentStepIndex == 21)     // 레시피 북 힌트 알려준 시점에 드래그 활성화
            {
                SetTrueRecipeDrag();
            }
            if (currentStepIndex == 24)     // 교수님의 2번째 편지 읽으면 강화 룬석 활성화
            {
                SetTrueTutorialRuneStone();
            }
            if (currentStepIndex == 34)     // 강화 마음대로 하라고 하고 드래그 활성화
            {
                SetTrueStampDrag();
            }
            if (currentStepIndex == 38)     // 양피지 업그레이드(2->3)
            {
                ScrollCreationManager.Instance.UpgradeScroll(2);
            }

            // 스텝이 보여진 상태로 설정
            step.isShown = true;

            // 비활성화할 버튼 처리
            if (step.ButtonsToDisable != null && step.ButtonsToDisable.Count > 0)
            {
                DisableButtons(step.ButtonsToDisable);
            }

            // 이전 스텝에서 하이라이팅 되어 있는 UI 비활성화
            // HighlightingPanel의 모든 자식 오브젝트 비활성화
            foreach (Transform child in highlightingPanel)
            {
                if (child.name == "Frame")
                    continue;
                child.gameObject.SetActive(false);
            }

            //tutorialTextUI.text = step.Text;            // UI에 표시되는 텍스트 갱신
            // `LocalizedStringEvent`를 사용하여 로컬라이즈된 텍스트 설정
            var localizedStringEvent = tutorialTextUI.GetComponent<LocalizeStringEvent>();

            if (localizedStringEvent == null)
            {
                localizedStringEvent = tutorialTextUI.gameObject.AddComponent<LocalizeStringEvent>();
            }

            // 로컬라이즈 텍스트 설정 및 강제 갱신
            localizedStringEvent.StringReference = step.Text;
            localizedStringEvent.RefreshString();

            // 비동기로 값을 직접 설정
            step.Text.GetLocalizedStringAsync().Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    tutorialTextUI.text = handle.Result; // 직접 텍스트 갱신
                    //Debug.Log($"Tutorial text updated to: {tutorialTextUI.text}");
                }
                else
                {
                    Debug.LogError($"Failed to load localized string for step {index}");
                }
            };

            // 텍스트 스타일 업데이트
            tutorialTextUI.fontSize = step.FontSize;                           // 폰트 크기 설정
            tutorialTextUI.alignment = step.Alignment;                        // 텍스트 정렬 설정
            tutorialTextUI.margin = new Vector4(step.LeftMargin, 0, 0, 0);    // 왼쪽 마진 설정

            // UI 창의 너비, 높이를 단계별로 설정
            Vector2 newSize = tutorialUI.sizeDelta;
            newSize.x = step.UIWidth;
            newSize.y = step.UIHeight;
            tutorialUI.sizeDelta = newSize;

            confirmButton.gameObject.SetActive(step.HasConfirmButton);  // 확인 버튼 활성화 비활성화 결정
            skipButton.gameObject.SetActive(step.HasSkipButton);        // 스킵 버튼 활성화 비활성화 결정

            // 오브젝트 하이라이팅
            if (step.highlightingObject != null)
            {
                step.highlightingObject.gameObject.SetActive(true);
            }

            // 오브젝트 활성화
            if (step.activeObject != null)
            {
                step.activeObject.gameObject.SetActive(true);
            }

            // UI 내려오는 애니메이션 실행
            if (previousPosition != step.UIPosition
                || currentStepIndex == 14   // 교수님 첫 편지 확대 해제 직후
                || currentStepIndex == 24   // 교수님 두 번째 편지 확대 해제 직후
                || currentStepIndex == 36)  // 양피지 해금 이벤트
            {
                tutorialUI.anchoredPosition = new Vector2(step.UIPosition.x, Screen.height); // 화면 위에서 시작
                tutorialUI.DOAnchorPos(step.UIPosition, 0.3f) // 새 위치로 내려옴
                    .OnComplete(() =>
                    {
                        // tutorialAnimator.SetTrigger("Spread");
                        // StartCoroutine(ToggleUIFlag(false));
                        // SoundManager.Instance.PlaySFX("Papergrab");
                        SpreadUI();
                    });
            }
            else
            {
                // 위치가 같다면 바로 화면 밖에서 내려오는 애니메이션 없음
                tutorialUI.anchoredPosition = step.UIPosition;
                // tutorialAnimator.SetTrigger("Spread");
                // StartCoroutine(ToggleUIFlag(false));
                // SoundManager.Instance.PlaySFX("Papergrab");
                SpreadUI();
            }
            // 이전 위치 업데이트
            previousPosition = step.UIPosition;
            whileFoldAnim = false;

            //isStepping = false;
            // SpreadUI 애니메이션 끝날 때까지 기다리고 isStepping = false 해주기
            StartCoroutine(WaitForSpreadAnimation());
        }

        private IEnumerator WaitForSpreadAnimation()
        {
            // 애니메이션 상태가 "TutoScrollSpreadAnim"로 전환될 때까지 대기
            yield return new WaitUntil(() => tutorialAnimator.GetCurrentAnimatorStateInfo(0).IsName("TutoScrollSpreadAnim"));

            // "TutoScrollSpreadAnim" 애니메이션이 끝날 때까지 대기
            yield return new WaitForSeconds(tutorialAnimator.GetCurrentAnimatorStateInfo(0).length);

            confirmButton.interactable = true;
            isStepping = false;
        }

        /// <summary>
        /// 튜토리얼의 다음 스텝으로 넘어감.
        /// 확인 버튼을 누르거나, 플레이어의 행동으로 이벤트가 끝나면 호출됨.
        /// </summary>
        public void NextStep()
        {
            isStepping = true;
            confirmButton.interactable = false; // 확인 버튼 연타 방지

            if (!isFold)    // UI가 열려 있는 상태이면 실행
            {
                // Animator를 통해 창 닫기 애니메이션 실행
                // tutorialAnimator.SetTrigger("Fold");
                // SoundManager.Instance.PlaySFX("Scrollup");
                // whileFoldAnim = true;
                FoldUI();
            }

            // 애니메이션이 끝난 후 다음 단계로 넘어가기
            StartCoroutine(WaitForCloseAnimation());
        }

        private IEnumerator WaitForCloseAnimation()
        {
            if (!isFold)
            {
                // 애니메이션 상태가 "close"로 전환될 때까지 대기
                yield return new WaitUntil(() => tutorialAnimator.GetCurrentAnimatorStateInfo(0).IsName("TutoScrollFoldAnim"));

                // "close" 상태 애니메이션이 끝날 때까지 대기
                yield return new WaitForSeconds(tutorialAnimator.GetCurrentAnimatorStateInfo(0).length);
            }

            TutorialStep currentStep = tutorialSteps[currentStepIndex];

            // 다음 스텝이 있는지 확인
            bool hasNextStep = currentStepIndex < tutorialSteps.Count - 1;

            if (hasNextStep)
            {
                TutorialStep nextStep = tutorialSteps[currentStepIndex + 1];

                // 현재 스텝과 다음 스텝의 UIPosition이 다를 경우에만 올라가는 애니메이션 실행, 또는 튜토리얼에서 화면 전환할 때
                if (currentStep.UIPosition != nextStep.UIPosition || currentStepIndex == 8)
                {
                    yield return tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
                }
            }
            else
            {
                // 마지막 스텝인 경우 무조건 올라가는 애니메이션 실행
                yield return tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
            }

            currentStepIndex++;

            // 특정 스텝에서 종료 처리 (currentStepIndex는 증가시키지만 ShowTutorialStep 호출 안 함)
            if (currentStepIndex - 1 == 13) // 올빼미 편지 클릭 대기
            {
                tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
                yield break; // 코루틴 종료
            }
            if (currentStepIndex - 1 == 22) // 0일차 의뢰를 모두 마치는 것 대기. 침대에서 자라고 할거임
            {
                tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
                DisableHighlightingPanel();
                yield break; // 코루틴 종료
            }
            if (currentStepIndex - 1 == 23) // 교수님의 두 번째 편지 확대 해제 대기
            {
                tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
                DisableHighlightingPanel();
                yield break; // 코루틴 종료
            }
            if (currentStepIndex - 1 == 35) // 강화 확장 튜토리얼 마무리 후 대기
            {
                tutorialUI.DOAnchorPos(new Vector2(tutorialUI.anchoredPosition.x, Screen.height + tutorialSteps[currentStepIndex].UIHeight), 0.3f).WaitForCompletion();
                SetTrueStampDrag();
                DisableHighlightingPanel();
                yield break; // 코루틴 종료
            }

            // 다음 스텝으로 이동
            if (hasNextStep)
            {
                ShowTutorialStep(currentStepIndex);
            }
            else
            {
                // 튜토리얼 종료 처리
                DisableHighlightingPanel();
                tutorialComplete = true;

                // 도전과제 해금
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_1");
            }
        }

        #region 버튼 비활성화 및 호버링 UI
        // 버튼 비활성화
        public void DisableButtons(List<Button> buttonsToDisable)
        {
            foreach (var button in buttonsToDisable)
            {
                if (button != null)
                {
                    // Button의 interactable을 false로 설정
                    button.interactable = false;

                    // 버튼의 ColorBlock 수정
                    var colors = button.colors;
                    colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 1f); // 어둡게 설정 (RGBA 값으로 조정 가능)
                    button.colors = colors;

                    // 버튼에 연결된 MagicDragController 비활성화
                    if (currentStepIndex == 13 && button.TryGetComponent<MagicDragController>(out MagicDragController magicDragController))
                    {
                        magicDragController.enabled = false;
                    }

                    // 버튼에 호버링 효과 추가
                    AddHoverEffect(button);
                }
            }
        }

        // 버튼 활성화
        public void EnableButtons(List<Button> buttonsToEnable)
        {
            foreach (var button in buttonsToEnable)
            {
                if (button != null)
                {
                    // Button을 활성화
                    button.interactable = true;

                    // 버튼에 연결된 MagicDragController 활성화
                    if (currentStepIndex == 14 && button.TryGetComponent<MagicDragController>(out MagicDragController magicDragController))
                    {
                        magicDragController.enabled = true;
                    }

                    // 버튼의 호버링 이벤트 제거 (필요한 경우)
                    RemoveHoverEffect(button);
                }
            }
        }

        // 마우스 호버링 효과 추가
        private void AddHoverEffect(Button button)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

            var pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener((data) =>
            {
                if (!button.interactable && hoverUI != null)
                {
                    // 기존에 비활성화된 호버링 UI를 활성화
                    if (!hoverUI.activeSelf)
                    {
                        hoverUI.SetActive(true);
                    }

                    // 텍스트 업데이트
                    //hoverUI.GetComponentInChildren<TextMeshProUGUI>().text = "튜토리얼을 먼저 완료하세요.";

                    // 초기 위치 설정
                    UpdateHoverUIPosition();

                    // 마우스가 버튼 위에 있음을 표시
                    isHovering = true;
                }
            });

            var pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener((data) =>
            {
                if (hoverUI != null && hoverUI.activeSelf)
                {
                    // 호버링 UI 비활성화
                    hoverUI.SetActive(false);
                }

                // 마우스가 버튼에서 벗어남
                isHovering = false;
            });

            trigger.triggers.Add(pointerEnterEntry);
            trigger.triggers.Add(pointerExitEntry);
        }

        // 매 프레임마다 호출되어 호버링 UI 위치를 업데이트
        private void Update()
        {
            if (isHovering && hoverUI.activeSelf)
            {
                UpdateHoverUIPosition();
            }
        }

        // 호버링 UI 위치를 업데이트하는 메서드
        private void UpdateHoverUIPosition()
        {
            // 마우스 위치 가져오기
            Vector2 screenPosition = Input.mousePosition;

            // 호버링 UI RectTransform 가져오기
            RectTransform hoverTransform = hoverUI.GetComponent<RectTransform>();

            // 호버링 UI의 너비와 높이 가져오기
            float hoverUIHalfWidth = hoverTransform.rect.width / 2f;
            float hoverUIHalfHeight = hoverTransform.rect.height / 2f;

            // 캔버스의 크기 가져오기
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            // 기본적으로 마우스 오른쪽 아래에 위치
            Vector2 offset = new Vector2(hoverUIHalfWidth + 10f, -hoverUIHalfHeight - 10f); // 오른쪽 10px, 아래로 10px 간격
            screenPosition += offset;

            // 화면 밖으로 나가는지 확인
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint);

            // 오른쪽으로 나가는 경우, 왼쪽으로 변경
            if (localPoint.x + hoverUIHalfWidth > canvasSize.x / 2f) // 화면 오른쪽 나감
            {
                screenPosition -= new Vector2(hoverUIHalfWidth * 2 + 20f, 0f); // 왼쪽으로 이동
            }

            // 아래쪽으로 나가는 경우, 위쪽으로 이동
            if (localPoint.y - hoverUIHalfHeight < -canvasSize.y / 2f) // 화면 아래 나감
            {
                screenPosition += new Vector2(0f, hoverUIHalfHeight * 2 + 20f); // 위쪽으로 이동
            }

            // 위/아래 경계 확인 (Y축 조정)
            float adjustedY = Mathf.Clamp(localPoint.y, -canvasSize.y / 2f + hoverUIHalfHeight, canvasSize.y / 2f - hoverUIHalfHeight);
            screenPosition.y = Mathf.Min(screenPosition.y, canvas.worldCamera.WorldToScreenPoint(canvasRect.position).y + adjustedY);

            // 마우스 위치를 월드 좌표로 변환
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRect,
                screenPosition,
                canvas.worldCamera,
                out Vector3 worldPosition);

            // 호버링 UI 위치를 월드 좌표에 설정
            hoverTransform.position = worldPosition;
        }

        private void RemoveHoverEffect(Button button)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.Clear();
            }
        }
        #endregion

        #region 튜토리얼 스킵
        public void SkipTutorial()
        {
            Debug.Log("튜토리얼 스킵 확인 창 켜기.");
            skipCheckUI.gameObject.SetActive(true);
        }

        public void SkipYes()
        {
            Debug.Log("튜토리얼 스킵~");
            tutorialComplete = true;
            SetTrueTutorialRuneStone(); // 꺼놨던 강화 확장 룬들 다시 켜줌
            SetTrueStampDrag();         // 꺼놨던 도장 드래그 켜줌
            SetTrueRecipeDrag();        // 꺼놨던 레시피북 드래그 켜줌
            currentStepIndex = tutorialSteps.Count;

            // 의뢰를 다시 할당
            RequestManager.Instance.todayRequests.Clear();
            RequestManager.Instance.AssignRequestForToday();

            // 도전과제 해금
            SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_1");

            this.gameObject.SetActive(false);
        }

        public void SkipNo()
        {
            Debug.Log("튜토리얼 스킵 안해~");
            skipCheckUI.gameObject.SetActive(false);
        }
        #endregion

        public void EnableHighlightingPanel()
        {
            highlightingPanel.gameObject.SetActive(true);
            Image panelImage = highlightingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0, 0, 0, 0.9f);    // 반투명 검정색
            }
        }

        public void DisableHighlightingPanel()
        {
            highlightingPanel.gameObject.SetActive(false);
            // Image panelImage = highlightingPanel.GetComponent<Image>();
            // if (panelImage != null)
            // {
            //     panelImage.color = new Color(0, 0, 0, 0);    // 완전 투명
            // }
        }

        #region UI접기 펴기 애니메이션
        public void ToggleFoldUIAnim()
        {
            if (isStepping) // 스텝 진행으로 인한 자동 애니메이션 중에는 수동으로 불가.
            {
                return;
            }
            if (isFold)
            {
                SpreadUI();
            }
            else
            {
                FoldUI();
            }
        }

        private void SpreadUI()
        {
            if (whileFoldAnim)
            {
                return;
            }
            foldButton.interactable = false;

            whileFoldAnim = true;
            tutorialAnimator.SetTrigger("Spread");
            StartCoroutine(ToggleUIFlag(false));
            SoundManager.Instance.PlaySFX("Papergrab");
        }

        private void FoldUI()
        {
            if (whileFoldAnim)
            {
                return;
            }
            foldButton.interactable = false;

            whileFoldAnim = true;
            tutorialAnimator.SetTrigger("Fold");
            StartCoroutine(ToggleUIFlag(true));
            SoundManager.Instance.PlaySFX("Scrollup");
        }

        private IEnumerator ToggleUIFlag(bool flag)
        {
            yield return new WaitForSeconds(0.25f);
            foldButton.interactable = true;
            isFold = flag;
            whileFoldAnim = false; // 애니메이션 중복 방지
        }
        #endregion

        private void DisableScroll()
        {
            tutorialUI.gameObject.SetActive(false);
        }
    }
}