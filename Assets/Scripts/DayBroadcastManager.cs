using UnityEngine;
using TMPro;

public class DayBroadcastManager : MonoBehaviour
{
    [Header("UI Elements (1 Panel, 1 Text)")]
    public GameObject mainPanel;      // Obiekt: BackGorund
    public TextMeshProUGUI mainText;  // Obiekt: Text

    [Header("Skrypty Powiązane")]
    public PlayerStats playerStats;
    public Inventory inventory;
    public spawnObjects itemSpawner;   // Podpnij tutaj obiekt ze skryptem spawnObjects

    private string[] currentRadioMessages;
    private int currentRadioIndex = 0;
    private bool hasReadRadioToday = false;

    // Etapy: Radio -> Dialog -> ItemCheck -> Rationing -> Victory/GameOver
    private enum EventStage { Idle, Radio, Dialog, ItemCheck, Rationing, GameOver, Victory }
    private EventStage currentStage = EventStage.Idle;

    void Start()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    // ==========================================
    // 1. OTWARCIE RADIA (Kliknięcie w radio w grze)
    // ==========================================
    public void OpenRadio()
    {
        if (currentStage == EventStage.GameOver || currentStage == EventStage.Victory)
        {
            return;
        }

        if (hasReadRadioToday)
        {
            Debug.Log("[RADIO] Broadcast for today has already been listened to.");
            return;
        }

        currentStage = EventStage.Radio;
        int currentDay = (playerStats != null) ? playerStats.day : 1;
        LoadMessagesForDay(currentDay);

        if (mainPanel != null) mainPanel.SetActive(true);
        ShowCurrentRadioMessage();
    }

    // ==========================================
    // 2. PODPIĘTE POD PRZYCISK: NextButton
    // ==========================================
    public void OnNextButtonPressed()
    {
        switch (currentStage)
        {
            case EventStage.Radio:
                currentRadioIndex++;
                if (currentRadioMessages != null && currentRadioIndex < currentRadioMessages.Length)
                {
                    ShowCurrentRadioMessage();
                }
                else
                {
                    // Dzień 5: po przeczytaniu radia od razu wygrana
                    if (playerStats != null && playerStats.day >= 5)
                    {
                        TriggerVictory();
                    }
                    else
                    {
                        hasReadRadioToday = true;
                        StartDayEvent();
                    }
                }
                break;

            case EventStage.Dialog:
                PerformItemCheck();
                break;

            case EventStage.ItemCheck:
                StartRationingPhase();
                break;

            case EventStage.Rationing:
                ApplyRationingAndNextDay();
                break;

            case EventStage.GameOver:
            case EventStage.Victory:
                if (mainPanel != null) mainPanel.SetActive(false);
                break;
        }
    }

    // ==========================================
    // 3. EVENT FABULARNY (DIALOG)
    // ==========================================
    private void StartDayEvent()
    {
        currentStage = EventStage.Dialog;

        int currentDay = (playerStats != null) ? playerStats.day : 1;

        if (mainText != null)
        {
            switch (currentDay)
            {
                case 1:
                    mainText.text = "A shelf in the shelter has loosened and is about to collapse. You need to secure it before everything falls off.";
                    break;

                case 2:
                    mainText.text = "You notice a dark space under the cabinet. Something might be hidden down there, but it's too dark to see clearly.";
                    break;

                case 3:
                    mainText.text = "You found an old, rusty can of food, but it lacks a pull tab and the regular opener won't budge. You need something heavy to crack it open.";
                    break;

                case 4:
                    mainText.text = "While searching a dusty corner of the shelter, you got a nasty, painful wooden splinter deep in your hand.";
                    break;

                case 5:
                    mainText.text = "Boredom sets in. You decide to rummage through the old junk piled up in the corner and spot a forgotten backpack.";
                    break;

                default:
                    mainText.text = "Another quiet day in the shelter...";
                    break;
            }
        }
    }

    // ==========================================
    // 4. SPRAWDZENIE PRZEDMIOTU
    // ==========================================
    private void PerformItemCheck()
    {
        currentStage = EventStage.ItemCheck;

        int currentDay = (playerStats != null) ? playerStats.day : 1;

        if (mainText != null)
        {
            switch (currentDay)
            {
                case 1:
                    // Dzień 1: Tape
                    if (inventory != null && inventory.tape > 0)
                    {
                        inventory.tape--;
                        if (playerStats != null) playerStats.sanity += 10;
                        mainText.text = "You have duct tape! You manage to firmly secure the shelf. Everything stays in place. (+10 Sanity, -1 Tape)";
                    }
                    else
                    {
                        if (playerStats != null) playerStats.sanity -= 20;
                        mainText.text = "You don't have duct tape! The shelf collapses with a loud crash, destroying supplies and causing panic. (-20 Sanity)";
                    }
                    break;

                case 2:
                    // Dzień 2: Flashlight
                    if (inventory != null && inventory.flashlight > 0)
                    {
                        inventory.food++;
                        mainText.text = "You use your flashlight to light up the gap. You find a can of soup! (+1 Food)";
                    }
                    else
                    {
                        mainText.text = "You don't have a flashlight. It's too dark to reach inside safely, so you leave empty-handed.";
                    }
                    break;

                case 3:
                    // Dzień 3: Axe
                    if (inventory != null && inventory.axe > 0)
                    {
                        inventory.food++;
                        if (playerStats != null) playerStats.hunder = Mathf.Min(100, playerStats.hunder + 20);
                        mainText.text = "You use the axe to split open the rusty can! You manage to recover the edible food inside. (+1 Food, +20 Hunger)";
                    }
                    else
                    {
                        if (playerStats != null) playerStats.sanity -= 10;
                        mainText.text = "Without a proper tool, the can slips away into the shadows and breaks open, completely wasted. (-10 Sanity)";
                    }
                    break;

                case 4:
                    // Dzień 4: Medkit
                    if (inventory != null && inventory.medkit > 0)
                    {
                        inventory.medkit--;

                        mainText.text = "You use the medkit to carefully remove the splinter and disinfect the wound. All good! (-1 Medkit)";
                    }
                    else
                    {
                        if (playerStats != null) playerStats.sanity -= 10;
                        mainText.text = "You have no medical supplies to disinfect the wound. Your hand swells up and throbs painfully. (-10 Sanity)";
                    }
                    break;

                case 5:
                    // Dzień 5: Backpack
                    if (inventory != null && inventory.backpack > 0)
                    {
                        if (playerStats != null) playerStats.sanity += 15;
                        mainText.text = "You search the backpack's side pockets and find a preserved pack of chewing gum! A small moment of relief. (+15 Sanity)";
                    }
                    else
                    {
                        mainText.text = "The backpack turns out to be completely empty, containing nothing but dust and cobwebs.";
                    }
                    break;
            }
        }

        RefreshInventoryAndStats();
    }

    // ==========================================
    // 5. RACJONOWANIE (JEDZENIE I PICIE)
    // ==========================================
    private void StartRationingPhase()
    {
        currentStage = EventStage.Rationing;

        if (mainText != null)
        {
            int foodAvailable = (inventory != null) ? inventory.food : 0;
            int waterAvailable = (inventory != null) ? inventory.water : 0;

            mainText.text = $"END OF DAY RATIONS:\n\n" +
                            $"Food available: {foodAvailable}\n" +
                            $"Water available: {waterAvailable}\n\n" +
                            $"Consuming food and water restores +20 Hunger/Hydration and +5 Sanity each.\n" +
                            $"Click 'Next' to consume 1 Food and 1 Water if available.";
        }
    }

    private void ApplyRationingAndNextDay()
    {
        // Obsługa Jedzenia
        if (inventory != null && inventory.food > 0)
        {
            inventory.food--;
            if (playerStats != null)
            {
                playerStats.hunder = Mathf.Min(100, playerStats.hunder + 20);
                playerStats.sanity += 5;
            }
        }
        else if (playerStats != null)
        {
            playerStats.hunder -= 15;
        }

        // Obsługa Wody
        if (inventory != null && inventory.water > 0)
        {
            inventory.water--;
            if (playerStats != null)
            {
                playerStats.hydration = Mathf.Min(100, playerStats.hydration + 20);
                playerStats.sanity += 5;
            }
        }
        else if (playerStats != null)
        {
            playerStats.hydration -= 20;
        }

        RefreshInventoryAndStats();

        // Sprawdzanie warunku śmierci
        if (CheckIfPlayerDied())
        {
            return;
        }

        if (mainPanel != null) mainPanel.SetActive(false);
        AdvanceToNextDay();
    }

    // ==========================================
    // 6. SPRAWDZANIE ŻYCIA / WARUNKÓW ŚMIERCI
    // ==========================================
    private bool CheckIfPlayerDied()
    {
        if (playerStats != null)
        {
            if (playerStats.hunder < 1 || playerStats.hydration < 1 || playerStats.sanity < 1)
            {
                currentStage = EventStage.GameOver;

                Debug.Log($"[GAME OVER] Gracz zginął! Hunder: {playerStats.hunder}, Hydration: {playerStats.hydration}, Sanity: {playerStats.sanity}");

                if (mainText != null)
                {
                    mainText.text = "GAME OVER\n\nYou did not survive the harsh conditions of the shelter...";
                }

                if (mainPanel != null) mainPanel.SetActive(true);
                return true;
            }
        }
        return false;
    }

    private void TriggerVictory()
    {
        currentStage = EventStage.Victory;

        Debug.Log("[VICTORY] Gracz wygrał grę!");

        if (mainText != null)
        {
            mainText.text = "VICTORY!\n\nThe rescue team arrived and safely evacuated you from the shelter. You survived!";
        }

        if (mainPanel != null) mainPanel.SetActive(true);
    }

    // ==========================================
    // 7. NASTĘPNY DZIEŃ
    // ==========================================
    public void AdvanceToNextDay()
    {
        if (playerStats != null)
        {
            playerStats.day++;
        }

        hasReadRadioToday = false;
        currentStage = EventStage.Idle;
        Debug.Log($"[SYSTEM] Advanced to Day {(playerStats != null ? playerStats.day : 0)}. Radio is ready again.");
    }

    // --- Metody Pomocnicze ---
    private void RefreshInventoryAndStats()
    {
        if (playerStats != null) 
        {
            playerStats.UpdateUI();
        }

        if (inventory != null)
        {
            inventory.SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
        }

        if (itemSpawner != null)
        {
            itemSpawner.CheckAndHideObjects();
        }
    }

    private void ShowCurrentRadioMessage()
    {
        if (mainText != null && currentRadioMessages != null && currentRadioIndex < currentRadioMessages.Length)
        {
            mainText.text = currentRadioMessages[currentRadioIndex];
        }
    }

    private void LoadMessagesForDay(int dayNumber)
    {
        currentRadioIndex = 0;

        switch (dayNumber)
        {
            case 1:
                currentRadioMessages = new string[]
                {
                    "CIVIL DEFENSE EMERGENCY ALERT:\nGet into your shelter immediately and DO NOT go outside under any circumstances!",
                    "IMPORTANT BROADCAST UPDATE:\nWe have reason to believe secondary targeted attacks are imminent. Hostile entities are active—trust ONLY this frequency."
                };
                break;

            case 2:
                currentRadioMessages = new string[]
                {
                    "DAY 2 BROADCAST:\nStatic crackles through the speaker... 'If anyone is listening, remain quiet and secure all doors.'"
                };
                break;

            case 3:
                currentRadioMessages = new string[]
                {
                    "DAY 3 BROADCAST:\n'Situation is improving. Rescue teams are mobilizing. Prepare yourself for an upcoming emergency evacuation!'"
                };
                break;

            case 4:
                currentRadioMessages = new string[]
                {
                    "DAY 4 BROADCAST:\n'Complications encountered along the main route. Rescue teams are delayed. Hang tight and hold your position!'"
                };
                break;

            case 5:
                currentRadioMessages = new string[]
                {
                    "DAY 5 BROADCAST:\n'Rescue teams have arrived in your sector! Opening shelter doors now!'"
                };
                break;

            default:
                currentRadioMessages = new string[] { "NO BROADCAST AVAILABLE FOR TODAY." };
                break;
        }
    }
}