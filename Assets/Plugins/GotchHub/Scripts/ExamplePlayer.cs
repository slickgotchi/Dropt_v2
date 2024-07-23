using UnityEngine;

namespace GotchiHub
{
    public class ExamplePlayer : MonoBehaviour
    {
        private float k_speed = 6.22f;
        public SpriteRenderer BodySpriteRenderer;

        [Header("Sprites")]
        public Sprite m_frontSprite;
        public Sprite m_backSprite;
        public Sprite m_leftSprite;
        public Sprite m_rightSprite;

        private void Start()
        {
            if (GotchiDataManager.Instance != null)
            {
                Debug.Log("Listening to gotchi data manager");
                GotchiDataManager.Instance.onSelectedGotchi += OnSelectedGotchiChanged;
            }

            BodySpriteRenderer.material = GotchiDataManager.Instance.Material_Sprite_Unlit_Default;
        }

        private void OnDestroy()
        {
            if (GotchiDataManager.Instance != null)
            {
                GotchiDataManager.Instance.onSelectedGotchi -= OnSelectedGotchiChanged;
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);
            transform.Translate(movement * k_speed * Time.deltaTime, Space.World);

            UpdateSpriteDirection(movement);
        }

        private void UpdateSpriteDirection(Vector3 movement)
        {
            if (movement.magnitude > 0)
            {
                if (movement.y > 0)
                {
                    BodySpriteRenderer.sprite = m_backSprite;
                }
                else if (movement.y < 0)
                {
                    BodySpriteRenderer.sprite = m_frontSprite;
                }
                else if (movement.x > 0)
                {
                    BodySpriteRenderer.sprite = m_rightSprite;
                }
                else if (movement.x < 0)
                {
                    BodySpriteRenderer.sprite = m_leftSprite;
                }
            }
        }

        private void OnSelectedGotchiChanged(int newGotchiId)
        {
            Debug.Log("Selected Gotchi ID changed to: " + newGotchiId);
            var gotchiSvgs = GotchiDataManager.Instance.GetGotchiSvgsById(newGotchiId);

            // Cache the sprites
            m_frontSprite = GetSpriteFromSvgString(gotchiSvgs.Front);
            m_backSprite = GetSpriteFromSvgString(gotchiSvgs.Back);
            m_leftSprite = GetSpriteFromSvgString(gotchiSvgs.Left);
            m_rightSprite = GetSpriteFromSvgString(gotchiSvgs.Right);

            // Set initial sprite
            BodySpriteRenderer.sprite = m_frontSprite;
            BodySpriteRenderer.material = GotchiDataManager.Instance.Material_Unlit_VectorGradient;
        }

        private Sprite GetSpriteFromSvgString(string svgString)
        {
            // Convert SVG string to a Sprite
            return CustomSvgLoader.CreateSvgSprite(GotchiDataManager.Instance.stylingGame.CustomizeSVG(svgString), new Vector2(0.5f, 0.15f));
        }
    }
}