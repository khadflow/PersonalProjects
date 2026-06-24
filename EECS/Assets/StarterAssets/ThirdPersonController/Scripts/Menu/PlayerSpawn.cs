using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using Cinemachine; // Enforce Cinemachine v2 namespace for compilation consistency

namespace StarterAssets
{
    public class PlayerSpawn : MonoBehaviour
    {
        /* UI Management */
        private bool ActiveGame;
        private Vector3 MenuPos;
        private Image image;

        public GameObject PlayerSelect; // For Player Select Reference
        private PlayerSelect PlayerSelection;

        public EventSystem MainMenuEvent;
        public Button[] MainMenuButtons;

        public EventSystem PlayerSelectEvent;
        public Button[] PlayerSelectButtons;

        public GameObject Round;
        private TextMeshProUGUI tmp;

        private float MenuTimer;
        private float MenuCountdown = 5.0f;

        /* Player Spawn Management */
        private float dist = 3.0f;
        private GameObject P1, P2;
        private Vector3 P1StartLocation, P2StartLocation;
        public GameObject _mainCamera;

        // NEW: Assign an empty GameObject located on your flat stage floor here via the Inspector
        [Header("Stage Positioning")]
        public Transform StageFloorAnchor;

        private GameObject Player;
        private GameObject Player2;
        private bool Players_Spawned;

        void Start()
        {
            foreach (Button b in PlayerSelectButtons)
            {
                b.gameObject.SetActive(false);
            }

            // Round Display
            Round.SetActive(false);

            // Menu Components
            PlayerSelection = PlayerSelect.gameObject.GetComponentInChildren<PlayerSelect>();
            image = GetComponent<Image>();

            // Default Players
            SetPlayerOne(PlayerSelection.Characters[0]);
            SetPlayerTwo(PlayerSelection.Characters[1]);

            // UI
            ActiveGame = false;
            Players_Spawned = false;
            MenuPos = transform.position;
            PlayerSelection.SetMenuPos(MenuPos);
        }

        void Update()
        {
            if (ActiveGame && !Players_Spawned)
            {
                C_Controller.NumberOfPlayers = 0;

                // Grab a safe fallback coordinate if the anchor field hasn't been set in the inspector yet
                Vector3 referenceBase = StageFloorAnchor != null ? StageFloorAnchor.position : new Vector3(0f, 0f, 0f);

                // Calculate horizontal offsets cleanly off the floor anchor instead of reading the floating sky camera
                P1StartLocation = new Vector3(referenceBase.x - dist, referenceBase.y, referenceBase.z);
                P2StartLocation = new Vector3(referenceBase.x + dist, referenceBase.y, referenceBase.z);

                // Spawn characters cleanly onto the floor coordinates
                P1 = Instantiate(Player, P1StartLocation, Quaternion.Euler(0.0f, 90.0f, 0.0f));
                P2 = Instantiate(Player2, P2StartLocation, Quaternion.Euler(0.0f, 270.0f, 0.0f));
                P1.tag = "Player";
                P2.tag = "Player2";

                // Register targets to your tracking group
                if (CinemachineTargetGroupManager.Instance != null)
                {
                    CinemachineTargetGroupManager.Instance.AddPlayerToTargetGroup(P1.transform);
                    CinemachineTargetGroupManager.Instance.AddPlayerToTargetGroup(P2.transform);
                }

                // Activate your virtual camera seamlessly now that players are safely localized on the field
                CinemachineVirtualCamera vCam = GameObject.FindAnyObjectByType<CinemachineVirtualCamera>();
                if (vCam != null)
                {
                    vCam.gameObject.SetActive(true);
                    Debug.Log("[CAMERA] Cinemachine Virtual Camera successfully activated!");
                }
                else
                {
                    GameObject vCamObj = GameObject.Find("Virtual Camera");
                    if (vCamObj != null)
                    {
                        vCamObj.SetActive(true);
                        Debug.Log("[CAMERA] Virtual Camera found via name search and activated!");
                    }
                }

                // Hide Main Menu
                Players_Spawned = true;
                GameObject mainMenu = GameObject.Find("MainMenuCanvas");
                mainMenu.SetActive(false);
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
                    C_Controller.ResetRound();

                    MainMenuEvent.gameObject.SetActive(true);

                    // Hide Main Menu
                    Players_Spawned = true;
                    GameObject mainMenu = GameObject.Find("MainMenuCanvas");
                    mainMenu.SetActive(true);
                }

                if (C_Controller.Round <= 2 && ActiveGame)
                {
                    tmp.text = "Round " + C_Controller.Round.ToString();
                }
            }

            if (!ActiveGame && Players_Spawned)
            {
                if (MenuTimer < Time.time)
                {
                    transform.position = MenuPos;
                    P1.GetComponent<C_Controller>().DestroyHealthBars();
                    P2.GetComponent<C_Controller>().DestroyHealthBars();
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
            MainMenuEvent.gameObject.SetActive(false);
            PlayerSelectEvent.gameObject.SetActive(true);

            image.gameObject.SetActive(false);
        }

        // TODO: Implement proper canvas-scale anchoring for health bars
        public void SetPlayerOne(GameObject P1)
        {
            Player = P1;
            Player.gameObject.GetComponent<C_Controller>().Degrees = 90.0f;
            Player.gameObject.GetComponent<C_Controller>().SetPlayerNumber(1);
            Player.gameObject.GetComponent<C_Controller>().SetHealth();
        }

        public void SetPlayerTwo(GameObject P2)
        {
            Player2 = P2;
            Player2.gameObject.GetComponent<C_Controller>().Degrees = 270.0f;
            Player2.gameObject.GetComponent<C_Controller>().SetPlayerNumber(2);
            Player2.gameObject.GetComponent<C_Controller>().SetHealth();
        }

        public void ActivateGame()
        {
            if (!ActiveGame)
            {
                C_Controller.ResetSwitchCondiition();
                C_Controller.ResetRoundCoolDown();
                Round.SetActive(true);

                tmp = Round.GetComponent<TextMeshProUGUI>();
                tmp.text = "Round " + C_Controller.Round.ToString();
                ActiveGame = true;
                MainMenuEvent.gameObject.SetActive(false);
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

            // Return to Main Menu
            PlayerSelectEvent.gameObject.SetActive(false);
            MainMenuEvent.gameObject.SetActive(true);

            image = GetComponent<Image>();
            image.gameObject.SetActive(true);
        }

        public void EndGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
        }
    }
}