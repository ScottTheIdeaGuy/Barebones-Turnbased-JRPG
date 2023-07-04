using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRPG
{
    public delegate void BattleAction(BattleCharacter character, BattleCharacter target);
    public enum TargetType { None, MyParty, OtherParty, All};
    public class BattleOption
    {
        public readonly string moveName;
        public BattleAction onAction = null;
        public TargetType targetType = TargetType.None;
        public bool clickable;

        public static readonly BattleOption Punch = new BattleOption("Punch", TargetType.OtherParty, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int attackPower = 10;
            int damageDealt = tar.GetDamageReceived(chr.GetDamageInflicted(attackPower));

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " threw " + chr.pronouns[2] + " fist."));
            BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " took " + damageDealt + " damage", () => { tar.currentHealth = Math.Max(tar.currentHealth - damageDealt, 0); }));
            if (tar.currentHealth - damageDealt <= 0)
                BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " went up in smoke."));
        });
        
        public static readonly BattleOption BitchSlap = new BattleOption("Bitch Slap", TargetType.OtherParty, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int attackPower = 5;
            int damageDealt = tar.GetDamageReceived(chr.GetDamageInflicted(attackPower));

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " smacked " + tar.name + " \nupside the face."));
            BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " took\n" + damageDealt + " bitch damage", () => { tar.currentHealth = Math.Max(tar.currentHealth - damageDealt, 0); }));
            if (tar.currentHealth - damageDealt <= 0)
                BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " went up in smoke."));
        });

        public static readonly BattleOption Juke = new BattleOption("Juke", TargetType.OtherParty, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int defenseDecrease = 1;

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " moved fast,\nand faked " + tar.name + " out."));
            BattleSystem.beatQueue.Add(new BattleBeat(tar.name + "'s defense\nfell by " + defenseDecrease + "!", () => { tar.defenseStatBuff -= defenseDecrease; }));
        });

        public static readonly BattleOption Eat = new BattleOption("Eat", TargetType.None, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int healthRecovered = 15;

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " found gum stuck to\n" + chr.pronouns[2] + " shoe"));
            BattleSystem.beatQueue.Add(new BattleBeat(chr.pronouns[0].Capitalize() + " recovered " + healthRecovered + "HP", () => { chr.currentHealth = Math.Min(chr.currentHealth + healthRecovered, chr.maxHealth); }));
            if (chr.currentHealth + healthRecovered >= chr.maxHealth)
                BattleSystem.beatQueue.Add(new BattleBeat(chr.pronouns[2].Capitalize() + " HP was maxed out!"));
        });

        public static readonly BattleOption PsychUp = new BattleOption("Psych Up", TargetType.None, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int attackIncrease = 1;

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " whispered \"u got dis\"\nto " + chr.pronouns[3]));
            BattleSystem.beatQueue.Add(new BattleBeat(chr.pronouns[2].Capitalize() + " attack\nrose by " + attackIncrease + "!", () => { chr.attackStatBuff += attackIncrease; }));
        });

        public static readonly BattleOption FreezerBurn = new BattleOption("Freezer Burn", TargetType.OtherParty, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int attackPower = 7;
            int damageDealt = tar.GetDamageReceived(chr.GetDamageInflicted(attackPower));

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " spewed dry ice\nat " + tar.name + "'s face"));
            BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " took " + damageDealt + " ice damage", () => { tar.currentHealth = Math.Max(tar.currentHealth - damageDealt, 0); }));
            if (tar.currentHealth - damageDealt <= 0)
                BattleSystem.beatQueue.Add(new BattleBeat(tar.name + " was frozen solid"));
        });
        
        public static readonly BattleOption FrostBuild = new BattleOption("Frost Build", TargetType.None, (BattleCharacter chr, BattleCharacter tar) =>
        {
            int defenseIncrease = 1;

            BattleSystem.beatQueue.Add(new BattleBeat(chr.name + " built up frost"));
            BattleSystem.beatQueue.Add(new BattleBeat(chr.pronouns[2].Capitalize() + " defense\nrose by " + defenseIncrease + "!", () => { chr.defenseStatBuff += defenseIncrease; }));
        });

        public BattleOption(string moveName, TargetType targetType, BattleAction onAction)
        {
            this.moveName = moveName;
            this.targetType = targetType;
            this.onAction = onAction;
            this.clickable = true;
        }                
    }
}
