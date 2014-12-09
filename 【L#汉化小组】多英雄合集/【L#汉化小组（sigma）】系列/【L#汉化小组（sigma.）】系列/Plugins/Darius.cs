﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Darius : PluginBase
    {
        public Darius()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 425);
            W = new Spell(SpellSlot.W, 210);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.5f, 300f, 1500f, false, SkillshotType.SkillshotCone);
        }

        public static bool packetCast;
        public string buffName = "DariusHemo";
      

        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "使用Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "使用W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "使用E").SetValue(true));
            config.AddItem(new MenuItem("eRangeCheck", "超出范围使用E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "使用R").SetValue(true));
            config.AddItem(new MenuItem("rKS", "使用R抢头").SetValue(false));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "使用Q").SetValue(false));
            config.AddItem(new MenuItem("UseWHarass", "使用W").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "使用E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] { "控线", "清线", "全部", "不" }, 1)));
            config.AddItem(new MenuItem("useWFarm", "W").SetValue(new StringList(new[] { "控线", "清线", "全部", "不" }, 3)));
            config.AddItem(new MenuItem("JungleActive", "清野").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "使用Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "使用W").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            packetCast = Config.Item("packetCast").GetValue<bool>();
            if (Config.Item("rKS").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsValidTarget(R.Range) && enemy.IsEnemy && enemy.Health < getDMGULT(enemy))
                    {
                        R.CastOnUnit(enemy);
                    }
                }
            }

            if (ComboActive)
            {
                combo();
            }
            if (HarassActive)
            {
                harass();
            }
            if (WaveClearActive)
            {
                waveClear();
            }
            if (FreezeActive)
            {
                freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
        }

        private float getDMGULT(Obj_AI_Hero target)
        {
            foreach (var buff in target.Buffs)
            {
                if (buff.DisplayName == buffName && buff.Count > 1)
                {
                    return R.GetDamage(target) * (1 + buff.Count / 5);
                }
            }
            return R.GetDamage(target);
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var e2 = Config.Item("eRangeCheck").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (useR)
                {
                    if (getDMGULT(Target) > Target.Health && R.IsReady())
                        {
                            R.CastOnUnit(Target, true);
                            return;
                        }
                }

                if (Player.Distance(Target) < E.Range && useE && E.IsReady() && !e2 || Player.Distance(Target) < E.Range && useE && e2 && E.IsReady() && Player.Distance(Target) > 430)
                {
                    E.Cast(Target, packetCast);
                    return;
                }
                //castItems(Target);
                if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);
                    return;
                }
                if (Player.Distance(Target) < Orbwalking.GetRealAutoAttackRange(Player) && useW && W.IsReady())
                {
                    W.CastOnUnit(Player, packetCast);
                    return;
                }
            }
        }
        private void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var e2 = Config.Item("eRangeCheck").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Player.Distance(Target) < E.Range && useE && E.IsReady() && !e2 ||
                    Player.Distance(Target) < E.Range && useE && e2 && E.IsReady() && Player.Distance(Target) > 430)
                {
                    E.Cast(Target, packetCast);
                    return;
                }
                //castItems(Target);
                if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);
                    return;
                }
                if (Player.Distance(Target) < Orbwalking.GetRealAutoAttackRange(Player) && useW && W.IsReady())
                {
                    W.CastOnUnit(Player, packetCast);
                    return;
                }
            }
        }

        private void waveClear()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    //castItems(minion, true);
                    if (Player.Distance(minion) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Player, packetCast);
                        return;
                    }
                    if (Player.Distance(minion) < Orbwalking.GetRealAutoAttackRange(Player) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                        return;
                    }
                }

            }
        }
        private void freeze()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (Player.Distance(minion) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Player, packetCast);
                        return;
                    }
                    if (Player.Distance(minion) < Orbwalking.GetRealAutoAttackRange(Player) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                        return;
                    }
                }
            }
        }
        private void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    //castItems(minion, true);
                    if (Player.Distance(minion) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Player, packetCast);
                        return;
                    }
                    if (Player.Distance(minion) < Orbwalking.GetRealAutoAttackRange(Player) && useW && W.IsReady())
                    {
                        W.CastOnUnit(Player, packetCast);
                        return;
                    }
                }
            }
        }


    }
}
