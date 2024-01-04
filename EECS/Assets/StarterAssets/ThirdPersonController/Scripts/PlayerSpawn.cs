using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarterAssets
{
    public class PlayerSpawn : MonoBehaviour
    {   
        /* UI Management */
        private bool ActiveGame;
        private Vector3 MenuPos;
        private Vector3 HiddenPos;
        private Image image;
        public Button[] MainMenuButtons;
        public Button[] PlayerSelectButtons;

        private float MenuTimer;
        private float MenuCountdown = 5.0f;

        /* Player Spawn Management */
        private float dist = 3.0f;
        private PlayerSelect PlayerSelection;
        private GameObject P1, P2;
        private Vector3 P1StartLocation, P2StartLocation;
        public GameObject _mainCamera;

        private GameObject Player;
        private GameObject Player2;
        private bool Players_Spawned;
        
        void Start()
        {
            foreach (Button b in PlayerSelectButtons)
            {
                b.gameObject.SetActive(false);
            }

            // Menu Components
            PlayerSelection = GetComponent<PlayerSelect>();
            image = GetComponent<Image>();

            // Default Players
            SetPlayerOne(PlayerSelection.Characters[0]);
            SetPlayerTwo(PlayerSelection.Characters[1]);

            // UI
            ActiveGame = false;
            Players_Spawned = false;
            MenuPos = transform.position;
            PlayerSelection.SetMenuPos(MenuPos);
            HiddenPos = new Vector3(_mainCamera.transform.position.x - 500, _mainCamera.transform.position.y - 500, _mainCamera.transform.position.z);
        }

        void Update()
        {
            if (ActiveGame && !Players_Spawned)
            {
                //SceneManager.LoadScene("Playground");
                C_Controller.NumPlayers = 0;

                P1StartLocation = new Vector3(_mainCamera.transform.position.x - dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);
                P2StartLocation = new Vector3(_mainCamera.transform.position.x + dist, _mainCamera.transform.position.y - 2, _mainCamera.transform.position.z + 20);

                P1 = Instantiate(Player, P1StartLocation, Quaternion.Euler(0.0f, 90.0f, 0.0f));
                P2 = Instantiate(Player2, P2StartLocation, Quaternion.Euler(0.0f, 270.0f, 0.0f));
                P1.tag = "Player";
                P2.tag = "Player2";

                Players_Spawned = true;
                this.transform.position = HiddenPos;
            }
            if (ActiveGame)
            {
                C_Controller player1_controller, player2_controller;
                player1_controller = P1.gameObject.GetComponent<C_Controller>();
                player2_controller = P2.gameObject.GetComponent<C_Controller>();

                if (C_Controller.Round > 2)
                {
                    MenuTimer = Time.time + MenuCountdown;
                    ActiveGame = false;
                    C_Controller.Round = 1;
                } 
            } 

            if (!ActiveGame && Players_Spawned)
            {
                if (MenuTimer < Time.time)
                {
                    transform.position = MenuPos;
                    Destroy(P1);
                    Destroy(P2);
                    Players_Spawned = false;
                }
            }
        }

        /* Character Selection Management */
        public void CharacterSelect()
        {
            PlayerSelection.Selecting();
            foreach (Button b in MainMenuButtons)
            {
                b.gameObject.SetActive(false);
            }
            foreach (Button b in PlayerSelectButtons)
            {
                b.gameObject.SetActive(true);
            }
            image.gameObject.SetActive(false);
        }

        public void SetPlayerOne(GameObject P1)
        {
            Player = P1;
        }

        public void SetPlayerTwo(GameObject P2)
        {
            Player2 = P2;
        }

        public void ActivateGame()
        {
            if (!ActiveGame)
            {
                ActiveGame = true;
            }
        }

        public void ResetMainMenu()
        {
            foreach (Button b in PlayerSelectButtons)
            {
                b.gameObject.SetActive(false);
            }

            foreach (Button b in MainMenuButtons)
            {
                b.gameObject.SetActive(true);
            }

            image.gameObject.SetActive(true);
        }

        public void EndGame()
        {
            this.gameObject.SetActive(false);
            ActiveGame = false;
            Application.Quit();
        }
    }
}