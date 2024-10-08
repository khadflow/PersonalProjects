using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerSelect : MonoBehaviour
    {
        /* UI Management */
        public GameObject MainMenu; // For PlayerSpawn Reference
        private PlayerSpawn PlayerSpawnObj;
        private Vector3 MenuPos;
        private Vector3 CharacterSelectPos;

        /* Player Spawn Management */
        private static int selectedCharacter;
        private static int PlayerNumSelect = 1;
        public GameObject[] Characters;
        public GameObject[] CharacterModels;
        private GameObject displayedCharacter;

        /* Event Management */
        public EventSystem MainMenuEvent;
        public EventSystem PlayerSelectEvent;

        private bool isSelecting = true;
        public GameObject _mainCamera;

        void Start()
        {
            // Player Spawn Management
            selectedCharacter = 0;
            PlayerSpawnObj = MainMenu.gameObject.GetComponent<PlayerSpawn>();
            
            CharacterSelectPos = new Vector3(_mainCamera.transform.position.x, _mainCamera.transform.position.y - 1, _mainCamera.transform.position.z + 2);
        }

        public void SetMenuPos(Vector3 pos)
        {
            MenuPos = pos;
        }

        void Update()
        {
                        
        }

        /* Start Character Selection Process */
        public void Selecting()
        {
            if (!isSelecting)
            {
                isSelecting = true;
            }
            displayedCharacter = Instantiate(CharacterModels[selectedCharacter], CharacterSelectPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
        }

        public void Next()
        {
            if (isSelecting) {
                Destroy(displayedCharacter);
                selectedCharacter = (selectedCharacter + 1) % CharacterModels.Length;
                displayedCharacter = Instantiate(CharacterModels[selectedCharacter], CharacterSelectPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
            }
        }

        public void Prev()
        {
            if (isSelecting)
            {
                Destroy(displayedCharacter);
                selectedCharacter = (selectedCharacter - 1) % CharacterModels.Length;
                if (selectedCharacter < 0)
                {
                    selectedCharacter = CharacterModels.Length - 1;
                }
                displayedCharacter = Instantiate(CharacterModels[selectedCharacter], CharacterSelectPos, Quaternion.Euler(0.0f, 180.0f, 0.0f));
            }
        }

        public void Select()
        {
            Debug.Log("Selecting Player " + PlayerNumSelect);
            if (PlayerNumSelect == 1)
            {
                PlayerSpawnObj.SetPlayerOne(Characters[selectedCharacter]);
                PlayerNumSelect++;
            } else if (PlayerNumSelect == 2)
            {
                PlayerSpawnObj.SetPlayerTwo(Characters[selectedCharacter]);
                isSelecting = false;
                Destroy(displayedCharacter);
                PlayerNumSelect = 1;

                // Event System Change
                PlayerSelectEvent.gameObject.SetActive(false);
                MainMenuEvent.gameObject.SetActive(true);

                PlayerSpawnObj.ResetMainMenu();
            }
        }
    }
}
