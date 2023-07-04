using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRPG
{
    public class BattleCharacter
    {
        public string name { get; protected set; }
        public readonly string[] pronouns = { "they", "them", "their", "themselves" };

        public bool playerControlled = false;

        public int level = 1;
        public int attackStat = 1;
        public int defenseStat = 1;
        public int maxHealth = 20;


        public BattleOption[] moveset = new BattleOption[6];
        public string deathDialog = " has fallen.";


        public int currentHealth;
        public int xp = 0;
        public int attackStatBuff = 0;
        public int defenseStatBuff = 0;        

        public BattleCharacter(string name, string[] pronouns)
        {
            this.name = name;
            this.pronouns = pronouns;
        }

        public BattleCharacter(BattleCharacter og)
        {
            this.pronouns = og.pronouns;
            playerControlled = og.playerControlled;
            level = og.level;
            attackStat = og.attackStat;
            defenseStat = og.defenseStat;
            moveset = og.moveset;
            maxHealth = og.maxHealth;
            deathDialog = og.deathDialog;


            string identifier = og.name.Substring(og.name.Length - 2);
            string characterName = og.name.Substring(0, og.name.Length - 2);

            switch (identifier)
            {
                case " A":
                    this.name = characterName + " B";
                    break;
                case " B":
                    this.name = characterName + " C";
                    break;
                case " C":
                    this.name = characterName + " D";
                    break;
                case " D":
                    this.name = characterName + " E";
                    break;

                default:
                    this.name = og.name + " B";
                    og.name += " A";
                    break;
            }
        }
        
        public void Initialize()
        {
            currentHealth = maxHealth;
        }

        public int GetDamageInflicted(int baseAttack)
        {
            double totalAttack = (attackStat + attackStatBuff) / 2.0;

            double attackMultiplier = 1 + totalAttack * Math.Log(totalAttack) / 2.0;

            int result = (int)Math.Floor(baseAttack * attackMultiplier);

            return result;
        }        
        
        public int GetDamageReceived(int baseDamage)
        {
            double totalDefense = (defenseStat + defenseStatBuff) / 2.0 + 1;

            double defenseMultiplier = 2.0 / totalDefense;

            int result = (int)Math.Floor(baseDamage * defenseMultiplier);

            return result;
        }
    }
}
