﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Version = System.Version;


namespace SigmaSeries.Plugins
{
    public class Mordekaiser : PluginBase
    {
        public Mordekaiser()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 670);
            R = new Spell(SpellSlot.R, 850);
        }

        public static bool packetCast;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "使用Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "使用W").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "使用E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "使用R").SetValue(false));
            config.AddItem(new MenuItem("controlMinion", "控制小兵").SetValue(true));
            config.AddItem(new MenuItem("forceR", "手动R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
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
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "控线", "清线", "全部", "不" }, 3)));
            config.AddItem(new MenuItem("JungleActive", "清野").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "使用Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "使用W").SetValue(true));
            config.AddItem(new MenuItem("UseEJung", "使用E").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "封包").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            packetCast = Config.Item("packetCast").GetValue<bool>();
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if ((Config.Item("forceR").GetValue<KeyBind>().Active) && target != null)
            {
                R.CastOnUnit(target, true);
            }
            if (ComboActive)
            {
                Combo();
            }
            if (HarassActive)
            {
                Harass();
            }
            if (WaveClearActive)
            {
                WaveClear();
            }
            if (FreezeActive)
            {
                Freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                Jungle();
            }
        }

        private void Combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useRCon = Config.Item("controlMinion").GetValue<bool>();
            var Target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Player.HasBuff("MordekaiserCOTGSelf") && useRCon)
                {
                    var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Position.Distance(champ.ServerPosition) < 2500 && champ.IsEnemy select champ).ToList();
                    nearChamps.OrderBy(x => Player.Position.Distance(x.ServerPosition));
                    if (nearChamps.Count > 0)
                    {
                        R.Cast(nearChamps.First().ServerPosition, packetCast);
                    }
                    else
                    {
                        R.Cast(Game.CursorPos, packetCast);
                    }
                }
                if (R.GetDamage(Target) > Target.Health && useR)
                {
                    R.CastOnUnit(Target);
                }
                if (Orbwalking.InAutoAttackRange(Target) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);;
                    return;
                }
                if (W.IsReady() && useW && wCast(Target))
                {
                    return;
                }
                if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                {
                    E.Cast(Target.Position, packetCast);
                }
            }
        }

        private void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Orbwalking.InAutoAttackRange(Target) && useQ && Q.IsReady())
                {
                    Q.CastOnUnit(Player, packetCast);;
                    return;
                }
                if (W.IsReady() && useW && wCast(Target))
                {
                    return;
                }
                if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                {
                    E.Cast(Target.Position, packetCast);
                }
            }
        }

        private void WaveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }
        private void Freeze()
        {

            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }
        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();

            if (JungleMinions.Count > 0)
            {
                foreach (var minion in JungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(Player.Position, packetCast);
                        return;
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion.Position, packetCast);
                        return;
                    }
                    if (W.IsReady() && useW)
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }

        public bool wCast(Obj_AI_Base wTarget)
        {
            if (Player.Distance(wTarget) < Orbwalking.GetRealAutoAttackRange(Player))
            {
                W.Cast(Player);
                return true;
            }
            var allies = (from champs in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champs) < W.Range && champs.IsAlly select champs).ToList();
            foreach (var ally in allies)
            {
                if (ally.Distance(wTarget) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    W.Cast(ally, packetCast);
                    return true;
                }
            }
            return false;
        }
    }
}
