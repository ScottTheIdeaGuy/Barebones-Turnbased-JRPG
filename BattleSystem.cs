using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JRPG
{
    public enum BattleState { Ongoing, Win, Lose, Flee };
    public static class BattleSystem
    {
        private static BattleCharacter[] playerParty;
        private static BattleCharacter[] foeParty;

        private static int turnCount;
        private static BattleCharacter[] characterSequence;
        private static BattleCharacter activeCharacter;

        private static bool displayingMenuOptions = false;      // whether menu options are currently being displayed

        public static List<BattleBeat> beatQueue = new List<BattleBeat>();

        private static TextObject dialogText = null;
        private static BattleOption[] menuOptions;
        private static TextObject[] menuOptionTexts;

        private static BattleState state;
        public static int menuIndex { get; private set; }

        private readonly static Color menuTextColor = Color.Yellow;
        private readonly static Color menuHighlightColor = Color.Orange;

        private static int menuSelectionLayer;

        private static BattleOption currentMove = null;
        private static Action finalAction = null;
        private static BattleCharacter currentTarget = null;

        public static void InitializeParty()
        {
            BattleCharacter heroLauren = new BattleCharacter("Lauren", new[] { "she", "her", "her", "herself" });
            heroLauren.moveset[0] = BattleOption.Punch;
            heroLauren.moveset[1] = BattleOption.PsychUp;
            heroLauren.moveset[2] = BattleOption.Eat;
            heroLauren.playerControlled = true;
            heroLauren.maxHealth = 30;
            heroLauren.defenseStat = 2;
            heroLauren.attackStat = 3;

            BattleCharacter heroZach = new BattleCharacter("Zach", new[] { "he", "him", "his", "himself" });
            heroZach.moveset[0] = BattleOption.BitchSlap;
            heroZach.moveset[1] = BattleOption.Juke;
            heroZach.playerControlled = true;
            heroZach.maxHealth = 30;
            heroZach.defenseStat = 1;
            heroZach.attackStat = 1;


            playerParty = new[] { heroLauren, heroZach };

            activeCharacter = null;
            heroLauren.Initialize();
            heroZach.Initialize();
        }

        public static void StartFight(BattleCharacter[] otherParty)
        {
            foeParty = otherParty;
            turnCount = 0;
            GameJRPG.currentState = GameState.Battle;
            characterSequence = playerParty.Concat(foeParty).ToArray();

            displayingMenuOptions = false;

            beatQueue = new List<BattleBeat>();
            dialogText = GameJRPG.CreateText("", new Point(5, GameJRPG.TARGET_HEIGHT - 100), TextAlign.Left, true, Color.LightGray, true, .035F);

            dialogText.skippable = true;

            int maxOptions = 6;
            menuOptionTexts = new TextObject[maxOptions];
            menuOptions = new BattleOption[maxOptions];

            for (int i = 0; i < maxOptions; i++)
            {
                menuOptionTexts[i] = GameJRPG.CreateText("", new Point(5 + (int)(i / 3) * 110, GameJRPG.TARGET_HEIGHT - 50 + (i % 3) * 20), TextAlign.Left, true, menuTextColor);
                menuOptions[i] = null;
            }


            menuIndex = 0;
            beatQueue.Add(new BattleBeat("You get first move!\nSTRIKE!"));

            foreach (BattleCharacter foe in foeParty)
                foe.Initialize();

            displayingMenuOptions = false;
            state = BattleState.Ongoing;
            activeCharacter = null;

            foreach (BattleCharacter chr in characterSequence)
            {
                chr.attackStatBuff = 0;
                chr.defenseStatBuff = 0;
            }

            menuSelectionLayer = -1;
            currentMove = null;
            finalAction = null;
            currentTarget = null;
        }

        public static void Update(GameTime gameTime)
        {
            bool oldDisplayingMenuOption = displayingMenuOptions;

            if (beatQueue.Count > 0)
            {
                // display events and battle dialog //
                BattleBeat activeBeat = beatQueue[0];

                if (!activeBeat.Started)
                {
                    activeBeat.Start(gameTime, .1F);
                    if (activeBeat.effect != null)
                        activeBeat.effect();
                    dialogText.ResetTextOnly(activeBeat.dialog, .015F);
                }

                if (activeBeat.IsComplete(gameTime) && dialogText.Complete)
                {
                    if (InputHandler.IsPressed(Button.Action, true))
                    {
                        beatQueue.Remove(activeBeat);
                    }
                }
            }
            else
            {
                if (state == BattleState.Ongoing)
                {
                    if (activeCharacter == null)
                    {
                        bool foeAlive = false;
                        bool playerAlive = false;

                        // check for if there are any remaining player controlled characters //
                        foreach (BattleCharacter player in playerParty)
                            if (player.currentHealth > 0)
                            {
                                playerAlive = true;
                                break;
                            }

                        // check for if there are any remaining enemies //
                        foreach (BattleCharacter foe in foeParty)
                            if (foe.currentHealth > 0)
                            {
                                foeAlive = true;
                                break;
                            }

                        if (!playerAlive)
                        {
                            state = BattleState.Lose;
                        }
                        else if (!foeAlive)
                        {
                            state = BattleState.Win;
                        }
                        else while (characterSequence[turnCount % characterSequence.Length].currentHealth <= 0)
                                turnCount++;

                        activeCharacter = characterSequence[turnCount % characterSequence.Length];
                    }
                    else
                    {
                        dialogText.text = "";

                        if (activeCharacter.playerControlled)
                        {
                            if (menuSelectionLayer == -1)   // initialize the sequence
                            {
                                PopulateMenuMoves(activeCharacter);
                                displayingMenuOptions = true;
                                currentMove = null;
                                menuSelectionLayer = 0;
                                menuIndex = 0;
                                currentTarget = null;
                            }
                            else if (menuSelectionLayer == 0)
                            {
                                if (MenuSelect())   // if no option was selected
                                {
                                    currentMove = activeCharacter.moveset[menuIndex];

                                    if (currentMove.targetType == TargetType.None)
                                    {
                                        finalAction = () => currentMove.onAction(activeCharacter, null);
                                        menuSelectionLayer = 2;
                                    }
                                    else
                                    {
                                        PopulateMenuTargets(GetPotentialTargets(true, currentMove.targetType));
                                        menuSelectionLayer = 1;
                                        menuIndex = 0;
                                    }
                                }
                                else
                                {
                                    displayingMenuOptions = true;
                                }
                            }
                            else if (menuSelectionLayer == 1)
                            {
                                if (MenuSelect())
                                {
                                    currentTarget = GetPotentialTargets(true, currentMove.targetType)[menuIndex];
                                    finalAction = () => currentMove.onAction(activeCharacter, currentTarget);
                                    menuSelectionLayer = 2;
                                    menuIndex = 0;
                                }
                                else
                                {
                                    displayingMenuOptions = true;
                                }
                            }
                            else if (menuSelectionLayer == 2)
                            {
                                finalAction.Invoke();   // do the move
                                DoNextTurn();

                                currentMove = null;
                                finalAction = null;
                                menuSelectionLayer = -1;
                                menuIndex = 0;
                                displayingMenuOptions = false;
                            }
                        }
                        else
                        {
                            displayingMenuOptions = false;

                            if (DetermineMoveAuto(activeCharacter))
                                DoNextTurn();
                        }
                    }

                    if (displayingMenuOptions != oldDisplayingMenuOption)
                    {
                        foreach (TextObject menuOption in menuOptionTexts)
                            menuOption.visible = displayingMenuOptions;
                    }
                }
                else if (state == BattleState.Win)
                {
                    beatQueue.Add(new BattleBeat("All enemies murdered! :D"));
                    foreach (BattleCharacter chr in playerParty)
                    {
                        int xpGained = 10;
                        beatQueue.Add(new BattleBeat(chr.name + " gained " + xpGained + "XP", () =>
                        {
                            chr.xp += xpGained;
                        }));
                    }

                    beatQueue.Add(new BattleBeat("", () =>
                    {
                        GameJRPG.currentState = GameState.OverworldPlay;
                    }));
                }
                else if (state == BattleState.Lose)
                {
                    beatQueue.Add(new BattleBeat("The good guys fucking died!!!"));
                    beatQueue.Add(new BattleBeat("", () =>
                    {
                        GameJRPG.currentState = GameState.OverworldPlay;
                    }));
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch, GameTime gameTiem)
        {
            // effects go here
        }

        private static void DoNextTurn()
        {
            activeCharacter = null;
            turnCount++;
        }

        private static bool DetermineMoveAuto(BattleCharacter chr)
        {
            List<BattleOption> usableMoves = new List<BattleOption>();
            foreach (BattleOption move in chr.moveset)
                if (move != null)
                    usableMoves.Add(move);

            int randomIndexMove = (new Random()).Next(usableMoves.Count);
            int randomIndexTarget = (new Random()).Next(playerParty.Length);

            chr.moveset[randomIndexMove].onAction(chr, playerParty[randomIndexTarget]);
            return true;
        }

        private static bool MenuSelect()
        {
            Point menuNavigation = new Point((int)(menuIndex / 3), menuIndex % 3);
            int menuIndexPrevious = menuIndex;

            if (InputHandler.IsPressed(Button.Left, true))
                menuNavigation.X--;
            if (InputHandler.IsPressed(Button.Right, true))
                menuNavigation.X++;
            if (InputHandler.IsPressed(Button.Up, true))
                menuNavigation.Y--;
            if (InputHandler.IsPressed(Button.Down, true))
                menuNavigation.Y++;

            if (menuNavigation.X < 0)
                menuNavigation.X = 0;
            if (menuNavigation.Y < 0)
                menuNavigation.Y = 0;
            if (menuNavigation.X > 1)
                menuNavigation.X = 1;
            if (menuNavigation.Y > 2)
                menuNavigation.Y = 2;

            int newIndex = menuNavigation.X * 3 + menuNavigation.Y;

            if (menuOptionTexts[newIndex].text != null && menuOptionTexts[newIndex].text != "")
            {
                for (int i = 0; i < menuOptionTexts.Length; i++)
                {
                    if (menuOptions[i] != null)
                    {
                        menuOptionTexts[i].color = menuOptions[i].clickable ? menuTextColor : Color.Gray;
                        menuOptionTexts[i].bounce = false;
                    }
                }

                menuIndex = newIndex;
                if (menuOptions[menuIndex].clickable)
                    menuOptionTexts[menuIndex].color = menuHighlightColor;
                menuOptionTexts[menuIndex].bounce = true;
            }

            if (InputHandler.IsPressed(Button.Action, true) && menuOptions[menuIndex].clickable)
                return true;
            else
                return false;
        }

        public static void PopulateMenuMoves(BattleCharacter chr)
        {
            for (int i = 0; i < menuOptions.Length; i++)
            {
                if (chr.moveset[i] != null)
                {
                    menuOptions[i] = chr.moveset[i];
                    menuOptionTexts[i].text = menuOptions[i].moveName;
                }
                else
                {
                    menuOptions[i] = null;
                    menuOptionTexts[i].text = "";
                }
            }
        }

        public static void PopulateMenuTargets(BattleCharacter[] possibleTargets)
        {
            for (int i = 0; i < menuOptions.Length; i++)
            {
                if (i < possibleTargets.Length && possibleTargets[i] != null)
                {
                    BattleCharacter target = possibleTargets[i];
                    menuOptions[i] = new BattleOption(target.name, TargetType.None, (BattleCharacter chr, BattleCharacter tar) =>
                    {
                        currentMove.onAction(activeCharacter, target);
                    });

                    menuOptionTexts[i].text = menuOptions[i].moveName;

                    if (target.currentHealth <= 0)
                    {
                        menuOptions[i].clickable = false;
                    }
                }
                else
                {
                    menuOptions[i] = null;
                    menuOptionTexts[i].text = "";
                }
            }
        }

        private static BattleCharacter[] GetPotentialTargets(bool isPlayer, TargetType targetType)
        {
            BattleCharacter[] potentialTargets;

            switch (currentMove.targetType)
            {
                case TargetType.All:
                    potentialTargets = characterSequence;
                    break;

                case TargetType.MyParty:
                    potentialTargets = isPlayer ? playerParty : foeParty;
                    break;

                case TargetType.OtherParty:
                    potentialTargets = isPlayer ? foeParty : playerParty;
                    break;

                default:
                    potentialTargets = null;
                    break;
            }

            return potentialTargets;
        }
    }
}
