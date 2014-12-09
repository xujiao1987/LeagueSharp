using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BlackPoppy
{
    class Program
    {
        // Generic
        public static readonly string champName = "Poppy";
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static readonly List<Spell> spellList = new List<Spell>();
        private static Spell Q, W, E, R;
        private static SpellSlot IgniteSlot;

        // Menu
        public static Menu menu;

        private static Orbwalking.Orbwalker OW;

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Champ validation
            if (player.ChampionName != champName) return;

            //Define spells
            Q = new Spell(SpellSlot.Q, 250f);
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 500f);
            R = new Spell(SpellSlot.R, 850f);
            spellList.AddRange(new[] { Q, W, E, R });

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            // Finetune spells
            E.SetTargetted(0.5f, 1450f);

            // Create menu
            createMenu();

            // Register events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            // Print
            Game.PrintChat(String.Format("<font color='#08F5F8'>blacky -</font> <font color='#FFFFFF'>{0} Loaded!</font>", champName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in spellList)
            {
                // Regular spell ranges
                var circleEntry = menu.Item("drawRange" + spell.Slot).GetValue<Circle>();
                if (circleEntry.Active)
                    Utility.DrawCircle(player.Position, spell.Range, circleEntry.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo(target);

            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active &&
               (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100) >
                menu.Item("harassMana").GetValue<Slider>().Value)
                OnHarass(target);

            // WaveClear
            if (menu.SubMenu("waveclear").Item("wcActive").GetValue<KeyBind>().Active &&
            (player.Mana / player.MaxMana * 100) >
            menu.Item("wcMana").GetValue<Slider>().Value)
                waveclear();

            // Misc
            if (menu.SubMenu("misc").Item("miscEscapeToMouse").GetValue<KeyBind>().Active)
                EscapeToMouse();

            // Killsteal
            Killsteal(target);

        }

        private static void OnCombo(Obj_AI_Hero target)
        {
            Menu comboMenu = menu.SubMenu("combo");
            bool useQ = comboMenu.Item("comboUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = comboMenu.Item("comboUseW").GetValue<bool>() && W.IsReady();
            bool useE = comboMenu.Item("comboUseE").GetValue<bool>() && E.IsReady();
            bool useR = comboMenu.Item("comboUseR").GetValue<bool>() && R.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useR && player.Distance(target) < R.Range && ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(R.Range)) >= menu.Item("comboLogicR").GetValue<Slider>().Value && menu.Item("DontUlt" + target.BaseSkinName) != null && menu.Item("DontUlt" + target.BaseSkinName).GetValue<bool>() == false)
            {
                if (target != null)
                    UltLogic();
            }

            if (useE && player.Distance(target) < E.Range)
            {
                if (target != null)
                    if (useW)
                    {
                        W.Cast(player, packets());
                    }
                ELogic();
            }

            if (useQ && player.Distance(target) < Q.Range)
            {
                if (target != null)
                    Q.Cast(player, packets());
            }

            if (target != null && menu.Item("miscIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
            player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (GetComboDamage(target) > target.Health)
                {
                    player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }
        }

        private static void OnHarass(Obj_AI_Hero target)
        {
            Menu harassMenu = menu.SubMenu("harass");
            bool useQ = harassMenu.Item("harassUseQ").GetValue<bool>() && Q.IsReady();

            if (useQ && player.Distance(target) < Q.Range)
            {
                if (target != null)
                    Q.Cast(player, packets());
            }

        }

        private static void EscapeToMouse()
        {
            Menu miscMenu = menu.SubMenu("misc");
            bool useW = miscMenu.Item("miscUseW").GetValue<bool>() && W.IsReady();

            if (useW)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                W.Cast(player, packets());
            }

            else
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
            }
        }

        private static void UltLogic()
        {
            Obj_AI_Hero newtarget = null;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (newtarget == null)
                {
                    newtarget = hero;
                }
                else
                {
                    if (hero.Health > newtarget.Health && hero.BaseAttackDamage < newtarget.BaseAttackDamage)
                    {
                        newtarget = hero;
                    }
                }
            }
            R.Cast(newtarget, packets());
        }

        private static void ELogic()
        {
            foreach (var hero in from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range))
                                 let prediction = E.GetPrediction(hero)
                                 where NavMesh.GetCollisionFlags(
                                 prediction.UnitPosition.To2D()
                                 .Extend(ObjectManager.Player.ServerPosition.To2D(), -300)
                                 .To3D())
                                 .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                 prediction.UnitPosition.To2D()
                                 .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                 -(300 / 2))
                                 .To3D())
                                 .HasFlag(CollisionFlags.Wall)
                                 select hero)
            {
                E.Cast(hero, packets());
            }
        }

        private static void Killsteal(Obj_AI_Hero target)
        {
            Menu killstealMenu = menu.SubMenu("killsteal");
            bool useQ = killstealMenu.Item("killstealUseQ").GetValue<bool>() && Q.IsReady();
            bool useE = killstealMenu.Item("killstealUseE").GetValue<bool>() && E.IsReady();

            if (target.HasBuffOfType(BuffType.Invulnerability)) return;

            if (useQ && target.Distance(player) < Q.Range)
            {
                if (Q.IsKillable(target))
                {
                    Q.Cast(player, packets());
                }
            }

            if (useE && target.Distance(player) < E.Range)
            {
                if (E.IsKillable(target))
                {
                    E.Cast(target, packets());
                }
            }
        }

        private static void waveclear()
        {
            Menu waveclearMenu = menu.SubMenu("waveclear");
            bool useQ = waveclearMenu.Item("wcUseQ").GetValue<bool>() && Q.IsReady();
            bool useW = waveclearMenu.Item("wcUseW").GetValue<bool>() && W.IsReady();
            bool useE = waveclearMenu.Item("wcUseE").GetValue<bool>() && E.IsReady();

            var allMinionsQ = MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (useQ)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.IsValidTarget() &&
                    Q.IsKillable(minion))
                    {
                        Q.CastOnUnit(player, packets());
                        return;
                    }
                }
            }

            if (useW && allMinionsQ.Count > 1)
            {
                W.Cast(player, packets());
            }

            if (useE)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(player.Distance(minion) * 1000 / 1450)) <
                    player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.CastOnUnit(minion, packets());
                        return;
                    }
                }
            }

            var jcreeps = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jcreeps.Count > 0)
            {
                var jcreep = jcreeps[0];

                if (useQ)
                {
                    Q.Cast(player, packets());
                }

                if (useW)
                {
                    W.Cast(player, packets());
                }

                if (useE)
                {
                    E.Cast(jcreep, packets());
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (R.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.R);

            if (W.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.E);

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (IgniteSlot != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        private static bool packets()
        {
            return menu.Item("miscPacket").GetValue<bool>();
        }

        private static void createMenu()
        {
            menu = new Menu("VIP-" + champName, "black" + champName, true);

            // Target selector
            Menu ts = new Menu("目标选择", "ts");
            menu.AddSubMenu(ts);
            SimpleTs.AddToMenu(ts);

            // Orbwalker
            Menu orbwalk = new Menu("走砍", "orbwalk");
            menu.AddSubMenu(orbwalk);
            OW = new Orbwalking.Orbwalker(orbwalk);

            // Combo
            Menu combo = new Menu("连招", "combo");
            menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("comboUseQ", "使用Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "使用W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "使用E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "使用R").SetValue(true));
            combo.AddItem(new MenuItem("comboLogicR", "敌人数量").SetValue(new Slider(2, 1, 5)));
            combo.AddItem(new MenuItem("comboActive", "连招").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass
            Menu harass = new Menu("骚扰", "harass");
            menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassUseQ", "使用Q").SetValue(true));
            harass.AddItem(new MenuItem("harassMana", "蓝量").SetValue(new Slider(40, 100, 0)));
            harass.AddItem(new MenuItem("harassActive", "骚扰").SetValue(new KeyBind('C', KeyBindType.Press)));

            // WaveClear
            Menu waveclear = new Menu("清线", "waveclear");
            menu.AddSubMenu(waveclear);
            waveclear.AddItem(new MenuItem("wcUseQ", "使用Q").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseW", "使用W").SetValue(true));
            waveclear.AddItem(new MenuItem("wcUseE", "使用E").SetValue(true));
            waveclear.AddItem(new MenuItem("wcMana", "蓝量").SetValue(new Slider(40, 100, 0)));
            waveclear.AddItem(new MenuItem("wcActive", "清线").SetValue(new KeyBind('V', KeyBindType.Press)));

            // Killsteal
            Menu killsteal = new Menu("抢头", "killsteal");
            menu.AddSubMenu(killsteal);
            killsteal.AddItem(new MenuItem("killstealUseQ", "使用Q").SetValue(true));
            killsteal.AddItem(new MenuItem("killstealUseE", "使用E").SetValue(true));

            // Misc
            Menu misc = new Menu("杂项", "misc");
            menu.AddSubMenu(misc);
            misc.AddItem(new MenuItem("miscPacket", "封包").SetValue(true));
            misc.AddItem(new MenuItem("miscIgnite", "使用点燃").SetValue(true));
            misc.AddItem(new MenuItem("miscEscapeToMouse", "逃跑").SetValue(new KeyBind('G', KeyBindType.Press)));
            misc.AddItem(new MenuItem("miscUseW", "使用W逃跑").SetValue(true));
            misc.AddItem(new MenuItem("DontUlt", "不R"));
            misc.AddItem(new MenuItem("sep0", "========="));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != player.Team))
                misc.AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            misc.AddItem(new MenuItem("sep1", "========="));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "显示伤害").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            // Drawings
            Menu drawings = new Menu("显示", "drawings");
            menu.AddSubMenu(drawings);
            drawings.AddItem(new MenuItem("drawRangeE", "E范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(new MenuItem("drawRangeR", "R范围").SetValue(new Circle(false, Color.Aquamarine)));
            drawings.AddItem(dmgAfterComboItem);

            // Finalizing
            menu.AddToMainMenu();
        }
    }
}
