using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Game.Request;

namespace Game.Invironment
{
    public class WindowViewController : MonoBehaviour
    {
        [Header("창문 이미지")]
        [SerializeField] private List<Image> image1; // 첫 번째 Image ( 방뷰, 책상 뷰 )
        [SerializeField] private List<Image> image2; // 두 번째 Image (투명도 조절용) ( 방뷰, 책상 뷰 )
        [SerializeField] private List<Image> cloudImage1; // 첫 번째 Image ( 구름 뷰 )
        [SerializeField] private List<Image> cloudImage2; // 두 번째 Image (투명도 조절용) ( 구름 뷰 )
        [Header("창밖 스프라이트")]
        [SerializeField] private Sprite[] sprites; // 전환할 스프라이트들
        [SerializeField] private Sprite[] cloudSprites; // 구름 스프라이트들
        [Header("전환 시간")]
        [SerializeField] private float transitionDuration = 30f; // 전환 시간
        private bool[] keys = new bool[3]; // 키 조건        
        private Coroutine _currentCoroutine;
        private int _todayRequestCount;
        private int _currentQuestIndex;

        private void Start()
        {
            InitializeImages();
        }

        private void InitializeImages()
        {
            switch (_todayRequestCount)
            {
                //2일차만 2번째 이미지로 시작
                case 2:
                    foreach (var image in image1)
                    {
                        image.sprite = sprites[0];
                    }
                    foreach (var cloud in cloudImage1)
                    {
                        cloud.sprite = cloudSprites[0];
                    }
                    keys[0] = true;
                    break;
                case 3:
                    foreach (var image in image1)
                    {
                        image.sprite = sprites[0];
                    }
                    foreach (var cloud in cloudImage1)
                    {
                        cloud.sprite = cloudSprites[0];
                    }
                    break;
                case 4:
                    foreach (var image in image1)
                    {
                        image.sprite = sprites[0];
                    }
                    foreach (var cloud in cloudImage1)
                    {
                        cloud.sprite = cloudSprites[0];
                    }
                    break;
                default:
                    foreach (var image in image1)
                    {
                        image.sprite = sprites[0];
                    }
                    foreach (var cloud in cloudImage1)
                    {
                        cloud.sprite = cloudSprites[0];
                    }
                    break;
            }
            foreach (var image in image1)
            {
                image.color = new Color(1, 1, 1, 1); // 완전 불투명
            }
            foreach (var image in image2)
            {
                image.color = new Color(1, 1, 1, 0); // 완전 투명
            }


            foreach (var image in cloudImage1)
            {
                image.color = new Color(1, 1, 1, 1); // 완전 불투명
            }
            foreach (var image in cloudImage2)
            {
                image.color = new Color(1, 1, 1, 0); // 완전 투명
            }
        }
        /// <summary>
        /// 창문 초기화 및 오늘 의뢰수 등록
        /// </summary>
        /// <param name="todayRequestCount">오늘 총 의뢰</param>
        public void StartTransition(int todayRequestCount)
        {
            //오늘 의뢰수 등록
            _todayRequestCount = todayRequestCount;
            ResetToInitialState();
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = StartCoroutine(TransitionImages());
        }

        public void ResetToInitialState()
        {
            if (_currentCoroutine != null)
            {
                StopAllCoroutines();
            }
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = false; // 모든 키 초기화
            }
            if (DayManager.Instance.day == 13)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = true;
                }
            }
            InitializeImages();
        }
        /// <summary>
        /// 시간 증가시켜주는 함수
        /// </summary>
        /// <param name="leftRequestCount">남은 의뢰 갯수</param>
        public void TimePassKey(int leftRequestCount)
        {
            // 남은 의뢰수로 현재 의뢰 인덱스 계산
            _currentQuestIndex = _todayRequestCount - leftRequestCount + 1;
            //스위치 내부 스위치 발동!!
            switch (_todayRequestCount)
            {
                case 2:
                    switch (_currentQuestIndex)
                    {
                        case 1:
                            keys[1] = true;
                            break;
                        case 2:
                            keys[2] = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    switch (_currentQuestIndex)
                    {
                        case 1:
                            keys[0] = true;
                            break;
                        case 2:
                            keys[1] = true;
                            break;
                        case 3:
                            keys[2] = true;
                            break;
                        default:
                            break;
                    }
                    break;
                case 4:
                    switch (_currentQuestIndex)
                    {
                        case 1:
                            keys[0] = true;
                            break;
                        case 2:
                            break;
                        case 3:
                            keys[1] = true;
                            break;
                        case 4:
                            keys[2] = true;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private IEnumerator TransitionImages()
        {
            int currentSpriteIndex = 0;

            while (true)
            {
                if (currentSpriteIndex >= sprites.Length - 1)
                    yield break;

                if (keys[currentSpriteIndex])
                {
                    yield return StartCoroutine(FadeImages(currentSpriteIndex, currentSpriteIndex + 1));
                    currentSpriteIndex++;
                }

                yield return null;
            }
        }

        private IEnumerator FadeImages(int fromIndex, int toIndex)
        {
            float timer = 0f;
            foreach (var image in image2)
            {
                image.sprite = sprites[toIndex];
            }
            foreach (var image in cloudImage2)
            {
                image.sprite = cloudSprites[toIndex];
            }
            while (timer <= transitionDuration)
            {
                float t = timer / transitionDuration;

                // 투명도 조정
                foreach (var image in image1)
                {
                    image.color = new Color(1, 1, 1, 1 - t);
                }
                foreach (var image in image2)
                {
                    image.color = new Color(1, 1, 1, t);
                }

                foreach (var image in cloudImage1)
                {
                    image.color = new Color(1, 1, 1, 1 - t);
                }
                foreach (var image in cloudImage2)
                {
                    image.color = new Color(1, 1, 1, t);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // 전환 완료 후 이미지 교체
            foreach (var image in image1)
            {
                image.sprite = sprites[toIndex];
            }
            foreach (var image in image1)
            {
                image.color = new Color(1, 1, 1, 1); // 완전 불투명
            }
            foreach (var image in image2)
            {
                image.color = new Color(1, 1, 1, 0); // 완전 투명
            }

            foreach (var image in cloudImage1)
            {
                image.sprite = cloudSprites[toIndex];
            }
            foreach (var image in cloudImage1)
            {
                image.color = new Color(1, 1, 1, 1); // 완전 불투명
            }
            foreach (var image in cloudImage2)
            {
                image.color = new Color(1, 1, 1, 0); // 완전 투명
            }
        }
    }
}