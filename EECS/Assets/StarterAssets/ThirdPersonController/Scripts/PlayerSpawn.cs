using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarterAssets
{
    public class PlayerSpawn : MonoBehaviour
    {
        public bool ActiveGame;

        public GameObject[] characters;
        private int selectedCharacter = 0;
        public GameObject _mainCamera;
        public GameObject Player;
        public GameObject Player2;
        private bool Players_Spawned;
        private Vector3 MenuPos;
        private float dist = 3.0f;

        private float MenuTimer;
        private float MenuCountdown = 5.0f;

        private GameObject P1, P2;

        private Vector3 P1StartLocation, P2StartLocation;

        // Start is called before the first frame update
        void Start()
        {
            ActiveGame = false;
            Players_Spawned = false;
            MenuPos = transform.position;
        }

        // Update is called once per frame
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
                this.transform.position = new Vector3(_mainCamera.transform.position.x - 500, _mainCamera.transform.position.y - 500, _mainCamera.transform.position.z);
            }
            if (ActiveGame)
            {
                C_Controller player1_controller, player2_controller;
                player1_controller = P1.gameObject.GetComponent<C_Controller>();
                player2_controller = P2.gameObject.GetComponent<C_Controller>();

                if ((player2_controller.health <= 0.0f || player1_controller.health <= 0.0f) && C_Controller.Round > 2)
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

        public void CharacterSelect()
        {
            // TODO
            /*characters[selectedCharacter].SetActive(false);
            selectedCharacter = (selectedCharacter + 1) % characters.Length;
            characters[selectedCharacter].SetActive(true);*/
        }

        public void ActivateGame()
        {
            if (!ActiveGame)
            {
                ActiveGame = true;
            }
        }

        public void EndGame()
        {
            transform.position = MenuPos;
            ActiveGame = false;
            Application.Quit();
        }
    }
}