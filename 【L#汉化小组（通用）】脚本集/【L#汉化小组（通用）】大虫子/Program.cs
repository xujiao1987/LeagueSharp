using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Cho_Gath
{
    internal class Program
    {
        public static string ChampionName = "Chogath";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell R;
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != ChampionName)
                return;

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 675);
            R = new Spell(SpellSlot.R, 175);

            Q.SetSkillshot(0.75f, 175f, 1000f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.60f, 300f, 1750f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);

            Config = new Menu("大虫子", ChampionName, true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("Killsteal", "抢人头").SetValue(false));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("自动", "AutoSpell"));
            Config.SubMenu("AutoSpell").AddItem(new MenuItem("AutoQ1", "自动Q移动").SetValue(true));
            Config.SubMenu("AutoSpell").AddItem(new MenuItem("AutoQ2", "自动Q突进").SetValue(true));

            Config.AddSubMenu(new Menu("其他", "Additonal"));
            Config.SubMenu("Additonal").AddItem(new MenuItem("AutoStack", "自动叠大招").SetValue(true));
            Config.SubMenu("Additonal").AddItem(new MenuItem("AutoInterrupt", "打断技能").SetValue(true));

            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q范围").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R范围").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
           
			Config.AddToMainMenu();

            Drawing.OnDraw    += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                ExecuteCombo();

            if (Config.Item("Killsteal").GetValue<bool>())
                ExecuteKillsteal();

            ExecuteAdditionals();
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("AutoInterrupt").GetValue<bool>() || !unit.IsValidTarget()) return;
            W.Cast(unit);
            Q.Cast(unit);
        }

        private static void ExecuteAdditionals()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var autoStack = Config.Item("AutoStack").GetValue<bool>();
            var count = 0;

            foreach (var buffs in ObjectManager.Player.Buffs.Where(buffs => buffs.DisplayName == "Feast"))
            {
                count = buffs.Count;
            }

            if (R.IsReady() && autoStack)
            foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(R.Range) && (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) > minion.Health)).Where(minion => count < 6))
                R.CastOnUnit(minion);

            var autoQ1 = Config.Item("AutoQ1").GetValue<bool>();
            var autoQ2 = Config.Item("AutoQ2").GetValue<bool>();

            foreach (var champion in from champion in ObjectManager.Get<Obj_AI_Hero>() 
            where champion.IsValidTarget(Q.Range) let qPrediction = Q.GetPrediction(champion) 
            where (qPrediction.Hitchance == HitChance.Immobile && autoQ1) ||(qPrediction.Hitchance == HitChance.Dashing && autoQ2) select champion)
                Q.Cast(champion, true, true);
        }

        private static void ExecuteCombo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();

            if (W.IsReady() && useW && ObjectManager.Player.Distance(target) <= W.Range)
                W.Cast(target, false, true);

            if (Q.IsReady() && useQ && ObjectManager.Player.Distance(target) <= Q.Range)
                Q.Cast(target, false, true);

            if (R.IsReady() && useR && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
                R.CastOnUnit(target, true);
        }

        private static void ExecuteKillsteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (R.IsReady() && hero.Distance(ObjectManager.Player) <= R.Range &&
                    ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R) > hero.Health)
                    R.CastOnUnit(hero, true);

                if (W.IsReady() && hero.Distance(ObjectManager.Player) <= W.Range &&
                    ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W) > hero.Health)
                    W.CastIfHitchanceEquals(hero, HitChance.High, true);

                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range &&
                    ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q) > hero.Health)
                    Q.CastIfHitchanceEquals(hero, HitChance.High, true);
            }
        }
    }
}